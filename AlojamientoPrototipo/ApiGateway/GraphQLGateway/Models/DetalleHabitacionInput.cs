namespace GraphQLGateway.Models;

public sealed record DetalleHabitacionInput(
    int HabitacionId,
    decimal PrecioPorNoche,
    int NumNoches);
