using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de gestión de permisos
/// </summary>
public class PermisoService : IPermisoService
{
    private readonly IPermisoRepository _permisoRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<PermisoService> _logger;

    private const string CACHE_KEY_PERMISOS_ALL = "permisos_all";
    private const string CACHE_KEY_PERMISOS_ROL = "permisos_rol_{0}";
    private const string CACHE_KEY_PERMISOS_USUARIO = "permisos_usuario_{0}";

    public PermisoService(
        IPermisoRepository permisoRepository,
        IRolRepository rolRepository,
        IUsuarioRepository usuarioRepository,
        IMemoryCache memoryCache,
        ILogger<PermisoService> logger)
    {
        _permisoRepository = permisoRepository;
        _rolRepository = rolRepository;
        _usuarioRepository = usuarioRepository;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los permisos existentes con caché
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetAllPermisosAsync()
    {
        if (!_memoryCache.TryGetValue(CACHE_KEY_PERMISOS_ALL, out IEnumerable<Permiso> permisos))
        {
            permisos = await _permisoRepository.GetAllAsync();
            _memoryCache.Set(CACHE_KEY_PERMISOS_ALL, permisos, TimeSpan.FromMinutes(30));
        }
        return permisos;
    }

    /// <summary>
    /// Obtiene un permiso por su ID
    /// </summary>
    public async Task<Permiso?> GetPermisoByIdAsync(Guid idPermiso)
    {
        return await _permisoRepository.GetByIdAsync(idPermiso);
    }

    /// <summary>
    /// Obtiene un permiso por su nombre
    /// </summary>
    public async Task<Permiso?> GetPermisoByNombreAsync(string nombre)
    {
        return await _permisoRepository.GetByNombreAsync(nombre);
    }

    /// <summary>
    /// Verifica si existe un permiso con el ID especificado
    /// </summary>
    public async Task<bool> ExistsByIdAsync(Guid idPermiso)
    {
        var permiso = await _permisoRepository.GetByIdAsync(idPermiso);
        return permiso != null;
    }
    
    /// <summary>
    /// Verifica si existe un permiso con el nombre especificado
    /// </summary>
    public async Task<bool> ExistsByNameAsync(string nombre)
    {
        var permiso = await _permisoRepository.GetByNombreAsync(nombre);
        return permiso != null;
    }

    /// <summary>
    /// Obtiene todos los permisos asignados a un rol específico
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetPermisosByRolIdAsync(Guid idRol)
    {
        string cacheKey = string.Format(CACHE_KEY_PERMISOS_ROL, idRol);

        if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Permiso> permisos))
        {
            permisos = await _permisoRepository.GetByRolIdAsync(idRol);
            _memoryCache.Set(cacheKey, permisos, TimeSpan.FromMinutes(15));
        }

