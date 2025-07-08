using System;
using System.Collections.Generic;

namespace DentalPro.Application.DTOs.Usuario;

/// <summary>
/// DTO para la actualización de un usuario existente
/// </summary>
public class UsuarioUpdateDto
{
    /// <summary>
    /// ID del usuario a actualizar
    /// </summary>
    public Guid IdUsuario { get; set; }
    
    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string Nombre { get; set; } = null!;
    
    /// <summary>
    /// Correo electrónico del usuario
    /// </summary>
    public string Correo { get; set; } = null!;
    
    /// <summary>
    /// Estado del usuario (activo/inactivo)
    /// </summary>
    public bool Activo { get; set; }
    
    /// <summary>
    /// Lista de IDs de roles asignados al usuario
    /// </summary>
    public List<Guid> RolIds { get; set; } = new();
}
