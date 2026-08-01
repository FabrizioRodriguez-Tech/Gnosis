using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gnosis.Business.Models;

namespace Gnosis.Business.Services
{
    public interface IBloqueTiempoService
    {
        Task<IEnumerable<BloqueTiempoModel>> ObtenerPorRangoAsync(DateTime desde, DateTime hasta);
        Task<BloqueTiempoModel> CrearAsync(BloqueTiempoModel nuevo);
        Task<bool> ActualizarAsync(BloqueTiempoModel actualizado);
        Task<bool> EliminarAsync(Guid id);
    }
}
