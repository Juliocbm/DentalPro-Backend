using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces;

/// <summary>
/// Servicio para la gestión de consultorios
/// </summary>
public interface IConsultorioService
{
    /// <summary>
    /// Obtiene todos los consultorios
    /// </summary>
    Task<IEnumerable<Consultorio>> GetAllAsync();
    
    /// <summary>
    /// Obtiene un consultorio por su ID
    /// </summary>
    Task<Consultorio?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Verifica si existe un consultorio con el ID especificado
    /// </summary>
    /// <param name="id">ID del consultorio a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    Task<bool> ExistsByIdAsync(Guid id);
}
