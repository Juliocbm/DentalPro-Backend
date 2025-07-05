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
            throw new Exception("El correo electrónico ya está registrado");
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

    public async Task<bool> UpdateAsync(Usuario usuario)
    {
        var existingUser = await _usuarioRepository.GetByIdAsync(usuario.IdUsuario);
        if (existingUser == null)
        {
            return false;
        }

        // Validar que el correo no exista para otro usuario
        var emailUser = await _usuarioRepository.GetByEmailAsync(usuario.Correo);
        if (emailUser != null && emailUser.IdUsuario != usuario.IdUsuario)
        {
            throw new Exception("El correo electrónico ya está en uso por otro usuario");
        }

        // Mantener el mismo hash de contraseña
        usuario.PasswordHash = existingUser.PasswordHash;
        
        await _usuarioRepository.UpdateAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();
        
        return true;
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
            throw new Exception("La contraseña actual es incorrecta");
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
}
