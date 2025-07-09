using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DentalPro.Domain.Entities;

namespace DentalPro.Infrastructure.Persistence.Configurations;

public class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("Paciente", "pacientes");

        builder.HasKey(p => p.IdPaciente);

        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Apellidos).HasMaxLength(100);
        builder.Property(p => p.Telefono).HasMaxLength(20);
        builder.Property(p => p.Correo).HasMaxLength(100);
        
        // Configuración de la relación con Consultorio
        builder.HasOne(p => p.Consultorio)
               .WithMany(c => c.Pacientes)
               .HasForeignKey(p => p.IdConsultorio);
    }
}
