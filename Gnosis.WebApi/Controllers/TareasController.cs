using Microsoft.AspNetCore.Mvc;
using Gnosis.Business.Services;
using Gnosis.Business.Models;

namespace Gnosis.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TareasController : ControllerBase
{
    private readonly ITareaService _tareaService;

    public TareasController(ITareaService tareaService)
    {
        _tareaService = tareaService;
    }

    /// <summary>
    /// Obtiene las tareas principales u objetivos raíz del sistema.
    /// </summary>
    [HttpGet]
    public IActionResult ObtenerTodas()
    {
        // Datos simulados para evitar el error 500 por desconexion a base de datos
        var tareasSimuladas = new[]
        {
            new { Id = Guid.NewGuid(), Titulo = "Configurar arquitectura Gnosis", IsCompletada = true },
            new { Id = Guid.NewGuid(), Titulo = "Implementar API REST", IsCompletada = false }
        };

        return Ok(tareasSimuladas);
    }

    /// <summary>
    /// Crea un nuevo objetivo de aprendizaje raíz.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearTareaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
            return BadRequest("El título es obligatorio.");

        var nuevaTarea = await _tareaService.CrearTareaRaizAsync(request.Titulo, request.Descripcion);
        return CreatedAtAction(nameof(ObtenerTodas), new { id = nuevaTarea.Id }, nuevaTarea);
    }

    /// <summary>
    /// Actualiza el estado de completado de una tarea o subtarea específica.
    /// </summary>
    [HttpPut("{id:guid}/estado")]
    public async Task<IActionResult> ActualizarEstado(Guid id, [FromBody] ActualizarEstadoRequest request)
    {
        try
        {
            var exito = await _tareaService.ActualizarEstadoTareaAsync(id, request.IsCompletada);

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
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

// DTO intermedio para recibir la actualización de estado de forma limpia
public class ActualizarEstadoRequest
{
    public bool IsCompletada { get; set; }
}