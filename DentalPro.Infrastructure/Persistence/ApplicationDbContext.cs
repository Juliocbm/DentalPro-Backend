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
        // Agrega aquí más DbSet según tus entidades

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

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
