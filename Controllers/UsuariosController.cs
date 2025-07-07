using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.DTOs.Rol;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.Interfaces;
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

        var usuarios = await _usuarioService.GetAllByConsultorioAsync(idConsultorio);
        
        var usuariosDto = usuarios.Select(u => new UsuarioDto
        {
            IdUsuario = u.IdUsuario,
            Nombre = u.Nombre,
            Correo = u.Correo,
            Activo = u.Activo,
            IdConsultorio = u.IdConsultorio,
            Roles = u.Roles
                .Where(r => r.Rol != null)
                .Select(r => new RolInfoDto
                {
                    Id = r.IdRol,
                    Nombre = r.Rol!.Nombre
                })
                .ToList()
        }).ToList();
        
        return Ok(usuariosDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioDto>> GetUsuario(Guid id)
    {
        var usuario = await _usuarioService.GetByIdAsync(id);
        if (usuario == null)
        {
            throw new NotFoundException("Usuario", id);
        }

        // Verificar que el usuario pertenezca al mismo consultorio que el usuario autenticado
        var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
        if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio) || usuario.IdConsultorio != idConsultorio)
        {
            throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
        }

        // Convertir a DTO
        var usuarioDto = new UsuarioDto
        {
            IdUsuario = usuario.IdUsuario,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            Activo = usuario.Activo,
            IdConsultorio = usuario.IdConsultorio,
            Roles = usuario.Roles
                .Where(r => r.Rol != null)
                .Select(r => new RolInfoDto
                {
                    Id = r.IdRol,
                    Nombre = r.Rol!.Nombre
                })
                .ToList()
        };
        
        return Ok(usuarioDto);
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<UsuarioDto>> CreateUsuario([FromBody] CreateUsuarioRequest request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new BadRequestException(ErrorMessages.PasswordMismatch);
        }

        // Obtener el ID del consultorio del token JWT
        var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
        if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio))
        {
            throw new BadRequestException("ID de consultorio no válido");
        }

        // Validar que el consultorio del request coincida con el del usuario autenticado
        if (request.IdConsultorio != idConsultorio)
        {
            throw new ForbiddenAccessException("No puede crear usuarios para otros consultorios");
        }

        // Crear el usuario
        var usuario = new Usuario
        {
            IdUsuario = Guid.NewGuid(),
            Nombre = request.Nombre,
            Correo = request.Correo,
            Activo = true,
            IdConsultorio = request.IdConsultorio
        };

        var createdUsuario = await _usuarioService.CreateAsyncWithRolIds(usuario, request.Password, request.RolIds);

        var usuarioDto = new UsuarioDto
        {
            IdUsuario = createdUsuario.IdUsuario,
            Nombre = createdUsuario.Nombre,
            Correo = createdUsuario.Correo,
            Activo = createdUsuario.Activo,
            IdConsultorio = createdUsuario.IdConsultorio,
            Roles = createdUsuario.Roles
                .Where(r => r.Rol != null)
                .Select(r => new RolInfoDto
                {
                    Id = r.IdRol,
                    Nombre = r.Rol!.Nombre
                })
                .ToList()
        };

        return CreatedAtAction(nameof(GetUsuario), new { id = usuarioDto.IdUsuario }, usuarioDto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<UsuarioDto>> UpdateUsuario(Guid id, [FromBody] UpdateUsuarioRequest request)
    {
        if (id != request.IdUsuario)
        {
            throw new BadRequestException("El ID de la URL no coincide con el ID en el cuerpo de la solicitud", ErrorCodes.ValidationFailed);
        }

        // Obtener el usuario actual
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

            // Actualizar propiedades
            usuario.Nombre = request.Nombre;
            usuario.Correo = request.Correo;
            usuario.Activo = request.Activo;
            
            // Actualizar el usuario
            await _usuarioService.UpdateAsync(usuario);

            // Actualizar roles si es necesario
            // Primero obtenemos los roles actuales como IDs
            var currentRolIds = usuario.Roles.Select(r => r.IdRol)
                                            .ToList();

            // Roles a añadir (están en request.RolIds pero no en currentRolIds)
            var rolesToAdd = request.RolIds.Except(currentRolIds).ToList();
            
            // Roles a eliminar (están en currentRolIds pero no en request.RolIds)
            var rolesToRemove = currentRolIds.Except(request.RolIds).ToList();

            // Añadir nuevos roles
            foreach (var rolId in rolesToAdd)
            {
                await _usuarioService.AsignarRolPorIdAsync(id, rolId);
            }

            // Eliminar roles
            foreach (var rolId in rolesToRemove)
            {
                await _usuarioService.RemoverRolPorIdAsync(id, rolId);
            }
        
            // Obtener el usuario actualizado para devolverlo en la respuesta
            var updatedUsuario = await _usuarioService.GetByIdAsync(id);
            if (updatedUsuario == null)
            {
                throw new NotFoundException("Usuario", id);
            }
        
            // Convertir a DTO
            var usuarioDto = new UsuarioDto
            {
                IdUsuario = updatedUsuario.IdUsuario,
                Nombre = updatedUsuario.Nombre,
                Correo = updatedUsuario.Correo,
                Activo = updatedUsuario.Activo,
                IdConsultorio = updatedUsuario.IdConsultorio,
                Roles = updatedUsuario.Roles
                    .Where(r => r.Rol != null)
                    .Select(r => new RolInfoDto
                    {
                        Id = r.IdRol,
                        Nombre = r.Rol!.Nombre
                    })
                    .ToList()
            };
        
            return Ok(usuarioDto);
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
