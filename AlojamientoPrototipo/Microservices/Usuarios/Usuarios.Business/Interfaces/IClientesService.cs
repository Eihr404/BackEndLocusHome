using Usuarios.Business.DTOs.Clientes;

namespace Usuarios.Business.Interfaces;

public interface IClientesService
{
    Task<IEnumerable<ClienteResponse>> GetAllAsync(int page, int size, string? nombre);
    Task<ClienteResponse?> GetByIdAsync(int clienteId);
    Task<ClienteResponse?> GetByCedulaAsync(string cedula);
    Task<ClienteResponse> AsegurarPerfilClienteAsync(AsegurarPerfilClienteRequest request);
    Task RegistrarClienteAsync(RegistrarClienteRequest request);
    Task ActualizarClienteAsync(int clienteId, ActualizarClienteRequest request);
    Task CambiarEstadoAsync(int clienteId, CambiarEstadoRequest request);
    Task EliminarClienteAsync(int clienteId);
}
