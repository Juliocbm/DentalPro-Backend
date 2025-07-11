using System;

namespace DentalPro.Application.DTOs.Audit
{
    /// <summary>
    /// DTO para registros de auditoría
    /// </summary>
    public class AuditLogDto
    {
        /// <summary>
        /// Identificador único del registro de auditoría
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Fecha y hora en que se realizó la acción
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Usuario que realizó la acción
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// Nombre del usuario que realizó la acción
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// Acción realizada (Create, Update, Delete, etc.)
        /// </summary>
        public string Action { get; set; }
        
        /// <summary>
        /// Tipo de entidad afectada (Usuario, Rol, Permiso, etc.)
        /// </summary>
        public string EntityType { get; set; }
        
        /// <summary>
        /// Identificador de la entidad afectada
        /// </summary>
        public Guid EntityId { get; set; }
        
        /// <summary>
        /// Detalles adicionales de la acción (formato JSON)
        /// </summary>
        public string Details { get; set; }
        
        /// <summary>
        /// Dirección IP desde la que se realizó la acción
        /// </summary>
        public string IpAddress { get; set; }
    }
}
