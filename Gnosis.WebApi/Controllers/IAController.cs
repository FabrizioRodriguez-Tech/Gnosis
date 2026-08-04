using Gnosis.Business.Models;
using Gnosis.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gnosis.WebApi.Controllers;

[ApiController]
[Route("api/IA")]
[Authorize]
public class IAController(IIAService iaService, ILogger<IAController> logger) : ControllerBase
{
    [HttpPost("consultar")]
    public async Task<IActionResult> Consultar([FromBody] IARequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Mensaje))
            return BadRequest("El mensaje no puede estar vacío.");

        try
        {
            var respuesta = await iaService.ConsultarAsync(request);
            return Ok(respuesta);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falló la consulta a la IA");
            // Igual que TareasController: se expone ex.Message (no el stack completo) para que el
            // error real se vea también en el cliente sin tener que ir a buscar los logs del server.
            return StatusCode(500, $"No se pudo consultar la IA en este momento: {ex.Message}");
        }
    }

    /// <summary>
    /// Task Breaker: genera 4-5 subtareas concretas para una tarea existente.
    /// </summary>
    [HttpPost("desglosar")]
    public async Task<IActionResult> Desglosar([FromBody] DesglosarTareaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TituloTarea))
            return BadRequest("El título de la tarea no puede estar vacío.");

        try
        {
            var respuesta = await iaService.DesglosarTareaAsync(request);
            return Ok(respuesta);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falló el desglose de tarea con IA");
            return StatusCode(500, $"No se pudo desglosar la tarea en este momento: {ex.Message}");
        }
    }

    /// <summary>
    /// Daily Retrospective: resumen ejecutivo del día a partir de las tareas completadas.
    /// </summary>
    [HttpPost("resumen-dia")]
    public async Task<IActionResult> ResumenDia([FromBody] ResumenDiaRequest request)
    {
        try
        {
            var respuesta = await iaService.GenerarResumenDiaAsync(request);
            return Ok(respuesta);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falló la generación del resumen del día");
            return StatusCode(500, $"No se pudo generar el resumen en este momento: {ex.Message}");
        }
    }

    /// <summary>
    /// Estima cuántos ciclos de Pomodoro requerirá una tarea.
    /// </summary>
    [HttpPost("estimar-pomodoros")]
    public async Task<IActionResult> EstimarPomodoros([FromBody] EstimarPomodorosRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TituloTarea))
            return BadRequest("El título de la tarea no puede estar vacío.");

        try
        {
            var respuesta = await iaService.EstimarPomodorosAsync(request);
            return Ok(respuesta);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falló la estimación de pomodoros");
            return StatusCode(500, $"No se pudo estimar en este momento: {ex.Message}");
        }
    }
}
