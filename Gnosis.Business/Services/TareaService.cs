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
    // 1. Obtener tareas principales (raíz) del usuario autenticado, con sus subtareas anidadas
    public async Task<IEnumerable<TareaModel>> ObtenerTareasPrincipalesAsync(Guid usuarioId)
    {
        var todas = (await tareaRepository.GetAllAsync())
            .Where(t => t.UsuarioId == usuarioId)
            .ToList();

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
            FechaEntrega = t.FechaEntrega,
            EtiquetaManual = t.EtiquetaManual,
            TareaPadreId = t.TareaPadreId,
            Subtareas = new List<TareaModel>()
        };

        if (subtareasPorPadre != null && subtareasPorPadre.TryGetValue(t.Id, out var hijos))
        {
            modelo.Subtareas = hijos.Select(h => MapearATareaModel(h)).ToList();
        }

        return modelo;
    }

    public async Task<TareaModel> CrearTareaRaizAsync(Guid usuarioId, Guid? id, string titulo, string? descripcion, Guid? tareaPadreId = null, DateTime? fechaEntrega = null)
    {
        var nuevaTarea = new Tarea
        {
            // Respeta el Id generado por el cliente (usado para la actualización optimista de la UI)
            // en vez de generar uno nuevo, para que ambos lados queden sincronizados.
            Id = id ?? Guid.NewGuid(),
            UsuarioId = usuarioId,
            Titulo = titulo,
            Descripcion = descripcion,
            // Si viene un padre, se persiste como subtarea; antes se ignoraba este dato y toda
            // subtarea quedaba guardada como tarea raíz (se "perdía" su padre al recargar la página).
            TareaPadreId = tareaPadreId,
            // Opcional: si viene (manual o propuesta por la IA), habilita el cálculo automático
            // de la etiqueta de urgencia en el cliente.
            FechaEntrega = fechaEntrega
        };

        await tareaRepository.AgregarAsync(nuevaTarea);

        return MapearATareaModel(nuevaTarea);
    }

    public async Task<bool> ActualizarFechaEntregaAsync(Guid usuarioId, Guid id, DateTime? fechaEntrega)
    {
        var tarea = await tareaRepository.GetByIdAsync(id);
        if (tarea == null || tarea.UsuarioId != usuarioId) return false;

        tarea.FechaEntrega = fechaEntrega;
        await tareaRepository.ActualizarAsync(tarea);

        return true;
    }

    public async Task<bool> ActualizarEtiquetaAsync(Guid usuarioId, Guid id, string? etiquetaManual)
    {
        var tarea = await tareaRepository.GetByIdAsync(id);
        if (tarea == null || tarea.UsuarioId != usuarioId) return false;

        // Cadena vacía se trata igual que null: "quitar el override manual, volver al cálculo automático".
        tarea.EtiquetaManual = string.IsNullOrWhiteSpace(etiquetaManual) ? null : etiquetaManual;
        await tareaRepository.ActualizarAsync(tarea);

        return true;
    }

    public async Task<TareaModel> DesglosarTareaAsync(Guid usuarioId, Guid tareaPadreId, string tituloSubtarea)
    {
        var subtarea = new Tarea
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Titulo = tituloSubtarea,
            // Antes no se asignaba: la subtarea se guardaba como tarea raíz, sin relación con su padre.
            TareaPadreId = tareaPadreId
        };

        await tareaRepository.AgregarAsync(subtarea);

        return new TareaModel
        {
            Id = subtarea.Id,
            Titulo = subtarea.Titulo,
            TareaPadreId = subtarea.TareaPadreId
        };
    }

    public async Task<bool> ActualizarEstadoTareaAsync(Guid usuarioId, Guid id, bool isCompletada)
    {
        var tarea = await tareaRepository.GetByIdAsync(id);
        // No solo "existe": tiene que ser del usuario que hizo la petición, si no cualquiera
        // podría cambiar el estado de tareas ajenas adivinando su Id.
        if (tarea == null || tarea.UsuarioId != usuarioId) return false;

        tarea.IsCompletada = isCompletada;
        // Registra cuándo se completó (para el dashboard); si se destilda, se limpia.
        tarea.FechaCompletada = isCompletada ? DateTime.UtcNow : null;

        await tareaRepository.ActualizarAsync(tarea);

        return true;
    }

    public async Task<bool> EliminarTareaAsync(Guid usuarioId, Guid id)
    {
        var tarea = await tareaRepository.GetByIdAsync(id);
        if (tarea == null || tarea.UsuarioId != usuarioId) return false;

        await tareaRepository.EliminarAsync(id);
        return true;
    }
}
