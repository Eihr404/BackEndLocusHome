# Fase 1 - GraphQL Gateway inicial

Este documento define la primera implementacion estricta de la Fase 1 del reto 3.

## Objetivo

Introducir una capa GraphQL para el marketplace movil sin reemplazar el gateway REST existente.

## Decision de arquitectura en esta iteracion

- `ApiGateway/ApiGateway` se mantiene para REST/YARP y compatibilidad con frontend web/admin.
- `ApiGateway/GraphQLGateway` es el nuevo punto de entrada para el marketplace movil.
- `GraphQLGateway` funciona como agregador y BFF; no mueve la logica de negocio fuera de los microservicios.
- El gateway GraphQL consume `REST V2` para que la evolucion movil no rompa a los clientes de `V1`.
- La federacion completa por subgrafos queda como incremento posterior dentro de la misma fase.

## Endpoint

- `/graphql`

## Base de consumo backend

- `Usuarios.API` por `api/v2/auth` y `api/v2/clientes`
- `Alojamientos.API` por `api/v2/alojamientos`, `api/v2/habitaciones`, `api/v2/fotos`, `api/v2/calendario`
- `Reservas.API` por `api/v2/reservas`
- `Facturacion.API` por `api/v2/facturas` y `api/v2/metodospago`

## Consultas iniciales

- `alojamientos`
- `alojamientoById(id)`
- `habitacionById(id)`
- `reservaById(id)`
- `reservasByCliente(clienteId)`
- `reservasByAlojamiento(alojamientoId)`
- `facturaByReservaId(reservaId)`
- `reservaMarketplaceById(reservaId)`

## Consultas orientadas a pantallas moviles

- `marketplaceCatalog(ciudad, admiteMascotas, tienePiscina, tieneParqueadero)`
- `marketplaceAlojamientoDetalle(alojamientoId)`
- `marketplaceClienteReservas(clienteId)`
- `marketplaceTiposAlojamiento`
- `marketplaceMetodosPago`

## Mutaciones iniciales

- `login(input)`
- `registrarCliente(input)`
- `asegurarPerfilCliente(input)`
- `crearReserva(input)`
- `crearFactura(input)`

## Composicion inicial

`reservaMarketplaceById` agrega:

- datos de `Reservas`
- datos de `Alojamientos`
- datos de `Facturacion`

Esto reduce overfetching desde el cliente movil y deja lista la base para el supergrafo.

## Relaciones anidadas ya habilitadas

- `Alojamiento.habitaciones`
- `Alojamiento.fotos`
- `Alojamiento.reservas`
- `Habitacion.disponibilidadMensual(mes, anio)`
- `Reserva.alojamiento`
- `Reserva.factura`
- `ReservaResumen.alojamiento`
- `ReservaResumen.factura`

## Vistas compuestas disponibles

- `marketplaceCatalog` devuelve tarjetas listas para catalogo con precio minimo e imagen principal.
- `marketplaceAlojamientoDetalle` devuelve alojamiento, habitaciones, fotos y rango de precios.
- `marketplaceClienteReservas` devuelve reservas del cliente enriquecidas con nombre de alojamiento y resumen de factura.

## Flujo movil ya soportado por GraphQL

- autenticacion inicial
- registro de cliente
- aseguramiento de perfil
- exploracion del catalogo
- consulta de detalle de alojamiento
- creacion de reserva
- consulta de reservas del cliente
- consulta de metodos de pago
- creacion de factura

## Regla de versionado

- `GraphQL` usa solo `V2`.
- `V1` se conserva para clientes existentes y compatibilidad operativa.
- Ningun cambio nuevo orientado a movil debe exigir migrar a los consumidores actuales de `V1`.
