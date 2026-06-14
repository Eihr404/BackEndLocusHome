namespace Shared.Kernel.Events;

public record ReservaCreadaEvent : IntegrationEvent
{
    public int ReservaId { get; init; }

    public int ClienteId { get; init; }

    public int AlojamientoId { get; init; }

    public string Estado { get; init; } = string.Empty;

    public List<int> HabitacionIds { get; init; } = [];
}
