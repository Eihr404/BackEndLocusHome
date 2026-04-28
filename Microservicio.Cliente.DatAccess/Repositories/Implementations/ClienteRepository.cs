using Microservicio.Cliente.DatAccess.Common;
using Microservicio.Cliente.DatAccess.Contexts;
using Microservicio.Cliente.DatAccess.Entities.Core;
using Microservicio.Cliente.DatAccess.Entities.Seguridad;
using Microservicio.Cliente.DatAccess.Repositories.Implementations;
using Microservicio.Cliente.DatAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microservicio.Cliente.DatAccess.Repositories.Implementations
{
    public class ClienteRepository : GenericRepository<ClienteEntity>, IClienteRepository
    {
        public ClienteRepository(BookingDbContext context) : base(context) { }

        public async Task<ClienteEntity?> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && !c.EliminadoLogico);
        }

        public async Task<PagedResult<ClienteEntity>> GetPaginadoAsync(int pageNumber, int pageSize)
        {
            var query = _dbSet.Where(c => !c.EliminadoLogico);
            int totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<ClienteEntity> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }

        public async Task<bool> ExisteAsync(int clienteId)
        {
            return await _dbSet.AnyAsync(c => c.ClienteId == clienteId && !c.EliminadoLogico);
        }
    }
}
