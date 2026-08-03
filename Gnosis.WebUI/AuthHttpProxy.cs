using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Gnosis.WebUI
{
    public interface IAuthHttpProxy
    {
        Task<AuthResultado> RegistrarAsync(string email, string password, string? nombreVisible);
        Task<AuthResultado> LoginAsync(string email, string password);
        Task<AuthResultado> ConfirmarCorreoAsync(string email, string token);
        Task<AuthResultado> EntrarComoInvitadoAsync();
        Task<OperacionResultado> ReenviarConfirmacionAsync(string email);
        Task<OperacionResultado> OlvidePasswordAsync(string email);
        Task<OperacionResultado> RestablecerPasswordAsync(string email, string token, string nuevaPassword);
    }

    // Resultado "seguro": nunca lanza excepción por credenciales incorrectas, para que
    // Login.razor/Registro.razor puedan mostrar el mensaje de error tal cual venga de la API.
    // NoConfirmado se usa solo por LoginAsync para distinguir "correo no confirmado" de otros errores
    // (así el login puede ofrecer un botón de "reenviar correo" en vez de un mensaje genérico).
    public class AuthResultado
    {
        public bool Exito { get; set; }
        public bool NoConfirmado { get; set; }
        public string? Token { get; set; }
        public string? Email { get; set; }
        public string? Error { get; set; }
        // Solo se usa cuando Exito=true y no hay token (caso Registrar: "revisa tu correo...").
        public string? Mensaje { get; set; }
    }

    // Resultado genérico para operaciones que no devuelven token (registrar-con-confirmación,
    // reenviar confirmación, olvidé/restablecer password) y solo importan éxito + mensaje.
    public class OperacionResultado
    {
        public bool Exito { get; set; }
        public string? Mensaje { get; set; }
    }

    public class AuthHttpProxy : IAuthHttpProxy
    {
        private readonly HttpClient _httpClient;

        public AuthHttpProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AuthResultado> RegistrarAsync(string email, string password, string? nombreVisible) =>
            await EnviarConTokenAsync("api/auth/registrar", new { Email = email, Password = password, NombreVisible = nombreVisible }, esperaToken: false);

        public async Task<AuthResultado> LoginAsync(string email, string password) =>
            await EnviarConTokenAsync("api/auth/login", new { Email = email, Password = password }, esperaToken: true);

        public async Task<AuthResultado> ConfirmarCorreoAsync(string email, string token) =>
            await EnviarConTokenAsync("api/auth/confirmar-correo", new { Email = email, Token = token }, esperaToken: true);

        public async Task<AuthResultado> EntrarComoInvitadoAsync() =>
            await EnviarConTokenAsync("api/auth/invitado", new { }, esperaToken: true);

        public async Task<OperacionResultado> ReenviarConfirmacionAsync(string email) =>
            await EnviarMensajeAsync("api/auth/reenviar-confirmacion", new { Email = email });

        public async Task<OperacionResultado> OlvidePasswordAsync(string email) =>
            await EnviarMensajeAsync("api/auth/olvide-password", new { Email = email });

        public async Task<OperacionResultado> RestablecerPasswordAsync(string email, string token, string nuevaPassword) =>
            await EnviarMensajeAsync("api/auth/restablecer-password", new { Email = email, Token = token, NuevaPassword = nuevaPassword });

        // RegistrarAsync/LoginAsync/ConfirmarCorreoAsync: pueden devolver un token (login/confirmar
        // siempre; registrar ya no, desde que exige confirmar correo antes de loguear).
        private async Task<AuthResultado> EnviarConTokenAsync(string ruta, object cuerpo, bool esperaToken)
        {
            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync(ruta, cuerpo);

                if (!respuesta.IsSuccessStatusCode)
                {
                    var error = await respuesta.Content.ReadAsStringAsync();
                    return new AuthResultado
                    {
                        Exito = false,
                        NoConfirmado = respuesta.StatusCode == HttpStatusCode.Forbidden,
                        Error = string.IsNullOrWhiteSpace(error) ? "No se pudo completar la operación." : error
                    };
                }

                if (!esperaToken)
                {
                    // Registrar: éxito sin token, el mensaje viene en MensajeResponse.
                    var mensaje = await respuesta.Content.ReadFromJsonAsync<MensajeApiResponse>();
                    return new AuthResultado { Exito = true, Mensaje = mensaje?.Mensaje };
                }

                var datos = await respuesta.Content.ReadFromJsonAsync<AuthApiResponse>();
                if (datos == null)
                    return new AuthResultado { Exito = false, Error = "Respuesta vacía del servidor." };

                return new AuthResultado { Exito = true, Token = datos.Token, Email = datos.Email };
            }
            catch (Exception ex)
            {
                return new AuthResultado { Exito = false, Error = $"No se pudo conectar con el servidor: {ex.Message}" };
            }
        }

        private async Task<OperacionResultado> EnviarMensajeAsync(string ruta, object cuerpo)
        {
            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync(ruta, cuerpo);
                var texto = await respuesta.Content.ReadAsStringAsync();

                if (!respuesta.IsSuccessStatusCode)
                    return new OperacionResultado { Exito = false, Mensaje = string.IsNullOrWhiteSpace(texto) ? "No se pudo completar la operación." : texto };

                var mensaje = await respuesta.Content.ReadFromJsonAsync<MensajeApiResponse>();
                return new OperacionResultado { Exito = true, Mensaje = mensaje?.Mensaje };
            }
            catch (Exception ex)
            {
                return new OperacionResultado { Exito = false, Mensaje = $"No se pudo conectar con el servidor: {ex.Message}" };
            }
        }

        private class AuthApiResponse
        {
            public string Token { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? NombreVisible { get; set; }
        }

        private class MensajeApiResponse
        {
            public string Mensaje { get; set; } = string.Empty;
        }
    }
}
