using Grpc.Core;
using Shared.Protos;
using Alojamientos.Business.Interfaces;
using Alojamientos.Business.DTOs;   

namespace Alojamientos.API.GrpcServices;

public class CalendarioGrpcService : CalendarioGrpc.CalendarioGrpcBase
{
    private readonly ICalendarioService _calendarioService;
    private readonly ILogger<CalendarioGrpcService> _logger;

    public CalendarioGrpcService(ICalendarioService calendarioService, ILogger<CalendarioGrpcService> logger)
    {
        _calendarioService = calendarioService;
        _logger = logger;
    }

    public override async Task<DisponibilidadResponse> VerificarDisponibilidad(DisponibilidadRequest request, ServerCallContext context)
    {
        try
        {
            if (!DateOnly.TryParse(request.FechaInicio, out var fechaInicio) ||
                !DateOnly.TryParse(request.FechaFin, out var fechaFin))
            {
                return new DisponibilidadResponse
                {
                    Disponible = false,
                    Mensaje = "Formato de fecha inválido. Use YYYY-MM-DD."
                };
            }

            // Usa ExistsBloqueoOcupacionAsync directamente — cubre rangos multi-mes correctamente
            bool disponible = await _calendarioService.VerificarDisponibilidadAsync(
                request.HabitacionId, fechaInicio, fechaFin);

            return new DisponibilidadResponse
            {
                Disponible = disponible,
                Mensaje = disponible
                    ? "Fechas disponibles."
                    : "Las fechas seleccionadas ya no están disponibles."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad por gRPC para habitacion {HabitacionId}", request.HabitacionId);
            return new DisponibilidadResponse
            {
                Disponible = false,
                Mensaje = "Error interno al verificar la disponibilidad."
            };
        }
    }

    public override async Task<BloqueoFechasResponse> BloquearFechas(BloqueoFechasRequest request, ServerCallContext context)
    {
        try
        {
            if (!DateOnly.TryParse(request.FechaInicio, out var fechaInicio))
            {
                return new BloqueoFechasResponse
                {
                    Exito = false,
                    Mensaje = $"FechaInicio inválida: {request.FechaInicio}"
                };
            }

            if (!DateOnly.TryParse(request.FechaFin, out var fechaFin))
            {
                return new BloqueoFechasResponse
                {
                    Exito = false,
                    Mensaje = $"FechaFin inválida: {request.FechaFin}"
                };
            }

            if (fechaFin <= fechaInicio)
            {
                return new BloqueoFechasResponse
                {
                    Exito = false,
                    Mensaje = "La fecha de salida debe ser posterior a la fecha de entrada."
                };
            }

            // IMPORTANTE: NO aplicar AddDays(-1) aquí.
            // CalendarioGateway ya envía la fechaFin correcta (sin restar días).
            // BloquearFechasAsync internamente bloquea fechaInicio..fechaFin inclusive.
            await _calendarioService.BloquearFechasAsync(new Alojamientos.Business.DTOs.BloquearFechasRequest
            {
                HabitacionId = request.HabitacionId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });

            return new BloqueoFechasResponse
            {
                Exito = true,
                Mensaje = "Fechas bloqueadas correctamente."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al bloquear fechas por gRPC para habitacion {HabitacionId}", request.HabitacionId);
            return new BloqueoFechasResponse
            {
                Exito = false,
                Mensaje = ex.Message
            };
        }
    }

    public override async Task<LiberarFechasResponse> LiberarFechas(LiberarFechasRequest request, ServerCallContext context)
    {
        try
        {
            if (!DateOnly.TryParse(request.FechaInicio, out var fechaInicio))
            {
                return new LiberarFechasResponse
                {
                    Exito = false,
                    Mensaje = $"FechaInicio invalida: {request.FechaInicio}"
                };
            }

            if (!DateOnly.TryParse(request.FechaFin, out var fechaFin))
            {
                return new LiberarFechasResponse
                {
                    Exito = false,
                    Mensaje = $"FechaFin invalida: {request.FechaFin}"
                };
            }

            await _calendarioService.LiberarFechasAsync(new Alojamientos.Business.DTOs.BloquearFechasRequest
            {
                HabitacionId = request.HabitacionId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });

            return new LiberarFechasResponse
            {
                Exito = true,
                Mensaje = "Fechas liberadas correctamente."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al liberar fechas por gRPC para habitacion {HabitacionId}", request.HabitacionId);
            return new LiberarFechasResponse
            {
                Exito = false,
                Mensaje = ex.Message
            };
        }
    }
}
