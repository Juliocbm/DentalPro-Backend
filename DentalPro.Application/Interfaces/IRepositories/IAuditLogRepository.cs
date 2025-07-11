using DentalPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DentalPro.Application.Interfaces.IRepositories
{
    /// <summary>
    /// Repositorio para acceso a datos de registros de auditoría
    /// </summary>
    public interface IAuditLogRepository
    {
        /// <summary>
        /// Crea un nuevo registro de auditoría
        /// </summary>
        /// <param name="auditLog">Registro de auditoría a crear</param>
        Task CreateAsync(AuditLog auditLog);
        
        /// <summary>
        /// Obtiene los registros de auditoría para una entidad específica
        /// </summary>
        /// <param name="entityType">Tipo de entidad</param>
        /// <param name="entityId">Identificador de la entidad</param>
        /// <returns>Lista de registros de auditoría</returns>
        Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId);
        
        /// <summary>
        /// Obtiene los registros de auditoría realizados por un usuario específico
        /// </summary>
        /// <param name="userId">Identificador del usuario</param>
        /// <returns>Lista de registros de auditoría</returns>
        Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId);
        
        /// <summary>
        /// Obtiene registros de auditoría por tipo de acción
        /// </summary>
        /// <param name="action">Tipo de acción</param>
        /// <param name="fromDate">Fecha de inicio (opcional)</param>
        /// <param name="toDate">Fecha de fin (opcional)</param>
        /// <returns>Lista de registros de auditoría</returns>
        Task<IEnumerable<AuditLog>> GetByActionAsync(string action, DateTime? fromDate = null, DateTime? toDate = null);
        
        /// <summary>
        /// Obtiene registros de auditoría para un consultorio específico
        /// </summary>
        /// <param name="consultorioId">Identificador del consultorio</param>
        /// <returns>Lista de registros de auditoría</returns>
        Task<IEnumerable<AuditLog>> GetByConsultorioAsync(Guid consultorioId);
        
        /// <summary>
        /// Devuelve un IQueryable de AuditLog para poder realizar consultas complejas y paginación
        /// </summary>
        /// <returns>IQueryable de registros de auditoría</returns>
        IQueryable<AuditLog> GetQueryable();
    }
}
