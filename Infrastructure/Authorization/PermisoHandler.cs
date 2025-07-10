using DentalPro.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DentalPro.Api.Infrastructure.Authorization;

/// <summary>
/// Handler para procesar requisitos de autorización basados en permisos
/// </summary>
public class PermisoHandler : AuthorizationHandler<PermisoRequirement>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PermisoHandler> _logger;

    public PermisoHandler(
        ICurrentUserService currentUserService,
        ILogger<PermisoHandler> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermisoRequirement requirement)
    {
        if (context.User == null || !context.User.Identity.IsAuthenticated)
        {
            _logger.LogWarning("Acceso denegado: El usuario no está autenticado");
            return;
        }

        try
        {
            var hasPermiso = await _currentUserService.HasPermisoAsync(requirement.NombrePermiso);
            
            if (hasPermiso)
            {
                _logger.LogInformation("Permiso {Permiso} verificado para usuario", requirement.NombrePermiso);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("Acceso denegado: Usuario no tiene permiso {Permiso}", requirement.NombrePermiso);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar permiso {Permiso}", requirement.NombrePermiso);
        }
    }
}
