using Gnosis.WebApi;
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

builder.Services.AddProjectServices(builder.Configuration);

var app = builder.Build();

// ============================================================================
// MIDDLEWARES / PIPELINE DE PETICIONES
// ============================================================================

app.MapOpenApi();
app.MapScalarApiReference();

app.UseCors("PermitirBlazor");

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();