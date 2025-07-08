using System.ComponentModel.DataAnnotations;

namespace DentalPro.Application.DTOs.Auth;

public class AuthRegisterDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El correo electrónico es requerido")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
    public string Correo { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "La confirmación de la contraseña es requerida")]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; set; } = null!;

    [Required(ErrorMessage = "El ID del consultorio es requerido")]
    public Guid IdConsultorio { get; set; }
    
    public List<string> Roles { get; set; } = new();
}
