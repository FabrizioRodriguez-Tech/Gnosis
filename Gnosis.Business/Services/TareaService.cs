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
    // 1. Obtener tareas principales
    public async Task<IEnumerable<TareaModel>> ObtenerTareasPrincipalesAsync()
    {
        var todas = await tareaRepository.GetAllAsync();

        return todas.Select(t => new TareaModel
        {
            Id = t.Id,
            Titulo = t.Titulo,
            Descripcion = t.Descripcion
        });
    }

    public async Task<TareaModel> CrearTareaRaizAsync(string titulo, string? descripcion)
    {
        var nuevaTarea = new Tarea
        {
            Id = Guid.NewGuid(),
            Titulo = titulo,
            Descripcion = descripcion
        };

        await tareaRepository.AgregarAsync(nuevaTarea);

        return new TareaModel
        {
            Id = nuevaTarea.Id,
            Titulo = nuevaTarea.Titulo,
            Descripcion = nuevaTarea.Descripcion
        };
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

        // Corregido: usamos ActualizarAsync como está definido en tu IRepository
        await tareaRepository.ActualizarAsync(tarea);

        return true;
    }
}