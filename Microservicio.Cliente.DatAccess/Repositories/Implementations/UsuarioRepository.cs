using Microservicio.Cliente.DatAccess.Contexts;
using Microservicio.Cliente.DatAccess.Entities.Seguridad;
using Microservicio.Cliente.DatAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microservicio.Cliente.DatAccess.Repositories.Implementations
{
    public class UsuarioRepository : GenericRepository<UsuarioEntity>, IUsuarioRepository
    {
        public UsuarioRepository(BookingDbContext context) : base(context) { }

        public async Task<UsuarioEntity?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && !u.EliminadoLogico);
        }

        public async Task<bool> EmailExisteAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<UsuarioEntity>> GetByRolAsync(int rolId)
        {
            return await _dbSet.Where(u => u.RolId == rolId && !u.EliminadoLogico).ToListAsync();
        }
    }
}
