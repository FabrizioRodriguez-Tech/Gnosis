using System;

namespace Gnosis.Domain.Entities
{
    public class BloqueTiempo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UsuarioId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        // Color en formato hexadecimal para distinguir bloques en la vista de semana
        public string? Color { get; set; }

        // Relación opcional con una Tarea existente (asignar una tarea a un horario)
        public Guid? TareaId { get; set; }
        public Tarea? Tarea { get; set; }
    }
}
