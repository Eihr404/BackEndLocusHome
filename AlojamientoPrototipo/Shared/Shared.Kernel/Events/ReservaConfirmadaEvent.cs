namespace Shared.Kernel.Events;

public record ReservaConfirmadaEvent : IntegrationEvent
{
    public int ReservaId { get; init; }

    public int FacturaId { get; init; }
}
