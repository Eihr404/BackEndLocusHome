namespace SoapFacturaGateway.Models;

public sealed class FacturaApiResponse
{
    public int FacturaId { get; init; }
    public int ReservaId { get; init; }
    public string? MetodoPagoTipo { get; init; }
    public decimal Monto { get; init; }
    public string Estado { get; init; } = string.Empty;
    public DateTime? FechaPago { get; init; }
    public List<FacturaDetalleApiResponse> Detalles { get; init; } = [];
}
