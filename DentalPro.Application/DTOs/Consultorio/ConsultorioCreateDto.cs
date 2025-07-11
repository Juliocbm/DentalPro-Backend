using System;

namespace DentalPro.Application.DTOs.Consultorio
{
    /// <summary>
    /// DTO para crear un nuevo consultorio
    /// </summary>
    public class ConsultorioCreateDto
    {
        /// <summary>
        /// Nombre del consultorio
        /// </summary>
        public string Nombre { get; set; } = string.Empty;
        
        /// <summary>
        /// Dirección del consultorio
        /// </summary>
        public string? Direccion { get; set; }
        
        /// <summary>
        /// Teléfono del consultorio
        /// </summary>
        public string? Telefono { get; set; }
        
        /// <summary>
        /// Correo electrónico del consultorio
        /// </summary>
        public string? Email { get; set; }
        
        /// <summary>
        /// Indica si el consultorio está activo
        /// </summary>
        public bool Activo { get; set; } = true;
    }
}
