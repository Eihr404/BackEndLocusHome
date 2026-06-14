using Microsoft.AspNetCore.Mvc;
using SoapFacturaGateway.Models;
using SoapFacturaGateway.Services;

namespace SoapFacturaGateway.Controllers;

[ApiController]
[Route("soap/facturacion")]
public class SoapFacturacionController : ControllerBase
{
    private readonly SoapEnvelopeBuilder _envelopeBuilder;
    private readonly FacturaXmlBuilder _facturaXmlBuilder;

    public SoapFacturacionController(SoapEnvelopeBuilder envelopeBuilder, FacturaXmlBuilder facturaXmlBuilder)
    {
        _envelopeBuilder = envelopeBuilder;
        _facturaXmlBuilder = facturaXmlBuilder;
    }

    [HttpGet("wsdl")]
    public IActionResult GetWsdl()
    {
        var wsdlPath = Path.Combine(AppContext.BaseDirectory, "Artifacts", "facturacion.wsdl");
        if (!System.IO.File.Exists(wsdlPath))
        {
            return NotFound();
        }

        return PhysicalFile(wsdlPath, "text/xml");
    }

    [HttpPost]
    [Consumes("text/xml", "application/soap+xml")]
    public async Task<IActionResult> GenerarFacturaSoap(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var requestBody = await reader.ReadToEndAsync(cancellationToken);
        var request = SoapRequestParser.Parse(requestBody);

        var invoiceXml = _facturaXmlBuilder.Build(new SoapFacturaPayload
        {
            FacturaId = request.FacturaId,
            ReservaId = request.ReservaId,
            Monto = request.Monto,
            MetodoPago = request.MetodoPago,
            FechaPago = request.FechaPago,
            CorrelationId = request.CorrelationId,
            Detalles = request.Detalles
        });

        var soapEnvelope = _envelopeBuilder.BuildSuccessResponse(invoiceXml);
        return Content(soapEnvelope, "text/xml");
    }
}
