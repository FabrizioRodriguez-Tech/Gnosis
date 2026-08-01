using System;
using System.Collections.Generic;

namespace Gnosis.Business.Models
{
    public class TareaModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool IsCompletada { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaCompletada { get; set; }
        public Guid? TareaPadreId { get; set; }
        public List<TareaModel> Subtareas { get; set; } = new List<TareaModel>();
    }
}