using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Servicio encargado de la generación y validación de tokens JWT y refresh tokens
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<TokenService> _logger;

    // Constantes para tokens
    private readonly TimeSpan _accessTokenDuration;
    private readonly TimeSpan _refreshTokenDuration;
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;

    public TokenService(
        IConfiguration config,
        IUsuarioRepository usuarioRepository,
        ILogger<TokenService> logger)
    {
        _config = config;
        _usuarioRepository = usuarioRepository;
        _logger = logger;

        // Configuración de duración de tokens (valores por defecto si no están configurados)
        _accessTokenDuration = TimeSpan.FromMinutes(double.TryParse(_config["Jwt:AccessTokenDurationMinutes"], out double accessTokenMinutes)
            ? accessTokenMinutes : 30);
        _refreshTokenDuration = TimeSpan.FromDays(double.TryParse(_config["Jwt:RefreshTokenDurationDays"], out double refreshTokenDays)
            ? refreshTokenDays : 7);

        // Configuración para la creación de JWT
        _jwtKey = _config["Jwt:Key"] ?? throw new ArgumentNullException("La clave JWT no está configurada");
        _jwtIssuer = _config["Jwt:Issuer"] ?? "DentalProApi";
        _jwtAudience = _config["Jwt:Audience"] ?? "DentalProClients";
    }

    /// <summary>
    /// Genera un token de acceso JWT para el usuario especificado
    /// </summary>
    public string GenerateAccessToken(Usuario user, DateTime expiracion)
    {
        try
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim("IdConsultorio", user.IdConsultorio.ToString())
            };

            // Agregar roles como claims
            if (user.Roles != null)
            {
                foreach (var rol in user.Roles.Where(r => r.Rol != null))
                {
                    claims.Add(new Claim(ClaimTypes.Role, rol.Rol!.Nombre));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: expiracion,
                signingCredentials: creds
            );

            _logger.LogInformation("Token de acceso generado para usuario {UserId} con expiración {Expiration}", 
                user.IdUsuario, expiracion);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar token de acceso para usuario {UserId}", user.IdUsuario);
            throw;
        }
    }

    /// <summary>
    /// Genera un refresh token para el usuario especificado y lo guarda en la base de datos
    /// </summary>
    public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId)
    {
        try
        {
            // Generar un token aleatorio seguro
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var token = Convert.ToBase64String(randomBytes);
            
            // Crear la entidad RefreshToken
            var refreshToken = new RefreshToken
            {
                Token = token,
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = GetRefreshTokenExpiration(),
                IdUsuario = userId,
                EstaRevocado = false
            };
            
            // Guardar el token en la base de datos
            var savedToken = await _usuarioRepository.AddRefreshTokenAsync(refreshToken);
            
            _logger.LogInformation("Refresh token generado para usuario {UserId} con expiración {Expiration}", 
                userId, refreshToken.FechaExpiracion);
                
            return savedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar refresh token para usuario {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Valida un token de acceso
    /// </summary>
    public bool ValidateAccessToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtKey);
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = true,
                ValidAudience = _jwtAudience,
                // Comprobar que el token no ha expirado
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            
            return validatedToken != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al validar token de acceso");
            return false;
        }
    }

    /// <summary>
    /// Obtiene la fecha de expiración para un nuevo token de acceso
    /// </summary>
    public DateTime GetAccessTokenExpiration()
    {
        return DateTime.UtcNow.Add(_accessTokenDuration);
    }

    /// <summary>
    /// Obtiene la fecha de expiración para un nuevo refresh token
    /// </summary>
    public DateTime GetRefreshTokenExpiration()
    {
        return DateTime.UtcNow.Add(_refreshTokenDuration);
    }
}
