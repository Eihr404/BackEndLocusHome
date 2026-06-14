using GraphQLGateway.Clients;
using GraphQLGateway.Models;

namespace GraphQLGateway.GraphQL;

public sealed class Query
{
    public Task<IReadOnlyList<Alojamiento>> GetAlojamientos(
        [Service] AlojamientosClient client,
        CancellationToken cancellationToken)
        => client.GetAlojamientosAsync(cancellationToken);

    public Task<Alojamiento?> GetAlojamientoById(
        int id,
        [Service] AlojamientosClient client,
        CancellationToken cancellationToken)
        => client.GetAlojamientoByIdAsync(id, cancellationToken);

    public Task<Habitacion?> GetHabitacionById(
        int id,
        [Service] AlojamientosClient client,
        CancellationToken cancellationToken)
        => client.GetHabitacionByIdAsync(id, cancellationToken);

    public Task<Reserva?> GetReservaById(
        int id,
        [Service] ReservasClient client,
        CancellationToken cancellationToken)
        => client.GetReservaByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<ReservaResumen>> GetReservasByCliente(
        int clienteId,
        [Service] ReservasClient client,
        CancellationToken cancellationToken)
        => client.GetReservasResumenByClienteAsync(clienteId, cancellationToken);

    public Task<IReadOnlyList<ReservaResumen>> GetReservasByAlojamiento(
        int alojamientoId,
        [Service] ReservasClient client,
        CancellationToken cancellationToken)
        => client.GetReservasResumenByAlojamientoAsync(alojamientoId, cancellationToken);

    public Task<Factura?> GetFacturaByReservaId(
        int reservaId,
        [Service] FacturacionClient client,
        CancellationToken cancellationToken)
        => client.GetFacturaByReservaIdAsync(reservaId, cancellationToken);

    public async Task<ReservaMarketplace?> GetReservaMarketplaceById(
        int reservaId,
        [Service] ReservasClient reservasClient,
        [Service] AlojamientosClient alojamientosClient,
        [Service] FacturacionClient facturacionClient,
        CancellationToken cancellationToken)
    {
        var reserva = await reservasClient.GetReservaByIdAsync(reservaId, cancellationToken);
        if (reserva is null)
        {
            return null;
        }

        var alojamientoTask = alojamientosClient.GetAlojamientoByIdAsync(reserva.AlojamientoId, cancellationToken);
        var facturaTask = facturacionClient.GetFacturaByReservaIdAsync(reserva.ReservaId, cancellationToken);

        await Task.WhenAll(alojamientoTask, facturaTask);

        return new ReservaMarketplace(
            reserva,
            await alojamientoTask,
            await facturaTask);
    }
}
