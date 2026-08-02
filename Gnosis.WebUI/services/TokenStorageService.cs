using Microsoft.JSInterop;

namespace Gnosis.WebUI.services;

// Guarda/lee el JWT en localStorage del navegador (persiste entre recargas de página y cierres
// de pestaña, a diferencia de una variable en memoria). Es el único lugar del proyecto que toca
// localStorage directamente, para no repetir el nombre de la clave en todos lados.
public class TokenStorageService(IJSRuntime js)
{
    private const string Clave = "gnosis_token";

    public async Task GuardarTokenAsync(string token) =>
        await js.InvokeVoidAsync("localStorage.setItem", Clave, token);

    public async Task<string?> ObtenerTokenAsync()
    {
        try
        {
            return await js.InvokeAsync<string?>("localStorage.getItem", Clave);
        }
        catch
        {
            // Puede fallar durante el prerender/primer render antes de que el JS interop esté listo.
            return null;
        }
    }

    public async Task BorrarTokenAsync() =>
        await js.InvokeVoidAsync("localStorage.removeItem", Clave);
}
