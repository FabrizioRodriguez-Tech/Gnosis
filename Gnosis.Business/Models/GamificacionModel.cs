using System;
using System.Collections.Generic;

namespace Gnosis.Business.Models
{
    // Un intento de siembra ya resuelto: "creció" (sesión de Pomodoro completada) o
    // "se marchitó" (sesión cancelada antes de tiempo). Se usa para pintar el jardín del mes.
    public class SiembraModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Fecha { get; set; }
        public bool Crecio { get; set; }
    }

    public class GamificacionModel
    {
        // XP total acumulado (histórico completo, no solo el mes actual).
        public int Xp { get; set; }

        // Nivel actual, derivado de Xp. Empieza en 1.
        public int Nivel { get; set; }

        // Progreso dentro del nivel actual (0 a XpParaSiguienteNivel).
        public int XpEnNivelActual { get; set; }

        // Cuánto XP hace falta para subir de nivel desde el punto de partida del nivel actual.
        public int XpParaSiguienteNivel { get; set; }

        // Racha de días consecutivos con actividad (reutiliza la misma lógica que el Dashboard).
        public int RachaDiasActivos { get; set; }

        // Siembras del mes en curso, para pintar el "jardín" en el Dashboard.
        public List<SiembraModel> SiembrasDelMes { get; set; } = new();
    }
}
