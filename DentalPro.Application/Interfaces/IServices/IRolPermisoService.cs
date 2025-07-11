using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Servicio para la gestión de permisos asignados a roles
/// </summary>
public interface IRolPermisoService
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
    Task<bool> AsignarPermisoAsync(Guid idRol, Guid idPermiso);

    /// <summary>
    /// Asigna varios permisos a un rol
    /// </summary>
    Task<bool> AsignarPermisosAsync(Guid idRol, IEnumerable<Guid> idsPermisos);

    /// <summary>
    /// Remueve un permiso de un rol
    /// </summary>
    Task<bool> RemoverPermisoAsync(Guid idRol, Guid idPermiso);

    /// <summary>
    /// Remueve varios permisos de un rol
    /// </summary>
    Task<bool> RemoverPermisosAsync(Guid idRol, IEnumerable<Guid> idsPermisos);
}
