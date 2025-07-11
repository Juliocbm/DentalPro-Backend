using AutoMapper;
using BCrypt.Net;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Permissions;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.DTOs.Permiso;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación de la fachada para operaciones de usuarios que delega las operaciones CRUD
/// al servicio especializado UsuarioManagementService y gestiona roles y permisos
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IPermisoRepository _permisoRepository;
    private readonly IPermisoService _permisoService;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UsuarioService> _logger;
    private readonly IUsuarioManagementService _usuarioManagementService;
    private readonly IUsuarioRoleService _usuarioRoleService;
    private readonly IUsuarioPermisoService _usuarioPermisoService;

    public UsuarioService(
        IUsuarioRepository usuarioRepository, 
        IRolRepository rolRepository,
        IPermisoRepository permisoRepository,
        IPermisoService permisoService,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IUsuarioManagementService usuarioManagementService,
        IUsuarioRoleService usuarioRoleService,
        IUsuarioPermisoService usuarioPermisoService,
        ILogger<UsuarioService> logger)
    {
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _rolRepository = rolRepository ?? throw new ArgumentNullException(nameof(rolRepository));
        _permisoRepository = permisoRepository ?? throw new ArgumentNullException(nameof(permisoRepository));
        _permisoService = permisoService ?? throw new ArgumentNullException(nameof(permisoService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _usuarioManagementService = usuarioManagementService ?? throw new ArgumentNullException(nameof(usuarioManagementService));
        _usuarioRoleService = usuarioRoleService ?? throw new ArgumentNullException(nameof(usuarioRoleService));
        _usuarioPermisoService = usuarioPermisoService ?? throw new ArgumentNullException(nameof(usuarioPermisoService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    public async Task<UsuarioDto?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Delegando GetByIdAsync a UsuarioManagementService para ID: {UserId}", id);
        
        // Convertir de Guid a int para el servicio especializado
        var idInt = BitConverter.ToInt32(id.ToByteArray(), 0);
        
        try
        {
            return await _usuarioManagementService.GetByIdAsync(idInt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario con ID {UserId}", id);
            throw; // Re-throw para mantener la pila de excepción
        }
    }

    /// <summary>
    /// Obtiene un usuario por su correo electrónico
    /// </summary>
    public async Task<UsuarioDto?> GetByEmailAsync(string email)
    {
        _logger.LogInformation("Delegando GetByEmailAsync a UsuarioManagementService para email: {Email}", email);
        
        try
        {
            return await _usuarioManagementService.GetByEmailAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario con email {Email}", email);
            throw; // Re-throw para mantener la pila de excepción
        }
    }

    /// <summary>
    /// Obtiene todos los usuarios de un consultorio específico
    /// </summary>
    public async Task<IEnumerable<UsuarioDto>> GetAllByConsultorioAsync(Guid idConsultorio)
    {
        _logger.LogInformation("Delegando GetAllByConsultorioAsync a UsuarioManagementService para consultorio: {ConsultorioId}", idConsultorio);
        
        // Convertir de Guid a int para el servicio especializado
        var consultorioIdInt = BitConverter.ToInt32(idConsultorio.ToByteArray(), 0);
        
        try
        {
            return await _usuarioManagementService.GetByConsultorioIdAsync(consultorioIdInt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios del consultorio {ConsultorioId}", idConsultorio);
            throw; // Re-throw para mantener la pila de excepción
        }
    }
    
    /// <summary>
    /// Métodos de validación para los validadores
    /// </summary>
    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        _logger.LogInformation("Delegando ExistsByIdAsync a UsuarioManagementService para ID: {UserId}", id);
        
        // Convertir de Guid a int para el servicio especializado
        var idInt = BitConverter.ToInt32(id.ToByteArray(), 0);
        
        try
        {
            return await _usuarioManagementService.ExistsByIdAsync(idInt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar existencia de usuario con ID {UserId}", id);
            throw; // Re-throw para mantener la pila de excepción
        }
    }

    /// <summary>
    /// Verifica si existe un usuario con el email especificado
    /// </summary>
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        _logger.LogInformation("Delegando ExistsByEmailAsync a UsuarioManagementService para email: {Email}", email);
        
        try
        {
            return await _usuarioManagementService.ExistsByEmailAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar existencia de usuario con email {Email}", email);
            throw; // Re-throw para mantener la pila de excepción
        }
    }
    
    /// <summary>
    /// Verifica si existe un usuario con el email especificado, excepto el usuario actual
    /// </summary>
    public async Task<bool> ExistsByEmailExceptCurrentAsync(string email, Guid currentId)
    {
        _logger.LogInformation("Delegando ExistsByEmailExceptUserAsync a UsuarioManagementService para email: {Email} y usuario: {UserId}", email, currentId);
        
        // Convertir de Guid a int para el servicio especializado
        var idInt = BitConverter.ToInt32(currentId.ToByteArray(), 0);
        
        try
        {
            return await _usuarioManagementService.ExistsByEmailExceptUserAsync(email, idInt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar existencia de usuario con email {Email} excepto el usuario {UserId}", email, currentId);
            throw; // Re-throw para mantener la pila de excepción
        }
    }

    /// <summary>
    /// Crea un nuevo usuario
    /// </summary>
    public async Task<UsuarioDto> CreateAsync(UsuarioCreateDto usuarioCreateDto)
    {
        _logger.LogInformation("Delegando CreateAsync a UsuarioManagementService para usuario: {Email}", usuarioCreateDto.Correo);
        
        try
        {
            return await _usuarioManagementService.CreateAsync(usuarioCreateDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario con correo {Email}", usuarioCreateDto.Correo);
            throw; // Re-throw para mantener la pila de excepción
        }
    }

    /// <summary>
    /// Actualiza un usuario usando un DTO de actualización con ID incluido
    /// </summary>
    public async Task<UsuarioDto> UpdateAsync(UsuarioUpdateDto usuarioUpdateDto)
    {
        _logger.LogInformation("Delegando UpdateAsync a UsuarioManagementService para ID: {UserId}", usuarioUpdateDto.IdUsuario);
        
        try
        {
            // Convertir de Guid a int para el servicio especializado
            var idInt = BitConverter.ToInt32(usuarioUpdateDto.IdUsuario.ToByteArray(), 0);
            
            // Crear nuevo DTO con ID int para el servicio especializado
            var usuarioUpdateDtoInt = new UsuarioUpdateDto
            {
                Nombre = usuarioUpdateDto.Nombre,
                Correo = usuarioUpdateDto.Correo,
                Activo = usuarioUpdateDto.Activo,
                RolIds = usuarioUpdateDto.RolIds
            };
            
            return await _usuarioManagementService.UpdateAsync(idInt, usuarioUpdateDtoInt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario con ID {UserId}", usuarioUpdateDto.IdUsuario);
            throw; // Re-throw para mantener la pila de excepción
        }
    }
    
    // Método original con ID explícito
    public async Task<UsuarioDto> UpdateAsync(Guid id, UsuarioUpdateDto usuarioUpdateDto)
    {
        _logger.LogInformation("Delegando UpdateAsync con ID explícito a UsuarioManagementService para ID: {UserId}", id);
        
        try
        {
            // Convertir de Guid a int para el servicio especializado
            var idInt = BitConverter.ToInt32(id.ToByteArray(), 0);
            
            // Crear nuevo DTO con ID int para el servicio especializado
            var usuarioUpdateDtoInt = new UsuarioUpdateDto
            {
                Nombre = usuarioUpdateDto.Nombre,
                Correo = usuarioUpdateDto.Correo,
                Activo = usuarioUpdateDto.Activo,
                RolIds = usuarioUpdateDto.RolIds
            };
            
            return await _usuarioManagementService.UpdateAsync(idInt, usuarioUpdateDtoInt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario con ID {UserId}", id);
            throw; // Re-throw para mantener la pila de excepción
        }
    }

    /// <summary>
    /// Elimina un usuario por su ID
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Delegando DeleteAsync a UsuarioManagementService para ID: {UserId}", id);
        
        try
        {
            // Convertir de Guid a int para el servicio especializado
            var idInt = BitConverter.ToInt32(id.ToByteArray(), 0);
            
            return await _usuarioManagementService.DeleteAsync(idInt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario con ID {UserId}", id);
            throw; // Re-throw para mantener la pila de excepción
        }
    }

    /// <summary>
    /// Cambia la contraseña de un usuario
    /// </summary>
    public async Task<bool> ChangePasswordAsync(Guid idUsuario, string currentPassword, string newPassword)
    {
        _logger.LogInformation("Delegando ChangePasswordAsync a UsuarioManagementService para ID: {UserId}", idUsuario);
        
        try
        {
            // Convertir de Guid a int para el servicio especializado
            var idInt = BitConverter.ToInt32(idUsuario.ToByteArray(), 0);
            
            // Crear DTO para cambio de contraseña
            var changePasswordDto = new UsuarioChangePasswordDto
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmNewPassword = newPassword
            };
            
            return await _usuarioManagementService.ChangePasswordAsync(idInt, changePasswordDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar contraseña de usuario con ID {UserId}", idUsuario);
            throw; // Re-throw para mantener la pila de excepción
        }
    }

    /// <summary>
    /// Asigna un rol a un usuario por nombre del rol
    /// </summary>
    public async Task<bool> AsignarRolAsync(Guid idUsuario, string nombreRol)
    {
        _logger.LogInformation("Delegando AsignarRolAsync a UsuarioRoleService para usuario {UserId} y rol {RolNombre}", idUsuario, nombreRol);
        
        try
        {
            return await _usuarioRoleService.AsignarRolAsync(idUsuario, nombreRol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delegando AsignarRolAsync a UsuarioRoleService para usuario {UserId} y rol {RolNombre}", idUsuario, nombreRol);
            throw;
        }
    }

    /// <summary>
    /// Remueve un rol de un usuario por nombre del rol
    /// </summary>
    public async Task<bool> RemoverRolAsync(Guid idUsuario, string nombreRol)
    {
        _logger.LogInformation("Delegando RemoverRolAsync a UsuarioRoleService para usuario {UserId} y rol {RolNombre}", idUsuario, nombreRol);
        
        try
        {
            return await _usuarioRoleService.RemoverRolAsync(idUsuario, nombreRol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delegando RemoverRolAsync a UsuarioRoleService para usuario {UserId} y rol {RolNombre}", idUsuario, nombreRol);
            throw;
        }
    }
    
    /// <summary>
    /// Asigna un rol a un usuario por ID del rol
    /// </summary>
    public async Task<bool> AsignarRolPorIdAsync(Guid idUsuario, Guid idRol)
    {
        _logger.LogInformation("Delegando AsignarRolPorIdAsync a UsuarioRoleService para usuario {UserId} y rol {RolId}", idUsuario, idRol);
        
        try
        {
            return await _usuarioRoleService.AsignarRolPorIdAsync(idUsuario, idRol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delegando AsignarRolPorIdAsync a UsuarioRoleService para usuario {UserId} y rol {RolId}", idUsuario, idRol);
            throw;
        }
    }
    
    /// <summary>
    /// Remueve un rol de un usuario por ID del rol
    /// </summary>
    public async Task<bool> RemoverRolPorIdAsync(Guid idUsuario, Guid idRol)
    {
        _logger.LogInformation("Delegando RemoverRolPorIdAsync a UsuarioRoleService para usuario {UserId} y rol {RolId}", idUsuario, idRol);
        
        try
        {
            return await _usuarioRoleService.RemoverRolPorIdAsync(idUsuario, idRol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delegando RemoverRolPorIdAsync a UsuarioRoleService para usuario {UserId} y rol {RolId}", idUsuario, idRol);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene los roles de un usuario
    /// </summary>
    public async Task<IEnumerable<string>> GetRolesUsuarioAsync(Guid idUsuario)
    {
        _logger.LogInformation("Delegando GetRolesUsuarioAsync a UsuarioRoleService para usuario {UserId}", idUsuario);
        
        try
        {
            return await _usuarioRoleService.GetRolesUsuarioAsync(idUsuario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delegando GetRolesUsuarioAsync a UsuarioRoleService para usuario {UserId}", idUsuario);
            throw;
        }
    }
    
    /// <summary>
    /// Verifica si un usuario tiene un rol específico
    /// </summary>
    public async Task<bool> HasRolAsync(Guid idUsuario, string nombreRol)
    {
        _logger.LogInformation("Delegando HasRolAsync a UsuarioRoleService para usuario {UserId} y rol {RolNombre}", idUsuario, nombreRol);
        
        try
        {
            return await _usuarioRoleService.HasRolAsync(idUsuario, nombreRol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delegando HasRolAsync a UsuarioRoleService para usuario {UserId} y rol {RolNombre}", idUsuario, nombreRol);
            throw;
        }
    }
    
    // Gestión de permisos
    /// <summary>
    /// Asigna un permiso a un usuario por nombre del permiso
    /// </summary>
    public async Task<bool> AsignarPermisoAsync(Guid idUsuario, string nombrePermiso)
    {
        _logger.LogInformation("Delegando AsignarPermisoAsync a UsuarioPermisoService para usuario {UserId} y permiso {PermisoNombre}", 
            idUsuario, nombrePermiso);
        
        try
        {
            return await _usuarioPermisoService.AsignarPermisoAsync(idUsuario, nombrePermiso);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al delegar AsignarPermisoAsync a UsuarioPermisoService para usuario {UserId} y permiso {PermisoNombre}", 
                idUsuario, nombrePermiso);
            throw; // Re-throw para mantener la pila de excepción
        }
    }
    
    /// <summary>
    /// Asigna un permiso a un usuario por ID del permiso
    /// </summary>
    public async Task<bool> AsignarPermisoAsync(Guid idUsuario, Guid idPermiso)
    {
        _logger.LogInformation("Delegando AsignarPermisoAsync por ID a UsuarioPermisoService para usuario {UserId} y permiso {PermisoId}", 
            idUsuario, idPermiso);
            
        try
        {
            return await _usuarioPermisoService.AsignarPermisoAsync(idUsuario, idPermiso);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al delegar AsignarPermisoAsync por ID a UsuarioPermisoService para usuario {UserId} y permiso {PermisoId}", 
                idUsuario, idPermiso);
            throw;
        }
    }
    
    /// <summary>
    /// Remueve un permiso de un usuario por nombre del permiso
    /// </summary>
    public async Task<bool> RemoverPermisoAsync(Guid idUsuario, string nombrePermiso)
    {
        _logger.LogInformation("Delegando RemoverPermisoAsync a UsuarioPermisoService para usuario {UserId} y permiso {PermisoNombre}", 
            idUsuario, nombrePermiso);
        
        try
        {
            return await _usuarioPermisoService.RemoverPermisoAsync(idUsuario, nombrePermiso);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al delegar RemoverPermisoAsync a UsuarioPermisoService para usuario {UserId} y permiso {PermisoNombre}", 
                idUsuario, nombrePermiso);
            throw;
        }
    }
    
    /// <summary>
    /// Remueve un permiso de un usuario por ID del permiso
    /// </summary>
    public async Task<bool> RemoverPermisoAsync(Guid idUsuario, Guid idPermiso)
    {
        _logger.LogInformation("Delegando RemoverPermisoAsync por ID a UsuarioPermisoService para usuario {UserId} y permiso {PermisoId}", 
            idUsuario, idPermiso);
        
        try
        {
            return await _usuarioPermisoService.RemoverPermisoAsync(idUsuario, idPermiso);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al delegar RemoverPermisoAsync por ID a UsuarioPermisoService para usuario {UserId} y permiso {PermisoId}", 
                idUsuario, idPermiso);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene los permisos de un usuario por su ID
    /// </summary>
    public async Task<IEnumerable<string>> GetPermisosAsync(Guid idUsuario)
    {
        _logger.LogInformation("Delegando GetPermisosAsync a UsuarioPermisoService para usuario {UserId}", idUsuario);
        
        try
        {
            return await _usuarioPermisoService.GetPermisosAsync(idUsuario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al delegar GetPermisosAsync a UsuarioPermisoService para usuario {UserId}", idUsuario);
            throw;
        }
    }
    
    // Implementación de los métodos de gestión de múltiples permisos
    
    public async Task<IEnumerable<PermisoDto>> GetPermisosUsuarioAsync(Guid idUsuario)
    {
        _logger.LogInformation("Delegando GetPermisosUsuarioAsync a UsuarioPermisoService para usuario {UserId}", idUsuario);
        
        try
        {
            return await _usuarioPermisoService.GetPermisosUsuarioAsync(idUsuario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al delegar GetPermisosUsuarioAsync a UsuarioPermisoService para usuario {UserId}", idUsuario);
            throw;
        }
    }
    
    public async Task<bool> AsignarPermisosUsuarioAsync(Guid idUsuario, IEnumerable<Guid> idsPermisos)
    {
        _logger.LogInformation("Delegando AsignarPermisosUsuarioAsync a UsuarioPermisoService para usuario {UserId}", idUsuario);
        
        try
        {
            return await _usuarioPermisoService.AsignarPermisosUsuarioAsync(idUsuario, idsPermisos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al delegar AsignarPermisosUsuarioAsync a UsuarioPermisoService para usuario {UserId}", idUsuario);
            throw;
        }
    }
    
    public async Task<bool> AsignarPermisosUsuarioByNombreAsync(Guid idUsuario, IEnumerable<string> nombresPermisos)
    {
        _logger.LogInformation("Delegando AsignarPermisosUsuarioByNombreAsync a UsuarioPermisoService para usuario {UserId}", idUsuario);
        
        try
        {
            return await _usuarioPermisoService.AsignarPermisosUsuarioByNombreAsync(idUsuario, nombresPermisos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al delegar AsignarPermisosUsuarioByNombreAsync a UsuarioPermisoService para usuario {UserId}", idUsuario);
            throw;
        }
    }
    
    /// <summary>
    /// Remueve múltiples permisos de un usuario por sus IDs
    /// </summary>
    public async Task<bool> RemoverPermisosUsuarioAsync(Guid idUsuario, IEnumerable<Guid> idsPermisos)
    {
        _logger.LogInformation("Delegando RemoverPermisosUsuarioAsync a UsuarioPermisoService para usuario {UserId}", idUsuario);
        
        try
        {
            return await _usuarioPermisoService.RemoverPermisosUsuarioAsync(idUsuario, idsPermisos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al delegar RemoverPermisosUsuarioAsync a UsuarioPermisoService para usuario {UserId}", idUsuario);
            throw;
        }
    }
    
    /// <summary>
    /// Remueve múltiples permisos de un usuario por sus nombres
    /// </summary>
    public async Task<bool> RemoverPermisosUsuarioByNombreAsync(Guid idUsuario, IEnumerable<string> nombresPermisos)
    {
        _logger.LogInformation("Delegando RemoverPermisosUsuarioByNombreAsync a UsuarioPermisoService para usuario {UserId}", idUsuario);
        
        try
        {
            return await _usuarioPermisoService.RemoverPermisosUsuarioByNombreAsync(idUsuario, nombresPermisos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al delegar RemoverPermisosUsuarioByNombreAsync a UsuarioPermisoService para usuario {UserId}", idUsuario);
            throw;
        }
    }
}
