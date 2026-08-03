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
                            Titulo = s,
                            FechaCreacion = DateTime.UtcNow,
                            Subtareas = new List<TareaModel>()
                        }).ToList();

                        tareas.Add(new TareaModel
                        {
                            Id = Guid.NewGuid(),
                            Titulo = t.Titulo,
                            FechaCreacion = DateTime.UtcNow,
                            Subtareas = subtareas
                        });
                    }
                    resultado.Tareas = tareas;
                }

                if (raw.Bloques != null && raw.Bloques.Any())
                {
                    resultado.Bloques = raw.Bloques.Select(b => new BloqueTiempoModel
                    {
                        Id = Guid.NewGuid(),
                        Titulo = b.Titulo,
                        FechaInicio = b.FechaHora,
                        FechaFin = b.FechaHora.AddMinutes(b.DuracionMinutos > 0 ? b.DuracionMinutos : 60)
                    }).ToList();
                }

                return resultado;
            }
            catch (Exception ex)
            {
                return new RespuestaIA { Texto = $"Error: {ex.Message}" };
            }
        }
    }
}