using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Reservas.Business.DTOs;
using Reservas.Business.Exceptions;
using Reservas.Business.Interfaces;
using Reservas.Business.Mappers;
using Reservas.DataManagement.Interfaces;
using Reservas.DataManagement.Models;
using MassTransit;
using Shared.Kernel.Correlation;
using Shared.Kernel.Events;

namespace Reservas.Business.Services;

public class ReservasService : IReservasService
{
    private const string CreateOperationName = "CrearReserva";
    private readonly IReservasDataService _reservasDataService;
    private readonly IIdempotentRequestDataService _idempotentRequestDataService;
    private readonly IDescuentosDataService _descuentosDataService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalendarioGateway _calendarioGateway;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly CorrelationContextAccessor _correlationAccessor;

    public ReservasService(
        IReservasDataService reservasDataService,
        IIdempotentRequestDataService idempotentRequestDataService,
        IDescuentosDataService descuentosDataService,
        IUnitOfWork unitOfWork,
        ICalendarioGateway calendarioGateway,
        IPublishEndpoint publishEndpoint,
        CorrelationContextAccessor correlationAccessor)
    {
        _reservasDataService = reservasDataService;
        _idempotentRequestDataService = idempotentRequestDataService;
        _descuentosDataService = descuentosDataService;
        _unitOfWork = unitOfWork;
        _calendarioGateway = calendarioGateway;
        _publishEndpoint = publishEndpoint;
        _correlationAccessor = correlationAccessor;
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
        var idempotencyKey = NormalizeIdempotencyKey(request.IdempotencyKey);
        var requestHash = ComputeRequestHash(request);

        var idempotentResponse = await TryResolveIdempotentResponseAsync(idempotencyKey, requestHash);
        if (idempotentResponse != null)
        {
            return idempotentResponse;
        }

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
            if (idempotencyKey != null)
            {
                var createdPending = await _idempotentRequestDataService.TryCreatePendingAsync(new IdempotentRequestDataModel
                {
                    IdempotencyKey = idempotencyKey,
                    OperationName = CreateOperationName,
                    RequestHash = requestHash,
                    Status = "Pending"
                });

                if (!createdPending)
                {
                    return await ResolveConcurrentDuplicateAsync(idempotencyKey, requestHash);
                }
            }

            var created = await _reservasDataService.CreateAsync(model);

            foreach (var habitacionRequest in request.Habitaciones)
            {
                await _calendarioGateway.BloquearFechasAsync(
                    habitacionRequest.HabitacionId,
                    request.FechaCheckIn,
                    request.FechaCheckOut);
            }

            if (idempotencyKey != null)
            {
                await _idempotentRequestDataService.MarkCompletedAsync(
                    CreateOperationName,
                    idempotencyKey,
                    created.ReservaId);
            }

            await _unitOfWork.CommitTransactionAsync();

            var createdResponse = await GetByIdAsync(created.ReservaId);

            var reservaCreada = EventFactory.ApplyMetadata(new ReservaCreadaEvent
            {
                ReservaId = created.ReservaId,
                ClienteId = created.ClienteId,
                AlojamientoId = created.AlojamientoId,
                Estado = created.Estado,
                HabitacionIds = created.DetallesHabitacion.Select(x => x.HabitacionId).ToList()
            }, "Reservas.API", _correlationAccessor);

            await _publishEndpoint.Publish(reservaCreada, publishContext =>
            {
                publishContext.Headers.Set(CorrelationConstants.HeaderName, reservaCreada.CorrelationId);
            });

            return createdResponse;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private async Task<ReservaResponse?> TryResolveIdempotentResponseAsync(string? idempotencyKey, string requestHash)
    {
        if (idempotencyKey == null)
        {
            return null;
        }

        var existing = await _idempotentRequestDataService.GetByKeyAsync(CreateOperationName, idempotencyKey);
        if (existing == null)
        {
            return null;
        }

        ValidateRequestHash(existing.RequestHash, requestHash);

        if (string.Equals(existing.Status, "Completed", StringComparison.OrdinalIgnoreCase) && existing.ResourceId.HasValue)
        {
            return await GetByIdAsync(existing.ResourceId.Value);
        }

        throw new DuplicateOperationInProgressException(CreateOperationName);
    }

    private async Task<ReservaResponse> ResolveConcurrentDuplicateAsync(string idempotencyKey, string requestHash)
    {
        var existing = await _idempotentRequestDataService.GetByKeyAsync(CreateOperationName, idempotencyKey)
            ?? throw new DuplicateOperationInProgressException(CreateOperationName);

        ValidateRequestHash(existing.RequestHash, requestHash);

        if (string.Equals(existing.Status, "Completed", StringComparison.OrdinalIgnoreCase) && existing.ResourceId.HasValue)
        {
            return await GetByIdAsync(existing.ResourceId.Value);
        }

        throw new DuplicateOperationInProgressException(CreateOperationName);
    }

    private static void ValidateRequestHash(string existingHash, string incomingHash)
    {
        if (!string.Equals(existingHash, incomingHash, StringComparison.Ordinal))
        {
            throw new IdempotencyKeyReuseException(CreateOperationName);
        }
    }

    private static string? NormalizeIdempotencyKey(string? idempotencyKey)
        => string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();

    private static string ComputeRequestHash(CrearReservaRequest request)
    {
        var canonicalPayload = JsonSerializer.Serialize(new
        {
            request.ClienteId,
            request.AlojamientoId,
            request.FechaCheckIn,
            request.FechaCheckOut,
            request.NumAdultos,
            request.NumNinos,
            request.LlevaMascotas,
            request.CodigoDescuento,
            Habitaciones = request.Habitaciones
                .Select(x => new { x.HabitacionId, x.PrecioPorNoche, x.NumNoches })
                .OrderBy(x => x.HabitacionId)
                .ThenBy(x => x.PrecioPorNoche)
                .ThenBy(x => x.NumNoches)
                .ToList()
        });

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalPayload));
        return Convert.ToHexString(bytes);
    }

    public async Task ActualizarEstadoAsync(int id, ActualizarEstadoReservaRequest request)
    {
        if (request.Estado.StartsWith("Cancelado", StringComparison.OrdinalIgnoreCase))
        {
            var reserva = await _reservasDataService.GetByIdAsync(id);
            if (reserva == null) throw new ReservaNotFoundException(id);

            foreach (var detalle in reserva.DetallesHabitacion)
            {
                await _calendarioGateway.LiberarFechasAsync(
                    detalle.HabitacionId,
                    reserva.FechaCheckIn,
                    reserva.FechaCheckOut);
            }
        }

        await _reservasDataService.UpdateStatusAsync(id, request.Estado);
    }

    public async Task<bool> ConfirmarReservaPorPagoAsync(int reservaId)
    {
        var reserva = await _reservasDataService.GetByIdAsync(reservaId)
            ?? throw new ReservaNotFoundException(reservaId);

        if (!string.Equals(reserva.Estado, "Pendiente", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        await _reservasDataService.UpdateStatusAsync(reservaId, "Confirmada");
        return true;
    }

    public async Task<bool> CancelarReservaPorPagoRechazadoAsync(int reservaId, string motivo)
    {
        var reserva = await _reservasDataService.GetByIdAsync(reservaId);
        if (reserva == null) throw new ReservaNotFoundException(reservaId);

        if (!string.Equals(reserva.Estado, "Pendiente", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        foreach (var detalle in reserva.DetallesHabitacion)
        {
            await _calendarioGateway.LiberarFechasAsync(
                detalle.HabitacionId,
                reserva.FechaCheckIn,
                reserva.FechaCheckOut);
        }

        await _reservasDataService.UpdateStatusAsync(reservaId, "Cancelado");
        return true;
    }
}
