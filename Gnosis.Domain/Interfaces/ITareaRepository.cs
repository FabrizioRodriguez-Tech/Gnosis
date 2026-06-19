using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gnosis.Domain.Entities;

namespace Gnosis.Domain.Interfaces
{
    public interface ITareaRepository : IRepository<Tarea>
    {
        // Hereda automáticamente GetByIdAsync, GetAllAsync, Agregar, etc.

        // Operación específica para la gestión de carga mental en Gnosis
        Task<IEnumerable<Tarea>> GetTareasPrincipalesAsync();
    }
}