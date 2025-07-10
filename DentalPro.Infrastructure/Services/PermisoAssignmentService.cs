using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio para la gestión de asignaciones de permisos a roles y usuarios
/// </summary>
public class PermisoAssignmentService : IPermisoAssignmentService
{
    private readonly IPermisoRepository _permisoRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPermisoCacheService _cacheService;
    private readonly ILogger<PermisoAssignmentService> _logger;

    public PermisoAssignmentService(
        IPermisoRepository permisoRepository,
        IRolRepository rolRepository,
        IUsuarioRepository usuarioRepository,
        IPermisoCacheService cacheService,
        ILogger<PermisoAssignmentService> logger)
    {
        _permisoRepository = permisoRepository;
        _rolRepository = rolRepository;
        _usuarioRepository = usuarioRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los permisos asignados a un rol específico
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetPermisosByRolIdAsync(Guid idRol)
    {
        var cacheKey = _cacheService.GetRolPermisosCacheKey(idRol);
        return await _cacheService.GetOrCreateCacheAsync(
            cacheKey,
            async () => await _permisoRepository.GetByRolIdAsync(idRol),
            TimeSpan.FromMinutes(15));
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
    /// Verifica si un rol tiene un permiso específico por nombre de permiso
    /// </summary>
    public async Task<bool> HasRolPermisoByNameAsync(Guid idRol, string nombrePermiso)
    {
        var permisosRol = await GetPermisosByRolIdAsync(idRol);
        return permisosRol.Any(p => p.Nombre.ToLower() == nombrePermiso.ToLower());
    }

    /// <summary>
    /// Verifica si un rol tiene un permiso específico por ID de permiso
    /// </summary>
    public async Task<bool> HasRolPermisoByIdAsync(Guid idRol, Guid idPermiso)
    {
        var permisosRol = await GetPermisosByRolIdAsync(idRol);
        return permisosRol.Any(p => p.IdPermiso == idPermiso);
    }

    /// <summary>
    /// Asigna un permiso a un rol
    /// </summary>
    public async Task<bool> AssignPermisoToRolAsync(Guid idRol, Guid idPermiso)
    {
        // Verificar que el rol existe
        var rol = await _rolRepository.GetByIdAsync(idRol);
        if (rol == null)
        {
            throw new NotFoundException("Rol", idRol);
        }

        // Verificar que el permiso existe
        var permiso = await _permisoRepository.GetByIdAsync(idPermiso);
        if (permiso == null)
        {
            throw new NotFoundException("Permiso", idPermiso);
        }

        // Asignar el permiso al rol
        bool result = await _permisoRepository.AsignarPermisoARolAsync(idRol, idPermiso);
        
        if (result)
        {
            // Invalidar la caché de permisos del rol
            await _cacheService.InvalidateRolPermisosCacheAsync(idRol);
            // Invalidar la caché de permisos de todos los usuarios que tienen este rol
            await _cacheService.InvalidateUsuariosCacheByRolIdAsync(idRol);
        }

        return result;
    }

    /// <summary>
    /// Asigna varios permisos a un rol
    /// </summary>
    public async Task<bool> AssignPermisosToRolAsync(Guid idRol, IEnumerable<Guid> permisoIds)
    {
        // Verificar que el rol existe
        var rol = await _rolRepository.GetByIdAsync(idRol);
        if (rol == null)
        {
            throw new NotFoundException("Rol", idRol);
        }

        // Verificar que todos los permisos existen
        foreach (var idPermiso in permisoIds)
        {
            var permiso = await _permisoRepository.GetByIdAsync(idPermiso);
            if (permiso == null)
            {
                throw new NotFoundException("Permiso", idPermiso);
            }
        }

        // Asignar los permisos al rol
        bool result = await _permisoRepository.AsignarPermisosARolAsync(idRol, permisoIds);
        
        if (result)
        {
            // Invalidar la caché de permisos del rol
            await _cacheService.InvalidateRolPermisosCacheAsync(idRol);
            // Invalidar la caché de permisos de todos los usuarios que tienen este rol
            await _cacheService.InvalidateUsuariosCacheByRolIdAsync(idRol);
        }

        return result;
    }

    /// <summary>
    /// Remueve un permiso de un rol
    /// </summary>
    public async Task<bool> RemovePermisoFromRolAsync(Guid idRol, Guid idPermiso)
    {
        // Verificar que el rol existe
        var rol = await _rolRepository.GetByIdAsync(idRol);
        if (rol == null)
        {
            throw new NotFoundException("Rol", idRol);
        }

        // Verificar que el permiso existe
        var permiso = await _permisoRepository.GetByIdAsync(idPermiso);
        if (permiso == null)
        {
            throw new NotFoundException("Permiso", idPermiso);
        }

        // Remover el permiso del rol
        bool result = await _permisoRepository.RemoverPermisoDeRolAsync(idRol, idPermiso);
        
        if (result)
        {
            // Invalidar la caché de permisos del rol
            await _cacheService.InvalidateRolPermisosCacheAsync(idRol);
            // Invalidar la caché de permisos de todos los usuarios que tienen este rol
            await _cacheService.InvalidateUsuariosCacheByRolIdAsync(idRol);
        }

        return result;
    }

    /// <summary>
    /// Remueve varios permisos de un rol
    /// </summary>
    public async Task<bool> RemovePermisosFromRolAsync(Guid idRol, IEnumerable<Guid> permisoIds)
    {
        // Verificar que el rol existe
        var rol = await _rolRepository.GetByIdAsync(idRol);
        if (rol == null)
        {
            throw new NotFoundException("Rol", idRol);
        }

        // Remover los permisos del rol
        bool result = await _permisoRepository.RemoverPermisosDeRolAsync(idRol, permisoIds);
        
        if (result)
        {
            // Invalidar la caché de permisos del rol
            await _cacheService.InvalidateRolPermisosCacheAsync(idRol);
            // Invalidar la caché de permisos de todos los usuarios que tienen este rol
            await _cacheService.InvalidateUsuariosCacheByRolIdAsync(idRol);
        }

        return result;
    }
    
    /// <summary>
    /// Obtiene todos los permisos asignados a un usuario específico (combinando permisos de todos sus roles)
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetPermisosByUsuarioIdAsync(Guid idUsuario)
    {
        string cacheKey = _cacheService.GetUsuarioPermisosCacheKey(idUsuario);

        return await _cacheService.GetOrCreateCacheAsync(
            cacheKey,
            async () =>
            {
                // Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdWithRolesAndPermisosAsync(idUsuario);
                if (usuario == null)
                {
                    throw new NotFoundException("Usuario", idUsuario);
                }

                // Obtener todos los permisos directos del usuario
                var permisosDirectos = await _usuarioRepository.GetUserPermisosAsync(idUsuario);

                // Obtener todos los roles del usuario
                var rolesUsuario = await _usuarioRepository.GetUserRolesAsync(idUsuario);

                // Obtener todos los permisos de cada rol y combinarlos
                var permisosRoles = new List<Permiso>();
                foreach (var rolNombre in rolesUsuario)
                {
                    var rol = await _rolRepository.GetByNombreAsync(rolNombre);
                    if (rol != null)
                    {
                        var permisosRol = await GetPermisosByRolIdAsync(rol.IdRol);
                        permisosRoles.AddRange(permisosRol);
                    }
                }

                // Combinar permisos directos y de roles, eliminando duplicados
                var todosPermisos = new HashSet<Permiso>(permisosRoles, new PermisoEqualityComparer());
                foreach (var permisoNombre in permisosDirectos)
                {
                    var permiso = await _permisoRepository.GetByNombreAsync(permisoNombre);
                    if (permiso != null)
                    {
                        todosPermisos.Add(permiso);
                    }
                }

                return todosPermisos;
            },
            TimeSpan.FromMinutes(10));
    }

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico por nombre de permiso
    /// </summary>
    public async Task<bool> HasUsuarioPermisoByNameAsync(Guid idUsuario, string nombrePermiso)
    {
        var permisosUsuario = await GetPermisosByUsuarioIdAsync(idUsuario);
        return permisosUsuario.Any(p => p.Nombre.ToLower() == nombrePermiso.ToLower());
    }

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico por ID de permiso
    /// </summary>
    public async Task<bool> HasUsuarioPermisoByIdAsync(Guid idUsuario, Guid idPermiso)
    {
        var permisosUsuario = await GetPermisosByUsuarioIdAsync(idUsuario);
        return permisosUsuario.Any(p => p.IdPermiso == idPermiso);
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
