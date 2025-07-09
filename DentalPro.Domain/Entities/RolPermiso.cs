using System;

namespace DentalPro.Domain.Entities;

/// <summary>
/// Entidad asociativa para la relación muchos a muchos entre roles y permisos
/// </summary>
public class RolPermiso
{
    public Guid IdRol { get; set; }
    public Guid IdPermiso { get; set; }
    
    // Propiedades de navegación
    public virtual Rol Rol { get; set; } = null!;
    public virtual Permiso Permiso { get; set; } = null!;
}
