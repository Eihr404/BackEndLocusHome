using Alojamientos.Business.DTOs;

namespace Alojamientos.Business.Interfaces;

public interface ICalendarioService
{
    Task<IEnumerable<CalendarioResponse>> GetDisponibilidadMensualAsync(int habitacionId, int mes, int anio);
    Task<IEnumerable<CalendarioResponse>> BloquearFechasAsync(BloquearFechasRequest request);
    Task<IEnumerable<CalendarioResponse>> LiberarFechasAsync(BloquearFechasRequest request);
    Task<bool> VerificarDisponibilidadAsync(int habitacionId, DateOnly fechaInicio, DateOnly fechaFin);
}
