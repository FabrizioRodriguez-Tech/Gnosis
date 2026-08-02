using System;

namespace Gnosis.Domain.Entities
{
    public class SesionEnfoque
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UsuarioId { get; set; }
        public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
        public DateTime? FechaFin { get; set; }
        public int DuracionMinutos { get; set; }
        public string TipoSesion { get; set; } = "Trabajo"; // Trabajo, DescansoCorto, DescansoLargo
    }
}