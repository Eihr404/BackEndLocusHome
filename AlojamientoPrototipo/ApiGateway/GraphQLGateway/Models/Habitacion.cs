namespace GraphQLGateway.Models;

public sealed record Habitacion(
    int HabitacionId,
    int AlojamientoId,
    string Nombre,
    string? Descripcion,
    int CapacidadAdultos,
    int CapacidadNinos,
    int NumBanos,
    int NumDormitorios,
    bool TieneCocina,
    bool TieneAireAcondicionado,
    decimal? SuperficieM2,
    decimal PrecioNoche);
