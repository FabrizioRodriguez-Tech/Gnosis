using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gnosis.Business.Models;
using Gnosis.Domain.Entities;

namespace Gnosis.Business.Services
{
    public interface ITareaService
    {
        Task<IEnumerable<TareaModel>> ObtenerTareasPrincipalesAsync();
        Task<TareaModel> CrearTareaRaizAsync(string titulo, string? descripcion);
        Task<TareaModel> DesglosarTareaAsync(Guid tareaPadreId, string tituloSubtarea);
        Task CambiarEstadoCompletadoAsync(Guid tareaId, bool completada);
    }
}