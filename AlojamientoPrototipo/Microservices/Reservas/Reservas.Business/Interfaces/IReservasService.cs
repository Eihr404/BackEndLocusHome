using Reservas.Business.DTOs;

namespace Reservas.Business.Interfaces;

public interface IReservasService
{
    Task<ReservaResponse> GetByIdAsync(int id);
    Task<IEnumerable<ReservaResponse>> GetByClienteIdAsync(int clienteId);
    Task<IEnumerable<ReservaResumenResponse>> GetResumenByClienteIdAsync(int clienteId);
    Task<IEnumerable<ReservaResponse>> GetByAlojamientoIdAsync(int alojamientoId);
    Task<IEnumerable<ReservaResumenResponse>> GetResumenByAlojamientoIdAsync(int alojamientoId);
    Task<ReservaResponse> CrearAsync(CrearReservaRequest request);
    Task ActualizarEstadoAsync(int id, ActualizarEstadoReservaRequest request);
    Task<bool> ConfirmarReservaPorPagoAsync(int reservaId);
    Task<bool> CancelarReservaPorPagoRechazadoAsync(int reservaId, string motivo);
}
