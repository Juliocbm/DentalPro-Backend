using System.ComponentModel.DataAnnotations;

namespace DentalPro.Application.DTOs.Usuario;

public class UpdateUsuarioRequest
{
    [Required(ErrorMessage = "El ID del usuario es requerido")]
    public Guid IdUsuario { get; set; }
    
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Nombre { get; set; } = null!;
    
    [Required(ErrorMessage = "El correo electrónico es requerido")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
    public string Correo { get; set; } = null!;
    
    public bool Activo { get; set; }
    
    public List<string> Roles { get; set; } = new();
}
