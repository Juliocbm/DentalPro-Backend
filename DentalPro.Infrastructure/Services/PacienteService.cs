using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DentalPro.Infrastructure.Services
{
    public class PacienteService : IPacienteService
    {
        private readonly IPacienteRepository _pacienteRepository;

        public PacienteService(IPacienteRepository pacienteRepository)
        {
            _pacienteRepository = pacienteRepository;
        }

        public async Task<IEnumerable<Paciente>> GetAllByConsultorioAsync(Guid idConsultorio)
        {
            return await _pacienteRepository.GetAllByConsultorioAsync(idConsultorio);
        }

        public async Task<Paciente?> GetByIdAsync(Guid id)
        {
            return await _pacienteRepository.GetByIdAsync(id);
        }

        public async Task<Paciente> CreateAsync(Paciente paciente)
        {
            // Verificación adicional de correo duplicado
            if (!string.IsNullOrEmpty(paciente.Correo))
            {
                var existingPaciente = await _pacienteRepository.GetByCorreoAsync(paciente.Correo);
                if (existingPaciente != null)
                {
                    throw new BadRequestException("El correo electrónico ya está registrado para otro paciente", ErrorCodes.DuplicateEmail);
                }
            }

            var newPaciente = await _pacienteRepository.CreateAsync(paciente);
            await _pacienteRepository.SaveChangesAsync();
            
            return newPaciente;
        }

        public async Task UpdateAsync(Paciente paciente)
        {
            var existingPaciente = await _pacienteRepository.GetByIdAsync(paciente.IdPaciente);
            if (existingPaciente == null)
            {
                throw new NotFoundException("Paciente", paciente.IdPaciente);
            }

            // Verificación adicional de correo duplicado
            if (!string.IsNullOrEmpty(paciente.Correo))
            {
                var pacienteWithSameEmail = await _pacienteRepository.GetByCorreoAsync(paciente.Correo);
                if (pacienteWithSameEmail != null && pacienteWithSameEmail.IdPaciente != paciente.IdPaciente)
                {
                    throw new BadRequestException("El correo electrónico ya está registrado para otro paciente", ErrorCodes.DuplicateEmail);
                }
            }

            // Mantener la fecha original de alta
            paciente.FechaAlta = existingPaciente.FechaAlta;
            
            await _pacienteRepository.UpdateAsync(paciente);
            await _pacienteRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var result = await _pacienteRepository.DeleteAsync(id);
            
            if (result)
            {
                await _pacienteRepository.SaveChangesAsync();
                return true;
            }
            
            return false;
        }
    }
}
