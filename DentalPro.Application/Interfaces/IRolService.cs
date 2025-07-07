using DentalPro.Application.DTOs.Rol;
using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces;

/// <summary>
/// Servicio para la gestión de roles
/// </summary>
public interface IRolService
{
    Task<IEnumerable<Rol>> GetAllAsync();
    Task<Rol?> GetByIdAsync(Guid id);
    Task<Rol?> GetByNombreAsync(string nombre);
    Task<RolDto> CreateAsync(RolDto rol);
    Task<bool> UpdateAsync(RolDto rolDto);
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// Verifica si existe un rol con el nombre especificado
    /// </summary>
    /// <param name="nombre">Nombre del rol a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    Task<bool> ExistsByNameAsync(string nombre);
    
    /// <summary>
    /// Verifica si existe un rol con el ID especificado
    /// </summary>
    /// <param name="id">ID del rol a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    Task<bool> ExistsByIdAsync(Guid id);
}
