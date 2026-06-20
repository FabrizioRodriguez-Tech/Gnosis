using Gnosis.Domain.Interfaces;
using Gnosis.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Gnosis.Infrastructure;

// Usamos el constructor principal directamente en la definición de la clase EfRepository<T>(GnosisDbContext context)
public class EfRepository<T>(GnosisDbContext context) : IRepository<T> where T : class
{
    protected readonly GnosisDbContext _context = context;

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task AgregarAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}