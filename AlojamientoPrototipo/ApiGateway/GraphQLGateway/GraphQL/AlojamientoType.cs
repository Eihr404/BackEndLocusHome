using GraphQLGateway.Clients;
using GraphQLGateway.Models;
using HotChocolate.Types;

namespace GraphQLGateway.GraphQL;

[ExtendObjectType<Alojamiento>]
public sealed class AlojamientoType
{
    public Task<IReadOnlyList<Habitacion>> GetHabitacionesAsync(
        [Parent] Alojamiento alojamiento,
        [Service] AlojamientosClient client,
        CancellationToken cancellationToken)
        => client.GetHabitacionesByAlojamientoIdAsync(alojamiento.AlojamientoId, cancellationToken);

    public Task<IReadOnlyList<Foto>> GetFotosAsync(
        [Parent] Alojamiento alojamiento,
        [Service] AlojamientosClient client,
        CancellationToken cancellationToken)
        => client.GetFotosByAlojamientoIdAsync(alojamiento.AlojamientoId, cancellationToken);

    public Task<IReadOnlyList<ReservaResumen>> GetReservasAsync(
        [Parent] Alojamiento alojamiento,
        [Service] ReservasClient client,
        CancellationToken cancellationToken)
        => client.GetReservasResumenByAlojamientoAsync(alojamiento.AlojamientoId, cancellationToken);
}
