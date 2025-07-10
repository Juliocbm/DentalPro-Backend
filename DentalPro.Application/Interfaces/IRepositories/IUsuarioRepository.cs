using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IRepositories;

public interface IUsuarioRepository : IGenericRepository<Usuario>
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task<IEnumerable<Usuario>> GetByConsultorioAsync(Guid idConsultorio);
    Task<IEnumerable<string>> GetUserRolesAsync(Guid idUsuario);
    Task<bool> AsignarRolAsync(Guid idUsuario, string rolNombre);
    Task<bool> RemoverRolAsync(Guid idUsuario, string rolNombre);
    Task<bool> AsignarRolPorIdAsync(Guid idUsuario, Guid idRol);
    Task<bool> RemoverRolPorIdAsync(Guid idUsuario, Guid idRol);
    Task<Usuario?> GetByIdWithRolesAsync(Guid idUsuario);
    
    // Métodos para gestión de refresh tokens
    Task<RefreshToken> AddRefreshTokenAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetRefreshTokenByTokenAsync(string token);
    Task RevokeRefreshTokenAsync(RefreshToken refreshToken);
    Task RevokeAllRefreshTokensAsync(Guid idUsuario);
}
