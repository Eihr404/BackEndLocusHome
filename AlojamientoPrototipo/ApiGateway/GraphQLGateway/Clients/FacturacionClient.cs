using System.Net.Http.Json;
using GraphQLGateway.Models;

namespace GraphQLGateway.Clients;

public sealed class FacturacionClient(HttpClient httpClient)
{
    public Task<Factura?> GetFacturaByReservaIdAsync(int reservaId, CancellationToken cancellationToken)
        => httpClient.GetFromJsonAsync<Factura>($"/api/v2/facturas/reserva/{reservaId}", cancellationToken);

    public async Task<FacturaResumen?> GetFacturaResumenByReservaIdAsync(int reservaId, CancellationToken cancellationToken)
    {
        var result = await httpClient.GetFromJsonAsync<List<FacturaResumen>>(
            $"/api/v2/facturas/resumen/reserva/{reservaId}",
            cancellationToken);

        return result?
            .OrderByDescending(factura => factura.FechaCreacion)
            .FirstOrDefault();
    }

    public async Task<IReadOnlyList<MetodoPago>> GetMetodosPagoAsync(CancellationToken cancellationToken)
    {
        var result = await httpClient.GetFromJsonAsync<List<MetodoPago>>(
            "/api/v2/metodospago",
            cancellationToken);

        return result ?? [];
    }

    public async Task<Factura> CrearFacturaAsync(CrearFacturaInput input, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "/api/v2/facturas",
            input,
            cancellationToken);

        await GraphQlHttpErrorHelper.EnsureSuccessAsync(
            response,
            "Facturacion.API",
            "Crear factura",
            cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<Factura>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("No se recibio factura creada desde Facturacion API.");
    }

    public async Task<OperationResult> AprobarFacturaAsync(int facturaId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PatchAsync(
            $"/api/v2/facturas/{facturaId}/aprobar",
            content: null,
            cancellationToken);

        await GraphQlHttpErrorHelper.EnsureSuccessAsync(
            response,
            "Facturacion.API",
            "Aprobar factura",
            cancellationToken);

        return new OperationResult(true, "Factura pagada y reserva confirmada.");
    }

    public async Task<OperationResult> RechazarFacturaAsync(int facturaId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PatchAsync(
            $"/api/v2/facturas/{facturaId}/rechazar",
            content: null,
            cancellationToken);

        await GraphQlHttpErrorHelper.EnsureSuccessAsync(
            response,
            "Facturacion.API",
            "Rechazar factura",
            cancellationToken);

        return new OperationResult(true, "Factura cancelada y reserva en proceso de compensacion.");
    }
}
