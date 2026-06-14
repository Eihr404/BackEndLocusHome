namespace GraphQLGateway.Models;

public sealed record Foto(
    int FotoId,
    int AlojamientoId,
    string Url,
    int Orden,
    string? Descripcion);
