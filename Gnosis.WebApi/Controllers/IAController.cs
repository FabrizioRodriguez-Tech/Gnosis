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
            return StatusCode(500, "No se pudo consultar la IA en este momento.");
        }
    }
}
