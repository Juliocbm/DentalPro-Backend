using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using DentalPro.Application.Common.Models;
using DentalPro.Application.DTOs.Audit;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DentalPro.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de auditoría
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(
            IAuditLogRepository auditLogRepository,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IHttpContextAccessor httpContextAccessor)
        {
            _auditLogRepository = auditLogRepository;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public async Task RegisterActionAsync(string action, string entityType, Guid entityId, Guid userId, string details = null)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                Details = details,
                IpAddress = GetClientIpAddress(),
                ConsultorioId = _currentUserService.GetCurrentConsultorioId()
            };

            await _auditLogRepository.CreateAsync(auditLog);
        }

        /// <inheritdoc />
        public async Task<PaginatedList<AuditLogDto>> GetAllLogsAsync(int pageNumber, int pageSize)
        {
            // Obtenemos los logs y aplicamos paginación
            var query = _auditLogRepository.GetQueryable()
                .OrderByDescending(x => x.Timestamp);

            // Crear lista paginada
            var paginatedLogs = await PaginatedList<AuditLog>.CreateAsync(
                query,
                pageNumber,
                pageSize,
                async q => await q.CountAsync(),
                async q => await q.ToListAsync()
            );

            // Mapear a DTOs
            var dtos = _mapper.Map<List<AuditLogDto>>(paginatedLogs.Items);

            // Crear y devolver la lista paginada de DTOs
            return new PaginatedList<AuditLogDto>(dtos, paginatedLogs.TotalCount, paginatedLogs.PageNumber, paginatedLogs.PageSize);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuditLogDto>> GetAuditLogsByEntityAsync(string entityType, Guid entityId)
        {
            var logs = await _auditLogRepository.GetByEntityAsync(entityType, entityId);
            return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
        }

        /// <inheritdoc />
        public async Task<PaginatedList<AuditLogDto>> GetLogsByEntityAsync(string entityType, Guid entityId, int pageNumber, int pageSize)
        {
            // Obtenemos los logs por entidad y aplicamos paginación
            var query = _auditLogRepository.GetQueryable()
                .Where(x => x.EntityType == entityType && x.EntityId == entityId)
                .OrderByDescending(x => x.Timestamp);

            // Crear lista paginada
            var paginatedLogs = await PaginatedList<AuditLog>.CreateAsync(
                query,
                pageNumber,
                pageSize,
                async q => await q.CountAsync(),
                async q => await q.ToListAsync()
            );

            // Mapear a DTOs
            var dtos = _mapper.Map<List<AuditLogDto>>(paginatedLogs.Items);

            // Crear y devolver la lista paginada de DTOs
            return new PaginatedList<AuditLogDto>(dtos, paginatedLogs.TotalCount, paginatedLogs.PageNumber, paginatedLogs.PageSize);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuditLogDto>> GetAuditLogsByUserAsync(Guid userId)
        {
            var logs = await _auditLogRepository.GetByUserAsync(userId);
            return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
        }

        /// <inheritdoc />
        public async Task<PaginatedList<AuditLogDto>> GetLogsByUserAsync(Guid userId, int pageNumber, int pageSize)
        {
            // Obtenemos los logs por usuario y aplicamos paginación
            var query = _auditLogRepository.GetQueryable()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Timestamp);

            // Crear lista paginada
            var paginatedLogs = await PaginatedList<AuditLog>.CreateAsync(
                query,
                pageNumber,
                pageSize,
                async q => await q.CountAsync(),
                async q => await q.ToListAsync()
            );

            // Mapear a DTOs
            var dtos = _mapper.Map<List<AuditLogDto>>(paginatedLogs.Items);

            // Crear y devolver la lista paginada de DTOs
            return new PaginatedList<AuditLogDto>(dtos, paginatedLogs.TotalCount, paginatedLogs.PageNumber, paginatedLogs.PageSize);
        }

        /// <summary>
        /// Obtiene la dirección IP del cliente actual
        /// </summary>
        private string GetClientIpAddress()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return "Unknown";
            }

            var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
            
            // Si está detrás de un proxy, intentar obtener la IP real
            var forwardedHeader = _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                // El formato puede ser múltiples IPs separadas por comas
                ipAddress = forwardedHeader.Split(',')[0].Trim();
            }

            return ipAddress ?? "Unknown";
        }
    }
}
