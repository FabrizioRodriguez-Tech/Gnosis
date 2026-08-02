using Microsoft.AspNetCore.Identity;

namespace Gnosis.Infrastructure.Identity;

// Usuario de la aplicación. Vive en Infrastructure (no en Domain) porque depende directamente
// de ASP.NET Core Identity/EF Core; Gnosis.Domain se mantiene libre de dependencias de framework
// (ver ADR-01/ADR-03). Las entidades del dominio (Tarea, SesionEnfoque, BloqueTiempo) solo guardan
// el Guid del dueño (UsuarioId), no una referencia a esta clase.
public class ApplicationUser : IdentityUser<Guid>
{
    public string? NombreVisible { get; set; }
}
