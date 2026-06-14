namespace GraphQLGateway.Models;

public sealed record ClienteReservasView(
    int ClienteId,
    IReadOnlyList<ReservaClienteView> Reservas);
