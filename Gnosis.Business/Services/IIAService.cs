using System.Threading.Tasks;
using Gnosis.Business.Models;

namespace Gnosis.Business.Services
{
    public interface IIAService
    {
        Task<IAResponse> ConsultarAsync(IARequest request);

        // Task Breaker: genera 4-5 subtareas concretas para una tarea existente.
        Task<DesglosarTareaResponse> DesglosarTareaAsync(DesglosarTareaRequest request);

        // Daily Retrospective: resumen ejecutivo del día a partir de las tareas ya completadas.
        Task<ResumenDiaResponse> GenerarResumenDiaAsync(ResumenDiaRequest request);

        // Estimador de Pomodoros: cuántos ciclos probablemente requiera una tarea.
        Task<EstimarPomodorosResponse> EstimarPomodorosAsync(EstimarPomodorosRequest request);
    }
}
