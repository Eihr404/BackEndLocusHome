using Microsoft.EntityFrameworkCore;
using Facturacion.DataAccess.Common;
using Facturacion.DataAccess.Contexts;
using Facturacion.DataAccess.Entities;
using Facturacion.DataAccess.Repositories.Interfaces;

namespace Facturacion.DataAccess.Repositories;

public class IdempotentRequestsRepository : RepositoryBase<IdempotentRequestEntity>, IIdempotentRequestsRepository
{
    private readonly FacturacionDbContext _facturacionContext;

    public IdempotentRequestsRepository(FacturacionDbContext context) : base(context)
    {
        _facturacionContext = context;
    }

    public Task<IdempotentRequestEntity?> GetByKeyAsync(string operationName, string idempotencyKey)
        => _facturacionContext.IdempotentRequests.FirstOrDefaultAsync(
            x => x.OperationName == operationName && x.IdempotencyKey == idempotencyKey);
}
