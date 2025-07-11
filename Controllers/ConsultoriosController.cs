using Microsoft.AspNetCore.Mvc;
using DentalPro.Application.DTOs.Consultorio;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Api.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DentalPro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequireConsultorioAccess]
    public class ConsultoriosController : ControllerBase
    {
        private readonly IConsultorioService _consultorioService;

        public ConsultoriosController(IConsultorioService consultorioService)
        {
            _consultorioService = consultorioService;
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
    }
}
