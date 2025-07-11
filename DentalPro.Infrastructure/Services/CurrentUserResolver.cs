using System.Security.Claims;
using DentalPro.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación básica para resolver el usuario actual desde los claims
/// Se usa para evitar dependencias circulares
/// </summary>
public class CurrentUserResolver : ICurrentUserResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserResolver> _logger;
    
    // Constantes para claims personalizadas
    private const string IdUsuarioClaim = ClaimTypes.NameIdentifier;
    private const string IdConsultorioClaim = "IdConsultorio";

    public CurrentUserResolver(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CurrentUserResolver> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el ID del usuario actual desde las claims del token JWT
    /// </summary>
    public Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(IdUsuarioClaim);
        
        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
        {
            _logger.LogDebug("No se encontró claim de ID de usuario");
            return null;
        }
        
        if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            _logger.LogWarning("Formato inválido para claim de ID de usuario: {ClaimValue}", userIdClaim.Value);
            return null;
        }
        
        return userId;
    }

    /// <summary>
    /// Obtiene el ID del consultorio del usuario actual desde las claims del token JWT
    /// </summary>
    public Guid? GetCurrentConsultorioId()
    {
        var consultorioClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(IdConsultorioClaim);
        
        if (consultorioClaim == null || string.IsNullOrEmpty(consultorioClaim.Value))
        {
            _logger.LogDebug("No se encontró claim de ID de consultorio");
            return null;
        }
        
        if (!Guid.TryParse(consultorioClaim.Value, out Guid consultorioId))
        {
            _logger.LogWarning("Formato inválido para claim de ID de consultorio: {ClaimValue}", consultorioClaim.Value);
            return null;
        }
        
        return consultorioId;
    }
}
