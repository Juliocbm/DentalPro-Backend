using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Interfaz para el servicio de gestión de permisos
/// </summary>
public interface IPermisoService
{
    /// <summary>
    /// Obtiene todos los permisos existentes
    /// </summary>
    Task<IEnumerable<Permiso>> GetAllPermisosAsync();

    /// <summary>
    /// Obtiene un permiso por su ID
    /// </summary>
    Task<Permiso?> GetPermisoByIdAsync(Guid idPermiso);

    /// <summary>
    /// Obtiene un permiso por su nombre
    /// </summary>
    Task<Permiso?> GetPermisoByNombreAsync(string nombre);

    /// <summary>
    /// Verifica si existe un permiso con el nombre especificado
    /// </summary>
    Task<bool> ExistsPermisoByNombreAsync(string nombre);

    /// <summary>
    /// Obtiene todos los permisos asignados a un rol específico
    /// </summary>
    Task<IEnumerable<Permiso>> GetPermisosByRolIdAsync(Guid idRol);

    /// <summary>
    /// Obtiene todos los permisos asignados a un rol por su nombre
    /// </summary>
    Task<IEnumerable<Permiso>> GetPermisosByRolNombreAsync(string nombreRol);

    /// <summary>
    /// Obtiene todos los permisos asignados a un usuario específico (combinando permisos de todos sus roles)
    /// </summary>
    Task<IEnumerable<Permiso>> GetPermisosByUsuarioIdAsync(Guid idUsuario);

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico por nombre de permiso
    /// </summary>
    Task<bool> HasUsuarioPermisoAsync(Guid idUsuario, string nombrePermiso);

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico por ID de permiso
    /// </summary>
    Task<bool> HasUsuarioPermisoAsync(Guid idUsuario, Guid idPermiso);

    /// <summary>
    /// Verifica si un rol tiene un permiso específico por nombre de permiso
    /// </summary>
    Task<bool> HasRolPermisoAsync(Guid idRol, string nombrePermiso);

    /// <summary>
    /// Verifica si un rol tiene un permiso específico por ID de permiso
    /// </summary>
    Task<bool> HasRolPermisoAsync(Guid idRol, Guid idPermiso);

    /// <summary>
    /// Asigna un permiso a un rol
    /// </summary>
    Task AssignPermisoToRolAsync(Guid idRol, Guid idPermiso);

    /// <summary>
    /// Asigna varios permisos a un rol
    /// </summary>
    Task AssignPermisosToRolAsync(Guid idRol, IEnumerable<Guid> idPermisos);

    /// <summary>
    /// Remueve un permiso de un rol
    /// </summary>
    Task RemovePermisoFromRolAsync(Guid idRol, Guid idPermiso);

    /// <summary>
    /// Remueve varios permisos de un rol
    /// </summary>
    Task RemovePermisosFromRolAsync(Guid idRol, IEnumerable<Guid> idPermisos);

    /// <summary>
    /// Crea un nuevo permiso
    /// </summary>
    Task<Permiso> AddPermisoAsync(Permiso permiso);

    /// <summary>
    /// Actualiza un permiso existente
    /// </summary>
    Task<Permiso> UpdatePermisoAsync(Permiso permiso);

    /// <summary>
    /// Elimina un permiso
    /// </summary>
    Task DeletePermisoAsync(Guid idPermiso);

    /// <summary>
    /// Invalida la caché de permisos para un usuario específico
    /// </summary>
    Task InvalidateUsuarioPermisosCacheAsync(Guid idUsuario);

    /// <summary>
    /// Invalida la caché de permisos para un rol específico
    /// </summary>
    Task InvalidateRolPermisosCacheAsync(Guid idRol);

    /// <summary>
    /// Invalida toda la caché de permisos
    /// </summary>
    Task InvalidateAllPermisosCacheAsync();
}
