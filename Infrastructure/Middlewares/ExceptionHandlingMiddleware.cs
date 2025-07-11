using System.Net;
using System.Text.Json;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DentalPro.Api.Infrastructure.Middlewares;

/// <summary>
/// Middleware para manejar excepciones de forma centralizada
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no manejado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";
        
        var errorResponse = new ErrorResponse
        {
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            // Casos específicos primero (orden importante: de más específico a más general)
            case ValidationException validationEx:
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.ErrorCode = validationEx.ErrorCode ?? ErrorCodes.ValidationFailed;
                errorResponse.Message = validationEx.Message;
                errorResponse.Error = "Validation Failed"; // Para compatibilidad con Angular
                errorResponse.ValidationErrors = validationEx.ValidationErrors;
                
                // Usar directamente el FormErrors de ValidationException
                if (validationEx.FormErrors?.Any() == true)
                {
                    errorResponse.FormErrors = validationEx.FormErrors;
                }
                else if (validationEx.ValidationErrors?.Any() == true)
                {
                    // Mantener compatibilidad hacia atrás si no hay FormErrors
                    var errorsGroupedByProperty = validationEx.ValidationErrors
                        .GroupBy(e => e.Property)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.Message).ToArray());
                    
                    errorResponse.FormErrors = errorsGroupedByProperty;
                }
                break;
                
            case NotFoundException notFoundEx:
                errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.ErrorCode = ErrorCodes.ResourceNotFound;
                errorResponse.Message = notFoundEx.Message;
                errorResponse.Error = "Not Found"; // Para compatibilidad con Angular
                break;
            
            case ForbiddenAccessException forbiddenEx:
                errorResponse.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.ErrorCode = ErrorCodes.ResourceAccessDenied;
                errorResponse.Message = forbiddenEx.Message;
                errorResponse.Error = "Forbidden"; // Para compatibilidad con Angular
                break;
            
            case BadRequestException badRequestEx:
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.ErrorCode = ErrorCodes.InvalidModelState;
                errorResponse.Message = badRequestEx.Message;
                errorResponse.Error = "Bad Request"; // Para compatibilidad con Angular
                break;
            
            case TooManyRequestsException tooManyEx:
                errorResponse.StatusCode = (int)HttpStatusCode.TooManyRequests; // 429
                errorResponse.ErrorCode = ErrorCodes.RateLimitExceeded;
                errorResponse.Message = tooManyEx.Message;
                errorResponse.Error = "Too Many Requests"; // Para compatibilidad con Angular
                break;
            
            // Caso base para cualquier ApplicationException que no sea un tipo específico arriba
            case Application.Common.Exceptions.ApplicationException appEx:
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.ErrorCode = appEx.ErrorCode;
                errorResponse.Message = appEx.Message;
                errorResponse.Error = "Bad Request"; // Para compatibilidad con Angular
                break;
            
            // Caso por defecto para cualquier otra excepción no controlada
            default:
                errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.ErrorCode = ErrorCodes.GeneralError;
                errorResponse.Message = ErrorMessages.DefaultError;
                errorResponse.Error = "Server Error"; // Para compatibilidad con Angular
                break;
        }

        // Solo incluir detalles del error en entorno de desarrollo
        if (_environment.IsDevelopment())
        {
            errorResponse.Details = exception.ToString();
        }

        response.StatusCode = errorResponse.StatusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(errorResponse, options);
        await response.WriteAsync(json);
    }
}
