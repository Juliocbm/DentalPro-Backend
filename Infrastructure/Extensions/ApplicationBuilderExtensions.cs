using Microsoft.AspNetCore.Builder;
using DentalPro.Api.Infrastructure.Middlewares;

namespace DentalPro.Api.Infrastructure.Extensions;

/// <summary>
/// Extensiones para IApplicationBuilder
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Agrega el middleware de manejo global de excepciones a la aplicación
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
    
    /// <summary>
    /// Agrega el middleware para estandarizar las respuestas de errores de autorización
    /// </summary>
    public static IApplicationBuilder UseStandardizedAuthorizationResponse(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuthorizationResponseMiddleware>();
    }
}
