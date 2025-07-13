using AutoMapper;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using DentalPro.Application.DTOs.Recordatorios;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using Microsoft.Extensions.Logging;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio especializado en notificaciones de recordatorios
/// </summary>
public class RecordatorioNotificacionService : IRecordatorioNotificacionService
{
    private readonly IRecordatorioRepository _recordatorioRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<RecordatorioNotificacionService> _logger;
    private readonly ICurrentUserService _currentUserService;

    public RecordatorioNotificacionService(
        IRecordatorioRepository recordatorioRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        ILogger<RecordatorioNotificacionService> logger)
    {
        _recordatorioRepository = recordatorioRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Marca un recordatorio como enviado
    /// </summary>
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
            _logger.LogInformation("El recordatorio {RecordatorioId} ya estaba marcado como enviado", id);
            return;
        }

        await _recordatorioRepository.MarkAsSentAsync(id);
        _logger.LogInformation("Recordatorio {RecordatorioId} marcado como enviado", id);
    }

    /// <summary>
    /// Obtiene recordatorios pendientes de envío
    /// </summary>
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
        _logger.LogInformation("Se encontraron {Count} recordatorios pendientes de envío", recordatorios.Count());
        return _mapper.Map<IEnumerable<RecordatorioDto>>(recordatorios);
    }
}
