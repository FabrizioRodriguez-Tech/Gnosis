// Ubicación real: Gnosis.WebUI/FondoPersistenceService.cs
using Microsoft.JSInterop;

namespace Gnosis.WebUI
{
    public class FondoPersistenceService
    {
        private const string Clave = "gnosis_fondo_seleccionado";
        private readonly IJSRuntime _js;

        public FondoPersistenceService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<string?> ObtenerFondoGuardadoAsync()
        {
            try
            {
                return await _js.InvokeAsync<string?>("localStorage.getItem", Clave);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Gnosis] Error al leer fondo de localStorage: {ex.Message}");
                return null;
            }
        }

        public async Task GuardarFondoAsync(string fondoId)
        {
            try
            {
                await _js.InvokeVoidAsync("localStorage.setItem", Clave, fondoId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Gnosis] Error al guardar fondo en localStorage: {ex.Message}");
            }
        }
    }
}