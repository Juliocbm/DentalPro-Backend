using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces;

public interface IUsuarioService
{
    Task<Usuario?> GetByIdAsync(Guid id);
    Task<Usuario?> GetByEmailAsync(string email);
    Task<IEnumerable<Usuario>> GetAllByConsultorioAsync(Guid idConsultorio);
    // Método original con nombres de roles (mantener para compatibilidad)
    Task<Usuario> CreateAsync(Usuario usuario, string password, List<string> roles);
    
    // Nuevo método que usa IDs de roles en lugar de nombres
    Task<Usuario> CreateAsyncWithRolIds(Usuario usuario, string password, List<Guid> rolIds);
    Task UpdateAsync(Usuario usuario);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ChangePasswordAsync(Guid id, string currentPassword, string newPassword);
    Task<bool> AsignarRolAsync(Guid idUsuario, string nombreRol);
    Task<bool> RemoverRolAsync(Guid idUsuario, string nombreRol);
    
    // Nuevos métodos para trabajar con IDs de rol en lugar de nombres
    Task<bool> AsignarRolPorIdAsync(Guid idUsuario, Guid idRol);
    Task<bool> RemoverRolPorIdAsync(Guid idUsuario, Guid idRol);
}
