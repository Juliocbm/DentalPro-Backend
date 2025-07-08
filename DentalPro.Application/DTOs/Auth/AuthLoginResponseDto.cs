namespace DentalPro.Application.DTOs.Auth;

public class AuthLoginResponseDto
{
    public string Token { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public Guid IdUsuario { get; set; }
    public Guid IdConsultorio { get; set; }
    public List<string> Roles { get; set; } = new();
}
