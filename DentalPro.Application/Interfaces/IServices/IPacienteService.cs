using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IServices
{
    public interface IPacienteService
    {
        Task<IEnumerable<Paciente>> GetAllByConsultorioAsync(Guid idConsultorio);
        Task<Paciente?> GetByIdAsync(Guid id);
        Task<Paciente> CreateAsync(Paciente paciente);
        Task UpdateAsync(Paciente paciente);
        Task<bool> DeleteAsync(Guid id);
    }
}
