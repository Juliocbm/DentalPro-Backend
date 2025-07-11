namespace DentalPro.Domain.Entities;

public class Rol
{
    public Guid IdRol { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }

    public ICollection<UsuarioRol> Usuarios { get; set; } = new List<UsuarioRol>();
}
