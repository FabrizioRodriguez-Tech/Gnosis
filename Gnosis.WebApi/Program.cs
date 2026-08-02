using System.Text;
using Gnosis.Infrastructure;
using Gnosis.WebApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// CONFIGURACIÓN DE SERVICIOS
// ============================================================================
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var origins = builder.Configuration.GetValue<string>("AllowedOrigins")?.Split(",")
              ?? new[] { "https://localhost:44372", "http://localhost:44372" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirBlazor", policy =>
    {
        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Autenticación por JWT Bearer: el Blazor WASM manda "Authorization: Bearer <token>" en cada
// petición; el token lo firma TokenService (Gnosis.Business) con esta misma clave.
var jwtClave = builder.Configuration["Jwt:Clave"]
    ?? throw new InvalidOperationException(
        "Falta configurar Jwt:Clave en appsettings.Development.json. Sin esto la API no puede validar tokens.");
var jwtEmisor = builder.Configuration["Jwt:Emisor"] ?? "Gnosis";
var jwtAudiencia = builder.Configuration["Jwt:Audiencia"] ?? "Gnosis";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtEmisor,
            ValidAudience = jwtAudiencia,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtClave))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddProjectServices(builder.Configuration);

var app = builder.Build();

// En Render (y cualquier server sin acceso a la Package Manager Console) no hay forma de correr
// Update-Database a mano, así que aplicamos las migraciones pendientes automáticamente al arrancar.
// Es seguro correrlo en cada inicio: si no hay migraciones pendientes, no hace nada.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GnosisDbContext>();
    await db.Database.MigrateAsync();
}

// ============================================================================
// MIDDLEWARES / PIPELINE DE PETICIONES
// ============================================================================

app.MapOpenApi();
app.MapScalarApiReference();

app.UseCors("PermitirBlazor");

// El orden importa: autenticación (¿quién eres?) siempre antes que autorización (¿puedes hacer esto?).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();