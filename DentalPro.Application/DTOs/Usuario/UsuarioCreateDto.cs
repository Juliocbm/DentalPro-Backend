using System;
using System.Collections.Generic;

namespace DentalPro.Application.DTOs.Usuario;

/// <summary>
/// DTO para la creación de un nuevo usuario
/// </summary>
public class UsuarioCreateDto
{
    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string Nombre { get; set; } = null!;
    
    /// <summary>
    /// Correo electrónico del usuario (será usado como nombre de usuario)
    /// </summary>
    public string Correo { get; set; } = null!;
    
    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    public string Password { get; set; } = null!;
    
    /// <summary>
    /// Confirmación de la contraseña
    /// </summary>
    public string ConfirmPassword { get; set; } = null!;
    
    /// <summary>
    /// ID del consultorio al que pertenece el usuario
    /// </summary>
    public Guid IdConsultorio { get; set; }
    
    /// <summary>
    /// Lista de IDs de roles asignados al usuario
    /// </summary>
    public List<Guid> RolIds { get; set; } = new();
}
