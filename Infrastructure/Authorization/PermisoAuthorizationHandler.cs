using DentalPro.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DentalPro.Api.Infrastructure.Authorization
{
    /// <summary>
    /// Manejador de autorización que verifica si el usuario tiene un permiso específico
    /// </summary>
    public class PermisoAuthorizationHandler : AuthorizationHandler<PermisoRequirement>
    {
        private readonly IPermisoService _permisoService;
        private readonly ILogger<PermisoAuthorizationHandler> _logger;

        public PermisoAuthorizationHandler(
            IPermisoService permisoService,
            ILogger<PermisoAuthorizationHandler> logger)
        {
            _permisoService = permisoService ?? throw new ArgumentNullException(nameof(permisoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermisoRequirement requirement)
        {
            if (context.User == null || !context.User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Intento de acceso sin autenticación a recurso protegido por permiso: {NombrePermiso}", requirement.NombrePermiso);
                return;
            }

            // Obtener el ID del usuario desde las claims
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("Usuario sin claim de ID intentando acceder a recurso protegido");
                return;
            }

            if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                _logger.LogWarning("ID de usuario inválido en claim: {ClaimValue}", userIdClaim.Value);
                return;
            }

            // Bypass para administradores: siempre tienen todos los permisos
            if (context.User.IsInRole("Administrador"))
            {
                _logger.LogInformation("Usuario administrador accediendo a recurso. Otorgando permiso automáticamente.");
                context.Succeed(requirement);
                return;
            }

            try
            {
                // Verificar si el usuario tiene el permiso específico
                bool hasPermiso = await _permisoService.HasUsuarioPermisoByNameAsync(userId, requirement.NombrePermiso);

                if (hasPermiso)
                {
                    _logger.LogInformation("Usuario {UserId} tiene permiso {NombrePermiso}. Acceso concedido.", userId, requirement.NombrePermiso);
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogWarning("Usuario {UserId} no tiene permiso {NombrePermiso}. Acceso denegado.", userId, requirement.NombrePermiso);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar permiso {NombrePermiso} para usuario {UserId}", requirement.NombrePermiso, userId);
            }
        }
    }
}
