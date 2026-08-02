using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gnosis.Business.Models;

namespace Gnosis.Business.Services
{
    public interface IBloqueTiempoService
    {
        Task<IEnumerable<BloqueTiempoModel>> ObtenerPorRangoAsync(Guid usuarioId, DateTime desde, DateTime hasta);
        Task<BloqueTiempoModel> CrearAsync(Guid usuarioId, BloqueTiempoModel nuevo);
        Task<bool> ActualizarAsync(Guid usuarioId, BloqueTiempoModel actualizado);
        Task<bool> EliminarAsync(Guid usuarioId, Guid id);
    }
}
