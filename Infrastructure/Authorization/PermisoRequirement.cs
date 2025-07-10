using Microsoft.AspNetCore.Authorization;

namespace DentalPro.Api.Infrastructure.Authorization
{
    /// <summary>
    /// Requisito de autorización para verificar que un usuario tiene un permiso específico
    /// </summary>
    public class PermisoRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Nombre del permiso requerido
        /// </summary>
        public string NombrePermiso { get; }

        /// <summary>
        /// Constructor que recibe el nombre del permiso requerido
        /// </summary>
        /// <param name="nombrePermiso">Nombre del permiso que se requiere para autorizar</param>
        public PermisoRequirement(string nombrePermiso)
        {
            NombrePermiso = nombrePermiso ?? throw new ArgumentNullException(nameof(nombrePermiso));
        }
    }
}
