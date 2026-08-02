using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Gnosis.WebUI;
using Gnosis.WebUI.services;
using Gnosis.WebUI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<TemporizadorPomodoro>();
builder.Services.AddSingleton<EstadoFondoActual>();
builder.Services.AddScoped<FondoPersistenceService>();
builder.Services.AddScoped<NotificacionService>();

// Cuentas de usuario: estado de autenticación basado en el JWT guardado en localStorage,
// más el handler que lo agrega automáticamente a cada petición hacia la API.
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<TokenStorageService>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddTransient<JwtAuthorizationHandler>();

// URL de la API: viene de wwwroot/appsettings.json (+ appsettings.Development.json/Production.json
// según el entorno). En local apunta a localhost:5173; en Render, a la URL pública del Web Service
// de Gnosis.WebApi. Antes estaba fija en el código, lo que rompía cualquier despliegue fuera de dev.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("Falta ApiBaseUrl en wwwroot/appsettings.json.");

// Login/registro: no requieren token (todavía no existe uno), así que no llevan el handler.
builder.Services.AddHttpClient<IAuthHttpProxy, AuthHttpProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<ITareaHttpProxy, TareaHttpProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddHttpClient<IBloqueTiempoHttpProxy, BloqueTiempoHttpProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddHttpClient<ISesionEnfoqueHttpProxy, SesionEnfoqueHttpProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddHttpClient<IEstadisticasHttpProxy, EstadisticasHttpProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddHttpClient<GnosisIAService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<JwtAuthorizationHandler>();

await builder.Build().RunAsync();
