namespace Shared.Kernel.Events;

public record PagoRechazadoEvent : IntegrationEvent
{
    public int ReservaId { get; init; }

    public int FacturaId { get; init; }

    public string Motivo { get; init; } = string.Empty;
}
