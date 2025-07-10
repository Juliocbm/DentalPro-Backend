using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IRepositories;

public interface IUsuarioRepository : IGenericRepository<Usuario>
{
    // Métodos de consulta
    Task<Usuario?> GetByEmailAsync(string email);
    Task<IEnumerable<Usuario>> GetByConsultorioAsync(Guid idConsultorio);
    Task<IEnumerable<Usuario>> GetUsuariosByRolIdAsync(Guid idRol);
    Task<Usuario?> GetByIdWithRolesAsync(Guid idUsuario);
    Task<Usuario?> GetByIdWithPermisosAsync(Guid idUsuario);
    Task<Usuario?> GetByIdWithRolesAndPermisosAsync(Guid idUsuario);
    
    // Gestión de roles
    Task<IEnumerable<string>> GetUserRolesAsync(Guid idUsuario);
    Task<bool> AsignarRolAsync(Guid idUsuario, string rolNombre);
    Task<bool> RemoverRolAsync(Guid idUsuario, string rolNombre);
    Task<bool> AsignarRolPorIdAsync(Guid idUsuario, Guid idRol);
    Task<bool> RemoverRolPorIdAsync(Guid idUsuario, Guid idRol);
    
    // Gestión de permisos directos del usuario
    Task<IEnumerable<string>> GetUserPermisosAsync(Guid idUsuario);
    Task<bool> AsignarPermisoAsync(Guid idUsuario, string nombrePermiso);
    Task<bool> RemoverPermisoAsync(Guid idUsuario, string nombrePermiso);
    Task<bool> AsignarPermisoAsync(Guid idUsuario, Guid idPermiso);
    Task<bool> RemoverPermisoAsync(Guid idUsuario, Guid idPermiso);
    Task<bool> TienePermisoDirectoAsync(Guid idUsuario, string nombrePermiso);
    
    // Métodos para gestión de refresh tokens
    Task<RefreshToken> AddRefreshTokenAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetRefreshTokenByTokenAsync(string token);
    Task RevokeRefreshTokenAsync(RefreshToken refreshToken);
    Task RevokeAllRefreshTokensAsync(Guid idUsuario);
}
