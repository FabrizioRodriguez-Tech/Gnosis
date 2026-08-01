// Ubicación real: Gnosis.WebUI/NotificacionService.cs
using Microsoft.JSInterop;

namespace Gnosis.WebUI.Services
{
    public class NotificacionService
    {
        private readonly IJSRuntime _js;

        public NotificacionService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task SolicitarPermisoAsync()
        {
            try
            {
                await _js.InvokeVoidAsync("gnosis.notificaciones.solicitarPermiso");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificacionService] Error al solicitar permiso: {ex.Message}");
            }
        }

        public async Task NotificarAsync(string titulo, string cuerpo)
        {
            try
            {
                await _js.InvokeVoidAsync("gnosis.notificaciones.notificar", titulo, cuerpo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificacionService] Error al notificar: {ex.Message}");
            }
        }
    }
}
