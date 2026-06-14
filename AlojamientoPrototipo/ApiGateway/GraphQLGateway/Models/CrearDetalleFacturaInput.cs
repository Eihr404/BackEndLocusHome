namespace GraphQLGateway.Models;

public sealed record CrearDetalleFacturaInput
{
    public string Descripcion { get; init; } = string.Empty;

    public int Cantidad { get; init; }

    public decimal PrecioUnitario { get; init; }
}
