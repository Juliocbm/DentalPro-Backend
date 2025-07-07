using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IRepositories
{
    public interface IPacienteRepository
    {
        Task<IEnumerable<Paciente>> GetAllByConsultorioAsync(Guid idConsultorio);
        Task<Paciente?> GetByIdAsync(Guid id);
        Task<Paciente?> GetByCorreoAsync(string correo);
        Task<Paciente> CreateAsync(Paciente paciente);
        Task UpdateAsync(Paciente paciente);
        Task<bool> DeleteAsync(Guid id);
        Task SaveChangesAsync();
    }
}
