using System;

namespace DentalPro.Application.DTOs.Audit
{
    /// <summary>
    /// Filtro para búsqueda avanzada de registros de auditoría
    /// </summary>
    public class AuditLogFilter
    {
        /// <summary>
        /// ID del usuario que realizó la acción
        /// </summary>
        public Guid? UserId { get; set; }
        
        /// <summary>
        /// Tipo de entidad afectada
        /// </summary>
        public string EntityType { get; set; }
        
        /// <summary>
        /// ID de la entidad afectada
        /// </summary>
        public Guid? EntityId { get; set; }
        
        /// <summary>
        /// Tipo de acción realizada (Create, Update, Delete, etc.)
        /// </summary>
        public string Action { get; set; }
        
        /// <summary>
        /// Fecha de inicio para filtrar
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// Fecha de fin para filtrar
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// ID del consultorio
        /// </summary>
        public Guid? ConsultorioId { get; set; }
        
        /// <summary>
        /// Número de página (para paginación)
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// Tamaño de página (para paginación)
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
