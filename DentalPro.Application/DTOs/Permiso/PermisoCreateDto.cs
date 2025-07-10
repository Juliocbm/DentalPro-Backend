namespace DentalPro.Application.DTOs.Permiso;

/// <summary>
/// DTO para la creación de un nuevo permiso
/// </summary>
public class PermisoCreateDto
{
    /// <summary>
    /// Nombre del permiso
    /// </summary>
    public string Nombre { get; set; } = null!;
    
    /// <summary>
    /// Descripción del permiso
    /// </summary>
    public string? Descripcion { get; set; }
}
