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
        public List<string> Subtareas { get; set; } = new();
    }

    public class BloqueIADto
    {
        public string Titulo { get; set; } = string.Empty;

        // Fecha y hora absolutas calculadas por la IA a partir de la fecha actual que se le da
        // en el prompt (no relativas — "el próximo martes" ya debe venir resuelto a una fecha real).
        public DateTime FechaHora { get; set; }
        public int DuracionMinutos { get; set; } = 60;
    }
}