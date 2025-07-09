using System;

namespace DentalPro.Application.DTOs.Citas;

/// <summary>
/// DTO para mostrar detalles completos de una cita incluyendo información de paciente y doctor
/// </summary>
public class CitaDetailDto
{
    public Guid IdCita { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public string? Motivo { get; set; }
    public string Estatus { get; set; } = null!;
    
    // Información del paciente
    public Guid IdPaciente { get; set; }
    public string NombreCompletoPaciente { get; set; } = null!;
    public string EmailPaciente { get; set; } = null!;
    public string? TelefonoPaciente { get; set; }
    
    // Información del doctor
    public Guid IdDoctor { get; set; }
    public string NombreCompletoDoctor { get; set; } = null!;
    public string EmailDoctor { get; set; } = null!;
    
    // Información adicional
    public int DuracionMinutos { get; set; }
    public bool TieneRecordatorios { get; set; }
}
