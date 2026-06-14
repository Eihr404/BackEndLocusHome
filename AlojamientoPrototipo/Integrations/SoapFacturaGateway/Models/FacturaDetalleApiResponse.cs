namespace SoapFacturaGateway.Models;

public sealed class FacturaDetalleApiResponse
{
    public int DetalleFacturaId { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
}
