using System.ComponentModel.DataAnnotations;

namespace DentalPro.Application.DTOs.Auth;

public class AuthLoginDto
{
    [Required(ErrorMessage = "El correo electrónico es requerido")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
    public string Correo { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es requerida")]
    public string Password { get; set; } = null!;
}
