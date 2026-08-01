using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gnosis.Domain.Interfaces;
using Gnosis.Domain.Entities;
using Gnosis.Business.Models;

namespace Gnosis.Business.Services;

// Usamos el constructor principal de C# para simplificar el código e inyectar el repositorio directamente
internal class TareaService(IRepository<Tarea> tareaRepository) : ITareaService
{
    // 1. Obtener tareas principales (raíz), con sus subtareas anidadas
    public async Task<IEnumerable<TareaModel>> ObtenerTareasPrincipalesAsync()
    {
        var todas = (await tareaRepository.GetAllAsync()).ToList();

        var subtareasPorPadre = todas
            .Where(t => t.TareaPadreId.HasValue)
            .GroupBy(t => t.TareaPadreId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        return todas
            .Where(t => t.TareaPadreId == null)
            .Select(t => MapearATareaModel(t, subtareasPorPadre));
    }

    private static TareaModel MapearATareaModel(Tarea t, Dictionary<Guid, List<Tarea>>? subtareasPorPadre = null)
    {
        var modelo = new TareaModel
        {
            Id = t.Id,
            Titulo = t.Titulo,
            Descripcion = t.Descripcion,
            IsCompletada = t.IsCompletada,
            FechaCreacion = t.FechaCreacion,
            FechaCompletada = t.FechaCompletada,
            TareaPadreId = t.TareaPadreId,
            Subtareas = new List<TareaModel>()
        };

        if (subtareasPorPadre != null && subtareasPorPadre.TryGetValue(t.Id, out var hijos))
        {
            modelo.Subtareas = hijos.Select(h => MapearATareaModel(h)).ToList();
        }

        return modelo;
    }

    public async Task<TareaModel> CrearTareaRaizAsync(Guid? id, string titulo, string? descripcion)
    {
        var nuevaTarea = new Tarea
        {
            // Respeta el Id generado por el cliente (usado para la actualización optimista de la UI)
            // en vez de generar uno nuevo, para que ambos lados queden sincronizados.
            Id = id ?? Guid.NewGuid(),
            Titulo = titulo,
            Descripcion = descripcion
        };

        await tareaRepository.AgregarAsync(nuevaTarea);

        return MapearATareaModel(nuevaTarea);
    }

    public async Task<TareaModel> DesglosarTareaAsync(Guid tareaPadreId, string tituloSubtarea)
    {
        var subtarea = new Tarea
        {
            Id = Guid.NewGuid(),
            Titulo = tituloSubtarea
        };

        await tareaRepository.AgregarAsync(subtarea);

        return new TareaModel
        {
            Id = subtarea.Id,
            Titulo = subtarea.Titulo
        };
    }

    public async Task CambiarEstadoCompletadoAsync(Guid tareaId, bool completada)
    {
        await Task.CompletedTask;
    }

    public async Task<bool> ActualizarEstadoTareaAsync(Guid id, bool isCompletada)
    {
        // Buscamos la tarea usando el método que sí existe en tu interfaz
        var tarea = await tareaRepository.GetByIdAsync(id);
        if (tarea == null) return false;

        tarea.IsCompletada = isCompletada;
        // Registra cuándo se completó (para el dashboard); si se destilda, se limpia.
        tarea.FechaCompletada = isCompletada ? DateTime.UtcNow : null;

        // Corregido: usamos ActualizarAsync como está definido en tu IRepository
        await tareaRepository.ActualizarAsync(tarea);

        return true;
    }
}