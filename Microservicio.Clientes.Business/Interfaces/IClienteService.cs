using Microservicio.Clientes.Business.DTOs.Clientes;
using Microservicio.Clientes.Business.DTOs.Shared;

namespace Microservicio.Clientes.Business.Interfaces;

/// <summary>Contrato del servicio de Clientes para la capa API.</summary>
public interface IClienteService
{
    Task<ClienteResponse> ObtenerPorIdAsync(int clienteId);
    Task<PagedResponse<ClienteResumenResponse>> BuscarAsync(BuscarClienteRequest request);
    Task<ClienteResponse> CrearAsync(CrearClienteRequest request);
    Task<ClienteResponse> ActualizarAsync(ActualizarClienteRequest request);
    Task EliminarAsync(int clienteId);
}
