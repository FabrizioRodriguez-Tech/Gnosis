using Gnosis.WebApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Controladores y OpenAPI Nativo de .NET 10
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 2. Invocar de forma segura la inyección interna
builder.Services.AddProjectServices(builder.Configuration);

var app = builder.Build();

// 3. Habilitar la documentación interactiva en desarrollo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthorization();
app.MapControllers();

await app.RunAsync();