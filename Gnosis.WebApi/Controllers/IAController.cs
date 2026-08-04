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
}
