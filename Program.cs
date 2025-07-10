using Microsoft.IdentityModel.Tokens;
using System.Text;
using DentalPro.Infrastructure;
using DentalPro.Infrastructure.Mappings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using DentalPro.Api.Infrastructure.Authorization;
using DentalPro.Api.Infrastructure.Extensions;
using DentalPro.Api.Infrastructure.Middlewares;
using DentalPro.Application.Common.Validators;
using DentalPro.Application.Common.Validators.Auth;
using DentalPro.Application.Common.Validators.Usuarios;
using DentalPro.Application.Common.Validators.Roles;
using FluentValidation;
using FluentValidation.AspNetCore;
using DentalPro.Application.Interfaces.IServices;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    // Registrar nuestro filtro de validación personalizado con Order = -9999 para que se ejecute antes
    options.Filters.Add<ValidationFilter>(-9999);
})
.AddFluentValidation(fv => 
{
    // Registrar todos los validadores de forma automática
    fv.RegisterValidatorsFromAssemblyContaining<AuthLoginDtoValidator>();
    
    // Desactivar la validación automática de DataAnnotations
    fv.DisableDataAnnotationsValidation = true;
    
    // No mostrar errores de validación en inglés (opcional)
    ValidatorOptions.Global.LanguageManager.Culture = new System.Globalization.CultureInfo("es");
    
    // IMPORTANTE: Deshabilitar la validación automática para permitir validadores asincrónicos
    fv.AutomaticValidationEnabled = false;
    
    // Configurar para usar nuestro filtro personalizado en lugar de la validación automática
    ValidatorOptions.Global.LanguageManager.Enabled = true;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Documento principal para las APIs core
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { 
        Title = "DentalPro API", 
        Version = "v1",
        Description = "API principal de DentalPro con todas las operaciones de negocio"
    });
    
    // Documento específico para las APIs de diagnóstico
    c.SwaggerDoc("diagnostico", new Microsoft.OpenApi.Models.OpenApiInfo { 
        Title = "DentalPro Diagnóstico API", 
        Version = "v1",
        Description = "APIs de diagnóstico y pruebas para DentalPro"
    });
    
    // Configurar documentos según grupos de API
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (!apiDesc.TryGetMethodInfo(out var methodInfo)) return false;
        
        var groupName = methodInfo.DeclaringType
            .GetCustomAttributes(true)
            .OfType<Microsoft.AspNetCore.Mvc.ApiExplorerSettingsAttribute>()
            .Select(attr => attr.GroupName)
            .FirstOrDefault();
            
        if (docName == "diagnostico")
            return groupName == "diagnostico";
        else
            return groupName != "diagnostico";
    });
    
    // Configurar Swagger para JWT
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese 'Bearer' [espacio] y luego su token JWT en el campo de texto"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero,  // Sin tolerancia de tiempo
            // Esto fuerza a validar la expiración en cada petición
            RequireExpirationTime = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Política para administradores
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Administrador"));
    
    // Política para doctores
    options.AddPolicy("RequireDoctorRole", policy => policy.RequireRole("Doctor"));
    
    // Política para asistentes
    options.AddPolicy("RequireAssistantRole", policy => policy.RequireRole("Asistente"));
    
    // Política para cualquier usuario autenticado
    options.AddPolicy("RequireAuthenticatedUser", policy => policy.RequireAuthenticatedUser());
    
    // Política para validar acceso a consultorio
    options.AddPolicy("RequireConsultorioAccess", policy => policy.Requirements.Add(new ConsultorioAccessRequirement()));
    
    // Las políticas de permisos se manejarán dinámicamente a través de PermisoPolicyProvider
});

// Registrar los manejadores de autorización personalizados
builder.Services.AddSingleton<IAuthorizationHandler, ConsultorioAccessHandler>();
builder.Services.AddScoped<IAuthorizationHandler, PermisoAuthorizationHandler>();

// Registrar el proveedor dinámico de políticas para permisos
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermisoPolicyProvider>();

builder.Services.AddInfrastructure(builder.Configuration);

// Registrar AutoMapper con todos los perfiles del ensamblado de infraestructura
// Esto registrará automáticamente CitaProfile, RecordatorioProfile, UsuarioProfile, RolProfile y PacienteProfile
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Registrar validadores específicos explícitamente
builder.Services.AddScoped<IValidator<DentalPro.Application.DTOs.Auth.AuthLoginDto>, AuthLoginDtoValidator>();
builder.Services.AddScoped<IValidator<DentalPro.Application.DTOs.Auth.AuthRegisterDto>, AuthRegisterDtoValidator>();
// Mantener para compatibilidad con código existente
builder.Services.AddScoped<IValidator<DentalPro.Application.DTOs.Usuario.UsuarioDto>, UsuarioDtoValidator>();

// Registrar validadores de roles (nuevo enfoque unificado)
builder.Services.AddScoped<IValidator<DentalPro.Application.DTOs.Rol.RolCreateDto>, DentalPro.Application.Common.Validators.Roles.RolCreateDtoValidator>();
builder.Services.AddScoped<IValidator<DentalPro.Application.DTOs.Rol.RolUpdateDto>, DentalPro.Application.Common.Validators.Roles.RolUpdateDtoValidator>();

// Registrar validadores de usuarios (nuevo enfoque unificado)
builder.Services.AddScoped<IValidator<DentalPro.Application.DTOs.Usuario.UsuarioCreateDto>, DentalPro.Application.Common.Validators.Usuarios.UsuarioCreateDtoValidator>();
builder.Services.AddScoped<IValidator<DentalPro.Application.DTOs.Usuario.UsuarioUpdateDto>, DentalPro.Application.Common.Validators.Usuarios.UsuarioUpdateDtoValidator>();

// Registrar validadores de pacientes
builder.Services.AddScoped<IValidator<DentalPro.Application.DTOs.Paciente.PacienteCreateDto>, DentalPro.Application.Common.Validators.Pacientes.PacienteCreateDtoValidator>();
builder.Services.AddScoped<IValidator<DentalPro.Application.DTOs.Paciente.PacienteUpdateDto>, DentalPro.Application.Common.Validators.Pacientes.PacienteUpdateDtoValidator>();

// Registrar servicios y repositorios de pacientes
builder.Services.AddScoped<DentalPro.Application.Interfaces.IRepositories.IPacienteRepository, DentalPro.Infrastructure.Persistence.Repositories.PacienteRepository>();
builder.Services.AddScoped<IPacienteService, DentalPro.Infrastructure.Services.PacienteService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DentalPro API v1");
        c.SwaggerEndpoint("/swagger/diagnostico/swagger.json", "Diagnóstico API");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Registrar el middleware de manejo global de excepciones
app.UseGlobalExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
