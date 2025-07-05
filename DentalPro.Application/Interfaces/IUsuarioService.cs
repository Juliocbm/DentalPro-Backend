using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces;

public interface IUsuarioService
{
    Task<Usuario?> GetByIdAsync(Guid id);
    Task<Usuario?> GetByEmailAsync(string email);
    Task<IEnumerable<Usuario>> GetAllByConsultorioAsync(Guid idConsultorio);
    Task<Usuario> CreateAsync(Usuario usuario, string password, List<string> roles);
    Task<bool> UpdateAsync(Usuario usuario);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ChangePasswordAsync(Guid id, string currentPassword, string newPassword);
    Task<bool> AsignarRolAsync(Guid idUsuario, string nombreRol);
    Task<bool> RemoverRolAsync(Guid idUsuario, string nombreRol);
}
