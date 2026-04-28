using Microservicio.Clientes.Business.DTOs.Maestros;

namespace Microservicio.Clientes.Business.Interfaces;

public interface IMaestrosService
{
    Task<IEnumerable<CiudadDto>> ObtenerCiudadesAsync();
    Task<IEnumerable<MonedaDto>> ObtenerMonedasAsync();
    Task<IEnumerable<TipoAlojamientoDto>> ObtenerTiposAlojamientoAsync();
    Task<IEnumerable<InstalacionDto>> ObtenerInstalacionesAsync();
}
