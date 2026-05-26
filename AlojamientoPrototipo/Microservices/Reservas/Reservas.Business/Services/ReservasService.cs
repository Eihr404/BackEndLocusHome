using Reservas.Business.DTOs;
using Reservas.Business.Exceptions;
using Reservas.Business.Interfaces;
using Reservas.Business.Mappers;
using Reservas.DataManagement.Interfaces;
using Reservas.DataManagement.Models;

namespace Reservas.Business.Services;

public class ReservasService : IReservasService
{
    private readonly IReservasDataService _reservasDataService;
    private readonly IDescuentosDataService _descuentosDataService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalendarioGateway _calendarioGateway;

    public ReservasService(
        IReservasDataService reservasDataService,
        IDescuentosDataService descuentosDataService,
        IUnitOfWork unitOfWork,
        ICalendarioGateway calendarioGateway)
    {
        _reservasDataService = reservasDataService;
        _descuentosDataService = descuentosDataService;
        _unitOfWork = unitOfWork;
        _calendarioGateway = calendarioGateway;
    }

    public async Task<ReservaResponse> GetByIdAsync(int id)
    {
        var reserva = await _reservasDataService.GetByIdAsync(id);
        if (reserva == null) throw new ReservaNotFoundException(id);
        return ReservasBusinessMapper.ToResponse(reserva);
    }

    public async Task<IEnumerable<ReservaResponse>> GetByClienteIdAsync(int clienteId)
    {
        var reservas = await _reservasDataService.GetByClienteIdAsync(clienteId);
        return reservas.Select(ReservasBusinessMapper.ToResponse);
    }

    public async Task<IEnumerable<ReservaResumenResponse>> GetResumenByClienteIdAsync(int clienteId)
    {
        var reservas = await _reservasDataService.GetByClienteIdAsync(clienteId);
        return reservas.Select(ReservasBusinessMapper.ToResumenResponse);
    }

    public async Task<IEnumerable<ReservaResponse>> GetByAlojamientoIdAsync(int alojamientoId)
    {
        var reservas = await _reservasDataService.GetByAlojamientoIdAsync(alojamientoId);
        return reservas.Select(ReservasBusinessMapper.ToResponse);
    }

    public async Task<IEnumerable<ReservaResumenResponse>> GetResumenByAlojamientoIdAsync(int alojamientoId)
    {
        var reservas = await _reservasDataService.GetByAlojamientoIdAsync(alojamientoId);
        return reservas.Select(ReservasBusinessMapper.ToResumenResponse);
    }

    public async Task<ReservaResponse> CrearAsync(CrearReservaRequest request)
    {
        if (request.FechaCheckOut <= request.FechaCheckIn)
            throw new FechasInvalidasException("La fecha de CheckOut debe ser posterior al CheckIn.");

        DescuentoDataModel? descuento = null;
        if (!string.IsNullOrEmpty(request.CodigoDescuento))
        {
            descuento = await _descuentosDataService.GetByCodigoAsync(request.CodigoDescuento);
            if (descuento == null || !descuento.Activo)
                throw new DescuentoInvalidoException(request.CodigoDescuento);
        }

        foreach (var habitacionRequest in request.Habitaciones)
        {
            await _calendarioGateway.VerificarDisponibilidadAsync(
                habitacionRequest.HabitacionId,
                request.FechaCheckIn,
                request.FechaCheckOut);
        }

        var detalles = new List<ReservaDetalleHabitacionDataModel>();
        decimal subTotal = 0;

        foreach (var habitacionRequest in request.Habitaciones)
        {
            var subTotalHabitacion = habitacionRequest.PrecioPorNoche * habitacionRequest.NumNoches;

            detalles.Add(new ReservaDetalleHabitacionDataModel
            {
                HabitacionId = habitacionRequest.HabitacionId,
                PrecioPorNoche = habitacionRequest.PrecioPorNoche,
                NumNoches = habitacionRequest.NumNoches,
                SubTotalHabitacion = subTotalHabitacion
            });

            subTotal += subTotalHabitacion;
        }

        decimal total = subTotal;
        if (descuento != null)
        {
            var montoDescuento = subTotal * (descuento.Porcentaje / 100m);
            total -= montoDescuento;
        }

        var model = new ReservaDataModel
        {
            ClienteId = request.ClienteId,
            AlojamientoId = request.AlojamientoId,
            FechaCheckIn = request.FechaCheckIn,
            FechaCheckOut = request.FechaCheckOut,
            NumAdultos = request.NumAdultos,
            NumNinos = request.NumNinos,
            LlevaMascotas = request.LlevaMascotas,
            NumHabitaciones = request.Habitaciones.Count,
            DescuentoId = descuento?.DescuentoId,
            SubTotal = subTotal,
            Total = total,
            Estado = "Pendiente",
            CodigoReserva = $"RES-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
            DetallesHabitacion = detalles
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var created = await _reservasDataService.CreateAsync(model);

            foreach (var habitacionRequest in request.Habitaciones)
            {
                await _calendarioGateway.BloquearFechasAsync(
                    habitacionRequest.HabitacionId,
                    request.FechaCheckIn,
                    request.FechaCheckOut);
            }

            await _unitOfWork.CommitTransactionAsync();

            return await GetByIdAsync(created.ReservaId);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task ActualizarEstadoAsync(int id, ActualizarEstadoReservaRequest request)
    {
        await _reservasDataService.UpdateStatusAsync(id, request.Estado);
    }
}
