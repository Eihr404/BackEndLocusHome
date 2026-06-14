namespace Reservas.API.Idempotency;

public interface IProcessedMessageStore
{
    Task<bool> HasProcessedAsync(Guid messageId, string consumer, CancellationToken cancellationToken = default);

    Task MarkProcessedAsync(Guid messageId, string consumer, CancellationToken cancellationToken = default);
}
