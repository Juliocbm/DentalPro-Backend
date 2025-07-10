using System;
using System.Collections.Generic;

namespace DentalPro.Application.DTOs.Rol;

/// <summary>
/// DTO para operaciones de asignación/remoción de permisos a roles
/// </summary>
public class RolPermisosDto
{
    /// <summary>
    /// Identificador único del rol
    /// </summary>
    public Guid IdRol { get; set; }
    
    /// <summary>
    /// Lista de identificadores de permisos que se asignarán o eliminarán
    /// </summary>
    public List<Guid> PermisoIds { get; set; } = new List<Guid>();
    
    /// <summary>
    /// Lista opcional de nombres de permisos que se asignarán o eliminarán (alternativa a IDs)
    /// </summary>
    public List<string>? PermisoNombres { get; set; }
}
