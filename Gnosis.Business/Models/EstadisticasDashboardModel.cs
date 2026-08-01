using System;
using System.Collections.Generic;

namespace Gnosis.Business.Models
{
    public class EstadisticaDiaModel
    {
        public DateTime Fecha { get; set; }
        public int SesionesEnfoque { get; set; }
        public int MinutosEnfoque { get; set; }
        public int TareasCompletadas { get; set; }
    }

    public class EstadisticasDashboardModel
    {
        public List<EstadisticaDiaModel> Dias { get; set; } = new();
        public int RachaDiasActivos { get; set; }
        public int TotalSesionesSemana { get; set; }
        public int TotalMinutosEnfoqueSemana { get; set; }
        public int TotalTareasCompletadasSemana { get; set; }
    }
}
