using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace DentalPro.Api.Infrastructure.Authorization;

/// <summary>
/// Atributo personalizado que valida que un usuario solo pueda acceder a recursos de su propio consultorio
/// </summary>
public class ConsultorioAccessAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IAuthorizationService _authorizationService;
    private readonly Guid? _resourceConsultorioId;

    public ConsultorioAccessAuthorizationFilter(IAuthorizationService authorizationService, Guid? resourceConsultorioId = null)
    {
        _authorizationService = authorizationService;
        _resourceConsultorioId = resourceConsultorioId;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userConsultorioIdClaim = user.FindFirst("IdConsultorio");
        if (userConsultorioIdClaim == null || !Guid.TryParse(userConsultorioIdClaim.Value, out var userConsultorioId))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Si se especificó un ID de consultorio específico para el recurso, usarlo
        // De lo contrario, intentar obtenerlo de los parámetros de la ruta
        Guid resourceConsultorioId;
        
        if (_resourceConsultorioId.HasValue)
        {
            resourceConsultorioId = _resourceConsultorioId.Value;
        }
        else
        {
            // Intentar obtener el ID del consultorio de los parámetros de la ruta o del cuerpo
            if (!TryGetConsultorioIdFromContext(context, out resourceConsultorioId))
            {
                // Si no podemos determinar el ID del consultorio, permitimos acceso (será responsabilidad del controlador validar)
                return;
            }
        }

        // Validar que el usuario pertenece al mismo consultorio que el recurso
        var authorizationResult = await _authorizationService
            .AuthorizeAsync(user, resourceConsultorioId, "ConsultorioAccess");

        if (!authorizationResult.Succeeded)
        {
            context.Result = new ForbidResult();
        }
    }

    private bool TryGetConsultorioIdFromContext(AuthorizationFilterContext context, out Guid consultorioId)
    {
        // Primero, intentar obtenerlo de la ruta
        if (context.RouteData.Values.ContainsKey("idConsultorio") &&
            Guid.TryParse(context.RouteData.Values["idConsultorio"]?.ToString(), out consultorioId))
        {
            return true;
        }

        // Luego, intentar obtenerlo de los parámetros de la acción
        var actionParameters = context.ActionDescriptor.Parameters;
        foreach (var param in actionParameters)
        {
            if (param.Name == "idConsultorio")
            {
                var value = context.HttpContext.Request.Query["idConsultorio"].FirstOrDefault();
                if (value != null && Guid.TryParse(value, out consultorioId))
                {
                    return true;
                }
            }
        }

        consultorioId = Guid.Empty;
        return false;
    }
}

/// <summary>
/// Atributo para aplicar la política de acceso a consultorio
/// </summary>
public class RequireConsultorioAccessAttribute : TypeFilterAttribute
{
    public RequireConsultorioAccessAttribute(string parameterName = null) 
        : base(typeof(ConsultorioAccessAuthorizationFilter))
    {
        Arguments = new object[] { null };
    }

    public RequireConsultorioAccessAttribute(Guid consultorioId) 
        : base(typeof(ConsultorioAccessAuthorizationFilter))
    {
        Arguments = new object[] { consultorioId };
    }
}