        return permisos;
    }

    /// <summary>
    /// Obtiene todos los permisos asignados a un rol por su nombre
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetPermisosByRolNameAsync(string nombreRol)
    {
        var rol = await _rolRepository.GetByNombreAsync(nombreRol);
        if (rol == null)
        {
            _logger.LogWarning("Intento de obtener permisos para rol inexistente: {NombreRol}", nombreRol);
            return Enumerable.Empty<Permiso>();
        }

        return await GetPermisosByRolIdAsync(rol.IdRol);
    }

    /// <summary>
    /// Obtiene todos los permisos asignados a un usuario específico (combinando permisos de todos sus roles)
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetPermisosByUsuarioIdAsync(Guid idUsuario)
    {
        string cacheKey = string.Format(CACHE_KEY_PERMISOS_USUARIO, idUsuario);

        if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Permiso> todosLosPermisos))
        {
            var usuario = await _usuarioRepository.GetByIdWithRolesAsync(idUsuario);
            if (usuario == null)
            {
                _logger.LogWarning("Intento de obtener permisos para usuario inexistente: {IdUsuario}", idUsuario);
                return Enumerable.Empty<Permiso>();
            }

            var permisosSet = new HashSet<Permiso>(new PermisoEqualityComparer());

            // Obtener permisos de todos los roles del usuario
            foreach (var usuarioRol in usuario.Roles)
            {
                var permisosRol = await GetPermisosByRolIdAsync(usuarioRol.IdRol);
                foreach (var permiso in permisosRol)
                {
                    permisosSet.Add(permiso);
                }
            }

            // También obtener permisos asignados directamente al usuario (si los hubiera)
            // Esta es una extensión que permitiría asignar permisos específicos a usuarios
            // si se implementa esa funcionalidad en el futuro

            todosLosPermisos = permisosSet.ToList();
            _memoryCache.Set(cacheKey, todosLosPermisos, TimeSpan.FromMinutes(10));
        }

        return todosLosPermisos;
    }

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico por nombre de permiso
    /// </summary>
    public async Task<bool> HasUsuarioPermisoByNameAsync(Guid idUsuario, string nombrePermiso)
    {
        var permisos = await GetPermisosByUsuarioIdAsync(idUsuario);
        return permisos.Any(p => p.Nombre.Equals(nombrePermiso, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico por ID de permiso
    /// </summary>
    public async Task<bool> HasUsuarioPermisoByIdAsync(Guid idUsuario, Guid idPermiso)
    {
        var permisos = await GetPermisosByUsuarioIdAsync(idUsuario);
        return permisos.Any(p => p.IdPermiso == idPermiso);
    }

    /// <summary>
    /// Verifica si un rol tiene un permiso específico por nombre de permiso
    /// </summary>
    public async Task<bool> HasRolPermisoByNameAsync(Guid idRol, string nombrePermiso)
    {
        var permisos = await GetPermisosByRolIdAsync(idRol);
        return permisos.Any(p => p.Nombre.Equals(nombrePermiso, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifica si un rol tiene un permiso específico por ID de permiso
    /// </summary>
    public async Task<bool> HasRolPermisoByIdAsync(Guid idRol, Guid idPermiso)
    {
        var permisos = await GetPermisosByRolIdAsync(idRol);
        return permisos.Any(p => p.IdPermiso == idPermiso);
    }

    /// <summary>
    /// Asigna un permiso a un rol
    /// </summary>
    public async Task AssignPermisoToRolAsync(Guid idRol, Guid idPermiso)
    {
        var rol = await _rolRepository.GetByIdAsync(idRol);
        if (rol == null)
        {
            throw new NotFoundException("Rol", idRol);
        }

        var permiso = await _permisoRepository.GetByIdAsync(idPermiso);
        if (permiso == null)
        {
            throw new NotFoundException("Permiso", idPermiso);
        }

        var result = await _permisoRepository.AsignarPermisosARolAsync(idRol, new[] { idPermiso });
        if (!result)
        {
            throw new BadRequestException($"No se pudo asignar el permiso {permiso.Nombre} al rol {rol.Nombre}");
        }

        // Invalidar caché relacionada
        InvalidateRolPermisosCacheAsync(idRol).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Asigna varios permisos a un rol
    /// </summary>
    public async Task AssignPermisosToRolAsync(Guid idRol, IEnumerable<Guid> idPermisos)
    {
        var rol = await _rolRepository.GetByIdAsync(idRol);
        if (rol == null)
        {
            throw new NotFoundException("Rol", idRol);
        }

        if (!idPermisos.Any())
        {
            return; // No hay nada que asignar
        }

        // Verificar que todos los permisos existen
        foreach (var idPermiso in idPermisos)
        {
            var permiso = await _permisoRepository.GetByIdAsync(idPermiso);
            if (permiso == null)
            {
                throw new NotFoundException("Permiso", idPermiso);
            }
        }

        var result = await _permisoRepository.AsignarPermisosARolAsync(idRol, idPermisos);
        if (!result)
        {
            throw new BadRequestException($"No se pudieron asignar los permisos al rol {rol.Nombre}");
        }

        // Invalidar caché relacionada
        await InvalidateRolPermisosCacheAsync(idRol);
    }

    /// <summary>
    /// Remueve un permiso de un rol
    /// </summary>
    public async Task RemovePermisoFromRolAsync(Guid idRol, Guid idPermiso)
    {
        var rol = await _rolRepository.GetByIdAsync(idRol);
        if (rol == null)
        {
            throw new NotFoundException("Rol", idRol);
        }

        var permiso = await _permisoRepository.GetByIdAsync(idPermiso);
        if (permiso == null)
        {
            throw new NotFoundException("Permiso", idPermiso);
        }

        var result = await _permisoRepository.RemoverPermisosDeRolAsync(idRol, new[] { idPermiso });
        if (!result)
        {
            throw new BadRequestException($"No se pudo remover el permiso {permiso.Nombre} del rol {rol.Nombre}");
        }

        // Invalidar caché relacionada
        await InvalidateRolPermisosCacheAsync(idRol);
    }

    /// <summary>
    /// Remueve varios permisos de un rol
    /// </summary>
    public async Task RemovePermisosFromRolAsync(Guid idRol, IEnumerable<Guid> idPermisos)
    {
        var rol = await _rolRepository.GetByIdAsync(idRol);
        if (rol == null)
        {
            throw new NotFoundException("Rol", idRol);
        }

        if (!idPermisos.Any())
        {
            return; // No hay nada que remover
        }

        var result = await _permisoRepository.RemoverPermisosDeRolAsync(idRol, idPermisos);
        if (!result)
        {
            throw new BadRequestException($"No se pudieron remover los permisos del rol {rol.Nombre}");
        }

        // Invalidar caché relacionada
        await InvalidateRolPermisosCacheAsync(idRol);
    }

    /// <summary>
    /// Crea un nuevo permiso
    /// </summary>
    public async Task<Permiso> AddPermisoAsync(Permiso permiso)
    {
        // Asignar un nuevo ID si no se proporciona uno
        if (permiso.IdPermiso == Guid.Empty)
        {
            permiso.IdPermiso = Guid.NewGuid();
        }

        // Crear el permiso
        var createdPermiso = await _permisoRepository.AddAsync(permiso);
        await _permisoRepository.SaveChangesAsync();

        // Invalidar caché
        await InvalidateAllPermisosCacheAsync();

        return createdPermiso;
    }

    /// <summary>
    /// Actualiza un permiso existente
    /// </summary>
    public async Task<Permiso> UpdatePermisoAsync(Permiso permiso)
    {
        // Actualizar el permiso
        await _permisoRepository.UpdateAsync(permiso);
        await _permisoRepository.SaveChangesAsync();

        // Invalidar caché
        await InvalidateAllPermisosCacheAsync();

        return permiso;
    }

    /// <summary>
    /// Elimina un permiso
    /// </summary>
    public async Task DeletePermisoAsync(Guid idPermiso)
    {
        var permiso = await _permisoRepository.GetByIdAsync(idPermiso);
        if (permiso == null)
        {
            throw new NotFoundException("Permiso", idPermiso);
        }

        // Verificar si es un permiso predeterminado del sistema
        if (permiso.PredeterminadoSistema)
        {
            throw new ForbiddenAccessException("No se pueden eliminar permisos predeterminados del sistema");
        }

        await _permisoRepository.RemoveAsync(permiso);
        await _permisoRepository.SaveChangesAsync();

        // Invalidar caché
        await InvalidateAllPermisosCacheAsync();
    }

    /// <summary>
    /// Invalida la caché de permisos para un usuario específico
    /// </summary>
    public Task InvalidateUsuarioPermisosCacheAsync(Guid idUsuario)
    {
        string cacheKey = string.Format(CACHE_KEY_PERMISOS_USUARIO, idUsuario);
        _memoryCache.Remove(cacheKey);
        _logger.LogDebug("Caché de permisos invalidada para usuario {IdUsuario}", idUsuario);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Invalida la caché de permisos para un rol específico
    /// </summary>
    public Task InvalidateRolPermisosCacheAsync(Guid idRol)
    {
        string cacheKey = string.Format(CACHE_KEY_PERMISOS_ROL, idRol);
        _memoryCache.Remove(cacheKey);
        _logger.LogDebug("Caché de permisos invalidada para rol {IdRol}", idRol);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Invalida toda la caché de permisos
    /// </summary>
    public Task InvalidateAllPermisosCacheAsync()
    {
        _memoryCache.Remove(CACHE_KEY_PERMISOS_ALL);
        _logger.LogDebug("Caché global de permisos invalidada");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Comparador para permisos usado en HashSet para evitar duplicados
/// </summary>
internal class PermisoEqualityComparer : IEqualityComparer<Permiso>
{
    public bool Equals(Permiso? x, Permiso? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;
        return x.IdPermiso == y.IdPermiso;
    }

    public int GetHashCode(Permiso obj)
    {
        return obj.IdPermiso.GetHashCode();
    }
}
