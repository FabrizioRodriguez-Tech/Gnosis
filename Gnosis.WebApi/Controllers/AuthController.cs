using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Gnosis.Business.Services;
using Gnosis.Infrastructure.Identity;

namespace Gnosis.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IEmailSender emailSender,
    IConfiguration configuracion,
    ILogger<AuthController> logger) : ControllerBase
{
    // Dirección base del cliente Blazor (WebUI), para construir los enlaces de los correos.
    // http://localhost:5254 en desarrollo; en producción se configura en appsettings/variables de entorno.
    private string UrlCliente => (configuracion["App:UrlCliente"] ?? "http://localhost:5254").TrimEnd('/');

    /// <summary>
    /// Crea una cuenta nueva (registro abierto: cualquiera puede registrarse) y envía un correo
    /// de confirmación. Ya no devuelve token: hasta que confirme el correo no puede iniciar sesión.
    /// </summary>
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("El correo y la contraseña son obligatorios.");

        // Si ya existe una cuenta con ese correo pero nunca se confirmó (típico de un intento
        // anterior donde el envío del correo falló, p. ej. por Smtp mal configurado), no bloqueamos
        // el registro: reenviamos la confirmación en vez de devolver "correo ya en uso".
        var usuarioExistente = await userManager.FindByEmailAsync(request.Email);
        if (usuarioExistente != null)
        {
            if (usuarioExistente.EmailConfirmed)
                return BadRequest("Ya existe una cuenta con ese correo. Inicia sesión o recupera tu contraseña.");

            if (!await EnviarCorreoConfirmacionAsync(usuarioExistente))
                return StatusCode(500, "La cuenta ya existía pero no se pudo enviar el correo de confirmación. Revisa la configuración de correo (Smtp) del servidor.");

            return Ok(new MensajeResponse("Ya existía una cuenta pendiente con ese correo: te reenviamos el enlace de confirmación."));
        }

        var usuario = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            NombreVisible = string.IsNullOrWhiteSpace(request.NombreVisible) ? null : request.NombreVisible
        };

        var resultado = await userManager.CreateAsync(usuario, request.Password);
        if (!resultado.Succeeded)
            return BadRequest(string.Join(" ", resultado.Errors.Select(e => e.Description)));

        if (!await EnviarCorreoConfirmacionAsync(usuario))
            return StatusCode(500, "La cuenta se creó pero no se pudo enviar el correo de confirmación. Revisa la configuración de correo (Smtp) del servidor y usa 'reenviar confirmación' desde el login.");

        return Ok(new MensajeResponse("Cuenta creada. Revisa tu correo para confirmarla antes de iniciar sesión."));
    }

    /// <summary>
    /// Verifica correo + contraseña. Si son correctos pero el correo aún no está confirmado,
    /// rechaza el login con un mensaje específico para poder guiar al usuario en el cliente.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var usuario = await userManager.FindByEmailAsync(request.Email);
        if (usuario == null || !await userManager.CheckPasswordAsync(usuario, request.Password))
            return Unauthorized("Correo o contraseña incorrectos.");

        if (!usuario.EmailConfirmed)
            return StatusCode(StatusCodes.Status403Forbidden, "Debes confirmar tu correo antes de iniciar sesión. Revisa tu bandeja de entrada.");

        var token = tokenService.GenerarToken(usuario.Id, usuario.Email!);
        return Ok(new AuthResponse { Token = token, Email = usuario.Email!, NombreVisible = usuario.NombreVisible });
    }

    /// <summary>
    /// Confirma el correo con el token recibido por email. Si es válido, además loguea de una vez
    /// (evita que el usuario tenga que volver a escribir la contraseña justo después de confirmar).
    /// </summary>
    [HttpPost("confirmar-correo")]
    public async Task<IActionResult> ConfirmarCorreo([FromBody] ConfirmarCorreoRequest request)
    {
        var usuario = await userManager.FindByEmailAsync(request.Email);
        if (usuario == null)
            return BadRequest("Enlace de confirmación inválido.");

        if (!usuario.EmailConfirmed)
        {
            var resultado = await userManager.ConfirmEmailAsync(usuario, request.Token);
            if (!resultado.Succeeded)
                return BadRequest("El enlace de confirmación es inválido o ya expiró.");
        }

        var token = tokenService.GenerarToken(usuario.Id, usuario.Email!);
        return Ok(new AuthResponse { Token = token, Email = usuario.Email!, NombreVisible = usuario.NombreVisible });
    }

    /// <summary>
    /// Reenvía el correo de confirmación. Responde igual exista o no la cuenta / ya esté confirmada,
    /// para no revelar qué correos están registrados.
    /// </summary>
    [HttpPost("reenviar-confirmacion")]
    public async Task<IActionResult> ReenviarConfirmacion([FromBody] ReenviarConfirmacionRequest request)
    {
        var usuario = await userManager.FindByEmailAsync(request.Email);
        if (usuario != null && !usuario.EmailConfirmed)
        {
            if (!await EnviarCorreoConfirmacionAsync(usuario))
                return StatusCode(500, "No se pudo enviar el correo. Revisa la configuración de correo (Smtp) del servidor.");
        }

        return Ok(new MensajeResponse("Si la cuenta existe y no ha sido confirmada, se envió un nuevo correo."));
    }

    /// <summary>
    /// Pide el enlace para restablecer contraseña. Responde igual exista o no la cuenta,
    /// para no revelar qué correos están registrados.
    /// </summary>
    [HttpPost("olvide-password")]
    public async Task<IActionResult> OlvidePassword([FromBody] OlvidePasswordRequest request)
    {
        var usuario = await userManager.FindByEmailAsync(request.Email);
        if (usuario != null)
        {
            try
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(usuario);
                var enlace = $"{UrlCliente}/restablecer-password?email={WebUtility.UrlEncode(usuario.Email)}&token={WebUtility.UrlEncode(token)}";
                await emailSender.EnviarAsync(usuario.Email!, "Restablece tu contraseña de Gnosis",
                    $"<p>Toca el siguiente enlace para elegir una contraseña nueva:</p><p><a href=\"{enlace}\">{enlace}</a></p><p>Si no pediste esto, ignora este correo.</p>");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "No se pudo enviar el correo de restablecimiento a {Email}", usuario.Email);
                return StatusCode(500, "No se pudo enviar el correo. Revisa la configuración de correo (Smtp) del servidor.");
            }
        }

        return Ok(new MensajeResponse("Si el correo existe, se envió un enlace para restablecer la contraseña."));
    }

    /// <summary>
    /// Aplica la contraseña nueva usando el token que llegó por correo.
    /// </summary>
    [HttpPost("restablecer-password")]
    public async Task<IActionResult> RestablecerPassword([FromBody] RestablecerPasswordRequest request)
    {
        var usuario = await userManager.FindByEmailAsync(request.Email);
        if (usuario == null)
            return BadRequest("Enlace inválido.");

        var resultado = await userManager.ResetPasswordAsync(usuario, request.Token, request.NuevaPassword);
        if (!resultado.Succeeded)
            return BadRequest(string.Join(" ", resultado.Errors.Select(e => e.Description)));

        return Ok(new MensajeResponse("Contraseña actualizada. Ya puedes iniciar sesión."));
    }

    // Devuelve false (en vez de dejar que la excepción tumbe el request) si el envío del correo
    // falla — típicamente porque Smtp:Clave en appsettings.Development.json sigue siendo el
    // placeholder o está mal, y no queremos que eso se vea como un 500 con stack trace crudo.
    private async Task<bool> EnviarCorreoConfirmacionAsync(ApplicationUser usuario)
    {
        try
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(usuario);
            var enlace = $"{UrlCliente}/confirmar-correo?email={WebUtility.UrlEncode(usuario.Email)}&token={WebUtility.UrlEncode(token)}";
            await emailSender.EnviarAsync(usuario.Email!, "Confirma tu correo en Gnosis",
                $"<p>Toca el siguiente enlace para confirmar tu cuenta en Gnosis:</p><p><a href=\"{enlace}\">{enlace}</a></p>");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo enviar el correo de confirmación a {Email}", usuario.Email);
            return false;
        }
    }
}

public class RegistrarUsuarioRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? NombreVisible { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ConfirmarCorreoRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class ReenviarConfirmacionRequest
{
    public string Email { get; set; } = string.Empty;
}

public class OlvidePasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class RestablecerPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NuevaPassword { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NombreVisible { get; set; }
}

public class MensajeResponse(string mensaje)
{
    public string Mensaje { get; set; } = mensaje;
}
