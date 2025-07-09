using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshToken", "seguridad");

        builder.HasKey(rt => rt.IdRefreshToken);

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(rt => rt.FechaCreacion)
            .IsRequired();

        builder.Property(rt => rt.FechaExpiracion)
            .IsRequired();

        builder.Property(rt => rt.EstaRevocado)
            .IsRequired()
            .HasDefaultValue(false);

        // Configuración de la relación con Usuario
        builder.HasOne(rt => rt.Usuario)
               .WithMany()
               .HasForeignKey(rt => rt.IdUsuario);

        // Índice para búsqueda eficiente por token
        builder.HasIndex(rt => rt.Token);
    }
}
