using AutoMapper;
using BCrypt.Net;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Logging;
namespace DentalPro.Infrastructure.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(
        IUsuarioRepository usuarioRepository, 
        IRolRepository rolRepository,
        IMapper mapper,
        ILogger<UsuarioService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
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

    // Método legacy renombrado para compatibilidad
    public async Task<Usuario> CreateLegacyAsync(Usuario usuario, string password, List<string> roles)
    {
        // Validar que el correo no exista
        var existingUser = await _usuarioRepository.GetByEmailAsync(usuario.Correo);
        if (existingUser != null)
        {
            throw new BadRequestException("El correo electrónico ya está registrado", ErrorCodes.DuplicateEmail);
        }

        // Hash de la contraseña
        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

        // Generar ID si no existe
        if (usuario.IdUsuario == Guid.Empty)
        {
            usuario.IdUsuario = Guid.NewGuid();
        }

        // Crear usuario
        var result = await _usuarioRepository.AddAsync(usuario);
        
        // Asignar roles
        if (roles != null && roles.Any())
        {
            foreach (var rolNombre in roles)
            {
                await _usuarioRepository.AsignarRolAsync(usuario.IdUsuario, rolNombre);
            }
        }
        else
        {
            // Rol por defecto
            await _usuarioRepository.AsignarRolAsync(usuario.IdUsuario, "Usuario");
        }

        await _usuarioRepository.SaveChangesAsync();
        
        return result;
    }
    
    // Método legacy renombrado para compatibilidad
    public async Task<Usuario> CreateLegacyWithRolIdsAsync(Usuario usuario, string password, List<Guid> rolIds)
    {
        // Validar que el correo no exista
        var existingUser = await _usuarioRepository.GetByEmailAsync(usuario.Correo);
        if (existingUser != null)
        {
            throw new BadRequestException("El correo electrónico ya está registrado", ErrorCodes.DuplicateEmail);
        }

        // Hash de la contraseña
        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

        // Generar ID si no existe
        if (usuario.IdUsuario == Guid.Empty)
        {
            usuario.IdUsuario = Guid.NewGuid();
        }

        // Crear usuario
        var result = await _usuarioRepository.AddAsync(usuario);
        
        // IMPORTANTE: Guardar el usuario en la base de datos ANTES de asignar roles
        await _usuarioRepository.SaveChangesAsync();
        
        // Asignar roles por ID
        if (rolIds != null && rolIds.Any())
        {
            foreach (var rolId in rolIds)
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
        
        return result;
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

    // Método legacy renombrado para compatibilidad
    public async Task UpdateLegacyAsync(Usuario usuario)
    {
        //Posiblemente innecesario, la posibilidad de condicion de carrera es bajo
        var existingUser = await _usuarioRepository.GetByIdAsync(usuario.IdUsuario);
        if (existingUser == null)
        {
            throw new NotFoundException("Usuario", usuario.IdUsuario);
        }

        // Validar que el correo no exista para otro usuario
        var emailUser = await _usuarioRepository.GetByEmailAsync(usuario.Correo);
        if (emailUser != null && emailUser.IdUsuario != usuario.IdUsuario)
        {
            throw new BadRequestException("El correo electrónico ya está en uso por otro usuario", ErrorCodes.DuplicateEmail);
        }

        // Mantener el mismo hash de contraseña
        usuario.PasswordHash = existingUser.PasswordHash;
        
        await _usuarioRepository.UpdateAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();
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
}
