using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DentalPro.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio de citas
/// </summary>
public class CitaRepository : ICitaRepository
{
    private readonly ApplicationDbContext _context;

    public CitaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Cita>> GetAllAsync()
    {
        return await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Doctor)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cita>> GetByConsultorioAsync(Guid idConsultorio)
    {
        return await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Doctor)
            .Where(c => c.Paciente.IdConsultorio == idConsultorio)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cita>> GetByPacienteAsync(Guid idPaciente)
    {
        return await _context.Citas
            .Include(c => c.Doctor)
            .Where(c => c.IdPaciente == idPaciente)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cita>> GetByDoctorAsync(Guid idDoctor)
    {
        return await _context.Citas
            .Include(c => c.Paciente)
            .Where(c => c.IdDoctor == idDoctor)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cita>> GetByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin, Guid idConsultorio)
    {
        return await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Doctor)
            .Where(c => c.FechaHoraInicio >= fechaInicio && 
                   c.FechaHoraFin <= fechaFin && 
                   c.Paciente.IdConsultorio == idConsultorio)
            .ToListAsync();
    }

    public async Task<Cita?> GetByIdAsync(Guid id)
    {
        return await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Doctor)
            .Include(c => c.Recordatorios)
            .FirstOrDefaultAsync(c => c.IdCita == id);
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _context.Citas.AnyAsync(c => c.IdCita == id);
    }

    public async Task<bool> HasOverlappingAppointmentsAsync(Guid idDoctor, DateTime fechaInicio, DateTime fechaFin, Guid? idCitaExcluir = null)
    {
        var query = _context.Citas.Where(c => 
            c.IdDoctor == idDoctor &&
            c.Estatus != "Cancelada" &&
            ((c.FechaHoraInicio <= fechaInicio && c.FechaHoraFin > fechaInicio) || // Cita existente cubre inicio de nueva cita
             (c.FechaHoraInicio < fechaFin && c.FechaHoraFin >= fechaFin) || // Cita existente cubre fin de nueva cita
             (c.FechaHoraInicio >= fechaInicio && c.FechaHoraFin <= fechaFin)) // Nueva cita cubre cita existente
        );

        // Si estamos actualizando una cita existente, excluimos esa cita de la comprobación
        if (idCitaExcluir.HasValue)
        {
            query = query.Where(c => c.IdCita != idCitaExcluir.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<Cita> AddAsync(Cita cita)
    {
        cita.IdCita = Guid.NewGuid();
        await _context.Citas.AddAsync(cita);
        await _context.SaveChangesAsync();
        return cita;
    }

    public async Task<Cita> UpdateAsync(Cita cita)
    {
        _context.Citas.Update(cita);
        await _context.SaveChangesAsync();
        return cita;
    }

    public async Task DeleteAsync(Guid id)
    {
        var cita = await _context.Citas.FindAsync(id);
        if (cita != null)
        {
            _context.Citas.Remove(cita);
            await _context.SaveChangesAsync();
        }
    }
}
