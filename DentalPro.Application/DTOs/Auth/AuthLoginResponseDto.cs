namespace DentalPro.Application.DTOs.Auth;

public class AuthLoginResponseDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime TokenExpiracion { get; set; }
    public string Nombre { get; set; } = null!;
    public Guid IdUsuario { get; set; }
    public Guid IdConsultorio { get; set; }
    public List<string> Roles { get; set; } = new();
}
