using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;
using DentalPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DentalPro.Infrastructure.Persistence.Repositories
{
    public class PacienteRepository : IPacienteRepository
    {
        private readonly ApplicationDbContext _context;

        public PacienteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Paciente>> GetAllByConsultorioAsync(Guid idConsultorio)
        {
            return await _context.Pacientes
                .Where(p => p.IdConsultorio == idConsultorio)
                .OrderBy(p => p.Nombre)
                .ThenBy(p => p.Apellidos)
                .ToListAsync();
        }

        public async Task<Paciente?> GetByIdAsync(Guid id)
        {
            return await _context.Pacientes
                .FirstOrDefaultAsync(p => p.IdPaciente == id);
        }

        public async Task<Paciente?> GetByCorreoAsync(string correo)
        {
            if (string.IsNullOrEmpty(correo)) return null;
            
            return await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Correo == correo);
        }

        public async Task<Paciente> CreateAsync(Paciente paciente)
        {
            paciente.IdPaciente = Guid.NewGuid();
            paciente.FechaAlta = DateTime.Now;
            
            await _context.Pacientes.AddAsync(paciente);
            return paciente;
        }

        public Task UpdateAsync(Paciente paciente)
        {
            _context.Pacientes.Update(paciente);
            return Task.CompletedTask;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var paciente = await GetByIdAsync(id);
            if (paciente == null)
            {
                return false;
            }

            _context.Pacientes.Remove(paciente);
            return true;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
