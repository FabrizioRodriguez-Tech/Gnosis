using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Gnosis.Business.Services;

// Usamos el constructor principal de C# para simplificar el código e inyectar la configuración directamente
internal class TokenService(IConfiguration configuration) : ITokenService
{
    public string GenerarToken(Guid usuarioId, string email)
    {
        var claveSecreta = configuration["Jwt:Clave"]
            ?? throw new InvalidOperationException(
                "Falta configurar Jwt:Clave (appsettings.Development.json). Sin esto no se pueden firmar tokens.");

        var emisor = configuration["Jwt:Emisor"] ?? "Gnosis";
        var audiencia = configuration["Jwt:Audiencia"] ?? "Gnosis";
        var minutosExpiracion = int.TryParse(configuration["Jwt:MinutosExpiracion"], out var minutos)
            ? minutos
            : 60 * 24 * 7; // 7 días por defecto: es una app de productividad personal, no hace falta re-loguear a diario

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            // ClaimTypes.NameIdentifier es el claim que ASP.NET Core usa por convención
            // para User.Identity en los controllers protegidos con [Authorize].
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var claveFirma = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveSecreta));
        var credenciales = new SigningCredentials(claveFirma, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: emisor,
            audience: audiencia,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutosExpiracion),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
