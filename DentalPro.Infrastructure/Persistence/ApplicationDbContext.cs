// Infrastructure/Persistence/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using DentalPro.Domain.Entities;
using System.Reflection;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Http;

namespace DentalPro.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        private readonly Guid? _consultorioId;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            var user = httpContextAccessor.HttpContext?.User;
            var idConsultorioClaim = user?.FindFirst("IdConsultorio")?.Value;
            _consultorioId = Guid.TryParse(idConsultorioClaim, out var id) ? id : null;
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Consultorio> Consultorios => Set<Consultorio>();
        public DbSet<Paciente> Pacientes => Set<Paciente>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Cita> Citas => Set<Cita>();
        public DbSet<Recordatorio> Recordatorios => Set<Recordatorio>();
        public DbSet<Rol> Roles => Set<Rol>();
        public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
        public DbSet<Permiso> Permisos => Set<Permiso>();
        public DbSet<RolPermiso> RolesPermisos => Set<RolPermiso>();
        public DbSet<UsuarioPermiso> UsuariosPermisos => Set<UsuarioPermiso>();
        public DbSet<DoctorDetail> DoctorDetails => Set<DoctorDetail>();
        // Entidades para el sistema de permisos y roles

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            
            // Configuración de DoctorDetail como entidad independiente
            modelBuilder.Entity<DoctorDetail>(builder =>
            {
                builder.ToTable("DoctorDetalle", "seguridad");
                
                builder.HasKey(d => d.IdDoctorDetail);
                
                builder.Property(d => d.Especialidad)
                    .HasMaxLength(100)
                    .IsRequired();
                    
                builder.Property(d => d.NumeroLicencia)
                    .HasMaxLength(50);
                    
                builder.Property(d => d.Certificaciones)
                    .HasMaxLength(1000);
                    
                // Configurar relación 1:1 con Usuario
                builder.HasOne(d => d.Usuario)
                    .WithOne()
                    .HasForeignKey<DoctorDetail>(d => d.IdUsuario)
                    .OnDelete(DeleteBehavior.Cascade);
            });
                
            // Configuración de clave primaria compuesta para RolPermiso
            modelBuilder.Entity<RolPermiso>()
                .HasKey(rp => new { rp.IdRol, rp.IdPermiso });
                
            // Configuración de relación muchos a muchos entre Rol y Permiso
            modelBuilder.Entity<RolPermiso>()
                .HasOne(rp => rp.Rol)
                .WithMany(r => r.Permisos)
                .HasForeignKey(rp => rp.IdRol);
                
            modelBuilder.Entity<RolPermiso>()
                .HasOne(rp => rp.Permiso)
                .WithMany(p => p.Roles)
                .HasForeignKey(rp => rp.IdPermiso);
                
            // Configuración de clave primaria compuesta para UsuarioPermiso
            modelBuilder.Entity<UsuarioPermiso>()
                .HasKey(up => new { up.IdUsuario, up.IdPermiso });
                
            // Configuración de relación muchos a muchos entre Usuario y Permiso
            modelBuilder.Entity<UsuarioPermiso>()
                .HasOne(up => up.Usuario)
                .WithMany(u => u.Permisos)
                .HasForeignKey(up => up.IdUsuario);
                
            modelBuilder.Entity<UsuarioPermiso>()
                .HasOne(up => up.Permiso)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(up => up.IdPermiso);

            // Filtro global por IdConsultorio para entidades que lo incluyan
            // Solo aplicar el filtro si tenemos un ID de consultorio
            if (_consultorioId.HasValue)
            {
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    if (entityType.ClrType.GetProperty("IdConsultorio") != null)
                    {
                        var parameter = Expression.Parameter(entityType.ClrType, "e");
                        var property = Expression.Property(parameter, "IdConsultorio");
                        var propertyType = ((PropertyInfo)property.Member).PropertyType;
                        var constant = Expression.Constant(_consultorioId.Value, propertyType);
                        var body = Expression.Equal(property, constant);
                        var lambda = Expression.Lambda(body, parameter);
                        modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                    }
                }
            }
        }
    }
}
