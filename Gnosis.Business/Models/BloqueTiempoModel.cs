using System;

namespace Gnosis.Business.Models
{
    public class BloqueTiempoModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string? Color { get; set; }
        public Guid? TareaId { get; set; }

        // Solo lectura: título de la tarea asignada, para mostrar en la agenda sin otra llamada
        public string? TareaTitulo { get; set; }
    }
}
