using DentalPro.Application.DTOs.Rol;
using DentalPro.Application.Interfaces;
using DentalPro.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
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
        var rolesDto = roles.Select(r => new RolDto
        {
            IdRol = r.IdRol,
            Nombre = r.Nombre
        }).ToList();
        
        return Ok(rolesDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RolDto>> GetRol(Guid id)
    {
        var rol = await _rolService.GetByIdAsync(id);
        if (rol == null)
        {
            return NotFound();
        }

        var rolDto = new RolDto
        {
            IdRol = rol.IdRol,
            Nombre = rol.Nombre
        };
        
        return Ok(rolDto);
    }

    [HttpPost]
    public async Task<ActionResult<RolDto>> CreateRol([FromBody] RolDto rolDto)
    {
        try
        {
            var rol = new Rol
            {
                Nombre = rolDto.Nombre
            };

            var createdRol = await _rolService.CreateAsync(rol);
            
            var result = new RolDto
            {
                IdRol = createdRol.IdRol,
                Nombre = createdRol.Nombre
            };

            return CreatedAtAction(nameof(GetRol), new { id = result.IdRol }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRol(Guid id, [FromBody] RolDto rolDto)
    {
        try
        {
            if (id != rolDto.IdRol)
            {
                return BadRequest("El ID de la URL no coincide con el ID en el cuerpo de la solicitud");
            }

            var rol = await _rolService.GetByIdAsync(id);
            if (rol == null)
            {
                return NotFound();
            }

            rol.Nombre = rolDto.Nombre;
            
            var result = await _rolService.UpdateAsync(rol);
            if (!result)
            {
                return BadRequest("No se pudo actualizar el rol");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRol(Guid id)
    {
        try
        {
            var rol = await _rolService.GetByIdAsync(id);
            if (rol == null)
            {
                return NotFound();
            }

            var result = await _rolService.DeleteAsync(id);
            if (!result)
            {
                return BadRequest("No se pudo eliminar el rol");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
