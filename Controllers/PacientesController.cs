using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DentalPro.Application.DTOs.Paciente;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Permissions;
using DentalPro.Api.Infrastructure.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

            var pacientesDto = await _pacienteService.GetAllByConsultorioAsync(idConsultorio);
            return Ok(pacientesDto);
        }

        [HttpGet("{id:guid}")]
        [RequirePermiso(PacientesPermissions.ViewDetail)]
        public async Task<ActionResult<PacienteDto>> GetPaciente(Guid id)
        {
            var pacienteDto = await _pacienteService.GetByIdAsync(id);
            
            if (pacienteDto == null)
            {
                throw new NotFoundException("Paciente", id);
            }

            // Verificar que el paciente pertenezca al consultorio del usuario autenticado
            var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
            if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio) || pacienteDto.IdConsultorio != idConsultorio)
            {
                throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
            }

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

            // El servicio gestionará la asignación del consultorio al paciente
            var createdPacienteDto = await _pacienteService.CreateAsync(createDto);
            
            return CreatedAtAction(nameof(GetPaciente), new { id = createdPacienteDto.IdPaciente }, createdPacienteDto);
        }

        [HttpPut("{id:guid}")]
        [RequirePermiso(PacientesPermissions.Update)]
        public async Task<IActionResult> UpdatePaciente(Guid id, PacienteUpdateDto updateDto)
        {
            if (id != updateDto.IdPaciente)
            {
                throw new BadRequestException("El ID de la URL no coincide con el ID en el cuerpo de la solicitud", ErrorCodes.ValidationFailed);
            }

            // Obtener el paciente actual para verificar su existencia y consultorio
            var pacienteDto = await _pacienteService.GetByIdAsync(id);
            if (pacienteDto == null)
            {
                throw new NotFoundException("Paciente", id);
            }

            // Verificar que pertenezca al mismo consultorio que el usuario autenticado
            var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
            if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio) || pacienteDto.IdConsultorio != idConsultorio)
            {
                throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
            }

            // El servicio manejará la actualización
            await _pacienteService.UpdateAsync(updateDto);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [RequirePermiso(PacientesPermissions.Delete)]
        public async Task<IActionResult> DeletePaciente(Guid id)
        {
            var pacienteDto = await _pacienteService.GetByIdAsync(id);
            if (pacienteDto == null)
            {
                throw new NotFoundException("Paciente", id);
            }

            // Verificar que pertenezca al mismo consultorio que el usuario autenticado
            var idConsultorioStr = User.FindFirst("IdConsultorio")?.Value;
            if (string.IsNullOrEmpty(idConsultorioStr) || !Guid.TryParse(idConsultorioStr, out var idConsultorio) || pacienteDto.IdConsultorio != idConsultorio)
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
