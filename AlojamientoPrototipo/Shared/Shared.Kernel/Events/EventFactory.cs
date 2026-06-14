using Shared.Kernel.Correlation;

namespace Shared.Kernel.Events;

public static class EventFactory
{
    public static T ApplyMetadata<T>(T @event, string eventSource, CorrelationContextAccessor correlationAccessor)
        where T : IntegrationEvent
    {
        return @event with
        {
            MessageId = Guid.NewGuid(),
            CorrelationId = correlationAccessor.CorrelationId ?? Guid.NewGuid().ToString("N"),
            EventSource = eventSource,
            OccurredAt = DateTime.UtcNow,
            Version = "v1"
        };
    }
}
