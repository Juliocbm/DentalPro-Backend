namespace DentalPro.Application.DTOs.Auth;

public class AuthRegisterResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public Guid IdUsuario { get; set; }
    public string Nombre { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
}
