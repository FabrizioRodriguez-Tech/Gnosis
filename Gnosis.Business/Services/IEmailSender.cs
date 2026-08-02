using System.Threading.Tasks;

namespace Gnosis.Business.Services
{
    // Abstracción para no acoplar el resto de la app (AuthController) al mecanismo concreto de
    // envío (hoy SMTP de Gmail, mañana podría ser otro proveedor) — mismo patrón que ITokenService.
    public interface IEmailSender
    {
        Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml);
    }
}
