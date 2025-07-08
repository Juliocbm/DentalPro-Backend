using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.DTOs.Rol;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DentalPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireAuthenticatedUser")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
    {
        // Obtener el ID del consultorio del token JWT
        var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
        if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio))
        {
            throw new BadRequestException("ID de consultorio no válido", ErrorCodes.InvalidConsultorio);
        }

        // El servicio ya devuelve DTOs mapeados
        var usuariosDto = await _usuarioService.GetAllByConsultorioAsync(idConsultorio);
        
        return Ok(usuariosDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioDto>> GetUsuario(Guid id)
    {
        var usuarioDto = await _usuarioService.GetByIdAsync(id);
        if (usuarioDto == null)
        {
            throw new NotFoundException("Usuario", id);
        }

        // Verificar que el usuario pertenezca al mismo consultorio que el usuario autenticado
        var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
        if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio) || usuarioDto.IdConsultorio != idConsultorio)
        {
            throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
        }

        // El DTO ya viene mapeado del servicio
        return Ok(usuarioDto);
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<UsuarioDto>> CreateUsuario([FromBody] UsuarioCreateDto usuarioCreateDto)
    {
        // Obtener el ID del consultorio del token JWT
        var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
        if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio))
        {
            throw new BadRequestException("ID de consultorio no válido", ErrorCodes.InvalidConsultorio);
        }

        // Validar que el consultorio del request coincida con el del usuario autenticado
        if (usuarioCreateDto.IdConsultorio != idConsultorio)
        {
            throw new ForbiddenAccessException("No puede crear usuarios para otros consultorios");
        }

        // La validación de la contraseña ahora se hace en el validador UsuarioCreateDtoValidator

        // Crear el usuario utilizando el nuevo método con DTO
        var createdUsuario = await _usuarioService.CreateAsync(usuarioCreateDto);

        // El DTO ya viene mapeado desde el servicio
        return CreatedAtAction(nameof(GetUsuario), new { id = createdUsuario.IdUsuario }, createdUsuario);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<UsuarioDto>> UpdateUsuario(Guid id, [FromBody] UsuarioUpdateDto usuarioUpdateDto)
    {
        if (id != usuarioUpdateDto.IdUsuario)
        {
            throw new BadRequestException("El ID de la URL no coincide con el ID en el cuerpo de la solicitud", ErrorCodes.ValidationFailed);
        }

        // Obtener el usuario actual para verificar su consultorio
        var usuario = await _usuarioService.GetByIdAsync(id);
        if (usuario == null)
        {
            throw new NotFoundException("Usuario", id);
        }

        // Verificar que pertenezca al mismo consultorio que el usuario autenticado
        var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
        if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio) || usuario.IdConsultorio != idConsultorio)
        {
            throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
        }
        
        // Utilizar el método actualizado que maneja los nuevos DTOs
        // Este método ya incluye la actualización de roles y validaciones
        var updatedUsuario = await _usuarioService.UpdateAsync(usuarioUpdateDto);
        
        return Ok(updatedUsuario);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> DeleteUsuario(Guid id)
    {
        // Verificar que el usuario existe
        var usuario = await _usuarioService.GetByIdAsync(id);
        if (usuario == null)
        {
            throw new NotFoundException("Usuario", id);
        }

        // Verificar que pertenece al mismo consultorio que el usuario autenticado
        var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
        if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio) || usuario.IdConsultorio != idConsultorio)
        {
            throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
        }

        // Eliminar el usuario
        var result = await _usuarioService.DeleteAsync(id);
        if (!result)
        {
            throw new BadRequestException("No se pudo eliminar el usuario");
        }

        return NoContent();
    }

    [HttpPost("{id}/cambiar-password")]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request)
    {
        // Verificar que el ID coincide
        if (id != request.IdUsuario)
        {
            throw new BadRequestException("El ID de la URL no coincide con el ID en el cuerpo de la solicitud");
        }

        // Verificar que el usuario existe
        var usuario = await _usuarioService.GetByIdAsync(id);
        if (usuario == null)
        {
            throw new NotFoundException("Usuario", id);
        }

        // Verificar que pertenece al mismo consultorio que el usuario autenticado
        var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
        if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio) || usuario.IdConsultorio != idConsultorio)
        {
            throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
        }

        // Verificar que la nueva contraseña y la confirmación coinciden
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            throw new BadRequestException(ErrorMessages.PasswordMismatch);
        }

        // Cambiar la contraseña
        var result = await _usuarioService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);
        if (!result)
        {
            throw new BadRequestException("No se pudo cambiar la contraseña");
        }

        return NoContent();
    }
}
