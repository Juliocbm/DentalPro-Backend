using Microsoft.AspNetCore.Authorization;
using System;

namespace DentalPro.Api.Infrastructure.Authorization
{
    /// <summary>
    /// Atributo para requerir un permiso específico para acceder a un controlador o acción
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequirePermisoAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Constructor que recibe el nombre del permiso requerido
        /// </summary>
        /// <param name="nombrePermiso">Nombre del permiso requerido para acceder al recurso</param>
        public RequirePermisoAttribute(string nombrePermiso)
        {
            if (string.IsNullOrWhiteSpace(nombrePermiso))
            {
                throw new ArgumentException("El nombre del permiso no puede estar vacío", nameof(nombrePermiso));
            }

            // La política de autorización seguirá el patrón "Permiso_[NombrePermiso]"
            Policy = $"Permiso_{nombrePermiso}";
        }
    }
}
