using DentalPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Rol", "seguridad");

        builder.HasKey(r => r.IdRol);

        builder.Property(r => r.Nombre).IsRequired().HasMaxLength(50);
    }
}
