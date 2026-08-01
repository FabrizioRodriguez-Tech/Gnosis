using System;
using System.Threading.Tasks;
using Gnosis.Business.Models;

namespace Gnosis.Business.Services
{
    public interface IEstadisticasService
    {
        Task<EstadisticasDashboardModel> ObtenerEstadisticasSemanaAsync(DateTime desde, DateTime hasta);
    }
}
