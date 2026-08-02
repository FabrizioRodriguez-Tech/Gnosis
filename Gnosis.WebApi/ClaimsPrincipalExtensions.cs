using System.Security.Claims;

namespace Gnosis.WebApi;

internal static class ClaimsPrincipalExtensions
{
    // El JWT guarda el Id del usuario en el claim estándar NameIdentifier (ver TokenService).
    // Los controllers [Authorize] lo usan para filtrar/asignar dueño en cada operación.
    public static Guid ObtenerUsuarioId(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(valor, out var id))
            throw new UnauthorizedAccessException("El token no trae un identificador de usuario válido.");

        return id;
    }
}
