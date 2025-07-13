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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio para la gestión de consultorios
/// </summary>
public class ConsultorioService : IConsultorioService
{
    private readonly IConsultorioRepository _consultorioRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<ConsultorioService> _logger;
    private readonly IAuditService _auditService;
    private readonly IConsultorioManagementService _consultorioManagementService;

    public ConsultorioService(
        IConsultorioRepository consultorioRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<ConsultorioService> logger,
        IAuditService auditService,
        IConsultorioManagementService consultorioManagementService)
    {
        _consultorioRepository = consultorioRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
        _auditService = auditService;
        _consultorioManagementService = consultorioManagementService;
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
        return _mapper.Map<IEnumerable<ConsultorioDto>>(consultorios);
    }

    /// <summary>
    /// Obtiene un consultorio por su ID
    /// </summary>
    public async Task<ConsultorioDto?> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("ConsultorioService: Delegando GetByIdAsync al servicio especializado");
            return await _consultorioManagementService.GetByIdAsync(id);
        }
        catch (Exception ex) when (ex is ForbiddenAccessException || ex is NotFoundException)
        {
            // Propagamos las excepciones de seguridad y no encontrado sin modificación
            _logger.LogWarning(ex, "ConsultorioService: Error controlado en GetByIdAsync: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            // Capturamos otras excepciones para logging centralizado y las re-lanzamos
            _logger.LogError(ex, "ConsultorioService: Error no controlado en GetByIdAsync: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Crea un nuevo consultorio
    /// </summary>
    public async Task<ConsultorioDto> CreateAsync(ConsultorioCreateDto consultorioDto)
    {
        // Verificar permiso para crear consultorios usando permisos granulares
        if (!await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.Create))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó crear un consultorio sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        // Mapear DTO a entidad
        var consultorio = _mapper.Map<Consultorio>(consultorioDto);

        var createdConsultorio = await _consultorioRepository.AddAsync(consultorio);
        await _consultorioRepository.SaveChangesAsync();

        var userId = _currentUserService.GetCurrentUserId();

        _logger.LogInformation("Usuario {UserId} creó un nuevo consultorio con ID {ConsultorioId}",
            userId, createdConsultorio.IdConsultorio);

        // Registrar acción en el servicio de auditoría
        await _auditService.RegisterActionAsync(
            "Create",
            "Consultorio",
            createdConsultorio.IdConsultorio,
            userId,
            JsonConvert.SerializeObject(new { consultorio = consultorioDto }));

        // Mapear la entidad creada a DTO y devolverla
        return _mapper.Map<ConsultorioDto>(createdConsultorio);
    }

    /// <summary>
    /// Actualiza un consultorio existente
    /// </summary>
    public async Task<ConsultorioDto> UpdateAsync(ConsultorioUpdateDto consultorioDto)
    {
        // Verificar permiso para actualizar consultorios usando permisos granulares
        if (!await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.Update))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó actualizar un consultorio sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
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

        var existingConsultorio = await _consultorioRepository.GetByIdAsync(consultorioDto.IdConsultorio);
        if (existingConsultorio == null)
        {
            throw new NotFoundException("Consultorio", consultorioDto.IdConsultorio);
        }

        // Mapear el DTO a la entidad existente
        _mapper.Map(consultorioDto, existingConsultorio);

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
    /// Elimina un consultorio por su ID
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        // Verificar permiso para eliminar consultorios usando permisos granulares
        if (!await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.Delete))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó eliminar un consultorio sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        var consultorio = await _consultorioRepository.GetByIdAsync(id);
        if (consultorio == null)
        {
            throw new NotFoundException("Consultorio", id);
        }

        // Aquí podríamos agregar validaciones adicionales como verificar que el consultorio
        // no tenga entidades relacionadas antes de eliminarlo

        await _consultorioRepository.RemoveAsync(consultorio);
        var result = await _consultorioRepository.SaveChangesAsync();

        if (result > 0)
        {
            var userId = _currentUserService.GetCurrentUserId();
            _logger.LogInformation("Usuario {UserId} eliminó el consultorio con ID {ConsultorioId}",
                userId, id);

            // Registrar acción en el servicio de auditoría
            await _auditService.RegisterActionAsync(
                "Delete",
                "Consultorio",
                id,
                userId,
                JsonConvert.SerializeObject(new { id = id }));
        }

        return result > 0;
    }

    /// <summary>
    /// Verifica si existe un consultorio con el ID especificado
    /// </summary>
    /// <param name="id">ID del consultorio a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return false;
        }
        
        var consultorio = await _consultorioRepository.GetByIdAsync(id);
        return consultorio != null;
    }
}
