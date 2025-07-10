using System;
using System.Collections.Generic;

namespace DentalPro.Domain.Entities;

/// <summary>
/// Representa un usuario del sistema con sus datos básicos y relaciones
/// </summary>
public class Usuario
{
    public Guid IdUsuario { get; set; }
    public string Nombre { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool Activo { get; set; }

    public Guid IdConsultorio { get; set; }
    public Consultorio? Consultorio { get; set; }

    // Propiedades específicas por tipo de usuario - navegación 1:1 a DoctorDetail
    public virtual DoctorDetail? DoctorDetail { get; set; }

    // Relaciones de navegación
    public virtual ICollection<UsuarioRol> Roles { get; set; } = new List<UsuarioRol>();
    public virtual ICollection<UsuarioPermiso> Permisos { get; set; } = new List<UsuarioPermiso>();
}
