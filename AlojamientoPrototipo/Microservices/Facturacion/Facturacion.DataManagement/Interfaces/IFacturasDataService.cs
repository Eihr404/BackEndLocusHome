using Facturacion.DataManagement.Models;

namespace Facturacion.DataManagement.Interfaces;

public interface IFacturasDataService
{
    Task<FacturaDataModel?> GetByIdAsync(int id);
    Task<IEnumerable<FacturaDataModel>> GetByReservaIdAsync(int reservaId);
    Task<FacturaDataModel> CreateAsync(FacturaDataModel model);
    Task UpdateStatusAsync(int id, string nuevoEstado);
}

public interface IIdempotentRequestDataService
{
    Task<IdempotentRequestDataModel?> GetByKeyAsync(string operationName, string idempotencyKey);
    Task<bool> TryCreatePendingAsync(IdempotentRequestDataModel model);
    Task MarkCompletedAsync(string operationName, string idempotencyKey, int resourceId);
}
