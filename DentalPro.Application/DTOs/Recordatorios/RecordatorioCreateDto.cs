namespace DentalPro.Application.DTOs.Recordatorios;

/// <summary>
/// DTO para crear recordatorios de citas
/// </summary>
public class RecordatorioCreateDto
{
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
}
