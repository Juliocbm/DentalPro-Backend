using System;

namespace DentalPro.Application.DTOs.Citas;

/// <summary>
/// DTO para la creación de citas
/// </summary>
public class CitaCreateDto
{
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public string? Motivo { get; set; }
    public Guid IdPaciente { get; set; }
    // El IdUsuario se obtendrá del token JWT del usuario autenticado
}
