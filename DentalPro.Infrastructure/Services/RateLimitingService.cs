using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DentalPro.Infrastructure.Services
{
    /// <summary>
    /// Servicio para limitar la tasa de intentos para ciertas operaciones
    /// </summary>
    public class RateLimitingService
    {
        private readonly IMemoryCache _cache;
        private readonly RateLimitingOptions _options;

        public RateLimitingService(IMemoryCache cache, IOptions<RateLimitingOptions> options)
        {
            _cache = cache;
            _options = options.Value;
        }

        /// <summary>
        /// Verifica si una solicitud está permitida para una clave y operación específicas
        /// </summary>
        /// <param name="key">Clave identificadora (ej: dirección IP, nombre de usuario)</param>
        /// <param name="operation">Nombre de la operación a limitar (ej: "login", "register")</param>
        /// <returns>True si la solicitud está permitida, False si excede los límites</returns>
        public async Task<bool> IsAllowedAsync(string key, string operation)
        {
            // Crear una clave compuesta para almacenar en cache
            string cacheKey = $"ratelimit:{operation}:{key}";
            
            // Intentar obtener contador actual
            if (!_cache.TryGetValue(cacheKey, out int attemptCount))
            {
                // Primera solicitud, establecer contador a 1 con tiempo de expiración
                _cache.Set(cacheKey, 1, TimeSpan.FromMinutes(_options.WindowMinutes));
                return true;
            }
            
            // Verificar si excede el límite de intentos
            if (attemptCount >= _options.MaxAttempts)
            {
                return false;
            }
            
            // Incrementar contador
            _cache.Set(cacheKey, attemptCount + 1, TimeSpan.FromMinutes(_options.WindowMinutes));
            return true;
        }

        /// <summary>
        /// Reinicia el contador para una clave y operación específicas (útil después de login exitoso)
        /// </summary>
        /// <param name="key">Clave identificadora</param>
        /// <param name="operation">Nombre de la operación</param>
        public void Reset(string key, string operation)
        {
            string cacheKey = $"ratelimit:{operation}:{key}";
            _cache.Remove(cacheKey);
        }
    }

    /// <summary>
    /// Opciones de configuración para el servicio de rate limiting
    /// </summary>
    public class RateLimitingOptions
    {
        /// <summary>
        /// Número máximo de intentos permitidos en el período de ventana
        /// </summary>
        public int MaxAttempts { get; set; } = 5;
        
        /// <summary>
        /// Duración de la ventana de tiempo en minutos
        /// </summary>
        public int WindowMinutes { get; set; } = 15;
    }
}
