using GraphQLGateway.Clients;
using GraphQLGateway.Models;
using HotChocolate.Types;

namespace GraphQLGateway.GraphQL;

[ExtendObjectType<Reserva>]
public sealed class ReservaType
{
    public Task<Alojamiento?> GetAlojamientoAsync(
        [Parent] Reserva reserva,
        [Service] AlojamientosClient client,
        CancellationToken cancellationToken)
        => client.GetAlojamientoByIdAsync(reserva.AlojamientoId, cancellationToken);

    public Task<Factura?> GetFacturaAsync(
        [Parent] Reserva reserva,
        [Service] FacturacionClient client,
        CancellationToken cancellationToken)
        => client.GetFacturaByReservaIdAsync(reserva.ReservaId, cancellationToken);
}
