namespace DentalPro.Domain.Entities;

public class RefreshToken
{
    public Guid IdRefreshToken { get; set; }
    public string Token { get; set; } = null!;
    public DateTime FechaExpiracion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool EstaRevocado { get; set; }
    
    // FK y propiedades de navegación
    public Guid IdUsuario { get; set; }
    public Usuario? Usuario { get; set; }
}
