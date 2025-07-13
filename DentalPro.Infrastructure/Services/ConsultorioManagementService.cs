using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using DentalPro.Application.DTOs.Consultorio;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DentalPro.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio especializado para operaciones CRUD de consultorios
    /// </summary>
    public class ConsultorioManagementService : IConsultorioManagementService
    {
        private readonly IConsultorioRepository _consultorioRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsultorioManagementService> _logger;
        private readonly IAuditService _auditService;

        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public ConsultorioManagementService(
            IConsultorioRepository consultorioRepository,
            ICurrentUserService currentUserService,
            IMapper mapper,
            ILogger<ConsultorioManagementService> logger,
            IAuditService auditService)
        {
            _consultorioRepository = consultorioRepository ?? throw new ArgumentNullException(nameof(consultorioRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        /// <summary>
        /// Obtiene un consultorio por su ID
        /// </summary>
        public async Task<ConsultorioDto?> GetByIdAsync(Guid id)
        {
            // Verificar permiso usando permisos granulares
            if (!await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.ViewDetail))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} no tiene permiso para ver detalles de consultorio",
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            // Verificación explícita de acceso a consultorio
            var hasGlobalAccess = await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.ViewAll);
            if (!hasGlobalAccess)
            {
                var userConsultorioId = _currentUserService.GetCurrentConsultorioId();
                if (id != userConsultorioId)
                {
                    _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó acceder a un consultorio diferente",
                        _currentUserService.GetCurrentUserId());
                    throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
                }
            }

            var consultorio = await _consultorioRepository.GetByIdAsync(id);
            if (consultorio == null)
            {
                _logger.LogWarning("Consultorio con ID {ConsultorioId} no encontrado", id);
                throw new NotFoundException("Consultorio", id);
            }

            var consultorioDto = _mapper.Map<ConsultorioDto>(consultorio);
            
            _logger.LogInformation("Consultorio con ID {ConsultorioId} recuperado correctamente", id);
            return consultorioDto;
        }

        /// <summary>
        /// Obtiene todos los consultorios
        /// </summary>
        public async Task<IEnumerable<ConsultorioDto>> GetAllAsync()
        {
            // Verificar permiso para ver todos los consultorios usando permisos granulares
            if (!await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.ViewAll))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó ver todos los consultorios sin el permiso requerido",
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            var consultorios = await _consultorioRepository.GetAllAsync();
            var consultoriosDto = _mapper.Map<IEnumerable<ConsultorioDto>>(consultorios);
            
            _logger.LogInformation("Usuario {UserId} consultó la lista completa de consultorios", 
                _currentUserService.GetCurrentUserId());
            
            return consultoriosDto;
        }

        /// <summary>
        /// Crea un nuevo consultorio
        /// </summary>
        public async Task<ConsultorioDto> CreateAsync(ConsultorioCreateDto consultorioDto)
        {
            // Verificar permiso para crear consultorios
            if (!await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.Create))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó crear un consultorio sin el permiso requerido",
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            // Validación adicional de datos
            if (consultorioDto == null)
            {
                throw new BadRequestException(ErrorMessages.InvalidId);
            }

            // Mapear DTO a entidad
            var consultorio = _mapper.Map<Consultorio>(consultorioDto);
            
            // Generar nuevo ID si no se proporciona
            if (consultorio.IdConsultorio == Guid.Empty)
            {
                consultorio.IdConsultorio = Guid.NewGuid();
            }

            // Guardar en repositorio
            await _consultorioRepository.AddAsync(consultorio);
            await _consultorioRepository.SaveChangesAsync();

            var userId = _currentUserService.GetCurrentUserId();
            _logger.LogInformation("Usuario {UserId} creó un nuevo consultorio con ID {ConsultorioId}",
                userId, consultorio.IdConsultorio);

            // Registrar acción en el servicio de auditoría
            await _auditService.RegisterActionAsync(
                "Create",
                "Consultorio",
                consultorio.IdConsultorio,
                userId,
                JsonConvert.SerializeObject(new { consultorio = consultorioDto }));

            // Devolver el consultorio creado como DTO
            return _mapper.Map<ConsultorioDto>(consultorio);
        }

        /// <summary>
        /// Actualiza un consultorio existente
        /// </summary>
        public async Task<ConsultorioDto> UpdateAsync(ConsultorioUpdateDto consultorioDto)
        {
            // Verificar permiso para actualizar consultorios
            if (!await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.Update))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó actualizar un consultorio sin el permiso requerido",
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            // Validación adicional de datos
            if (consultorioDto == null || consultorioDto.IdConsultorio == Guid.Empty)
            {
                throw new BadRequestException(ErrorMessages.InvalidId);
            }

            // Verificación explícita de acceso a consultorio
            var hasGlobalAccess = await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.ViewAll);
            if (!hasGlobalAccess)
            {
                var userConsultorioId = _currentUserService.GetCurrentConsultorioId();
                if (consultorioDto.IdConsultorio != userConsultorioId)
                {
                    _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó actualizar un consultorio diferente",
                        _currentUserService.GetCurrentUserId());
                    throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
                }
            }

            // Verificar que el consultorio existe
            var existingConsultorio = await _consultorioRepository.GetByIdAsync(consultorioDto.IdConsultorio);
            if (existingConsultorio == null)
            {
                _logger.LogWarning("Consultorio con ID {ConsultorioId} no encontrado", consultorioDto.IdConsultorio);
                throw new NotFoundException("Consultorio", consultorioDto.IdConsultorio);
            }

            // Mapear DTO a entidad existente (preservando valores que no se actualizan)
            _mapper.Map(consultorioDto, existingConsultorio);
            
            // Guardar cambios
            await _consultorioRepository.UpdateAsync(existingConsultorio);
            await _consultorioRepository.SaveChangesAsync();

            var userId = _currentUserService.GetCurrentUserId();
            _logger.LogInformation("Usuario {UserId} actualizó el consultorio con ID {ConsultorioId}",
                userId, existingConsultorio.IdConsultorio);

            // Registrar acción en el servicio de auditoría
            await _auditService.RegisterActionAsync(
                "Update",
                "Consultorio",
                existingConsultorio.IdConsultorio,
                userId,
                JsonConvert.SerializeObject(new { consultorio = consultorioDto }));

            // Devolver el consultorio actualizado como DTO
            return _mapper.Map<ConsultorioDto>(existingConsultorio);
        }

        /// <summary>
        /// Elimina un consultorio existente
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id)
        {
            // Verificar permiso para eliminar consultorios
            if (!await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.Delete))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó eliminar un consultorio sin el permiso requerido",
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            // Validación del ID
            if (id == Guid.Empty)
            {
                throw new BadRequestException(ErrorMessages.InvalidId);
            }

            // Verificación explícita de acceso a consultorio
            var hasGlobalAccess = await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.ViewAll);
            if (!hasGlobalAccess)
            {
                var userConsultorioId = _currentUserService.GetCurrentConsultorioId();
                if (id != userConsultorioId)
                {
                    _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó eliminar un consultorio diferente",
                        _currentUserService.GetCurrentUserId());
                    throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
                }
            }

            // Verificar que el consultorio existe
            var existingConsultorio = await _consultorioRepository.GetByIdAsync(id);
            if (existingConsultorio == null)
            {
                _logger.LogWarning("Consultorio con ID {ConsultorioId} no encontrado", id);
                throw new NotFoundException("Consultorio", id);
            }

            try
            {
                // Eliminar consultorio usando el método RemoveAsync disponible
                await _consultorioRepository.RemoveAsync(existingConsultorio);
                await _consultorioRepository.SaveChangesAsync();
                
                var userId = _currentUserService.GetCurrentUserId();
                _logger.LogInformation("Usuario {UserId} eliminó el consultorio con ID {ConsultorioId}",
                    userId, id);

                // Registrar acción en el servicio de auditoría
                await _auditService.RegisterActionAsync(
                    "Delete",
                    "Consultorio",
                    id,
                    userId,
                    JsonConvert.SerializeObject(new { id }));
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el consultorio con ID {ConsultorioId}", id);
                return false;
            }
        }

        /// <summary>
        /// Verifica si existe un consultorio con el ID especificado
        /// </summary>
        public async Task<bool> ExistsByIdAsync(Guid id)
        {
            // Validación básica
            if (id == Guid.Empty)
            {
                return false;
            }

            // Este método es utilizado principalmente por validadores, por lo que no
            // requiere permisos especiales para verificar existencia
            // Verificamos existencia consultando el objeto directamente
            var consultorio = await _consultorioRepository.GetByIdAsync(id);
            return consultorio != null;
        }
    }
}
