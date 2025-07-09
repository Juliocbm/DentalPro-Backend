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
    public bool Activo { get; set; }
    public Guid IdConsultorio { get; set; }
    
    /// <summary>
    /// Lista de roles con información completa (ID y nombre)
    /// </summary>
    public List<RolDetailDto> Roles { get; set; } = new();
    
    // DEPRECATED: Para mantener compatibilidad con el frontend existente
    // Será removido en futuras versiones
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [Obsolete("Use Roles property instead")]
    public List<string> RolesLegacy
    {
        get => Roles?.Select(r => r.Nombre).ToList() ?? new List<string>();
        set {} // Setter vacío para deserialización
    }
    
    // DEPRECATED: Para mantener compatibilidad con el frontend existente
    // Será removido en futuras versiones
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [Obsolete("Use Roles property instead")]
    public List<Guid> RolIds
    {
        get => Roles?.Select(r => r.Id).ToList() ?? new List<Guid>();
        set {} // Setter vacío para deserialización
    }
}
