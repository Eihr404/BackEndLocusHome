using Microservicio.Clientes.Business.DTOs.Calificaciones;

namespace Microservicio.Clientes.Business.Interfaces;

public interface ICalificacionesService
{
    Task<IEnumerable<CalificacionHotelDto>> ObtenerPorPropiedadAsync(int propiedadId);
    Task<CalificacionHotelDto> AgregarCalificacionAsync(CrearCalificacionHotelDto dto);
}
