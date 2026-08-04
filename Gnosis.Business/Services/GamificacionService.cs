using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gnosis.Business.Models;
using Gnosis.Domain.Entities;
using Gnosis.Domain.Interfaces;

namespace Gnosis.Business.Services;

// XP/Nivel y racha se calculan al vuelo a partir de datos que ya existen (tareas completadas,
// sesiones de enfoque) — no hay nada nuevo que persistir para eso. Lo único que sí necesita una
// tabla propia es el "Jardín de Enfoque": un registro por intento de siembra ya resuelto
// (creció / se marchitó), ver Gnosis.Domain.Entities.SiembraEnfoque.
internal class GamificacionService(
    IRepository<Tarea> tareaRepository,
    IRepository<SesionEnfoque> sesionRepository,
    IRepository<SiembraEnfoque> siembraRepository) : IGamificacionService
{
    // +10 XP por tarea completada, +25 XP por ciclo de Pomodoro (sesión de Enfoque) terminado.
    private const int XpPorTarea = 10;
    private const int XpPorPomodoro = 25;

    // Cuánto XP hace falta para pasar de un nivel al siguiente. Simple y predecible: cada
    // nivel cuesta lo mismo, para no tener que ajustar una curva a mano.
    private const int XpPorNivel = 100;

    public async Task<GamificacionModel> ObtenerAsync(Guid usuarioId)
    {
        var todasTareas = (await tareaRepository.GetAllAsync()).Where(t => t.UsuarioId == usuarioId).ToList();
        var todasSesiones = (await sesionRepository.GetAllAsync()).Where(s => s.UsuarioId == usuarioId).ToList();
        var todasSiembras = (await siembraRepository.GetAllAsync()).Where(s => s.UsuarioId == usuarioId).ToList();

        var tareasCompletadas = todasTareas.Where(t => t.FechaCompletada.HasValue).ToList();
        var sesionesEnfoque = todasSesiones.Where(s => s.TipoSesion == "Enfoque" && s.FechaFin.HasValue).ToList();

        var xp = tareasCompletadas.Count * XpPorTarea + sesionesEnfoque.Count * XpPorPomodoro;
        var nivel = (xp / XpPorNivel) + 1;
        var xpEnNivelActual = xp % XpPorNivel;

        var hoy = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var siembrasDelMes = todasSiembras
            .Where(s => s.Fecha.Date >= inicioMes.Date)
            .OrderBy(s => s.Fecha)
            .Select(s => new SiembraModel { Id = s.Id, Fecha = s.Fecha, Crecio = s.Crecio })
            .ToList();

        return new GamificacionModel
        {
            Xp = xp,
            Nivel = nivel,
            XpEnNivelActual = xpEnNivelActual,
            XpParaSiguienteNivel = XpPorNivel,
            RachaDiasActivos = CalcularRacha(sesionesEnfoque, tareasCompletadas),
            SiembrasDelMes = siembrasDelMes
        };
    }

    public async Task<SiembraModel> RegistrarSiembraAsync(Guid usuarioId, bool crecio)
    {
        var siembra = new SiembraEnfoque
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Fecha = DateTime.UtcNow,
            Crecio = crecio
        };

        await siembraRepository.AgregarAsync(siembra);

        return new SiembraModel { Id = siembra.Id, Fecha = siembra.Fecha, Crecio = siembra.Crecio };
    }

    // Misma lógica que EstadisticasService.CalcularRacha (días consecutivos hasta hoy con al
    // menos una sesión de enfoque o una tarea completada). Duplicada deliberadamente: es privada
    // allá y son solo unas líneas, no vale la pena crear una dependencia cruzada entre servicios
    // por esto.
    private static int CalcularRacha(List<SesionEnfoque> sesiones, List<Tarea> tareasCompletadas)
    {
        var diasActivos = new HashSet<DateTime>();

        foreach (var s in sesiones)
            diasActivos.Add(s.FechaFin!.Value.Date);

        foreach (var t in tareasCompletadas)
            diasActivos.Add(t.FechaCompletada!.Value.Date);

        var racha = 0;
        var dia = DateTime.UtcNow.Date;

        while (diasActivos.Contains(dia))
        {
            racha++;
            dia = dia.AddDays(-1);
        }

        return racha;
    }
}
