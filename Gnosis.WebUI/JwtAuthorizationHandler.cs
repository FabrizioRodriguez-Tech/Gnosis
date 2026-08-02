using System.Net.Http.Headers;
using Gnosis.WebUI.services;

namespace Gnosis.WebUI;

// DelegatingHandler que se cuelga de cada HttpClient hacia la API (Tareas, Agenda, Sesiones,
// Estadísticas) y le agrega "Authorization: Bearer <token>" automáticamente, para no repetir
// esa lógica en cada proxy.
public class JwtAuthorizationHandler(TokenStorageService tokenStorage) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokenStorage.ObtenerTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
