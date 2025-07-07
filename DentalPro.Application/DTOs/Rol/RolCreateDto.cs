namespace DentalPro.Application.DTOs.Rol;

/// <summary>
/// DTO para la creación de un nuevo rol
/// </summary>
public class RolCreateDto
{
    /// <summary>
    /// Nombre del rol
    /// </summary>
    public string Nombre { get; set; } = null!;
    
    /// <summary>
    /// Descripción opcional del rol
    /// </summary>
    public string? Descripcion { get; set; }
}
