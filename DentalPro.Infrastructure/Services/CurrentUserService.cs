using System.Security.Claims;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using DentalPro.Infrastructure.Persistence;
using DentalPro.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio para obtener información del usuario actual
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolPermisoRepository _rolPermisoRepository;
    private readonly ILogger<CurrentUserService> _logger;
    private readonly IMemoryCache _cache;
    
    // Tiempo de caché para datos de usuario (5 minutos)
    private readonly TimeSpan _userCacheDuration = TimeSpan.FromMinutes(5);
    
    // Constantes para claims personalizadas
    private const string IdUsuarioClaim = ClaimTypes.NameIdentifier;
    private const string IdConsultorioClaim = "IdConsultorio";

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IUsuarioRepository usuarioRepository,
        IRolPermisoRepository rolPermisoRepository,
        ILogger<CurrentUserService> logger,
        IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _usuarioRepository = usuarioRepository;
        _rolPermisoRepository = rolPermisoRepository;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Obtiene el ID del usuario actual desde las claims del token JWT
    /// </summary>
    public Guid GetCurrentUserId()
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(IdUsuarioClaim);

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Intento de acceso sin ID de usuario válido");
                throw new ForbiddenAccessException("No se pudo identificar al usuario actual.", ErrorCodes.Unauthorized);
            }

            return userGuid;
        }
        catch (ForbiddenAccessException)
        {
            throw; // Re-lanzamos las excepciones de autorización sin modificarlas
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el ID del usuario actual");
            throw new ForbiddenAccessException("Error al obtener la identidad del usuario.", ErrorCodes.Unauthorized);
        }
    }

    /// <summary>
    /// Obtiene el usuario actual completo con sus roles y relaciones
    /// </summary>
    public async Task<Usuario> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        
        // Intentar obtener desde caché
        var cacheKey = $"CurrentUser_{userId}";
        if (!_cache.TryGetValue(cacheKey, out Usuario? usuario))
        {
            usuario = await _usuarioRepository.GetByIdAsync(userId);

            if (usuario == null)
            {
                _logger.LogWarning("Usuario con ID {UserId} no encontrado en base de datos", userId);
                throw new NotFoundException("Usuario no encontrado", ErrorCodes.UserNotFound);
            }

            // Validar que el usuario esté activo
            if (!usuario.Activo)
            {
                _logger.LogWarning("Intento de acceso con usuario inactivo: {UserId}", userId);
                throw new ForbiddenAccessException("El usuario está desactivado.", ErrorCodes.UserInactive);
            }

            // Guardar en caché
            _cache.Set(cacheKey, usuario, _userCacheDuration);
        }

        return usuario;
    }

    /// <summary>
    /// Obtiene el ID del consultorio al que pertenece el usuario actual
    /// </summary>
    public Guid GetCurrentConsultorioId()
    {
        try
        {
            var consultorioId = _httpContextAccessor.HttpContext?.User.FindFirstValue(IdConsultorioClaim);

            if (string.IsNullOrEmpty(consultorioId) || !Guid.TryParse(consultorioId, out var consultorioGuid))
            {
                _logger.LogWarning("Intento de acceso sin ID de consultorio válido");
                throw new ForbiddenAccessException("No se pudo identificar el consultorio del usuario actual.", ErrorCodes.ConsultorioNotFound);
            }

            return consultorioGuid;
        }
        catch (ForbiddenAccessException)
        {
            throw; // Re-lanzamos las excepciones de autorización sin modificarlas
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el ID del consultorio del usuario actual");
            throw new ForbiddenAccessException("Error al obtener el consultorio del usuario.", ErrorCodes.ConsultorioNotFound);
        }
    }

    /// <summary>
    /// Verifica si el usuario actual tiene un rol específico
    /// </summary>
    public async Task<bool> IsInRoleAsync(string roleName)
    {
        // Verificamos si el claim de rol existe directamente en el token
        if (_httpContextAccessor.HttpContext?.User.IsInRole(roleName) == true)
        {
            return true;
        }
        
        // Si no está en el token, verificamos en la base de datos
        var userId = GetCurrentUserId();
        
        // Clave para cache de roles
        var cacheKey = $"UserRoles_{userId}";
        
        IEnumerable<string> roles;
        if (!_cache.TryGetValue(cacheKey, out roles))
        {
            roles = await _usuarioRepository.GetUserRolesAsync(userId);
            _cache.Set(cacheKey, roles, _userCacheDuration);
        }
        
        return roles.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Verifica si el usuario actual tiene un permiso específico
    /// </summary>
    public async Task<bool> HasPermisoAsync(string permisoCode)
    {
        if (string.IsNullOrEmpty(permisoCode))
        {
            return false;
        }
        
        var userId = GetCurrentUserId();
        var cacheKey = $"UserPermisos_{userId}";
        
        _logger.LogDebug("Verificando permiso {PermisoCode} para el usuario {UserId}", permisoCode, userId);
        
        // Verificar primero si ya tenemos los permisos en caché
        if (_cache.TryGetValue(cacheKey, out IEnumerable<string> cachedPermisos))
        {
            var tienePermiso = cachedPermisos.Any(p => p.Equals(permisoCode, StringComparison.OrdinalIgnoreCase));
            _logger.LogDebug("Permiso {PermisoCode} encontrado en caché: {TienePermiso}", permisoCode, tienePermiso);
            return tienePermiso;
        }

        _logger.LogDebug("Permisos no encontrados en caché para el usuario {UserId}, consultando repositorios", userId);
        
        // Si no están en caché, obtenerlos directamente del repositorio
        try
        {
            // Verificar permisos directos
            var tienePermisoDirecto = await _usuarioRepository.TienePermisoDirectoAsync(userId, permisoCode);
            _logger.LogDebug("Usuario {UserId} tiene permiso directo {PermisoCode}: {TienePermiso}", userId, permisoCode, tienePermisoDirecto);
            
            // Si no tiene el permiso directo, verificar permisos de roles
            bool tienePermisoRol = false;
            if (!tienePermisoDirecto)
            {
                var roles = await _usuarioRepository.GetRolesAsync(userId);
                _logger.LogDebug("Usuario {UserId} tiene {CantidadRoles} roles asignados", userId, roles.Count());
                
                foreach (var rol in roles)
                {
                    _logger.LogDebug("Verificando si rol {RolId} ({RolNombre}) tiene permiso {PermisoCode}", rol.IdRol, rol.Nombre, permisoCode);
                    if (await _usuarioRepository.TieneRolPermisoAsync(rol.IdRol, permisoCode))
                    {
                        tienePermisoRol = true;
                        _logger.LogDebug("Permiso {PermisoCode} encontrado en rol {RolId} ({RolNombre})", permisoCode, rol.IdRol, rol.Nombre);
                        break;
                    }
                }
            }
            
            // Obtener todos los códigos de permisos para el caché
            var permisosDirectos = await _usuarioRepository.GetUsuarioPermisosAsync(userId);
            _logger.LogDebug("Usuario {UserId} tiene {CantidadPermisosDirectos} permisos directos", userId, permisosDirectos.Count());
            
            // Obtener permisos de roles
            var permisosRoles = new HashSet<string>();
            var userRoles = await _usuarioRepository.GetRolesAsync(userId);
            foreach (var rol in userRoles)
            {
                var permisos = await _rolPermisoRepository.GetPermisosByRolIdAsync(rol.IdRol);
                foreach (var permiso in permisos.Where(p => p != null))
                {
                    permisosRoles.Add(permiso.Codigo); // Usar Codigo en lugar de Nombre
                }
            }
            _logger.LogDebug("Usuario {UserId} tiene {CantidadPermisosRoles} permisos vía roles", userId, permisosRoles.Count);
            
            // Combinar todos los permisos
            var todosPermisos = permisosDirectos.Union(permisosRoles).ToList();
            
            // Log detallado de los permisos para depuración
            _logger.LogDebug("Permisos consolidados para el usuario {UserId}: {Permisos}", 
                userId, string.Join(", ", todosPermisos));
            
            // Guardar en caché para consultas futuras
            _cache.Set(cacheKey, todosPermisos, _userCacheDuration);
            _logger.LogDebug("Permisos guardados en caché para el usuario {UserId} por {DuracionCache} segundos", 
                userId, _userCacheDuration.TotalSeconds);
            
            return tienePermisoDirecto || tienePermisoRol;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar permiso {PermisoCode} para usuario {UserId}", permisoCode, userId);
            return false;
        }
    }
    
    /// <summary>
    /// Verifica si el usuario tiene todos los permisos especificados
    /// </summary>
    public async Task<bool> HasAllPermisosAsync(IEnumerable<string> permisoNombres)
    {
        if (permisoNombres == null || !permisoNombres.Any())
        {
            return true; // Si no se especifican permisos, se considera que cumple la condición
        }
        
        foreach (var permisoNombre in permisoNombres)
        {
            if (!await HasPermisoAsync(permisoNombre))
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Verifica si el usuario tiene al menos uno de los permisos especificados
    /// </summary>
    public async Task<bool> HasAnyPermisoAsync(IEnumerable<string> permisoNombres)
    {
        if (permisoNombres == null || !permisoNombres.Any())
        {
            return false;
        }
        
        foreach (var permisoNombre in permisoNombres)
        {
            if (await HasPermisoAsync(permisoNombre))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Obtiene todos los permisos del usuario actual
    /// </summary>
    public async Task<IEnumerable<string>> GetPermisosAsync()
    {
        var userId = GetCurrentUserId();
        var cacheKey = $"UserPermisos_{userId}";
        
        // Verificar si ya tenemos los permisos en caché
        if (_cache.TryGetValue(cacheKey, out IEnumerable<string> cachedPermisos))
        {
            return cachedPermisos;
        }
        
        // Si no están en caché, obtenerlos directamente del repositorio
        try
        {
            // Obtener permisos directos del usuario
            var permisosDirectos = await _usuarioRepository.GetUsuarioPermisosAsync(userId);
            
            // Obtener permisos de roles
            var permisosRoles = new HashSet<string>();
            var userRoles = await _usuarioRepository.GetRolesAsync(userId);
            foreach (var rol in userRoles)
            {
                // Usar el repositorio inyectado en lugar de crear una nueva instancia
                var permisos = await _rolPermisoRepository.GetPermisosByRolIdAsync(rol.IdRol);
                foreach (var permiso in permisos.Where(p => p != null))
                {
                    permisosRoles.Add(permiso.Nombre);
                }
            }
            
            // Combinar todos los permisos
            var todosPermisos = permisosDirectos.Union(permisosRoles).ToList();
            
            // Guardar en caché
            _cache.Set(cacheKey, todosPermisos, _userCacheDuration);
            
            return todosPermisos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener permisos para usuario {UserId}", userId);
            return Array.Empty<string>();
        }
    }
    
    /// <summary>
    /// Invalida el caché del usuario actual, útil después de actualizaciones
    /// </summary>
    public void InvalidateCurrentUserCache()
    {
        try
        {
            var userId = GetCurrentUserId();
            _cache.Remove($"CurrentUser_{userId}");
            _cache.Remove($"UserRoles_{userId}");
            _cache.Remove($"UserPermisos_{userId}");
            _logger.LogInformation("Cache de usuario {UserId} invalidado", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al invalidar caché de usuario");
        }
    }
}
