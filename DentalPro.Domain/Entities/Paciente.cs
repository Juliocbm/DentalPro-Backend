namespace DentalPro.Domain.Entities;

public class Paciente
{
    public Guid IdPaciente { get; set; }
    public string Nombre { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public DateTime FechaNacimiento { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public DateTime FechaAlta { get; set; }

    public Guid IdConsultorio { get; set; }
    public Consultorio? Consultorio { get; set; }
}
