using DentalPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalPro.Infrastructure.Persistence.Configurations;

public class CitaConfiguration : IEntityTypeConfiguration<Cita>
{
    public void Configure(EntityTypeBuilder<Cita> builder)
    {
        builder.ToTable("Cita", "citas");

        builder.HasKey(c => c.IdCita);

        builder.Property(c => c.FechaHoraInicio)
            .IsRequired();

        builder.Property(c => c.FechaHoraFin)
            .IsRequired();

        builder.Property(c => c.Motivo)
            .HasMaxLength(250);

        builder.Property(c => c.Estatus)
            .HasMaxLength(20)
            .IsRequired();

        // Relaciones
        builder.HasOne(c => c.Paciente)
            .WithMany()
            .HasForeignKey(c => c.IdPaciente)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Usuario)
            .WithMany()
            .HasForeignKey(c => c.IdUsuario)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
