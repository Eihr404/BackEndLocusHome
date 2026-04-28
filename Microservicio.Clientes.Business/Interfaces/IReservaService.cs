using Microservicio.Clientes.Business.DTOs.Reservas;

namespace Microservicio.Clientes.Business.Interfaces;

public interface IReservaService
{
    Task<ReservaResponse?> ObtenerPorCodigoAsync(string codigo);
    Task<IReadOnlyCollection<ReservaResumenResponse>> ObtenerTodasAsync();
    Task<IReadOnlyCollection<ReservaResumenResponse>> ObtenerPorClienteAsync(int clienteId);
    Task<ReservaConfirmadaResponse> CrearAsync(CrearReservaRequest request);
    Task CambiarEstadoAsync(CambiarEstadoReservaRequest request);
    Task CancelarAsync(CancelarReservaRequest request);
}
