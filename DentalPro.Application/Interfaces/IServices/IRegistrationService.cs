using DentalPro.Application.DTOs.Auth;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Interfaz para el servicio especializado en registro de usuarios
/// </summary>
public interface IRegistrationService
{
    /// <summary>
    /// Registra un nuevo usuario en el sistema
    /// </summary>
    /// <param name="request">Datos del usuario a registrar</param>
    /// <returns>Respuesta con información del usuario creado</returns>
    Task<AuthRegisterResponseDto> RegisterAsync(AuthRegisterDto request);
    
    /// <summary>
    /// Verifica si un correo electrónico ya está registrado en el sistema
    /// </summary>
    /// <param name="email">Correo electrónico a verificar</param>
    /// <returns>True si el correo ya existe, False en caso contrario</returns>
    Task<bool> EmailExistsAsync(string email);
    
    /// <summary>
    /// Asigna un rol predeterminado a un usuario recién registrado
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <returns>True si se asignó correctamente, False en caso contrario</returns>
    Task<bool> AssignDefaultRoleAsync(Guid userId);
}
