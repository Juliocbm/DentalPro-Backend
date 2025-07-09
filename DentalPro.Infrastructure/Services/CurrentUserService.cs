using System.Security.Claims;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio para obtener información del usuario actual
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUsuarioRepository _usuarioRepository;
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
        ILogger<CurrentUserService> logger,
        IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _usuarioRepository = usuarioRepository;
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
    /// Invalida el caché del usuario actual, útil después de actualizaciones
    /// </summary>
    public void InvalidateCurrentUserCache()
    {
        try
        {
            var userId = GetCurrentUserId();
            _cache.Remove($"CurrentUser_{userId}");
            _cache.Remove($"UserRoles_{userId}");
            _logger.LogInformation("Cache de usuario {UserId} invalidado", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al invalidar caché de usuario");
        }
    }
}
