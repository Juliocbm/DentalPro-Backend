using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;

namespace DentalPro.Api.Infrastructure.Authorization;

public class ConsultorioAccessRequirement : IAuthorizationRequirement
{
    // Esta clase solo actúa como un marcador para el requisito
}

public class ConsultorioAccessHandler : AuthorizationHandler<ConsultorioAccessRequirement>
{
    private readonly ILogger<ConsultorioAccessHandler> _logger;

    public ConsultorioAccessHandler(ILogger<ConsultorioAccessHandler> logger = null)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        ConsultorioAccessRequirement requirement)
    {
        // Permitir acceso a SuperUsuarios siempre - ellos pueden acceder a datos de cualquier consultorio
        if (context.User.IsInRole("SuperUsuario"))
        {
            _logger?.LogInformation("Acceso permitido a usuario con rol SuperUsuario");
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        
        // Los Administradores ya no tienen bypass automático, deben pertenecer al mismo consultorio
        // para mantener la separación de datos entre consultorios

        // Verificar si tiene el claim necesario
        if (!context.User.HasClaim(c => c.Type == "IdConsultorio"))
        {
            _logger?.LogWarning("Usuario sin claim IdConsultorio");
            return Task.CompletedTask;
        }
        
        // El consultorio del usuario desde el claim
        var userConsultorioIdStr = context.User.FindFirst("IdConsultorio")?.Value;
        if (string.IsNullOrEmpty(userConsultorioIdStr) || !Guid.TryParse(userConsultorioIdStr, out var userConsultorioId))
        {
            _logger?.LogWarning($"Claim IdConsultorio inválido: {userConsultorioIdStr}");
            return Task.CompletedTask;
        }

        // Si no se pasa recurso, autorizamos basado solo en la presencia del claim válido
        if (context.Resource == null)
        {
            _logger?.LogInformation("Autorización basada solo en presencia de claim válido");
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Intentar extraer el ID del consultorio del recurso
        Guid? resourceConsultorioId = null;
        
        // Caso 1: El recurso es directamente un Guid
        if (context.Resource is Guid directGuid)
        {
            resourceConsultorioId = directGuid;
            _logger?.LogInformation($"Recurso es un Guid directo: {directGuid}");
        }
        // Caso 2: El recurso es un string que puede convertirse a Guid
        else if (context.Resource is string guidString && Guid.TryParse(guidString, out var parsedGuid))
        {
            resourceConsultorioId = parsedGuid;
            _logger?.LogInformation($"Recurso es un string convertible a Guid: {parsedGuid}");
        }

        // Si pudimos extraer un ID de consultorio del recurso, comparamos
        if (resourceConsultorioId.HasValue)
        {
            if (userConsultorioId == resourceConsultorioId.Value)
            {
                _logger?.LogInformation($"Autorizado: consultorio del usuario {userConsultorioId} coincide con el recurso {resourceConsultorioId.Value}");
                context.Succeed(requirement);
            }
            else
            {
                _logger?.LogWarning($"No autorizado: consultorio del usuario {userConsultorioId} no coincide con el recurso {resourceConsultorioId.Value}");
            }
        }
        else
        {
            // Si no pudimos extraer el ID del consultorio del recurso pero el usuario tiene un claim válido,
            // autorizamos de todas formas (comportamiento más permisivo)
            _logger?.LogInformation("Autorización por defecto (recurso no contiene Guid de consultorio)");
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
