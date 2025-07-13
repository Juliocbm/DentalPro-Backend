using System.Net;
using System.Text.Json;
using System.Security.Claims;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace DentalPro.Api.Infrastructure.Middlewares;

/// <summary>
/// Middleware para interceptar y estandarizar las respuestas de errores de autorización
/// </summary>
public class AuthorizationResponseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizationResponseMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public AuthorizationResponseMiddleware(
        RequestDelegate next, 
        ILogger<AuthorizationResponseMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Guarda una referencia al método original para poder reemplazarlo
        var originalBodyStream = context.Response.Body;

        try
        {
            // Continuar con el pipeline
            await _next(context);

            // Verificar si la respuesta es un error de autorización
            if (context.Response.StatusCode == 401 || context.Response.StatusCode == 403)
            {
                // Si el contenido ya ha sido escrito, no podemos cambiarlo
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("No se pudo estandarizar respuesta de autorización porque ya se había iniciado la escritura");
                    return;
                }

                await TransformAuthorizationResponseAsync(context);
            }
        }
        catch
        {
            // Cualquier excepción aquí será manejada por ExceptionHandlingMiddleware
            throw;
        }
    }

    private async Task TransformAuthorizationResponseAsync(HttpContext context)
    {
        // Crear una respuesta estandarizada
        var errorResponse = new ErrorResponse
        {
            Timestamp = DateTime.UtcNow
        };

        // Configurar según el tipo de error de autorización
        if (context.Response.StatusCode == 401)
        {
            errorResponse.StatusCode = StatusCodes.Status401Unauthorized;
            errorResponse.ErrorCode = ErrorCodes.Unauthorized;
            errorResponse.Message = ErrorMessages.UnauthorizedAccess;
            errorResponse.Error = "Unauthorized";
            
            _logger.LogWarning("Usuario no autenticado intentó acceder a un recurso protegido");
        }
        else // 403
        {
            errorResponse.StatusCode = StatusCodes.Status403Forbidden;
            errorResponse.ErrorCode = ErrorCodes.InsufficientPermissions;
            errorResponse.Message = ErrorMessages.InsufficientPermissions;
            errorResponse.Error = "Forbidden";
            
            _logger.LogWarning("Usuario autenticado {UserId} intentó acceder a un recurso sin permisos suficientes", 
                context.User.FindFirst("sub")?.Value ?? "desconocido");
        }
        
        // Solo incluir detalles de diagnóstico en entorno de desarrollo
        if (_environment.IsDevelopment())
        {
            // Crear información de diagnóstico útil
            var diagnosticInfo = new
            {
                Path = context.Request.Path.Value,
                Method = context.Request.Method,
                StatusCode = context.Response.StatusCode,
                Endpoint = context.GetEndpoint()?.DisplayName,
                UserIdentity = GetUserIdentityInfo(context),
                AuthenticationType = context.User?.Identity?.AuthenticationType ?? "None",
                AuthorizationHeaders = context.Request.Headers.ContainsKey("Authorization") 
                    ? "Present" : "Not present",
                TimestampUtc = DateTime.UtcNow.ToString("o")
            };
            
            errorResponse.Details = JsonSerializer.Serialize(diagnosticInfo, new JsonSerializerOptions {
                WriteIndented = true
            });
        }

        // Serializar la respuesta
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(errorResponse, options);

        // Configurar la respuesta
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = errorResponse.StatusCode;
        
        // Escribir la respuesta
        await context.Response.WriteAsync(json);
    }
    
    /// <summary>
    /// Extrae información útil de las claims del usuario para diagnóstico
    /// </summary>
    private object GetUserIdentityInfo(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return new { IsAuthenticated = false };
        }
        
        try
        {
            var claims = context.User.Claims.ToDictionary(
                c => c.Type,
                c => c.Value
            );
            
            // Devolver info relevante para diagnóstico de autorización
            return new
            {
                IsAuthenticated = true,
                Username = context.User.Identity.Name,
                UserId = claims.ContainsKey("sub") ? claims["sub"] : null,
                Email = claims.ContainsKey("email") ? claims["email"] : null,
                Roles = context.User.Claims
                    .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                    .Select(c => c.Value)
                    .ToList(),
                Claims = claims,
                // Intentar obtener info de consultorio
                ConsultorioId = claims.ContainsKey("idConsultorio") ? claims["idConsultorio"] : null
            };
        }
        catch (Exception ex)
        {
            // En caso de error, retornar info básica
            return new 
            { 
                IsAuthenticated = true,
                Error = "Error extracting user info: " + ex.Message
            };
        }
    }
}
