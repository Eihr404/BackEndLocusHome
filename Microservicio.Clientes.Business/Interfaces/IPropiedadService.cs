using Microservicio.Clientes.Business.DTOs.Propiedades;
using Microservicio.Clientes.Business.DTOs.Shared;

namespace Microservicio.Clientes.Business.Interfaces;

public interface IPropiedadService
{
    Task<PropiedadResponse> ObtenerPorIdAsync(int propiedadId);
    Task<PagedResponse<PropiedadTarjetaResponse>> BuscarAsync(BuscarPropiedadRequest request);
    Task<PropiedadResponse> CrearAsync(CrearPropiedadRequest request);
    Task<IReadOnlyCollection<PropiedadResponse>> ObtenerPorColaboradorAsync(int colaboradorId);
    Task CambiarEstadoAsync(CambiarEstadoPropiedadRequest request);
}
