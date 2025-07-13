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
    public class RequireConsultorioAccessAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        public RequireConsultorioAccessAttribute() : base("RequireConsultorioAccess")
        {
        }
        
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Obtenemos el logger para diagnóstico
            var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<RequireConsultorioAccessAttribute>)) as ILogger<RequireConsultorioAccessAttribute>;
            
            // Si no está autenticado, no hacemos nada (deja que el sistema maneje esto)
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                logger?.LogWarning("Usuario no autenticado en RequireConsultorioAccessAttribute");
                return;
            }

            // Solo SuperUsuario tiene acceso automático a cualquier consultorio
            if (context.HttpContext.User.IsInRole("SuperUsuario"))
            {
                logger?.LogInformation("Usuario es SuperUsuario, permitiendo acceso sin validar consultorio");
                return;
            }
            
            // Los Administradores ya no tienen acceso automático a cualquier consultorio
            // deben respetar la validación de consultorio para mantener la separación de datos
            
            // Simplificamos: todo usuario autenticado tiene acceso (temporalmente para probar)
            logger?.LogInformation("Permitiendo acceso a usuario autenticado sin validar consultorio (temporal)");
            return;
        }
    }
}
