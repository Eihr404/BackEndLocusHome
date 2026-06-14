
using Microsoft.EntityFrameworkCore;
using Reservas.DataAccess.Common;
using Reservas.DataAccess.Contexts;
using Reservas.DataAccess.Entities;
using Reservas.DataAccess.Repositories.Interfaces;

namespace Reservas.DataAccess.Repositories;

public class ReservasRepository : RepositoryBase<ReservaEntity>, IReservasRepository
{
    private readonly ReservasDbContext _reservasContext;

    public ReservasRepository(ReservasDbContext context) : base(context)
    {
        _reservasContext = context;
    }

    // Sobreescribimos FindAsync para incluir siempre DetallesHabitacion.
    // El RepositoryBase usa _dbSet.Where() sin Include, por lo que los detalles
    // quedarían siempre vacíos y el frontend no podría calcular disponibilidad.
    public override async Task<IEnumerable<ReservaEntity>> FindAsync(
        System.Linq.Expressions.Expression<Func<ReservaEntity, bool>> predicate)
    {
        return await _reservasContext.Reservas
            .Include(r => r.DetallesHabitacion)
            .Where(predicate)
            .ToListAsync();
    }

    public override async Task<ReservaEntity?> GetByIdAsync(int id)
    {
        return await _reservasContext.Reservas
            .Include(r => r.DetallesHabitacion)
            .FirstOrDefaultAsync(r => r.ReservaId == id);
    }
}

public class DescuentosRepository : RepositoryBase<DescuentoEntity>, IDescuentosRepository
{
    public DescuentosRepository(ReservasDbContext context) : base(context) { }
}

public class ReservaDetallesRepository : RepositoryBase<ReservaDetalleHabitacionEntity>, IReservaDetallesRepository
{
    public ReservaDetallesRepository(ReservasDbContext context) : base(context) { }
}

public class IdempotentRequestsRepository : RepositoryBase<IdempotentRequestEntity>, IIdempotentRequestsRepository
{
    private readonly ReservasDbContext _reservasContext;

    public IdempotentRequestsRepository(ReservasDbContext context) : base(context)
    {
        _reservasContext = context;
    }

    public Task<IdempotentRequestEntity?> GetByKeyAsync(string operationName, string idempotencyKey)
        => _reservasContext.IdempotentRequests.FirstOrDefaultAsync(
            x => x.OperationName == operationName && x.IdempotencyKey == idempotencyKey);
}
