# Reto 3 - Eventos, Correlation ID y SOAP

## Eventos de saga

Todos los eventos comparten:

- `MessageId`
- `CorrelationId`
- `EventSource`
- `OccurredAt`
- `Version`

Eventos implementados:

- `ReservaCreadaEvent`
- `PagoPendienteEvent`
- `FacturaPagadaEvent`
- `PagoAprobadoEvent`
- `PagoRechazadoEvent`
- `ReservaConfirmadaEvent`
- `ReservaCanceladaEvent`

## Correlation ID

- Header HTTP: `X-Correlation-ID`
- Propagación:
  - GraphQL Gateway -> microservicios REST
  - Reservas -> Alojamientos por gRPC/HTTP fallback
  - Facturación/Reservas -> RabbitMQ headers y payload de evento
  - SOAP gateway -> logs y XML generado

## SOAP

- WSDL: `Integrations/SoapFacturaGateway/Artifacts/facturacion.wsdl`
- Endpoint SOAP: `/soap/facturacion`
- WSDL público: `/soap/facturacion/wsdl`
