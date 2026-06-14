namespace GraphQLGateway.Models;

public sealed record CrearFacturaInput
{
    public int ReservaId { get; init; }

    public int? MetodoPagoId { get; init; }

    public decimal Monto { get; init; }

    public DateTime? FechaPago { get; init; }

    public string? IdempotencyKey { get; init; }

    public List<CrearDetalleFacturaInput> Detalles { get; init; } = [];
}
