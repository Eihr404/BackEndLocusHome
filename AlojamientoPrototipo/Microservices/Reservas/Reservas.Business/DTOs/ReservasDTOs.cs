using System.ComponentModel.DataAnnotations;

namespace Reservas.Business.DTOs;

public record DetalleHabitacionRequest(
    [Required] int HabitacionId,
    [Required] [Range(1, 10000)] decimal PrecioPorNoche,
    [Required] [Range(1, 365)] int NumNoches
);

public record CrearReservaRequest(
    [Required] int ClienteId,
    [Required] int AlojamientoId,
    [Required] DateOnly FechaCheckIn,
    [Required] DateOnly FechaCheckOut,
    int NumAdultos = 1,
    int NumNinos = 0,
    bool LlevaMascotas = false,
    string? CodigoDescuento = null,
    [Required] List<DetalleHabitacionRequest> Habitaciones = null!
);

public record ActualizarEstadoReservaRequest(
    [Required] [MaxLength(30)] string Estado
);

public record DetalleHabitacionResponse(
    int DetalleId,
    int HabitacionId,
    decimal PrecioPorNoche,
    int NumNoches,
    decimal SubTotalHabitacion
);

public record ReservaResponse(
    int ReservaId,
    int ClienteId,
    int AlojamientoId,
    DateOnly FechaCheckIn,
    DateOnly FechaCheckOut,
    int NumAdultos,
    int NumNinos,
    bool LlevaMascotas,
    int NumHabitaciones,
    decimal SubTotal,
    decimal Total,
    string Estado,
    string CodigoReserva,
    DateTime FechaCreacion,
    List<DetalleHabitacionResponse> DetallesHabitacion,
    string? CodigoDescuentoAplicado,
    decimal? PorcentajeDescuento
);
