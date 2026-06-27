using Gnosis.Business.Services;
using Gnosis.Domain.Interfaces;
using Gnosis.WebUI;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<TemporizadorPomodoro>();

builder.Services.AddHttpClient<ITareaHttpProxy, TareaHttpProxy>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5173");
});

builder.Services.AddHttpClient<IVideoService, VideoService>();

await builder.Build().RunAsync();