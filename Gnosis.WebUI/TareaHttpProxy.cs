using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Gnosis.Business.Models;

namespace Gnosis.WebUI
{
    public interface ITareaHttpProxy
    {
        Task<IEnumerable<TareaModel>> ObtenerTodasAsync();
        Task<TareaModel> CrearTareaAsync(TareaModel nuevaTarea);
        Task ActualizarEstadoTareaAsync(Guid id, bool isCompletada);

        // NUEVO: Registramos la acción de borrado en la interfaz
        Task EliminarTareaAsync(Guid id);
    }

    public class TareaHttpProxy : ITareaHttpProxy
    {
        private readonly HttpClient _httpClient;

        public TareaHttpProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<TareaModel>> ObtenerTodasAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<TareaModel>>("api/Tareas")
                   ?? new List<TareaModel>();
        }

        public async Task<TareaModel> CrearTareaAsync(TareaModel nuevaTarea)
        {
            var respuesta = await _httpClient.PostAsJsonAsync("api/Tareas", nuevaTarea);
            respuesta.EnsureSuccessStatusCode();
            return await respuesta.Content.ReadFromJsonAsync<TareaModel>() ?? nuevaTarea;
        }

        public async Task ActualizarEstadoTareaAsync(Guid id, bool isCompletada)
        {
            // El servidor espera { "isCompletada": true } (ActualizarEstadoRequest), no un booleano
            // suelto. Antes se mandaba el bool directo y la API respondía 400 Bad Request siempre.
            var respuesta = await _httpClient.PutAsJsonAsync($"api/Tareas/{id}/estado", new { IsCompletada = isCompletada });
            respuesta.EnsureSuccessStatusCode();
        }

        // NUEVO: Implementamos la llamada DELETE hacia el controlador de la API
        public async Task EliminarTareaAsync(Guid id)
        {
            var respuesta = await _httpClient.DeleteAsync($"api/Tareas/{id}");
            respuesta.EnsureSuccessStatusCode();
        }
    }
}