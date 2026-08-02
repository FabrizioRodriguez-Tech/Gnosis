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

        // Relación reflexiva para desglose jerárquico (Árbol de subtareas)
        public Guid? TareaPadreId { get; set; }
        public Tarea? TareaPadre { get; set; }
        public List<Tarea> Subtareas { get; set; } = new List<Tarea>();
    }
}