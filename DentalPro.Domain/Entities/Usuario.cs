namespace DentalPro.Domain.Entities;

public class Usuario
{
    public Guid IdUsuario { get; set; }
    public string Nombre { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool Activo { get; set; }

    public Guid IdConsultorio { get; set; }
    public Consultorio? Consultorio { get; set; }

    public ICollection<UsuarioRol> Roles { get; set; } = new List<UsuarioRol>();
}
