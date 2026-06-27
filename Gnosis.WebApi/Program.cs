using Gnosis.WebApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// CONFIGURACIÓN DE SERVICIOS
// ============================================================================

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirBlazor", policy =>
    {
        policy.WithOrigins("https://localhost:44372", "http://localhost:44372")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddProjectServices(builder.Configuration);

var app = builder.Build();

// ============================================================================
// MIDDLEWARES / PIPELINE DE PETICIONES
// ============================================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("PermitirBlazor");

app.UseAuthorization();
app.MapControllers();

await app.RunAsync();