using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace DentalPro.Api.Controllers;

/// <summary>
/// Controlador para diagnóstico y pruebas del sistema.
/// Consolida todas las funcionalidades de validación, diagnóstico y pruebas.
/// </summary>
[ApiController]
[Route("api/diagnostic")]
[ApiExplorerSettings(GroupName = "diagnostico")] // Agrupa estos endpoints en Swagger
public class DiagnosticController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IValidator<AuthLoginDto> _loginValidator;
    private readonly IServiceProvider _serviceProvider;

    public DiagnosticController(
        IConfiguration config, 
        IValidator<AuthLoginDto> loginValidator,
        IServiceProvider serviceProvider)
    {
        _config = config;
        _loginValidator = loginValidator;
        _serviceProvider = serviceProvider;
    }

    #region System Status

    /// <summary>
    /// Verifica que la API esté funcionando correctamente
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public ActionResult HealthCheck()
    {
        return Ok(new { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0"
        });
    }

    #endregion

    #region Validation Tests
    /// <summary>
    /// Endpoint genérico para validar cualquier DTO de la aplicación
    /// </summary>
    /// <remarks>
    /// Ejemplos de uso:
    /// - Para validar AuthLoginDto: POST /api/diagnostic/validation?dtoType=Auth.AuthLoginDto
    /// - Para validar UsuarioCreateDto: POST /api/diagnostic/validation?dtoType=Usuario.UsuarioCreateDto
    /// - Para validar RolUpdateDto: POST /api/diagnostic/validation?dtoType=Rol.RolUpdateDto
    /// </remarks>
    [HttpPost("validation")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateDto([FromBody] JsonElement requestData, [FromQuery] string dtoType)
    {
        if (string.IsNullOrEmpty(dtoType))
        {
            throw new BadRequestException("Debe especificar el tipo de DTO a validar mediante el parámetro dtoType");
        }

        // 1. Determinar el tipo completo del DTO basado en la nomenclatura del proyecto
        Type dtoTypeObj = null;
        
        try
        {     
            // Intentar buscar en diferentes namespaces según la convención del proyecto
            string[] namespacesToTry = {
                $"DentalPro.Application.DTOs.{dtoType}",
                $"DentalPro.Application.DTOs.{dtoType.Split('.')[0]}.{dtoType}"
            };
            
            foreach (var ns in namespacesToTry)
            {
                dtoTypeObj = Type.GetType(ns);
                if (dtoTypeObj != null) break;
            }
            
            // Si no se encontró por el nombre completo, intentar inferir por la última parte
            if (dtoTypeObj == null)
            {
                var assemblyTypes = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                    .Select(Assembly.Load)
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.Namespace != null && t.Namespace.StartsWith("DentalPro.Application.DTOs"));
                    
                string dtoName = dtoType.Contains(".") ? dtoType.Split('.').Last() : dtoType;
                dtoTypeObj = assemblyTypes.FirstOrDefault(t => t.Name.Equals(dtoName, StringComparison.OrdinalIgnoreCase));
            }
            
            if (dtoTypeObj == null)
            {
                throw new BadRequestException($"No se pudo encontrar el tipo de DTO: {dtoType}");
            }

            // 2. Deserializar el objeto JSON al tipo específico
            var dto = JsonSerializer.Deserialize(requestData.GetRawText(), dtoTypeObj, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // 3. Encontrar y resolver el validador apropiado
            // Buscar un validador con el patrón: [Entidad][Operación/Tipo]DtoValidator
            var validatorTypeName = $"{dtoTypeObj.Name}Validator";
            var validatorType = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                .Select(Assembly.Load)
                .SelectMany(a => a.GetTypes())
                .Where(t => t.Name.Equals(validatorTypeName, StringComparison.OrdinalIgnoreCase) && 
                       typeof(IValidator).IsAssignableFrom(t))
                .FirstOrDefault();

            if (validatorType == null)
            {
                throw new BadRequestException($"No se encontró un validador para el tipo {dtoType}");
            }

            // 4. Intentar resolver el validador desde el contenedor DI
            var validatorServiceType = typeof(IValidator<>).MakeGenericType(dtoTypeObj);
            var validator = _serviceProvider.GetService(validatorServiceType);

            if (validator == null)
            {
                throw new BadRequestException($"No se pudo resolver un validador para {dtoType} desde el contenedor de servicios");
            }

            // 5. Ejecutar la validación usando reflection
            var validateMethod = validatorServiceType.GetMethod("ValidateAsync", 
                new[] { dtoTypeObj, typeof(CancellationToken) });

            if (validateMethod == null)
            {
                throw new BadRequestException("No se pudo encontrar el método ValidateAsync en el validador");
            }

            var validationResult = await (Task<FluentValidation.Results.ValidationResult>)validateMethod.Invoke(
                validator, new[] { dto, CancellationToken.None });

            // 6. Procesar el resultado de la validación
            if (!validationResult.IsValid)
            {
                var formErrors = new Dictionary<string, string[]>();
                
                foreach (var error in validationResult.Errors.GroupBy(e => e.PropertyName))
                {
                    var fieldName = error.Key;
                    
                    // Convertir primera letra a minúscula para compatibilidad con Angular
                    if (!string.IsNullOrEmpty(fieldName) && fieldName.Length > 0)
                    {
                        fieldName = char.ToLowerInvariant(fieldName[0]) + 
                            (fieldName.Length > 1 ? fieldName.Substring(1) : "");
                    }
                    
                    formErrors[fieldName] = error.Select(e => e.ErrorMessage).ToArray();
                }
                
                // IMPORTANTE: No envolver la ValidationException en otra excepción
                throw new Application.Common.Exceptions.ValidationException(
                    $"Validación fallida para {dtoType}", 
                    ErrorCodes.ValidationFailed, 
                    formErrors);
            }

            // 7. Devolver resultado exitoso con información del tipo validado
            return Ok(new { 
                message = $"Validación exitosa para {dtoType}",
                validatedType = dtoTypeObj.FullName,
                validator = validatorType.FullName
            });
        }
        catch (Application.Common.Exceptions.ValidationException)
        {
            // Permitir que la excepción de validación se propague directamente al middleware
            throw;
        }
        catch (BadRequestException)
        {
            throw; // Re-lanzar nuestras propias excepciones de BadRequest
        }
        catch (Exception ex)
        {
            // Solo para errores inesperados, no para errores de validación
            throw new BadRequestException($"Error al validar {dtoType}: {ex.Message}");
        }
    }

    #endregion

    #region Authentication & Token Tests

    /// <summary>
    /// Valida y muestra información de un token JWT
    /// </summary>
    [HttpGet("auth/token")]
    [AllowAnonymous]
    public ActionResult ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new BadRequestException("Token no proporcionado");
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
            
            // Solo para propósitos de diagnóstico, intentamos leer el token sin validarlo
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            var claims = jwtToken.Claims.Select(c => new {
                Type = c.Type,
                Value = c.Value
            }).ToList();
            
            var roleClaims = claims.Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").ToList();
            
            // Información de validación del token
            return Ok(new
            {
                ValidUntil = jwtToken.ValidTo,
                Issuer = jwtToken.Issuer,
                Audience = jwtToken.Audiences.FirstOrDefault(),
                Claims = claims,
                RoleClaims = roleClaims,
                TokenConfig = new {
                    ConfiguredIssuer = _config["Jwt:Issuer"],
                    ConfiguredAudience = _config["Jwt:Audience"]
                }
            });
        }
        catch (Exception)
        {
            throw new BadRequestException("Error al validar el token");
        }
    }

    /// <summary>
    /// Muestra los claims del usuario autenticado actual
    /// </summary>
    [HttpGet("auth/claims")]
    [Authorize]
    public ActionResult GetCurrentUserClaims()
    {
        var claims = User.Claims.Select(c => new 
        {
            Type = c.Type,
            Value = c.Value
        }).ToList();
        
        // Verificar específicamente los claims de rol
        var rolesClaims = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        
        // Verificar si el usuario está en el rol de Administrador
        var isAdmin = User.IsInRole("Administrador");
        
        return Ok(new 
        { 
            AllClaims = claims,
            Roles = rolesClaims,
            IsInAdminRole = isAdmin,
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false
        });
    }

    #endregion
}
