using DentalPro.Application.DTOs.Recordatorios;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Api.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecordatoriosController : ControllerBase
{
    private readonly IRecordatorioService _recordatorioService;
    private readonly ILogger<RecordatoriosController> _logger;

    public RecordatoriosController(IRecordatorioService recordatorioService, ILogger<RecordatoriosController> logger)
    {
        _recordatorioService = recordatorioService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los recordatorios de una cita
    /// </summary>
    [HttpGet("cita/{idCita:guid}")]
    [RequireConsultorioAccess]
    public async Task<ActionResult<IEnumerable<RecordatorioDto>>> GetByCita(Guid idCita)
    {
        var recordatorios = await _recordatorioService.GetByCitaAsync(idCita);
        return Ok(recordatorios);
    }

    /// <summary>
    /// Obtiene un recordatorio por su ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequireConsultorioAccess]
    public async Task<ActionResult<RecordatorioDto>> GetById(Guid id)
    {
        var recordatorio = await _recordatorioService.GetByIdAsync(id);
        return Ok(recordatorio);
    }

    /// <summary>
    /// Crea un nuevo recordatorio
    /// </summary>
    [HttpPost]
    [RequireConsultorioAccess]
    public async Task<ActionResult<RecordatorioDto>> Create(RecordatorioCreateDto recordatorioDto)
    {
        var recordatorio = await _recordatorioService.CreateAsync(recordatorioDto);
        return CreatedAtAction(nameof(GetById), new { id = recordatorio.IdRecordatorio }, recordatorio);
    }

    /// <summary>
    /// Actualiza un recordatorio existente
    /// </summary>
    [HttpPut]
    [RequireConsultorioAccess]
    public async Task<ActionResult<RecordatorioDto>> Update(RecordatorioUpdateDto recordatorioDto)
    {
        var recordatorio = await _recordatorioService.UpdateAsync(recordatorioDto);
        return Ok(recordatorio);
    }

    /// <summary>
    /// Marca un recordatorio como enviado
    /// </summary>
    [HttpPatch("marcar-enviado/{id:guid}")]
    [RequireConsultorioAccess]
    public async Task<ActionResult> MarkAsSent(Guid id)
    {
        await _recordatorioService.MarkAsSentAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Obtiene todos los recordatorios pendientes de envío (para sistema de notificaciones)
    /// </summary>
    [HttpGet("pendientes")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<IEnumerable<RecordatorioDto>>> GetPending()
    {
        var recordatorios = await _recordatorioService.GetPendingAsync();
        return Ok(recordatorios);
    }

    /// <summary>
    /// Elimina un recordatorio
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _recordatorioService.DeleteAsync(id);
        return NoContent();
    }
}
