using System.Net.Http.Json;
using Reservas.Business.Exceptions;
using Reservas.Business.Interfaces;
using Shared.Protos;

namespace Reservas.API.Integrations;

public class CalendarioGateway : ICalendarioGateway
{
    private readonly CalendarioGrpc.CalendarioGrpcClient _grpcClient;
    private readonly HttpClient _httpClient;
    private readonly ILogger<CalendarioGateway> _logger;

    public CalendarioGateway(
        CalendarioGrpc.CalendarioGrpcClient grpcClient,
        HttpClient httpClient,
        ILogger<CalendarioGateway> logger)
    {
        _grpcClient = grpcClient;
        _httpClient = httpClient;
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
            });

            if (!response.Disponible)
            {
                throw new BusinessRuleException(
                    $"Habitación {habitacionId} no disponible: {response.Mensaje}");
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
                "Fallo la verificación gRPC de disponibilidad para habitacion {HabitacionId}. Se continuará con el bloqueo definitivo por HTTP.",
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
            });

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
                "Fallo el bloqueo gRPC de fechas para habitacion {HabitacionId}. Se intentará fallback HTTP.",
                habitacionId);
        }

        var payload = new
        {
            habitacionId,
            fechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
            fechaFin = fechaFin.AddDays(-1).ToString("yyyy-MM-dd")
        };

        var responseHttp = await _httpClient.PostAsJsonAsync("Calendario/bloquear", payload);
        if (!responseHttp.IsSuccessStatusCode)
        {
            var body = await responseHttp.Content.ReadAsStringAsync();
            throw new BusinessRuleException(
                $"No fue posible bloquear la disponibilidad de la habitación {habitacionId}: {body}");
        }
    }
}
