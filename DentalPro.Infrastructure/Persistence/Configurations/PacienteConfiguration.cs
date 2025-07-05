using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Persistence.Configurations;

public class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("Paciente");

        builder.HasKey(p => p.IdPaciente);

        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Apellidos).HasMaxLength(100);
        builder.Property(p => p.Telefono).HasMaxLength(20);
        builder.Property(p => p.Correo).HasMaxLength(100);
    }
}
