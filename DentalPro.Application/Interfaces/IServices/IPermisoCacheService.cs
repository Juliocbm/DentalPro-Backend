namespace DentalPro.Application.Interfaces.IServices;

/// <summary>
/// Servicio para la gestión de caché de permisos
/// </summary>
public interface IPermisoCacheService
{
    /// <summary>
    /// Obtiene un valor de la caché o lo crea si no existe
    /// </summary>
    /// <typeparam name="T">Tipo del valor a cachear</typeparam>
    /// <param name="key">Clave de caché</param>
    /// <param name="factory">Función para crear el valor si no existe en caché</param>
    /// <param name="expirationTime">Tiempo de expiración opcional</param>
    Task<T> GetOrCreateCacheAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expirationTime = null);

    /// <summary>
    /// Invalida la caché de permisos para un usuario específico
    /// </summary>
    Task InvalidateUsuarioPermisosCacheAsync(Guid idUsuario);

    /// <summary>
    /// Invalida la caché de permisos para todos los usuarios que tienen un rol específico
    /// </summary>
    Task InvalidateUsuariosCacheByRolIdAsync(Guid idRol);

    /// <summary>
    /// Invalida la caché de permisos para un rol específico
    /// </summary>
    Task InvalidateRolPermisosCacheAsync(Guid idRol);

    /// <summary>
    /// Invalida toda la caché de permisos
    /// </summary>
    Task InvalidateAllPermisosCacheAsync();

    /// <summary>
    /// Obtiene la clave de caché para los permisos de un rol
    /// </summary>
    string GetRolPermisosCacheKey(Guid idRol);

    /// <summary>
    /// Obtiene la clave de caché para los permisos de un usuario
    /// </summary>
    string GetUsuarioPermisosCacheKey(Guid idUsuario);

    /// <summary>
    /// Obtiene la clave de caché para todos los permisos
    /// </summary>
    string GetAllPermisosCacheKey();
}
