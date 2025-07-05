using Microsoft.IdentityModel.Tokens;
using System.Text;
using DentalPro.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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

builder.Services.AddAuthorization();

builder.Services.AddInfrastructure(builder.Configuration);


var app = builder.Build();

// Crear la base de datos si no existe al iniciar la aplicación
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DentalPro.Infrastructure.Persistence.ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
    
    // Inicializar datos predeterminados si es necesario
    if (!dbContext.Consultorios.Any())
    {
        // Crear un consultorio predeterminado
        var consultorioId = Guid.NewGuid();
        dbContext.Consultorios.Add(new DentalPro.Domain.Entities.Consultorio
        {
            IdConsultorio = consultorioId,
            Nombre = "Consultorio Predeterminado",
            Direccion = "Dirección Predeterminada",
            Telefono = "123456789",
            Email = "info@consultorio.com",
            Activo = true
        });
        
        // Crear roles predeterminados si no existen
        var roles = new[] { "Administrador", "Doctor", "Recepcionista", "Usuario" };
        foreach (var rolNombre in roles)
        {
            if (!dbContext.Set<DentalPro.Domain.Entities.Rol>().Any(r => r.Nombre == rolNombre))
            {
                dbContext.Set<DentalPro.Domain.Entities.Rol>().Add(new DentalPro.Domain.Entities.Rol
                {
                    IdRol = Guid.NewGuid(),
                    Nombre = rolNombre,
                    Descripcion = $"Rol de {rolNombre}"
                });
            }
        }
        
        dbContext.SaveChanges();
        
        // Mostrar información útil en la consola
        Console.WriteLine($"Consultorio predeterminado creado con ID: {consultorioId}");
        Console.WriteLine("Usa este ID para registrar el primer usuario.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
