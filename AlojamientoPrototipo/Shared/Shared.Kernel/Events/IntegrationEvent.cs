namespace Shared.Kernel.Events;

public abstract record IntegrationEvent
{
    public Guid MessageId { get; init; } = Guid.NewGuid();

    public string CorrelationId { get; init; } = Guid.NewGuid().ToString("N");

    public string EventSource { get; init; } = string.Empty;

    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public string Version { get; init; } = "v1";
}
