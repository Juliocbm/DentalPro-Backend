using System;
using System.Threading.Tasks;
using DentalPro.Application.Common.Constants;
using DentalPro.Application.Common.Exceptions;
using DentalPro.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DentalPro.Api.Infrastructure.Filters
{
    /// <summary>
    /// Filtro de acción para aplicar limitación de solicitudes por IP/Usuario
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RateLimitingAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _operation;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="operation">Nombre de la operación a limitar (ej: "login", "register")</param>
        public RateLimitingAttribute(string operation)
        {
            _operation = operation;
        }
        
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var rateLimitingService = context.HttpContext.RequestServices.GetRequiredService<RateLimitingService>();
            
            // Obtener IP del cliente como clave para limitar
            string clientIp = GetClientIpAddress(context);
            
            // Verificar si la solicitud está permitida
            if (!await rateLimitingService.IsAllowedAsync(clientIp, _operation))
            {
                // Lanzar excepción que será manejada por el middleware global
                throw new TooManyRequestsException(ErrorMessages.TooManyRequests);
            }
            
            // Si la solicitud está permitida, continuar con la ejecución
            var resultContext = await next();
            
            // Si la operación fue exitosa (código 2xx), reiniciar contador
            if (resultContext.Result is ObjectResult objectResult && objectResult.StatusCode >= 200 && objectResult.StatusCode < 300)
            {
                rateLimitingService.Reset(clientIp, _operation);
            }
        }
        
        private string GetClientIpAddress(ActionExecutingContext context)
        {
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            
            // Si está detrás de un proxy, intentar obtener la IP real
            var forwardedHeader = context.HttpContext.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                // El formato puede ser múltiples IPs separadas por comas
                ipAddress = forwardedHeader.Split(',')[0].Trim();
            }
            
            return ipAddress ?? "Unknown";
        }
    }
}
