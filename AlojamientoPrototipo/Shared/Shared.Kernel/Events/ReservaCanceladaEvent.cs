namespace Shared.Kernel.Events;

public record ReservaCanceladaEvent : IntegrationEvent
{
    public int ReservaId { get; init; }

    public string Motivo { get; init; } = string.Empty;
}
