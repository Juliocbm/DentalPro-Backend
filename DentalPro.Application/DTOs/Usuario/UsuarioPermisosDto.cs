using System;
using System.Collections.Generic;

namespace DentalPro.Application.DTOs.Usuario;

/// <summary>
/// DTO para representar la asignación o eliminación de permisos de un usuario
/// </summary>
public class UsuarioPermisosDto
{
    /// <summary>
    /// Identificador único del usuario
    /// </summary>
    public Guid IdUsuario { get; set; }
    
    /// <summary>
    /// Lista de identificadores de permisos que se asignarán o eliminarán
    /// </summary>
    public List<Guid> PermisoIds { get; set; } = new List<Guid>();
    
    /// <summary>
    /// Lista opcional de nombres de permisos que se asignarán o eliminarán (alternativa a IDs)
    /// </summary>
    public List<string>? PermisoNombres { get; set; }
}
