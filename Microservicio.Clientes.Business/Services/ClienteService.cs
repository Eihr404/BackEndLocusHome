using Microservicio.Clientes.Business.DTOs.Clientes;
using Microservicio.Clientes.Business.DTOs.Shared;
using Microservicio.Clientes.Business.Exceptions;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.Business.Mappers;
using Microservicio.Clientes.Business.Validators;
using Microservicio.Clientes.DataManagement.Interfaces;
using Microservicio.Clientes.DataManagement.Models;

namespace Microservicio.Clientes.Business.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteDataService _dataService;

    public ClienteService(IClienteDataService dataService) => _dataService = dataService;

    public async Task<ClienteResponse> ObtenerPorIdAsync(int clienteId)
    {
        var model = await _dataService.ObtenerPorIdAsync(clienteId)
            ?? throw new NotFoundException("Cliente", clienteId);
        return ClienteBusinessMapper.ToResponse(model);
    }

    public async Task<PagedResponse<ClienteResumenResponse>> BuscarAsync(BuscarClienteRequest request)
    {
        var filtro = new ClienteFiltroDataModel
        {
            Nombre             = request.Nombre,
            Email              = request.Email,
            CalificacionMinima = request.CalificacionMinima,
            PageNumber         = request.PageNumber,
            PageSize           = request.PageSize
        };
        var paged = await _dataService.BuscarAsync(filtro);
        return ClienteBusinessMapper.ToPagedResponse(paged);
    }

    public async Task<ClienteResponse> CrearAsync(CrearClienteRequest request)
    {
        ClienteValidator.Validar(request);
        var dataModel = ClienteBusinessMapper.ToDataModel(request);
        var id = await _dataService.CrearAsync(dataModel);
        var creado = await _dataService.ObtenerPorIdAsync(id)
            ?? throw new BusinessException("No se pudo recuperar el cliente recién creado.");
        return ClienteBusinessMapper.ToResponse(creado);
    }

    public async Task<ClienteResponse> ActualizarAsync(ActualizarClienteRequest request)
    {
        ClienteValidator.Validar(request);
        var existente = await _dataService.ObtenerPorIdAsync(request.ClienteId)
            ?? throw new NotFoundException("Cliente", request.ClienteId);
        existente.Telefono  = request.Telefono;
        existente.Domicilio = request.Domicilio;
        existente.FotoUrl   = request.FotoUrl;
        await _dataService.ActualizarAsync(existente);
        return ClienteBusinessMapper.ToResponse(existente);
    }

    public async Task EliminarAsync(int clienteId)
    {
        _ = await _dataService.ObtenerPorIdAsync(clienteId)
            ?? throw new NotFoundException("Cliente", clienteId);
        await _dataService.EliminarAsync(clienteId);
    }
}
