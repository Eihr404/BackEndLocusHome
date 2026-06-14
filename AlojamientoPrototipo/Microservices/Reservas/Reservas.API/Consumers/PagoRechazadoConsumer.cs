using MassTransit;
using Reservas.Business.Interfaces;
using Shared.Kernel.Correlation;
using Shared.Kernel.Events;

namespace Reservas.API.Consumers;

public class PagoRechazadoConsumer : IConsumer<PagoRechazadoEvent>
{
    private readonly IReservasService _reservasService;
    private readonly CorrelationContextAccessor _correlationAccessor;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PagoRechazadoConsumer> _logger;

    public PagoRechazadoConsumer(
        IReservasService reservasService,
        CorrelationContextAccessor correlationAccessor,
        IPublishEndpoint publishEndpoint,
        ILogger<PagoRechazadoConsumer> logger)
    {
        _reservasService = reservasService;
        _correlationAccessor = correlationAccessor;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PagoRechazadoEvent> context)
    {
        var evento = context.Message;
        _correlationAccessor.CorrelationId = evento.CorrelationId;
        var procesado = await _reservasService.CancelarReservaPorPagoRechazadoAsync(evento.ReservaId, evento.Motivo);
        if (!procesado)
        {
            _logger.LogInformation(
                "PagoRechazadoEvent ignorado por guard de estado. ReservaId={ReservaId}, Estado no pendiente.",
                evento.ReservaId);
            return;
        }

        var reservaCancelada = EventFactory.ApplyMetadata(new ReservaCanceladaEvent
        {
            ReservaId = evento.ReservaId,
            Motivo = evento.Motivo
        }, "Reservas.API", _correlationAccessor);

        await _publishEndpoint.Publish(reservaCancelada, publishContext =>
        {
            publishContext.Headers.Set(CorrelationConstants.HeaderName, reservaCancelada.CorrelationId);
        }, context.CancellationToken);
    }
}
