using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Interfaces;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;
using BCrypt.Net;
namespace DentalPro.Infrastructure.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;

    public UsuarioService(IUsuarioRepository usuarioRepository, IRolRepository rolRepository)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
    }

    public async Task<Usuario?> GetByIdAsync(Guid id)
    {
        return await _usuarioRepository.GetByIdAsync(id);
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _usuarioRepository.GetByEmailAsync(email);
    }

    public async Task<IEnumerable<Usuario>> GetAllByConsultorioAsync(Guid idConsultorio)
    {
        return await _usuarioRepository.GetByConsultorioAsync(idConsultorio);
    }

    public async Task<Usuario> CreateAsync(Usuario usuario, string password, List<string> roles)
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
    
    public async Task<Usuario> CreateAsyncWithRolIds(Usuario usuario, string password, List<Guid> rolIds)
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

    public async Task UpdateAsync(Usuario usuario)
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
