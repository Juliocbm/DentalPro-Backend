using System;
using System.Collections.Generic;
using System.Linq;
using DentalPro.Application.DTOs.Rol;

namespace DentalPro.Application.DTOs.Usuario;

public class UsuarioDto
{
    public Guid IdUsuario { get; set; }
    public string Nombre { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string? Telefono { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public Guid IdConsultorio { get; set; }
    
    /// <summary>
    /// Lista de roles con información completa (ID y nombre)
    /// </summary>
    public List<RolDetailDto> Roles { get; set; } = new();
}
