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

        // Fecha de entrega opcional (manual o puesta por la IA); a partir de esta fecha se
        // calcula EtiquetaEfectiva más abajo.
        public DateTime? FechaEntrega { get; set; }

        // Override manual de la etiqueta; si está puesto, gana sobre el cálculo automático.
        public string? EtiquetaManual { get; set; }

        public Guid? TareaPadreId { get; set; }
        public List<TareaModel> Subtareas { get; set; } = new List<TareaModel>();

        // Etiqueta que se muestra en la UI: manual si el usuario puso una, si no se calcula sola
        // a partir de FechaEntrega y la fecha de hoy — por eso "avanza sola" con el paso de los
        // días sin que nadie tenga que tocar nada. Una tarea completada nunca muestra etiqueta.
        public string? EtiquetaEfectiva
        {
            get
            {
                if (IsCompletada) return null;
                if (!string.IsNullOrWhiteSpace(EtiquetaManual)) return EtiquetaManual;
                if (FechaEntrega == null) return null;

                var diasRestantes = (FechaEntrega.Value.Date - DateTime.Today).Days;
                if (diasRestantes < 0) return "Vencida";
                if (diasRestantes <= 1) return "Urgente";
                if (diasRestantes <= 3) return "Próxima";
                return "A tiempo";
            }
        }

        // Rango para ordenar la lista (menor = aparece primero): vencidas y urgentes arriba,
        // completadas siempre al final sin importar su etiqueta.
        public int RangoUrgencia
        {
            get
            {
                if (IsCompletada) return 100;
                return EtiquetaEfectiva switch
                {
                    "Vencida" => 0,
                    "Urgente" => 1,
                    "Próxima" => 2,
                    "A tiempo" => 3,
                    _ => 4
                };
            }
        }
    }
}