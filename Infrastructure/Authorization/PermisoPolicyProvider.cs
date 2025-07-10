using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace DentalPro.Api.Infrastructure.Authorization
{
    /// <summary>
    /// Proveedor de políticas de autorización dinámico para permisos
    /// </summary>
    public class PermisoPolicyProvider : IAuthorizationPolicyProvider
    {
        private const string PERMISO_PREFIX = "Permiso_";
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

        public PermisoPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        /// <summary>
        /// Obtiene la política predeterminada
        /// </summary>
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

        /// <summary>
        /// Obtiene la política de reserva
        /// </summary>
        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();

        /// <summary>
        /// Obtiene o genera dinámicamente una política de autorización basada en el nombre de la política
        /// </summary>
        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Si la política comienza con el prefijo de permisos, crear una política dinámica
            if (policyName.StartsWith(PERMISO_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                var nombrePermiso = policyName.Substring(PERMISO_PREFIX.Length);
                
                // Crear un constructor de política
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new PermisoRequirement(nombrePermiso));
                
                return Task.FromResult<AuthorizationPolicy?>(policy.Build());
            }

            // Para cualquier otra política, usar el proveedor predeterminado
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
