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

builder.Services.AddHttpClient<ITareaHttpProxy, TareaHttpProxy>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5173");
});

builder.Services.AddHttpClient<IBloqueTiempoHttpProxy, BloqueTiempoHttpProxy>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5173");
});

builder.Services.AddHttpClient<ISesionEnfoqueHttpProxy, SesionEnfoqueHttpProxy>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5173");
});

builder.Services.AddHttpClient<IEstadisticasHttpProxy, EstadisticasHttpProxy>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5173");
});

builder.Services.AddHttpClient<GnosisIAService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5173");
});

await builder.Build().RunAsync();
