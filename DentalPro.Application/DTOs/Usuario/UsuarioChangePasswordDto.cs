using System.ComponentModel.DataAnnotations;

namespace DentalPro.Application.DTOs.Usuario;

public class UsuarioChangePasswordDto
{
    [Required(ErrorMessage = "El ID del usuario es requerido")]
    public Guid IdUsuario { get; set; }
    
    [Required(ErrorMessage = "La contraseña actual es requerida")]
    public string CurrentPassword { get; set; } = null!;
    
    [Required(ErrorMessage = "La nueva contraseña es requerida")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string NewPassword { get; set; } = null!;
    
    [Required(ErrorMessage = "La confirmación de la nueva contraseña es requerida")]
    [Compare("NewPassword", ErrorMessage = "La nueva contraseña y su confirmación no coinciden")]
    public string ConfirmNewPassword { get; set; } = null!;
}
