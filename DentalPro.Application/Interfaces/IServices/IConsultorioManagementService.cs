using DentalPro.Application.DTOs.Consultorio;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Servicio especializado para operaciones CRUD de consultorios
/// </summary>
public interface IConsultorioManagementService
{
    /// <summary>
    /// Obtiene todos los consultorios
    /// </summary>
    Task<IEnumerable<ConsultorioDto>> GetAllAsync();
    
    /// <summary>
    /// Obtiene un consultorio por su ID
    /// </summary>
    Task<ConsultorioDto?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Crea un nuevo consultorio
    /// </summary>
    /// <param name="consultorioDto">DTO con los datos del consultorio a crear</param>
    /// <returns>DTO del consultorio creado</returns>
    Task<ConsultorioDto> CreateAsync(ConsultorioCreateDto consultorioDto);
    
    /// <summary>
    /// Actualiza un consultorio existente
    /// </summary>
    /// <param name="consultorioDto">DTO con los datos del consultorio a actualizar</param>
    /// <returns>DTO del consultorio actualizado</returns>
    Task<ConsultorioDto> UpdateAsync(ConsultorioUpdateDto consultorioDto);
    
    /// <summary>
    /// Elimina un consultorio por su ID
    /// </summary>
    /// <param name="id">ID del consultorio a eliminar</param>
    /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// Verifica si existe un consultorio con el ID especificado
    /// </summary>
    /// <param name="id">ID del consultorio a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    Task<bool> ExistsByIdAsync(Guid id);
}
