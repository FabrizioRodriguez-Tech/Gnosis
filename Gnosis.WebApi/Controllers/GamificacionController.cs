using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gnosis.Business.Services;

namespace Gnosis.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GamificacionController : ControllerBase
{
    private readonly IGamificacionService _gamificacionService;

    public GamificacionController(IGamificacionService gamificacionService)
    {
        _gamificacionService = gamificacionService;
    }

    /// <summary>
    /// Devuelve XP, nivel, racha y el jardín (siembras) del mes en curso.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Obtener()
    {
        try
        {
            var datos = await _gamificacionService.ObtenerAsync(User.ObtenerUsuarioId());
            return Ok(datos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno en el servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Registra el resultado de una siembra (sesión de Pomodoro en modo Enfoque):
    /// crecio = true si se completó, false si se canceló antes de tiempo.
    /// </summary>
    [HttpPost("siembras")]
    public async Task<IActionResult> RegistrarSiembra([FromBody] RegistrarSiembraRequest request)
    {
        try
        {
            var siembra = await _gamificacionService.RegistrarSiembraAsync(User.ObtenerUsuarioId(), request.Crecio);
            return Ok(siembra);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno en el servidor: {ex.Message}");
        }
    }
}

public class RegistrarSiembraRequest
{
    public bool Crecio { get; set; }
}
