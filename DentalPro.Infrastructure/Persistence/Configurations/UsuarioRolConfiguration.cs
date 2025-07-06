using DentalPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRol>
{
    public void Configure(EntityTypeBuilder<UsuarioRol> builder)
    {
        builder.ToTable("UsuarioRol", "seguridad");

        builder.HasKey(ur => new { ur.IdUsuario, ur.IdRol });

        builder.HasOne(ur => ur.Usuario)
               .WithMany(u => u.Roles)
               .HasForeignKey(ur => ur.IdUsuario);

        builder.HasOne(ur => ur.Rol)
               .WithMany(r => r.Usuarios)
               .HasForeignKey(ur => ur.IdRol);
    }
}
