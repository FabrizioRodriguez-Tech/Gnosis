using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Gnosis.WebUI.services;

// AuthenticationStateProvider a la medida: no valida la firma del token (eso lo hace la API
// en cada petición), solo lee sus claims para saber quién es el usuario y si ya expiró,
// de forma que <AuthorizeView>/<AuthorizeRouteView> sepan qué mostrar en el cliente.
public class JwtAuthenticationStateProvider(TokenStorageService tokenStorage) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonimo = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokenStorage.ObtenerTokenAsync();
        var claims = string.IsNullOrWhiteSpace(token) ? null : LeerClaimsDelToken(token);

        if (claims == null)
            return new AuthenticationState(Anonimo);

        var identidad = new ClaimsIdentity(claims, authenticationType: "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identidad));
    }

    // Se llama después de guardar/borrar el token para que la UI reaccione de inmediato
    // (sin esto, <AuthorizeView> seguiría mostrando el estado viejo hasta el próximo render).
    public void NotificarCambioDeSesion() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    private static List<Claim>? LeerClaimsDelToken(string token)
    {
        try
        {
            var partes = token.Split('.');
            if (partes.Length != 3) return null;

            var json = Encoding.UTF8.GetString(ParseBase64Url(partes[1]));
            var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            if (payload == null) return null;

            var claims = new List<Claim>();
            foreach (var (tipo, valor) in payload)
            {
                if (valor.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in valor.EnumerateArray())
                        claims.Add(new Claim(tipo, item.ToString()));
                }
                else
                {
                    claims.Add(new Claim(tipo, valor.ToString()));
                }
            }

            // Claim estándar "exp" = segundos desde epoch. Si ya venció, se trata como no logueado.
            var exp = claims.FirstOrDefault(c => c.Type == "exp");
            if (exp != null && long.TryParse(exp.Value, out var expUnix)
                && DateTimeOffset.FromUnixTimeSeconds(expUnix) < DateTimeOffset.UtcNow)
            {
                return null;
            }

            return claims;
        }
        catch
        {
            return null;
        }
    }

    private static byte[] ParseBase64Url(string base64Url)
    {
        var base64 = base64Url.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        return Convert.FromBase64String(base64);
    }
}
