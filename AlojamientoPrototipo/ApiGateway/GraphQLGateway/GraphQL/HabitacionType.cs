using GraphQLGateway.Clients;
using GraphQLGateway.Models;
using HotChocolate.Types;

namespace GraphQLGateway.GraphQL;

[ExtendObjectType<Habitacion>]
public sealed class HabitacionType
{
    public Task<IReadOnlyList<CalendarioDisponibilidad>> GetDisponibilidadMensualAsync(
        [Parent] Habitacion habitacion,
        int mes,
        int anio,
        [Service] AlojamientosClient client,
        CancellationToken cancellationToken)
        => client.GetDisponibilidadByHabitacionAsync(habitacion.HabitacionId, mes, anio, cancellationToken);
}
