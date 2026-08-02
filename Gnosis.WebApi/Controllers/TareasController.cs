using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gnosis.Business.Services;
using Gnosis.Business.Models;

namespace Gnosis.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TareasController : ControllerBase
{
    private readonly ITareaService _tareaService;

    public TareasController(ITareaService tareaService)
    {
        _tareaService = tareaService;
    }

    /// <summary>
    /// Obtiene las tareas principales u objetivos raíz del usuario autenticado.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodas()
    {
        try
        {
            var tareas = await _tareaService.ObtenerTareasPrincipalesAsync(User.ObtenerUsuarioId());
            return Ok(tareas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno en el servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea un nuevo objetivo de aprendizaje raíz.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearTareaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
            return BadRequest("El título es obligatorio.");

        try
        {
            var nuevaTarea = await _tareaService.CrearTareaRaizAsync(User.ObtenerUsuarioId(), request.Id, request.Titulo, request.Descripcion, request.TareaPadreId);
            return CreatedAtAction(nameof(ObtenerTodas), new { id = nuevaTarea.Id }, nuevaTarea);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno en el servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza el estado de completado de una tarea o subtarea específica.
    /// </summary>
    [HttpPut("{id:guid}/estado")]
    public async Task<IActionResult> ActualizarEstado(Guid id, [FromBody] ActualizarEstadoRequest request)
    {
        try
        {
            var exito = await _tareaService.ActualizarEstadoTareaAsync(User.ObtenerUsuarioId(), id, request.IsCompletada);

            if (!exito)
                return NotFound("La tarea o subtarea especificada no existe.");

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno en el servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Elimina una tarea o subtarea. Si es una tarea raíz con subtareas, estas se eliminan en cascada.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        try
        {
            var exito = await _tareaService.EliminarTareaAsync(User.ObtenerUsuarioId(), id);

            if (!exito)
                return NotFound("La tarea o subtarea especificada no existe.");

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno en el servidor: {ex.Message}");
        }
    }
}

// DTO intermedio para recibir los datos de creación de forma limpia
public class CrearTareaRequest
{
    // Id opcional generado por el cliente (Blazor) para su actualización optimista de UI;
    // si viene, el servidor lo respeta en vez de generar uno propio.
    public Guid? Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    // Antes no existía: el cliente ya enviaba este dato (TareaModel.TareaPadreId) pero el DTO
    // lo descartaba silenciosamente, así que ninguna subtarea quedaba ligada a su padre en la BD.
    public Guid? TareaPadreId { get; set; }
}

// DTO intermedio para recibir la actualización de estado de forma limpia
public class ActualizarEstadoRequest
{
    public bool IsCompletada { get; set; }
}
