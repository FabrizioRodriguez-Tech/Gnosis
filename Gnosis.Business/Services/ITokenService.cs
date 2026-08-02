using System;

namespace Gnosis.Business.Services
{
    public interface ITokenService
    {
        // Genera el JWT que el cliente Blazor guarda y reenvía en cada petición
        // (header Authorization: Bearer <token>).
        string GenerarToken(Guid usuarioId, string email);
    }
}
