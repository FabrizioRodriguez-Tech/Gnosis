// Ubicación real: Gnosis.WebUI/GnosisIAService.cs
using System.Net.Http.Json;
using Gnosis.Business.Models;

namespace Gnosis.WebUI.services
{
    public class RespuestaIA
    {
        public string Texto { get; set; } = string.Empty;
        public List<TareaModel>? Tareas { get; set; }
        public bool CreoTareas => Tareas != null && Tareas.Any();
        public List<BloqueTiempoModel>? Bloques { get; set; }
        public bool CreoBloques => Bloques != null && Bloques.Any();
    }

    public class MensajeHistorial
    {
        public string Rol { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
    }

    public class GnosisIAService
    {
        private readonly HttpClient _http;

        public GnosisIAService(HttpClient http)
        {
            _http = http;
        }

        public async Task<RespuestaIA> ConsultarAsync(string mensaje, IEnumerable<MensajeHistorial>? historial = null)
        {
            try
            {
                // Conversión explícita de MensajeHistorial (WebUI) → MensajeHistorialDto (Business)
                var historialDto = historial?
                    .Select(m => new MensajeHistorialDto
                    {
                        Rol = m.Rol,
                        Contenido = m.Contenido
                    })
                    .ToList();

                var request = new IARequest
                {
                    Mensaje = mensaje,
                    Historial = historialDto
                };

                var response = await _http.PostAsJsonAsync("api/IA/consultar", request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return new RespuestaIA { Texto = $"Error del servidor: {error}" };
                }

                var raw = await response.Content.ReadFromJsonAsync<IAResponse>();

                if (raw == null)
                    return new RespuestaIA { Texto = "Sin respuesta del servidor." };

                var resultado = new RespuestaIA { Texto = raw.Texto };

                if (raw.Modo == "tareas" && raw.Tareas != null)
                {
                    var tareas = new List<TareaModel>();
                    foreach (var t in raw.Tareas)
                    {
                        var subtareas = t.Subtareas.Select(s => new TareaModel
                        {
                            Id = Guid.NewGuid(),
                            Titulo = s.Titulo,
                            FechaCreacion = DateTime.UtcNow,
                            FechaEntrega = s.FechaEntrega,
                            Subtareas = new List<TareaModel>()
                        }).ToList();

                        tareas.Add(new TareaModel
                        {
                            Id = Guid.NewGuid(),
                            Titulo = t.Titulo,
                            FechaCreacion = DateTime.UtcNow,
                            FechaEntrega = t.FechaEntrega,
                            Subtareas = subtareas
                        });
                    }
                    resultado.Tareas = tareas;
                }

                if (raw.Bloques != null && raw.Bloques.Any())
                {
                    // Título → Id de las tareas/subtareas que se acaban de crear en esta misma
                    // respuesta, para poder resolver "tituloTareaVinculada" a un Guid real. Solo
                    // cubre tareas nuevas de este turno (las de antes no tienen forma de identificarse).
                    var idsPorTitulo = (resultado.Tareas ?? new List<TareaModel>())
                        .SelectMany(t => new[] { t }.Concat(t.Subtareas ?? new List<TareaModel>()))
                        .GroupBy(t => t.Titulo)
                        .ToDictionary(g => g.Key, g => g.First().Id);

                    resultado.Bloques = raw.Bloques.Select(b => new BloqueTiempoModel
                    {
                        Id = Guid.NewGuid(),
                        Titulo = b.Titulo,
                        FechaInicio = b.FechaHora,
                        FechaFin = b.FechaHora.AddMinutes(b.DuracionMinutos > 0 ? b.DuracionMinutos : 60),
                        TareaId = !string.IsNullOrEmpty(b.TituloTareaVinculada) && idsPorTitulo.TryGetValue(b.TituloTareaVinculada, out var idVinculado)
                            ? idVinculado
                            : null
                    }).ToList();
                }

                return resultado;
            }
            catch (Exception ex)
            {
                return new RespuestaIA { Texto = $"Error: {ex.Message}" };
            }
        }

        // Task Breaker: pide 4-5 subtareas concretas para una tarea existente.
        public async Task<List<string>> DesglosarTareaAsync(string tituloTarea, string? descripcionTarea = null, DateTime? fechaEntrega = null)
        {
            try
            {
                var request = new DesglosarTareaRequest
                {
                    TituloTarea = tituloTarea,
                    DescripcionTarea = descripcionTarea,
                    FechaEntrega = fechaEntrega
                };
                var response = await _http.PostAsJsonAsync("api/IA/desglosar", request);
                if (!response.IsSuccessStatusCode) return new List<string>();

                var raw = await response.Content.ReadFromJsonAsync<DesglosarTareaResponse>();
                return raw?.Subtareas ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        // Daily Retrospective: resumen ejecutivo de las tareas completadas hoy.
        public async Task<string> GenerarResumenDiaAsync(List<string> tareasCompletadas, int minutosEnfoque = 0, int sesionesEnfoque = 0)
        {
            try
            {
                var request = new ResumenDiaRequest
                {
                    TareasCompletadas = tareasCompletadas,
                    MinutosEnfoque = minutosEnfoque,
                    SesionesEnfoque = sesionesEnfoque
                };
                var response = await _http.PostAsJsonAsync("api/IA/resumen-dia", request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return $"No se pudo generar el resumen: {error}";
                }

                var raw = await response.Content.ReadFromJsonAsync<ResumenDiaResponse>();
                return raw?.Resumen ?? "No se pudo generar el resumen.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Estimador de Pomodoros: cuántos ciclos probablemente requiera una tarea.
        public async Task<EstimarPomodorosResponse> EstimarPomodorosAsync(string tituloTarea, string? descripcionTarea = null)
        {
            try
            {
                var request = new EstimarPomodorosRequest { TituloTarea = tituloTarea, DescripcionTarea = descripcionTarea };
                var response = await _http.PostAsJsonAsync("api/IA/estimar-pomodoros", request);
                if (!response.IsSuccessStatusCode) return new EstimarPomodorosResponse { Pomodoros = 0 };

                var raw = await response.Content.ReadFromJsonAsync<EstimarPomodorosResponse>();
                return raw ?? new EstimarPomodorosResponse { Pomodoros = 0 };
            }
            catch
            {
                return new EstimarPomodorosResponse { Pomodoros = 0 };
            }
        }
    }
}