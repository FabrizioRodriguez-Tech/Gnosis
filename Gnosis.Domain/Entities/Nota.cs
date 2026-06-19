using System;

namespace Gnosis.Domain.Entities
{
    public class Nota
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}