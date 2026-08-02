using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gnosis.Business.Models;
using Gnosis.Domain.Entities;

namespace Gnosis.Business.Services
{
    public interface ITareaService
    {
        Task<IEnumerable<TareaModel>> ObtenerTareasPrincipalesAsync(Guid usuarioId);
        Task<TareaModel> CrearTareaRaizAsync(Guid usuarioId, Guid? id, string titulo, string? descripcion, Guid? tareaPadreId = null);
        Task<TareaModel> DesglosarTareaAsync(Guid usuarioId, Guid tareaPadreId, string tituloSubtarea);
        Task<bool> ActualizarEstadoTareaAsync(Guid usuarioId, Guid id, bool isCompletada);
        Task<bool> EliminarTareaAsync(Guid usuarioId, Guid id);
    }
}