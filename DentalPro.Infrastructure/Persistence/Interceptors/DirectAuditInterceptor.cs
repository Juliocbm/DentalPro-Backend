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
    /// Interceptor directo que escribe registros de auditoría sin depender de repositorios ni servicios
    /// </summary>
    public class DirectAuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<DirectAuditInterceptor> _logger;

        // Lista de tipos de entidad que no queremos auditar
        private readonly HashSet<string> _excludedEntityTypes = new HashSet<string>
        {
            nameof(AuditLog),  // Evitamos auditar los propios logs
            "RefreshToken"     // No auditar tokens de refresco
        };

        public DirectAuditInterceptor(
            IHttpContextAccessor httpContextAccessor,
            ILogger<DirectAuditInterceptor> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is ApplicationDbContext dbContext)
            {
                try
                {
                    // Intentamos obtener el usuario directamente desde HttpContext
                    var userId = GetCurrentUserId();
                    
                    // Si no hay usuario, no hacemos auditoría automática
                    if (userId != Guid.Empty)
                    {
                        await AuditChangesAsync(dbContext, eventData.EntitiesSavedCount);
                    }
                }
                catch (Exception ex)
                {
                    // Capturamos excepciones para que no afecten la operación principal
                    _logger.LogError(ex, "Error al crear registros de auditoría automáticos");
                }
            }
            
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private async Task AuditChangesAsync(ApplicationDbContext dbContext, int entitiesSavedCount)
        {
            // No hacemos auditoría si no se guardaron entidades
            if (entitiesSavedCount <= 0)
                return;

            try
            {
                // Obtener el usuario directamente del HttpContext
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return;

                var consultorioId = GetCurrentConsultorioId();
                var ipAddress = GetClientIpAddress();
                var timestamp = DateTime.UtcNow;

                // Obtenemos las entidades que fueron modificadas
                var entries = dbContext.ChangeTracker.Entries()
                    .Where(e => !_excludedEntityTypes.Contains(e.Entity.GetType().Name) &&
                           (e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted) &&
                           e.Entity.GetType() != typeof(AuditLog))
                    .ToList();

                // Creamos un registro de auditoría para cada entidad modificada
                var auditLogs = new List<AuditLog>();
                
                foreach (var entry in entries)
                {
                    // Solo procesamos entidades que tienen un Id de tipo Guid
                    if (!TryGetEntityId(entry.Entity, out Guid entityId))
                        continue;

                    string action = GetAuditAction(entry.State);
                    string entityType = entry.Entity.GetType().Name;
                    var details = GetAuditDetails(entry);
                    
                    var auditLog = new AuditLog
                    {
                        Action = action,
                        EntityType = entityType,
                        EntityId = entityId,
                        UserId = userId,
                        Details = details,
                        IpAddress = ipAddress,
                        IdConsultorio = consultorioId,
                        Timestamp = timestamp
                    };
                    
                    auditLogs.Add(auditLog);
                }

                // Si tenemos logs para guardar, los guardamos directamente
                if (auditLogs.Any())
                {
                    try
                    {
                        // Agregamos directamente al contexto y guardamos
                        // sin usar el repositorio para evitar ciclos
                        dbContext.AuditLogs.AddRange(auditLogs);
                        
                        // Deshabilitamos temporalmente el interceptor para evitar recursión infinita
                        dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                        await dbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true);
                        dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
                    }
                    catch (Exception ex) when (ex.Message.Contains("Invalid object name") && ex.Message.Contains("AuditLogs"))
                    {
                        // Si la tabla de auditoría no existe, simplemente lo registramos y continuamos
                        // para no bloquear el flujo principal
                        _logger.LogWarning(ex, "No se pudieron guardar los registros de auditoría porque la tabla no existe.");
                        dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar auditoría");
            }
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
