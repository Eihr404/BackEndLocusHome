# Fase 3 - RabbitMQ, Saga e Idempotencia

## Que se implemento

### 1. Contrato base de eventos

Todos los eventos de integracion heredan de `IntegrationEvent` y comparten:

- `MessageId`
- `CorrelationId`
- `EventSource`
- `OccurredAt`
- `Version`

La metadata se completa desde `EventFactory`.

### 2. Flujo de saga actual

El flujo asincrono actual queda asi:

1. `Reservas.API` crea la reserva y publica `ReservaCreadaEvent`.
2. `Facturacion.API` crea la factura y publica `PagoPendienteEvent`.
3. Si el pago es aprobado, `Facturacion.API` publica:
   - `FacturaPagadaEvent`
   - `PagoAprobadoEvent`
4. `Reservas.API` consume `FacturaPagadaEvent`, confirma la reserva y publica `ReservaConfirmadaEvent`.
5. Si el pago es rechazado, `Facturacion.API` publica `PagoRechazadoEvent`.
6. `Reservas.API` consume `PagoRechazadoEvent`, libera fechas bloqueadas, cancela la reserva y publica `ReservaCanceladaEvent`.

### 3. Compensacion

La compensacion de la saga vive en `PagoRechazadoConsumer`:

- cambia el estado de la reserva a cancelada
- libera fechas en calendario por cada habitacion involucrada
- publica `ReservaCanceladaEvent`

### 4. Idempotencia en consumidores

Se usa el patron inbox en `Reservas.API` con la tabla `processed_messages`.

Cada registro guarda:

- `MessageId`
- `Consumer`
- `ProcessedAt`

## Correccion aplicada

La clave de `processed_messages` fue corregida a compuesta:

- `MessageId`
- `Consumer`

Esto evita falsos duplicados entre consumidores distintos del mismo servicio.

Ademas, `Reservas.API` usa `UseInMemoryOutbox` en el pipeline de consumo para evitar publicar eventos derivados duplicados cuando MassTransit reintenta un mensaje.

## Que falta al desplegar

- aplicar la migracion o ajuste SQL correspondiente para reflejar la clave compuesta en la base de datos de `Reservas`
- probar duplicados reales publicando el mismo `MessageId` mas de una vez
- evidenciar cola normal, retry y cola de error de RabbitMQ

## Ajuste SQL sugerido para `processed_messages`

Si la tabla ya existe con clave primaria solo por `MessageId`, debe actualizarse a clave compuesta:

```sql
ALTER TABLE processed_messages DROP CONSTRAINT IF EXISTS processed_messages_pkey;

ALTER TABLE processed_messages
ADD CONSTRAINT processed_messages_pkey PRIMARY KEY (message_id, consumer);

CREATE INDEX IF NOT EXISTS ix_processed_messages_processed_at
ON processed_messages (processed_at);
```
