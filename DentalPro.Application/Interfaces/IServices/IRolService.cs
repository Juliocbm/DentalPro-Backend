using DentalPro.Application.DTOs.Rol;
using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Servicio para la gestión de roles
/// </summary>
public interface IRolService
{
    /// <summary>
    /// Obtiene todos los roles
    /// </summary>
    Task<IEnumerable<RolDto>> GetAllAsync();
    
    /// <summary>
    /// Obtiene un rol por su ID
    /// </summary>
    Task<RolDto?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Obtiene un rol por su nombre
    /// </summary>
    Task<RolDto?> GetByNombreAsync(string nombre);
    
    /// <summary>
    /// Crea un nuevo rol
    /// </summary>
    Task<RolDto> CreateAsync(RolCreateDto rolCreateDto);
    
    /// <summary>
    /// Actualiza un rol existente
    /// </summary>
    Task<RolDto> UpdateAsync(RolUpdateDto rolUpdateDto);
    
    /// <summary>
    /// Elimina un rol por su ID
    /// </summary>
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
