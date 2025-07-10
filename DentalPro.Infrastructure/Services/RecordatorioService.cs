using AutoMapper;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using DentalPro.Application.DTOs.Recordatorios;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de recordatorios
/// </summary>
public class RecordatorioService : IRecordatorioService
{
    private readonly IRecordatorioRepository _recordatorioRepository;
    private readonly ICitaRepository _citaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<RecordatorioService> _logger;
    private readonly ICurrentUserService _currentUserService;

    public RecordatorioService(
        IRecordatorioRepository recordatorioRepository,
        ICitaRepository citaRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        ILogger<RecordatorioService> logger)
    {
        _recordatorioRepository = recordatorioRepository;
        _citaRepository = citaRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<IEnumerable<RecordatorioDto>> GetByCitaAsync(Guid idCita)
    {
        // Verificar permiso para ver recordatorios por cita
        if (!await _currentUserService.HasPermisoAsync(RecordatoriosPermissions.ViewByCita))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó ver recordatorios por cita sin el permiso requerido", 
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        // Verificar si existe la cita
        var citaExists = await _citaRepository.ExistsByIdAsync(idCita);
        if (!citaExists)
        {
            throw new NotFoundException(ErrorMessages.CitaNotFound, ErrorCodes.CitaNotFound);
        }

        var recordatorios = await _recordatorioRepository.GetByCitaAsync(idCita);
        return _mapper.Map<IEnumerable<RecordatorioDto>>(recordatorios);
    }

    public async Task<RecordatorioDto> GetByIdAsync(Guid id)
    {
        // Verificar permiso para ver un recordatorio
        if (!await _currentUserService.HasPermisoAsync(RecordatoriosPermissions.View))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó ver un recordatorio sin el permiso requerido", 
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        var recordatorio = await _recordatorioRepository.GetByIdAsync(id);
        if (recordatorio == null)
        {
            throw new NotFoundException("El recordatorio no existe.", ErrorCodes.ResourceNotFound);
        }

        return _mapper.Map<RecordatorioDto>(recordatorio);
    }

    public async Task<RecordatorioDto> CreateAsync(RecordatorioCreateDto recordatorioDto)
    {
        // Verificar permiso para crear recordatorios
        if (!await _currentUserService.HasPermisoAsync(RecordatoriosPermissions.Create))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó crear un recordatorio sin el permiso requerido", 
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        // Verificar si existe la cita
        var cita = await _citaRepository.GetByIdAsync(recordatorioDto.IdCita);
        if (cita == null)
        {
            throw new NotFoundException(ErrorMessages.CitaNotFound, ErrorCodes.CitaNotFound);
        }

        // Verificar que la fecha programada sea anterior a la fecha de la cita
        if (recordatorioDto.FechaProgramada > cita.FechaHoraInicio)
        {
            throw new BadRequestException(
                "La fecha programada del recordatorio debe ser anterior a la fecha de la cita.",
                ErrorCodes.InvalidOperation);
        }

        var recordatorio = _mapper.Map<Recordatorio>(recordatorioDto);
        recordatorio.Enviado = false;
        recordatorio.FechaEnvio = null;

        var recordatorioCreated = await _recordatorioRepository.AddAsync(recordatorio);
        return _mapper.Map<RecordatorioDto>(recordatorioCreated);
    }

    public async Task<RecordatorioDto> UpdateAsync(RecordatorioUpdateDto recordatorioDto)
    {
        // Verificar permiso para actualizar recordatorios
        if (!await _currentUserService.HasPermisoAsync(RecordatoriosPermissions.Update))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó actualizar un recordatorio sin el permiso requerido", 
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        // Verificar si existe el recordatorio
        var recordatorioExistente = await _recordatorioRepository.GetByIdAsync(recordatorioDto.IdRecordatorio);
        if (recordatorioExistente == null)
        {
            throw new NotFoundException("El recordatorio no existe.", ErrorCodes.ResourceNotFound);
        }

        // Verificar si existe la cita
        var cita = await _citaRepository.GetByIdAsync(recordatorioDto.IdCita);
        if (cita == null)
        {
            throw new NotFoundException(ErrorMessages.CitaNotFound, ErrorCodes.CitaNotFound);
        }

        // No permitir actualizar recordatorios ya enviados
        if (recordatorioExistente.Enviado)
        {
            throw new BadRequestException(
                "No se puede modificar un recordatorio que ya ha sido enviado.",
                ErrorCodes.InvalidOperation);
        }

        // Verificar que la fecha programada sea anterior a la fecha de la cita
        if (recordatorioDto.FechaProgramada > cita.FechaHoraInicio)
        {
            throw new BadRequestException(
                "La fecha programada del recordatorio debe ser anterior a la fecha de la cita.",
                ErrorCodes.InvalidOperation);
        }

        // Actualizar recordatorio
        _mapper.Map(recordatorioDto, recordatorioExistente);
        var recordatorioUpdated = await _recordatorioRepository.UpdateAsync(recordatorioExistente);
        return _mapper.Map<RecordatorioDto>(recordatorioUpdated);
    }

    public async Task MarkAsSentAsync(Guid id)
    {
        // Verificar permiso para marcar recordatorios como enviados
        if (!await _currentUserService.HasPermisoAsync(RecordatoriosPermissions.MarkAsSent))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó marcar un recordatorio como enviado sin el permiso requerido", 
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        var recordatorio = await _recordatorioRepository.GetByIdAsync(id);
        if (recordatorio == null)
        {
            throw new NotFoundException("El recordatorio no existe.", ErrorCodes.ResourceNotFound);
        }

        if (recordatorio.Enviado)
        {
            // Si ya está marcado como enviado, no hacemos nada
            return;
        }

        await _recordatorioRepository.MarkAsSentAsync(id);
    }

    public async Task<IEnumerable<RecordatorioDto>> GetPendingAsync()
    {
        // Verificar permiso para ver recordatorios pendientes
        if (!await _currentUserService.HasPermisoAsync(RecordatoriosPermissions.ViewPending))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó ver recordatorios pendientes sin el permiso requerido", 
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        var recordatorios = await _recordatorioRepository.GetPendingAsync();
        return _mapper.Map<IEnumerable<RecordatorioDto>>(recordatorios);
    }

    public async Task DeleteAsync(Guid id)
    {
        // Verificar permiso para eliminar recordatorios
        if (!await _currentUserService.HasPermisoAsync(RecordatoriosPermissions.Delete))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó eliminar un recordatorio sin el permiso requerido", 
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        var recordatorio = await _recordatorioRepository.GetByIdAsync(id);
        if (recordatorio == null)
        {
            throw new NotFoundException("El recordatorio no existe.", ErrorCodes.ResourceNotFound);
        }

        // No permitir eliminar recordatorios ya enviados
        if (recordatorio.Enviado)
        {
            throw new BadRequestException(
                "No se puede eliminar un recordatorio que ya ha sido enviado.",
                ErrorCodes.InvalidOperation);
        }

        await _recordatorioRepository.DeleteAsync(id);
    }
}
