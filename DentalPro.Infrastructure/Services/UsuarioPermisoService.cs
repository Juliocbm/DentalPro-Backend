using AutoMapper;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using DentalPro.Application.DTOs.Permiso;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DentalPro.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio para gestionar los permisos directos de usuario
    /// </summary>
    public class UsuarioPermisoService : IUsuarioPermisoService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPermisoRepository _permisoRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ILogger<UsuarioPermisoService> _logger;

        public UsuarioPermisoService(
            IUsuarioRepository usuarioRepository,
            IPermisoRepository permisoRepository,
            ICurrentUserService currentUserService,
            IMapper mapper,
            ILogger<UsuarioPermisoService> logger)
        {
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _permisoRepository = permisoRepository ?? throw new ArgumentNullException(nameof(permisoRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Asigna un permiso a un usuario por nombre del permiso
        /// </summary>
        public async Task<bool> AsignarPermisoAsync(Guid idUsuario, string nombrePermiso)
        {
            // Verificar permiso para asignar permisos
            if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.AssignPermisos))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó asignar permiso por nombre sin el permiso requerido", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            _logger.LogInformation("Asignando permiso {PermisoNombre} a usuario {UserId}", nombrePermiso, idUsuario);
            
            try
            {
                // Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
                if (usuario == null)
                {
                    _logger.LogWarning("No se puede asignar permiso: usuario {UserId} no existe", idUsuario);
                    throw new NotFoundException("Usuario", idUsuario);
                }
                
                // Verificar que el permiso existe
                var permiso = await _permisoRepository.GetByNombreAsync(nombrePermiso);
                if (permiso == null)
                {
                    _logger.LogWarning("No se puede asignar permiso: permiso {PermisoNombre} no existe", nombrePermiso);
                    throw new NotFoundException("Permiso", nombrePermiso);
                }
                
                // Asignar el permiso
                var result = await _usuarioRepository.AsignarPermisoAsync(idUsuario, permiso.IdPermiso);
                await _usuarioRepository.SaveChangesAsync();
                
                return result;
            }
            catch (Exception ex) when (!(ex is NotFoundException) && !(ex is ForbiddenAccessException))
            {
                _logger.LogError(ex, "Error al asignar permiso {PermisoNombre} a usuario {UserId}", nombrePermiso, idUsuario);
                throw;
            }
        }

        /// <summary>
        /// Asigna un permiso a un usuario por ID del permiso
        /// </summary>
        public async Task<bool> AsignarPermisoAsync(Guid idUsuario, Guid idPermiso)
        {
            // Verificar permiso para asignar permisos
            if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.AssignPermisos))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó asignar permiso por ID sin el permiso requerido", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            _logger.LogInformation("Asignando permiso {PermisoId} a usuario {UserId}", idPermiso, idUsuario);
            
            try
            {
                // Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
                if (usuario == null)
                {
                    _logger.LogWarning("No se puede asignar permiso: usuario {UserId} no existe", idUsuario);
                    throw new NotFoundException("Usuario", idUsuario);
                }
                
                // Verificar que el permiso existe
                var permiso = await _permisoRepository.GetByIdAsync(idPermiso);
                if (permiso == null)
                {
                    _logger.LogWarning("No se puede asignar permiso: permiso con ID {PermisoId} no existe", idPermiso);
                    throw new NotFoundException("Permiso", idPermiso);
                }
                
                // Asignar el permiso
                var result = await _usuarioRepository.AsignarPermisoAsync(idUsuario, idPermiso);
                await _usuarioRepository.SaveChangesAsync();
                
                return result;
            }
            catch (Exception ex) when (!(ex is NotFoundException) && !(ex is ForbiddenAccessException))
            {
                _logger.LogError(ex, "Error al asignar permiso con ID {PermisoId} a usuario {UserId}", idPermiso, idUsuario);
                throw;
            }
        }

        /// <summary>
        /// Remueve un permiso de un usuario por nombre del permiso
        /// </summary>
        public async Task<bool> RemoverPermisoAsync(Guid idUsuario, string nombrePermiso)
        {
            // Verificar permiso para remover permisos
            if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.RemovePermisos))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó remover permiso por nombre sin el permiso requerido", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            _logger.LogInformation("Removiendo permiso {PermisoNombre} de usuario {UserId}", nombrePermiso, idUsuario);
            
            try
            {
                // Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
                if (usuario == null)
                {
                    _logger.LogWarning("No se puede remover permiso: usuario {UserId} no existe", idUsuario);
                    throw new NotFoundException("Usuario", idUsuario);
                }
                
                // Verificar que el permiso existe
                var permiso = await _permisoRepository.GetByNombreAsync(nombrePermiso);
                if (permiso == null)
                {
                    _logger.LogWarning("No se puede remover permiso: permiso {PermisoNombre} no existe", nombrePermiso);
                    throw new NotFoundException("Permiso", nombrePermiso);
                }
                
                // Remover el permiso
                var result = await _usuarioRepository.RemoverPermisoAsync(idUsuario, permiso.IdPermiso);
                await _usuarioRepository.SaveChangesAsync();
                
                return result;
            }
            catch (Exception ex) when (!(ex is NotFoundException) && !(ex is ForbiddenAccessException))
            {
                _logger.LogError(ex, "Error al remover permiso {PermisoNombre} de usuario {UserId}", nombrePermiso, idUsuario);
                throw;
            }
        }

        /// <summary>
        /// Remueve un permiso de un usuario por ID del permiso
        /// </summary>
        public async Task<bool> RemoverPermisoAsync(Guid idUsuario, Guid idPermiso)
        {
            // Verificar permiso para remover permisos
            if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.RemovePermisos))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó remover permiso por ID sin el permiso requerido", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            _logger.LogInformation("Removiendo permiso {PermisoId} de usuario {UserId}", idPermiso, idUsuario);
            
            try
            {
                // Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
                if (usuario == null)
                {
                    _logger.LogWarning("No se puede remover permiso: usuario {UserId} no existe", idUsuario);
                    throw new NotFoundException("Usuario", idUsuario);
                }
                
                // Verificar que el permiso existe
                var permiso = await _permisoRepository.GetByIdAsync(idPermiso);
                if (permiso == null)
                {
                    _logger.LogWarning("No se puede remover permiso: permiso con ID {PermisoId} no existe", idPermiso);
                    throw new NotFoundException("Permiso", idPermiso);
                }
                
                // Remover el permiso
                var result = await _usuarioRepository.RemoverPermisoAsync(idUsuario, idPermiso);
                await _usuarioRepository.SaveChangesAsync();
                
                return result;
            }
            catch (Exception ex) when (!(ex is NotFoundException) && !(ex is ForbiddenAccessException))
            {
                _logger.LogError(ex, "Error al remover permiso con ID {PermisoId} de usuario {UserId}", idPermiso, idUsuario);
                throw;
            }
        }

        /// <summary>
        /// Obtiene los permisos directos de un usuario por su ID
        /// </summary>
        public async Task<IEnumerable<string>> GetPermisosAsync(Guid idUsuario)
        {
            // Verificar permiso para ver permisos
            if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.ViewPermisos))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó obtener permisos de usuario sin el permiso requerido", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            _logger.LogInformation("Obteniendo permisos de usuario {UserId}", idUsuario);
            
            try
            {
                // Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
                if (usuario == null)
                {
                    _logger.LogWarning("No se pueden obtener permisos: usuario {UserId} no existe", idUsuario);
                    throw new NotFoundException("Usuario", idUsuario);
                }
                
                // Obtener los permisos
                var permisos = await _usuarioRepository.GetUserPermisosAsync(idUsuario);
                return permisos;
            }
            catch (Exception ex) when (!(ex is NotFoundException) && !(ex is ForbiddenAccessException))
            {
                _logger.LogError(ex, "Error al obtener permisos de usuario {UserId}", idUsuario);
                throw;
            }
        }

        /// <summary>
        /// Obtiene información detallada de permisos directos de un usuario
        /// </summary>
        public async Task<IEnumerable<PermisoDto>> GetPermisosUsuarioAsync(Guid idUsuario)
        {
            // Verificar permiso para ver permisos
            if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.ViewPermisos))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó obtener permisos detallados de usuario sin el permiso requerido", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            _logger.LogInformation("Obteniendo permisos detallados de usuario {UserId}", idUsuario);
            
            try
            {
                // Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
                if (usuario == null)
                {
                    _logger.LogWarning("No se pueden obtener permisos detallados: usuario {UserId} no existe", idUsuario);
                    throw new NotFoundException("Usuario", idUsuario);
                }
                
                // Obtener los permisos directos
                var permisos = await _usuarioRepository.GetUsuarioPermisosAsync(idUsuario);
                return _mapper.Map<IEnumerable<PermisoDto>>(permisos);
            }
            catch (Exception ex) when (!(ex is NotFoundException) && !(ex is ForbiddenAccessException))
            {
                _logger.LogError(ex, "Error al obtener permisos detallados de usuario {UserId}", idUsuario);
                throw;
            }
        }

        /// <summary>
        /// Asigna múltiples permisos a un usuario por sus IDs
        /// </summary>
        public async Task<bool> AsignarPermisosUsuarioAsync(Guid idUsuario, IEnumerable<Guid> idsPermisos)
        {
            // Verificar permiso para asignar permisos
            if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.AssignPermisos))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó asignar múltiples permisos sin el permiso requerido", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            _logger.LogInformation("Asignando múltiples permisos a usuario {UserId}", idUsuario);
            
            try
            {
                // Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
                if (usuario == null)
                {
                    _logger.LogWarning("No se pueden asignar permisos: usuario {UserId} no existe", idUsuario);
                    throw new NotFoundException("Usuario", idUsuario);
                }
                
                // Verificar que todos los permisos existen
                foreach (var idPermiso in idsPermisos)
                {
                    var permiso = await _permisoRepository.GetByIdAsync(idPermiso);
                    if (permiso == null)
                    {
                        _logger.LogWarning("No se pueden asignar permisos: permiso con ID {PermisoId} no existe", idPermiso);
                        throw new NotFoundException("Permiso", idPermiso);
                    }
                }
                
                // Asignar todos los permisos
                var result = true;
                foreach (var idPermiso in idsPermisos)
                {
                    result = result && await _usuarioRepository.AsignarPermisoAsync(idUsuario, idPermiso);
                }
                
                await _usuarioRepository.SaveChangesAsync();
                
                return result;
            }
            catch (Exception ex) when (!(ex is NotFoundException) && !(ex is ForbiddenAccessException))
            {
                _logger.LogError(ex, "Error al asignar múltiples permisos a usuario {UserId}", idUsuario);
                throw;
            }
        }

        /// <summary>
        /// Asigna múltiples permisos a un usuario por sus nombres
        /// </summary>
        public async Task<bool> AsignarPermisosUsuarioByNombreAsync(Guid idUsuario, IEnumerable<string> nombresPermisos)
        {
            // Verificar permiso para asignar permisos
            if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.AssignPermisos))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó asignar múltiples permisos por nombre sin el permiso requerido", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            _logger.LogInformation("Asignando múltiples permisos por nombre a usuario {UserId}", idUsuario);
            
            try
            {
                // Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
                if (usuario == null)
                {
                    _logger.LogWarning("No se pueden asignar permisos: usuario {UserId} no existe", idUsuario);
                    throw new NotFoundException("Usuario", idUsuario);
                }
                
                // Verificar que todos los permisos existen y recopilar sus IDs
                var idsPermisos = new List<Guid>();
                foreach (var nombrePermiso in nombresPermisos)
                {
                    var permiso = await _permisoRepository.GetByNombreAsync(nombrePermiso);
                    if (permiso == null)
                    {
                        _logger.LogWarning("No se pueden asignar permisos: permiso {PermisoNombre} no existe", nombrePermiso);
                        throw new NotFoundException("Permiso", nombrePermiso);
                    }
                    
                    idsPermisos.Add(permiso.IdPermiso);
                }
                
                // Usar el método existente para asignar por IDs
                return await AsignarPermisosUsuarioAsync(idUsuario, idsPermisos);
            }
            catch (Exception ex) when (!(ex is NotFoundException) && !(ex is ForbiddenAccessException))
            {
                _logger.LogError(ex, "Error al asignar múltiples permisos por nombre a usuario {UserId}", idUsuario);
                throw;
            }
        }

        /// <summary>
        /// Remueve múltiples permisos de un usuario por sus IDs
        /// </summary>
        public async Task<bool> RemoverPermisosUsuarioAsync(Guid idUsuario, IEnumerable<Guid> idsPermisos)
        {
            // Verificar permiso para remover permisos
            if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.RemovePermisos))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó remover múltiples permisos sin el permiso requerido", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            _logger.LogInformation("Removiendo múltiples permisos de usuario {UserId}", idUsuario);
            
            try
            {
                // Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
                if (usuario == null)
                {
                    _logger.LogWarning("No se pueden remover permisos: usuario {UserId} no existe", idUsuario);
                    throw new NotFoundException("Usuario", idUsuario);
                }
                
                // Verificar que todos los permisos existen
                foreach (var idPermiso in idsPermisos)
                {
                    var permiso = await _permisoRepository.GetByIdAsync(idPermiso);
                    if (permiso == null)
                    {
                        _logger.LogWarning("No se pueden remover permisos: permiso con ID {PermisoId} no existe", idPermiso);
                        throw new NotFoundException("Permiso", idPermiso);
                    }
                }
                
                // Remover todos los permisos
                var result = true;
                foreach (var idPermiso in idsPermisos)
                {
                    result = result && await _usuarioRepository.RemoverPermisoAsync(idUsuario, idPermiso);
                }
                
                await _usuarioRepository.SaveChangesAsync();
                
                return result;
            }
            catch (Exception ex) when (!(ex is NotFoundException) && !(ex is ForbiddenAccessException))
            {
                _logger.LogError(ex, "Error al remover múltiples permisos de usuario {UserId}", idUsuario);
                throw;
            }
        }

        /// <summary>
        /// Remueve múltiples permisos de un usuario por sus nombres
        /// </summary>
        public async Task<bool> RemoverPermisosUsuarioByNombreAsync(Guid idUsuario, IEnumerable<string> nombresPermisos)
        {
            // Verificar permiso para remover permisos
            if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.RemovePermisos))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó remover múltiples permisos por nombre sin el permiso requerido", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorMessages.InsufficientPermissions);
            }

            _logger.LogInformation("Removiendo múltiples permisos por nombre de usuario {UserId}", idUsuario);
            
            try
            {
                // Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
                if (usuario == null)
                {
                    _logger.LogWarning("No se pueden remover permisos: usuario {UserId} no existe", idUsuario);
                    throw new NotFoundException("Usuario", idUsuario);
                }
                
                // Verificar que todos los permisos existen y recopilar sus IDs
                var idsPermisos = new List<Guid>();
                foreach (var nombrePermiso in nombresPermisos)
                {
                    var permiso = await _permisoRepository.GetByNombreAsync(nombrePermiso);
                    if (permiso == null)
                    {
                        _logger.LogWarning("No se pueden remover permisos: permiso {PermisoNombre} no existe", nombrePermiso);
                        throw new NotFoundException("Permiso", nombrePermiso);
                    }
                    
                    idsPermisos.Add(permiso.IdPermiso);
                }
                
                // Usar el método existente para remover por IDs
                return await RemoverPermisosUsuarioAsync(idUsuario, idsPermisos);
            }
            catch (Exception ex) when (!(ex is NotFoundException) && !(ex is ForbiddenAccessException))
            {
                _logger.LogError(ex, "Error al remover múltiples permisos por nombre de usuario {UserId}", idUsuario);
                throw;
            }
        }

        /// <summary>
        /// Verifica si un usuario tiene un permiso específico directamente asignado
        /// </summary>
        public async Task<bool> HasPermisoDirectoAsync(Guid idUsuario, string nombrePermiso)
        {
            try
            {
                // Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
                if (usuario == null)
                {
                    _logger.LogWarning("No se puede verificar permiso: usuario {UserId} no existe", idUsuario);
                    throw new NotFoundException("Usuario", idUsuario);
                }
                
                // Verificar el permiso
                return await _usuarioRepository.TienePermisoDirectoAsync(idUsuario, nombrePermiso);
            }
            catch (Exception ex) when (!(ex is NotFoundException))
            {
                _logger.LogError(ex, "Error al verificar si usuario {UserId} tiene permiso {PermisoNombre}", idUsuario, nombrePermiso);
                throw;
            }
        }
        
        /// <summary>
        /// Verifica si un usuario tiene un permiso específico por su nombre (incluye permisos directos y heredados por rol)
        /// </summary>
        public async Task<bool> HasPermisoByNameAsync(Guid idUsuario, string nombrePermiso)
        {
            try
            {
                _logger.LogInformation("Verificando si usuario {UserId} tiene el permiso {PermisoNombre}", idUsuario, nombrePermiso);
                
                // Verificar que el usuario existe
                var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
                if (usuario == null)
                {
                    _logger.LogWarning("No se puede verificar permiso: usuario {UserId} no existe", idUsuario);
                    throw new NotFoundException("Usuario", idUsuario);
                }
                
                // Primero verificar si tiene el permiso directo
                if (await HasPermisoDirectoAsync(idUsuario, nombrePermiso))
                {
                    return true;
                }
                
                // Si no tiene el permiso directo, verificar si lo tiene a través de sus roles
                var roles = await _usuarioRepository.GetRolesAsync(idUsuario);
                if (!roles.Any())
                {
                    return false;
                }
                
                // Verificar permiso en cada rol
                foreach (var rol in roles)
                {
                    if (await _usuarioRepository.TieneRolPermisoAsync(rol.IdRol, nombrePermiso))
                    {
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex) when (!(ex is NotFoundException))
            {
                _logger.LogError(ex, "Error al verificar si usuario {UserId} tiene permiso {PermisoNombre} (directos+roles)", idUsuario, nombrePermiso);
                throw;
            }
        }
    }
}
