using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Gnosis.Business.Services;

// Implementación concreta de IEmailSender vía SMTP (Gmail), usando MailKit en vez de
// System.Net.Mail.SmtpClient: este último es obsoleto y da problemas de autenticación
// intermitentes contra Gmail ("5.7.0 Authentication Required") incluso con una contraseña
// de aplicación válida. Vive junto a TokenService en Business (no en Infrastructure) porque
// Infrastructure solo depende de Domain en este proyecto.
internal class SmtpEmailSender(IConfiguration configuracion) : IEmailSender
{
    public async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        var host = configuracion["Smtp:Host"]
            ?? throw new InvalidOperationException("Falta configurar Smtp:Host (appsettings.Development.json).");
        var puerto = int.TryParse(configuracion["Smtp:Puerto"], out var p) ? p : 587;
        var usuario = configuracion["Smtp:Usuario"]
            ?? throw new InvalidOperationException("Falta configurar Smtp:Usuario (appsettings.Development.json).");
        var clave = configuracion["Smtp:Clave"]
            ?? throw new InvalidOperationException("Falta configurar Smtp:Clave (appsettings.Development.json). Debe ser una 'contraseña de aplicación' de Gmail, no tu contraseña normal.");
        var nombreRemitente = configuracion["Smtp:NombreRemitente"] ?? "Gnosis";

        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress(nombreRemitente, usuario));
        mensaje.To.Add(MailboxAddress.Parse(destinatario));
        mensaje.Subject = asunto;
        mensaje.Body = new TextPart("html") { Text = cuerpoHtml };

        using var cliente = new SmtpClient();
        // StartTls explícito: el puerto 587 de Gmail negocia la conexión en texto plano y luego
        // sube a TLS con STARTTLS (a diferencia del 465, que sería SSL implícito desde el inicio).
        await cliente.ConnectAsync(host, puerto, SecureSocketOptions.StartTls);
        await cliente.AuthenticateAsync(usuario, clave);
        await cliente.SendAsync(mensaje);
        await cliente.DisconnectAsync(quit: true);
    }
}
