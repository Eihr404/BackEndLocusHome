namespace GraphQLGateway.Models;

public sealed record CalendarioDisponibilidad
{
    public int CalendarioId { get; init; }

    public int HabitacionId { get; init; }

    public DateOnly Fecha { get; init; }

    public string Estado { get; init; } = string.Empty;
}
