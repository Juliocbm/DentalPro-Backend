using DentalPro.Application.DTOs.Citas;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Servicio especializado para enviar notificaciones relacionadas con citas
/// </summary>
public interface ICitaNotificacionService
{
    /// <summary>
    /// Envía una notificación de confirmación de una nueva cita
    /// </summary>
    Task SendConfirmacionCitaAsync(CitaDto cita);
    
    /// <summary>
    /// Envía una notificación de actualización de una cita
    /// </summary>
    Task SendActualizacionCitaAsync(CitaDto citaActualizada);
    
    /// <summary>
    /// Envía una notificación de cancelación de una cita
    /// </summary>
    Task SendCancelacionCitaAsync(Guid idCita, string motivoCancelacion = null);
    
    /// <summary>
    /// Envía un recordatorio de cita
    /// </summary>
    Task SendRecordatorioCitaAsync(CitaDto cita);
}
