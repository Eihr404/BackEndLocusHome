using System.Net.Http.Json;
using Grpc.Core;
using Reservas.Business.Exceptions;
using Reservas.Business.Interfaces;
using Shared.Kernel.Correlation;
using Shared.Protos;

namespace Reservas.API.Integrations;

public class CalendarioGateway : ICalendarioGateway
{
    private readonly CalendarioGrpc.CalendarioGrpcClient _grpcClient;
    private readonly HttpClient _httpClient;
    private readonly CorrelationContextAccessor _correlationAccessor;
    private readonly ILogger<CalendarioGateway> _logger;

    public CalendarioGateway(
        CalendarioGrpc.CalendarioGrpcClient grpcClient,
        HttpClient httpClient,
        CorrelationContextAccessor correlationAccessor,
        ILogger<CalendarioGateway> logger)
    {
        _grpcClient = grpcClient;
        _httpClient = httpClient;
        _correlationAccessor = correlationAccessor;
        _logger = logger;
    }

    public async Task VerificarDisponibilidadAsync(int habitacionId, DateOnly fechaInicio, DateOnly fechaFin)
    {
        try
        {
            var response = await _grpcClient.VerificarDisponibilidadAsync(new DisponibilidadRequest
            {
                HabitacionId = habitacionId,
                FechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
                FechaFin = fechaFin.ToString("yyyy-MM-dd")
            }, BuildMetadata());

            if (!response.Disponible)
            {
                throw new BusinessRuleException(
                    $"Habitacion {habitacionId} no disponible: {response.Mensaje}");
            }
        }
        catch (BusinessRuleException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Fallo la verificacion gRPC de disponibilidad para habitacion {HabitacionId}. Se continuara con el bloqueo definitivo por HTTP.",
                habitacionId);
        }
    }

    public async Task BloquearFechasAsync(int habitacionId, DateOnly fechaInicio, DateOnly fechaFin)
    {
        try
        {
            var response = await _grpcClient.BloquearFechasAsync(new BloqueoFechasRequest
            {
                HabitacionId = habitacionId,
                FechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
                FechaFin = fechaFin.ToString("yyyy-MM-dd")
            }, BuildMetadata());

            if (!response.Exito)
            {
                throw new InvalidOperationException(response.Mensaje);
            }

            return;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Fallo el bloqueo gRPC de fechas para habitacion {HabitacionId}. Se intentara fallback HTTP.",
                habitacionId);
        }

        var payload = new
        {
            habitacionId,
            fechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
            fechaFin = fechaFin.ToString("yyyy-MM-dd")
        };

        var responseHttp = await _httpClient.PostAsJsonAsync("Calendario/bloquear", payload);
        if (!responseHttp.IsSuccessStatusCode)
        {
            var body = await responseHttp.Content.ReadAsStringAsync();
            throw new BusinessRuleException(
                $"No fue posible bloquear la disponibilidad de la habitacion {habitacionId}: {body}");
        }
    }

    public async Task LiberarFechasAsync(int habitacionId, DateOnly fechaInicio, DateOnly fechaFin)
    {
        try
        {
            var response = await _grpcClient.LiberarFechasAsync(new LiberarFechasRequest
            {
                HabitacionId = habitacionId,
                FechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
                FechaFin = fechaFin.ToString("yyyy-MM-dd")
            }, BuildMetadata());

            if (!response.Exito)
            {
                throw new InvalidOperationException(response.Mensaje);
            }

            return;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Fallo la liberacion gRPC de fechas para habitacion {HabitacionId}. Se intentara fallback HTTP.",
                habitacionId);
        }

        var payload = new
        {
            habitacionId,
            fechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
            fechaFin = fechaFin.ToString("yyyy-MM-dd")
        };

        var responseHttp = await _httpClient.PostAsJsonAsync("Calendario/liberar", payload);
        if (!responseHttp.IsSuccessStatusCode)
        {
            var body = await responseHttp.Content.ReadAsStringAsync();
            throw new BusinessRuleException(
                $"No fue posible liberar la disponibilidad de la habitacion {habitacionId}: {body}");
        }
    }

    private Metadata BuildMetadata()
    {
        var metadata = new Metadata();
        if (!string.IsNullOrWhiteSpace(_correlationAccessor.CorrelationId))
        {
            metadata.Add(CorrelationConstants.HeaderName.ToLowerInvariant(), _correlationAccessor.CorrelationId);
        }

        return metadata;
    }
}
