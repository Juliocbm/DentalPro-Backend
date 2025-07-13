using DentalPro.Application.DTOs.Recordatorios;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Interfaz para el servicio especializado en notificaciones de recordatorios
/// </summary>
public interface IRecordatorioNotificacionService
{
    /// <summary>
    /// Marca un recordatorio como enviado
    /// </summary>
    Task MarkAsSentAsync(Guid id);
    
    /// <summary>
    /// Obtiene recordatorios pendientes de envío
    /// </summary>
    Task<IEnumerable<RecordatorioDto>> GetPendingAsync();
}
