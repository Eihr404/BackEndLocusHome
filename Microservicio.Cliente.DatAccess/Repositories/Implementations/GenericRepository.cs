using Microservicio.Cliente.DatAccess.Contexts;
using Microservicio.Cliente.DatAccess.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Cliente.DatAccess.Repositories.Implementations
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        protected readonly BookingDbContext _context;

        // Lazy: se resuelve en el primer acceso, no en el constructor
        private DbSet<T>? _dbSetBacking;
        internal DbSet<T> _dbSet => _dbSetBacking ??= _context.Set<T>();

        public GenericRepository(BookingDbContext context)
        {
            _context = context;
        }

        public IQueryable<T> Query()
            => _dbSet.AsQueryable();

        public async Task<IEnumerable<T>> GetAllAsync()
            => await _dbSet.ToListAsync();

        public async Task<T?> GetByIdAsync(int id)
            => await _dbSet.FindAsync(id);

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
