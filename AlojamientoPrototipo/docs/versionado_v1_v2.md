# Versionado V1 y V2

## Regla aplicada

- `V1` conserva los endpoints REST previos para clientes existentes.
- `V2` expone la evolucion funcional para movil, GraphQL y nuevas capacidades sin pisar `V1`.
- `GraphQLGateway` consume explicitamente `V2`.

## Corte arquitectonico

- `REST/YARP V1`: compatibilidad con clientes actuales y admin web.
- `REST V2`: evolucion de endpoints para nuevos consumidores.
- `GraphQL`: contrato exclusivo para marketplace movil, montado sobre `V2`.
- `gRPC` y `RabbitMQ`: integracion interna entre microservicios, compartida por ambas versiones.

## Microservicios versionados

### Alojamientos

- `api/v1/alojamientos`
- `api/v1/habitaciones`
- `api/v1/fotos`
- `api/v1/calendario`
- `api/v2/alojamientos`
- `api/v2/habitaciones`
- `api/v2/fotos`
- `api/v2/calendario`
- `POST /api/v2/calendario/liberar` existe solo en `V2`.

### Reservas

- `api/v1/reservas`
- `api/v2/reservas`

### Facturacion

- `api/v1/facturas`
- `api/v1/metodospago`
- `api/v2/facturas`
- `api/v2/metodospago`

### Usuarios

- `api/v1/auth`
- `api/v1/clientes`
- `api/v1/usuarios`
- `api/v1/localizaciones`
- `api/v2/auth`
- `api/v2/clientes`

## Consumo movil

El cliente movil no debe consumir `V1` ni llamar microservicios directamente.

- `GraphQLGateway` -> `api/v2/alojamientos`
- `GraphQLGateway` -> `api/v2/reservas`
- `GraphQLGateway` -> `api/v2/facturas`
- `GraphQLGateway` -> `api/v2/metodospago`
- `GraphQLGateway` -> `api/v2/auth`
- `GraphQLGateway` -> `api/v2/clientes`

## Implicacion practica

- Un cliente legado puede seguir usando `V1` sin cambiar rutas.
- La app movil y las capacidades nuevas se construyen sobre `V2`.
- La evolucion futura debe entrar primero en `V2`; solo se replica en `V1` si hace falta mantener compatibilidad.
