namespace DentalPro.Application.DTOs.Rol;

public class RolDto
{
    public Guid IdRol { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
}
