using Microsoft.EntityFrameworkCore;
using Facturacion.DataAccess.Entities;
using Facturacion.DataAccess.Repositories.Interfaces;
using Facturacion.DataManagement.Interfaces;
using Facturacion.DataManagement.Models;

namespace Facturacion.DataManagement.Services;

public class IdempotentRequestDataService : IIdempotentRequestDataService
{
    private readonly IIdempotentRequestsRepository _repository;

    public IdempotentRequestDataService(IIdempotentRequestsRepository repository)
    {
        _repository = repository;
    }

    public async Task<IdempotentRequestDataModel?> GetByKeyAsync(string operationName, string idempotencyKey)
    {
        var entity = await _repository.GetByKeyAsync(operationName, idempotencyKey);
        return entity == null ? null : ToDataModel(entity);
    }

    public async Task<bool> TryCreatePendingAsync(IdempotentRequestDataModel model)
    {
        try
        {
            await _repository.AddAsync(new IdempotentRequestEntity
            {
                IdempotencyKey = model.IdempotencyKey,
                OperationName = model.OperationName,
                RequestHash = model.RequestHash,
                Status = model.Status,
                ResourceId = model.ResourceId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task MarkCompletedAsync(string operationName, string idempotencyKey, int resourceId)
    {
        var entity = await _repository.GetByKeyAsync(operationName, idempotencyKey)
            ?? throw new KeyNotFoundException($"No existe registro idempotente para {operationName} con key {idempotencyKey}.");

        entity.Status = "Completed";
        entity.ResourceId = resourceId;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
    }

    private static IdempotentRequestDataModel ToDataModel(IdempotentRequestEntity entity)
        => new()
        {
            IdempotencyKey = entity.IdempotencyKey,
            OperationName = entity.OperationName,
            RequestHash = entity.RequestHash,
            Status = entity.Status,
            ResourceId = entity.ResourceId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
}
