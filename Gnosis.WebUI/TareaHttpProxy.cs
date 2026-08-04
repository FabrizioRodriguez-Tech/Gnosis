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
        Task ActualizarFechaEntregaAsync(Guid id, DateTime? fechaEntrega);
        Task ActualizarEtiquetaAsync(Guid id, string? etiquetaManual);

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
            await LanzarSiErrorAsync(respuesta);
            return await respuesta.Content.ReadFromJsonAsync<TareaModel>() ?? nuevaTarea;
        }

        public async Task ActualizarEstadoTareaAsync(Guid id, bool isCompletada)
        {
            // El servidor espera { "isCompletada": true } (ActualizarEstadoRequest), no un booleano
            // suelto. Antes se mandaba el bool directo y la API respondía 400 Bad Request siempre.
            var respuesta = await _httpClient.PutAsJsonAsync($"api/Tareas/{id}/estado", new { IsCompletada = isCompletada });
            await LanzarSiErrorAsync(respuesta);
        }

        public async Task ActualizarFechaEntregaAsync(Guid id, DateTime? fechaEntrega)
        {
            var respuesta = await _httpClient.PutAsJsonAsync($"api/Tareas/{id}/fecha-entrega", new { FechaEntrega = fechaEntrega });
            await LanzarSiErrorAsync(respuesta);
        }

        public async Task ActualizarEtiquetaAsync(Guid id, string? etiquetaManual)
        {
            var respuesta = await _httpClient.PutAsJsonAsync($"api/Tareas/{id}/etiqueta", new { EtiquetaManual = etiquetaManual });
            await LanzarSiErrorAsync(respuesta);
        }

        // NUEVO: Implementamos la llamada DELETE hacia el controlador de la API
        public async Task EliminarTareaAsync(Guid id)
        {
            var respuesta = await _httpClient.DeleteAsync($"api/Tareas/{id}");
            await LanzarSiErrorAsync(respuesta);
        }

        // EnsureSuccessStatusCode() por sí solo tira un mensaje genérico ("Response status code
        // does not indicate success: 500") sin el cuerpo real de la respuesta — por eso los errores
        // de creación de tareas se veían siempre igual ("Verifica tu conexión...") sin pista de la
        // causa real (ej. una columna nueva que faltaba porque no se aplicó una migración). Esto lee
        // el cuerpo y lo mete en la excepción para que quede en la consola del navegador.
        private static async Task LanzarSiErrorAsync(HttpResponseMessage respuesta)
        {
            if (respuesta.IsSuccessStatusCode) return;
            var detalle = await respuesta.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{(int)respuesta.StatusCode} {respuesta.ReasonPhrase}: {detalle}");
        }
    }
}