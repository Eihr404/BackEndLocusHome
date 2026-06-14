using System.Net.Http.Json;
using GraphQLGateway.Models;
using System.Net;

namespace GraphQLGateway.Clients;

public sealed class AlojamientosClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<Alojamiento>> GetAlojamientosAsync(CancellationToken cancellationToken)
    {
        var result = await httpClient.GetFromJsonAsync<List<Alojamiento>>(
            "/api/v2/alojamientos",
            cancellationToken);

        return result ?? [];
    }

    public async Task<Alojamiento?> GetAlojamientoByIdAsync(int id, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"/api/v2/alojamientos/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Alojamiento>(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<TipoAlojamiento>> GetTiposAlojamientoAsync(CancellationToken cancellationToken)
    {
        var result = await httpClient.GetFromJsonAsync<List<TipoAlojamiento>>(
            "/api/v2/alojamientos/tipos",
            cancellationToken);

        return result ?? [];
    }

    public async Task<IReadOnlyList<Habitacion>> GetHabitacionesByAlojamientoIdAsync(int alojamientoId, CancellationToken cancellationToken)
    {
        var result = await httpClient.GetFromJsonAsync<List<Habitacion>>(
            $"/api/v2/alojamientos/{alojamientoId}/habitaciones",
            cancellationToken);

        return result ?? [];
    }

    public async Task<Habitacion?> GetHabitacionByIdAsync(int id, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"/api/v2/habitaciones/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Habitacion>(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<Foto>> GetFotosByAlojamientoIdAsync(int alojamientoId, CancellationToken cancellationToken)
    {
        var result = await httpClient.GetFromJsonAsync<List<Foto>>(
            $"/api/v2/fotos/alojamiento/{alojamientoId}",
            cancellationToken);

        return result ?? [];
    }

    public async Task<IReadOnlyList<CalendarioDisponibilidad>> GetDisponibilidadByHabitacionAsync(
        int habitacionId,
        int mes,
        int anio,
        CancellationToken cancellationToken)
    {
        var result = await httpClient.GetFromJsonAsync<List<CalendarioDisponibilidad>>(
            $"/api/v2/calendario/habitacion/{habitacionId}?mes={mes}&anio={anio}",
            cancellationToken);

        return result ?? [];
    }
}
