// Ubicación real: Gnosis.Business/Models/IAModels.cs
namespace Gnosis.Business.Models
{
    public class IARequest
    {
        public string Mensaje { get; set; } = string.Empty;

        // Historial de mensajes anteriores para dar contexto a Groq
        public List<MensajeHistorialDto>? Historial { get; set; }
    }

    public class MensajeHistorialDto
    {
        public string Rol { get; set; } = string.Empty;      // "user" o "assistant"
        public string Contenido { get; set; } = string.Empty;
    }

    public class IAResponse
    {
        public string Modo { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public List<TareaIADto>? Tareas { get; set; }

        // Bloques de Agenda que la IA propone crear (solo cuando el usuario pide explícitamente
        // programar/agendar algo). Independiente de "Tareas": puede venir solo, o junto con tareas
        // nuevas si el usuario pidió ambas cosas en el mismo mensaje.
        public List<BloqueIADto>? Bloques { get; set; }
    }

    public class TareaIADto
    {
        public string Titulo { get; set; } = string.Empty;

        // Fecha de entrega opcional de la tarea principal (si el usuario la dio o la insinuó).
        // Habilita la etiqueta de urgencia automática (Vencida/Urgente/Próxima/A tiempo) en la UI.
        public DateTime? FechaEntrega { get; set; }

        public List<SubtareaIADto> Subtareas { get; set; } = new();
    }

    public class SubtareaIADto
    {
        public string Titulo { get; set; } = string.Empty;

        // Fecha de entrega opcional de ESTA subtarea en particular (ej. el día que le toca a ese
        // bloque de ejercicios dentro del plan) — cada subtarea puede tener su propia urgencia,
        // independiente de la fecha límite general de la tarea principal.
        public DateTime? FechaEntrega { get; set; }
    }

    public class BloqueIADto
    {
        public string Titulo { get; set; } = string.Empty;

        // Fecha y hora absolutas calculadas por la IA a partir de la fecha actual que se le da
        // en el prompt (no relativas — "el próximo martes" ya debe venir resuelto a una fecha real).
        public DateTime FechaHora { get; set; }
        public int DuracionMinutos { get; set; } = 60;

        // Opcional: título EXACTO de una tarea o subtarea que la IA está creando en la MISMA
        // respuesta (dentro de "tareas"), para vincular este bloque a ella. GnosisIAService hace
        // el match por título después de generar los Guids reales — solo funciona para tareas
        // nuevas del mismo turno, no para tareas ya existentes de antes.
        public string? TituloTareaVinculada { get; set; }
    }
}