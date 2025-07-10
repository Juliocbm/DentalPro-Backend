using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de gestión de permisos como una fachada que delega en servicios especializados
/// </summary>
public class PermisoService : IPermisoService
{
    private readonly IPermisoManagementService _managementService;
    private readonly IPermisoAssignmentService _assignmentService;
    private readonly IPermisoCacheService _cacheService;
    private readonly ILogger<PermisoService> _logger;

    public PermisoService(
        IPermisoManagementService managementService,
        IPermisoAssignmentService assignmentService,
        IPermisoCacheService cacheService,
        ILogger<PermisoService> logger)
    {
        _managementService = managementService;
        _assignmentService = assignmentService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los permisos existentes
    /// </summary>
    public Task<IEnumerable<Permiso>> GetAllPermisosAsync()
    {
        return _managementService.GetAllPermisosAsync();
    }

    /// <summary>
    /// Obtiene un permiso por su ID
    /// </summary>
    public Task<Permiso?> GetPermisoByIdAsync(Guid idPermiso)
    {
        return _managementService.GetPermisoByIdAsync(idPermiso);
    }

    /// <summary>
    /// Obtiene un permiso por su nombre
    /// </summary>
    public Task<Permiso?> GetPermisoByNombreAsync(string nombre)
    {
        return _managementService.GetPermisoByNombreAsync(nombre);
    }

    /// <summary>
    /// Verifica si existe un permiso con el ID especificado
    /// </summary>
    public Task<bool> ExistsByIdAsync(Guid idPermiso)
    {
        return _managementService.ExistsByIdAsync(idPermiso);
    }
    
    /// <summary>
    /// Verifica si existe un permiso con el nombre especificado
    /// </summary>
    public Task<bool> ExistsByNameAsync(string nombre)
    {
        return _managementService.ExistsByNameAsync(nombre);
    }

    /// <summary>
    /// Obtiene todos los permisos asignados a un rol específico
    /// </summary>
    public Task<IEnumerable<Permiso>> GetPermisosByRolIdAsync(Guid idRol)
    {
        return _assignmentService.GetPermisosByRolIdAsync(idRol);
    }

    /// <summary>
    /// Obtiene todos los permisos asignados a un rol por su nombre
    /// </summary>
    public Task<IEnumerable<Permiso>> GetPermisosByRolNameAsync(string nombreRol)
    {
        return _assignmentService.GetPermisosByRolNameAsync(nombreRol);
    }

    /// <summary>
    /// Obtiene todos los permisos asignados a un usuario específico (combinando permisos de todos sus roles)
    /// </summary>
    public Task<IEnumerable<Permiso>> GetPermisosByUsuarioIdAsync(Guid idUsuario)
    {
        return _assignmentService.GetPermisosByUsuarioIdAsync(idUsuario);
    }

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico por nombre de permiso
    /// </summary>
    public Task<bool> HasUsuarioPermisoByNameAsync(Guid idUsuario, string nombrePermiso)
    {
        return _assignmentService.HasUsuarioPermisoByNameAsync(idUsuario, nombrePermiso);
    }

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico por ID de permiso
    /// </summary>
    public Task<bool> HasUsuarioPermisoByIdAsync(Guid idUsuario, Guid idPermiso)
    {
        return _assignmentService.HasUsuarioPermisoByIdAsync(idUsuario, idPermiso);
    }

    /// <summary>
    /// Verifica si un rol tiene un permiso específico por nombre de permiso
    /// </summary>
    public Task<bool> HasRolPermisoByNameAsync(Guid idRol, string nombrePermiso)
    {
        return _assignmentService.HasRolPermisoByNameAsync(idRol, nombrePermiso);
    }

    /// <summary>
    /// Verifica si un rol tiene un permiso específico por ID de permiso
    /// </summary>
    public Task<bool> HasRolPermisoByIdAsync(Guid idRol, Guid idPermiso)
    {
        return _assignmentService.HasRolPermisoByIdAsync(idRol, idPermiso);
    }

    /// <summary>
    /// Asigna un permiso a un rol
    /// </summary>
    public async Task AssignPermisoToRolAsync(Guid idRol, Guid idPermiso)
    {
        await _assignmentService.AssignPermisoToRolAsync(idRol, idPermiso);
    }

    /// <summary>
    /// Asigna varios permisos a un rol
    /// </summary>
    public async Task AssignPermisosToRolAsync(Guid idRol, IEnumerable<Guid> idPermisos)
    {
        await _assignmentService.AssignPermisosToRolAsync(idRol, idPermisos);
    }

    /// <summary>
    /// Remueve un permiso de un rol
    /// </summary>
    public async Task RemovePermisoFromRolAsync(Guid idRol, Guid idPermiso)
    {
        await _assignmentService.RemovePermisoFromRolAsync(idRol, idPermiso);
    }

    /// <summary>
    /// Remueve varios permisos de un rol
    /// </summary>
    public async Task RemovePermisosFromRolAsync(Guid idRol, IEnumerable<Guid> idPermisos)
    {
        await _assignmentService.RemovePermisosFromRolAsync(idRol, idPermisos);
    }

    /// <summary>
    /// Crea un nuevo permiso
    /// </summary>
    public Task<Permiso> AddPermisoAsync(Permiso permiso)
    {
        return _managementService.AddPermisoAsync(permiso);
    }

    /// <summary>
    /// Actualiza un permiso existente
    /// </summary>
    public Task<Permiso> UpdatePermisoAsync(Permiso permiso)
    {
        return _managementService.UpdatePermisoAsync(permiso);
    }

    /// <summary>
    /// Elimina un permiso
    /// </summary>
    public Task DeletePermisoAsync(Guid idPermiso)
    {
        return _managementService.DeletePermisoAsync(idPermiso);
    }


    
    /// <summary>
    /// Invalida la caché de permisos para un usuario específico
    /// </summary>
    public Task InvalidateUsuarioPermisosCacheAsync(Guid idUsuario)
    {
        return _cacheService.InvalidateUsuarioPermisosCacheAsync(idUsuario);
    }

    /// <summary>
    /// Invalida la caché de permisos para un rol específico
    /// </summary>
    public Task InvalidateRolPermisosCacheAsync(Guid idRol)
    {
        return _cacheService.InvalidateRolPermisosCacheAsync(idRol);
    }

    /// <summary>
    /// Invalida toda la caché de permisos
    /// </summary>
    public Task InvalidateAllPermisosCacheAsync()
    {
        return _cacheService.InvalidateAllPermisosCacheAsync();
    }
}


