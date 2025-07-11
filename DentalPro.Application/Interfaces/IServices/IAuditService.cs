using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DentalPro.Application.Common.Models;
using DentalPro.Application.DTOs.Audit;

namespace DentalPro.Application.Interfaces.IServices
{
    /// <summary>
    /// Servicio para registrar acciones de auditoría en el sistema
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Registra una acción realizada por un usuario
        /// </summary>
        /// <param name="action">Acción realizada (ej: "Create", "Update", "Delete", "AssignPermission")</param>
        /// <param name="entityType">Tipo de entidad afectada (ej: "Usuario", "Rol", "Permiso")</param>
        /// <param name="entityId">Identificador de la entidad afectada</param>
        /// <param name="userId">Identificador del usuario que realizó la acción</param>
        /// <param name="details">Detalles adicionales de la acción en formato JSON (opcional)</param>
        Task RegisterActionAsync(string action, string entityType, Guid entityId, Guid userId, string details = null);
        
        /// <summary>
        /// Obtiene todos los registros de auditoría con paginación
        /// </summary>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Lista paginada de registros de auditoría</returns>
        Task<PaginatedList<AuditLogDto>> GetAllLogsAsync(int pageNumber, int pageSize);
        
        /// <summary>
        /// Obtiene las acciones de auditoría para una entidad específica (sin paginar)
        /// </summary>
        /// <param name="entityType">Tipo de entidad</param>
        /// <param name="entityId">Identificador de la entidad</param>
        /// <returns>Lista de registros de auditoría</returns>
        Task<IEnumerable<AuditLogDto>> GetAuditLogsByEntityAsync(string entityType, Guid entityId);
        
        /// <summary>
        /// Obtiene las acciones de auditoría para una entidad específica con paginación
        /// </summary>
        /// <param name="entityType">Tipo de entidad</param>
        /// <param name="entityId">Identificador de la entidad</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Lista paginada de registros de auditoría</returns>
        Task<PaginatedList<AuditLogDto>> GetLogsByEntityAsync(string entityType, Guid entityId, int pageNumber, int pageSize);
        
        /// <summary>
        /// Obtiene las acciones de auditoría realizadas por un usuario específico (sin paginar)
        /// </summary>
        /// <param name="userId">Identificador del usuario</param>
        /// <returns>Lista de registros de auditoría</returns>
        Task<IEnumerable<AuditLogDto>> GetAuditLogsByUserAsync(Guid userId);
        
        /// <summary>
        /// Obtiene las acciones de auditoría realizadas por un usuario específico con paginación
        /// </summary>
        /// <param name="userId">Identificador del usuario</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Lista paginada de registros de auditoría</returns>
        Task<PaginatedList<AuditLogDto>> GetLogsByUserAsync(Guid userId, int pageNumber, int pageSize);
        
        /// <summary>
        /// Realiza una búsqueda avanzada de registros de auditoría con múltiples filtros
        /// </summary>
        /// <param name="filter">Filtros para la búsqueda</param>
        /// <returns>Lista paginada de registros de auditoría</returns>
        Task<PaginatedList<AuditLogDto>> SearchLogsAsync(AuditLogFilter filter);
    }
}
