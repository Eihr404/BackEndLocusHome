using System.Net.Http.Json;
using GraphQLGateway.Models;

namespace GraphQLGateway.Clients;

public sealed class ReservasClient(HttpClient httpClient)
{
    public Task<Reserva?> GetReservaByIdAsync(int id, CancellationToken cancellationToken)
        => httpClient.GetFromJsonAsync<Reserva>($"/api/v2/reservas/{id}", cancellationToken);

    public async Task<IReadOnlyList<ReservaResumen>> GetReservasResumenByClienteAsync(int clienteId, CancellationToken cancellationToken)
    {
        var result = await httpClient.GetFromJsonAsync<List<ReservaResumen>>(
            $"/api/v2/reservas/resumen/cliente/{clienteId}",
            cancellationToken);

        return result ?? [];
    }

    public async Task<IReadOnlyList<ReservaResumen>> GetReservasResumenByAlojamientoAsync(int alojamientoId, CancellationToken cancellationToken)
    {
        var result = await httpClient.GetFromJsonAsync<List<ReservaResumen>>(
            $"/api/v2/reservas/resumen/alojamiento/{alojamientoId}",
            cancellationToken);

        return result ?? [];
    }

    public async Task<Reserva> CrearReservaAsync(CrearReservaInput input, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "/api/v2/reservas",
            input,
            cancellationToken);

        await GraphQlHttpErrorHelper.EnsureSuccessAsync(
            response,
            "Reservas.API",
            "Crear reserva",
            cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<Reserva>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("No se recibio reserva creada desde Reservas API.");
    }

}
