using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gnosis.Business.Services;

namespace Gnosis.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EstadisticasController : ControllerBase
{
    private readonly IEstadisticasService _estadisticasService;

    public EstadisticasController(IEstadisticasService estadisticasService)
    {
        _estadisticasService = estadisticasService;
    }

    /// <summary>
    /// Devuelve estadísticas de productividad (sesiones, minutos de enfoque, tareas completadas y racha)
    /// para un rango de fechas. Sin parámetros, usa los últimos 7 días.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        try
        {
            var fin = hasta ?? DateTime.UtcNow.Date.AddDays(1);
            var inicio = desde ?? fin.AddDays(-7);

            var estadisticas = await _estadisticasService.ObtenerEstadisticasSemanaAsync(User.ObtenerUsuarioId(), inicio, fin);
            return Ok(estadisticas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno en el servidor: {ex.Message}");
        }
    }
}
