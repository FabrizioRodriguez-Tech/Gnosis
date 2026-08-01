using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Gnosis.WebUI
{
    public interface ISesionEnfoqueHttpProxy
    {
        Task RegistrarSesionAsync(string tipoSesion, int duracionMinutos);
    }

    public class SesionEnfoqueHttpProxy : ISesionEnfoqueHttpProxy
    {
        private readonly HttpClient _httpClient;

        public SesionEnfoqueHttpProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task RegistrarSesionAsync(string tipoSesion, int duracionMinutos)
        {
            var respuesta = await _httpClient.PostAsJsonAsync("api/SesionesEnfoque", new
            {
                TipoSesion = tipoSesion,
                DuracionMinutos = duracionMinutos
            });
            respuesta.EnsureSuccessStatusCode();
        }
    }
}
