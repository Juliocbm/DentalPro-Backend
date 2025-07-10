namespace DentalPro.Application.DTOs.Permiso;

/// <summary>
/// DTO básico para permisos
/// </summary>
public class PermisoDto
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
}
