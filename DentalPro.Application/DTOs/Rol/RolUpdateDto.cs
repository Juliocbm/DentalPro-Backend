using System;

namespace DentalPro.Application.DTOs.Rol;

/// <summary>
/// DTO para la actualización de un rol existente
/// </summary>
public class RolUpdateDto
{
    /// <summary>
    /// Identificador único del rol a actualizar
    /// </summary>
    public Guid IdRol { get; set; }
    
    /// <summary>
    /// Nombre del rol
    /// </summary>
    public string Nombre { get; set; } = null!;
    
    /// <summary>
    /// Descripción opcional del rol
    /// </summary>
    public string? Descripcion { get; set; }
}
