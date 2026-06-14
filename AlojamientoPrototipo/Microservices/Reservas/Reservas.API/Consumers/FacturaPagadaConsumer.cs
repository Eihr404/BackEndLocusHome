using MassTransit;
using Reservas.Business.Interfaces;
using Shared.Kernel.Correlation;
using Shared.Kernel.Events;

namespace Reservas.API.Consumers;

public class FacturaPagadaConsumer : IConsumer<FacturaPagadaEvent>
{
    private readonly IReservasService _reservasService;
    private readonly CorrelationContextAccessor _correlationAccessor;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<FacturaPagadaConsumer> _logger;

    public FacturaPagadaConsumer(
        IReservasService reservasService,
        CorrelationContextAccessor correlationAccessor,
        IPublishEndpoint publishEndpoint,
        ILogger<FacturaPagadaConsumer> logger)
    {
        _reservasService = reservasService;
        _correlationAccessor = correlationAccessor;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FacturaPagadaEvent> context)
    {
        var evento = context.Message;
        _correlationAccessor.CorrelationId = evento.CorrelationId;

        _logger.LogInformation(
            "Evento FacturaPagadaEvent recibido: ReservaId={ReservaId}, FacturaId={FacturaId}, Monto={Monto}, CorrelationId={CorrelationId}",
            evento.ReservaId, evento.FacturaId, evento.MontoPagado, evento.CorrelationId);

        var procesado = await _reservasService.ConfirmarReservaPorPagoAsync(evento.ReservaId);
        if (!procesado)
        {
            _logger.LogInformation(
                "FacturaPagadaEvent ignorado por guard de estado. ReservaId={ReservaId}, Estado no pendiente.",
                evento.ReservaId);
            return;
        }

        var reservaConfirmada = EventFactory.ApplyMetadata(new ReservaConfirmadaEvent
        {
            ReservaId = evento.ReservaId,
            FacturaId = evento.FacturaId
        }, "Reservas.API", _correlationAccessor);

        await _publishEndpoint.Publish(reservaConfirmada, publishContext =>
        {
            publishContext.Headers.Set(CorrelationConstants.HeaderName, reservaConfirmada.CorrelationId);
        }, context.CancellationToken);

        _logger.LogInformation(
            "Reserva {ReservaId} actualizada a Confirmada tras pago de factura {FacturaId}",
            evento.ReservaId, evento.FacturaId);
    }
}
