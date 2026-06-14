using System.Xml.Linq;
using SoapFacturaGateway.Models;

namespace SoapFacturaGateway.Services;

public sealed class FacturaXmlBuilder
{
    public string Build(SoapFacturaPayload payload)
    {
        var document = new XDocument(
            new XElement("FacturaElectronica",
                new XElement("FacturaId", payload.FacturaId),
                new XElement("ReservaId", payload.ReservaId),
                new XElement("Monto", payload.Monto),
                new XElement("MetodoPago", payload.MetodoPago),
                new XElement("FechaPago", payload.FechaPago?.ToString("O") ?? string.Empty),
                new XElement("CorrelationId", payload.CorrelationId),
                new XElement("Detalles",
                    payload.Detalles.Select(d =>
                        new XElement("Detalle",
                            new XElement("Descripcion", d.Descripcion),
                            new XElement("Cantidad", d.Cantidad),
                            new XElement("PrecioUnitario", d.PrecioUnitario))))));

        return document.ToString(SaveOptions.DisableFormatting);
    }
}
