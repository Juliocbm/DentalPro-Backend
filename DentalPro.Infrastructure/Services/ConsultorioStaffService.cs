using AutoMapper;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using DentalPro.Application.DTOs.Consultorio;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio especializado para la gestión del personal de un consultorio
/// </summary>
public class ConsultorioStaffService : IConsultorioStaffService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConsultorioRepository _consultorioRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<ConsultorioStaffService> _logger;
    private readonly IAuditService _auditService;

    /// <summary>
    /// Constructor con inyección de dependencias
    /// </summary>
    public ConsultorioStaffService(
        IUsuarioRepository usuarioRepository,
        IConsultorioRepository consultorioRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<ConsultorioStaffService> logger,
        IAuditService auditService)
    {
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _consultorioRepository = consultorioRepository ?? throw new ArgumentNullException(nameof(consultorioRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    /// <summary>
    /// Obtiene todos los doctores asociados a un consultorio específico
    /// </summary>
    /// <param name="consultorioId">ID del consultorio</param>
    /// <returns>Lista de doctores del consultorio</returns>
    public async Task<IEnumerable<UsuarioDto>> GetDoctoresByConsultorioAsync(Guid consultorioId)
    {
        // Verificar permiso para ver doctores
        if (!await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.ViewDoctores))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó ver doctores sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        // Verificar acceso al consultorio
        await ValidateConsultorioAccessAsync(consultorioId);

        // Verificar que el consultorio exista
        if (!await ConsultorioExistsAsync(consultorioId))
        {
            throw new NotFoundException("Consultorio", consultorioId);
        }

        // Obtener todos los usuarios del consultorio
        var usuarios = await _usuarioRepository.GetByConsultorioAsync(consultorioId);
        
        // Filtrar solo los doctores (mediante el rol "Doctor")
        var doctores = new List<Usuario>();
        foreach (var usuario in usuarios)
        {
            if (await _usuarioRepository.HasRolAsync(usuario.IdUsuario, "Doctor"))
            {
                doctores.Add(usuario);
            }
        }

        _logger.LogInformation("Usuario {UserId} obtuvo la lista de doctores del consultorio {ConsultorioId}",
            _currentUserService.GetCurrentUserId(), consultorioId);

        return _mapper.Map<IEnumerable<UsuarioDto>>(doctores);
    }

    /// <summary>
    /// Obtiene todos los asistentes asociados a un consultorio específico
    /// </summary>
    /// <param name="consultorioId">ID del consultorio</param>
    /// <returns>Lista de asistentes del consultorio</returns>
    public async Task<IEnumerable<UsuarioDto>> GetAsistentesByConsultorioAsync(Guid consultorioId)
    {
        // Verificar permiso para ver asistentes
        if (!await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.ViewAsistentes))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó ver asistentes sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        // Verificar acceso al consultorio
        await ValidateConsultorioAccessAsync(consultorioId);

        // Verificar que el consultorio exista
        if (!await ConsultorioExistsAsync(consultorioId))
        {
            throw new NotFoundException("Consultorio", consultorioId);
        }

        // Obtener todos los usuarios del consultorio
        var usuarios = await _usuarioRepository.GetByConsultorioAsync(consultorioId);
        
        // Filtrar solo los asistentes (mediante el rol "Asistente")
        var asistentes = new List<Usuario>();
        foreach (var usuario in usuarios)
        {
            if (await _usuarioRepository.HasRolAsync(usuario.IdUsuario, "Asistente"))
            {
                asistentes.Add(usuario);
            }
        }

        _logger.LogInformation("Usuario {UserId} obtuvo la lista de asistentes del consultorio {ConsultorioId}",
            _currentUserService.GetCurrentUserId(), consultorioId);

        return _mapper.Map<IEnumerable<UsuarioDto>>(asistentes);
    }

    /// <summary>
    /// Asigna un doctor a un consultorio
    /// </summary>
    /// <param name="usuarioId">ID del usuario doctor</param>
    /// <param name="consultorioId">ID del consultorio</param>
    /// <returns>True si la asignación fue exitosa</returns>
    public async Task<bool> AsignarDoctorAsync(Guid usuarioId, Guid consultorioId)
    {
        // Verificar permiso para asignar personal
        if (!await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.AssignStaff))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó asignar doctor sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        // Verificar que el consultorio exista
        if (!await ConsultorioExistsAsync(consultorioId))
        {
            throw new NotFoundException("Consultorio", consultorioId);
        }

        // Verificar acceso al consultorio
        await ValidateConsultorioAccessAsync(consultorioId);

        // Verificar que el usuario exista
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario == null)
        {
            throw new NotFoundException("Usuario", usuarioId);
        }

        // Verificar que el usuario tenga el rol de Doctor
        if (!await _usuarioRepository.HasRolAsync(usuarioId, "Doctor"))
        {
            _logger.LogWarning("Error en AsignarDoctorAsync: El usuario {UserId} no tiene el rol de Doctor", usuarioId);
            throw new BadRequestException(ErrorMessages.DefaultError, "El usuario no tiene el rol de Doctor");
        }

        // Asignar el consultorio al usuario (aquí asumo que hay una propiedad IdConsultorio en la entidad Usuario)
        usuario.IdConsultorio = consultorioId;
        await _usuarioRepository.UpdateAsync(usuario);
        var result = await _usuarioRepository.SaveChangesAsync();

        if (result > 0)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            _logger.LogInformation("Usuario {UserId} asignó al doctor {DoctorId} al consultorio {ConsultorioId}",
                currentUserId, usuarioId, consultorioId);

            // Registrar acción en el servicio de auditoría
            await _auditService.RegisterActionAsync(
                "AsignarDoctor",
                "Usuario",
                usuarioId,
                currentUserId,
                JsonConvert.SerializeObject(new { usuarioId, consultorioId }));

            return true;
        }

        return false;
    }

    /// <summary>
    /// Asigna un asistente a un consultorio
    /// </summary>
    /// <param name="usuarioId">ID del usuario asistente</param>
    /// <param name="consultorioId">ID del consultorio</param>
    /// <returns>True si la asignación fue exitosa</returns>
    public async Task<bool> AsignarAsistenteAsync(Guid usuarioId, Guid consultorioId)
    {
        // Verificar permiso para asignar personal
        if (!await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.AssignStaff))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó asignar asistente sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        // Verificar que el consultorio exista
        if (!await ConsultorioExistsAsync(consultorioId))
        {
            throw new NotFoundException("Consultorio", consultorioId);
        }

        // Verificar acceso al consultorio
        await ValidateConsultorioAccessAsync(consultorioId);

        // Verificar que el usuario exista
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario == null)
        {
            throw new NotFoundException("Usuario", usuarioId);
        }

        // Verificar que el usuario tenga el rol de Asistente
        if (!await _usuarioRepository.HasRolAsync(usuarioId, "Asistente"))
        {
            _logger.LogWarning("Error en AsignarAsistenteAsync: El usuario {UserId} no tiene el rol de Asistente", usuarioId);
            throw new BadRequestException(ErrorMessages.DefaultError, "El usuario no tiene el rol de Asistente");
        }

        // Asignar el consultorio al usuario
        usuario.IdConsultorio = consultorioId;
        await _usuarioRepository.UpdateAsync(usuario);
        var result = await _usuarioRepository.SaveChangesAsync();

        if (result > 0)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            _logger.LogInformation("Usuario {UserId} asignó al asistente {AsistenteId} al consultorio {ConsultorioId}",
                currentUserId, usuarioId, consultorioId);

            // Registrar acción en el servicio de auditoría
            await _auditService.RegisterActionAsync(
                "AsignarAsistente",
                "Usuario",
                usuarioId,
                currentUserId,
                JsonConvert.SerializeObject(new { usuarioId, consultorioId }));

            return true;
        }

        return false;
    }

    /// <summary>
    /// Desvincula un miembro del personal de un consultorio
    /// </summary>
    /// <param name="usuarioId">ID del usuario a desvincular</param>
    /// <param name="consultorioId">ID del consultorio</param>
    /// <returns>True si la desvinculación fue exitosa</returns>
    public async Task<bool> DesvincularMiembroAsync(Guid usuarioId, Guid consultorioId)
    {
        // Verificar permiso para remover personal
        if (!await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.RemoveStaff))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó desvincular miembro sin el permiso requerido",
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        // Verificar que el consultorio exista
        if (!await ConsultorioExistsAsync(consultorioId))
        {
            throw new NotFoundException("Consultorio", consultorioId);
        }

        // Verificar acceso al consultorio
        await ValidateConsultorioAccessAsync(consultorioId);

        // Verificar que el usuario exista
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario == null)
        {
            throw new NotFoundException("Usuario", usuarioId);
        }

        // Verificar que el usuario pertenezca al consultorio especificado
        if (usuario.IdConsultorio != consultorioId)
        {
            _logger.LogWarning("Error en DesvincularMiembroAsync: El usuario {UserId} no pertenece al consultorio {ConsultorioId}",
                usuarioId, consultorioId);
            throw new BadRequestException(ErrorMessages.DefaultError, "El usuario no pertenece al consultorio especificado");
        }

        // Desvincular el usuario del consultorio (estableciendo IdConsultorio a Guid.Empty)
        usuario.IdConsultorio = Guid.Empty;
        await _usuarioRepository.UpdateAsync(usuario);
        var result = await _usuarioRepository.SaveChangesAsync();

        if (result > 0)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            _logger.LogInformation("Usuario {UserId} desvinculó al miembro {MiembroId} del consultorio {ConsultorioId}",
                currentUserId, usuarioId, consultorioId);

            // Registrar acción en el servicio de auditoría
            await _auditService.RegisterActionAsync(
                "DesvincularMiembro",
                "Usuario",
                usuarioId,
                currentUserId,
                JsonConvert.SerializeObject(new { usuarioId, consultorioId }));

            return true;
        }

        return false;
    }

    #region Métodos privados de ayuda

    /// <summary>
    /// Valida el acceso del usuario actual al consultorio especificado
    /// </summary>
    /// <param name="consultorioId">ID del consultorio a validar</param>
    /// <exception cref="ForbiddenAccessException">Si el usuario no tiene acceso global y el consultorio no es el suyo</exception>
    private async Task ValidateConsultorioAccessAsync(Guid consultorioId)
    {
        var hasGlobalAccess = await _currentUserService.HasPermisoAsync(ConsultoriosPermissions.ViewAll);
        if (!hasGlobalAccess)
        {
            var userConsultorioId = _currentUserService.GetCurrentConsultorioId();
            if (consultorioId != userConsultorioId)
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó acceder a un consultorio diferente",
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.DifferentConsultorio);
            }
        }
    }

    /// <summary>
    /// Verifica si existe un consultorio con el ID especificado
    /// </summary>
    /// <param name="id">ID del consultorio a verificar</param>
    /// <returns>True si existe, False en caso contrario</returns>
    private async Task<bool> ConsultorioExistsAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return false;
        }
        
        var consultorio = await _consultorioRepository.GetByIdAsync(id);
        return consultorio != null;
    }

    #endregion
}
