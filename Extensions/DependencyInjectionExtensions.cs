using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using DentalPro.Application.Interfaces.IRepositories;
using DentalPro.Application.Interfaces.IServices;
using DentalPro.Infrastructure.Services;

namespace DentalPro.Api.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Registro del servicio de auditoría
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IAuditLogRepository, DentalPro.Infrastructure.Repositories.AuditLogRepository>();
            
            return services;
        }
    }
}
