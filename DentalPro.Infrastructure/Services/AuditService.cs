using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Models;
using DentalPro.Application.DTOs.Audit;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DentalPro.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de auditoría
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUserResolver _currentUserResolver;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            IAuditLogRepository auditLogRepository,
            IMapper mapper,
            ICurrentUserResolver currentUserResolver,
            IHttpContextAccessor httpContextAccessor,
            IUsuarioRepository usuarioRepository,
            ILogger<AuditService> logger)
        {
            _auditLogRepository = auditLogRepository;
            _mapper = mapper;
            _currentUserResolver = currentUserResolver;
            _httpContextAccessor = httpContextAccessor;
            _usuarioRepository = usuarioRepository;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task RegisterActionAsync(string action, string entityType, Guid entityId, Guid userId, string details = null, Guid? idConsultorio = null)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                Details = details,
                IpAddress = GetClientIpAddress(),
                IdConsultorio = idConsultorio ?? _currentUserResolver.GetCurrentConsultorioId()
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
            var dtos = _mapper.Map<IEnumerable<AuditLogDto>>(logs).ToList();
            
            // Enriquecer con nombres de usuarios
            await EnrichAuditLogsWithUserNames(dtos);
            
            return dtos;
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
            var dtos = _mapper.Map<IEnumerable<AuditLogDto>>(logs).ToList();
            
            // Enriquecer con nombres de usuarios
            await EnrichAuditLogsWithUserNames(dtos);
            
            return dtos;
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
        
        /// <summary>
        /// Método de búsqueda avanzada con múltiples filtros
        /// </summary>
        /// <param name="filter">Filtros para la búsqueda</param>
        /// <returns>Lista paginada de registros de auditoría</returns>
        public async Task<PaginatedList<AuditLogDto>> SearchLogsAsync(AuditLogFilter filter)
        {
            if (filter == null)
                throw new BadRequestException("El filtro no puede ser nulo");
            
            var query = _auditLogRepository.GetQueryable();
            
            // Aplicar filtros
            if (filter.UserId.HasValue)
            {
                query = query.Where(x => x.UserId == filter.UserId.Value);
            }
            
            if (!string.IsNullOrEmpty(filter.EntityType))
            {
                query = query.Where(x => x.EntityType == filter.EntityType);
            }
            
            if (filter.EntityId.HasValue)
            {
                query = query.Where(x => x.EntityId == filter.EntityId.Value);
            }
            
            if (!string.IsNullOrEmpty(filter.Action))
            {
                query = query.Where(x => x.Action == filter.Action);
            }
            
            if (filter.StartDate.HasValue)
            {
                query = query.Where(x => x.Timestamp >= filter.StartDate.Value);
            }
            
            if (filter.EndDate.HasValue)
            {
                query = query.Where(x => x.Timestamp <= filter.EndDate.Value);
            }
            
            if (filter.ConsultorioId.HasValue)
            {
                query = query.Where(x => x.IdConsultorio == filter.ConsultorioId.Value);
            }
            
            // Aplicar ordenación (por defecto, descendente por fecha)
            query = query.OrderByDescending(x => x.Timestamp);
            
            // Aplicar paginación
            var pageNumber = filter.PageNumber > 0 ? filter.PageNumber : 1;
            var pageSize = filter.PageSize > 0 ? filter.PageSize : 10;
            
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
            
            // Enriquecer con nombres de usuarios
            await EnrichAuditLogsWithUserNames(dtos);
            
            // Crear y devolver la lista paginada de DTOs
            return new PaginatedList<AuditLogDto>(dtos, paginatedLogs.TotalCount, paginatedLogs.PageNumber, paginatedLogs.PageSize);
        }
        
        /// <summary>
        /// Enriquece los DTOs de auditoría con nombres de usuario
        /// </summary>
        /// <param name="dtos">Lista de DTOs a enriquecer</param>
        private async Task EnrichAuditLogsWithUserNames(List<AuditLogDto> dtos)
        {
            if (dtos == null || !dtos.Any())
                return;
                
            try
            {
                // Obtenemos los IDs de usuario únicos
                var userIds = dtos.Select(d => d.UserId).Distinct().ToList();
                
                // Obtenemos la información de usuarios en batch
                var userInfoDict = new Dictionary<Guid, string>();
                
                foreach (var userId in userIds)
                {
                    try
                    {
                        var user = await _usuarioRepository.GetByIdAsync(userId);
                        if (user != null)
                        {
                            var displayName = !string.IsNullOrEmpty(user.Nombre) 
                                ? user.Nombre
                                : user.Correo;
                                
                            userInfoDict[userId] = displayName;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error al obtener información del usuario {UserId}", userId);
                    }
                }
                
                // Asignamos los nombres a los DTOs
                foreach (var dto in dtos)
                {
                    if (userInfoDict.TryGetValue(dto.UserId, out string userName))
                    {
                        dto.UserName = userName;
                    }
                    else
                    {
                        dto.UserName = "Usuario Desconocido";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enriquecer los registros de auditoría con nombres de usuario");
            }
        }
    }
}
