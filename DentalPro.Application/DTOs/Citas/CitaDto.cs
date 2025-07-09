using System;

namespace DentalPro.Application.DTOs.Citas;

/// <summary>
/// DTO para operaciones generales con citas
/// </summary>
public class CitaDto
{
    public Guid IdCita { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public string? Motivo { get; set; }
    public string Estatus { get; set; } = null!;
    public Guid IdPaciente { get; set; }
    public Guid IdDoctor { get; set; }
    
    // Propiedades calculadas en el mapeo
    public string NombrePaciente { get; set; } = null!;
    public string NombreDoctor { get; set; } = null!;
}
