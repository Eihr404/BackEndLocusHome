namespace Reservas.Business.Interfaces;

public interface ICalendarioGateway
{
    Task VerificarDisponibilidadAsync(int habitacionId, DateOnly fechaInicio, DateOnly fechaFin);
    Task BloquearFechasAsync(int habitacionId, DateOnly fechaInicio, DateOnly fechaFin);
    Task LiberarFechasAsync(int habitacionId, DateOnly fechaInicio, DateOnly fechaFin);
}
