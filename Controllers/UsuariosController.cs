using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.Interfaces;
using DentalPro.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
    {
        // Obtener el ID del consultorio del token JWT
        var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
        if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio))
        {
            return BadRequest("ID de consultorio no válido");
        }

        var usuarios = await _usuarioService.GetAllByConsultorioAsync(idConsultorio);
        
        var usuariosDto = usuarios.Select(u => new UsuarioDto
        {
            IdUsuario = u.IdUsuario,
            Nombre = u.Nombre,
            Correo = u.Correo,
            Activo = u.Activo,
            IdConsultorio = u.IdConsultorio,
            Roles = u.Roles.Select(r => r.Rol?.Nombre ?? string.Empty)
                           .Where(r => !string.IsNullOrEmpty(r))
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
            return NotFound();
        }

        // Verificar que el usuario pertenezca al mismo consultorio que el usuario autenticado
        var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
        if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio) || usuario.IdConsultorio != idConsultorio)
        {
            return Forbid();
        }

        // Convertir a DTO
        var usuarioDto = new UsuarioDto
        {
            IdUsuario = usuario.IdUsuario,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            Activo = usuario.Activo,
            IdConsultorio = usuario.IdConsultorio,
            Roles = usuario.Roles.Select(r => r.Rol?.Nombre ?? string.Empty)
                             .Where(r => !string.IsNullOrEmpty(r))
                             .ToList()
        };
        
        return Ok(usuarioDto);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<UsuarioDto>> CreateUsuario([FromBody] CreateUsuarioRequest request)
    {
        try
        {
            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest("Las contraseñas no coinciden");
            }

            // Obtener el ID del consultorio del token JWT
            var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
            if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio))
            {
                return BadRequest("ID de consultorio no válido");
            }

            // Validar que el consultorio del request coincida con el del usuario autenticado
            if (request.IdConsultorio != idConsultorio)
            {
                return BadRequest("No puede crear usuarios para otros consultorios");
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

            var createdUsuario = await _usuarioService.CreateAsync(usuario, request.Password, request.Roles);

            // Convertir a DTO para la respuesta
            var usuarioDto = new UsuarioDto
            {
                IdUsuario = createdUsuario.IdUsuario,
                Nombre = createdUsuario.Nombre,
                Correo = createdUsuario.Correo,
                Activo = createdUsuario.Activo,
                IdConsultorio = createdUsuario.IdConsultorio,
                Roles = createdUsuario.Roles.Select(r => r.Rol?.Nombre ?? string.Empty)
                                     .Where(r => !string.IsNullOrEmpty(r))
                                     .ToList()
            };

            return CreatedAtAction(nameof(GetUsuario), new { id = usuarioDto.IdUsuario }, usuarioDto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdateUsuario(Guid id, [FromBody] UpdateUsuarioRequest request)
    {
        try
        {
            if (id != request.IdUsuario)
            {
                return BadRequest("El ID de la URL no coincide con el ID en el cuerpo de la solicitud");
            }

            // Obtener el usuario actual
            var usuario = await _usuarioService.GetByIdAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // Verificar que pertenezca al mismo consultorio que el usuario autenticado
            var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
            if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio) || usuario.IdConsultorio != idConsultorio)
            {
                return Forbid();
            }

            // Actualizar propiedades
            usuario.Nombre = request.Nombre;
            usuario.Correo = request.Correo;
            usuario.Activo = request.Activo;
            
            // Actualizar el usuario
            var result = await _usuarioService.UpdateAsync(usuario);
            if (!result)
            {
                return BadRequest("No se pudo actualizar el usuario");
            }

            // Actualizar roles si es necesario
            // Primero obtenemos los roles actuales
            var currentRoles = usuario.Roles.Select(r => r.Rol?.Nombre ?? string.Empty)
                                           .Where(r => !string.IsNullOrEmpty(r))
                                           .ToList();

            // Roles a añadir (están en request.Roles pero no en currentRoles)
            var rolesToAdd = request.Roles.Except(currentRoles).ToList();
            
            // Roles a eliminar (están en currentRoles pero no en request.Roles)
            var rolesToRemove = currentRoles.Except(request.Roles).ToList();

            // Añadir nuevos roles
            foreach (var rol in rolesToAdd)
            {
                await _usuarioService.AsignarRolAsync(id, rol);
            }

            // Eliminar roles
            foreach (var rol in rolesToRemove)
            {
                await _usuarioService.RemoverRolAsync(id, rol);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteUsuario(Guid id)
    {
        try
        {
            // Verificar que el usuario existe
            var usuario = await _usuarioService.GetByIdAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // Verificar que pertenece al mismo consultorio que el usuario autenticado
            var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
            if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio) || usuario.IdConsultorio != idConsultorio)
            {
                return Forbid();
            }

            // Eliminar el usuario
            var result = await _usuarioService.DeleteAsync(id);
            if (!result)
            {
                return BadRequest("No se pudo eliminar el usuario");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/cambiar-password")]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request)
    {
        try
        {
            // Verificar que el ID coincide
            if (id != request.IdUsuario)
            {
                return BadRequest("El ID de la URL no coincide con el ID en el cuerpo de la solicitud");
            }

            // Verificar que el usuario existe
            var usuario = await _usuarioService.GetByIdAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // Verificar que pertenece al mismo consultorio que el usuario autenticado
            var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
            if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio) || usuario.IdConsultorio != idConsultorio)
            {
                return Forbid();
            }

            // Verificar que la nueva contraseña y la confirmación coinciden
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return BadRequest("La nueva contraseña y su confirmación no coinciden");
            }

            // Cambiar la contraseña
            var result = await _usuarioService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);
            if (!result)
            {
                return BadRequest("No se pudo cambiar la contraseña");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
