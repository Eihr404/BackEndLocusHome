using System.Globalization;
using System.Xml.Linq;
using SoapFacturaGateway.Models;

namespace SoapFacturaGateway.Services;

public static class SoapRequestParser
{
    public static SoapFacturaPayload Parse(string soapEnvelope)
    {
        var document = XDocument.Parse(soapEnvelope);
        XNamespace fac = "http://schemas.locushome/facturacion";

        var body = document.Descendants(fac + "GenerarFactura").FirstOrDefault()
            ?? throw new InvalidOperationException("No se encontró la operación GenerarFactura.");

        return new SoapFacturaPayload
        {
            FacturaId = ParseInt(body.Element(fac + "FacturaId")?.Value),
            ReservaId = ParseInt(body.Element(fac + "ReservaId")?.Value),
            Monto = ParseDecimal(body.Element(fac + "Monto")?.Value),
            MetodoPago = body.Element(fac + "MetodoPago")?.Value ?? string.Empty,
            FechaPago = DateTime.TryParse(body.Element(fac + "FechaPago")?.Value, out var fechaPago) ? fechaPago : null,
            CorrelationId = body.Element(fac + "CorrelationId")?.Value ?? string.Empty,
            Detalles = body.Elements(fac + "Detalle")
                .Select(d => new SoapFacturaDetallePayload
                {
                    Descripcion = d.Element(fac + "Descripcion")?.Value ?? string.Empty,
                    Cantidad = ParseInt(d.Element(fac + "Cantidad")?.Value),
                    PrecioUnitario = ParseDecimal(d.Element(fac + "PrecioUnitario")?.Value)
                }).ToList()
        };
    }

    private static int ParseInt(string? value)
        => int.TryParse(value, out var result) ? result : 0;

    private static decimal ParseDecimal(string? value)
        => decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0m;
}
