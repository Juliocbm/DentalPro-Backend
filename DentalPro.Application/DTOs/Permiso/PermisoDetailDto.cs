using DentalPro.Application.DTOs.Rol;

namespace DentalPro.Application.DTOs.Permiso;

/// <summary>
/// DTO para mostrar información detallada de un permiso, incluyendo roles asociados
/// </summary>
public class PermisoDetailDto
{
    /// <summary>
    /// Identificador único del permiso
    /// </summary>
    public Guid IdPermiso { get; set; }
    
    /// <summary>
    /// Nombre del permiso
    /// </summary>
    public string Nombre { get; set; } = null!;
    
    /// <summary>
    /// Descripción del permiso
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }
    
    /// <summary>
    /// Roles que tienen asignado este permiso
    /// </summary>
    public ICollection<RolDto> Roles { get; set; } = new List<RolDto>();
}
