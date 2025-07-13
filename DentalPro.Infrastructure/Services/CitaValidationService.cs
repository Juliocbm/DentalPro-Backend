using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.DTOs.Citas;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio para validación de citas
/// </summary>
public class CitaValidationService : ICitaValidationService
{
    private readonly ICitaRepository _citaRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly ILogger<CitaValidationService> _logger;

    public CitaValidationService(
        ICitaRepository citaRepository,
        IPacienteRepository pacienteRepository,
        IDoctorRepository doctorRepository,
        ILogger<CitaValidationService> logger)
    {
        _citaRepository = citaRepository ?? throw new ArgumentNullException(nameof(citaRepository));
        _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
        _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ValidateForCreateAsync(CitaCreateDto citaDto)
    {
        _logger.LogInformation("Validando datos para creación de cita");
        
        // Validar que exista el paciente
        var paciente = await _pacienteRepository.GetByIdAsync(citaDto.IdPaciente);
        if (paciente == null)
        {
            _logger.LogWarning("El paciente con ID {IdPaciente} no existe", citaDto.IdPaciente);
            throw new NotFoundException(ErrorMessages.PacienteNotFound, ErrorCodes.PacienteNotFound);
        }
        
        // Validar que el usuario sea un doctor
        var esDoctor = await IsUserDoctorAsync(citaDto.IdDoctor);
        if (!esDoctor)
        {
            _logger.LogWarning("El usuario con ID {IdDoctor} no es un doctor", citaDto.IdDoctor);
            throw new ValidationException(
                "Solo se pueden agendar citas a usuarios con rol de Doctor", 
                ErrorCodes.InvalidOperation);
        }
        
        // Validar rango de tiempo
        if (citaDto.FechaHoraFin <= citaDto.FechaHoraInicio)
        {
            _logger.LogWarning("Intento de crear cita con hora de fin ({HoraFin}) anterior o igual a hora de inicio ({HoraInicio})", 
                citaDto.FechaHoraFin, citaDto.FechaHoraInicio);
            throw new BadRequestException(ErrorMessages.CitaInvalidTimeRange, ErrorCodes.CitaInvalidTimeRange);
        }
        
        // Validar que la fecha no sea pasada
        if (citaDto.FechaHoraInicio < DateTime.Now)
        {
            _logger.LogWarning("Intento de crear cita para una fecha pasada: {FechaHora}", citaDto.FechaHoraInicio);
            throw new BadRequestException(ErrorMessages.CitaPastDate, ErrorCodes.CitaPastDate);
        }
        
        // Verificar si hay traslape de citas
        var hayTraslape = await HasOverlappingAppointmentsAsync(
            citaDto.IdDoctor, 
            citaDto.FechaHoraInicio, 
            citaDto.FechaHoraFin);
            
        if (hayTraslape)
        {
            _logger.LogWarning("Hay traslape con otra cita para el doctor {IdDoctor} en el horario {Inicio} - {Fin}", 
                citaDto.IdDoctor, citaDto.FechaHoraInicio, citaDto.FechaHoraFin);
            throw new BadRequestException(ErrorMessages.CitaOverlap, ErrorCodes.CitaOverlap);
        }
        
        _logger.LogInformation("Validación para creación de cita completada satisfactoriamente");
    }
    
    public async Task ValidateForUpdateAsync(CitaUpdateDto citaDto, Cita citaExistente)
    {
        _logger.LogInformation("Validando datos para actualización de cita {IdCita}", citaDto.IdCita);
        
        // Validar que la cita no esté cancelada si se intenta cambiar a un estado diferente
        if (citaExistente.Estatus == "Cancelada" && citaDto.Estatus != "Cancelada")
        {
            _logger.LogWarning("Intento de actualizar una cita cancelada {IdCita}", citaDto.IdCita);
            throw new BadRequestException(ErrorMessages.CitaCancelled, ErrorCodes.CitaCancelled);
        }
        
        // Si cambia el doctor, validar que el nuevo sea doctor
        if (citaDto.IdDoctor != citaExistente.IdDoctor)
        {
            var esDoctor = await IsUserDoctorAsync(citaDto.IdDoctor);
            if (!esDoctor)
            {
                _logger.LogWarning("El usuario con ID {IdDoctor} no es un doctor", citaDto.IdDoctor);
                throw new ValidationException(
                    "Solo se pueden agendar citas a usuarios con rol de Doctor", 
                    ErrorCodes.InvalidOperation);
            }
        }
        
        // Validar que exista el paciente
        var paciente = await _pacienteRepository.GetByIdAsync(citaDto.IdPaciente);
        if (paciente == null)
        {
            _logger.LogWarning("El paciente con ID {IdPaciente} no existe", citaDto.IdPaciente);
            throw new NotFoundException(ErrorMessages.PacienteNotFound, ErrorCodes.PacienteNotFound);
        }
        
        // Validar rango de tiempo
        if (citaDto.FechaHoraFin <= citaDto.FechaHoraInicio)
        {
            _logger.LogWarning("Intento de actualizar cita con hora de fin ({HoraFin}) anterior o igual a hora de inicio ({HoraInicio})", 
                citaDto.FechaHoraFin, citaDto.FechaHoraInicio);
            throw new BadRequestException(ErrorMessages.CitaInvalidTimeRange, ErrorCodes.CitaInvalidTimeRange);
        }
        
        // Si la fecha es futura, verificar que no sea pasada
        if (citaDto.FechaHoraInicio.Date > DateTime.Now.Date && citaDto.FechaHoraInicio < DateTime.Now)
        {
            _logger.LogWarning("Intento de actualizar cita para una fecha pasada: {FechaHora}", citaDto.FechaHoraInicio);
            throw new BadRequestException(ErrorMessages.CitaPastDate, ErrorCodes.CitaPastDate);
        }
        
        // Verificar si hay traslape de citas (excluyendo la cita actual)
        var hayTraslape = await HasOverlappingAppointmentsAsync(
            citaDto.IdDoctor, 
            citaDto.FechaHoraInicio, 
            citaDto.FechaHoraFin, 
            citaDto.IdCita);
            
        if (hayTraslape)
        {
            _logger.LogWarning("Hay traslape con otra cita para el doctor {IdDoctor} en el horario {Inicio} - {Fin}", 
                citaDto.IdDoctor, citaDto.FechaHoraInicio, citaDto.FechaHoraFin);
            throw new BadRequestException(ErrorMessages.CitaOverlap, ErrorCodes.CitaOverlap);
        }
        
        _logger.LogInformation("Validación para actualización de cita {IdCita} completada satisfactoriamente", citaDto.IdCita);
    }
    
    public async Task ValidateForCancelAsync(Guid idCita)
    {
        _logger.LogInformation("Validando datos para cancelación de cita {IdCita}", idCita);
        
        var cita = await _citaRepository.GetByIdAsync(idCita);
        if (cita == null)
        {
            _logger.LogWarning("No se encontró la cita con ID {IdCita} para cancelar", idCita);
            throw new NotFoundException(ErrorMessages.CitaNotFound, ErrorCodes.CitaNotFound);
        }
        
        // Verificar si la cita ya está cancelada
        if (cita.Estatus == "Cancelada")
        {
            _logger.LogWarning("La cita con ID {IdCita} ya está cancelada", idCita);
            throw new BadRequestException(ErrorMessages.CitaCancelled, ErrorCodes.CitaCancelled);
        }
        
        _logger.LogInformation("Validación para cancelación de cita {IdCita} completada satisfactoriamente", idCita);
    }
    
    public async Task ValidateForDeleteAsync(Guid idCita)
    {
        _logger.LogInformation("Validando datos para eliminación de cita {IdCita}", idCita);
        
        var cita = await _citaRepository.GetByIdAsync(idCita);
        if (cita == null)
        {
            _logger.LogWarning("No se encontró la cita con ID {IdCita} para eliminar", idCita);
            throw new NotFoundException(ErrorMessages.CitaNotFound, ErrorCodes.CitaNotFound);
        }
        
        // Solo permitimos eliminar citas que estén canceladas o en el pasado
        if (cita.Estatus != "Cancelada" && cita.FechaHoraInicio > DateTime.Now)
        {
            _logger.LogWarning("Intento de eliminar cita {IdCita} programada para el futuro y no cancelada", idCita);
            throw new BadRequestException("No se pueden eliminar citas programadas para el futuro. Cancele la cita primero.",
                ErrorCodes.InvalidOperation);
        }
        
        _logger.LogInformation("Validación para eliminación de cita {IdCita} completada satisfactoriamente", idCita);
    }
    
    public async Task<bool> HasOverlappingAppointmentsAsync(Guid idDoctor, DateTime fechaHoraInicio, DateTime fechaHoraFin, Guid? idCitaExcluir = null)
    {
        return await _citaRepository.HasOverlappingAppointmentsAsync(idDoctor, fechaHoraInicio, fechaHoraFin, idCitaExcluir);
    }
    
    public async Task<bool> IsUserDoctorAsync(Guid idUsuario)
    {
        return await _doctorRepository.IsUserDoctorAsync(idUsuario);
    }
    
    public async Task<DoctorDetail> GetDoctorDetailAsync(Guid idDoctor)
    {
        var doctorDetail = await _doctorRepository.GetDoctorDetailByIdAsync(idDoctor);
        if (doctorDetail == null)
        {
            _logger.LogWarning("No se encontraron detalles para el doctor con ID {IdDoctor}", idDoctor);
            throw new NotFoundException("No se encontró información del doctor", ErrorCodes.ResourceNotFound);
        }
        
        return doctorDetail;
    }
}
