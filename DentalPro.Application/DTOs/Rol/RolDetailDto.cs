using System;

namespace DentalPro.Application.DTOs.Rol
{
    /// <summary>
    /// DTO que contiene información detallada de un rol (ID y nombre)
    /// </summary>
    public class RolDetailDto
    {
        /// <summary>
        /// Identificador único del rol
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Nombre del rol
        /// </summary>
        public string Nombre { get; set; } = string.Empty;
    }
}
