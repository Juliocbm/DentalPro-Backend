using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Servicio para la gestión básica de permisos (CRUD)
/// </summary>
public interface IPermisoManagementService
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
    /// Verifica si existe un permiso con el ID especificado
    /// </summary>
    Task<bool> ExistsByIdAsync(Guid idPermiso);
    
    /// <summary>
    /// Verifica si existe un permiso con el nombre especificado
    /// </summary>
    Task<bool> ExistsByNameAsync(string nombre);

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
}
