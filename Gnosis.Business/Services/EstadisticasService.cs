using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gnosis.Business.Models;
using Gnosis.Domain.Entities;
using Gnosis.Domain.Interfaces;

namespace Gnosis.Business.Services;

// Agrega datos de SesionEnfoque y Tarea para el dashboard de productividad.
// Usamos el constructor principal de C# para simplificar el código e inyectar los repositorios directamente.
internal class EstadisticasService(
    IRepository<SesionEnfoque> sesionRepository,
    IRepository<Tarea> tareaRepository) : IEstadisticasService
{
    public async Task<EstadisticasDashboardModel> ObtenerEstadisticasSemanaAsync(DateTime desde, DateTime hasta)
    {
        var todasSesiones = (await sesionRepository.GetAllAsync()).ToList();
        var todasTareas = (await tareaRepository.GetAllAsync()).ToList();

        // Solo las sesiones de tipo "Enfoque" cuentan como tiempo de trabajo real (no los descansos)
        var sesionesEnfoque = todasSesiones.Where(s => s.TipoSesion == "Enfoque" && s.FechaFin.HasValue).ToList();
        var tareasCompletadas = todasTareas.Where(t => t.FechaCompletada.HasValue).ToList();

        var dias = new List<EstadisticaDiaModel>();
        for (var fecha = desde.Date; fecha < hasta.Date; fecha = fecha.AddDays(1))
        {
            var sesionesDelDia = sesionesEnfoque.Where(s => s.FechaFin!.Value.Date == fecha).ToList();
            var tareasDelDia = tareasCompletadas.Count(t => t.FechaCompletada!.Value.Date == fecha);

            dias.Add(new EstadisticaDiaModel
            {
                Fecha = fecha,
                SesionesEnfoque = sesionesDelDia.Count,
                MinutosEnfoque = sesionesDelDia.Sum(s => s.DuracionMinutos),
                TareasCompletadas = tareasDelDia
            });
        }

        return new EstadisticasDashboardModel
        {
            Dias = dias,
            RachaDiasActivos = CalcularRacha(sesionesEnfoque, tareasCompletadas),
            TotalSesionesSemana = dias.Sum(d => d.SesionesEnfoque),
            TotalMinutosEnfoqueSemana = dias.Sum(d => d.MinutosEnfoque),
            TotalTareasCompletadasSemana = dias.Sum(d => d.TareasCompletadas)
        };
    }

    // Cuenta días consecutivos hasta hoy con al menos una sesión de enfoque o una tarea completada.
    // Usa TODO el historial (no solo la semana visible) para que la racha sea correcta.
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
