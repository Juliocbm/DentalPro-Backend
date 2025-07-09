using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DentalPro.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio de recordatorios
/// </summary>
public class RecordatorioRepository : IRecordatorioRepository
{
    private readonly ApplicationDbContext _context;

    public RecordatorioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Recordatorio>> GetByCitaAsync(Guid idCita)
    {
        return await _context.Recordatorios
            .Where(r => r.IdCita == idCita)
            .ToListAsync();
    }

    public async Task<Recordatorio?> GetByIdAsync(Guid id)
    {
        return await _context.Recordatorios
            .Include(r => r.Cita)
            .FirstOrDefaultAsync(r => r.IdRecordatorio == id);
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _context.Recordatorios.AnyAsync(r => r.IdRecordatorio == id);
    }

    public async Task<IEnumerable<Recordatorio>> GetPendingAsync()
    {
        return await _context.Recordatorios
            .Include(r => r.Cita)
            .ThenInclude(c => c.Paciente)
            .Where(r => !r.Enviado && r.Cita.Estatus != "Cancelada")
            .ToListAsync();
    }

    public async Task<Recordatorio> AddAsync(Recordatorio recordatorio)
    {
        recordatorio.IdRecordatorio = Guid.NewGuid();
        await _context.Recordatorios.AddAsync(recordatorio);
        await _context.SaveChangesAsync();
        return recordatorio;
    }

    public async Task<Recordatorio> UpdateAsync(Recordatorio recordatorio)
    {
        _context.Recordatorios.Update(recordatorio);
        await _context.SaveChangesAsync();
        return recordatorio;
    }

    public async Task MarkAsSentAsync(Guid id)
    {
        var recordatorio = await _context.Recordatorios.FindAsync(id);
        if (recordatorio != null)
        {
            recordatorio.Enviado = true;
            recordatorio.FechaEnvio = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var recordatorio = await _context.Recordatorios.FindAsync(id);
        if (recordatorio != null)
        {
            _context.Recordatorios.Remove(recordatorio);
            await _context.SaveChangesAsync();
        }
    }
}
