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
/// Implementación de la fachada para operaciones de citas que delega en servicios especializados
/// </summary>
public class CitaService : ICitaService
{
    private readonly ICitaManagementService _citaManagementService;
    private readonly ICitaValidationService _citaValidationService;
    private readonly ICitaNotificacionService _citaNotificacionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CitaService> _logger;

    public CitaService(
        ICitaManagementService citaManagementService,
        ICitaValidationService citaValidationService,
        ICitaNotificacionService citaNotificacionService,
        ICurrentUserService currentUserService,
        IPacienteRepository pacienteRepository,
        IMapper mapper,
        ILogger<CitaService> logger)
    {
        _citaManagementService = citaManagementService ?? throw new ArgumentNullException(nameof(citaManagementService));
        _citaValidationService = citaValidationService ?? throw new ArgumentNullException(nameof(citaValidationService));
        _citaNotificacionService = citaNotificacionService ?? throw new ArgumentNullException(nameof(citaNotificacionService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<CitaDto>> GetAllAsync()
    {
        _logger.LogInformation("Iniciando operación GetAllAsync");
        
        // Verificar si el usuario tiene permiso para ver todas las citas
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.ViewAll);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de acceso no autorizado a todas las citas");
            throw new ForbiddenAccessException("No tiene permiso para ver todas las citas", ErrorCodes.InsufficientPermissions);
        }
        
        try
        {
            return await _citaManagementService.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las citas");
            throw; // Re-lanzar para mantener el stack trace
        }
    }

    public async Task<IEnumerable<CitaDto>> GetByDateRangeAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        _logger.LogInformation("Iniciando operación GetByDateRangeAsync para el rango {FechaInicio} - {FechaFin}", 
            fechaInicio, fechaFin);
        
        // Verificar si el usuario tiene permiso para ver citas
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.View);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de acceso no autorizado a citas por rango de fechas");
            throw new ForbiddenAccessException("No tiene permiso para ver citas", ErrorCodes.InsufficientPermissions);
        }
        
        try
        {
            // Obtenemos el ID del consultorio del usuario actual
            var idConsultorio = _currentUserService.GetCurrentConsultorioId();
            return await _citaManagementService.GetByDateRangeAsync(fechaInicio, fechaFin, idConsultorio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener citas por rango de fechas {FechaInicio} - {FechaFin}", 
                fechaInicio, fechaFin);
            throw; // Re-lanzar para mantener el stack trace
        }
    }

    public async Task<IEnumerable<CitaDto>> GetByPacienteAsync(Guid idPaciente)
    {
        _logger.LogInformation("Iniciando operación GetByPacienteAsync para paciente {IdPaciente}", idPaciente);
        
        // Verificar si el usuario tiene permiso para ver citas de pacientes
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.ViewByPaciente);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de acceso no autorizado a citas de un paciente");
            throw new ForbiddenAccessException("No tiene permiso para ver citas de pacientes", ErrorCodes.InsufficientPermissions);
        }
        
        try
        {
            // Verificar que exista el paciente antes de buscar sus citas
            var paciente = await _pacienteRepository.GetByIdAsync(idPaciente);
            if (paciente == null)
            {
                throw new NotFoundException(ErrorMessages.PacienteNotFound, ErrorCodes.PacienteNotFound);
            }
            
            return await _citaManagementService.GetByPacienteAsync(idPaciente);
        }
        catch (NotFoundException)
        {
            throw; // Re-lanzamos excepciones de no encontrado sin modificar
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener citas para el paciente {IdPaciente}", idPaciente);
            throw; // Re-lanzar para mantener el stack trace
        }
    }

    public async Task<IEnumerable<CitaDto>> GetByDoctorAsync(Guid idDoctor)
    {
        _logger.LogInformation("Iniciando operación GetByDoctorAsync para doctor {IdDoctor}", idDoctor);
        
        // Verificar si el usuario tiene permiso para ver citas de doctores
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.ViewByDoctor);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de acceso no autorizado a citas de un doctor");
            throw new ForbiddenAccessException("No tiene permiso para ver citas de doctores", ErrorCodes.InsufficientPermissions);
        }
        
        try
        {
            // Verificar que el usuario sea doctor
            var esDoctor = await _citaValidationService.IsUserDoctorAsync(idDoctor);
            if (!esDoctor)
            {
                throw new ValidationException("El usuario especificado no es un doctor", ErrorCodes.InvalidOperation);
            }
            
            // Obtenemos el ID del consultorio del usuario actual
            var idConsultorio = _currentUserService.GetCurrentConsultorioId();
            return await _citaManagementService.GetByDoctorAsync(idDoctor, idConsultorio);
        }
        catch (ValidationException)
        {
            throw; // Re-lanzamos excepciones de validación sin modificar
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener citas para el doctor {IdDoctor}", idDoctor);
            throw; // Re-lanzar para mantener el stack trace
        }
    }

    public async Task<CitaDetailDto> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Iniciando operación GetByIdAsync para cita {IdCita}", id);
        
        // Verificar si el usuario tiene permiso para ver citas
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.View);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de acceso no autorizado a cita por ID");
            throw new ForbiddenAccessException("No tiene permiso para ver citas", ErrorCodes.InsufficientPermissions);
        }
        
        try
        {
            return await _citaManagementService.GetByIdAsync(id);
        }
        catch (NotFoundException)
        {
            throw; // Re-lanzamos excepciones de no encontrado sin modificar
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cita con ID {IdCita}", id);
            throw; // Re-lanzar para mantener el stack trace
        }
    }

    public async Task<CitaDto> CreateAsync(CitaCreateDto citaDto, Guid idUsuarioActual)
    {
        _logger.LogInformation("Iniciando operación CreateAsync para nueva cita");
        
        // Verificar si el usuario tiene permiso para crear citas
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.Create);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de creación no autorizada de cita");
            throw new ForbiddenAccessException("No tiene permiso para crear citas", ErrorCodes.InsufficientPermissions);
        }
        
        try
        {
            // Validar los datos de la cita
            await _citaValidationService.ValidateForCreateAsync(citaDto);
            
            // Mapear DTO a entidad
            var cita = _mapper.Map<Cita>(citaDto);
            //cita.FechaCreacion = DateTime.Now;
            //cita.IdUsuarioCreacion = idUsuarioActual;
            cita.Estatus = "Programada";
            
            // Crear la cita en la base de datos
            var citaCreada = await _citaManagementService.CreateAsync(cita);
            
            // Enviar notificación de confirmación
            await _citaNotificacionService.SendConfirmacionCitaAsync(citaCreada);
            
            return citaCreada;
        }
        catch (ValidationException)
        {
            throw; // Re-lanzamos excepciones de validación sin modificar
        }
        catch (BadRequestException)
        {
            throw; // Re-lanzamos excepciones de solicitud incorrecta sin modificar
        }
        catch (NotFoundException)
        {
            throw; // Re-lanzamos excepciones de no encontrado sin modificar
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear nueva cita");
            throw; // Re-lanzar para mantener el stack trace
        }
    }

    public async Task<CitaDto> UpdateAsync(CitaUpdateDto citaDto)
    {
        _logger.LogInformation("Iniciando operación UpdateAsync para cita {IdCita}", citaDto.IdCita);
        
        // Verificar si el usuario tiene permiso para actualizar citas
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.Update);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de actualización no autorizada de cita");
            throw new ForbiddenAccessException("No tiene permiso para actualizar citas", ErrorCodes.InsufficientPermissions);
        }
        
        try
        {
            // Obtener la cita existente
            var citaDetailDto = await _citaManagementService.GetByIdAsync(citaDto.IdCita);
            if (citaDetailDto == null)
            {
                throw new NotFoundException(ErrorMessages.CitaNotFound, ErrorCodes.CitaNotFound);
            }
            
            // Mapear el DTO de detalle a entidad para tener todos los datos
            var citaExistente = _mapper.Map<Cita>(citaDetailDto);
            
            // Validar los datos de actualización
            await _citaValidationService.ValidateForUpdateAsync(citaDto, citaExistente);
            
            // Mapear los cambios del DTO al objeto existente
            _mapper.Map(citaDto, citaExistente);
            
            // Actualizar la cita en la base de datos
            var citaActualizada = await _citaManagementService.UpdateAsync(citaExistente);
            
            // Enviar notificación de actualización
            await _citaNotificacionService.SendActualizacionCitaAsync(citaActualizada);
            
            return citaActualizada;
        }
        catch (ValidationException)
        {
            throw; // Re-lanzamos excepciones de validación sin modificar
        }
        catch (BadRequestException)
        {
            throw; // Re-lanzamos excepciones de solicitud incorrecta sin modificar
        }
        catch (NotFoundException)
        {
            throw; // Re-lanzamos excepciones de no encontrado sin modificar
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cita con ID {IdCita}", citaDto.IdCita);
            throw; // Re-lanzar para mantener el stack trace
        }
    }

    public async Task CancelAsync(Guid id)
    {
        _logger.LogInformation("Iniciando operación CancelAsync para cita {IdCita}", id);
        
        // Verificar si el usuario tiene permiso para cancelar citas
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.Cancel);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de cancelación no autorizada de cita");
            throw new ForbiddenAccessException("No tiene permiso para cancelar citas", ErrorCodes.InsufficientPermissions);
        }
        
        try
        {
            // Validar que se pueda cancelar la cita
            await _citaValidationService.ValidateForCancelAsync(id);
            
            // Cancelar la cita en la base de datos
            await _citaManagementService.CancelAsync(id);
            
            // Enviar notificación de cancelación
            await _citaNotificacionService.SendCancelacionCitaAsync(id);
        }
        catch (BadRequestException)
        {
            throw; // Re-lanzamos excepciones de solicitud incorrecta sin modificar
        }
        catch (NotFoundException)
        {
            throw; // Re-lanzamos excepciones de no encontrado sin modificar
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar cita con ID {IdCita}", id);
            throw; // Re-lanzar para mantener el stack trace
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Iniciando operación DeleteAsync para cita {IdCita}", id);
        
        // Verificar si el usuario tiene permiso para eliminar citas
        var hasPermiso = await _currentUserService.HasPermisoAsync(CitasPermissions.Delete);
        if (!hasPermiso)
        {
            _logger.LogWarning("Intento de eliminación no autorizada de cita");
            throw new ForbiddenAccessException("No tiene permiso para eliminar citas", ErrorCodes.InsufficientPermissions);
        }
        
        try
        {
            // Validar que se pueda eliminar la cita
            await _citaValidationService.ValidateForDeleteAsync(id);
            
            // Eliminar la cita en la base de datos
            await _citaManagementService.DeleteAsync(id);
        }
        catch (BadRequestException)
        {
            throw; // Re-lanzamos excepciones de solicitud incorrecta sin modificar
        }
        catch (NotFoundException)
        {
            throw; // Re-lanzamos excepciones de no encontrado sin modificar
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cita con ID {IdCita}", id);
            throw; // Re-lanzar para mantener el stack trace
        }
    }
}
