using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.DTOs.Auth;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio especializado en autenticación de usuarios
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IAuditService _auditService;
    private readonly IConfiguration _config;

    // Constantes para bloqueo de cuentas
    private readonly int _maxLoginAttempts;
    private readonly int _lockoutDurationMinutes;

    public AuthenticationService(
        IUsuarioRepository usuarioRepository,
        ITokenService tokenService,
        ILogger<AuthenticationService> logger,
        IAuditService auditService,
        IConfiguration config)
    {
        _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        // Configuración de bloqueo de cuentas
        _maxLoginAttempts = int.TryParse(_config["Auth:MaxLoginAttempts"], out int attempts) 
            ? attempts : 5;
        _lockoutDurationMinutes = int.TryParse(_config["Auth:LockoutDurationMinutes"], out int duration) 
            ? duration : 30;
    }

    /// <summary>
    /// Inicia sesión en el sistema verificando credenciales y generando tokens
    /// </summary>
    public async Task<AuthLoginResponseDto> LoginAsync(AuthLoginDto request)
    {
        if (request == null) 
            throw new BadRequestException(ErrorMessages.InvalidCredentials, ErrorCodes.InvalidCredentials);

        _logger.LogInformation("Intento de autenticación para usuario {Email}", request.Email);
        
        // 1. Buscar el usuario por email
        var user = await _usuarioRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Intento de autenticación fallido: usuario {Email} no encontrado", request.Email);
            await _auditService.LogSecurityEventAsync("Login", "Failure", $"Usuario no encontrado: {request.Email}");
            throw new BadRequestException(ErrorMessages.InvalidCredentials, ErrorCodes.InvalidCredentials);
        }
        
        // 2. Verificar si el usuario está activo
        if (!user.Activo)
        {
            _logger.LogWarning("Intento de autenticación fallido: usuario {Email} inactivo", request.Email);
            await _auditService.LogSecurityEventAsync("Login", "Failure", $"Cuenta inactiva: {request.Email}");
            throw new BadRequestException(ErrorMessages.AccountDisabled, ErrorCodes.AccountDisabled);
        }
        
        // 3. Verificar si la cuenta está bloqueada
        if (user.FechaBloqueo.HasValue && user.FechaBloqueo.Value > DateTime.UtcNow)
        {
            var tiempoRestante = user.FechaBloqueo.Value - DateTime.UtcNow;
            _logger.LogWarning("Intento de autenticación fallido: usuario {Email} bloqueado por {Minutes} minutos", 
                request.Email, tiempoRestante.TotalMinutes);
            await _auditService.LogSecurityEventAsync("Login", "Failure", $"Cuenta bloqueada: {request.Email}");
            throw new BadRequestException(string.Format(ErrorMessages.AccountLocked, Math.Ceiling(tiempoRestante.TotalMinutes)),
                ErrorCodes.AccountLocked);
        }
        
        // 4. Verificar la contraseña
        bool isValidPassword = VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt);
        
        if (!isValidPassword)
        {
            // Incrementar contador de intentos fallidos
            user.IntentosLogin = (user.IntentosLogin ?? 0) + 1;
            
            // Verificar si se debe bloquear la cuenta
            if (user.IntentosLogin >= _maxLoginAttempts)
            {
                user.FechaBloqueo = DateTime.UtcNow.AddMinutes(_lockoutDurationMinutes);
                _logger.LogWarning("Cuenta bloqueada para usuario {Email} por {Duration} minutos", 
                    request.Email, _lockoutDurationMinutes);
            }
            
            await _usuarioRepository.UpdateLoginAttemptsAsync(user.IdUsuario, user.IntentosLogin, user.FechaBloqueo);
            
            _logger.LogWarning("Intento de autenticación fallido: contraseña incorrecta para {Email}. Intentos: {Attempts}", 
                request.Email, user.IntentosLogin);
            await _auditService.LogSecurityEventAsync("Login", "Failure", $"Contraseña incorrecta: {request.Email}");
            
            throw new BadRequestException(ErrorMessages.InvalidCredentials, ErrorCodes.InvalidCredentials);
        }
        
        // 5. Autenticación exitosa: restablecer intentos de inicio de sesión
        if (user.IntentosLogin > 0 || user.FechaBloqueo.HasValue)
        {
            user.IntentosLogin = 0;
            user.FechaBloqueo = null;
            await _usuarioRepository.UpdateLoginAttemptsAsync(user.IdUsuario, 0, null);
        }
        
        // 6. Generar tokens de autenticación
        var accessTokenExpiration = _tokenService.GetAccessTokenExpiration();
        var accessToken = _tokenService.GenerateAccessToken(user, accessTokenExpiration);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.IdUsuario);
        
        // 7. Registrar acceso exitoso
        _logger.LogInformation("Autenticación exitosa para usuario {Email} (ID: {UserId})", 
            request.Email, user.IdUsuario);
        await _auditService.LogSecurityEventAsync("Login", "Success", 
            $"Autenticación exitosa: {request.Email} (ID: {user.IdUsuario})");
        
        // 8. Retornar respuesta con tokens
        return new AuthLoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            Expiracion = accessTokenExpiration,
            IdUsuario = user.IdUsuario,
            Nombre = user.Nombre,
            Email = user.Email,
            Roles = user.Roles?.Select(r => r.Rol?.Nombre).Where(r => r != null).ToList() ?? new List<string>(),
            IdConsultorio = user.IdConsultorio
        };
    }

    /// <summary>
    /// Actualiza un token de acceso usando un refresh token
    /// </summary>
    public async Task<AuthLoginResponseDto> RefreshTokenAsync(AuthRefreshTokenDto request)
    {
        if (request == null || string.IsNullOrEmpty(request.RefreshToken))
            throw new BadRequestException(ErrorMessages.InvalidToken, ErrorCodes.InvalidToken);
            
        _logger.LogInformation("Solicitud de renovación de token recibida");
        
        // 1. Buscar el refresh token en la base de datos
        var storedToken = await _usuarioRepository.GetRefreshTokenAsync(request.RefreshToken);
        if (storedToken == null)
        {
            _logger.LogWarning("Intento de renovación con token inexistente");
            await _auditService.LogSecurityEventAsync("RefreshToken", "Failure", "Token no encontrado");
            throw new BadRequestException(ErrorMessages.InvalidToken, ErrorCodes.InvalidToken);
        }
        
        // 2. Verificar que el token no esté expirado o revocado
        if (storedToken.EstaRevocado)
        {
            _logger.LogWarning("Intento de renovación con token revocado para usuario {UserId}", storedToken.IdUsuario);
            await _auditService.LogSecurityEventAsync("RefreshToken", "Failure", 
                $"Token revocado para usuario {storedToken.IdUsuario}");
            throw new BadRequestException(ErrorMessages.RevokedToken, ErrorCodes.RevokedToken);
        }
        
        if (storedToken.FechaExpiracion < DateTime.UtcNow)
        {
            _logger.LogWarning("Intento de renovación con token expirado para usuario {UserId}", storedToken.IdUsuario);
            await _auditService.LogSecurityEventAsync("RefreshToken", "Failure", 
                $"Token expirado para usuario {storedToken.IdUsuario}");
            throw new BadRequestException(ErrorMessages.ExpiredToken, ErrorCodes.ExpiredToken);
        }
        
        // 3. Obtener el usuario asociado al token
        var user = await _usuarioRepository.GetByIdAsync(storedToken.IdUsuario);
        if (user == null)
        {
            _logger.LogWarning("Usuario no encontrado para token válido, ID: {UserId}", storedToken.IdUsuario);
            await _auditService.LogSecurityEventAsync("RefreshToken", "Failure", 
                $"Usuario no encontrado: {storedToken.IdUsuario}");
            throw new NotFoundException(ErrorMessages.UserNotFound, ErrorCodes.UserNotFound);
        }
        
        // 4. Verificar que el usuario esté activo
        if (!user.Activo)
        {
            _logger.LogWarning("Intento de renovación para usuario inactivo {UserId}", user.IdUsuario);
            await _auditService.LogSecurityEventAsync("RefreshToken", "Failure", 
                $"Usuario inactivo: {user.IdUsuario}");
            throw new BadRequestException(ErrorMessages.AccountDisabled, ErrorCodes.AccountDisabled);
        }
        
        // 5. Generar nuevos tokens
        var accessTokenExpiration = _tokenService.GetAccessTokenExpiration();
        var accessToken = _tokenService.GenerateAccessToken(user, accessTokenExpiration);
        
        // 6. Registrar la renovación exitosa
        _logger.LogInformation("Renovación de token exitosa para usuario {UserId}", user.IdUsuario);
        await _auditService.LogSecurityEventAsync("RefreshToken", "Success", 
            $"Renovación exitosa para usuario {user.IdUsuario}");
        
        // 7. Devolver los tokens (se mantiene el refresh token existente)
        return new AuthLoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = storedToken.Token,
            Expiracion = accessTokenExpiration,
            IdUsuario = user.IdUsuario,
            Nombre = user.Nombre,
            Email = user.Email,
            Roles = user.Roles?.Select(r => r.Rol?.Nombre).Where(r => r != null).ToList() ?? new List<string>(),
            IdConsultorio = user.IdConsultorio
        };
    }

    /// <summary>
    /// Revoca un refresh token, invalidando la sesión del usuario
    /// </summary>
    public async Task RevokeTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new BadRequestException(ErrorMessages.InvalidToken, ErrorCodes.InvalidToken);
            
        _logger.LogInformation("Solicitud de revocación de token recibida");
        
        // 1. Buscar el token en la base de datos
        var storedToken = await _usuarioRepository.GetRefreshTokenAsync(refreshToken);
        if (storedToken == null)
        {
            _logger.LogWarning("Intento de revocación con token inexistente");
            return; // No error needed, token doesn't exist anyway
        }
        
        // 2. Verificar si ya está revocado
        if (storedToken.EstaRevocado)
        {
            _logger.LogInformation("Token ya revocado para usuario {UserId}", storedToken.IdUsuario);
            return; // Already revoked, nothing to do
        }
        
        // 3. Revocar el token
        storedToken.EstaRevocado = true;
        storedToken.FechaRevocacion = DateTime.UtcNow;
        await _usuarioRepository.UpdateRefreshTokenAsync(storedToken);
        
        // 4. Registrar la revocación
        _logger.LogInformation("Token revocado para usuario {UserId}", storedToken.IdUsuario);
        await _auditService.LogSecurityEventAsync("RevokeToken", "Success", 
            $"Token revocado para usuario {storedToken.IdUsuario}");
    }
    
    /// <summary>
    /// Verifica que la contraseña proporcionada coincida con el hash almacenado
    /// </summary>
    /// <param name="password">Contraseña a verificar</param>
    /// <param name="storedHash">Hash almacenado en la base de datos</param>
    /// <param name="storedSalt">Salt almacenado en la base de datos</param>
    /// <returns>True si la contraseña es correcta, false en caso contrario</returns>
    private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        if (storedHash == null || storedHash.Length == 0 || storedSalt == null || storedSalt.Length == 0)
            return false;
            
        if (string.IsNullOrEmpty(password))
            return false;
            
        try
        {
            using var hmac = new HMACSHA512(storedSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            
            // Comparar el hash calculado con el hash almacenado
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != storedHash[i])
                    return false;
            }
            
            return true;
        }
        catch
        {
            // En caso de error al verificar la contraseña, consideramos que es inválida
            _logger.LogError("Error verificando hash de contraseña");
            return false;
        }
    }
}
