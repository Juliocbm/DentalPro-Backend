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

    public AuthService(ApplicationDbContext context, IConfiguration config, 
        IUsuarioRepository usuarioRepository, IRolRepository rolRepository)
    {
        _context = context;
        _config = config;
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Usuarios
            .Include(u => u.Roles)
            .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Correo == request.Correo && u.Activo);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new BadRequestException(ErrorMessages.InvalidCredentials, ErrorCodes.InvalidCredentials);

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
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Nombre = user.Nombre,
            IdUsuario = user.IdUsuario,
            IdConsultorio = user.IdConsultorio,
            Roles = user.Roles.Select(r => r.Rol!.Nombre).ToList()
        };
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
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

        return new RegisterResponse
        {
            Success = true,
            Message = "Usuario registrado exitosamente",
            IdUsuario = newUser.IdUsuario,
            Nombre = newUser.Nombre,
            Correo = newUser.Correo,
            Roles = userRoles.ToList()
        };
    }
}
