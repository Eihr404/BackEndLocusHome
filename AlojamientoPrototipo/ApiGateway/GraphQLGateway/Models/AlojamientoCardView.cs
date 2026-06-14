namespace GraphQLGateway.Models;

public sealed record AlojamientoCardView(
    int AlojamientoId,
    int? SocioId,
    int? TipoAlojamientoId,
    string Nombre,
    string TipoAlojamiento,
    string Ciudad,
    string Direccion,
    decimal PrecioNocheMinimo,
    string Moneda,
    int Estrellas,
    string? ImagenUrl,
    bool AdmiteMascotas,
    bool TienePiscina,
    bool TieneParqueadero,
    bool Disponible,
    string? Descripcion,
    string Estado);
