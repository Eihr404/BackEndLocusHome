namespace GraphQLGateway.Models;

public sealed record ReservaMarketplace(
    Reserva Reserva,
    Alojamiento? Alojamiento,
    Factura? Factura);
