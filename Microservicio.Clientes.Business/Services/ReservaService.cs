using Microservicio.Clientes.Business.DTOs.Reservas;
using Microservicio.Clientes.Business.DTOs.Propiedades;
using Microservicio.Clientes.Business.Exceptions;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.Business.Mappers;
using Microservicio.Clientes.Business.Validators;
using Microservicio.Clientes.DataManagement.Interfaces;

namespace Microservicio.Clientes.Business.Services;

public class ReservaService : IReservaService
{
    private readonly IReservaDataService _dataService;
    private readonly IPropiedadService _propiedadService;

    public ReservaService(IReservaDataService dataService, IPropiedadService propiedadService)
    {
        _dataService = dataService;
        _propiedadService = propiedadService;
    }

    public async Task<ReservaResponse?> ObtenerPorCodigoAsync(string codigo)
    {
        var model = await _dataService.ObtenerPorCodigoAsync(codigo);
        return model == null ? null : ReservaBusinessMapper.ToResponse(model);
    }

    public async Task<IReadOnlyCollection<ReservaResumenResponse>> ObtenerTodasAsync()
    {
        var lista = await _dataService.ObtenerTodasAsync();
        return lista.Select(ReservaBusinessMapper.ToResumen).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<ReservaResumenResponse>> ObtenerPorClienteAsync(int clienteId)
    {
        var lista = await _dataService.ObtenerPorClienteAsync(clienteId);
        return lista.Select(ReservaBusinessMapper.ToResumen).ToList().AsReadOnly();
    }

    public async Task<ReservaConfirmadaResponse> CrearAsync(CrearReservaRequest request)
    {
        ReservaValidator.Validar(request);

        // 1. Validar disponibilidad directamente via IPropiedadService (sin red)
        var filtro = new BuscarPropiedadRequest
        {
            NumAdultos    = request.NumAdultos,
            NumNinos      = request.NumNinos,
            FechaCheckIn  = request.FechaCheckIn,
            FechaCheckOut = request.FechaCheckOut,
            PageSize      = 1000
        };
        var paged = await _propiedadService.BuscarAsync(filtro);
        var disponible = paged.Items.Any(p => p.PropiedadId == request.PropiedadId);
        if (!disponible)
        {
            throw new BusinessException("La propiedad no está disponible para las fechas seleccionadas.");
        }

        // 2. Crear reserva en la BD
        var dataModel = ReservaBusinessMapper.ToDataModel(request);
        var creada = await _dataService.CrearReservaAsync(dataModel);
        return ReservaBusinessMapper.ToConfirmacion(creada);
    }

    public async Task CambiarEstadoAsync(CambiarEstadoReservaRequest request)
    {
        var estados = new[] { "Pendiente", "Confirmada", "Cancelada", "Completada", "NoShow" };
        if (!estados.Contains(request.NuevoEstado))
            throw new ValidationException([$"Estado inválido. Use: {string.Join(", ", estados)}"]);
        await _dataService.CambiarEstadoAsync(request.ReservaId, request.NuevoEstado);
    }

    public async Task CancelarAsync(CancelarReservaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
            throw new ValidationException(["El motivo de cancelación es obligatorio."]);
        await _dataService.CambiarEstadoAsync(request.ReservaId, "Cancelada");
    }
}
