namespace SoapFacturaGateway.Services;

public sealed class SoapEnvelopeBuilder
{
    public string BuildSuccessResponse(string xmlPayload)
        => $"""
           <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:fac="http://schemas.locushome/facturacion">
             <soapenv:Body>
               <fac:GenerarFacturaResponse>
                 <fac:Estado>OK</fac:Estado>
                 <fac:FacturaXml><![CDATA[{xmlPayload}]]></fac:FacturaXml>
               </fac:GenerarFacturaResponse>
             </soapenv:Body>
           </soapenv:Envelope>
           """;
}
