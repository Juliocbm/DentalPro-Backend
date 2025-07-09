using DentalPro.Application.DTOs.Recordatorios;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Interfaz para el servicio de recordatorios de citas
/// </summary>
public interface IRecordatorioService
{
    /// <summary>
    /// Obtiene todos los recordatorios de una cita
    /// </summary>
    Task<IEnumerable<RecordatorioDto>> GetByCitaAsync(Guid idCita);
    
    /// <summary>
    /// Obtiene un recordatorio por su ID
    /// </summary>
    Task<RecordatorioDto> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Crea un nuevo recordatorio
    /// </summary>
    Task<RecordatorioDto> CreateAsync(RecordatorioCreateDto recordatorioDto);
    
    /// <summary>
    /// Actualiza un recordatorio existente
    /// </summary>
    Task<RecordatorioDto> UpdateAsync(RecordatorioUpdateDto recordatorioDto);
    
    /// <summary>
    /// Marca un recordatorio como enviado
    /// </summary>
    Task MarkAsSentAsync(Guid id);
    
    /// <summary>
    /// Obtiene recordatorios pendientes de envío
    /// </summary>
    Task<IEnumerable<RecordatorioDto>> GetPendingAsync();
    
    /// <summary>
    /// Elimina un recordatorio
    /// </summary>
    Task DeleteAsync(Guid id);
}
