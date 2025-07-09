using System;
using System.Collections.Generic;

namespace DentalPro.Domain.Entities;

/// <summary>
/// Representa un rol en el sistema, que puede ser predefinido o creado dinámicamente
/// </summary>
public class Rol
{
    public Guid IdRol { get; set; }
    public string Nombre { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    
    /// <summary>
    /// Indica si el rol es un rol base del sistema (true) o personalizado (false)
    /// </summary>
    public bool EsSistema { get; set; }
    
    /// <summary>
    /// Indica si el rol está activo y puede ser asignado a usuarios
    /// </summary>
    public bool Activo { get; set; } = true;

    // Relaciones de navegación
    public virtual ICollection<UsuarioRol> Usuarios { get; set; } = new List<UsuarioRol>();
    public virtual ICollection<RolPermiso> Permisos { get; set; } = new List<RolPermiso>();
}
