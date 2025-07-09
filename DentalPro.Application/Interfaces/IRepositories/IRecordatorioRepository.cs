using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IRepositories;

/// <summary>
/// Interfaz para el repositorio de recordatorios
/// </summary>
public interface IRecordatorioRepository
{
    /// <summary>
    /// Obtiene todos los recordatorios de una cita
    /// </summary>
    Task<IEnumerable<Recordatorio>> GetByCitaAsync(Guid idCita);
    
    /// <summary>
    /// Obtiene un recordatorio por su ID
    /// </summary>
    Task<Recordatorio?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Verifica si existe un recordatorio con el ID especificado
    /// </summary>
    Task<bool> ExistsByIdAsync(Guid id);
    
    /// <summary>
    /// Obtiene recordatorios pendientes de envío
    /// </summary>
    Task<IEnumerable<Recordatorio>> GetPendingAsync();
    
    /// <summary>
    /// Agrega un nuevo recordatorio
    /// </summary>
    Task<Recordatorio> AddAsync(Recordatorio recordatorio);
    
    /// <summary>
    /// Actualiza un recordatorio existente
    /// </summary>
    Task<Recordatorio> UpdateAsync(Recordatorio recordatorio);
    
    /// <summary>
    /// Marca un recordatorio como enviado
    /// </summary>
    Task MarkAsSentAsync(Guid id);
    
    /// <summary>
    /// Elimina un recordatorio
    /// </summary>
    Task DeleteAsync(Guid id);
}
