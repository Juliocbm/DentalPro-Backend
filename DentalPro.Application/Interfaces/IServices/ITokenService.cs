using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Interfaz para el servicio especializado en la generación y validación de tokens
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Genera un token de acceso (JWT) para un usuario
    /// </summary>
    /// <param name="user">Usuario para el que se genera el token</param>
    /// <param name="expiracion">Fecha de expiración del token</param>
    /// <returns>Token JWT serializado</returns>
    string GenerateAccessToken(Usuario user, DateTime expiracion);
    
    /// <summary>
    /// Genera un refresh token y lo guarda en la base de datos
    /// </summary>
    /// <param name="userId">ID del usuario para el que se genera el token</param>
    /// <returns>Entidad RefreshToken creada</returns>
    Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId);
    
    /// <summary>
    /// Valida un token de acceso
    /// </summary>
    /// <param name="token">Token a validar</param>
    /// <returns>True si el token es válido, False en caso contrario</returns>
    bool ValidateAccessToken(string token);
    
    /// <summary>
    /// Obtiene la fecha de expiración para un nuevo token de acceso
    /// </summary>
    /// <returns>Fecha y hora de expiración</returns>
    DateTime GetAccessTokenExpiration();
    
    /// <summary>
    /// Obtiene la fecha de expiración para un nuevo refresh token
    /// </summary>
    /// <returns>Fecha y hora de expiración</returns>
    DateTime GetRefreshTokenExpiration();
}
