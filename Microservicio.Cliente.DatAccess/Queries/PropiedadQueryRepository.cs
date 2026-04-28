using Microservicio.Cliente.DatAccess.Contexts;
using Microservicio.Cliente.DatAccess.Entities.Core;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microservicio.Cliente.DatAccess.Common;

namespace Microservicio.Cliente.DatAccess.Queries
{
    // Separa las consultas complejas de lectura para no mezclarlas con el CRUD básico
    public class PropiedadQueryRepository
    {
        private readonly BookingDbContext _context;

        public PropiedadQueryRepository(BookingDbContext context)
        {
            _context = context;
        }

        // Método de ejemplo para buscar alojamientos de forma paginada para los clientes
        public async Task<PagedResult<PropiedadEntity>> BuscarActivasPorCiudadPaginadoAsync(int ciudadId, int pageNumber, int pageSize)
        {
            var query = _context.Propiedades
                .Where(p => p.CiudadId == ciudadId && p.Estado == "Activa");

            int totalCount = await query.CountAsync();
            
            var items = await query
                .OrderByDescending(p => p.CalificacionPromedio)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<PropiedadEntity>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
