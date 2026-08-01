// Gnosis.WebUI/Services/SoundService.cs
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Gnosis.WebUI.Services
{
    public interface ISoundService
    {
        Task ReproducirSonidoAsync(string sonido = "notificacion");
        Task ReproducirTickAsync();
    }

    public class SoundService : ISoundService
    {
        private readonly IJSRuntime _js;

        public SoundService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task ReproducirSonidoAsync(string sonido = "notificacion")
        {
            try
            {
                // Usar Web Audio API para generar tono
                await _js.InvokeVoidAsync("gnosis.reproducirSonido", sonido);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SoundService] Error: {ex.Message}");
            }
        }

        public async Task ReproducirTickAsync()
        {
            try
            {
                await _js.InvokeVoidAsync("gnosis.reproducirTick");
            }
            catch { }
        }
    }
}