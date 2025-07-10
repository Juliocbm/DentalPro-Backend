using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using DentalPro.Application.DTOs.Permiso;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Api.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DentalPro.Api.Controllers;

/// <summary>
/// Controlador para la gestión de permisos del sistema
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PermisosController : ControllerBase
{
    private readonly IPermisoService _permisoService;
    private readonly IUsuarioService _usuarioService;

    public PermisosController(
        IPermisoService permisoService,
        IUsuarioService usuarioService)
    {
        _permisoService = permisoService;
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Obtiene todos los permisos disponibles en el sistema
    /// </summary>
    /// <returns>Lista de todos los permisos disponibles</returns>
    /// <response code="200">Devuelve la lista de permisos</response>
    /// <response code="403">El usuario no tiene permisos para ver todos los permisos</response>
    [HttpGet]
    [RequirePermiso(PermisosPermissions.ViewAll)]
    public async Task<ActionResult<IEnumerable<PermisoDto>>> GetPermisos()
    {
        var permisos = await _permisoService.GetAllPermisosAsync();
        return Ok(permisos);
    }

    /// <summary>
    /// Obtiene un permiso por su ID
    /// </summary>
    /// <param name="id">ID del permiso a consultar</param>
    /// <returns>Detalles del permiso solicitado</returns>
    /// <response code="200">Devuelve el permiso solicitado</response>
    /// <response code="404">El permiso no existe</response>
    /// <response code="403">El usuario no tiene permisos para ver los detalles de un permiso</response>
    [HttpGet("{id}")]
    [RequirePermiso(PermisosPermissions.ViewDetail)]
    public async Task<ActionResult<PermisoDetailDto>> GetPermiso(Guid id)
    {
        var permiso = await _permisoService.GetPermisoByIdAsync(id);
        if (permiso == null)
        {
            throw new NotFoundException("Permiso", id);
        }
        
        return Ok(permiso);
    }

    /// <summary>
    /// Obtiene permisos asignados a un usuario específico
    /// </summary>
    /// <param name="idUsuario">ID del usuario cuyos permisos se desean consultar</param>
    /// <returns>Lista de permisos asignados al usuario</returns>
    /// <response code="200">Devuelve los permisos del usuario</response>
    /// <response code="404">El usuario no existe</response>
    /// <response code="403">No tiene permisos para ver los permisos de usuarios</response>
    [HttpGet("usuario/{idUsuario}")]
    [RequirePermiso(UsuariosPermissions.ViewPermisos)]
    public async Task<ActionResult<IEnumerable<PermisoDto>>> GetPermisosUsuario(Guid idUsuario)
    {
        // Verificar que el usuario existe
        if (!await _usuarioService.ExistsByIdAsync(idUsuario))
        {
            throw new NotFoundException("Usuario", idUsuario);
        }
        
        var permisos = await _usuarioService.GetPermisosUsuarioAsync(idUsuario);
        return Ok(permisos);
    }
    
    /// <summary>
    /// Asigna permisos directamente a un usuario
    /// </summary>
    /// <param name="idUsuario">ID del usuario al que se asignarán permisos</param>
    /// <param name="request">Datos de los permisos a asignar</param>
    /// <returns>Resultado de la operación</returns>
    /// <response code="204">Permisos asignados correctamente</response>
    /// <response code="400">Solicitud inválida o error al asignar permisos</response>
    /// <response code="404">El usuario o algún permiso no existe</response>
    /// <response code="403">No tiene permisos para asignar permisos a usuarios</response>
    [HttpPost("usuario/{idUsuario}/asignar")]
    [RequirePermiso(PermisosPermissions.AssignToUsers)]
    public async Task<IActionResult> AsignarPermisosUsuario(Guid idUsuario, [FromBody] DentalPro.Application.DTOs.Usuario.UsuarioPermisosDto request)
    {
        // Verificar que el ID coincide
        if (idUsuario != request.IdUsuario)
        {
            throw new BadRequestException("El ID de la URL no coincide con el ID en el cuerpo de la solicitud");
        }
        
        bool result = false;
        
        // Asignar permisos por ID
        if (request.PermisoIds != null && request.PermisoIds.Count > 0)
        {
            result = await _usuarioService.AsignarPermisosUsuarioAsync(idUsuario, request.PermisoIds);
        }
        
        // Asignar permisos por nombre
        if (request.PermisoNombres != null && request.PermisoNombres.Count > 0)
        {
            result = await _usuarioService.AsignarPermisosUsuarioByNombreAsync(idUsuario, request.PermisoNombres);
        }
        
        if (!result)
        {
            throw new BadRequestException("No se pudieron asignar los permisos al usuario");
        }
        
        return NoContent();
    }
    
    /// <summary>
    /// Remueve permisos directamente de un usuario
    /// </summary>
    /// <param name="idUsuario">ID del usuario del que se removerán permisos</param>
    /// <param name="request">Datos de los permisos a remover</param>
    /// <returns>Resultado de la operación</returns>
    /// <response code="204">Permisos removidos correctamente</response>
    /// <response code="400">Solicitud inválida o error al remover permisos</response>
    /// <response code="404">El usuario o algún permiso no existe</response>
    /// <response code="403">No tiene permisos para remover permisos a usuarios</response>
    [HttpPost("usuario/{idUsuario}/remover")]
    [RequirePermiso(PermisosPermissions.RemoveFromUsers)]
    public async Task<IActionResult> RemoverPermisosUsuario(Guid idUsuario, [FromBody] DentalPro.Application.DTOs.Usuario.UsuarioPermisosDto request)
    {
        // Verificar que el ID coincide
        if (idUsuario != request.IdUsuario)
        {
            throw new BadRequestException("El ID de la URL no coincide con el ID en el cuerpo de la solicitud");
        }
        
        bool result = false;
        
        // Remover permisos por ID
        if (request.PermisoIds != null && request.PermisoIds.Count > 0)
        {
            result = await _usuarioService.RemoverPermisosUsuarioAsync(idUsuario, request.PermisoIds);
        }
        
        // Remover permisos por nombre
        if (request.PermisoNombres != null && request.PermisoNombres.Count > 0)
        {
            result = await _usuarioService.RemoverPermisosUsuarioByNombreAsync(idUsuario, request.PermisoNombres);
        }
        
        if (!result)
        {
            throw new BadRequestException("No se pudieron remover los permisos del usuario");
        }
        
        return NoContent();
    }
}
