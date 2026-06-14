namespace GraphQLGateway.Models;

public sealed record MetodoPago
{
    public int MetodoPagoId { get; init; }

    public string Tipo { get; init; } = string.Empty;
}
