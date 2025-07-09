using DentalPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalPro.Infrastructure.Persistence.Configurations;

public class RecordatorioConfiguration : IEntityTypeConfiguration<Recordatorio>
{
    public void Configure(EntityTypeBuilder<Recordatorio> builder)
    {
        builder.ToTable("Recordatorio", "citas");

        builder.HasKey(r => r.IdRecordatorio);

        builder.Property(r => r.Tipo)
            .HasMaxLength(50);

        builder.Property(r => r.Medio)
            .HasMaxLength(50);

        builder.Property(r => r.Enviado)
            .IsRequired();

        // Relaciones
        builder.HasOne(r => r.Cita)
            .WithMany(c => c.Recordatorios)
            .HasForeignKey(r => r.IdCita)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
