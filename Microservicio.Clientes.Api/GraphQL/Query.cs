using Microservicio.Clientes.Business.DTOs.Clientes;
using Microservicio.Clientes.Business.Interfaces;

namespace Microservicio.Clientes.Api.GraphQL;

public class Query
{
    // Resolver para obtener todos los clientes. 
    // GraphQL permite pedir solo los campos que el cliente necesita
    public async Task<IEnumerable<ClienteResumenResponse>> GetClientes([Service] IClienteService clienteService)
    {
        var paged = await clienteService.BuscarAsync(new BuscarClienteRequest());
        return paged.Items;
    }

    // Resolver para obtener propiedades
    public async Task<IEnumerable<Microservicio.Clientes.Business.DTOs.Propiedades.PropiedadTarjetaResponse>> GetPropiedades([Service] IPropiedadService propiedadService)
    {
        var paged = await propiedadService.BuscarAsync(new Microservicio.Clientes.Business.DTOs.Propiedades.BuscarPropiedadRequest());
        return paged.Items;
    }
}
