using AutoMapper;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using DentalPro.Application.DTOs.Consultorio;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Logging;
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

    public ConsultorioService(
        IConsultorioRepository consultorioRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<ConsultorioService> logger)
    {
        _consultorioRepository = consultorioRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los consultorios
    /// </summary>
    public async Task<IEnumerable<ConsultorioDto>> GetAllAsync()
    {
        // Verificar permiso para ver todos los consultorios (permiso de administración)
        if (!await _currentUserService.IsInRoleAsync("Admin"))
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
        // Verificar permiso o rol necesario
        if (!await _currentUserService.IsInRoleAsync("Admin"))
        {
            // Si no es administrador, verificar si el consultorio es el mismo al que pertenece
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
            return null;
        }

        return _mapper.Map<ConsultorioDto>(consultorio);
    }

    /// <summary>
    /// Crea un nuevo consultorio
    /// </summary>
    public async Task<ConsultorioDto> CreateAsync(ConsultorioCreateDto consultorioDto)
    {
        // Verificar permiso para crear consultorios (sólo administradores)
        if (!await _currentUserService.IsInRoleAsync("Admin"))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó crear un consultorio sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        // Mapear DTO a entidad
        var consultorio = _mapper.Map<Consultorio>(consultorioDto);
        
        var createdConsultorio = await _consultorioRepository.AddAsync(consultorio);
        await _consultorioRepository.SaveChangesAsync();
        
        _logger.LogInformation("Usuario {UserId} creó un nuevo consultorio con ID {ConsultorioId}", 
            _currentUserService.GetCurrentUserId(), createdConsultorio.IdConsultorio);
        
        // Mapear la entidad creada a DTO y devolverla
        return _mapper.Map<ConsultorioDto>(createdConsultorio);
    }

    /// <summary>
    /// Actualiza un consultorio existente
    /// </summary>
    public async Task<ConsultorioDto> UpdateAsync(ConsultorioUpdateDto consultorioDto)
    {
        // Verificar permiso para actualizar consultorios (sólo administradores)
        if (!await _currentUserService.IsInRoleAsync("Admin"))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó actualizar un consultorio sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
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
        
        _logger.LogInformation("Usuario {UserId} actualizó el consultorio con ID {ConsultorioId}", 
            _currentUserService.GetCurrentUserId(), existingConsultorio.IdConsultorio);
                
        // Devolver el consultorio actualizado como DTO
        return _mapper.Map<ConsultorioDto>(existingConsultorio);
    }

    /// <summary>
    /// Elimina un consultorio por su ID
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        // Verificar permiso para eliminar consultorios (sólo administradores)
        if (!await _currentUserService.IsInRoleAsync("Admin"))
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
        
        _logger.LogInformation("Usuario {UserId} eliminó el consultorio con ID {ConsultorioId}", 
            _currentUserService.GetCurrentUserId(), id);
            
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
