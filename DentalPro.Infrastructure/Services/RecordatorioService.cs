using DentalPro.Application.DTOs.Recordatorios;
using DentalPro.Application.Interfaces.IServices;
using Microsoft.Extensions.Logging;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de recordatorios que actúa como fachada
/// delegando en servicios especializados
/// </summary>
public class RecordatorioService : IRecordatorioService
{
    private readonly IRecordatorioManagementService _managementService;
    private readonly IRecordatorioNotificacionService _notificacionService;
    private readonly ILogger<RecordatorioService> _logger;

    public RecordatorioService(
        IRecordatorioManagementService managementService,
        IRecordatorioNotificacionService notificacionService,
        ILogger<RecordatorioService> logger)
    {
        _managementService = managementService;
        _notificacionService = notificacionService;
        _logger = logger;
    }

    #region Delegación a IRecordatorioManagementService

    /// <summary>
    /// Delega en RecordatorioManagementService para obtener recordatorios por cita
    /// </summary>
    public async Task<IEnumerable<RecordatorioDto>> GetByCitaAsync(Guid idCita)
    {
        _logger.LogInformation("Delegando GetByCitaAsync({CitaId}) a RecordatorioManagementService", idCita);
        return await _managementService.GetByCitaAsync(idCita);
    }

    /// <summary>
    /// Delega en RecordatorioManagementService para obtener un recordatorio por ID
    /// </summary>
    public async Task<RecordatorioDto> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Delegando GetByIdAsync({RecordatorioId}) a RecordatorioManagementService", id);
        return await _managementService.GetByIdAsync(id);
    }

    /// <summary>
    /// Delega en RecordatorioManagementService para crear un recordatorio
    /// </summary>
    public async Task<RecordatorioDto> CreateAsync(RecordatorioCreateDto recordatorioDto)
    {
        _logger.LogInformation("Delegando CreateAsync para cita {CitaId} a RecordatorioManagementService", recordatorioDto.IdCita);
        return await _managementService.CreateAsync(recordatorioDto);
    }

    /// <summary>
    /// Delega en RecordatorioManagementService para actualizar un recordatorio
    /// </summary>
    public async Task<RecordatorioDto> UpdateAsync(RecordatorioUpdateDto recordatorioDto)
    {
        _logger.LogInformation("Delegando UpdateAsync({RecordatorioId}) a RecordatorioManagementService", recordatorioDto.IdRecordatorio);
        return await _managementService.UpdateAsync(recordatorioDto);
    }

    #endregion

    #region Delegación a IRecordatorioNotificacionService

    /// <summary>
    /// Delega en RecordatorioNotificacionService para marcar un recordatorio como enviado
    /// </summary>
    public async Task MarkAsSentAsync(Guid id)
    {
        _logger.LogInformation("Delegando MarkAsSentAsync({RecordatorioId}) a RecordatorioNotificacionService", id);
        await _notificacionService.MarkAsSentAsync(id);
    }

    /// <summary>
    /// Delega en RecordatorioNotificacionService para obtener recordatorios pendientes de envío
    /// </summary>
    public async Task<IEnumerable<RecordatorioDto>> GetPendingAsync()
    {
        _logger.LogInformation("Delegando GetPendingAsync() a RecordatorioNotificacionService");
        return await _notificacionService.GetPendingAsync();
    }

    /// <summary>
    /// Delega en RecordatorioManagementService para eliminar un recordatorio
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Delegando DeleteAsync({RecordatorioId}) a RecordatorioManagementService", id);
        await _managementService.DeleteAsync(id);
    }

    #endregion
}
