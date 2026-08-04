using System;

namespace Gnosis.Domain.Entities
{
    // "Focus Forest": cada vez que el usuario arranca una sesión de Pomodoro en modo Enfoque
    // "siembra" una planta. Si completa la sesión, la planta crece (Crecio=true); si la cancela
    // antes de tiempo, se marchita (Crecio=false). Se guarda un registro por intento, ya resuelto
    // (no hay estado "pendiente" en la base — mientras la sesión está corriendo solo vive en
    // memoria del cliente, ver Home.razor).
    public class SiembraEnfoque
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UsuarioId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public bool Crecio { get; set; }
    }
}
