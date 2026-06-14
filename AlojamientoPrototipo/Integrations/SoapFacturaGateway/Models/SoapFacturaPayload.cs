namespace SoapFacturaGateway.Models;

public sealed class SoapFacturaPayload
{
    public int FacturaId { get; init; }
    public int ReservaId { get; init; }
    public decimal Monto { get; init; }
    public string MetodoPago { get; init; } = string.Empty;
    public DateTime? FechaPago { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
    public List<SoapFacturaDetallePayload> Detalles { get; init; } = [];
}
