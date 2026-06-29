using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Gnosis.WebUI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<TemporizadorPomodoro>();
builder.Services.AddSingleton<EstadoFondoActual>();
builder.Services.AddScoped<FondoPersistenceService>();

builder.Services.AddHttpClient<ITareaHttpProxy, TareaHttpProxy>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5173");
});

await builder.Build().RunAsync();