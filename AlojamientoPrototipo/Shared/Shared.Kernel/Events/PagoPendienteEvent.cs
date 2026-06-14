namespace Shared.Kernel.Events;

public record PagoPendienteEvent : IntegrationEvent
{
    public int ReservaId { get; init; }

    public int FacturaId { get; init; }

    public decimal Monto { get; init; }
}
