using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Gnosis.Business.Models;

namespace Gnosis.WebUI
{
    public interface IEstadisticasHttpProxy
    {
        Task<EstadisticasDashboardModel> ObtenerEstadisticasSemanaAsync(DateTime desde, DateTime hasta);
    }

    public class EstadisticasHttpProxy : IEstadisticasHttpProxy
    {
        private readonly HttpClient _httpClient;

        public EstadisticasHttpProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<EstadisticasDashboardModel> ObtenerEstadisticasSemanaAsync(DateTime desde, DateTime hasta)
        {
            var query = $"api/Estadisticas?desde={Uri.EscapeDataString(desde.ToString("o"))}&hasta={Uri.EscapeDataString(hasta.ToString("o"))}";
            return await _httpClient.GetFromJsonAsync<EstadisticasDashboardModel>(query)
                   ?? new EstadisticasDashboardModel();
        }
    }
}
