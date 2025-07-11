using DentalPro.Api.Infrastructure.Authorization;
using DentalPro.Application.Common.Models;
using DentalPro.Application.Common.Permissions;
using DentalPro.Application.DTOs.Audit;
using DentalPro.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DentalPro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        /// <summary>
        /// Obtiene todos los registros de auditoría con paginación
        /// </summary>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Lista paginada de registros de auditoría</returns>
        [HttpGet]
        [RequirePermiso(AuditPermissions.ViewAll)]
        public async Task<ActionResult<PaginatedList<AuditLogDto>>> GetAllLogs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var logs = await _auditService.GetAllLogsAsync(pageNumber, pageSize);
            return Ok(logs);
        }

        /// <summary>
        /// Obtiene los registros de auditoría para una entidad específica
        /// </summary>
        /// <param name="entityType">Tipo de entidad</param>
        /// <param name="entityId">ID de la entidad</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Lista paginada de registros de auditoría de la entidad</returns>
        [HttpGet("entity/{entityType}/{entityId}")]
        [RequirePermiso(AuditPermissions.ViewEntity)]
        public async Task<ActionResult<PaginatedList<AuditLogDto>>> GetEntityLogs(
            string entityType, 
            Guid entityId,
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10)
        {
            var logs = await _auditService.GetLogsByEntityAsync(entityType, entityId, pageNumber, pageSize);
            return Ok(logs);
        }

        /// <summary>
        /// Obtiene los registros de auditoría para un usuario específico
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Lista paginada de registros de auditoría del usuario</returns>
        [HttpGet("user/{userId}")]
        [RequirePermiso(AuditPermissions.ViewByUser)]
        public async Task<ActionResult<PaginatedList<AuditLogDto>>> GetUserLogs(
            Guid userId,
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10)
        {
            var logs = await _auditService.GetLogsByUserAsync(userId, pageNumber, pageSize);
            return Ok(logs);
        }
    }
}
