namespace GraphQLGateway.Models;

public sealed record Cliente
{
    public int ClienteId { get; init; }

    public int? UsuarioId { get; init; }

    public string Cedula { get; init; } = string.Empty;

    public string? FotoUrl { get; init; }

    public string? Telefono { get; init; }

    public string? Domicilio { get; init; }

    public string Email { get; init; } = string.Empty;

    public int TotalReservas { get; init; }

    public DateTime FechaCreacion { get; init; }
}
