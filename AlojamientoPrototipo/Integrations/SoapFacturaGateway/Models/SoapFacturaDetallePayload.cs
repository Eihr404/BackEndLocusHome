namespace SoapFacturaGateway.Models;

public sealed class SoapFacturaDetallePayload
{
    public string Descripcion { get; init; } = string.Empty;
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
}
