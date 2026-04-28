using Microservicio.Cliente.DatAccess.Entities.Core;
using Microservicio.Cliente.DatAccess.Entities.Reservas;
using Microservicio.Clientes.DataManagement.Models;

namespace Microservicio.Clientes.DataManagement.Mappers;

/// <summary>
/// Convierte ReservaEntity (BD) → ReservaDataModel (Capa 2).
/// </summary>
public static class ReservaDataMapper
{
    public static ReservaDataModel ToDataModel(ReservaEntity entity)
    {
        return new ReservaDataModel
        {
            ReservaId      = entity.ReservaId,
            CodigoReserva  = entity.CodigoReserva,
            ClienteId      = entity.ClienteId,
            PropiedadId    = entity.PropiedadId,
            FechaCheckIn   = entity.FechaCheckIn,
            FechaCheckOut  = entity.FechaCheckOut,
            NumAdultos     = entity.NumAdultos,
            NumNinos       = entity.NumNinos,
            LlevaMascotas  = entity.LlevaMascotas,
            Total          = entity.Total,
            Estado         = entity.Estado
        };
    }

    // Construye una nueva ReservaEntity desde el modelo de creación
    public static ReservaEntity ToEntity(CrearReservaDataModel modelo)
    {
        return new ReservaEntity
        {
            ClienteId     = modelo.ClienteId,
            PropiedadId   = modelo.PropiedadId,
            FechaCheckIn  = modelo.FechaCheckIn,
            FechaCheckOut = modelo.FechaCheckOut,
            NumAdultos    = modelo.NumAdultos,
            NumNinos      = modelo.NumNinos,
            LlevaMascotas = modelo.LlevaMascotas,
            MonedaId      = modelo.MonedaId,
            NumHabitaciones = modelo.HabitacionIds.Count,
            Estado        = "Pendiente",
            CodigoReserva = $"BK-{DateTime.UtcNow:yyyyMMddHHmmss}-{modelo.ClienteId}",
            FechaCreacion = DateTime.UtcNow,
            EliminadoLogico = false
        };
    }
}
