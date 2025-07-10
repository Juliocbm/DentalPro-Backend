namespace DentalPro.Application.DTOs.Permiso;

/// <summary>
/// DTO para la actualización de un permiso existente
/// </summary>
public class PermisoUpdateDto
{
    /// <summary>
    /// Identificador único del permiso a actualizar
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
}
