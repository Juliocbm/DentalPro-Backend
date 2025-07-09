using System;

namespace DentalPro.Domain.Entities;

/// <summary>
/// Entidad asociativa para asignar permisos específicos a usuarios, independientemente de sus roles
/// </summary>
public class UsuarioPermiso
{
    public Guid IdUsuario { get; set; }
    public Guid IdPermiso { get; set; }
    
    /// <summary>
    /// Indica si el permiso está otorgado (true) o explícitamente denegado (false)
    /// La denegación explícita tiene prioridad sobre permisos heredados de roles
    /// </summary>
    public bool Otorgado { get; set; } = true;
    
    // Propiedades de navegación
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Permiso Permiso { get; set; } = null!;
}
