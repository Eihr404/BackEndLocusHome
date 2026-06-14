using GraphQLGateway.Clients;
using GraphQLGateway.Models;
using HotChocolate.Types;

namespace GraphQLGateway.GraphQL;

[ExtendObjectType<ReservaResumen>]
public sealed class ReservaResumenType
{
    public Task<Alojamiento?> GetAlojamientoAsync(
        [Parent] ReservaResumen reserva,
        [Service] AlojamientosClient client,
        CancellationToken cancellationToken)
        => client.GetAlojamientoByIdAsync(reserva.AlojamientoId, cancellationToken);

    public Task<Factura?> GetFacturaAsync(
        [Parent] ReservaResumen reserva,
        [Service] FacturacionClient client,
        CancellationToken cancellationToken)
        => client.GetFacturaByReservaIdAsync(reserva.ReservaId, cancellationToken);
}
