using Microsoft.AspNetCore.Mvc;
using DentalPro.Application.DTOs.Consultorio;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Api.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DentalPro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequireConsultorioAccess]
    public class ConsultoriosController : ControllerBase
    {
        private readonly IConsultorioService _consultorioService;
        private readonly ILogger<ConsultoriosController> _logger;

        public ConsultoriosController(IConsultorioService consultorioService, ILogger<ConsultoriosController> logger)
        {
            _consultorioService = consultorioService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los consultorios
        /// </summary>
        [HttpGet]
        [RequirePermiso(ConsultoriosPermissions.ViewAll)]
        public async Task<ActionResult<IEnumerable<ConsultorioDto>>> GetAll()
        {
            var consultorios = await _consultorioService.GetAllAsync();
            return Ok(consultorios);
        }

        /// <summary>
        /// Obtiene un consultorio por su ID
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermiso(ConsultoriosPermissions.ViewDetail)]
        public async Task<ActionResult<ConsultorioDto>> GetById(Guid id)
        {
            var consultorio = await _consultorioService.GetByIdAsync(id);
            if (consultorio == null)
            {
                throw new NotFoundException("Consultorio", id);
            }
            return Ok(consultorio);
        }

        /// <summary>
        /// Crea un nuevo consultorio
        /// </summary>
        [HttpPost]
        [RequirePermiso(ConsultoriosPermissions.Create)]
        public async Task<ActionResult<ConsultorioDto>> Create(ConsultorioCreateDto consultorioDto)
        {
            var createdConsultorio = await _consultorioService.CreateAsync(consultorioDto);
            return CreatedAtAction(
                nameof(GetById),
                new { id = createdConsultorio.IdConsultorio },
                createdConsultorio);
        }

        /// <summary>
        /// Actualiza un consultorio existente
        /// </summary>
        [HttpPut]
        [RequirePermiso(ConsultoriosPermissions.Update)]
        public async Task<ActionResult<ConsultorioDto>> Update(ConsultorioUpdateDto consultorioDto)
        {
            var updatedConsultorio = await _consultorioService.UpdateAsync(consultorioDto);
            return Ok(updatedConsultorio);
        }

        /// <summary>
        /// Elimina un consultorio por su ID
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermiso(ConsultoriosPermissions.Delete)]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _consultorioService.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Verifica si existe un consultorio con el ID especificado
        /// </summary>
        [HttpGet("exists/{id}")]
        [RequirePermiso(ConsultoriosPermissions.ViewDetail)]
        public async Task<ActionResult<bool>> Exists(Guid id)
        {
            var exists = await _consultorioService.ExistsByIdAsync(id);
            return Ok(exists);
        }

        #region Gestión de Personal

        /// <summary>
        /// Obtiene todos los doctores asociados a un consultorio específico
        /// </summary>
        /// <param name="consultorioId">ID del consultorio</param>
        [HttpGet("{consultorioId}/doctores")]
        [RequirePermiso(ConsultoriosPermissions.ViewDoctores)]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetDoctoresByConsultorio(Guid consultorioId)
        {
            try
            {
                _logger.LogInformation("ConsultoriosController: Obteniendo doctores del consultorio {ConsultorioId}", consultorioId);
                var doctores = await _consultorioService.GetDoctoresByConsultorioAsync(consultorioId);
                return Ok(doctores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener doctores del consultorio {ConsultorioId}: {Message}", consultorioId, ex.Message);
                throw; // Las excepciones serán manejadas por el middleware global
            }
        }

        /// <summary>
        /// Obtiene todos los asistentes asociados a un consultorio específico
        /// </summary>
        /// <param name="consultorioId">ID del consultorio</param>
        [HttpGet("{consultorioId}/asistentes")]
        [RequirePermiso(ConsultoriosPermissions.ViewAsistentes)]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetAsistentesByConsultorio(Guid consultorioId)
        {
            try
            {
                _logger.LogInformation("ConsultoriosController: Obteniendo asistentes del consultorio {ConsultorioId}", consultorioId);
                var asistentes = await _consultorioService.GetAsistentesByConsultorioAsync(consultorioId);
                return Ok(asistentes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener asistentes del consultorio {ConsultorioId}: {Message}", consultorioId, ex.Message);
                throw; // Las excepciones serán manejadas por el middleware global
            }
        }

        /// <summary>
        /// Asigna un doctor a un consultorio
        /// </summary>
        /// <param name="usuarioId">ID del usuario doctor</param>
        /// <param name="consultorioId">ID del consultorio</param>
        [HttpPost("doctor/{usuarioId}/{consultorioId}")]
        [RequirePermiso(ConsultoriosPermissions.AssignStaff)]
        public async Task<ActionResult<bool>> AsignarDoctor(Guid usuarioId, Guid consultorioId)
        {
            try
            {
                _logger.LogInformation("ConsultoriosController: Asignando doctor {UsuarioId} al consultorio {ConsultorioId}", 
                    usuarioId, consultorioId);
                var result = await _consultorioService.AsignarDoctorAsync(usuarioId, consultorioId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar doctor {UsuarioId} al consultorio {ConsultorioId}: {Message}", 
                    usuarioId, consultorioId, ex.Message);
                throw; // Las excepciones serán manejadas por el middleware global
            }
        }

        /// <summary>
        /// Asigna un asistente a un consultorio
        /// </summary>
        /// <param name="usuarioId">ID del usuario asistente</param>
        /// <param name="consultorioId">ID del consultorio</param>
        [HttpPost("asistente/{usuarioId}/{consultorioId}")]
        [RequirePermiso(ConsultoriosPermissions.AssignStaff)]
        public async Task<ActionResult<bool>> AsignarAsistente(Guid usuarioId, Guid consultorioId)
        {
            try
            {
                _logger.LogInformation("ConsultoriosController: Asignando asistente {UsuarioId} al consultorio {ConsultorioId}", 
                    usuarioId, consultorioId);
                var result = await _consultorioService.AsignarAsistenteAsync(usuarioId, consultorioId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar asistente {UsuarioId} al consultorio {ConsultorioId}: {Message}", 
                    usuarioId, consultorioId, ex.Message);
                throw; // Las excepciones serán manejadas por el middleware global
            }
        }

        /// <summary>
        /// Desvincula un miembro del personal de un consultorio
        /// </summary>
        /// <param name="usuarioId">ID del usuario a desvincular</param>
        /// <param name="consultorioId">ID del consultorio</param>
        [HttpPost("desvincular/{usuarioId}/{consultorioId}")]
        [RequirePermiso(ConsultoriosPermissions.RemoveStaff)]
        public async Task<ActionResult<bool>> DesvincularMiembro(Guid usuarioId, Guid consultorioId)
        {
            try
            {
                _logger.LogInformation("ConsultoriosController: Desvinculando miembro {UsuarioId} del consultorio {ConsultorioId}", 
                    usuarioId, consultorioId);
                var result = await _consultorioService.DesvincularMiembroAsync(usuarioId, consultorioId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desvincular miembro {UsuarioId} del consultorio {ConsultorioId}: {Message}", 
                    usuarioId, consultorioId, ex.Message);
                throw; // Las excepciones serán manejadas por el middleware global
            }
        }

        #endregion
    }
}
