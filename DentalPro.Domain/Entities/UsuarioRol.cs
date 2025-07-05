namespace DentalPro.Domain.Entities;

public class UsuarioRol
{
    public Guid IdUsuario { get; set; }
    public Usuario? Usuario { get; set; }

    public Guid IdRol { get; set; }
    public Rol? Rol { get; set; }
}
