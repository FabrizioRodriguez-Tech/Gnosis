using System;
using System.Threading.Tasks;
using Gnosis.Business.Models;

namespace Gnosis.Business.Services
{
    public interface IEstadisticasService
    {
        Task<EstadisticasDashboardModel> ObtenerEstadisticasSemanaAsync(Guid usuarioId, DateTime desde, DateTime hasta);
    }
}
