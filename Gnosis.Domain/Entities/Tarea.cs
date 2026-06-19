using System;
using System.Collections.Generic;

namespace Gnosis.Domain.Entities
{
    public class Tarea
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool IsCompletada { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Relación reflexiva para desglose jerárquico (Árbol de subtareas)
        public Guid? TareaPadreId { get; set; }
        public Tarea? TareaPadre { get; set; }
        public List<Tarea> Subtareas { get; set; } = new List<Tarea>();
    }
}