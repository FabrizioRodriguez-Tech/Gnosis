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

        // INTERFAZ: Registramos el método para actualizar el estado
        Task ActualizarEstadoTareaAsync(Guid id, bool isCompletada);
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

        // IMPLEMENTACIÓN: Enviamos el cambio mediante un PUT hacia la API
        public async Task ActualizarEstadoTareaAsync(Guid id, bool isCompletada)
        {
            var respuesta = await _httpClient.PutAsJsonAsync($"api/Tareas/{id}/estado", isCompletada);
            respuesta.EnsureSuccessStatusCode();
        }
    }
}