using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gnosis.Business.Services;
using Gnosis.Business.Models;

namespace Gnosis.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BloquesTiempoController : ControllerBase
{
    private readonly IBloqueTiempoService _bloqueTiempoService;

    public BloquesTiempoController(IBloqueTiempoService bloqueTiempoService)
    {
        _bloqueTiempoService = bloqueTiempoService;
    }

    /// <summary>
    /// Obtiene los bloques de tiempo que caen dentro de un rango de fechas (ej. una semana).
    /// Si no se especifican fechas, devuelve la semana actual.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerPorRango([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        try
        {
            var inicio = desde ?? DateTime.UtcNow.Date.AddDays(-7);
            var fin = hasta ?? DateTime.UtcNow.Date.AddDays(7);

            var bloques = await _bloqueTiempoService.ObtenerPorRangoAsync(User.ObtenerUsuarioId(), inicio, fin);
            return Ok(bloques);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno en el servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea un nuevo bloque de tiempo en la agenda.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] BloqueTiempoModel nuevoBloque)
    {
        if (string.IsNullOrWhiteSpace(nuevoBloque.Titulo))
            return BadRequest("El título es obligatorio.");

        try
        {
            var creado = await _bloqueTiempoService.CrearAsync(User.ObtenerUsuarioId(), nuevoBloque);
            return CreatedAtAction(nameof(ObtenerPorRango), new { id = creado.Id }, creado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno en el servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza un bloque de tiempo existente (mover/redimensionar/reasignar tarea).
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] BloqueTiempoModel bloqueActualizado)
    {
        if (id != bloqueActualizado.Id)
            return BadRequest("El id de la ruta no coincide con el del cuerpo de la petición.");

        try
        {
            var exito = await _bloqueTiempoService.ActualizarAsync(User.ObtenerUsuarioId(), bloqueActualizado);
            if (!exito)
                return NotFound("El bloque de tiempo especificado no existe.");

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno en el servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Elimina un bloque de tiempo de la agenda.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        try
        {
            var exito = await _bloqueTiempoService.EliminarAsync(User.ObtenerUsuarioId(), id);
            if (!exito)
                return NotFound("El bloque de tiempo especificado no existe.");

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno en el servidor: {ex.Message}");
        }
    }
}
