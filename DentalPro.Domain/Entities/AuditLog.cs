using System;

namespace DentalPro.Domain.Entities
{
    /// <summary>
    /// Entidad que representa un registro de auditoría en el sistema
    /// </summary>
    public class AuditLog
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
        
        /// <summary>
        /// Identificador del consultorio al que pertenece la acción (si aplica)
        /// </summary>
        public Guid? IdConsultorio { get; set; }
        
        /// <summary>
        /// Crea una nueva instancia de AuditLog
        /// </summary>
        public AuditLog()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
        }
    }
}
