using DentalPro.Application.DTOs.Usuario;
using DentalPro.Application.DTOs.Permiso;
using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IServices;

public interface IUsuarioService
{
    // Métodos de consulta
    Task<UsuarioDto?> GetByIdAsync(Guid id);
    Task<UsuarioDto?> GetByEmailAsync(string email);
    Task<IEnumerable<UsuarioDto>> GetAllByConsultorioAsync(Guid idConsultorio);
    
    // Métodos CRUD con DTOs estandarizados
    Task<UsuarioDto> CreateAsync(UsuarioCreateDto usuarioCreateDto);
    Task<UsuarioDto> UpdateAsync(UsuarioUpdateDto usuarioUpdateDto);
    Task<bool> DeleteAsync(Guid id);
    
    // Gestión de contraseñas
    Task<bool> ChangePasswordAsync(Guid id, string currentPassword, string newPassword);
    
    // Gestión de roles
    Task<bool> AsignarRolAsync(Guid idUsuario, string nombreRol);
    Task<bool> RemoverRolAsync(Guid idUsuario, string nombreRol);
    Task<bool> AsignarRolPorIdAsync(Guid idUsuario, Guid idRol);
    Task<bool> RemoverRolPorIdAsync(Guid idUsuario, Guid idRol);
    Task<IEnumerable<string>> GetRolesUsuarioAsync(Guid idUsuario);
    Task<bool> HasRolAsync(Guid idUsuario, string nombreRol);
    
    // Gestión de permisos
    Task<bool> AsignarPermisoAsync(Guid idUsuario, string nombrePermiso);
    Task<bool> AsignarPermisoAsync(Guid idUsuario, Guid idPermiso);
    Task<bool> RemoverPermisoAsync(Guid idUsuario, string nombrePermiso);
    Task<bool> RemoverPermisoAsync(Guid idUsuario, Guid idPermiso);
    Task<IEnumerable<string>> GetPermisosAsync(Guid idUsuario);
    
    // Gestión de múltiples permisos
    Task<bool> AsignarPermisosUsuarioAsync(Guid idUsuario, IEnumerable<Guid> idsPermisos);
    Task<bool> AsignarPermisosUsuarioByNombreAsync(Guid idUsuario, IEnumerable<string> nombresPermisos);
    Task<bool> RemoverPermisosUsuarioAsync(Guid idUsuario, IEnumerable<Guid> idsPermisos);
    Task<bool> RemoverPermisosUsuarioByNombreAsync(Guid idUsuario, IEnumerable<string> nombresPermisos);
    Task<IEnumerable<PermisoDto>> GetPermisosUsuarioAsync(Guid idUsuario);
    
    // Métodos de validación para los validadores
    Task<bool> ExistsByIdAsync(Guid id);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByEmailExceptCurrentAsync(string email, Guid currentId);
}
