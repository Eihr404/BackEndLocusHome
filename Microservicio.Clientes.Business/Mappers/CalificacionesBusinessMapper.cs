using Microservicio.Cliente.DatAccess.Entities.Calificaciones;
using Microservicio.Clientes.Business.DTOs.Calificaciones;

namespace Microservicio.Clientes.Business.Mappers;

public static class CalificacionesBusinessMapper
{
    public static CalificacionHotelDto ToResponse(CalificacionHotelEntity entity) => new CalificacionHotelDto
    {
        CalificacionId = entity.CalificacionId,
        PropiedadId = entity.PropiedadId,
        ClienteId = entity.ClienteId,
        ReservaId = entity.ReservaId,
        Puntuacion = entity.Puntuacion,
        Comentario = entity.Comentario,
        FechaCreacion = entity.FechaCreacion
    };

    public static CalificacionHotelEntity ToDataModel(CrearCalificacionHotelDto dto) => new CalificacionHotelEntity
    {
        PropiedadId = dto.PropiedadId,
        ClienteId = dto.ClienteId,
        ReservaId = dto.ReservaId,
        Puntuacion = dto.Puntuacion,
        Comentario = dto.Comentario,
        FechaCreacion = DateTime.UtcNow
    };
}
