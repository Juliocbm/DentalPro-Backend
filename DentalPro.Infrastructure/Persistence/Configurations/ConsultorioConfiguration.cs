using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Persistence.Configurations;

public class ConsultorioConfiguration : IEntityTypeConfiguration<Consultorio>
{
    public void Configure(EntityTypeBuilder<Consultorio> builder)
    {
        builder.ToTable("Consultorio", "configuracion");

        builder.HasKey(c => c.IdConsultorio);

        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(c => c.RazonSocial).HasMaxLength(150);
        builder.Property(c => c.RFC).HasMaxLength(13);
        builder.Property(c => c.Logo).HasMaxLength(255);
        builder.Property(c => c.HorarioAtencion).HasMaxLength(255);
        builder.Property(c => c.PlanSuscripcion).HasMaxLength(50);
    }
}
