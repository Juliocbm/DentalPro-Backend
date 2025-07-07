using System;

namespace DentalPro.Application.DTOs.Paciente
{
    public class PacienteUpdateDto
    {
        public Guid IdPaciente { get; set; }
        public string Nombre { get; set; } = null!;
        public string Apellidos { get; set; } = null!;
        public DateTime FechaNacimiento { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
    }
}
