using Microsoft.IdentityModel.Tokens;
using System.Text;
using DentalPro.Infrastructure;
using DentalPro.Application.Common.Mappings;
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
    fv.RegisterValidatorsFromAssemblyContaining<LoginRequestValidator>();
    
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
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "DentalPro API", Version = "v1" });
    
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
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
    options.AddPolicy("ConsultorioAccess", policy => policy.Requirements.Add(new ConsultorioAccessRequirement()));
});

// Registrar los manejadores de autorización personalizados
builder.Services.AddSingleton<IAuthorizationHandler, ConsultorioAccessHandler>();

builder.Services.AddInfrastructure(builder.Configuration);

// Registrar AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Registrar validadores específicos explícitamente
builder.Services.AddScoped<IValidator<DentalPro.Application.DTOs.Auth.LoginRequest>, LoginRequestValidator>();
builder.Services.AddScoped<IValidator<DentalPro.Application.DTOs.Auth.RegisterRequest>, RegisterRequestValidator>();
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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Registrar el middleware de manejo global de excepciones
app.UseGlobalExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
