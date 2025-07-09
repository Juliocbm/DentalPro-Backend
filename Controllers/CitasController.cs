using DentalPro.Application.Common.Constants;
using DentalPro.Application.DTOs.Citas;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Api.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DentalPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitasController : ControllerBase
{
    private readonly ICitaService _citaService;
    private readonly ILogger<CitasController> _logger;

    public CitasController(ICitaService citaService, ILogger<CitasController> logger)
    {
        _citaService = citaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las citas del consultorio del usuario actual
    /// </summary>
    [HttpGet]
    [RequireConsultorioAccess]
    public async Task<ActionResult<IEnumerable<CitaDto>>> GetAll()
    {
        var citas = await _citaService.GetAllAsync();
        return Ok(citas);
    }

    /// <summary>
    /// Obtiene todas las citas en un rango de fechas
    /// </summary>
    [HttpGet("rango")]
    [RequireConsultorioAccess]
    public async Task<ActionResult<IEnumerable<CitaDto>>> GetByDateRange(
        [FromQuery] DateTime fechaInicio, 
        [FromQuery] DateTime fechaFin)
    {
        var citas = await _citaService.GetByDateRangeAsync(fechaInicio, fechaFin);
        return Ok(citas);
    }

    /// <summary>
    /// Obtiene todas las citas de un paciente específico
    /// </summary>
    [HttpGet("paciente/{idPaciente:guid}")]
    [RequireConsultorioAccess]
    public async Task<ActionResult<IEnumerable<CitaDto>>> GetByPaciente(Guid idPaciente)
    {
        var citas = await _citaService.GetByPacienteAsync(idPaciente);
        return Ok(citas);
    }

    /// <summary>
    /// Obtiene todas las citas de un usuario (doctor) específico
    /// </summary>
    [HttpGet("usuario/{idUsuario:guid}")]
    [RequireConsultorioAccess]
    public async Task<ActionResult<IEnumerable<CitaDto>>> GetByUsuario(Guid idUsuario)
    {
        var citas = await _citaService.GetByUsuarioAsync(idUsuario);
        return Ok(citas);
    }

    /// <summary>
    /// Obtiene una cita por su ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequireConsultorioAccess]
    public async Task<ActionResult<CitaDetailDto>> GetById(Guid id)
    {
        var cita = await _citaService.GetByIdAsync(id);
        return Ok(cita);
    }

    /// <summary>
    /// Crea una nueva cita
    /// </summary>
    [HttpPost]
    [RequireConsultorioAccess]
    public async Task<ActionResult<CitaDto>> Create(CitaCreateDto citaDto)
    {
        // Obtener el ID del usuario actual desde el token
        var idUsuarioActual = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var citaCreada = await _citaService.CreateAsync(citaDto, idUsuarioActual);
        return CreatedAtAction(nameof(GetById), new { id = citaCreada.IdCita }, citaCreada);
    }

    /// <summary>
    /// Actualiza una cita existente
    /// </summary>
    [HttpPut]
    [RequireConsultorioAccess]
    public async Task<ActionResult<CitaDto>> Update(CitaUpdateDto citaDto)
    {
        var citaActualizada = await _citaService.UpdateAsync(citaDto);
        return Ok(citaActualizada);
    }

    /// <summary>
    /// Cancela una cita
    /// </summary>
    [HttpPatch("cancelar/{id:guid}")]
    [RequireConsultorioAccess]
    public async Task<ActionResult> Cancel(Guid id)
    {
        await _citaService.CancelAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Elimina una cita
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _citaService.DeleteAsync(id);
        return NoContent();
    }
}
