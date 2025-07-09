using DentalPro.Domain.Entities;

namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Interfaz para obtener información sobre el usuario actual de la aplicación
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Obtiene el ID del usuario actual
    /// </summary>
    Guid GetCurrentUserId();
    
    /// <summary>
    /// Obtiene el usuario actual completo con sus roles y relaciones
    /// </summary>
    Task<Usuario> GetCurrentUserAsync();
    
    /// <summary>
    /// Obtiene el ID del consultorio al que pertenece el usuario actual
    /// </summary>
    Guid GetCurrentConsultorioId();
    
    /// <summary>
    /// Verifica si el usuario actual tiene un rol específico
    /// </summary>
    Task<bool> IsInRoleAsync(string roleName);
    
    /// <summary>
    /// Invalida el caché del usuario actual, útil después de actualizaciones
    /// </summary>
    void InvalidateCurrentUserCache();
}
