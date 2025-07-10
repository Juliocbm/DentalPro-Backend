using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using DentalPro.Application.DTOs.Permiso;
using DentalPro.Application.DTOs.Rol;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Api.Infrastructure.Authorization;
using DentalPro.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DentalPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRolService _rolService;

    public RolesController(IRolService rolService)
    {
        _rolService = rolService;
    }

    [HttpGet]
    [RequirePermiso(RolesPermissions.ViewAll)]
    public async Task<ActionResult<IEnumerable<RolDto>>> GetRoles()
    {
        var roles = await _rolService.GetAllAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    [RequirePermiso(RolesPermissions.ViewDetail)]
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
    [RequirePermiso(RolesPermissions.Create)]
    public async Task<ActionResult<RolDto>> CreateRol([FromBody] RolCreateDto rolCreateDto)
    {       
        var createdRol = await _rolService.CreateAsync(rolCreateDto);
        return CreatedAtAction(nameof(GetRol), new { id = createdRol.IdRol }, createdRol);
    }

    [HttpPut("{id}")]
    [RequirePermiso(RolesPermissions.Update)]
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
    [RequirePermiso(RolesPermissions.Delete)]
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
    
    /// <summary>
    /// Obtiene los permisos de un rol específico
    /// </summary>
    [HttpGet("{id}/permisos")]
    [RequirePermiso(RolesPermissions.ViewPermisos)]
    public async Task<ActionResult<IEnumerable<PermisoDto>>> GetPermisosRol(Guid id)
    {
        // El servicio ya verifica la existencia del rol y lanza excepciones si es necesario
        var permisos = await _rolService.GetPermisosRolAsync(id);
        return Ok(permisos);
    }
    
    /// <summary>
    /// Asigna permisos a un rol
    /// </summary>
    [HttpPost("{id}/permisos")]
    [RequirePermiso(RolesPermissions.AssignPermisos)]
    public async Task<IActionResult> AsignarPermisosRol(Guid id, [FromBody] RolPermisosDto request)
    {
        // Verificar que el ID coincide
        if (id != request.IdRol)
        {
            throw new BadRequestException("El ID de la URL no coincide con el ID en el cuerpo de la solicitud");
        }

        bool result = false;
        
        // Asignar permisos por ID
        if (request.PermisoIds != null && request.PermisoIds.Count > 0)
        {
            result = await _rolService.AsignarPermisosRolAsync(id, request.PermisoIds);
        }
        
        // Asignar permisos por nombre
        if (request.PermisoNombres != null && request.PermisoNombres.Count > 0)
        {
            result = await _rolService.AsignarPermisosRolByNombreAsync(id, request.PermisoNombres);
        }
        
        if (!result)
        {
            throw new BadRequestException("No se pudieron asignar los permisos al rol");
        }
        
        return NoContent();
    }
    
    /// <summary>
    /// Elimina permisos de un rol
    /// </summary>
    [HttpDelete("{id}/permisos")]
    [RequirePermiso(RolesPermissions.RemovePermisos)]
    public async Task<IActionResult> RemoverPermisosRol(Guid id, [FromBody] RolPermisosDto request)
    {
        // Verificar que el ID coincide
        if (id != request.IdRol)
        {
            throw new BadRequestException("El ID de la URL no coincide con el ID en el cuerpo de la solicitud");
        }

        bool result = false;
        
        // Remover permisos por ID
        if (request.PermisoIds != null && request.PermisoIds.Count > 0)
        {
            result = await _rolService.RemoverPermisosRolAsync(id, request.PermisoIds);
        }
        
        // Remover permisos por nombre
        if (request.PermisoNombres != null && request.PermisoNombres.Count > 0)
        {
            result = await _rolService.RemoverPermisosRolByNombreAsync(id, request.PermisoNombres);
        }
        
        if (!result)
        {
            throw new BadRequestException("No se pudieron remover los permisos del rol");
        }
        
        return NoContent();
    }
}
