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
        try
        {
            _logger.LogInformation("ConsultorioService: Delegando GetAllAsync al servicio especializado");
            return await _consultorioManagementService.GetAllAsync();
        }
        catch (Exception ex) when (ex is ForbiddenAccessException)
        {
            // Propagamos las excepciones de seguridad sin modificación
            _logger.LogWarning(ex, "ConsultorioService: Error controlado en GetAllAsync: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            // Capturamos otras excepciones para logging centralizado y las re-lanzamos
            _logger.LogError(ex, "ConsultorioService: Error no controlado en GetAllAsync: {Message}", ex.Message);
            throw;
        }
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
        try
        {
            _logger.LogInformation("ConsultorioService: Delegando CreateAsync al servicio especializado");
            return await _consultorioManagementService.CreateAsync(consultorioDto);
        }
        catch (Exception ex) when (ex is ForbiddenAccessException || ex is BadRequestException)
        {
            // Propagamos las excepciones de seguridad y validación sin modificación
            _logger.LogWarning(ex, "ConsultorioService: Error controlado en CreateAsync: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            // Capturamos otras excepciones para logging centralizado y las re-lanzamos
            _logger.LogError(ex, "ConsultorioService: Error no controlado en CreateAsync: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Actualiza un consultorio existente
    /// </summary>
    public async Task<ConsultorioDto> UpdateAsync(ConsultorioUpdateDto consultorioDto)
    {
        try
        {
            _logger.LogInformation("ConsultorioService: Delegando UpdateAsync al servicio especializado");
            return await _consultorioManagementService.UpdateAsync(consultorioDto);
        }
        catch (Exception ex) when (ex is ForbiddenAccessException || ex is NotFoundException || ex is BadRequestException)
        {
            // Propagamos las excepciones de seguridad, no encontrado y validación sin modificación
            _logger.LogWarning(ex, "ConsultorioService: Error controlado en UpdateAsync: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            // Capturamos otras excepciones para logging centralizado y las re-lanzamos
            _logger.LogError(ex, "ConsultorioService: Error no controlado en UpdateAsync: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Elimina un consultorio por su ID
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("ConsultorioService: Delegando DeleteAsync al servicio especializado");
            return await _consultorioManagementService.DeleteAsync(id);
        }
        catch (Exception ex) when (ex is ForbiddenAccessException || ex is NotFoundException)
        {
            // Propagamos las excepciones de seguridad y no encontrado sin modificación
            _logger.LogWarning(ex, "ConsultorioService: Error controlado en DeleteAsync: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            // Capturamos otras excepciones para logging centralizado y las re-lanzamos
            _logger.LogError(ex, "ConsultorioService: Error no controlado en DeleteAsync: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Verifica si existe un consultorio con el ID especificado
    /// </summary>
    /// <param name="id">ID del consultorio a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("ConsultorioService: Delegando ExistsByIdAsync al servicio especializado");
            return await _consultorioManagementService.ExistsByIdAsync(id);
        }
        catch (Exception ex)
        {
            // Este método no debería lanzar excepciones relacionadas con permisos
            // ya que es una verificación básica, pero podemos capturar errores técnicos
            _logger.LogError(ex, "ConsultorioService: Error en ExistsByIdAsync: {Message}", ex.Message);
            throw;
        }
    }
}
