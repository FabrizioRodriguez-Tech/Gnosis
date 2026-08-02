using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gnosis.Business.Models;
using Gnosis.Domain.Entities;
using Gnosis.Domain.Interfaces;

namespace Gnosis.Business.Services;

// Usamos el constructor principal de C# para simplificar el código e inyectar los repositorios directamente
internal class BloqueTiempoService(
    IRepository<BloqueTiempo> bloqueRepository,
    IRepository<Tarea> tareaRepository) : IBloqueTiempoService
{
    public async Task<IEnumerable<BloqueTiempoModel>> ObtenerPorRangoAsync(Guid usuarioId, DateTime desde, DateTime hasta)
    {
        var bloques = (await bloqueRepository.GetAllAsync()).Where(b => b.UsuarioId == usuarioId);
        var tareas = (await tareaRepository.GetAllAsync()).Where(t => t.UsuarioId == usuarioId);
        var tareasPorId = tareas.ToDictionary(t => t.Id, t => t.Titulo);

        return bloques
            .Where(b => b.FechaInicio < hasta && b.FechaFin > desde)
            .OrderBy(b => b.FechaInicio)
            .Select(b => MapearAModelo(b, tareasPorId));
    }

    public async Task<BloqueTiempoModel> CrearAsync(Guid usuarioId, BloqueTiempoModel nuevo)
    {
        if (nuevo.FechaFin <= nuevo.FechaInicio)
            throw new ArgumentException("La fecha de fin debe ser posterior a la fecha de inicio.");

        // La tarea vinculada, si viene, tiene que existir Y ser del mismo usuario (si no,
        // se podría enlazar un bloque a una tarea ajena, o a un Id que nunca se persistió).
        if (nuevo.TareaId.HasValue)
        {
            var tareaVinculada = await tareaRepository.GetByIdAsync(nuevo.TareaId.Value);
            if (tareaVinculada == null || tareaVinculada.UsuarioId != usuarioId)
                throw new ArgumentException("La tarea vinculada no existe.");
        }

        var entidad = new BloqueTiempo
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Titulo = nuevo.Titulo,
            Descripcion = nuevo.Descripcion,
            FechaInicio = ComoUtc(nuevo.FechaInicio),
            FechaFin = ComoUtc(nuevo.FechaFin),
            Color = nuevo.Color,
            TareaId = nuevo.TareaId
        };

        await bloqueRepository.AgregarAsync(entidad);

        var tareaTitulo = entidad.TareaId.HasValue
            ? (await tareaRepository.GetByIdAsync(entidad.TareaId.Value))?.Titulo
            : null;

        return MapearAModelo(entidad, tareaTitulo);
    }

    public async Task<bool> ActualizarAsync(Guid usuarioId, BloqueTiempoModel actualizado)
    {
        if (actualizado.FechaFin <= actualizado.FechaInicio)
            throw new ArgumentException("La fecha de fin debe ser posterior a la fecha de inicio.");

        var existente = await bloqueRepository.GetByIdAsync(actualizado.Id);
        if (existente == null || existente.UsuarioId != usuarioId) return false;

        if (actualizado.TareaId.HasValue)
        {
            var tareaVinculada = await tareaRepository.GetByIdAsync(actualizado.TareaId.Value);
            if (tareaVinculada == null || tareaVinculada.UsuarioId != usuarioId)
                throw new ArgumentException("La tarea vinculada no existe.");
        }

        existente.Titulo = actualizado.Titulo;
        existente.Descripcion = actualizado.Descripcion;
        existente.FechaInicio = ComoUtc(actualizado.FechaInicio);
        existente.FechaFin = ComoUtc(actualizado.FechaFin);
        existente.Color = actualizado.Color;
        existente.TareaId = actualizado.TareaId;

        await bloqueRepository.ActualizarAsync(existente);
        return true;
    }

    public async Task<bool> EliminarAsync(Guid usuarioId, Guid id)
    {
        var existente = await bloqueRepository.GetByIdAsync(id);
        if (existente == null || existente.UsuarioId != usuarioId) return false;

        await bloqueRepository.EliminarAsync(id);
        return true;
    }

    // Postgres guarda FechaInicio/FechaFin como "timestamp with time zone" y Npgsql exige que el
    // DateTime venga marcado como Utc (los inputs datetime-local del formulario llegan con Kind=Unspecified).
    // No convertimos el valor, solo lo etiquetamos: en esta app la hora se trata como hora local del usuario.
    private static DateTime ComoUtc(DateTime fecha) =>
        fecha.Kind == DateTimeKind.Utc ? fecha : DateTime.SpecifyKind(fecha, DateTimeKind.Utc);

    private static BloqueTiempoModel MapearAModelo(BloqueTiempo b, IReadOnlyDictionary<Guid, string> tareasPorId)
    {
        string? tareaTitulo = null;
        if (b.TareaId.HasValue)
            tareasPorId.TryGetValue(b.TareaId.Value, out tareaTitulo);

        return MapearAModelo(b, tareaTitulo);
    }

    private static BloqueTiempoModel MapearAModelo(BloqueTiempo b, string? tareaTitulo) => new()
    {
        Id = b.Id,
        Titulo = b.Titulo,
        Descripcion = b.Descripcion,
        FechaInicio = b.FechaInicio,
        FechaFin = b.FechaFin,
        Color = b.Color,
        TareaId = b.TareaId,
        TareaTitulo = tareaTitulo
    };
}
