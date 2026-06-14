# Idempotencia de negocio

## Objetivo

Evitar que una misma operacion de negocio se ejecute dos veces cuando el cliente:

- hace doble clic
- reintenta por timeout
- reenvia el mismo request desde movil o web

## Operaciones cubiertas

- `CrearReserva`
- `CrearFactura`

## Flujo

1. El cliente envia `IdempotencyKey`.
2. La capa de aplicacion calcula un `RequestHash` con el payload canonico.
3. Si ya existe una solicitud completada con la misma key y el mismo hash:
   - se devuelve el recurso ya creado
4. Si existe la misma key pero con otro hash:
   - se rechaza como conflicto
5. Si existe la misma key en estado `Pending`:
   - se rechaza como operacion en proceso
6. Si no existe:
   - se registra como `Pending`
   - se ejecuta la transaccion de negocio
   - se actualiza a `Completed` con el id del recurso creado

## Tabla SQL para Reservas

```sql
CREATE TABLE IF NOT EXISTS public.idempotent_requests (
  idempotency_key character varying(128) NOT NULL,
  operation_name character varying(64) NOT NULL,
  request_hash character varying(128) NOT NULL,
  status character varying(32) NOT NULL,
  resource_id integer,
  created_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT idempotent_requests_pkey PRIMARY KEY (idempotency_key, operation_name)
);

CREATE INDEX IF NOT EXISTS ix_idempotent_requests_created_at
ON public.idempotent_requests (created_at);

CREATE INDEX IF NOT EXISTS ix_idempotent_requests_resource_id
ON public.idempotent_requests (resource_id);
```

## Tabla SQL para Facturacion

```sql
CREATE TABLE IF NOT EXISTS public.idempotent_requests (
  idempotency_key character varying(128) NOT NULL,
  operation_name character varying(64) NOT NULL,
  request_hash character varying(128) NOT NULL,
  status character varying(32) NOT NULL,
  resource_id integer,
  created_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT idempotent_requests_pkey PRIMARY KEY (idempotency_key, operation_name)
);

CREATE INDEX IF NOT EXISTS ix_idempotent_requests_created_at
ON public.idempotent_requests (created_at);

CREATE INDEX IF NOT EXISTS ix_idempotent_requests_resource_id
ON public.idempotent_requests (resource_id);
```

## Uso recomendado

- `GraphQL` y `REST V2` deben enviar `idempotencyKey` en:
  - `crearReserva`
  - `crearFactura`

## Resultado esperado

- misma `IdempotencyKey` + mismo payload:
  - mismo resultado
- misma `IdempotencyKey` + payload distinto:
  - conflicto
- misma `IdempotencyKey` mientras la primera sigue en proceso:
  - conflicto
