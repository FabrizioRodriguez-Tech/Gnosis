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
    public async Task<IActionResult> ObtenerTodas()
    {
        var tareas = await _tareaService.ObtenerTareasPrincipalesAsync();
        return Ok(tareas);
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
}

// DTO intermedio para recibir los datos de la petición HTTP de forma limpia
public class CrearTareaRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}