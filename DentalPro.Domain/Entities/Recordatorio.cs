using System;

namespace DentalPro.Domain.Entities;

public class Recordatorio
{
    public Guid IdRecordatorio { get; set; }
    public Guid IdCita { get; set; }
    public string? Tipo { get; set; }
    public string? Medio { get; set; }
    public bool Enviado { get; set; }
    public DateTime? FechaEnvio { get; set; }
    
    // Propiedad de navegación
    public virtual Cita Cita { get; set; } = null!;
}
