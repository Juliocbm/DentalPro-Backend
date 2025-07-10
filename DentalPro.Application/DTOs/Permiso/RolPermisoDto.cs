using System;
using System.Collections.Generic;

namespace DentalPro.Application.DTOs.Permiso;

/// <summary>
/// DTO para operaciones de asignación/remoción de permisos a roles
/// </summary>
public class RolPermisoDto
{
    /// <summary>
    /// Identificador único del rol
    /// </summary>
    public Guid IdRol { get; set; }
    
    /// <summary>
    /// Lista de IDs de permisos para asignar o remover
    /// </summary>
    public IEnumerable<Guid> PermisoIds { get; set; } = new List<Guid>();
}
