namespace DentalPro.Domain.Entities;

public class Consultorio
{
    public Guid IdConsultorio { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? RazonSocial { get; set; }
    public string? RFC { get; set; }
    public string? Logo { get; set; }
    public string? HorarioAtencion { get; set; }
    public string? PlanSuscripcion { get; set; }
    public bool EstatusSuscripcion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Navegación
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();
}
