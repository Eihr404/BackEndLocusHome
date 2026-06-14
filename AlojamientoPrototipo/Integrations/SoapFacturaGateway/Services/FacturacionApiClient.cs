using System.Net.Http.Json;
using SoapFacturaGateway.Models;

namespace SoapFacturaGateway.Services;

public sealed class FacturacionApiClient
{
    private readonly HttpClient _httpClient;

    public FacturacionApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<FacturaApiResponse> GetFacturaByIdAsync(int facturaId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetFromJsonAsync<FacturaApiResponse>($"/api/v1/facturas/{facturaId}", cancellationToken);
        return response ?? throw new InvalidOperationException("No se pudo recuperar la factura para SOAP.");
    }
}
