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

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Registrar servicios del framework
        services.AddMemoryCache();
        // Registrar DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Registrar HttpContextAccessor para acceso al contexto HTTP
        services.AddHttpContextAccessor();

        // Registrar repositorios
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<IConsultorioRepository, ConsultorioRepository>();
        services.AddScoped<ICitaRepository, CitaRepository>();
        services.AddScoped<IRecordatorioRepository, RecordatorioRepository>();
        
        // Registrar servicios
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IRolService, RolService>();
        services.AddScoped<IConsultorioService, ConsultorioService>();
        services.AddScoped<ICitaService, CitaService>();
        services.AddScoped<IRecordatorioService, RecordatorioService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        // Registrar validadores asíncronos
        services.AddScoped<PacienteExistenceAsyncValidator>();
        services.AddScoped<CitaExistenceAsyncValidator>();
        services.AddScoped<RecordatorioExistenceAsyncValidator>();
        
        return services;
    }
}
