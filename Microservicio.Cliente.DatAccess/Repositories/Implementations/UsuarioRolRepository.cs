using Microservicio.Cliente.DatAccess.Contexts;
using Microservicio.Cliente.DatAccess.Entities.Seguridad;
using Microservicio.Cliente.DatAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microservicio.Cliente.DatAccess.Repositories.Implementations
{
    public class UsuarioRolRepository : GenericRepository<UsuarioRolEntity>, IUsuarioRolRepository
    {
        public UsuarioRolRepository(BookingDbContext context) : base(context) { }

        public async Task<IEnumerable<UsuarioRolEntity>> GetRolesPorUsuarioAsync(int usuarioId)
        {
            return await _dbSet
                .Include(ur => ur.Rol)
                .Where(ur => ur.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task AsignarRolAsync(int usuarioId, int rolId)
        {
            bool yaExiste = await TieneRolAsync(usuarioId, rolId);
            if (!yaExiste)
            {
                await _dbSet.AddAsync(new UsuarioRolEntity { UsuarioId = usuarioId, RolId = rolId });
                await _context.SaveChangesAsync();
            }
        }

        public async Task RevocarRolAsync(int usuarioId, int rolId)
        {
            var asignacion = await _dbSet.FirstOrDefaultAsync(ur => ur.UsuarioId == usuarioId && ur.RolId == rolId);
            if (asignacion != null)
            {
                _dbSet.Remove(asignacion);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> TieneRolAsync(int usuarioId, int rolId)
        {
            return await _dbSet.AnyAsync(ur => ur.UsuarioId == usuarioId && ur.RolId == rolId);
        }
    }
}
