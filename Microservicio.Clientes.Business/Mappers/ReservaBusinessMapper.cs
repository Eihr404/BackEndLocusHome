using Microservicio.Clientes.Business.DTOs.Reservas;
using Microservicio.Clientes.DataManagement.Models;

namespace Microservicio.Clientes.Business.Mappers;

public static class ReservaBusinessMapper
{
    public static CrearReservaDataModel ToDataModel(CrearReservaRequest req) => new()
    {
        ClienteId     = req.ClienteId,
        PropiedadId   = req.PropiedadId,
        HabitacionIds = req.HabitacionIds,
        FechaCheckIn  = req.FechaCheckIn,
        FechaCheckOut = req.FechaCheckOut,
        NumAdultos    = req.NumAdultos,
        NumNinos      = req.NumNinos,
        LlevaMascotas = req.LlevaMascotas,
        MonedaId      = req.MonedaId,
        MetodoPagoId  = req.MetodoPagoId
    };

    public static ReservaResponse ToResponse(ReservaDataModel m) => new()
    {
        ReservaId     = m.ReservaId,
        CodigoReserva = m.CodigoReserva,
        ClienteId     = m.ClienteId,
        PropiedadId   = m.PropiedadId,
        FechaCheckIn  = m.FechaCheckIn,
        FechaCheckOut = m.FechaCheckOut,
        NumAdultos    = m.NumAdultos,
        NumNinos      = m.NumNinos,
        LlevaMascotas = m.LlevaMascotas,
        Total         = m.Total,
        Estado        = m.Estado
    };

    public static ReservaResumenResponse ToResumen(ReservaDataModel m) => new()
    {
        ReservaId     = m.ReservaId,
        CodigoReserva = m.CodigoReserva,
        FechaCheckIn  = m.FechaCheckIn,
        FechaCheckOut = m.FechaCheckOut,
        Total         = m.Total,
        Estado        = m.Estado
    };

    public static ReservaConfirmadaResponse ToConfirmacion(ReservaDataModel m) => new()
    {
        CodigoReserva = m.CodigoReserva,
        FechaCheckIn  = m.FechaCheckIn,
        FechaCheckOut = m.FechaCheckOut,
        Total         = m.Total,
        Mensaje       = $"Reserva {m.CodigoReserva} creada exitosamente."
    };
}
