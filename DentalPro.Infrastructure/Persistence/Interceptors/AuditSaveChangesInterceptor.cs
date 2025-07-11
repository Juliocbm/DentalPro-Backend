using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DentalPro.Infrastructure.Persistence.Interceptors
{
    /// <summary>
    /// Interceptor para registrar automáticamente cambios en las entidades como registros de auditoría
    /// </summary>
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditSaveChangesInterceptor> _logger;

        // Lista de tipos de entidad que no queremos auditar (opcional)
        private readonly HashSet<string> _excludedEntityTypes = new HashSet<string>
        {
            nameof(AuditLog) // Evitamos auditar los propios logs de auditoría
        };

        public AuditSaveChangesInterceptor(
            ICurrentUserService currentUserService,
            IAuditService auditService,
            ILogger<AuditSaveChangesInterceptor> logger)
        {
            _currentUserService = currentUserService;
            _auditService = auditService;
            _logger = logger;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            await AuditChanges(eventData.Context);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            AuditChanges(eventData.Context).GetAwaiter().GetResult();
            return base.SavingChanges(eventData, result);
        }

        private async Task AuditChanges(DbContext context)
        {
            if (context == null)
                return;

            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                var entries = context.ChangeTracker.Entries()
                    .Where(e => !_excludedEntityTypes.Contains(e.Entity.GetType().Name) &&
                           (e.State == EntityState.Added || 
                            e.State == EntityState.Modified || 
                            e.State == EntityState.Deleted))
                    .ToList();

                foreach (var entry in entries)
                {
                    await CreateAuditLogForEntry(entry, userId);
                }
            }
            catch (Exception ex)
            {
                // Es importante que cualquier fallo en la auditoría no impida las operaciones principales
                // Por eso capturamos y registramos la excepción, pero no la propagamos
                _logger.LogError(ex, "Error al crear registros de auditoría automáticos");
            }
        }

        private async Task CreateAuditLogForEntry(EntityEntry entry, Guid userId)
        {
            var entityType = entry.Entity.GetType().Name;
            
            // Solo procesamos entidades que tienen un Id de tipo Guid
            if (!TryGetEntityId(entry.Entity, out Guid entityId))
                return;

            string action = GetAuditAction(entry.State);
            var details = GetAuditDetails(entry);

            await _auditService.RegisterActionAsync(
                action,
                entityType,
                entityId,
                userId,
                details
            );
        }

        private bool TryGetEntityId(object entity, out Guid entityId)
        {
            entityId = Guid.Empty;
            
            // Intentamos obtener la propiedad Id, considerando distintos nombres comunes
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
                    // Para entidades nuevas, registramos todos los valores iniciales (excepto binarios grandes)
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
