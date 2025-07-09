namespace DentalPro.Application.DTOs.Recordatorios;

/// <summary>
/// DTO para recordatorios de citas
/// </summary>
public class RecordatorioDto
{
    /// <summary>
    /// Identificador único del recordatorio
    /// </summary>
    public Guid IdRecordatorio { get; set; }
    
    /// <summary>
    /// Identificador de la cita asociada
    /// </summary>
    public Guid IdCita { get; set; }
    
    /// <summary>
    /// Tipo de recordatorio (Email, SMS, etc.)
    /// </summary>
    public string Tipo { get; set; } = null!;
    
    /// <summary>
    /// Fecha y hora programada para el envío
    /// </summary>
    public DateTime FechaProgramada { get; set; }
    
    /// <summary>
    /// Mensaje del recordatorio
    /// </summary>
    public string Mensaje { get; set; } = null!;
    
    /// <summary>
    /// Indica si el recordatorio ya fue enviado
    /// </summary>
    public bool Enviado { get; set; }
    
    /// <summary>
    /// Fecha y hora del envío (null si no ha sido enviado)
    /// </summary>
    public DateTime? FechaEnvio { get; set; }
}
