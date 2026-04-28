using Grpc.Core;
using Microservicio.Clientes.Business.DTOs.Propiedades;
using Microservicio.Clientes.Business.Interfaces;

namespace Microservicio.Clientes.Api.Grpc;

public class InventarioGrpcServiceImpl : InventarioGrpcService.InventarioGrpcServiceBase
{
    private readonly IPropiedadService _propiedadService;

    public InventarioGrpcServiceImpl(IPropiedadService propiedadService)
    {
        _propiedadService = propiedadService;
    }

    public override async Task<VerificarDisponibilidadResponse> ConsultarDisponibilidad(
        VerificarDisponibilidadRequest request, 
        ServerCallContext context)
    {
        // Convertimos el request de gRPC al request de nuestro Dominio
        var filtro = new BuscarPropiedadRequest
        {
            NumAdultos = request.NumAdultos,
            NumNinos = request.NumNinos,
            FechaCheckIn = string.IsNullOrEmpty(request.FechaCheckin) ? null : DateTime.Parse(request.FechaCheckin),
            FechaCheckOut = string.IsNullOrEmpty(request.FechaCheckout) ? null : DateTime.Parse(request.FechaCheckout)
        };

        // En la vida real aquí llamaríamos a un endpoint o método optimizado del _propiedadService
        // Por ahora, reutilizamos la lógica de BuscarAsync para ver si la propiedad viene en los resultados
        var paged = await _propiedadService.BuscarAsync(filtro);
        var existe = paged.Items.Any(p => p.PropiedadId == request.PropiedadId);

        return new VerificarDisponibilidadResponse
        {
            Disponible = existe,
            Mensaje = existe ? "Propiedad disponible para las fechas seleccionadas." : "Propiedad no disponible o sin capacidad suficiente."
        };
    }
}
