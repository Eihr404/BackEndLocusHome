using Grpc.Core;
using Microservicio.Clientes.Api.Protos;
using Microservicio.Clientes.Business.Interfaces;

namespace Microservicio.Clientes.Api.GrpcServices;

public class ClientesGrpcService : ClientesGrpc.ClientesGrpcBase
{
    private readonly IClienteService _clienteService;

    public ClientesGrpcService(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    public override async Task<ClienteResumenReply> ObtenerClienteResumen(ClienteRequest request, ServerCallContext context)
    {
        // En un microservicio real, aquA- se consultarA-a a la BD o Service Layer
        // para traer los datos del cliente solicitado
        var cliente = await _clienteService.ObtenerPorIdAsync(request.ClienteId);

        if (cliente == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Cliente con ID {request.ClienteId} no encontrado"));
        }

        return new ClienteResumenReply
        {
            ClienteId = cliente.ClienteId,
            NombreCompleto = cliente.NombreCompleto ?? string.Empty,
            Email = cliente.Email ?? string.Empty,
            NivelFidelidad = cliente.Calificacion >= 4.5m ? "VIP" : "Normal"
        };
    }
}
