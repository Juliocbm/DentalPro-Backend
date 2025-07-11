using System;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Interfaz mínima para resolver el usuario actual desde los claims
/// Se usa para evitar dependencias circulares
/// </summary>
public interface ICurrentUserResolver
{
    /// <summary>
    /// Obtiene el ID del usuario actual desde las claims
    /// </summary>
    Guid? GetCurrentUserId();
    
    /// <summary>
    /// Obtiene el ID del consultorio del usuario actual desde las claims
    /// </summary>
    Guid? GetCurrentConsultorioId();
}
