using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario", "seguridad");

        builder.HasKey(u => u.IdUsuario);

        builder.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Correo).IsRequired().HasMaxLength(100);
        builder.Property(u => u.PasswordHash).IsRequired();

        builder.HasIndex(u => u.Correo).IsUnique();

        builder.HasOne(u => u.Consultorio)
               .WithMany(c => c.Usuarios)
               .HasForeignKey(u => u.IdConsultorio);
    }
}
