namespace DentalPro.Application.DTOs.Usuario;

public class UsuarioDto
{
    public Guid IdUsuario { get; set; }
    public string Nombre { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public bool Activo { get; set; }
    public Guid IdConsultorio { get; set; }
    public List<string> Roles { get; set; } = new();
}
