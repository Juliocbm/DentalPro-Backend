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
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<PermisoAuthorizationHandler> _logger;

        public PermisoAuthorizationHandler(
            ICurrentUserService currentUserService,
            ILogger<PermisoAuthorizationHandler> logger)
        {
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
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

            // Bypass para SuperUsuarios: siempre tienen todos los permisos
            if (context.User.IsInRole("SuperUsuario"))
            {
                _logger.LogInformation("SuperUsuario accediendo a recurso. Otorgando permiso automáticamente.");
                context.Succeed(requirement);
                return;
            }
            
            // Los Administradores ya no tienen bypass automático, deben tener el permiso específico
            // para mantener el principio de menor privilegio y la separación por consultorio

            try
            {
                // Verificar si el usuario tiene el permiso específico usando CurrentUserService
                bool hasPermiso = await _currentUserService.HasPermisoAsync(requirement.NombrePermiso);

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
