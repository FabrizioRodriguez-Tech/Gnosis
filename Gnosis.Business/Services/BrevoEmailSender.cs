using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Gnosis.Business.Services;

// Implementación de IEmailSender vía la API HTTP de Brevo (antes Sendinblue), en vez de SMTP.
// Render bloquea el tráfico saliente a los puertos SMTP (25/465/587) en su plan Free, así que
// SmtpEmailSender (MailKit) se quedaba colgado hasta hacer timeout ahí. La API de Brevo usa
// HTTPS normal (puerto 443), que no está bloqueado.
internal class BrevoEmailSender(HttpClient httpClient, IConfiguration configuracion) : IEmailSender
{
    public async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        var apiKey = configuracion["Brevo:ApiKey"]
            ?? throw new InvalidOperationException("Falta configurar Brevo:ApiKey (appsettings.Development.json o variables de entorno).");
        var remitenteEmail = configuracion["Brevo:RemitenteEmail"]
            ?? throw new InvalidOperationException("Falta configurar Brevo:RemitenteEmail.");
        var remitenteNombre = configuracion["Brevo:RemitenteNombre"] ?? "Gnosis";

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
        request.Headers.Add("api-key", apiKey);
        request.Headers.Add("accept", "application/json");
        request.Content = JsonContent.Create(new
        {
            sender = new { name = remitenteNombre, email = remitenteEmail },
            to = new[] { new { email = destinatario } },
            subject = asunto,
            htmlContent = cuerpoHtml
        });

        var respuesta = await httpClient.SendAsync(request);
        if (!respuesta.IsSuccessStatusCode)
        {
            var detalle = await respuesta.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Brevo devolvió {(int)respuesta.StatusCode}: {detalle}");
        }
    }
}
