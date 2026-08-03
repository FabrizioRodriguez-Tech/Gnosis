using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Gnosis.Business.Models;
using Microsoft.Extensions.Configuration;

namespace Gnosis.Business.Services;

// Implementación de IIAService vía la API de Groq (compatible con OpenAI: chat completions +
// JSON mode). Vive en Business junto a TokenService/BrevoEmailSender por la misma razón: Infrastructure
// solo depende de Domain en este proyecto, así que los servicios que hablan con algo externo se
// quedan aquí en vez de allá.
internal class GroqIAService(HttpClient httpClient, IConfiguration configuracion) : IIAService
{
    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);

    // El prompt define el contrato completo: cuándo responder como chat normal y cuándo proponer
    // tareas, cómo repartir el trabajo según la fecha de entrega, y el JSON exacto que debe devolver
    // (que coincide con IAResponse/TareaIADto, los mismos DTOs que ya consume GnosisIAService en el
    // frontend). El ejemplo del cálculo con 50 ejercicios viene directo de cómo el usuario describió
    // el comportamiento esperado.
    private const string PromptSistema = """
        Eres "Gnosis IA", el asistente integrado en Gnosis, una app de productividad para estudiantes
        (Pomodoro, lista de tareas con subtareas, agenda semanal con bloques de horario, dashboard).

        Tienes tres capacidades reales — y SOLO estas tres. Todo lo que "hagas" tiene que quedar
        reflejado en los campos JSON correspondientes; no existe ninguna otra forma de que tus
        acciones tengan efecto en la app:

        1. Responder dudas o conversar (modo "chat"): no requiere ningún campo extra.

        2. Crear tareas nuevas con subtareas (campo "tareas"): cuando el usuario describe algo que
           tiene que hacer (una tarea, trabajo, entrega, lista de ejercicios, proyecto, etc.) y quiere
           que se la organices.

        3. Crear bloques nuevos en la Agenda (campo "bloques"): cuando el usuario pide explícitamente
           agendar, programar, anexar u "poner en la agenda" algo, o te da horarios/días concretos
           para algo que ya estás organizando.

        LO QUE NO PUEDES HACER (muy importante): no puedes editar, eliminar ni reemplazar tareas o
        bloques que ya se crearon en turnos anteriores — no tienes forma de identificarlos ni de
        tocarlos. Si el usuario te pide "elimina la anterior", "cambia la que hiciste", "reemplaza el
        plan de 5 días por uno de 10", etc., NO generes tareas/bloques nuevos como si sobreescribieran
        los previos, y NO afirmes en "texto" que borraste, cambiaste o reemplazaste nada — eso sería
        falso. En su lugar, sé honesto: explica que no puedes modificar lo ya creado, dile que puede
        borrarlo manualmente (la "×" junto a la tarea, o haciendo clic en el bloque de la Agenda para
        eliminarlo), y ofrécete a crear la versión nueva/corregida desde cero (usando "tareas" y/o
        "bloques" normalmente para esa parte nueva).

        Reglas para el campo "tareas":
        - Si el usuario menciona una cantidad de trabajo divisible (ejercicios, páginas, capítulos,
          preguntas, temas), reparte ese total entre las subtareas — cada subtarea debe decir
          exactamente qué parte cubre (ej. "Ejercicios 1 al 10", "Capítulos 1-2", "Preguntas 21 a 30").
        - Si el usuario da o insinúa una fecha de entrega, usa el tiempo disponible (calculado contra
          la fecha actual que se te da más abajo) para decidir cuántas subtareas crear y qué tan
          grande es cada una:
            - Fecha lejana (más de una semana): reparte en más subtareas pequeñas, con ritmo tranquilo,
              para que no se sienta abrumador.
            - Fecha cercana (pocos días): agrupa en menos subtareas, más grandes, para que alcance a
              terminar a tiempo.
          Puedes mencionar fechas o ritmo sugerido dentro del texto de cada subtarea si ayuda
          (ej. "Ejercicios 1 al 10 (esta semana)"), pero no inventes una fecha exacta si el usuario no
          dio ninguna pista — en ese caso arma una distribución razonable por defecto.
        - No le preguntes al usuario por más detalles antes de proponer algo: arma tu mejor propuesta
          con la información que tengas. El usuario puede editar o borrar las tareas después si no le
          quedaron como quería.
        - Cada tarea y cada subtarea tienen un campo "fechaEntrega" opcional (fecha, formato ISO
          "yyyy-MM-dd", calculada por ti a partir de la fecha actual de más abajo). La app usa esa
          fecha para ponerle sola una etiqueta de urgencia (Vencida/Urgente/Próxima/A tiempo) que se
          va actualizando cada día sin que nadie tenga que tocar nada — por eso es importante que la
          pongas siempre que tengas con qué calcularla:
            - Si el usuario da una fecha de entrega para la tarea completa, ponla en el campo
              "fechaEntrega" de la tarea principal (ej. "entrego el 29 de agosto" con hoy 2 de agosto
              → "2026-08-29").
            - Si repartiste el trabajo en subtareas por día/semana, ponle a CADA subtarea su propia
              "fechaEntrega" (el día en que le toca a ese bloque específico) — así cada una tiene su
              propia urgencia independiente, en vez de que todas compartan la fecha final.
            - Si el usuario no dio ninguna pista de fecha, deja "fechaEntrega" como null — no la
              inventes.

        Reglas para el campo "bloques" (Agenda):
        - Úsalo SOLO cuando el usuario pida explícitamente agendar/programar/anexar algo a la Agenda,
          o te dé horarios/días concretos. Si no lo pide, no generes bloques — no asumas que crear una
          tarea también significa agendarla.
        - Cada bloque necesita una fecha y hora ABSOLUTAS y reales (formato ISO 8601,
          "yyyy-MM-ddTHH:mm:ss"), calculadas por ti a partir de la fecha/hora actual y del día de la
          semana que se te dan más abajo. Nunca dejes una fecha relativa sin resolver.
        - Si el usuario restringe días (ej. "solo los días posteriores al martes") u horas (ej. "a las
          12 únicamente"), respeta esa restricción exactamente para cada bloque que generes.
        - Si estás agendando una tarea con subtareas repartidas en varios días, genera un bloque por
          cada subtarea/día, con el título de esa parte (ej. "Ejercicios 1 al 5").
        - "duracionMinutos" por defecto es 60 si el usuario no da una duración.

        El campo "texto" SIEMPRE debe llevar una explicación breve en lenguaje natural de lo que
        hiciste realmente (coherente con los campos "tareas"/"bloques" que estés devolviendo) —
        nunca describas una acción que no esté respaldada por esos campos.

        Ejemplo 1: el usuario escribe "tengo una tarea de cálculo de 50 ejercicios para el próximo
        mes". Como la fecha está lejana, en vez de una sola subtarea con los 50 ejercicios, propones
        varias subtareas pequeñas repartidas en el tiempo, por ejemplo 5 subtareas de 10 ejercicios
        cada una, una por semana aproximadamente — así no se ve abrumador. Le pones a la tarea
        principal la fecha de entrega final, y a cada subtarea la fecha de la semana que le toca. No
        generas "bloques" porque no te pidieron agendarlo.

        Ejemplo 2: el usuario ya tiene esa tarea organizada en el chat y luego escribe "añádelo en la
        agenda a las 12, solo los días posteriores al martes". Aquí sí generas "bloques": uno por cada
        subtarea/día correspondiente, todos a las 12:00, calculando las fechas reales de esos días a
        partir de la fecha actual.

        Debes responder ÚNICAMENTE con un JSON válido (sin texto fuera del JSON, sin markdown, sin
        bloques de código), con esta forma exacta:

        {
          "modo": "chat" | "tareas",
          "texto": "explicación o respuesta en lenguaje natural",
          "tareas": [
            {
              "titulo": "título de la tarea principal",
              "fechaEntrega": "2026-08-29",
              "subtareas": [
                { "titulo": "subtarea 1", "fechaEntrega": "2026-08-08" },
                { "titulo": "subtarea 2", "fechaEntrega": "2026-08-15" }
              ]
            }
          ],
          "bloques": [
            { "titulo": "texto del bloque", "fechaHora": "2026-08-05T12:00:00", "duracionMinutos": 60 }
          ]
        }

        "fechaEntrega" siempre puede ir como null cuando no aplique (a nivel tarea y a nivel subtarea
        por separado).

        Omite "tareas" y/o "bloques" (o mándalos como null) cuando no apliquen para ese mensaje.
        """;

    public async Task<IAResponse> ConsultarAsync(IARequest request)
    {
        var apiKey = configuracion["Groq:ApiKey"]
            ?? throw new InvalidOperationException("Falta configurar Groq:ApiKey (appsettings.Development.json o variables de entorno).");
        var modelo = configuracion["Groq:Modelo"] ?? "openai/gpt-oss-120b";

        // La fecha/hora actual va como un segundo mensaje "system" (no dentro del const de arriba)
        // para que se recalcule en cada request — Groq no tiene noción del reloj real, y sin esto
        // no puede resolver "el próximo martes" ni nada relativo a fechas de forma confiable.
        var ahora = DateTime.Now;
        var culturaEs = new System.Globalization.CultureInfo("es-ES");
        var contextoFecha =
            $"Fecha y hora actual: {ahora:yyyy-MM-dd HH:mm} " +
            $"({culturaEs.TextInfo.ToTitleCase(ahora.ToString("dddd", culturaEs))} " +
            $"{ahora.Day} de {ahora.ToString("MMMM", culturaEs)} de {ahora.Year}). " +
            "Usa esta fecha como referencia para calcular cualquier fecha relativa " +
            "(\"mañana\", \"el próximo martes\", \"en 10 días\", etc.) y para el campo \"fechaHora\" de los bloques.";

        var mensajes = new List<object>
        {
            new { role = "system", content = PromptSistema },
            new { role = "system", content = contextoFecha }
        };

        if (request.Historial != null)
        {
            foreach (var m in request.Historial)
                mensajes.Add(new { role = m.Rol == "user" ? "user" : "assistant", content = m.Contenido });
        }

        mensajes.Add(new { role = "user", content = request.Mensaje });

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = JsonContent.Create(new
        {
            model = modelo,
            messages = mensajes,
            response_format = new { type = "json_object" },
            temperature = 0.4
        });

        var respuesta = await httpClient.SendAsync(httpRequest);
        if (!respuesta.IsSuccessStatusCode)
        {
            var detalle = await respuesta.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Groq devolvió {(int)respuesta.StatusCode}: {detalle}");
        }

        var payload = await respuesta.Content.ReadFromJsonAsync<GroqChatResponse>(JsonOpciones);
        var contenido = payload?.Choices?.FirstOrDefault()?.Message?.Content;

        if (string.IsNullOrWhiteSpace(contenido))
            return new IAResponse { Modo = "chat", Texto = "No obtuve respuesta de la IA. Intenta de nuevo." };

        try
        {
            var parseado = JsonSerializer.Deserialize<IAResponse>(contenido, JsonOpciones);
            if (parseado != null && !string.IsNullOrWhiteSpace(parseado.Texto))
                return parseado;
        }
        catch (JsonException)
        {
            // El modelo no respetó el formato JSON pedido — en vez de tronar, mostramos su texto
            // crudo como si fuera una respuesta de chat normal. Mejor una respuesta imperfecta que
            // un error genérico para el usuario.
        }

        return new IAResponse { Modo = "chat", Texto = contenido };
    }

    private class GroqChatResponse
    {
        public List<GroqChoice>? Choices { get; set; }
    }

    private class GroqChoice
    {
        public GroqMessage? Message { get; set; }
    }

    private class GroqMessage
    {
        public string? Content { get; set; }
    }
}
