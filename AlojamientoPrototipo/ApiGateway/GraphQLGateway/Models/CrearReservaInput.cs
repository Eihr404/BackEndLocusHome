namespace GraphQLGateway.Models;

public sealed record CrearReservaInput(
    int ClienteId,
    int AlojamientoId,
    DateOnly FechaCheckIn,
    DateOnly FechaCheckOut,
    int NumAdultos,
    int NumNinos,
    bool LlevaMascotas,
    string? CodigoDescuento,
    string? IdempotencyKey,
    List<DetalleHabitacionInput> Habitaciones);
