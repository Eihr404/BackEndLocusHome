using MassTransit;
using Shared.Kernel.Correlation;
using Shared.Kernel.Events;
using SoapFacturaGateway.Services;

namespace SoapFacturaGateway.Consumers;

public sealed class PagoAprobadoConsumer : IConsumer<PagoAprobadoEvent>
{
    private readonly FacturacionApiClient _facturacionApiClient;
    private readonly SoapFacturaDispatchService _dispatchService;
    private readonly CorrelationContextAccessor _correlationAccessor;
    private readonly ILogger<PagoAprobadoConsumer> _logger;

    public PagoAprobadoConsumer(
        FacturacionApiClient facturacionApiClient,
        SoapFacturaDispatchService dispatchService,
        CorrelationContextAccessor correlationAccessor,
        ILogger<PagoAprobadoConsumer> logger)
    {
        _facturacionApiClient = facturacionApiClient;
        _dispatchService = dispatchService;
        _correlationAccessor = correlationAccessor;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PagoAprobadoEvent> context)
    {
        _correlationAccessor.CorrelationId = context.Message.CorrelationId;
        var factura = await _facturacionApiClient.GetFacturaByIdAsync(context.Message.FacturaId, context.CancellationToken);
        await _dispatchService.DispatchAsync(factura, context.Message.CorrelationId, context.CancellationToken);

        _logger.LogInformation(
            "Factura {FacturaId} convertida a XML SOAP para la reserva {ReservaId}",
            context.Message.FacturaId,
            context.Message.ReservaId);
    }
}
