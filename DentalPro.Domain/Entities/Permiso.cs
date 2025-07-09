using System;
using System.Collections.Generic;

namespace DentalPro.Domain.Entities;

/// <summary>
/// Representa un permiso dentro del sistema
/// Los permisos son fijos y predefinidos en el sistema
/// </summary>
public class Permiso
{
    public Guid IdPermiso { get; set; }
    
    /// <summary>
    /// Código único que identifica el permiso, usado en la lógica de autorización
    /// </summary>
    public string Codigo { get; set; } = null!;
    
    /// <summary>
    /// Nombre descriptivo del permiso
    /// </summary>
    public string Nombre { get; set; } = null!;
    
    /// <summary>
    /// Descripción detallada del permiso
    /// </summary>
    public string Descripcion { get; set; } = null!;
    
    /// <summary>
    /// Módulo al que pertenece este permiso
    /// </summary>
    public string Modulo { get; set; } = null!;
    
    /// <summary>
    /// Indica si el permiso corresponde a una operación específica (true)
    /// o al acceso general a un módulo (false)
    /// </summary>
    public bool EsOperacion { get; set; } = true;
    
    /// <summary>
    /// Indica si es un permiso base del sistema que no puede ser modificado
    /// </summary>
    public bool PredeterminadoSistema { get; set; }
    
    // Relaciones de navegación
    public virtual ICollection<RolPermiso> Roles { get; set; } = new List<RolPermiso>();
    public virtual ICollection<UsuarioPermiso> Usuarios { get; set; } = new List<UsuarioPermiso>();
}
