using Microsoft.EntityFrameworkCore;
using Reservas.DataAccess.Contexts;
using Reservas.DataAccess.Entities;

namespace Reservas.API.Idempotency;

public sealed class ProcessedMessageStore : IProcessedMessageStore
{
    private readonly ReservasDbContext _dbContext;

    public ProcessedMessageStore(ReservasDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> HasProcessedAsync(Guid messageId, string consumer, CancellationToken cancellationToken = default)
        => _dbContext.Set<ProcessedMessageEntity>()
            .AnyAsync(
                x => x.MessageId == messageId.ToString("N") && x.Consumer == consumer,
                cancellationToken);

    public async Task MarkProcessedAsync(Guid messageId, string consumer, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<ProcessedMessageEntity>().Add(new ProcessedMessageEntity
        {
            MessageId = messageId.ToString("N"),
            Consumer = consumer,
            ProcessedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
