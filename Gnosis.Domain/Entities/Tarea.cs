using System;
using System.Collections.Generic;

namespace Gnosis.Domain.Entities
{
    public class Tarea
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Dueño de la tarea. Toda consulta/escritura debe filtrar por este campo
        // para que cada usuario solo vea sus propios datos.
        public Guid UsuarioId { get; set; }

        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool IsCompletada { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Se completa cuando IsCompletada pasa a true; se limpia si vuelve a false. Usado para el dashboard.
        public DateTime? FechaCompletada { get; set; }

        // Fecha de entrega opcional (la puede poner el usuario a mano, o la IA cuando el mensaje
        // trae o insinúa una fecha límite). A partir de esta fecha se calcula la etiqueta de
        // urgencia (Vencida/Urgente/Próxima/A tiempo) — no se guarda la etiqueta en sí, se recalcula
        // cada vez a partir de esta fecha para que "avance sola" con el paso de los días.
        public DateTime? FechaEntrega { get; set; }

        // Etiqueta puesta a mano por el usuario, que pisa el cálculo automático a partir de
        // FechaEntrega (ej. si quiere marcar algo "Urgente" aunque falten varios días). Null =
        // no hay override, se usa la etiqueta calculada.
        public string? EtiquetaManual { get; set; }

        // Relación reflexiva para desglose jerárquico (Árbol de subtareas)
        public Guid? TareaPadreId { get; set; }
        public Tarea? TareaPadre { get; set; }
        public List<Tarea> Subtareas { get; set; } = new List<Tarea>();
    }
}