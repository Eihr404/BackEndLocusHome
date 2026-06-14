namespace GraphQLGateway.Models;

public sealed record ReservaResumen(
    int ReservaId,
    int ClienteId,
    int AlojamientoId,
    DateOnly FechaCheckIn,
    DateOnly FechaCheckOut,
    int NumHabitaciones,
    decimal Total,
    string Estado,
    string CodigoReserva,
    DateTime FechaCreacion);
