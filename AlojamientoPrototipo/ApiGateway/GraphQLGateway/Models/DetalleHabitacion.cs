namespace GraphQLGateway.Models;

public sealed record DetalleHabitacion(
    int DetalleId,
    int HabitacionId,
    decimal PrecioPorNoche,
    int NumNoches,
    decimal SubTotalHabitacion);
