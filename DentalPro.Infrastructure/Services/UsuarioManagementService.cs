using AutoMapper;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DentalPro.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio para gestión CRUD de usuarios
    /// </summary>
    public class UsuarioManagementService : IUsuarioManagementService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRolRepository _rolRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ILogger<UsuarioManagementService> _logger;
        private readonly IAuditService _auditService;

        public UsuarioManagementService(
            IUsuarioRepository usuarioRepository,
            IRolRepository rolRepository,
            ICurrentUserService currentUserService,
            IMapper mapper,
            ILogger<UsuarioManagementService> logger,
            IAuditService auditService)
        {
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _rolRepository = rolRepository ?? throw new ArgumentNullException(nameof(rolRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        public async Task<UsuarioDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Obteniendo usuario con ID: {UserId}", id);
            
            // Verificar permiso para ver detalles de usuario
            if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.ViewDetail))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó ver detalles de usuario sin el permiso requerido", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorCodes.InsufficientPermissions, ErrorMessages.InsufficientPermissions);
            }

            // Convertir el id int a Guid
            var guidId = new Guid(BitConverter.GetBytes(id).Concat(new byte[12]).ToArray());
            var usuario = await _usuarioRepository.GetByIdAsync(guidId);
            
            if (usuario == null)
            {
                return null;
            }
            
            // Verificar que el usuario pertenezca al mismo consultorio o tenga permiso para ver todos los consultorios
            var currentConsultorioId = _currentUserService.GetCurrentConsultorioId();
            if (usuario.IdConsultorio != currentConsultorioId && 
                !await _currentUserService.HasPermisoAsync(UsuariosPermissions.ViewAllConsultorios))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó ver usuario de otro consultorio", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorCodes.InvalidConsultorio, ErrorMessages.DifferentConsultorio);
            }
            
            return _mapper.Map<UsuarioDto>(usuario);
        }

        /// <summary>
        /// Obtiene un usuario por su correo electrónico
        /// </summary>
        public async Task<UsuarioDto?> GetByEmailAsync(string email)
        {
            _logger.LogInformation("Buscando usuario por correo: {Email}", email);
            
            // Verificar permiso para ver detalles de usuario
            if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.ViewDetail))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó buscar usuario por email sin el permiso requerido", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorCodes.InsufficientPermissions, ErrorMessages.InsufficientPermissions);
            }

            var usuario = await _usuarioRepository.GetByEmailAsync(email);
            
            if (usuario == null)
            {
                return null;
            }
            
            // Verificar que el usuario pertenezca al mismo consultorio
            var currentConsultorioId = _currentUserService.GetCurrentConsultorioId();
            if (usuario.IdConsultorio != currentConsultorioId && 
                !await _currentUserService.HasPermisoAsync(UsuariosPermissions.ViewAllConsultorios))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó ver usuario de otro consultorio", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorCodes.InvalidConsultorio, ErrorMessages.DifferentConsultorio);
            }
            
            return _mapper.Map<UsuarioDto>(usuario);
        }

        /// <summary>
        /// Obtiene todos los usuarios de un consultorio específico
        /// </summary>
        public async Task<IEnumerable<UsuarioDto>> GetByConsultorioIdAsync(int consultorioId)
        {
            _logger.LogInformation("Obteniendo usuarios del consultorio: {ConsultorioId}", consultorioId);
            
            // Verificar permiso para listar usuarios
            if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.ViewAll))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó listar usuarios sin el permiso requerido", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorCodes.InsufficientPermissions, ErrorMessages.InsufficientPermissions);
            }
            
            // Convertir el id int a Guid
            var guidConsultorioId = new Guid(BitConverter.GetBytes(consultorioId).Concat(new byte[12]).ToArray());
            
            // Verificar que sea el consultorio del usuario actual o tenga permisos para ver todos
            var currentConsultorioId = _currentUserService.GetCurrentConsultorioId();
            if (guidConsultorioId != currentConsultorioId && 
                !await _currentUserService.HasPermisoAsync(UsuariosPermissions.ViewAllConsultorios))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó listar usuarios de otro consultorio", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorCodes.InvalidConsultorio, ErrorMessages.DifferentConsultorio);
            }

            var usuarios = await _usuarioRepository.GetByConsultorioAsync(guidConsultorioId);
            return _mapper.Map<IEnumerable<UsuarioDto>>(usuarios);
        }

        /// <summary>
        /// Verifica si existe un usuario con el ID especificado
        /// </summary>
        public async Task<bool> ExistsByIdAsync(int id)
        {
            // Convertir el id int a Guid
            var guidId = new Guid(BitConverter.GetBytes(id).Concat(new byte[12]).ToArray());
            var usuario = await _usuarioRepository.GetByIdAsync(guidId);
            return usuario != null;
        }

        /// <summary>
        /// Verifica si existe un usuario con el email especificado
        /// </summary>
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            var usuario = await _usuarioRepository.GetByEmailAsync(email);
            return usuario != null;
        }

        /// <summary>
        /// Verifica si existe un usuario con el email especificado, excepto el usuario actual
        /// </summary>
        public async Task<bool> ExistsByEmailExceptUserAsync(string email, int userId)
        {
            var guidId = new Guid(BitConverter.GetBytes(userId).Concat(new byte[12]).ToArray());
            var usuario = await _usuarioRepository.GetByEmailAsync(email);
            return usuario != null && usuario.IdUsuario != guidId;
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        public async Task<UsuarioDto> CreateAsync(UsuarioCreateDto usuarioCreateDto)
        {
            _logger.LogInformation("Creando nuevo usuario con correo: {Email}", usuarioCreateDto.Correo);
            
            // Verificar permiso para crear usuarios
            if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.Create))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó crear usuario sin el permiso requerido", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorCodes.InsufficientPermissions, ErrorMessages.InsufficientPermissions);
            }
            
            // Verificar que el email no esté en uso
            if (await ExistsByEmailAsync(usuarioCreateDto.Correo))
            {
                _logger.LogWarning("No se puede crear usuario: el email {Email} ya está en uso", usuarioCreateDto.Correo);
                throw new BadRequestException(ErrorCodes.DuplicateEmail, "El correo electrónico ya está en uso.");
            }

            // Mapear DTO a entidad
            var usuario = _mapper.Map<Usuario>(usuarioCreateDto);
            usuario.IdUsuario = Guid.NewGuid();
            
            // Hash de la contraseña
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuarioCreateDto.Password);
            
            // Crear usuario
            await _usuarioRepository.AddAsync(usuario);
            await _usuarioRepository.SaveChangesAsync();
            
            // Asignar roles si se especificaron
            if (usuarioCreateDto.RolIds != null && usuarioCreateDto.RolIds.Any())
            {
                foreach (var rolId in usuarioCreateDto.RolIds)
                {
                    await _usuarioRepository.AsignarRolPorIdAsync(usuario.IdUsuario, rolId);
                }
            }
            else
            {
                // Buscar el rol "Usuario" para asignarlo por defecto
                var rolUsuario = await _rolRepository.GetByNombreAsync("Usuario");
                
                if (rolUsuario != null)
                {
                    await _usuarioRepository.AsignarRolPorIdAsync(usuario.IdUsuario, rolUsuario.IdRol);
                }
                else
                {
                    _logger.LogWarning("No se encontró el rol de Usuario para asignar por defecto");
                    // Intentar asignar el rol por nombre como fallback
                    await _usuarioRepository.AsignarRolAsync(usuario.IdUsuario, "Usuario");
                }
            }

            // Guardar los cambios de roles
            await _usuarioRepository.SaveChangesAsync();
            
            // Recargar el usuario con sus roles para mapear correctamente
            var usuarioCompleto = await _usuarioRepository.GetByIdWithRolesAsync(usuario.IdUsuario);
            
            // Registrar auditoría
            await _auditService.RegisterActionAsync(
                "Create",
                "Usuario",
                usuario.IdUsuario,
                _currentUserService.GetCurrentUserId(),
                JsonSerializer.Serialize(new { 
                    Email = usuario.Correo,
                    NombreCompleto = usuario.Nombre,
                    FechaCreacion = DateTime.UtcNow,
                    ConsultorioId = usuario.IdConsultorio,
                    Roles = usuarioCreateDto.RolIds?.Count() ?? 1
                })
            );
            
            return _mapper.Map<UsuarioDto>(usuarioCompleto);
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        public async Task<UsuarioDto> UpdateAsync(int id, UsuarioUpdateDto usuarioUpdateDto)
        {
            _logger.LogInformation("Actualizando usuario con ID: {UserId}", id);
            
            // Verificar permiso para actualizar usuarios
            if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.Update))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó actualizar usuario sin el permiso requerido", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorCodes.InsufficientPermissions, ErrorMessages.InsufficientPermissions);
            }

            // Convertir el id int a Guid
            var guidId = new Guid(BitConverter.GetBytes(id).Concat(new byte[12]).ToArray());
            
            // Verificar que el usuario existe
            var existingUser = await _usuarioRepository.GetByIdWithRolesAsync(guidId);
            if (existingUser == null)
            {
                _logger.LogWarning("No se puede actualizar usuario: el usuario con ID {UserId} no existe", id);
                throw new NotFoundException("Usuario", id);
            }
            
            // Verificar que el email no esté en uso por otro usuario
            if (!string.Equals(existingUser.Correo, usuarioUpdateDto.Correo, StringComparison.OrdinalIgnoreCase))
            {
                if (await ExistsByEmailExceptUserAsync(usuarioUpdateDto.Correo, id))
                {
                    _logger.LogWarning("No se puede actualizar usuario: el email {Email} ya está en uso por otro usuario", 
                        usuarioUpdateDto.Correo);
                    throw new BadRequestException(ErrorCodes.DuplicateEmail, "El correo electrónico ya está en uso por otro usuario.");
                }
            }
            
            // Actualizar las propiedades en la entidad existente
            existingUser.Nombre = usuarioUpdateDto.Nombre;
            existingUser.Correo = usuarioUpdateDto.Correo;
            existingUser.Activo = usuarioUpdateDto.Activo;
            existingUser.Telefono = usuarioUpdateDto.Telefono;
            
            // Guardar los cambios en el usuario
            await _usuarioRepository.UpdateAsync(existingUser);
            await _usuarioRepository.SaveChangesAsync();
            
            // Gestionar roles (limpiar y reasignar)
            var currentRoles = existingUser.Roles.ToList();
            
            // Eliminar roles actuales
            foreach (var rolUsuario in currentRoles)
            {
                await _usuarioRepository.RemoverRolPorIdAsync(existingUser.IdUsuario, rolUsuario.IdRol);
            }
            
            // Asignar los nuevos roles
            foreach (var rolId in usuarioUpdateDto.RolIds)
            {
                await _usuarioRepository.AsignarRolPorIdAsync(existingUser.IdUsuario, rolId);
            }
            
            await _usuarioRepository.SaveChangesAsync();
            
            // Recargar el usuario con sus roles actualizados
            var usuarioActualizado = await _usuarioRepository.GetByIdWithRolesAsync(existingUser.IdUsuario);
            
            // Registrar auditoría
            await _auditService.RegisterActionAsync(
                "Update",
                "Usuario",
                existingUser.IdUsuario,
                _currentUserService.GetCurrentUserId(),
                JsonSerializer.Serialize(new { 
                    Original = new {
                        Nombre = existingUser.Nombre,
                        Correo = existingUser.Correo,
                        Telefono = existingUser.Telefono,
                        Activo = existingUser.Activo,
                        RolesCount = currentRoles.Count
                    },
                    Actualizado = new {
                        Nombre = usuarioUpdateDto.Nombre,
                        Correo = usuarioUpdateDto.Correo,
                        Telefono = usuarioUpdateDto.Telefono,
                        Activo = usuarioUpdateDto.Activo,
                        RolesCount = usuarioUpdateDto.RolIds?.Count() ?? 0
                    }
                })
            );
            
            return _mapper.Map<UsuarioDto>(usuarioActualizado);
        }

        /// <summary>
        /// Elimina un usuario por su ID
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Eliminando usuario con ID: {UserId}", id);
            
            // Verificar permiso para eliminar usuarios
            if (!await _currentUserService.HasPermisoAsync(UsuariosPermissions.Delete))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó eliminar usuario sin el permiso requerido", 
                    _currentUserService.GetCurrentUserId());
                throw new ForbiddenAccessException(ErrorCodes.InsufficientPermissions, ErrorMessages.InsufficientPermissions);
            }

            // Convertir el id int a Guid
            var guidId = new Guid(BitConverter.GetBytes(id).Concat(new byte[12]).ToArray());
            
            var usuario = await _usuarioRepository.GetByIdAsync(guidId);
            if (usuario == null)
            {
                _logger.LogWarning("No se puede eliminar usuario: el usuario con ID {UserId} no existe", id);
                return false;
            }
            
            // Verificar que no se esté eliminando al usuario actual
            if (usuario.IdUsuario == _currentUserService.GetCurrentUserId())
            {
                _logger.LogWarning("Un usuario no puede eliminarse a sí mismo: {UserId}", id);
                throw new BadRequestException(ErrorCodes.InvalidOperation, "No puedes eliminar tu propia cuenta de usuario.");
            }

            // Guardar datos del usuario para auditoría antes de eliminarlo
            var userData = new {
                IdUsuario = usuario.IdUsuario,
                Email = usuario.Correo,
                NombreCompleto = usuario.Nombre,
                IdConsultorio = usuario.IdConsultorio,
                FechaCreacion = DateTime.UtcNow,
                Activo = usuario.Activo
            };
            
            // Eliminar usuario
            await _usuarioRepository.RemoveAsync(usuario);
            await _usuarioRepository.SaveChangesAsync();
            
            _logger.LogInformation("Usuario con ID {UserId} eliminado correctamente", id);
            
            // Registrar auditoría
            await _auditService.RegisterActionAsync(
                "Delete",
                "Usuario",
                guidId,
                _currentUserService.GetCurrentUserId(),
                JsonSerializer.Serialize(userData)
            );
            return true;
        }

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int id, UsuarioChangePasswordDto changePasswordDto)
        {
            _logger.LogInformation("Cambiando contraseña para usuario con ID: {UserId}", id);
            
            // Convertir el id int a Guid
            var guidId = new Guid(BitConverter.GetBytes(id).Concat(new byte[12]).ToArray());
            
            // Verificar que el usuario exista
            var usuario = await _usuarioRepository.GetByIdAsync(guidId);
            if (usuario == null)
            {
                _logger.LogWarning("No se puede cambiar contraseña: el usuario con ID {UserId} no existe", id);
                return false;
            }
            
            // Verificar si es el propio usuario o tiene permiso para cambiar contraseñas
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (guidId != currentUserId && !await _currentUserService.HasPermisoAsync(UsuariosPermissions.ChangePassword))
            {
                _logger.LogWarning("Acceso denegado: Usuario {UserId} intentó cambiar contraseña de otro usuario sin el permiso requerido", 
                    currentUserId);
                throw new ForbiddenAccessException(ErrorCodes.InsufficientPermissions, ErrorMessages.InsufficientPermissions);
            }

            // Validar contraseña actual
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, usuario.PasswordHash))
            {
                _logger.LogWarning("Contraseña actual incorrecta para usuario {UserId}", id);
                throw new BadRequestException(ErrorCodes.InvalidCredentials, ErrorMessages.InvalidCredentials);
            }

            // Actualizar contraseña
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            
            await _usuarioRepository.UpdateAsync(usuario);
            await _usuarioRepository.SaveChangesAsync();
            
            _logger.LogInformation("Contraseña cambiada correctamente para usuario {UserId}", id);
            
            // Registrar auditoría (sin incluir contraseñas en los detalles)
            await _auditService.RegisterActionAsync(
                "ChangePassword",
                "Usuario",
                guidId,
                _currentUserService.GetCurrentUserId(),
                JsonSerializer.Serialize(new { 
                    Timestamp = DateTime.UtcNow,
                    ChangedBy = guidId == _currentUserService.GetCurrentUserId() ? "Self" : "Administrator"
                })
            );
            return true;
        }
    }
}
