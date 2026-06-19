using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gnosis.Domain.Entities;
using Gnosis.Domain.Interfaces;
using Gnosis.Business.Models;

namespace Gnosis.Business.Services
{
    // Implementación interna blindada
    internal class TareaService : ITareaService
    {
        private readonly ITareaRepository _tareaRepository;

 
        public TareaService(ITareaRepository tareaRepository)
        {
            _tareaRepository = tareaRepository;
        }

        public async Task<IEnumerable<TareaModel>> ObtenerTareasPrincipalesAsync()
        {
            var tareas = await _tareaRepository.GetTareasPrincipalesAsync();
            return tareas.Select(MapearAModel).ToList();
        }

        public async Task<TareaModel> CrearTareaRaizAsync(string titulo, string? descripcion)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título de la tarea no puede estar vacío.");

            var nuevaTarea = new Tarea { Titulo = titulo, Descripcion = descripcion };
            await _tareaRepository.AgregarAsync(nuevaTarea);
            return MapearAModel(nuevaTarea);
        }

        public async Task<TareaModel> DesglosarTareaAsync(Guid tareaPadreId, string tituloSubtarea)
        {
            if (string.IsNullOrWhiteSpace(tituloSubtarea))
                throw new ArgumentException("El desglose requiere un título válido.");

            var padre = await _tareaRepository.GetByIdAsync(tareaPadreId);
            if (padre == null) throw new KeyNotFoundException("La tarea padre no existe.");

            var subtarea = new Tarea { Titulo = tituloSubtarea, TareaPadreId = tareaPadreId };
            padre.Subtareas.Add(subtarea);

            await _tareaRepository.ActualizarAsync(padre);
            return MapearAModel(subtarea);
        }

        public async Task CambiarEstadoCompletadoAsync(Guid tareaId, bool completada)
        {
            var tarea = await _tareaRepository.GetByIdAsync(tareaId);
            if (tarea == null) throw new KeyNotFoundException("Tarea no encontrada.");

            tarea.IsCompletada = completada;
            await _tareaRepository.ActualizarAsync(tarea);
        }
        private TareaModel MapearAModel(Tarea tarea) => new TareaModel
        {
            Id = tarea.Id,
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            IsCompletada = tarea.IsCompletada,
            FechaCreacion = tarea.FechaCreacion,
            TareaPadreId = tarea.TareaPadreId,
            Subtareas = tarea.Subtareas?.Select(MapearAModel).ToList() ?? new List<TareaModel>()
        };
    }
}