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
    }

    public class TareaIADto
    {
        public string Titulo { get; set; } = string.Empty;
        public List<string> Subtareas { get; set; } = new();
    }
}