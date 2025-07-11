using AutoMapper;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using DentalPro.Application.DTOs.Paciente;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DentalPro.Infrastructure.Services
{
    public class PacienteService : IPacienteService
    {
        private readonly IPacienteRepository _pacienteRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ILogger<PacienteService> _logger;

        public PacienteService(
            IPacienteRepository pacienteRepository,
            ICurrentUserService currentUserService,
            IMapper mapper,
            ILogger<PacienteService> logger)
        {
            _pacienteRepository = pacienteRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<PacienteDto>> GetAllByConsultorioAsync(Guid idConsultorio)
        {
            // Verificar permiso para ver todos los pacientes
            if (!await _currentUserService.HasPermisoAsync(PacientesPermissions.ViewAll))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó ver todos los pacientes sin el permiso requerido",
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            // Verificar que el consultorio del usuario coincida con el consultorio solicitado
            var userConsultorioId = _currentUserService.GetCurrentConsultorioId();
            if (userConsultorioId != idConsultorio)
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó acceder a pacientes de un consultorio diferente",
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
            }

            var pacientes = await _pacienteRepository.GetAllByConsultorioAsync(idConsultorio);
            return _mapper.Map<IEnumerable<PacienteDto>>(pacientes);
        }

        public async Task<PacienteDto?> GetByIdAsync(Guid id)
        {
            // Verificar permiso para ver detalles de un paciente
            if (!await _currentUserService.HasPermisoAsync(PacientesPermissions.ViewDetail))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó ver detalles de un paciente sin el permiso requerido",
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            var paciente = await _pacienteRepository.GetByIdAsync(id);

            // Si el paciente no existe, devolver null
            if (paciente == null)
            {
                return null;
            }

            // Verificar que pertenezca al mismo consultorio que el usuario
            var userConsultorioId = _currentUserService.GetCurrentConsultorioId();
            if (userConsultorioId != paciente.IdConsultorio)
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó acceder a un paciente de un consultorio diferente",
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
            }

            // Mapear a DTO y devolver
            return _mapper.Map<PacienteDto>(paciente);
        }

        public async Task<PacienteDto> CreateAsync(PacienteCreateDto pacienteDto)
        {
            // Verificar permiso para crear pacientes
            if (!await _currentUserService.HasPermisoAsync(PacientesPermissions.Create))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó crear un paciente sin el permiso requerido",
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            // Mapear DTO a entidad
            var paciente = _mapper.Map<Paciente>(pacienteDto);

            // Asegurar que el consultorio sea el del usuario autenticado
            var userConsultorioId = _currentUserService.GetCurrentConsultorioId();
            paciente.IdConsultorio = userConsultorioId;
            
            // Establecer fecha de alta actual
            paciente.FechaAlta = DateTime.Now;

            // Verificar si ya existe un paciente con el mismo correo
            if (!string.IsNullOrEmpty(paciente.Correo))
            {
                var existingPaciente = await _pacienteRepository.GetByCorreoAsync(paciente.Correo);
                if (existingPaciente != null)
                {
                    throw new BadRequestException("El correo electrónico ya está registrado para otro paciente", ErrorCodes.DuplicateEmail);
                }
            }

            var createdPaciente = await _pacienteRepository.CreateAsync(paciente);
            await _pacienteRepository.SaveChangesAsync();
            
            _logger.LogInformation("Usuario {UserId} creó un nuevo paciente con ID {PacienteId}", 
                _currentUserService.GetCurrentUserId(), createdPaciente.IdPaciente);
            
            // Mapear la entidad creada a DTO y devolverla
            return _mapper.Map<PacienteDto>(createdPaciente);
        }

        public async Task<PacienteDto> UpdateAsync(PacienteUpdateDto pacienteDto)
        {
            // Verificar permiso para actualizar pacientes
            if (!await _currentUserService.HasPermisoAsync(PacientesPermissions.Update))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó actualizar un paciente sin el permiso requerido",
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            var existingPaciente = await _pacienteRepository.GetByIdAsync(pacienteDto.IdPaciente);
            if (existingPaciente == null)
            {
                throw new NotFoundException("Paciente", pacienteDto.IdPaciente);
            }

            // Verificar que el paciente pertenezca al consultorio del usuario
            var userConsultorioId = _currentUserService.GetCurrentConsultorioId();
            if (existingPaciente.IdConsultorio != userConsultorioId)
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó actualizar un paciente de un consultorio diferente",
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
            }

            // Mapear el DTO a la entidad existente, manteniendo valores que no deben cambiar
            _mapper.Map(pacienteDto, existingPaciente);

            // Asegurar que no se cambie el consultorio del paciente
            existingPaciente.IdConsultorio = userConsultorioId;

            // Verificación adicional de correo duplicado
            if (!string.IsNullOrEmpty(existingPaciente.Correo))
            {
                var pacienteWithSameEmail = await _pacienteRepository.GetByCorreoAsync(existingPaciente.Correo);
                if (pacienteWithSameEmail != null && pacienteWithSameEmail.IdPaciente != existingPaciente.IdPaciente)
                {
                    throw new BadRequestException("El correo electrónico ya está registrado para otro paciente", ErrorCodes.DuplicateEmail);
                }
            }
            
            await _pacienteRepository.UpdateAsync(existingPaciente);
            await _pacienteRepository.SaveChangesAsync();
            
            _logger.LogInformation("Usuario {UserId} actualizó el paciente con ID {PacienteId}", 
                _currentUserService.GetCurrentUserId(), existingPaciente.IdPaciente);
                
            // Devolver el paciente actualizado como DTO
            return _mapper.Map<PacienteDto>(existingPaciente);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            // Verificar permiso para eliminar pacientes
            if (!await _currentUserService.HasPermisoAsync(PacientesPermissions.Delete))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó eliminar un paciente sin el permiso requerido",
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            // Verificar que el paciente existe y pertenece al consultorio del usuario
            var paciente = await _pacienteRepository.GetByIdAsync(id);
            if (paciente == null)
            {
                throw new NotFoundException("Paciente", id);
            }

            var userConsultorioId = _currentUserService.GetCurrentConsultorioId();
            if (paciente.IdConsultorio != userConsultorioId)
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó eliminar un paciente de un consultorio diferente",
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
            }

            var result = await _pacienteRepository.DeleteAsync(id);
            
            if (result)
            {
                await _pacienteRepository.SaveChangesAsync();
                _logger.LogInformation("Usuario {UserId} eliminó el paciente con ID {PacienteId}", 
                    _currentUserService.GetCurrentUserId(), id);
                return true;
            }
            
            return false;
        }
    }
}
