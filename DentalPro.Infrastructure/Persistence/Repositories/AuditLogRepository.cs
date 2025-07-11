using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;
using DentalPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DentalPro.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio para registros de auditoría
    /// </summary>
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(AuditLog auditLog)
        {
            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId)
        {
            return await _context.AuditLogs
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId)
        {
            return await _context.AuditLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.AuditLogs.Where(a => a.Action == action);
            
            if (fromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= toDate.Value);
            }
            
            return await query
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByConsultorioAsync(Guid consultorioId)
        {
            return await _context.AuditLogs
                .Where(a => a.ConsultorioId == consultorioId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public IQueryable<AuditLog> GetQueryable()
        {
            return _context.AuditLogs.AsQueryable();
        }
    }
}
