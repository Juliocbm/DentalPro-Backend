using DentalPro.Application.DTOs.Usuario;
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
    
    // Métodos de validación para los validadores
    Task<bool> ExistsByIdAsync(Guid id);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByEmailExceptCurrentAsync(string email, Guid currentId);
}
