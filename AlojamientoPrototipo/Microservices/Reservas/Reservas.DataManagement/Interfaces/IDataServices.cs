using Reservas.DataManagement.Models;

namespace Reservas.DataManagement.Interfaces;

public interface IReservasDataService
{
    Task<ReservaDataModel?> GetByIdAsync(int id);
    Task<IEnumerable<ReservaDataModel>> GetByClienteIdAsync(int clienteId);
    Task<IEnumerable<ReservaDataModel>> GetByAlojamientoIdAsync(int alojamientoId);
    Task<ReservaDataModel> CreateAsync(ReservaDataModel model);
    Task UpdateStatusAsync(int id, string nuevoEstado);
    Task DeleteAsync(int id);
}

public interface IDescuentosDataService
{
    Task<DescuentoDataModel?> GetByCodigoAsync(string codigo);
}

public interface IIdempotentRequestDataService
{
    Task<IdempotentRequestDataModel?> GetByKeyAsync(string operationName, string idempotencyKey);
    Task<bool> TryCreatePendingAsync(IdempotentRequestDataModel model);
    Task MarkCompletedAsync(string operationName, string idempotencyKey, int resourceId);
}
