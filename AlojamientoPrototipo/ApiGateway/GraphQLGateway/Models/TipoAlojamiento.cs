namespace GraphQLGateway.Models;

public sealed record TipoAlojamiento(
    int TipoAlojamientoId,
    string Nombre,
    string? Descripcion);
