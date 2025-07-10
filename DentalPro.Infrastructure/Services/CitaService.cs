using AutoMapper;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using DentalPro.Application.DTOs.Citas;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de citas
/// </summary>
public class CitaService : ICitaService
{
    private readonly ICitaRepository _citaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<CitaService> _logger;

    public CitaService(
        ICitaRepository citaRepository,
        IUsuarioRepository usuarioRepository,
        IPacienteRepository pacienteRepository,
        IDoctorRepository doctorRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<CitaService> logger)
    {
        _citaRepository = citaRepository;
        _usuarioRepository = usuarioRepository;
        _pacienteRepository = pacienteRepository;
        _doctorRepository = doctorRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CitaDto>> GetAllAsync()
    {
        // Verificar si el usuario tiene permiso para ver todas las citas
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.ViewAll);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de acceso no autorizado a todas las citas");
            throw new ForbiddenAccessException("No tiene permiso para ver todas las citas");
        }
        
        var citas = await _citaRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CitaDto>>(citas);
    }

    public async Task<IEnumerable<CitaDto>> GetByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        // Verificar si el usuario tiene permiso para ver citas
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.View);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de acceso no autorizado a citas por rango de fechas");
            throw new ForbiddenAccessException("No tiene permiso para ver citas");
        }
        
        // Obtenemos el ID del consultorio del usuario actual usando el nuevo servicio
        var idConsultorio = _currentUserService.GetCurrentConsultorioId();
        var citas = await _citaRepository.GetByDateRangeAsync(fechaInicio, fechaFin, idConsultorio);
        return _mapper.Map<IEnumerable<CitaDto>>(citas);
    }

    public async Task<IEnumerable<CitaDto>> GetByPacienteAsync(Guid idPaciente)
    {
        // Verificar si el usuario tiene permiso para ver citas de pacientes
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.ViewByPaciente);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de acceso no autorizado a citas de un paciente");
            throw new ForbiddenAccessException("No tiene permiso para ver citas de pacientes");
        }
        
        var paciente = await _pacienteRepository.GetByIdAsync(idPaciente);
        if (paciente == null)
        {
            throw new NotFoundException(ErrorMessages.PacienteNotFound, ErrorCodes.PacienteNotFound);
        }

        var citas = await _citaRepository.GetByPacienteAsync(idPaciente);
        return _mapper.Map<IEnumerable<CitaDto>>(citas);
    }

    public async Task<IEnumerable<CitaDto>> GetByDoctorAsync(Guid idDoctor)
    {
        // Verificar si el usuario tiene permiso para ver citas de doctores
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.ViewByDoctor);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de acceso no autorizado a citas de un doctor");
            throw new ForbiddenAccessException("No tiene permiso para ver citas de doctores");
        }
        
        // Verificar que el usuario exista
        var doctor = await _usuarioRepository.GetByIdAsync(idDoctor);
        if (doctor == null)
        {
            throw new NotFoundException(ErrorMessages.UsuarioNotFound, ErrorCodes.UsuarioNotFound);
        }
        
        // Verificar que el usuario sea un doctor usando DoctorDetail
        var doctorDetail = await _doctorRepository.GetDoctorDetailByIdAsync(idDoctor);
        var esDoctor = await _doctorRepository.IsUserDoctorAsync(idDoctor);
        if (!esDoctor)
        {
            throw new BadRequestException("El usuario no tiene rol de Doctor", ErrorCodes.InvalidOperation);
        }

        var citas = await _citaRepository.GetByDoctorAsync(idDoctor);
        return _mapper.Map<IEnumerable<CitaDto>>(citas);
    }

    public async Task<CitaDetailDto> GetByIdAsync(Guid id)
    {
        // Verificar si el usuario tiene permiso para ver citas
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.View);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de acceso no autorizado a detalle de cita");
            throw new ForbiddenAccessException("No tiene permiso para ver detalles de citas");
        }
        
        var cita = await _citaRepository.GetByIdAsync(id);
        if (cita == null)
        {
            throw new NotFoundException(ErrorMessages.CitaNotFound, ErrorCodes.CitaNotFound);
        }

        var citaDetailDto = _mapper.Map<CitaDetailDto>(cita);
        
        // Calculamos la duración en minutos
        citaDetailDto.DuracionMinutos = (int)(cita.FechaHoraFin - cita.FechaHoraInicio).TotalMinutes;
        
        // Verificamos si tiene recordatorios
        citaDetailDto.TieneRecordatorios = cita.Recordatorios.Any();
        
        // Si el doctor tiene detalles adicionales, los incluimos
        var doctorDetail = await _doctorRepository.GetDoctorDetailByIdAsync(cita.IdDoctor);
        if (doctorDetail != null)
        {
            citaDetailDto.Especialidad = doctorDetail.Especialidad;
        }
        
        return citaDetailDto;
    }

    public async Task<CitaDto> CreateAsync(CitaCreateDto citaDto, Guid idUsuarioActual)
    {
        // Verificar si el usuario tiene permiso para crear citas
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.Create);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de creación no autorizada de cita");
            throw new ForbiddenAccessException("No tiene permiso para crear citas");
        }
        
        // Verificar si existe el doctor
        var doctor = await _usuarioRepository.GetByIdAsync(citaDto.IdDoctor);
        if (doctor == null)
        {
            throw new NotFoundException(ErrorMessages.UsuarioNotFound, ErrorCodes.UsuarioNotFound);
        }
        
        // Verificar que el usuario sea un doctor y obtener sus detalles
        var doctorDetail = await _doctorRepository.GetDoctorDetailByIdAsync(citaDto.IdDoctor);
        var esDoctor = await _doctorRepository.IsUserDoctorAsync(citaDto.IdDoctor);
        if (!esDoctor)
        {
            throw new BadRequestException(
                "Solo se pueden agendar citas a usuarios con rol de Doctor", 
                ErrorCodes.InvalidOperation);
        }

        // Verificar si hay traslape de citas para este doctor
        var hayTraslape = await _citaRepository.HasOverlappingAppointmentsAsync(
            citaDto.IdDoctor, citaDto.FechaHoraInicio, citaDto.FechaHoraFin);

        if (hayTraslape)
        {
            throw new BadRequestException(ErrorMessages.CitaOverlap, ErrorCodes.CitaOverlap);
        }

        // Verificar que la fecha de inicio no sea pasada
        if (citaDto.FechaHoraInicio < DateTime.Now)
        {
            throw new BadRequestException(ErrorMessages.CitaPastDate, ErrorCodes.CitaPastDate);
        }

        // Verificar que la fecha de fin sea posterior a la de inicio
        if (citaDto.FechaHoraFin <= citaDto.FechaHoraInicio)
        {
            throw new BadRequestException(ErrorMessages.CitaInvalidTimeRange, ErrorCodes.CitaInvalidTimeRange);
        }

        // Crear la cita
        var cita = _mapper.Map<Cita>(citaDto);
        cita.Estatus = "Programada";  // Estado inicial por defecto

        var citaCreated = await _citaRepository.AddAsync(cita);
        return _mapper.Map<CitaDto>(citaCreated);
    }

    public async Task<CitaDto> UpdateAsync(CitaUpdateDto citaDto)
    {
        // Verificar si el usuario tiene permiso para actualizar citas
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.Update);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de actualización no autorizada de cita");
            throw new ForbiddenAccessException("No tiene permiso para actualizar citas");
        }
        
        // Verificar si existe la cita
        var citaExistente = await _citaRepository.GetByIdAsync(citaDto.IdCita);
        if (citaExistente == null)
        {
            throw new NotFoundException(ErrorMessages.CitaNotFound, ErrorCodes.CitaNotFound);
        }

        // Verificar si la cita está cancelada
        if (citaExistente.Estatus == "Cancelada" && citaDto.Estatus != "Cancelada")
        {
            throw new BadRequestException(ErrorMessages.CitaCancelled, ErrorCodes.CitaCancelled);
        }
        
        // Verificar que el usuario asignado sea un doctor
        if (citaDto.IdDoctor != citaExistente.IdDoctor)
        {
            var doctorDetail = await _doctorRepository.GetDoctorDetailByIdAsync(citaDto.IdDoctor);
            var esDoctor = await _doctorRepository.IsUserDoctorAsync(citaDto.IdDoctor);
            if (!esDoctor)
            {
                throw new ValidationException(
                    "Solo se pueden agendar citas a usuarios con rol de Doctor", 
                    ErrorCodes.InvalidOperation);
            }
        }

        // Verificar si existe el paciente
        var paciente = await _pacienteRepository.GetByIdAsync(citaDto.IdPaciente);
        if (paciente == null)
        {
            throw new NotFoundException(ErrorMessages.PacienteNotFound, ErrorCodes.PacienteNotFound);
        }

        // Verificar si hay traslape de citas para este doctor (excluyendo la cita actual)
        var hayTraslape = await _citaRepository.HasOverlappingAppointmentsAsync(
            citaDto.IdDoctor, citaDto.FechaHoraInicio, citaDto.FechaHoraFin, citaDto.IdCita);

        if (hayTraslape)
        {
            throw new BadRequestException(ErrorMessages.CitaOverlap, ErrorCodes.CitaOverlap);
        }

        // Si la fecha es futura, verificar que no sea pasada
        if (citaDto.FechaHoraInicio.Date > DateTime.Now.Date && citaDto.FechaHoraInicio < DateTime.Now)
        {
            throw new BadRequestException(ErrorMessages.CitaPastDate, ErrorCodes.CitaPastDate);
        }

        // Verificar que la fecha de fin sea posterior a la de inicio
        if (citaDto.FechaHoraFin <= citaDto.FechaHoraInicio)
        {
            throw new BadRequestException(ErrorMessages.CitaInvalidTimeRange, ErrorCodes.CitaInvalidTimeRange);
        }

        // Actualizar la cita
        _mapper.Map(citaDto, citaExistente);
        var citaActualizada = await _citaRepository.UpdateAsync(citaExistente);
        return _mapper.Map<CitaDto>(citaActualizada);
    }

    public async Task CancelAsync(Guid id)
    {
        // Verificar si el usuario tiene permiso para cancelar citas
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.Cancel);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de cancelación no autorizada de cita");
            throw new ForbiddenAccessException("No tiene permiso para cancelar citas");
        }
        
        var cita = await _citaRepository.GetByIdAsync(id);
        if (cita == null)
        {
            throw new NotFoundException(ErrorMessages.CitaNotFound, ErrorCodes.CitaNotFound);
        }

        cita.Estatus = "Cancelada";
        await _citaRepository.UpdateAsync(cita);
    }

    public async Task DeleteAsync(Guid id)
    {
        // Verificar si el usuario tiene permiso para eliminar citas
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.Delete);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de eliminación no autorizada de cita");
            throw new ForbiddenAccessException("No tiene permiso para eliminar citas");
        }
        
        var cita = await _citaRepository.GetByIdAsync(id);
        if (cita == null)
        {
            throw new NotFoundException(ErrorMessages.CitaNotFound, ErrorCodes.CitaNotFound);
        }

        // Solo permitimos eliminar citas que estén canceladas o en el pasado
        if (cita.Estatus != "Cancelada" && cita.FechaHoraInicio > DateTime.Now)
        {
            throw new BadRequestException("No se pueden eliminar citas programadas para el futuro. Cancele la cita primero.");
        }

        await _citaRepository.DeleteAsync(id);
    }
}
