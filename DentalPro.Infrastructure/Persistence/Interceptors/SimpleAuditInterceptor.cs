using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DentalPro.Infrastructure.Persistence.Interceptors
{
    /// <summary>
    /// Interceptor simplificado para auditoría automática que evita dependencias circulares
    /// </summary>
    public class SimpleAuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ILogger<SimpleAuditInterceptor> _logger;

        // Lista de tipos de entidad que no queremos auditar
        private readonly HashSet<string> _excludedEntityTypes = new HashSet<string>
        {
            nameof(AuditLog),  // Evitamos auditar los propios logs
            "RefreshToken"     // No auditar tokens de refresco
        };

        public SimpleAuditInterceptor(
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository,
            ILogger<SimpleAuditInterceptor> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            await AuditChangesAsync(eventData.Context);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            AuditChangesAsync(eventData.Context).GetAwaiter().GetResult();
            return base.SavingChanges(eventData, result);
        }

        private async Task AuditChangesAsync(DbContext context)
        {
            if (context == null)
                return;

            try
            {
                // Intentamos obtener el usuario directamente desde HttpContext
                var userId = GetCurrentUserId();
                
                // Si no hay usuario, no hacemos auditoría automática
                if (userId == Guid.Empty)
                {
                    return;
                }

                var entries = context.ChangeTracker.Entries()
                    .Where(e => !_excludedEntityTypes.Contains(e.Entity.GetType().Name) &&
                           (e.State == EntityState.Added || 
                            e.State == EntityState.Modified || 
                            e.State == EntityState.Deleted))
                    .ToList();

                foreach (var entry in entries)
                {
                    await CreateAuditLogAsync(entry, userId);
                }
            }
            catch (Exception ex)
            {
                // Capturamos excepciones para que no afecten la operación principal
                _logger.LogError(ex, "Error al crear registros de auditoría automáticos");
            }
        }

        private async Task CreateAuditLogAsync(EntityEntry entry, Guid userId)
        {
            var entityType = entry.Entity.GetType().Name;
            
            // Solo procesamos entidades que tienen un Id de tipo Guid
            if (!TryGetEntityId(entry.Entity, out Guid entityId))
                return;

            string action = GetAuditAction(entry.State);
            var details = GetAuditDetails(entry);
            
            // Creamos y guardamos el log directamente sin pasar por IAuditService
            var auditLog = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                Details = details,
                IpAddress = GetClientIpAddress(),
                ConsultorioId = GetCurrentConsultorioId()
            };
            
            await _auditLogRepository.CreateAsync(auditLog);
        }
        
        private Guid GetCurrentUserId()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null || !httpContext.User.Identity.IsAuthenticated)
                    return Guid.Empty;
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return userId;
                }
                
                return Guid.Empty;
            }
            catch
            {
                return Guid.Empty;
            }
        }
        
        private Guid GetCurrentConsultorioId()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null || !httpContext.User.Identity.IsAuthenticated)
                    return Guid.Empty;
                
                var consultorioClaim = httpContext.User.FindFirst("IdConsultorio");
                if (consultorioClaim != null && Guid.TryParse(consultorioClaim.Value, out Guid consultorioId))
                {
                    return consultorioId;
                }
                
                return Guid.Empty;
            }
            catch
            {
                return Guid.Empty;
            }
        }
        
        private string GetClientIpAddress()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    return "Unknown";
                
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                return ipAddress ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private bool TryGetEntityId(object entity, out Guid entityId)
        {
            entityId = Guid.Empty;
            
            var idProperty = entity.GetType().GetProperty("Id") ??
                             entity.GetType().GetProperty("ID") ??
                             entity.GetType().GetProperty($"{entity.GetType().Name}Id");

            if (idProperty != null && idProperty.PropertyType == typeof(Guid))
            {
                entityId = (Guid)idProperty.GetValue(entity);
                return true;
            }

            return false;
        }

        private string GetAuditAction(EntityState state)
        {
            return state switch
            {
                EntityState.Added => "Create",
                EntityState.Modified => "Update",
                EntityState.Deleted => "Delete",
                _ => "Unknown"
            };
        }

        private string GetAuditDetails(EntityEntry entry)
        {
            var details = new Dictionary<string, object>();

            switch (entry.State)
            {
                case EntityState.Added:
                    // Para entidades nuevas, registramos todos los valores iniciales
                    details["Initial"] = entry.Properties
                        .Where(p => p.Metadata.ClrType != typeof(byte[]))
                        .ToDictionary(
                            p => p.Metadata.Name,
                            p => p.CurrentValue);
                    break;

                case EntityState.Modified:
                    // Para actualizaciones, registramos solo los cambios
                    var changes = new Dictionary<string, object>();
                    var originalValues = new Dictionary<string, object>();

                    foreach (var property in entry.Properties.Where(p => p.IsModified && p.Metadata.ClrType != typeof(byte[])))
                    {
                        changes[property.Metadata.Name] = property.CurrentValue;
                        originalValues[property.Metadata.Name] = property.OriginalValue;
                    }

                    details["Changes"] = changes;
                    details["Original"] = originalValues;
                    break;

                case EntityState.Deleted:
                    // Para eliminaciones, registramos todos los valores que existían
                    details["Deleted"] = entry.Properties
                        .Where(p => p.Metadata.ClrType != typeof(byte[]))
                        .ToDictionary(
                            p => p.Metadata.Name,
                            p => p.CurrentValue);
                    break;
            }

            return JsonSerializer.Serialize(details);
        }
    }
}
