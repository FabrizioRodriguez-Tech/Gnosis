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

    // Nombres en español a mano (sin CultureInfo/ICU): evita que esto reviente en un entorno con
    // globalización invariante (ej. contenedores recortados) donde new CultureInfo("es-ES") lanza
    // CultureNotFoundException — esto ya tumbó CADA consulta a la IA una vez, ver ConsultarAsync.
    private static readonly string[] DiasEs = { "domingo", "lunes", "martes", "miércoles", "jueves", "viernes", "sábado" };
    private static readonly string[] MesesEs = { "enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre" };

    // El prompt define el contrato completo: cuándo responder como chat normal y cuándo proponer
    // tareas, cómo repartir el trabajo según la fecha de entrega, y el JSON exacto que debe devolver
    // (que coincide con IAResponse/TareaIADto, los mismos DTOs que ya consume GnosisIAService en el
    // frontend). El ejemplo del cálculo con 50 ejercicios viene directo de cómo el usuario describió
    // el comportamiento esperado.
    private const string PromptSistemaChat = """
        Eres "Eve", el asistente de IA integrado en Gnosis, una app de productividad para estudiantes
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
        - Si el usuario pide que un bloque quede VINCULADO a una tarea o subtarea que estás creando
          en esta MISMA respuesta (ej. "que esa tarea esté ligada al bloque"), ponle al bloque el
          campo "tituloTareaVinculada" con el título EXACTO (carácter por carácter) de esa tarea o
          subtarea tal cual la escribiste en "tareas". Solo funciona con tareas/subtareas que estés
          creando en este mismo mensaje — NO puedes vincular un bloque a una tarea de un turno
          anterior (no tienes forma de identificarla). Si no aplica, deja "tituloTareaVinculada" en
          null, y no afirmes en "texto" que vinculaste algo si no puede ser así.

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
            {
              "titulo": "texto del bloque",
              "fechaHora": "2026-08-05T12:00:00",
              "duracionMinutos": 60,
              "tituloTareaVinculada": null
            }
          ]
        }

        "fechaEntrega" siempre puede ir como null cuando no aplique (a nivel tarea y a nivel subtarea
        por separado).

        Omite "tareas" y/o "bloques" (o mándalos como null) cuando no apliquen para ese mensaje.
        """;

    private const string PromptSistemaDesglose = """
        Eres "Eve", el asistente de IA integrado en Gnosis, una app de productividad para estudiantes.
        Tu única tarea ahora es desglosar UNA tarea que el usuario ya tiene en su lista en 4 o 5
        subtareas lógicas, concretas y accionables (no genéricas como "trabajar en el proyecto" —
        cada una debe describir un paso real y específico).

        Responde ÚNICAMENTE con un JSON válido (sin texto fuera del JSON, sin markdown), con esta
        forma exacta:

        { "subtareas": ["subtarea 1", "subtarea 2", "subtarea 3", "subtarea 4"] }

        Entre 4 y 5 elementos, ni más ni menos.
        """;

    private const string PromptSistemaResumenDia = """
        Eres "Eve", el asistente de IA integrado en Gnosis, una app de productividad para estudiantes.
        Se te da la lista de tareas que el usuario completó hoy (y opcionalmente minutos de enfoque y
        sesiones de Pomodoro). Genera un resumen ejecutivo breve (2 a 4 frases), en tono cercano y
        motivador pero sin exagerar, que destaque lo logrado y, si aplica, sugiera un cierre de día
        razonable. Si no hay tareas completadas, dilo con honestidad y anima a retomar mañana — no
        inventes logros que no están en la lista.

        Responde ÚNICAMENTE con un JSON válido (sin texto fuera del JSON, sin markdown), con esta
        forma exacta:

        { "resumen": "texto del resumen ejecutivo" }
        """;

    private const string PromptSistemaEstimador = """
        Eres "Eve", el asistente de IA integrado en Gnosis, una app de productividad para estudiantes.
        Se te da el título (y opcionalmente la descripción) de una tarea. Estima cuántos ciclos de
        Pomodoro (bloques de ~25 minutos de trabajo enfocado) probablemente requiera completarla,
        basándote en la complejidad y alcance que describe el título/descripción. Sé realista: la
        mayoría de tareas de estudio caben entre 1 y 6 pomodoros; usa números más altos solo para
        tareas claramente grandes (proyectos, entregas con muchas partes).

        Responde ÚNICAMENTE con un JSON válido (sin texto fuera del JSON, sin markdown), con esta
        forma exacta:

        { "pomodoros": 3, "justificacion": "explicación breve de por qué" }
        """;

    public async Task<IAResponse> ConsultarAsync(IARequest request)
    {
        var mensajes = new List<object>
        {
            new { role = "system", content = PromptSistemaChat },
            new { role = "system", content = ContextoFechaActual() }
        };

        if (request.Historial != null)
        {
            foreach (var m in request.Historial)
                mensajes.Add(new { role = m.Rol == "user" ? "user" : "assistant", content = m.Contenido });
        }

        mensajes.Add(new { role = "user", content = request.Mensaje });

        var contenido = await EjecutarLlamadaAsync(mensajes);

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

    public async Task<DesglosarTareaResponse> DesglosarTareaAsync(DesglosarTareaRequest request)
    {
        var detalle = $"Tarea: {request.TituloTarea}";
        if (!string.IsNullOrWhiteSpace(request.DescripcionTarea))
            detalle += $"\nDescripción: {request.DescripcionTarea}";
        if (request.FechaEntrega.HasValue)
            detalle += $"\nFecha de entrega: {request.FechaEntrega:yyyy-MM-dd}";

        var mensajes = new List<object>
        {
            new { role = "system", content = PromptSistemaDesglose },
            new { role = "system", content = ContextoFechaActual() },
            new { role = "user", content = detalle }
        };

        var contenido = await EjecutarLlamadaAsync(mensajes);

        try
        {
            var parseado = JsonSerializer.Deserialize<DesglosarTareaResponse>(contenido, JsonOpciones);
            if (parseado?.Subtareas != null && parseado.Subtareas.Count > 0)
                return parseado;
        }
        catch (JsonException)
        {
            // Se maneja abajo devolviendo una lista vacía en vez de tronar.
        }

        return new DesglosarTareaResponse();
    }

    public async Task<ResumenDiaResponse> GenerarResumenDiaAsync(ResumenDiaRequest request)
    {
        var detalle = request.TareasCompletadas.Count == 0
            ? "No se completó ninguna tarea hoy."
            : "Tareas completadas hoy:\n" + string.Join("\n", request.TareasCompletadas.Select(t => $"- {t}"));

        detalle += $"\nMinutos de enfoque: {request.MinutosEnfoque}\nSesiones de Pomodoro: {request.SesionesEnfoque}";

        var mensajes = new List<object>
        {
            new { role = "system", content = PromptSistemaResumenDia },
            new { role = "user", content = detalle }
        };

        var contenido = await EjecutarLlamadaAsync(mensajes);

        try
        {
            var parseado = JsonSerializer.Deserialize<ResumenDiaResponse>(contenido, JsonOpciones);
            if (parseado != null && !string.IsNullOrWhiteSpace(parseado.Resumen))
                return parseado;
        }
        catch (JsonException)
        {
            // Se maneja abajo.
        }

        return new ResumenDiaResponse { Resumen = contenido ?? "No se pudo generar el resumen." };
    }

    public async Task<EstimarPomodorosResponse> EstimarPomodorosAsync(EstimarPomodorosRequest request)
    {
        var detalle = $"Tarea: {request.TituloTarea}";
        if (!string.IsNullOrWhiteSpace(request.DescripcionTarea))
            detalle += $"\nDescripción: {request.DescripcionTarea}";

        var mensajes = new List<object>
        {
            new { role = "system", content = PromptSistemaEstimador },
            new { role = "user", content = detalle }
        };

        var contenido = await EjecutarLlamadaAsync(mensajes);

        try
        {
            var parseado = JsonSerializer.Deserialize<EstimarPomodorosResponse>(contenido, JsonOpciones);
            if (parseado != null && parseado.Pomodoros > 0)
                return parseado;
        }
        catch (JsonException)
        {
            // Se maneja abajo.
        }

        return new EstimarPomodorosResponse { Pomodoros = 1, Justificacion = "Estimación por defecto (la IA no respondió en el formato esperado)." };
    }

    // La fecha/hora actual va como un mensaje "system" recalculado en cada request — Groq no
    // tiene noción del reloj real, y sin esto no puede resolver "el próximo martes" ni nada
    // relativo a fechas de forma confiable.
    private static string ContextoFechaActual()
    {
        var ahora = DateTime.Now;
        var nombreDia = DiasEs[(int)ahora.DayOfWeek];
        var nombreMes = MesesEs[ahora.Month - 1];
        return $"Fecha y hora actual: {ahora:yyyy-MM-dd HH:mm} " +
               $"({char.ToUpper(nombreDia[0]) + nombreDia[1..]} {ahora.Day} de {nombreMes} de {ahora.Year}). " +
               "Usa esta fecha como referencia para calcular cualquier fecha relativa " +
               "(\"mañana\", \"el próximo martes\", \"en 10 días\", etc.) y para el campo \"fechaHora\" de los bloques.";
    }

    // Helper compartido: arma la petición HTTP a Groq (JSON mode) con la lista de mensajes que le
    // pase cada método público, y devuelve el contenido de texto ya extraído de la respuesta.
    // Centraliza la lectura de la API key/modelo, el manejo de errores HTTP y el parseo del
    // envoltorio de "choices" de Groq — cada método público solo se preocupa de su propio prompt y
    // de cómo interpretar el JSON de vuelta.
    private async Task<string> EjecutarLlamadaAsync(List<object> mensajes)
    {
        var apiKey = configuracion["Groq:ApiKey"]
            ?? throw new InvalidOperationException("Falta configurar Groq:ApiKey (appsettings.Development.json o variables de entorno).");
        var modelo = configuracion["Groq:Modelo"] ?? "openai/gpt-oss-120b";

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
            var detalleError = await respuesta.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Groq devolvió {(int)respuesta.StatusCode}: {detalleError}");
        }

        var payload = await respuesta.Content.ReadFromJsonAsync<GroqChatResponse>(JsonOpciones);
        return payload?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
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
