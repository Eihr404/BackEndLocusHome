using System.Net.Http.Json;
using GraphQLGateway.Models;

namespace GraphQLGateway.Clients;

public sealed class UsuariosClient(HttpClient httpClient)
{
    public async Task<LoginSession> LoginAsync(LoginInput input, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "/api/v2/auth/login",
            input,
            cancellationToken);

        await GraphQlHttpErrorHelper.EnsureSuccessAsync(
            response,
            "Usuarios.API",
            "Login",
            cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<LoginSession>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("No se recibio sesion desde Usuarios API.");
    }

    public async Task<OperationResult> RegistrarClienteAsync(RegistrarClienteInput input, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "/api/v2/clientes/registrar",
            input,
            cancellationToken);

        await GraphQlHttpErrorHelper.EnsureSuccessAsync(
            response,
            "Usuarios.API",
            "Registrar cliente",
            cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<MessageResponse>(cancellationToken: cancellationToken);
        return new OperationResult(true, result?.Mensaje ?? "Cliente registrado exitosamente");
    }

    public async Task<Cliente> AsegurarPerfilAsync(AsegurarPerfilClienteInput input, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "/api/v2/clientes/asegurar-perfil",
            input,
            cancellationToken);

        await GraphQlHttpErrorHelper.EnsureSuccessAsync(
            response,
            "Usuarios.API",
            "Asegurar perfil",
            cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<Cliente>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("No se recibio perfil de cliente desde Usuarios API.");
    }
}
