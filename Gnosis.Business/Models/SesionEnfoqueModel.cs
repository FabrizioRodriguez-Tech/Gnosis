using System;

namespace Gnosis.Business.Models
{
    public class SesionEnfoqueModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int DuracionMinutos { get; set; }
        public string TipoSesion { get; set; } = string.Empty;
    }
}
