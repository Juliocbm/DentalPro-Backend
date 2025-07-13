using DentalPro.Application.DTOs.Auth;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Interfaz para el servicio especializado en autenticación de usuarios
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Inicia sesión en el sistema
    /// </summary>
    /// <param name="request">Credenciales de inicio de sesión</param>
    /// <returns>Respuesta con tokens de autenticación</returns>
    Task<AuthLoginResponseDto> LoginAsync(AuthLoginDto request);
    
    /// <summary>
    /// Actualiza un token de acceso usando un refresh token
    /// </summary>
    /// <param name="request">Refresh token actual</param>
    /// <returns>Nuevo par de tokens de autenticación</returns>
    Task<AuthLoginResponseDto> RefreshTokenAsync(AuthRefreshTokenDto request);
    
    /// <summary>
    /// Revoca un refresh token, invalidando la sesión del usuario
    /// </summary>
    /// <param name="refreshToken">Token a revocar</param>
    Task RevokeTokenAsync(string refreshToken);
}
