using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.DTOs.Auth;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Domain.Entities;
using DentalPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using ApplicationException = System.ApplicationException;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Servicio encargado de la gestión del registro de usuarios en el sistema
/// </summary>
public class RegistrationService : IRegistrationService
{
    private readonly ApplicationDbContext _context;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IConfiguration _config;
    private readonly Lazy<IAuditService> _auditService;

    public RegistrationService(
        ApplicationDbContext context,
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository,
        IConfiguration config,
        Lazy<IAuditService> auditService)
    {
        _context = context;
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
        _config = config;
        _auditService = auditService;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema
    /// </summary>
    /// <param name="request">Datos del usuario a registrar</param>
    /// <returns>Respuesta con información del usuario creado</returns>
    public async Task<AuthRegisterResponseDto> RegisterAsync(AuthRegisterDto request)
    {
        // Validar que el correo no esté registrado
        if (await EmailExistsAsync(request.Correo))
        {
            throw new BadRequestException(
                $"El correo electrónico {request.Correo} ya está registrado", 
                ErrorCodes.DuplicateEmail);
        }

        // Validar que el consultorio exista
        var consultorio = await _context.Consultorios
            .FirstOrDefaultAsync(c => c.IdConsultorio == request.IdConsultorio);
        
        if (consultorio == null)
        {
            throw new NotFoundException(
                $"No se encontró el consultorio con ID {request.IdConsultorio}", 
                ErrorCodes.ConsultorioNotFound);
        }

        // Crear la entidad de usuario
        var nuevoUsuario = new Usuario
        {
            IdUsuario = Guid.NewGuid(),
            Nombre = request.Nombre,
            Correo = request.Correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            UltimoAcceso = null,
            Telefono = request.Telefono,
            IntentosFallidos = 0,
            BloqueoHasta = null,
            IdConsultorio = request.IdConsultorio
        };

        try
        {
            // Agregar usuario a la base de datos
            await _usuarioRepository.AddAsync(nuevoUsuario);
            
            // Asignar roles
            bool rolesAsignados = false;
            
            // Si se especificaron roles en la solicitud, intentar asignarlos
            if (request.Roles != null && request.Roles.Any())
            {
                foreach (var rolNombre in request.Roles)
                {
                    var rol = await _rolRepository.GetByNombreAsync(rolNombre);
                    if (rol != null)
                    {
                        await _usuarioRepository.AsignarRolPorIdAsync(nuevoUsuario.IdUsuario, rol.IdRol);
                        rolesAsignados = true;
                    }
                }
            }
            
            // Si no se asignaron roles, asignar rol predeterminado
            if (!rolesAsignados)
            {
                await AssignDefaultRoleAsync(nuevoUsuario.IdUsuario);
            }

            // Registrar acción en auditoría
            await _auditService.Value.RegisterActionAsync(
                "UserRegistration",
                "Users",
                nuevoUsuario.IdUsuario,
                nuevoUsuario.IdUsuario,
                System.Text.Json.JsonSerializer.Serialize(new { 
                    Nombre = nuevoUsuario.Nombre, 
                    Correo = nuevoUsuario.Correo,
                    IdConsultorio = nuevoUsuario.IdConsultorio
                }),
                idConsultorio: nuevoUsuario.IdConsultorio
            );

            // Obtener los roles asignados para la respuesta
            var rolesAsignadosObj = await _context.UsuarioRoles
                .Where(ur => ur.IdUsuario == nuevoUsuario.IdUsuario)
                .Include(ur => ur.Rol)
                .Select(ur => ur.Rol!.Nombre)
                .ToListAsync();

            // Crear la respuesta
            return new AuthRegisterResponseDto
            {
                Success = true,
                Message = "Usuario registrado correctamente",
                IdUsuario = nuevoUsuario.IdUsuario,
                Nombre = nuevoUsuario.Nombre,
                Correo = nuevoUsuario.Correo,
                Telefono = nuevoUsuario.Telefono,
                Roles = rolesAsignadosObj
            };
        }
        catch (Exception ex)
        {
            // En caso de error, registrar en auditoría y propagar la excepción
            await _auditService.Value.RegisterActionAsync(
                "UserRegistrationError",
                "Users",
                Guid.Empty,
                Guid.Empty,
                System.Text.Json.JsonSerializer.Serialize(new { 
                    Error = ex.Message, 
                    InnerError = ex.InnerException?.Message
                }),
                idConsultorio: request.IdConsultorio
            );
            
            throw new ApplicationException("Error al registrar el usuario", ex);
        }
    }

    /// <summary>
    /// Verifica si un correo electrónico ya está registrado en el sistema
    /// </summary>
    /// <param name="email">Correo electrónico a verificar</param>
    /// <returns>True si el correo ya existe, False en caso contrario</returns>
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Usuarios.AnyAsync(u => u.Correo == email);
    }

    /// <summary>
    /// Asigna un rol predeterminado a un usuario recién registrado
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <returns>True si se asignó correctamente, False en caso contrario</returns>
    public async Task<bool> AssignDefaultRoleAsync(Guid userId)
    {
        try
        {
            // Obtener el nombre del rol predeterminado desde la configuración o usar un valor por defecto
            string defaultRoleName = _config["Security:DefaultRole"] ?? "Usuario";
            
            // Buscar el rol predeterminado
            var defaultRole = await _rolRepository.GetByNombreAsync(defaultRoleName);
            
            if (defaultRole == null)
            {
                // Si el rol predeterminado no existe, registrar en auditoría y usar el primer rol disponible
                await _auditService.Value.RegisterActionAsync(
                    "DefaultRoleNotFound",
                    "Users",
                    userId,
                    userId,
                    System.Text.Json.JsonSerializer.Serialize(new { DefaultRoleName = defaultRoleName })
                );
                
                defaultRole = await _context.Roles.FirstOrDefaultAsync();
                
                if (defaultRole == null)
                {
                    // Si no hay roles en el sistema, registrar error y retornar false
                    await _auditService.Value.RegisterActionAsync(
                        "NoRolesAvailable",
                        "Users",
                        userId,
                        userId,
                        "No hay roles disponibles para asignar al usuario"
                    );
                    
                    return false;
                }
            }
            
            // Asignar el rol al usuario
            await _usuarioRepository.AsignarRolPorIdAsync(userId, defaultRole.IdRol);
            
            // Registrar asignación de rol en auditoría
            await _auditService.Value.RegisterActionAsync(
                "DefaultRoleAssigned",
                "Users",
                userId,
                userId,
                System.Text.Json.JsonSerializer.Serialize(new { 
                    RolId = defaultRole.IdRol, 
                    RolNombre = defaultRole.Nombre 
                })
            );
            
            return true;
        }
        catch (Exception ex)
        {
            // En caso de error, registrar en auditoría y retornar false
            await _auditService.Value.RegisterActionAsync(
                "DefaultRoleAssignmentError",
                "Users",
                userId,
                userId,
                System.Text.Json.JsonSerializer.Serialize(new { 
                    Error = ex.Message, 
                    InnerError = ex.InnerException?.Message 
                })
            );
            
            return false;
        }
    }
}
