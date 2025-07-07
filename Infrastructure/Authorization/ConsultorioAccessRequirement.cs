using Microsoft.AspNetCore.Authorization;

namespace DentalPro.Api.Infrastructure.Authorization;

public class ConsultorioAccessRequirement : IAuthorizationRequirement
{
    // Esta clase solo actúa como un marcador para el requisito
}

public class ConsultorioAccessHandler : AuthorizationHandler<ConsultorioAccessRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        ConsultorioAccessRequirement requirement)
    {
        if (!context.User.HasClaim(c => c.Type == "IdConsultorio"))
        {
            // Si no tiene el claim, no se puede validar
            return Task.CompletedTask;
        }
        
        // El contexto de recurso contendrá el ID del consultorio al que se intenta acceder
        // Esto se configurará cuando se llame al servicio de autorización desde los controladores
        
        if (context.Resource is Guid consultorioId)
        {
            var userConsultorioId = context.User.FindFirst("IdConsultorio")?.Value;
            if (!string.IsNullOrEmpty(userConsultorioId) && Guid.TryParse(userConsultorioId, out var userConsId))
            {
                if (userConsId == consultorioId)
                {
                    context.Succeed(requirement);
                }
            }
        }

        return Task.CompletedTask;
    }
}
