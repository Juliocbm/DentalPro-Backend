using System;
using System.Collections.Generic;

namespace DentalPro.Domain.Entities;

public class Cita
{
    public Guid IdCita { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public string? Motivo { get; set; }
    public string Estatus { get; set; } = null!;
    public Guid IdPaciente { get; set; }
    public Guid IdDoctor { get; set; }

    // Propiedades de navegación
    public virtual Paciente Paciente { get; set; } = null!;
    public virtual Usuario Doctor { get; set; } = null!;
    public virtual ICollection<Recordatorio> Recordatorios { get; set; } = new List<Recordatorio>();
}
