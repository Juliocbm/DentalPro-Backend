using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Infrastructure.Persistence;
using DentalPro.Infrastructure.Persistence.Repositories;
using DentalPro.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using DentalPro.Application.Common.Validators.Async;
using DentalPro.Infrastructure.Persistence.Interceptors;
using Microsoft.Extensions.Logging;
using DentalPro.Infrastructure.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Registrar servicios del framework
        services.AddMemoryCache();
        
        // Registrar el interceptor de auditoría
        services.AddScoped<AuditSaveChangesInterceptor>();
        
        // Registrar DbContext con interceptores
        services.AddDbContext<ApplicationDbContext>((sp, options) => {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            
            // Agregar interceptor de auditoría
            var auditInterceptor = sp.GetRequiredService<AuditSaveChangesInterceptor>();
            options.AddInterceptors(auditInterceptor);
        });

        // Registrar HttpContextAccessor para acceso al contexto HTTP
        services.AddHttpContextAccessor();

        // Registrar repositorios
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<IPermisoRepository, PermisoRepository>();
        services.AddScoped<IRolPermisoRepository, RolPermisoRepository>();
        services.AddScoped<IUsuarioPermisoRepository, UsuarioPermisoRepository>();
        services.AddScoped<IConsultorioRepository, ConsultorioRepository>();
        services.AddScoped<ICitaRepository, CitaRepository>();
        services.AddScoped<IRecordatorioRepository, RecordatorioRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        
        // Registrar servicios
        services.AddScoped<IAuthService, AuthService>();
        
        // Servicios de usuario
        services.AddScoped<IUsuarioManagementService, UsuarioManagementService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IUsuarioRoleService, UsuarioRoleService>();
        services.AddScoped<IUsuarioPermisoService, UsuarioPermisoService>();
        services.AddScoped<IRolService, RolService>();
        // Servicios de permisos especializados
        services.AddScoped<IPermisoCacheService, PermisoCacheService>();
        services.AddScoped<IPermisoManagementService, PermisoManagementService>();
        services.AddScoped<IPermisoAssignmentService, PermisoAssignmentService>();
        // Servicios de permisos especializados (sin fachada)
        services.AddScoped<IRolPermisoService, RolPermisoService>();
        services.AddScoped<IConsultorioService, ConsultorioService>();
        services.AddScoped<ICitaService, CitaService>();
        services.AddScoped<IRecordatorioService, RecordatorioService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        // Registrar servicio de auditoría
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditService, AuditService>();
        
        // Registrar validadores asíncronos
        services.AddScoped<PacienteExistenceAsyncValidator>();
        services.AddScoped<CitaExistenceAsyncValidator>();
        services.AddScoped<RecordatorioExistenceAsyncValidator>();
        // Los validadores genéricos no necesitan ser registrados directamente
        // ya que se crean mediante factory methods en los validadores de DTOs
        
        return services;
    }
}
