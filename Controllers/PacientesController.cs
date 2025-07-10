using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DentalPro.Application.DTOs.Paciente;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Permissions;
using DentalPro.Api.Infrastructure.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DentalPro.Application.Interfaces.IServices;

namespace DentalPro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PacientesController : ControllerBase
    {
        private readonly IPacienteService _pacienteService;

        public PacientesController(IPacienteService pacienteService)
        {
            _pacienteService = pacienteService;
        }

        [HttpGet]
        [RequirePermiso(PacientesPermissions.ViewAll)]
        public async Task<ActionResult<IEnumerable<PacienteDto>>> GetPacientes()
        {
            var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
            
            if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio))
            {
                throw new ForbiddenAccessException(ErrorMessages.ConsultorioRequired);
            }

            var pacientes = await _pacienteService.GetAllByConsultorioAsync(idConsultorio);
            
            var pacientesDto = pacientes.Select(p => new PacienteDto
            {
                IdPaciente = p.IdPaciente,
                Nombre = p.Nombre,
                Apellidos = p.Apellidos,
                FechaNacimiento = p.FechaNacimiento,
                Telefono = p.Telefono,
                Correo = p.Correo,
                FechaAlta = p.FechaAlta,
                IdConsultorio = p.IdConsultorio
            }).ToList();

            return Ok(pacientesDto);
        }

        [HttpGet("{id:guid}")]
        [RequirePermiso(PacientesPermissions.ViewDetail)]
        public async Task<ActionResult<PacienteDto>> GetPaciente(Guid id)
        {
            var paciente = await _pacienteService.GetByIdAsync(id);
            
            if (paciente == null)
            {
                throw new NotFoundException("Paciente", id);
            }

            // Verificar que el paciente pertenezca al consultorio del usuario autenticado
            var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
            if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio) || paciente.IdConsultorio != idConsultorio)
            {
                throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
            }

            var pacienteDto = new PacienteDto
            {
                IdPaciente = paciente.IdPaciente,
                Nombre = paciente.Nombre,
                Apellidos = paciente.Apellidos,
                FechaNacimiento = paciente.FechaNacimiento,
                Telefono = paciente.Telefono,
                Correo = paciente.Correo,
                FechaAlta = paciente.FechaAlta,
                IdConsultorio = paciente.IdConsultorio
            };

            return Ok(pacienteDto);
        }

        [HttpPost]
        [RequirePermiso(PacientesPermissions.Create)]
        public async Task<ActionResult<PacienteDto>> CreatePaciente(PacienteCreateDto createDto)
        {
            var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
            
            if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio))
            {
                throw new ForbiddenAccessException(ErrorMessages.ConsultorioRequired);
            }

            var paciente = new Paciente
            {
                Nombre = createDto.Nombre,
                Apellidos = createDto.Apellidos,
                FechaNacimiento = createDto.FechaNacimiento,
                Telefono = createDto.Telefono,
                Correo = createDto.Correo,
                IdConsultorio = idConsultorio
            };

            var createdPaciente = await _pacienteService.CreateAsync(paciente);

            var pacienteDto = new PacienteDto
            {
                IdPaciente = createdPaciente.IdPaciente,
                Nombre = createdPaciente.Nombre,
                Apellidos = createdPaciente.Apellidos,
                FechaNacimiento = createdPaciente.FechaNacimiento,
                Telefono = createdPaciente.Telefono,
                Correo = createdPaciente.Correo,
                FechaAlta = createdPaciente.FechaAlta,
                IdConsultorio = createdPaciente.IdConsultorio
            };

            return CreatedAtAction(nameof(GetPaciente), new { id = pacienteDto.IdPaciente }, pacienteDto);
        }

        [HttpPut("{id:guid}")]
        [RequirePermiso(PacientesPermissions.Update)]
        public async Task<IActionResult> UpdatePaciente(Guid id, PacienteUpdateDto updateDto)
        {
            if (id != updateDto.IdPaciente)
            {
                throw new BadRequestException("El ID de la URL no coincide con el ID en el cuerpo de la solicitud", ErrorCodes.ValidationFailed);
            }

            // Obtener el paciente actual
            var paciente = await _pacienteService.GetByIdAsync(id);
            if (paciente == null)
            {
                throw new NotFoundException("Paciente", id);
            }

            // Verificar que pertenezca al mismo consultorio que el usuario autenticado
            var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
            if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio) || paciente.IdConsultorio != idConsultorio)
            {
                throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
            }

            // Actualizar propiedades
            paciente.Nombre = updateDto.Nombre;
            paciente.Apellidos = updateDto.Apellidos;
            paciente.FechaNacimiento = updateDto.FechaNacimiento;
            paciente.Telefono = updateDto.Telefono;
            paciente.Correo = updateDto.Correo;
            
            // Actualizar el paciente
            await _pacienteService.UpdateAsync(paciente);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [RequirePermiso(PacientesPermissions.Delete)]
        public async Task<IActionResult> DeletePaciente(Guid id)
        {
            var paciente = await _pacienteService.GetByIdAsync(id);
            if (paciente == null)
            {
                throw new NotFoundException("Paciente", id);
            }

            // Verificar que pertenezca al mismo consultorio que el usuario autenticado
            var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
            if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio) || paciente.IdConsultorio != idConsultorio)
            {
                throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
            }

            var result = await _pacienteService.DeleteAsync(id);
            if (!result)
            {
                throw new BadRequestException("No se pudo eliminar el paciente", ErrorCodes.InvalidOperation);
            }

            return NoContent();
        }
    }
}
