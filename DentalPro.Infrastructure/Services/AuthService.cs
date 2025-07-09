using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.DTOs.Auth;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Domain.Entities;
using DentalPro.Infrastructure.Persistence;
using DentalPro.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using DentalPro.Application.Interfaces.IServices;

namespace DentalPro.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    
    // Constantes para tokens
    private readonly TimeSpan _accessTokenDuration;
    private readonly TimeSpan _refreshTokenDuration;

    public AuthService(ApplicationDbContext context, IConfiguration config, 
        IUsuarioRepository usuarioRepository, IRolRepository rolRepository)
    {
        _context = context;
        _config = config;
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;

        // Configuración de duración de tokens (valores por defecto si no están configurados)
        _accessTokenDuration = TimeSpan.FromMinutes(double.TryParse(_config["Jwt:AccessTokenDurationMinutes"], out double accessTokenMinutes) 
            ? accessTokenMinutes : 30);
        _refreshTokenDuration = TimeSpan.FromDays(double.TryParse(_config["Jwt:RefreshTokenDurationDays"], out double refreshTokenDays) 
            ? refreshTokenDays : 7);
    }

    public async Task<AuthLoginResponseDto> LoginAsync(AuthLoginDto request)
    {
        var user = await _context.Usuarios
            .Include(u => u.Roles)
            .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Correo == request.Correo && u.Activo);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new BadRequestException(ErrorMessages.InvalidCredentials, ErrorCodes.InvalidCredentials);

        // Revocar todos los tokens de refresco existentes para este usuario como medida de seguridad
        await _usuarioRepository.RevokeAllRefreshTokensAsync(user.IdUsuario);
        
        // Generar tokens nuevos
        var tokenExpiracion = DateTime.UtcNow.Add(_accessTokenDuration);
        var accessToken = GenerateAccessToken(user, tokenExpiracion);
        var refreshToken = await GenerateRefreshTokenAsync(user.IdUsuario);

        return new AuthLoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            TokenExpiracion = tokenExpiracion,
            Nombre = user.Nombre,
            IdUsuario = user.IdUsuario,
            IdConsultorio = user.IdConsultorio,
            Roles = user.Roles.Select(r => r.Rol!.Nombre).ToList()
        };
    }

    public async Task<AuthRegisterResponseDto> RegisterAsync(AuthRegisterDto request)
    {
        if (request.Password != request.ConfirmPassword)
        {
            throw new BadRequestException("Las contraseñas no coinciden", ErrorCodes.ValidationFailed);
        }

        // Verificar que el email no exista ya
        var existingUser = await _usuarioRepository.GetByEmailAsync(request.Correo);
        if (existingUser != null)
        {
            throw new BadRequestException("El correo ya está registrado", ErrorCodes.DuplicateEmail);
        }

        // Verificar que el consultorio exista
        var consultorio = await _context.Consultorios.FindAsync(request.IdConsultorio);
        if (consultorio == null)
        {
            throw new NotFoundException("Consultorio", request.IdConsultorio);
        }

        // Crear el nuevo usuario
        var newUser = new Usuario
        {
            IdUsuario = Guid.NewGuid(),
            Nombre = request.Nombre,
            Correo = request.Correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IdConsultorio = request.IdConsultorio,
            Activo = true
        };

        // Agregar el usuario a la base de datos
        await _usuarioRepository.AddAsync(newUser);
        
        // IMPORTANTE: Guardar el usuario en la base de datos antes de asignarle roles
        await _usuarioRepository.SaveChangesAsync();

        // Asignar roles (ahora el usuario ya existe en la base de datos)
        if (request.Roles != null && request.Roles.Any())
        {
            foreach (var rolNombre in request.Roles)
            {
                await _usuarioRepository.AsignarRolAsync(newUser.IdUsuario, rolNombre);
            }
        }
        else
        {
            // Asignar rol por defecto si no se especifica
            await _usuarioRepository.AsignarRolAsync(newUser.IdUsuario, "Usuario");
        }
        
        // No es necesario llamar a SaveChangesAsync nuevamente aquí, ya que AsignarRolAsync ya guarda los cambios

        // Obtener los roles asignados
        var userRoles = await _usuarioRepository.GetUserRolesAsync(newUser.IdUsuario);

        return new AuthRegisterResponseDto
        {
            Success = true,
            Message = "Usuario registrado correctamente",
            IdUsuario = newUser.IdUsuario,
            Nombre = newUser.Nombre,
            Correo = newUser.Correo,
            Roles = userRoles.ToList()
        };
    }
    
    public async Task<AuthLoginResponseDto> RefreshTokenAsync(AuthRefreshTokenDto request)
    {
        // Validar el refresh token
        var refreshToken = await _usuarioRepository.GetRefreshTokenByTokenAsync(request.RefreshToken);
        if (refreshToken == null)
        {
            throw new BadRequestException("Token de refresco inválido o expirado", ErrorCodes.InvalidToken);
        }

        // Obtener el usuario asociado al token
        var user = await _context.Usuarios
            .Include(u => u.Roles)
            .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.IdUsuario == refreshToken.IdUsuario && u.Activo);

        if (user == null)
        {
            // Revocar el token si el usuario no existe o está inactivo
            await _usuarioRepository.RevokeRefreshTokenAsync(refreshToken);
            throw new BadRequestException("Usuario no encontrado o inactivo", ErrorCodes.InvalidToken);
        }

        // Revocar el token actual
        await _usuarioRepository.RevokeRefreshTokenAsync(refreshToken);

        // Generar nuevos tokens
        var tokenExpiracion = DateTime.UtcNow.Add(_accessTokenDuration);
        var accessToken = GenerateAccessToken(user, tokenExpiracion);
        var newRefreshToken = await GenerateRefreshTokenAsync(user.IdUsuario);

        return new AuthLoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            TokenExpiracion = tokenExpiracion,
            Nombre = user.Nombre,
            IdUsuario = user.IdUsuario,
            IdConsultorio = user.IdConsultorio,
            Roles = user.Roles.Select(r => r.Rol!.Nombre).ToList()
        };
    }
    
    public async Task RevokeTokenAsync(string refreshToken)
    {
        var token = await _usuarioRepository.GetRefreshTokenByTokenAsync(refreshToken);
        if (token == null)
        {
            // Si no encontramos el token o ya está revocado, no hay acción necesaria
            return;
        }
        
        await _usuarioRepository.RevokeRefreshTokenAsync(token);
    }

    #region Métodos privados para manejo de tokens
    
    private string GenerateAccessToken(Usuario user, DateTime expiracion)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.IdUsuario.ToString()),
            new Claim(ClaimTypes.Name, user.Nombre),
            new Claim("IdConsultorio", user.IdConsultorio.ToString())
        };

        foreach (var rol in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, rol.Rol!.Nombre));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiracion,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId)
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
            FechaExpiracion = DateTime.UtcNow.Add(_refreshTokenDuration),
            IdUsuario = userId,
            EstaRevocado = false
        };
        
        // Guardar el token en la base de datos
        return await _usuarioRepository.AddRefreshTokenAsync(refreshToken);
    }
    
    #endregion
}
