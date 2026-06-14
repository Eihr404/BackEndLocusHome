using GraphQLGateway.Clients;
using GraphQLGateway.Models;

namespace GraphQLGateway.GraphQL;

public sealed class Mutation
{
    public Task<LoginSession> LoginAsync(
        LoginInput input,
        [Service] UsuariosClient client,
        CancellationToken cancellationToken)
        => client.LoginAsync(input, cancellationToken);

    public Task<OperationResult> RegistrarClienteAsync(
        RegistrarClienteInput input,
        [Service] UsuariosClient client,
        CancellationToken cancellationToken)
        => client.RegistrarClienteAsync(input, cancellationToken);

    public Task<Cliente> AsegurarPerfilClienteAsync(
        AsegurarPerfilClienteInput input,
        [Service] UsuariosClient client,
        CancellationToken cancellationToken)
        => client.AsegurarPerfilAsync(input, cancellationToken);

    public Task<Reserva> CrearReservaAsync(
        CrearReservaInput input,
        [Service] ReservasClient client,
        CancellationToken cancellationToken)
        => client.CrearReservaAsync(input, cancellationToken);

    public Task<Factura> CrearFacturaAsync(
        CrearFacturaInput input,
        [Service] FacturacionClient client,
        CancellationToken cancellationToken)
        => client.CrearFacturaAsync(input, cancellationToken);

    public Task<OperationResult> AprobarFacturaAsync(
        int facturaId,
        [Service] FacturacionClient client,
        CancellationToken cancellationToken)
        => client.AprobarFacturaAsync(facturaId, cancellationToken);

    public Task<OperationResult> RechazarFacturaAsync(
        int facturaId,
        [Service] FacturacionClient client,
        CancellationToken cancellationToken)
        => client.RechazarFacturaAsync(facturaId, cancellationToken);

}
