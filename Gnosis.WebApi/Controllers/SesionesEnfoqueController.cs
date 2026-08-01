using Microsoft.AspNetCore.Mvc;
using Gnosis.Business.Services;

namespace Gnosis.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SesionesEnfoqueController : ControllerBase
{
    private readonly ISesionEnfoqueService _sesionEnfoqueService;

    public SesionesEnfoqueController(ISesionEnfoqueService sesionEnfoqueService)
    {
        _sesionEnfoqueService = sesionEnfoqueService;
    }

    /// <summary>
    /// Registra una sesión de enfoque/descanso que ya terminó. El Pomodoro del cliente la reporta
    /// una sola vez, al finalizar el ciclo (no hay un paso previo de "iniciar").
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarSesionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TipoSesion))
            return BadRequest("El tipo de sesión es obligatorio.");

        try
        {
            var sesion = await _sesionEnfoqueService.RegistrarSesionCompletadaAsync(request.TipoSesion, request.DuracionMinutos);
            return Ok(sesion);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno en el servidor: {ex.Message}");
        }
    }
}

// DTO intermedio para recibir el registro de sesión de forma limpia
public class RegistrarSesionRequest
{
    public string TipoSesion { get; set; } = string.Empty;
    public int DuracionMinutos { get; set; }
}
