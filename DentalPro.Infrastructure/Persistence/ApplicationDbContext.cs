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
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        // Entidades para el sistema de permisos y roles

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            
            // Configuración de entidad Usuario
            modelBuilder.Entity<Usuario>(builder => {
                builder.ToTable("Usuario", "seguridad");
                builder.HasKey(u => u.IdUsuario);
                
                builder.Property(u => u.Nombre)
                    .HasMaxLength(100)
                    .IsRequired();
                                        
                builder.Property(u => u.Correo)
                    .HasMaxLength(150)
                    .IsRequired();
                    
                builder.Property(u => u.PasswordHash)
                    .HasMaxLength(500)
                    .IsRequired();
            });

            // Configuración de RefreshToken
            modelBuilder.Entity<RefreshToken>(builder => {
                builder.ToTable("RefreshToken", "seguridad");
                builder.HasKey(rt => rt.IdRefreshToken);
                
                builder.Property(rt => rt.Token)
                    .HasMaxLength(500)
                    .IsRequired();                    
            });
            
            // Configuración de UsuarioRol
            modelBuilder.Entity<UsuarioRol>(builder => {
                builder.ToTable("UsuarioRol", "seguridad");
                builder.HasKey(ur => new { ur.IdUsuario, ur.IdRol });
                
                builder.HasOne(ur => ur.Usuario)
                    .WithMany(u => u.Roles)
                    .HasForeignKey(ur => ur.IdUsuario);
                    
                builder.HasOne(ur => ur.Rol)
                    .WithMany(r => r.Usuarios)
                    .HasForeignKey(ur => ur.IdRol);
            });
            
            // Configuración de DoctorDetail como entidad independiente
            modelBuilder.Entity<DoctorDetail>(builder =>
            {
                builder.ToTable("DoctorDetalle", "seguridad");
                
                // Usar IdUsuario como clave primaria para simplificar la relación 1:1
                builder.HasKey(d => d.IdUsuario);
                
                builder.Property(d => d.Especialidad)
                    .HasMaxLength(100)
                    .IsRequired();
                    
                builder.Property(d => d.NumeroLicencia)
                    .HasMaxLength(50);
                    
                builder.Property(d => d.Certificaciones)
                    .HasMaxLength(1000);
                    
                // Configurar relación 1:1 con Usuario donde IdUsuario es tanto PK como FK
                builder.HasOne(d => d.Usuario)
                    .WithOne(u => u.DoctorDetail) // Especificar la propiedad de navegación en Usuario
                    .HasForeignKey<DoctorDetail>(d => d.IdUsuario)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de entidad Permiso
            modelBuilder.Entity<Permiso>(builder => {
                builder.ToTable("Permiso", "seguridad");
                builder.HasKey(p => p.IdPermiso);
                
                builder.Property(p => p.Codigo)
                    .HasMaxLength(100)
                    .IsRequired();
                    
                builder.Property(p => p.Nombre)
                    .HasMaxLength(100)
                    .IsRequired();
                    
                builder.Property(p => p.Descripcion)
                    .HasMaxLength(500);
                    
                builder.Property(p => p.Modulo)
                    .HasMaxLength(50);
            });

            // Configuración de entidad Rol
            modelBuilder.Entity<Rol>(builder => {
                builder.ToTable("Rol", "seguridad");
                builder.HasKey(r => r.IdRol);
                
                builder.Property(r => r.Nombre)
                    .HasMaxLength(50)
                    .IsRequired();
                    
                builder.Property(r => r.Descripcion)
                    .HasMaxLength(200);
            });

            // Configuración de clave primaria compuesta para RolPermiso
            modelBuilder.Entity<RolPermiso>(builder => {
                builder.ToTable("RolPermiso", "seguridad");
                builder.HasKey(rp => new { rp.IdRol, rp.IdPermiso });
                
                // Configuración de relación muchos a muchos entre Rol y Permiso
                builder.HasOne(rp => rp.Rol)
                    .WithMany(r => r.Permisos)
                    .HasForeignKey(rp => rp.IdRol);
                    
                builder.HasOne(rp => rp.Permiso)
                    .WithMany(p => p.Roles)
                    .HasForeignKey(rp => rp.IdPermiso);
            });
                
            // Configuración de clave primaria compuesta para UsuarioPermiso
            // Configuración de clave primaria compuesta para UsuarioPermiso y nombre de tabla
            modelBuilder.Entity<UsuarioPermiso>(builder => {
                builder.ToTable("UsuarioPermiso", "seguridad");
                builder.HasKey(up => new { up.IdUsuario, up.IdPermiso });
                
                // Configuración de relación muchos a muchos entre Usuario y Permiso
                builder.HasOne(up => up.Usuario)
                    .WithMany(u => u.Permisos)
                    .HasForeignKey(up => up.IdUsuario);
                    
                builder.HasOne(up => up.Permiso)
                    .WithMany(p => p.Usuarios)
                    .HasForeignKey(up => up.IdPermiso);
            });
                
                
            // Configuración de AuditLog
            modelBuilder.Entity<AuditLog>(builder =>
            {
                builder.ToTable("AuditLogs", "auditoria");
                
                builder.HasKey(a => a.Id);
                
                builder.Property(a => a.Action)
                    .IsRequired()
                    .HasMaxLength(50);
                    
                builder.Property(a => a.EntityType)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                builder.Property(a => a.Details)
                    .HasMaxLength(4000);
                    
                builder.Property(a => a.IpAddress)
                    .HasMaxLength(50);
                    
                // Los registros de auditoría no deben filtrarse por consultorio
                // en las consultas globales - se filtrará explícitamente cuando sea necesario
            });

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
