using Microservicio.Clientes.Business.DTOs.Habitaciones;

namespace Microservicio.Clientes.Business.Interfaces;

public interface IHabitacionesService
{
    Task<IEnumerable<HabitacionDto>> ObtenerPorPropiedadAsync(int propiedadId);
    Task<HabitacionDto> ObtenerPorIdAsync(int id);
    Task<HabitacionDto> CrearAsync(CrearHabitacionDto dto);
    Task<HabitacionDto> ActualizarAsync(int id, ActualizarHabitacionDto dto);
    Task EliminarAsync(int id);
}
