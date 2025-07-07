using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.DTOs.Rol;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireAdminRole")]
public class RolesController : ControllerBase
{
    private readonly IRolService _rolService;

    public RolesController(IRolService rolService)
    {
        _rolService = rolService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RolDto>>> GetRoles()
    {
        var roles = await _rolService.GetAllAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RolDto>> GetRol(Guid id)
    {
        var rolDto = await _rolService.GetByIdAsync(id);
        if (rolDto == null)
        {
            throw new NotFoundException("Rol", id);
        }
        
        return Ok(rolDto);
    }

    [HttpPost]
    public async Task<ActionResult<RolDto>> CreateRol([FromBody] RolCreateDto rolCreateDto)
    {       
        var createdRol = await _rolService.CreateAsync(rolCreateDto);
        return CreatedAtAction(nameof(GetRol), new { id = createdRol.IdRol }, createdRol);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RolDto>> UpdateRol(Guid id, [FromBody] RolUpdateDto rolUpdateDto)
    {
        if (id != rolUpdateDto.IdRol)
        {
            throw new BadRequestException("El ID de la URL no coincide con el ID en el cuerpo de la solicitud");
        }

        // La validación de existencia ahora se maneja en el servicio
        var updatedRol = await _rolService.UpdateAsync(rolUpdateDto);
        return Ok(updatedRol);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRol(Guid id)
    {
        var rol = await _rolService.GetByIdAsync(id);
        if (rol == null)
        {
            throw new NotFoundException("Rol", id);
        }

        var result = await _rolService.DeleteAsync(id);
        if (!result)
        {
            throw new BadRequestException("No se pudo eliminar el rol");
        }

        return NoContent();
    }
}
