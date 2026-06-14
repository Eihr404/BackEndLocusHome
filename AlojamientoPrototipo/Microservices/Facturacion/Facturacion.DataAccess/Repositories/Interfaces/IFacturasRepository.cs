using Facturacion.DataAccess.Entities;

namespace Facturacion.DataAccess.Repositories.Interfaces;

public interface IFacturasRepository : IRepositoryBase<FacturaEntity> { }

public interface IIdempotentRequestsRepository : IRepositoryBase<IdempotentRequestEntity>
{
    Task<IdempotentRequestEntity?> GetByKeyAsync(string operationName, string idempotencyKey);
}
