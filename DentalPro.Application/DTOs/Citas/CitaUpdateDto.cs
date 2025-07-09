using System;

namespace DentalPro.Application.DTOs.Citas;

/// <summary>
/// DTO para la actualización de citas
/// </summary>
public class CitaUpdateDto
{
    public Guid IdCita { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public string? Motivo { get; set; }
    public string Estatus { get; set; } = null!;
    public Guid IdPaciente { get; set; }
    public Guid IdDoctor { get; set; }
}
