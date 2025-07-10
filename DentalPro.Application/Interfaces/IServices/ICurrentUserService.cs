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
    /// Verifica si el usuario actual tiene un permiso específico, ya sea directamente o a través de sus roles
    /// </summary>
    /// <param name="permisoNombre">Nombre del permiso a verificar</param>
    /// <returns>True si el usuario tiene el permiso, false en caso contrario</returns>
    Task<bool> HasPermisoAsync(string permisoNombre);
    
    /// <summary>
    /// Verifica si el usuario actual tiene todos los permisos especificados
    /// </summary>
    /// <param name="permisoNombres">Lista de nombres de permisos a verificar</param>
    /// <returns>True si el usuario tiene todos los permisos, false en caso contrario</returns>
    Task<bool> HasAllPermisosAsync(IEnumerable<string> permisoNombres);
    
    /// <summary>
    /// Verifica si el usuario actual tiene al menos uno de los permisos especificados
    /// </summary>
    /// <param name="permisoNombres">Lista de nombres de permisos a verificar</param>
    /// <returns>True si el usuario tiene al menos uno de los permisos, false en caso contrario</returns>
    Task<bool> HasAnyPermisoAsync(IEnumerable<string> permisoNombres);
    
    /// <summary>
    /// Obtiene la lista de permisos del usuario actual
    /// </summary>
    /// <returns>Lista de nombres de permisos</returns>
    Task<IEnumerable<string>> GetPermisosAsync();
    
    /// <summary>
    /// Invalida el caché del usuario actual, útil después de actualizaciones
    /// </summary>
    void InvalidateCurrentUserCache();
}
