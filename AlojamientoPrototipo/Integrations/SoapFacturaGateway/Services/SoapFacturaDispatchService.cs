using SoapFacturaGateway.Models;

namespace SoapFacturaGateway.Services;

public sealed class SoapFacturaDispatchService
{
    private readonly FacturaXmlBuilder _facturaXmlBuilder;
    private readonly ILogger<SoapFacturaDispatchService> _logger;

    public SoapFacturaDispatchService(FacturaXmlBuilder facturaXmlBuilder, ILogger<SoapFacturaDispatchService> logger)
    {
        _facturaXmlBuilder = facturaXmlBuilder;
        _logger = logger;
    }

    public Task DispatchAsync(FacturaApiResponse factura, string correlationId, CancellationToken cancellationToken)
    {
        var payload = new SoapFacturaPayload
        {
            FacturaId = factura.FacturaId,
            ReservaId = factura.ReservaId,
            Monto = factura.Monto,
            MetodoPago = factura.MetodoPagoTipo ?? string.Empty,
            FechaPago = factura.FechaPago,
            CorrelationId = correlationId,
            Detalles = factura.Detalles.Select(d => new SoapFacturaDetallePayload
            {
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario
            }).ToList()
        };

        var xml = _facturaXmlBuilder.Build(payload);
        _logger.LogInformation("XML SOAP generado para FacturaId={FacturaId}: {Xml}", factura.FacturaId, xml);
        return Task.CompletedTask;
    }
}
