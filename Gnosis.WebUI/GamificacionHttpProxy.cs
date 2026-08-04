using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Gnosis.Business.Models;

namespace Gnosis.WebUI
{
    public interface IGamificacionHttpProxy
    {
        Task<GamificacionModel> ObtenerAsync();
        Task<SiembraModel> RegistrarSiembraAsync(bool crecio);
    }

    public class GamificacionHttpProxy : IGamificacionHttpProxy
    {
        private readonly HttpClient _httpClient;

        public GamificacionHttpProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GamificacionModel> ObtenerAsync()
        {
            return await _httpClient.GetFromJsonAsync<GamificacionModel>("api/Gamificacion")
                   ?? new GamificacionModel();
        }

        public async Task<SiembraModel> RegistrarSiembraAsync(bool crecio)
        {
            var respuesta = await _httpClient.PostAsJsonAsync("api/Gamificacion/siembras", new { Crecio = crecio });
            await LanzarSiErrorAsync(respuesta);
            return await respuesta.Content.ReadFromJsonAsync<SiembraModel>() ?? new SiembraModel { Crecio = crecio };
        }

        private static async Task LanzarSiErrorAsync(HttpResponseMessage respuesta)
        {
            if (respuesta.IsSuccessStatusCode) return;
            var detalle = await respuesta.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{(int)respuesta.StatusCode} {respuesta.ReasonPhrase}: {detalle}");
        }
    }
}
