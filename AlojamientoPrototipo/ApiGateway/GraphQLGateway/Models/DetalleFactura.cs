namespace GraphQLGateway.Models;

public sealed record DetalleFactura
{
    public int DetalleFacturaId { get; init; }

    public string Descripcion { get; init; } = string.Empty;

    public int Cantidad { get; init; }

    public decimal PrecioUnitario { get; init; }

    public decimal SubTotal { get; init; }
}
