using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Servicio para la gestión de roles de usuarios, extrayendo esta responsabilidad
/// de UsuarioService para mejorar la separación de responsabilidades
/// </summary>
public class UsuarioRoleService : IUsuarioRoleService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UsuarioRoleService> _logger;

    public UsuarioRoleService(
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository,
        ICurrentUserService currentUserService,
        ILogger<UsuarioRoleService> logger)
    {
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _rolRepository = rolRepository ?? throw new ArgumentNullException(nameof(rolRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Asigna un rol a un usuario por nombre del rol
    /// </summary>
    public async Task<bool> AsignarRolAsync(Guid idUsuario, string nombreRol)
    {
        // Verificar permiso para administrar roles de usuario
        if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.AssignRoles))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó asignar rol sin el permiso requerido", 
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        _logger.LogInformation("Asignando rol {RolNombre} a usuario {UserId}", nombreRol, idUsuario);
        
        try
        {
            // Verificar que el usuario existe
            var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
            if (usuario == null)
            {
                _logger.LogWarning("No se puede asignar rol: usuario {UserId} no existe", idUsuario);
                throw new NotFoundException("Usuario", idUsuario);
            }
            
            // Verificar que el rol existe
            var rol = await _rolRepository.GetByNombreAsync(nombreRol);
            if (rol == null)
            {
                _logger.LogWarning("No se puede asignar rol: rol {RolNombre} no existe", nombreRol);
                throw new NotFoundException("Rol", nombreRol);
            }
            
            // Asignar el rol
            var result = await _usuarioRepository.AsignarRolAsync(idUsuario, nombreRol);
            await _usuarioRepository.SaveChangesAsync();
            
            return result;
        }
        catch (Exception ex) when (!(ex is NotFoundException) && !(ex is ForbiddenAccessException))
        {
            _logger.LogError(ex, "Error al asignar rol {RolNombre} a usuario {UserId}", nombreRol, idUsuario);
            throw;
        }
    }

    /// <summary>
    /// Remueve un rol de un usuario por nombre del rol
    /// </summary>
    public async Task<bool> RemoverRolAsync(Guid idUsuario, string nombreRol)
    {
        // Verificar permiso para administrar roles de usuario
        if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.AssignRoles))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó remover rol sin el permiso requerido", 
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        _logger.LogInformation("Removiendo rol {RolNombre} a usuario {UserId}", nombreRol, idUsuario);
        
        try
        {
            // Verificar que el usuario existe
            var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
            if (usuario == null)
            {
                _logger.LogWarning("No se puede remover rol: usuario {UserId} no existe", idUsuario);
                throw new NotFoundException("Usuario", idUsuario);
            }
            
            // Verificar que el rol existe
            var rol = await _rolRepository.GetByNombreAsync(nombreRol);
            if (rol == null)
            {
                _logger.LogWarning("No se puede remover rol: rol {RolNombre} no existe", nombreRol);
                throw new NotFoundException("Rol", nombreRol);
            }
            
            // Remover el rol
            var result = await _usuarioRepository.RemoverRolAsync(idUsuario, nombreRol);
            await _usuarioRepository.SaveChangesAsync();
            
            return result;
        }
        catch (Exception ex) when (!(ex is NotFoundException) && !(ex is ForbiddenAccessException))
        {
            _logger.LogError(ex, "Error al remover rol {RolNombre} de usuario {UserId}", nombreRol, idUsuario);
            throw;
        }
    }

    /// <summary>
    /// Asigna un rol a un usuario por ID del rol
    /// </summary>
    public async Task<bool> AsignarRolPorIdAsync(Guid idUsuario, Guid idRol)
    {
        // Verificar permiso para administrar roles de usuario
        if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.AssignRoles))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó asignar rol sin el permiso requerido", 
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        _logger.LogInformation("Asignando rol {RolId} a usuario {UserId}", idRol, idUsuario);
        
        try
        {
            // Verificar que el usuario existe
            var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
            if (usuario == null)
            {
                _logger.LogWarning("No se puede asignar rol: usuario {UserId} no existe", idUsuario);
                throw new NotFoundException("Usuario", idUsuario);
            }
            
            // Verificar que el rol existe
            var rol = await _rolRepository.GetByIdAsync(idRol);
            if (rol == null)
            {
                _logger.LogWarning("No se puede asignar rol: rol con ID {RolId} no existe", idRol);
                throw new NotFoundException("Rol", idRol);
            }
            
            // Asignar el rol
            var result = await _usuarioRepository.AsignarRolPorIdAsync(idUsuario, idRol);
            await _usuarioRepository.SaveChangesAsync();
            
            return result;
        }
        catch (Exception ex) when (!(ex is NotFoundException) && !(ex is ForbiddenAccessException))
        {
            _logger.LogError(ex, "Error al asignar rol {RolId} a usuario {UserId}", idRol, idUsuario);
            throw;
        }
    }

    /// <summary>
    /// Remueve un rol de un usuario por ID del rol
    /// </summary>
    public async Task<bool> RemoverRolPorIdAsync(Guid idUsuario, Guid idRol)
    {
        // Verificar permiso para administrar roles de usuario
        if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.AssignRoles))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó remover rol sin el permiso requerido", 
                _currentUserService.GetCurrentUserId());
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }

        _logger.LogInformation("Removiendo rol {RolId} de usuario {UserId}", idRol, idUsuario);
        
        try
        {
            // Verificar que el usuario existe
            var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
            if (usuario == null)
            {
                _logger.LogWarning("No se puede remover rol: usuario {UserId} no existe", idUsuario);
                throw new NotFoundException("Usuario", idUsuario);
            }
            
            // Verificar que el rol existe
            var rol = await _rolRepository.GetByIdAsync(idRol);
            if (rol == null)
            {
                _logger.LogWarning("No se puede remover rol: rol con ID {RolId} no existe", idRol);
                throw new NotFoundException("Rol", idRol);
            }
            
            // Remover el rol
            var result = await _usuarioRepository.RemoverRolPorIdAsync(idUsuario, idRol);
            await _usuarioRepository.SaveChangesAsync();
            
            return result;
        }
        catch (Exception ex) when (!(ex is NotFoundException) && !(ex is ForbiddenAccessException))
        {
            _logger.LogError(ex, "Error al remover rol {RolId} de usuario {UserId}", idRol, idUsuario);
            throw;
        }
    }

    /// <summary>
    /// Obtiene los roles de un usuario
    /// </summary>
    public async Task<IEnumerable<string>> GetRolesUsuarioAsync(Guid idUsuario)
    {
        // Verificar permiso para ver roles de usuario
        // El usuario puede ver sus propios roles o si tiene el permiso específico
        var currentUserId = _currentUserService.GetCurrentUserId();
        if (currentUserId != idUsuario && !await _currentUserService.HasPermisoAsync(UsuariosPermissions.ViewRoles))
        {
            _logger.LogWarning("Acceso denegado: Usuario {CurrentUserId} intentó ver roles del usuario {UserId} sin el permiso requerido", 
                currentUserId, idUsuario);
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }
        
        _logger.LogInformation("Obteniendo roles para el usuario {UserId}", idUsuario);
        
        try
        {
            // Verificar que el usuario existe
            var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
            if (usuario == null)
            {
                _logger.LogWarning("No se pueden obtener roles: usuario {UserId} no existe", idUsuario);
                throw new NotFoundException("Usuario", idUsuario);
            }
            
            // Obtener los roles
            return await _usuarioRepository.GetUserRolesAsync(idUsuario);
        }
        catch (Exception ex) when (!(ex is NotFoundException) && !(ex is ForbiddenAccessException))
        {
            _logger.LogError(ex, "Error al obtener roles del usuario {UserId}", idUsuario);
            throw;
        }
    }

    /// <summary>
    /// Verifica si un usuario tiene un rol específico
    /// </summary>
    public async Task<bool> HasRolAsync(Guid idUsuario, string nombreRol)
    {
        // Verificar permiso para ver roles de usuario
        // El usuario puede verificar sus propios roles o si tiene el permiso específico
        var currentUserId = _currentUserService.GetCurrentUserId();
        if (currentUserId != idUsuario && !await _currentUserService.HasPermisoAsync(UsuariosPermissions.ViewRoles))
        {
            _logger.LogWarning("Acceso denegado: Usuario {CurrentUserId} intentó verificar rol del usuario {UserId} sin el permiso requerido", 
                currentUserId, idUsuario);
            throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
        }
        
        _logger.LogInformation("Verificando si usuario {UserId} tiene el rol {RolNombre}", idUsuario, nombreRol);
        
        try
        {
            // Verificar que el usuario existe
            var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
            if (usuario == null)
            {
                _logger.LogWarning("No se puede verificar rol: usuario {UserId} no existe", idUsuario);
                throw new NotFoundException("Usuario", idUsuario);
            }
            
            // Verificar que el rol existe
            var rol = await _rolRepository.GetByNombreAsync(nombreRol);
            if (rol == null)
            {
                _logger.LogWarning("No se puede verificar rol: rol {RolNombre} no existe", nombreRol);
                throw new NotFoundException("Rol", nombreRol);
            }
            
            // Verificar si el usuario tiene el rol
            return await _usuarioRepository.HasRolAsync(idUsuario, nombreRol);
        }
        catch (Exception ex) when (!(ex is NotFoundException) && !(ex is ForbiddenAccessException))
        {
            _logger.LogError(ex, "Error al verificar si usuario {UserId} tiene el rol {RolNombre}", idUsuario, nombreRol);
            throw;
        }
    }
}
