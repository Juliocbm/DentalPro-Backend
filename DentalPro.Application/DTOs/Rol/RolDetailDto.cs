using System;
using System.Collections.Generic;
using DentalPro.Application.DTOs.Permiso;

namespace DentalPro.Application.DTOs.Rol
{
    /// <summary>
    /// DTO que contiene información detallada de un rol
    /// </summary>
    public class RolDetailDto
    {
        /// <summary>
        /// Identificador único del rol
        /// </summary>
        public Guid IdRol { get; set; }
        
        /// <summary>
        /// Nombre del rol
        /// </summary>
        public string Nombre { get; set; } = string.Empty;
        
        /// <summary>
        /// Descripción del rol
        /// </summary>
        public string? Descripcion { get; set; }
        
        /// <summary>
        /// Fecha de creación
        /// </summary>
        public DateTime FechaCreacion { get; set; }
        
        /// <summary>
        /// Permisos asignados a este rol
        /// </summary>
        public ICollection<PermisoDto> Permisos { get; set; } = new List<PermisoDto>();
    }
}
