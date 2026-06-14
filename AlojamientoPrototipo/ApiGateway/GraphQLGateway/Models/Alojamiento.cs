namespace GraphQLGateway.Models;

public sealed record Alojamiento(
    int AlojamientoId,
    int SocioId,
    int TipoAlojamientoId,
    string TipoAlojamientoNombre,
    string Nombre,
    string? Ciudad,
    string Direccion,
    string? Descripcion,
    int? Estrellas,
    decimal CalificacionPromedio,
    int TotalResenas,
    bool AdmiteMascotas,
    bool TienePiscina,
    bool TieneParqueadero,
    string Estado,
    DateTime FechaCreacion);
