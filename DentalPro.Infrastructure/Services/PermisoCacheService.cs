using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DentalPro.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de caché para permisos
/// </summary>
public class PermisoCacheService : IPermisoCacheService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<PermisoCacheService> _logger;

    private const string CACHE_KEY_PERMISOS_ALL = "permisos_all";
    private const string CACHE_KEY_PERMISOS_ROL = "permisos_rol_{0}";
    private const string CACHE_KEY_PERMISOS_USUARIO = "permisos_usuario_{0}";
    
    public PermisoCacheService(
        IUsuarioRepository usuarioRepository,
        IMemoryCache memoryCache,
        ILogger<PermisoCacheService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene un valor de la caché o lo crea si no existe
    /// </summary>
    public async Task<T> GetOrCreateCacheAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expirationTime = null)
    {
        if (!_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            cachedValue = await factory();
            
            var cacheOptions = new MemoryCacheEntryOptions();
            if (expirationTime.HasValue)
            {
                cacheOptions.SetAbsoluteExpiration(expirationTime.Value);
            }
            else
            {
                cacheOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
            }
            
            _memoryCache.Set(key, cachedValue, cacheOptions);
            _logger.LogDebug("Valor creado en caché para la clave {Key}", key);
        }
        
        return cachedValue;
    }

    /// <summary>
    /// Invalida la caché de permisos para un usuario específico
    /// </summary>
    public Task InvalidateUsuarioPermisosCacheAsync(Guid idUsuario)
    {
        string cacheKey = GetUsuarioPermisosCacheKey(idUsuario);
        _memoryCache.Remove(cacheKey);
        _logger.LogDebug("Caché de permisos invalidada para usuario {IdUsuario}", idUsuario);
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Invalida la caché de permisos para todos los usuarios que tienen un rol específico
    /// </summary>
    public async Task InvalidateUsuariosCacheByRolIdAsync(Guid idRol)
    {
        // Obtener todos los usuarios que tienen este rol
        var usuarios = await _usuarioRepository.GetUsuariosByRolIdAsync(idRol);
        
        // Invalidar caché para cada usuario
        foreach (var usuario in usuarios)
        {
            await InvalidateUsuarioPermisosCacheAsync(usuario.IdUsuario);
        }
        
        _logger.LogDebug("Caché de permisos invalidada para {Count} usuarios con rol {IdRol}", usuarios.Count(), idRol);
    }

    /// <summary>
    /// Invalida la caché de permisos para un rol específico
    /// </summary>
    public Task InvalidateRolPermisosCacheAsync(Guid idRol)
    {
        string cacheKey = GetRolPermisosCacheKey(idRol);
        _memoryCache.Remove(cacheKey);
        _logger.LogDebug("Caché de permisos invalidada para rol {IdRol}", idRol);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Invalida toda la caché de permisos
    /// </summary>
    public Task InvalidateAllPermisosCacheAsync()
    {
        string cacheKey = GetAllPermisosCacheKey();
        _memoryCache.Remove(cacheKey);
        _logger.LogDebug("Caché global de permisos invalidada");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Obtiene la clave de caché para los permisos de un rol
    /// </summary>
    public string GetRolPermisosCacheKey(Guid idRol)
    {
        return string.Format(CACHE_KEY_PERMISOS_ROL, idRol);
    }

    /// <summary>
    /// Obtiene la clave de caché para los permisos de un usuario
    /// </summary>
    public string GetUsuarioPermisosCacheKey(Guid idUsuario)
    {
        return string.Format(CACHE_KEY_PERMISOS_USUARIO, idUsuario);
    }

    /// <summary>
    /// Obtiene la clave de caché para todos los permisos
    /// </summary>
    public string GetAllPermisosCacheKey()
    {
        return CACHE_KEY_PERMISOS_ALL;
    }
}
