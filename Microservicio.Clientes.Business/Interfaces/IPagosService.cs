using Microservicio.Clientes.Business.DTOs.Pagos;

namespace Microservicio.Clientes.Business.Interfaces;

public interface IPagosService
{
    Task<IEnumerable<PagoDto>> ObtenerPagosPorReservaAsync(int reservaId);
    Task<PagoDto> ProcesarPagoAsync(ProcesarPagoDto dto);
}
