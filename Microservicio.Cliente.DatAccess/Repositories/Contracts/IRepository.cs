using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microservicio.Cliente.DatAccess.Repositories.Contracts
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> Query();
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }
}
