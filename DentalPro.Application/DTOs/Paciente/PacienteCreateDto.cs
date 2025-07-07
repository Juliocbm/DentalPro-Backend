using System;

namespace DentalPro.Application.DTOs.Paciente
{
    public class PacienteCreateDto
    {
        public string Nombre { get; set; } = null!;
        public string Apellidos { get; set; } = null!;
        public DateTime FechaNacimiento { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
    }
}
