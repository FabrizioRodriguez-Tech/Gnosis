using System;
using System.Threading.Tasks;
using Gnosis.Business.Models;

namespace Gnosis.Business.Services
{
    public interface IGamificacionService
    {
        // XP, nivel, racha y jardín del mes en curso, todo calculado a partir de datos existentes
        // (tareas completadas, sesiones de enfoque) más el historial de siembras.
        Task<GamificacionModel> ObtenerAsync(Guid usuarioId);

        // Registra el resultado de un intento de siembra (sesión de Pomodoro en modo Enfoque):
        // crecio = true si se completó el ciclo, false si se canceló antes de tiempo.
        Task<SiembraModel> RegistrarSiembraAsync(Guid usuarioId, bool crecio);
    }
}
