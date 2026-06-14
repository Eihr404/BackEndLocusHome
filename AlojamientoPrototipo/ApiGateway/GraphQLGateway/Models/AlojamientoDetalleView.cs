namespace GraphQLGateway.Models;

public sealed record AlojamientoDetalleView(
    Alojamiento Alojamiento,
    IReadOnlyList<Habitacion> Habitaciones,
    IReadOnlyList<Foto> Fotos,
    decimal? PrecioNocheMinimo,
    decimal? PrecioNocheMaximo);
