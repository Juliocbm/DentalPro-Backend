using DentalPro.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;

namespace DentalPro.Api.Infrastructure.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SameConsultorioAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        public SameConsultorioAttribute() : base("SameConsultorio")
        {
        }
        
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Obtenemos el logger para diagnóstico
            var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<SameConsultorioAttribute>)) as ILogger<SameConsultorioAttribute>;
            
            // Si no está autenticado, no hacemos nada (deja que el sistema maneje esto)
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                logger?.LogWarning("Usuario no autenticado en SameConsultorioAttribute");
                return;
            }

            // Prueba con autorisación simple: si el usuario es administrador, siempre permitir acceso
            if (context.HttpContext.User.IsInRole("Administrador"))
            {
                logger?.LogInformation("Usuario es Administrador, permitiendo acceso sin validar consultorio");
                return;
            }
            
            // Simplificamos: todo usuario autenticado tiene acceso (temporalmente para probar)
            logger?.LogInformation("Permitiendo acceso a usuario autenticado sin validar consultorio (temporal)");
            return;
        }
    }
}
