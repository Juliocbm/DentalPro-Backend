using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Servicio para la gestión de asignaciones de permisos a roles y usuarios
/// </summary>
public interface IPermisoAssignmentService
{
    /// <summary>
    /// Obtiene todos los permisos asignados a un rol específico
    /// </summary>
    Task<IEnumerable<Permiso>> GetPermisosByRolIdAsync(Guid idRol);

    /// <summary>
    /// Obtiene todos los permisos asignados a un rol por su nombre
    /// </summary>
    Task<IEnumerable<Permiso>> GetPermisosByRolNameAsync(string nombreRol);

    /// <summary>
    /// Verifica si un rol tiene un permiso específico por nombre de permiso
    /// </summary>
    Task<bool> HasRolPermisoByNameAsync(Guid idRol, string nombrePermiso);

    /// <summary>
    /// Verifica si un rol tiene un permiso específico por ID de permiso
    /// </summary>
    Task<bool> HasRolPermisoByIdAsync(Guid idRol, Guid idPermiso);

    /// <summary>
    /// Asigna un permiso a un rol
    /// </summary>
    Task<bool> AssignPermisoToRolAsync(Guid idRol, Guid idPermiso);

    /// <summary>
    /// Asigna varios permisos a un rol
    /// </summary>
    Task<bool> AssignPermisosToRolAsync(Guid idRol, IEnumerable<Guid> permisoIds);

    /// <summary>
    /// Remueve un permiso de un rol
    /// </summary>
    Task<bool> RemovePermisoFromRolAsync(Guid idRol, Guid idPermiso);

    /// <summary>
    /// Remueve varios permisos de un rol
    /// </summary>
    Task<bool> RemovePermisosFromRolAsync(Guid idRol, IEnumerable<Guid> permisoIds);
    
    /// <summary>
    /// Obtiene todos los permisos asignados a un usuario específico (combinando permisos de todos sus roles)
    /// </summary>
    Task<IEnumerable<Permiso>> GetPermisosByUsuarioIdAsync(Guid idUsuario);

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico por nombre de permiso
    /// </summary>
    Task<bool> HasUsuarioPermisoByNameAsync(Guid idUsuario, string nombrePermiso);

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico por ID de permiso
    /// </summary>
    Task<bool> HasUsuarioPermisoByIdAsync(Guid idUsuario, Guid idPermiso);
}
