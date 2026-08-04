using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Gnosis.Business.Models;

namespace Gnosis.WebUI
{
    public interface IBloqueTiempoHttpProxy
    {
        Task<IEnumerable<BloqueTiempoModel>> ObtenerPorRangoAsync(DateTime desde, DateTime hasta);
        Task<BloqueTiempoModel> CrearAsync(BloqueTiempoModel nuevoBloque);
        Task ActualizarAsync(BloqueTiempoModel bloqueActualizado);
        Task EliminarAsync(Guid id);
    }

    public class BloqueTiempoHttpProxy : IBloqueTiempoHttpProxy
    {
        private readonly HttpClient _httpClient;

        public BloqueTiempoHttpProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<BloqueTiempoModel>> ObtenerPorRangoAsync(DateTime desde, DateTime hasta)
        {
            var query = $"api/BloquesTiempo?desde={Uri.EscapeDataString(desde.ToString("o"))}&hasta={Uri.EscapeDataString(hasta.ToString("o"))}";
            return await _httpClient.GetFromJsonAsync<IEnumerable<BloqueTiempoModel>>(query)
                   ?? new List<BloqueTiempoModel>();
        }

        public async Task<BloqueTiempoModel> CrearAsync(BloqueTiempoModel nuevoBloque)
        {
            var respuesta = await _httpClient.PostAsJsonAsync("api/BloquesTiempo", nuevoBloque);
            await LanzarSiErrorAsync(respuesta);
            return await respuesta.Content.ReadFromJsonAsync<BloqueTiempoModel>() ?? nuevoBloque;
        }

        public async Task ActualizarAsync(BloqueTiempoModel bloqueActualizado)
        {
            var respuesta = await _httpClient.PutAsJsonAsync($"api/BloquesTiempo/{bloqueActualizado.Id}", bloqueActualizado);
            await LanzarSiErrorAsync(respuesta);
        }

        public async Task EliminarAsync(Guid id)
        {
            var respuesta = await _httpClient.DeleteAsync($"api/BloquesTiempo/{id}");
            await LanzarSiErrorAsync(respuesta);
        }

        // Mismo motivo que en TareaHttpProxy: EnsureSuccessStatusCode() sola no trae el cuerpo de
        // la respuesta, así que los errores (ej. FK violation si el bloque queda vinculado a una
        // tarea que no llegó a guardarse) se veían siempre igual, sin pista de la causa real.
        private static async Task LanzarSiErrorAsync(HttpResponseMessage respuesta)
        {
            if (respuesta.IsSuccessStatusCode) return;
            var detalle = await respuesta.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{(int)respuesta.StatusCode} {respuesta.ReasonPhrase}: {detalle}");
        }
    }
}
