using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DentalPro.Application.DTOs.Paciente;
using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IServices
{
    public interface IPacienteService
    {
        Task<IEnumerable<PacienteDto>> GetAllByConsultorioAsync(Guid idConsultorio);
        Task<PacienteDto?> GetByIdAsync(Guid id);
        Task<PacienteDto> CreateAsync(PacienteCreateDto pacienteDto);
        Task<PacienteDto> UpdateAsync(PacienteUpdateDto pacienteDto);
        Task<bool> DeleteAsync(Guid id);
    }
}
