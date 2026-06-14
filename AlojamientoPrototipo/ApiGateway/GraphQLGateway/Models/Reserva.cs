namespace GraphQLGateway.Models;

public sealed record Reserva(
    int ReservaId,
    int ClienteId,
    int AlojamientoId,
    DateOnly FechaCheckIn,
    DateOnly FechaCheckOut,
    int NumAdultos,
    int NumNinos,
    bool LlevaMascotas,
    int NumHabitaciones,
    decimal SubTotal,
    decimal Total,
    string Estado,
    string CodigoReserva,
    DateTime FechaCreacion,
    List<DetalleHabitacion> DetallesHabitacion,
    string? CodigoDescuentoAplicado,
    decimal? PorcentajeDescuento);
