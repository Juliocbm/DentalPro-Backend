using AutoMapper;
using BCrypt.Net;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.DTOs.Permiso;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace DentalPro.Infrastructure.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IPermisoRepository _permisoRepository;
    private readonly IPermisoService _permisoService;
    private readonly IMapper _mapper;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(
        IUsuarioRepository usuarioRepository, 
        IRolRepository rolRepository,
        IPermisoRepository permisoRepository,
        IPermisoService permisoService,
        IMapper mapper,
        ILogger<UsuarioService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
        _permisoRepository = permisoRepository;
        _permisoService = permisoService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UsuarioDto?> GetByIdAsync(Guid id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        return usuario != null ? _mapper.Map<UsuarioDto>(usuario) : null;
    }

    public async Task<UsuarioDto?> GetByEmailAsync(string email)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(email);
        return usuario != null ? _mapper.Map<UsuarioDto>(usuario) : null;
    }

    public async Task<IEnumerable<UsuarioDto>> GetAllByConsultorioAsync(Guid idConsultorio)
    {
        var usuarios = await _usuarioRepository.GetByConsultorioAsync(idConsultorio);
        return _mapper.Map<IEnumerable<UsuarioDto>>(usuarios);
    }
    
    // Métodos de validación para los validadores
    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _usuarioRepository.GetByIdAsync(id) != null;
    }
    
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _usuarioRepository.GetByEmailAsync(email) != null;
    }
    
    public async Task<bool> ExistsByEmailExceptCurrentAsync(string email, Guid currentId)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(email);
        return usuario != null && usuario.IdUsuario != currentId;
    }

    // Nuevo método estandarizado con DTO
    public async Task<UsuarioDto> CreateAsync(UsuarioCreateDto usuarioCreateDto)
    {
        _logger.LogInformation("Creando nuevo usuario con correo: {Email}", usuarioCreateDto.Correo);
        
        // Mapear DTO a entidad
        var usuario = _mapper.Map<Usuario>(usuarioCreateDto);
        usuario.IdUsuario = Guid.NewGuid();
        
        // Hash de la contraseña
        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuarioCreateDto.Password);
        
        // Crear usuario
        await _usuarioRepository.AddAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();
        
        // Asignar roles
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
                // Si no existe el rol Usuario, usar el método original
                await _usuarioRepository.AsignarRolAsync(usuario.IdUsuario, "Usuario");
            }
        }

        // Guardar los cambios de roles
        await _usuarioRepository.SaveChangesAsync();
        
        // Recargar el usuario con sus roles para mapear correctamente
        var usuarioCompleto = await _usuarioRepository.GetByIdAsync(usuario.IdUsuario);
        
        return _mapper.Map<UsuarioDto>(usuarioCompleto);
    }

    // Nuevo método estandarizado con DTO
    public async Task<UsuarioDto> UpdateAsync(UsuarioUpdateDto usuarioUpdateDto)
    {
        _logger.LogInformation("Actualizando usuario con ID: {UserId}", usuarioUpdateDto.IdUsuario);
        
        // Verificar que el usuario existe
        var existingUser = await _usuarioRepository.GetByIdAsync(usuarioUpdateDto.IdUsuario);
        if (existingUser == null)
        {
            throw new NotFoundException("Usuario", usuarioUpdateDto.IdUsuario);
        }
        
        // Actualizar las propiedades directamente en la entidad existente
        // en lugar de crear una nueva instancia
        existingUser.Nombre = usuarioUpdateDto.Nombre;
        existingUser.Correo = usuarioUpdateDto.Correo;
        existingUser.Activo = usuarioUpdateDto.Activo;
        
        // Guardar los cambios en el usuario
        await _usuarioRepository.SaveChangesAsync();
        
        // Gestionar roles (limpiar y reasignar)
        // Primero obtenemos los roles actuales del usuario
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
        var usuarioActualizado = await _usuarioRepository.GetByIdAsync(existingUser.IdUsuario);
        return _mapper.Map<UsuarioDto>(usuarioActualizado);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
        {
            return false;
        }

        await _usuarioRepository.RemoveAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid id, string currentPassword, string newPassword)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
        {
            return false;
        }

        // Validar contraseña actual
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, usuario.PasswordHash))
        {
            throw new BadRequestException("La contraseña actual es incorrecta", ErrorCodes.InvalidCredentials);
        }

        // Actualizar contraseña
        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        
        await _usuarioRepository.UpdateAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> AsignarRolAsync(Guid idUsuario, string nombreRol)
    {
        return await _usuarioRepository.AsignarRolAsync(idUsuario, nombreRol);
    }

    public async Task<bool> RemoverRolAsync(Guid idUsuario, string nombreRol)
    {
        return await _usuarioRepository.RemoverRolAsync(idUsuario, nombreRol);
    }
    
    public async Task<bool> AsignarRolPorIdAsync(Guid idUsuario, Guid idRol)
    {
        return await _usuarioRepository.AsignarRolPorIdAsync(idUsuario, idRol);
    }
    
    public async Task<bool> RemoverRolPorIdAsync(Guid idUsuario, Guid idRol)
    {
        return await _usuarioRepository.RemoverRolPorIdAsync(idUsuario, idRol);
    }
    
    // Gestión de permisos
    public async Task<bool> AsignarPermisoAsync(Guid idUsuario, string nombrePermiso)
    {
        _logger.LogInformation("Asignando permiso {PermisoNombre} a usuario {UserId}", nombrePermiso, idUsuario);
        
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
            _logger.LogWarning("No se puede asignar permiso: {PermisoNombre} no existe", nombrePermiso);
            throw new NotFoundException("Permiso", nombrePermiso);
        }
        
        // Asignar el permiso
        var result = await _usuarioRepository.AsignarPermisoAsync(idUsuario, permiso.IdPermiso);
        await _usuarioRepository.SaveChangesAsync();
        
        return result;
    }
    
    public async Task<bool> AsignarPermisoAsync(Guid idUsuario, Guid idPermiso)
    {
        _logger.LogInformation("Asignando permiso con ID {PermisoId} a usuario {UserId}", idPermiso, idUsuario);
        
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
    
    public async Task<bool> RemoverPermisoAsync(Guid idUsuario, string nombrePermiso)
    {
        _logger.LogInformation("Removiendo permiso {PermisoNombre} de usuario {UserId}", nombrePermiso, idUsuario);
        
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
            _logger.LogWarning("No se puede remover permiso: {PermisoNombre} no existe", nombrePermiso);
            throw new NotFoundException("Permiso", nombrePermiso);
        }
        
        // Remover el permiso
        var result = await _usuarioRepository.RemoverPermisoAsync(idUsuario, permiso.IdPermiso);
        await _usuarioRepository.SaveChangesAsync();
        
        return result;
    }
    
    public async Task<bool> RemoverPermisoAsync(Guid idUsuario, Guid idPermiso)
    {
        _logger.LogInformation("Removiendo permiso con ID {PermisoId} de usuario {UserId}", idPermiso, idUsuario);
        
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
    
    public async Task<IEnumerable<string>> GetPermisosAsync(Guid idUsuario)
    {
        _logger.LogInformation("Obteniendo permisos del usuario {UserId}", idUsuario);
        
        // Verificar que el usuario existe
        var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
        if (usuario == null)
        {
            _logger.LogWarning("No se pueden obtener permisos: usuario {UserId} no existe", idUsuario);
            throw new NotFoundException("Usuario", idUsuario);
        }
        
        // Obtener permisos directos del usuario
        var permisos = await _usuarioRepository.GetUserPermisosAsync(idUsuario);
        
        return permisos;
    }
    
    // Implementación de los métodos de gestión de múltiples permisos
    
    public async Task<IEnumerable<PermisoDto>> GetPermisosUsuarioAsync(Guid idUsuario)
    {
        _logger.LogInformation("Obteniendo permisos detallados del usuario {UserId}", idUsuario);
        
        // Verificar que el usuario existe
        var usuario = await _usuarioRepository.GetByIdAsync(idUsuario);
        if (usuario == null)
        {
            _logger.LogWarning("No se pueden obtener permisos detallados: usuario {UserId} no existe", idUsuario);
            throw new NotFoundException("Usuario", idUsuario);
        }
        
        // Obtener objetos Permiso completos del usuario
        var permisos = await _permisoService.GetPermisosByUsuarioIdAsync(idUsuario);
        
        // Mapear a DTOs
        return _mapper.Map<IEnumerable<PermisoDto>>(permisos);
    }
    
    public async Task<bool> AsignarPermisosUsuarioAsync(Guid idUsuario, IEnumerable<Guid> idsPermisos)
    {
        _logger.LogInformation("Asignando múltiples permisos a usuario {UserId}", idUsuario);
        
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
    
    public async Task<bool> AsignarPermisosUsuarioByNombreAsync(Guid idUsuario, IEnumerable<string> nombresPermisos)
    {
        _logger.LogInformation("Asignando múltiples permisos por nombre a usuario {UserId}", idUsuario);
        
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
    
    public async Task<bool> RemoverPermisosUsuarioAsync(Guid idUsuario, IEnumerable<Guid> idsPermisos)
    {
        _logger.LogInformation("Removiendo múltiples permisos de usuario {UserId}", idUsuario);
        
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
    
    public async Task<bool> RemoverPermisosUsuarioByNombreAsync(Guid idUsuario, IEnumerable<string> nombresPermisos)
    {
        _logger.LogInformation("Removiendo múltiples permisos por nombre de usuario {UserId}", idUsuario);
        
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
}
