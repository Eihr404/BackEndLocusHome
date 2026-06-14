# Documentación de Arquitectura — LocusHome

> Proyecto: `BackEndLocusHome` / `AlojamientoPrototipo`
> Stack: .NET 9 · Angular 21 · PostgreSQL · RabbitMQ · gRPC
> Despliegue: Render.com

---

## Tabla de contenidos

1. [Visión general](#1-visión-general)
2. [Frontend Angular](#2-frontend-angular)
3. [API Gateway (YARP)](#3-api-gateway-yarp)
4. [Microservicios](#4-microservicios)
   - [Estructura común por microservicio](#41-estructura-común-por-microservicio)
   - [Microservicio Usuarios](#42-microservicio-usuarios)
   - [Microservicio Alojamientos](#43-microservicio-alojamientos)
   - [Microservicio Reservas](#44-microservicio-reservas)
   - [Microservicio Facturación](#45-microservicio-facturación)
5. [Comunicación entre microservicios](#5-comunicación-entre-microservicios)
   - [gRPC — Calendario](#51-grpc--calendario)
   - [Event Bus — RabbitMQ + MassTransit](#52-event-bus--rabbitmq--masstransit)
6. [Capa Shared](#6-capa-shared)
7. [Bases de datos](#7-bases-de-datos)
8. [Servicios externos](#8-servicios-externos)
9. [Infraestructura y despliegue](#9-infraestructura-y-despliegue)
10. [Patrones y convenciones](#10-patrones-y-convenciones)

---

## 1. Visión general

LocusHome es una plataforma de reservas de alojamiento (hoteles, apartamentos, cabañas) compuesta por:

- Un **frontend SPA en Angular** servido por Nginx.
- Un **API Gateway** basado en YARP que enruta tráfico externo.
- Cuatro **microservicios independientes** en ASP.NET Core 9, cada uno con su propia base de datos PostgreSQL.
- Un **canal gRPC** para comunicación síncrona entre Reservas y Alojamientos.
- Un **Event Bus** (RabbitMQ + MassTransit) para comunicación asíncrona entre Facturación y Reservas.
- Recursos externos: Cloudinary (imágenes), CloudAMQP (RabbitMQ en nube), Render.com (hosting).

```
[Angular SPA]
      ↓ HTTP/REST
[API Gateway — YARP]
      ↓ HTTP/REST (proxy + path transform)
┌─────────┬──────────────┬──────────┬─────────────┐
│ Usuarios│ Alojamientos │ Reservas │ Facturación │
└────┬────┴──────┬───────┴─────┬────┴──────┬──────┘
     │           │   gRPC ←────┘           │
     │           │                         │
  DB_Usu     DB_Aloj     DB_Res         DB_Fact
                          ↑ FacturaPagadaEvent (RabbitMQ)
                          └──────────────────────────────┘
```

---

## 2. Frontend Angular

**Tecnología:** Angular 21 · TypeScript 5.9 · Nginx  
**Puerto en producción:** 5173  
**Pruebas:** Playwright + Vitest

### Estructura de rutas

```
/                        → HomePageComponent       (público)
/explorar                → CatalogPageComponent     (público)
/alojamientos/:id        → PropertyDetailPageComponent (público)
/login                   → LoginPageComponent       (público)
/registro                → RegisterPageComponent    (público)
/mis-reservas            → ReservationsPageComponent (requiere auth)

/socio                   → PartnerDashboardPageComponent (requiere auth + rol 'socio')
/socio/propiedades       → PartnerPropertiesPageComponent
/socio/reservas          → PartnerReservationsPageComponent
```

Hay dos layouts: `PublicLayoutComponent` (visitantes y clientes) y `PartnerLayoutComponent` (socios/propietarios). La protección de rutas usa dos guards:

- `authGuard` — verifica que haya sesión activa.
- `roleGuard` — verifica que el usuario tenga el rol requerido (lee `data.roles` del route).

### Servicios Angular

| Servicio | Responsabilidad |
|---|---|
| `auth.service.ts` | Login, registro, manejo de token JWT, estado de sesión |
| `alojamientos.service.ts` | Consulta y gestión de alojamientos y habitaciones |
| `reservas.service.ts` | Crear y consultar reservas del cliente |
| `facturacion.service.ts` | Consulta de facturas y métodos de pago |
| `partner.service.ts` | Operaciones del socio (propiedades, reservas entrantes) |

### Configuración de endpoints (`api.config.ts`)

```typescript
API_GATEWAY_BASE_URL    = 'https://israel-apigateway.onrender.com/api/v1/israel-hernandez'
USUARIOS_API_BASE_URL   = 'https://israel-usuarios-api.onrender.com/api/v1'
ALOJAMIENTOS_API_BASE_URL = 'https://israel-alojamientos-api.onrender.com/api/v1'
RESERVAS_API_BASE_URL   = 'https://israel-reservas-api.onrender.com/api/v1'
FACTURACION_API_BASE_URL = 'https://israel-facturacion-api.onrender.com/api/v1'

CLOUDINARY_CLOUD_NAME   = 'dzxkufyfp'
CLOUDINARY_UPLOAD_PRESET = 'locushome_uploads'
```

Las imágenes se suben directamente desde el browser a Cloudinary (sin pasar por el backend) usando un preset `unsigned`.

---

## 3. API Gateway (YARP)

**Proyecto:** `ApiGateway/ApiGateway`  
**Tecnología:** ASP.NET Core 9 + YARP (Yet Another Reverse Proxy)  
**Puerto:** 5028

El Gateway tiene una responsabilidad única: enrutar el tráfico externo a los microservicios internos, transformando los paths de entrada. También expone un Swagger con el contrato público.

### Rutas configuradas

| Ruta externa (entrada) | Microservicio destino | Ruta interna |
|---|---|---|
| `/api/v1/israel-hernandez/booking/{**remainder}` | Reservas | `/api/v1/reservas/{**remainder}` |
| `/api/v1/israel-hernandez/alojamientos/{**remainder}` | Alojamientos | `/api/v1/alojamientos/{**remainder}` |
| `/api/v1/israel-hernandez/calendario/{**remainder}` | Alojamientos | `/api/v1/calendario/{**remainder}` |

### Clusters (destinos)

```json
"reservas-cluster":     "https://reservas-api.example.com/"
"alojamientos-cluster": "https://alojamientos-api.example.com/"
```

CORS configurado con `AllowAnyOrigin / AllowAnyHeader / AllowAnyMethod`.

---

## 4. Microservicios

### 4.1 Estructura común por microservicio

Todos los microservicios siguen exactamente la misma arquitectura en capas (Clean Architecture):

```
Microservicio.API
│   Controllers/V1/
│   Extensions/          (ServiceCollectionExtensions, CorsExtensions, SwaggerExtensions)
│   GrpcServices/        (solo Alojamientos)
│   Middleware/          (ExceptionHandlingMiddleware)
│   Models/Common/       (ApiResponse, ApiErrorResponse)
│   Program.cs
│
Microservicio.Business
│   DTOs/
│   Exceptions/          (BusinessExceptions — domain errors)
│   Interfaces/          (IService contracts)
│   Mappers/
│   Services/            (implementaciones de negocio)
│   EventPublishers/     (solo Alojamientos y Usuarios)
│
Microservicio.DataManagement
│   Interfaces/          (IDataService, IUnitOfWork)
│   Models/              (DataModels — modelos de persistencia)
│   Mappers/
│   Services/            (DataService implementations)
│
Microservicio.DataAccess
    Configurations/      (EF Core Fluent API)
    Contexts/            (DbContext)
    Entities/            (entidades de base de datos)
    Repositories/        (implementaciones + interfaces IRepository)
    Common/              (RepositoryBase, PagedResult)
```

Todos usan:
- `ExceptionHandlingMiddleware` para manejo global de errores
- Swagger siempre habilitado (incluso en producción para el prototipo)
- EF Core con `UseLowerCaseNamingConvention()` para PostgreSQL
- Patrón Repository + Unit of Work

---

### 4.2 Microservicio Usuarios

**Proyecto:** `Microservices/Usuarios`  
**Base de datos:** `DB_Usuarios`

#### Controllers y endpoints

| Controller | Endpoints |
|---|---|
| `AuthController` | POST /auth (login, registro) |
| `ClientesController` | CRUD de clientes (perfil, datos personales) |
| `UsuariosController` | Gestión de cuentas de usuario |
| `LocalizacionesController` | Gestión de áreas geoespaciales |

#### Entidades principales

**Usuarios**
- `UsuarioId`, `Rol` (Cliente / Socio), `Email`, `PasswordHash` (bcrypt), `NombreCompleto`, `Estado`, `FechaCreacion`

**Clientes**
- `ClienteId`, `UsuarioId` (FK → Usuarios), `Cedula`, `FotoUrl`, `Telefono`, `Domicilio`, `Email`, `TotalReservas`

**Localizaciones**
- `LocalizacionId`, `Area` (tipo `POLYGON` de PostgreSQL), `Descripcion`
- Usa `NetTopologySuite` para soporte geoespacial

#### Servicios de negocio

- `AuthService` — login con JWT, hash de contraseñas
- `ClientesService` — CRUD completo del perfil de cliente
- `UsuariosService` — gestión de cuentas
- `LocalizacionesService` — operaciones sobre polígonos geoespaciales

---

### 4.3 Microservicio Alojamientos

**Proyecto:** `Microservices/Alojamientos`  
**Base de datos:** `DB_Alojamientos`  
**Protocolo adicional:** gRPC (servidor)

#### Controllers y endpoints

| Controller | Endpoints |
|---|---|
| `AlojamientosController` | CRUD de alojamientos (listado, detalle, crear, editar) |
| `HabitacionesController` | CRUD de habitaciones por alojamiento |
| `FotosController` | Subida y gestión de fotos (vía Cloudinary) |
| `CalendarioController` | Consulta y gestión de disponibilidad |

#### Entidades principales

**TiposAlojamiento** — Hotel, Cabaña, Apartamento, etc.

**Alojamientos**
- `AlojamientoId`, `SocioId` (ref lógica a Usuarios), `TipoAlojamientoId`, `Ciudad`, `Nombre`, `Descripcion`, `Direccion`, `Coordenadas` (tipo `POINT`), `Estrellas` (1-5), `CalificacionPromedio`, `AdmiteMascotas`, `TienePiscina`, `TieneParqueadero`, `Estado` (Pendiente / Activo / Inactivo)

**Habitaciones**
- `HabitacionId`, `AlojamientoId`, `Nombre`, `CapacidadAdultos`, `CapacidadNinos`, `NumBanos`, `NumDormitorios`, `TieneCocina`, `TieneAireAcondicionado`, `SuperficieM2`, `PrecioNoche`

**AlojamientoFotos**
- `FotoId`, `AlojamientoId`, `Url` (Cloudinary), `Orden`, `Descripcion`

**CalendarioDisponibilidad**
- Registra bloqueos de fechas por habitación

#### gRPC Server

Este microservicio expone un servidor gRPC (`CalendarioGrpcService`) con dos métodos:

```protobuf
service CalendarioGrpc {
  rpc VerificarDisponibilidad (DisponibilidadRequest) returns (DisponibilidadResponse);
  rpc BloquearFechas (BloqueoFechasRequest) returns (BloqueoFechasResponse);
}
```

- `VerificarDisponibilidad` — dado un `habitacion_id` y rango de fechas, retorna si está disponible.
- `BloquearFechas` — bloquea un rango de fechas para una habitación (se llama al confirmar una reserva).

El contrato `.proto` vive en `Shared/Shared.Protos/Protos/calendario.proto`.

#### Servicios de negocio

- `AlojamientosService` — CRUD con validaciones de dominio
- `HabitacionesService` — gestión de habitaciones
- `FotosService` — integración con `CloudinaryUploadService`
- `CalendarioService` — lógica de disponibilidad y bloqueo de fechas (`VerificarDisponibilidadAsync`, `BloquearFechasAsync`)

---

### 4.4 Microservicio Reservas

**Proyecto:** `Microservices/Reservas`  
**Base de datos:** `DB_Reservas`  
**Protocolo adicional:** gRPC (cliente) + RabbitMQ (consumidor)

#### Controllers y endpoints

| Controller | Endpoints |
|---|---|
| `ReservasController` | GET por id, por cliente, por alojamiento; POST crear; PATCH estado |

Endpoints disponibles:
```
GET    /api/v1/reservas/{id}
GET    /api/v1/reservas/cliente/{clienteId}
GET    /api/v1/reservas/resumen/cliente/{clienteId}
GET    /api/v1/reservas/alojamiento/{alojamientoId}
GET    /api/v1/reservas/resumen/alojamiento/{alojamientoId}
POST   /api/v1/reservas
PATCH  /api/v1/reservas/{id}/estado
```

#### Entidades principales

**Descuentos** — `DescuentoId`, `Codigo`, `Porcentaje`, `Activo`

**Reservas**
- `ReservaId`, `DescuentoId`, `ClienteId` (ref lógica), `AlojamientoId` (ref lógica), `FechaCheckIn`, `FechaCheckOut`, `Estado`, `MontoTotal`, timestamps

**ReservaDetalleHabitacion** — detalle de qué habitaciones incluye cada reserva

#### Flujo de creación de reserva

1. Validar que `FechaCheckOut > FechaCheckIn`.
2. Si hay código de descuento, verificar que sea válido y activo.
3. Para cada habitación solicitada, llamar a Alojamientos por **gRPC** (`VerificarDisponibilidad`).
4. Persistir la reserva en DB_Reservas.
5. Para cada habitación confirmada, llamar a Alojamientos por **gRPC** (`BloquearFechas`).

#### Consumidor de eventos

Consume `FacturaPagadaEvent` desde RabbitMQ via MassTransit (`FacturaPagadaConsumer`). Cuando se recibe el evento, actualiza el estado de la reserva a "Pagada".

---

### 4.5 Microservicio Facturación

**Proyecto:** `Microservices/Facturacion`  
**Base de datos:** `DB_Facturacion`  
**Protocolo adicional:** RabbitMQ (publicador)

#### Controllers y endpoints

| Controller | Endpoints |
|---|---|
| `FacturasController` | CRUD de facturas, registro de pagos |
| `MetodosPagoController` | Consulta de métodos de pago disponibles |

#### Entidades principales

**MetodosPagoCliente** — `MetodoPagoId`, `Tipo` (DEBITO / CREDITO / EnSitio)

**Facturas**
- `FacturaId`, `ReservaId` (ref lógica), `MetodoPagoId`, `Monto`, `Estado` (Aprobado / Pendiente / Rechazado), `FechaPago`, timestamps

**DetalleFacturas** — `DetalleFacturaId`, `FacturaId`, `Descripcion`, `Cantidad`, `PrecioUnitario`

**AuditoriaGeneral** — tabla de auditoría transversal con `NombreTabla`, `Operacion`, y contexto de cambio

#### Publicación de eventos

Al registrar un pago aprobado, publica `FacturaPagadaEvent` a RabbitMQ:

```csharp
public record FacturaPagadaEvent
{
    public int ReservaId { get; init; }
    public int FacturaId { get; init; }
    public decimal MontoPagado { get; init; }
    public DateTime FechaPago { get; init; }
}
```

---

## 5. Comunicación entre microservicios

### 5.1 gRPC — Calendario

**Dirección:** Reservas → Alojamientos (cliente → servidor)  
**Contrato:** `Shared/Shared.Protos/Protos/calendario.proto`

```protobuf
syntax = "proto3";
option csharp_namespace = "Shared.Protos";
package calendario;

service CalendarioGrpc {
  rpc VerificarDisponibilidad (DisponibilidadRequest) returns (DisponibilidadResponse);
  rpc BloquearFechas (BloqueoFechasRequest) returns (BloqueoFechasResponse);
}

message DisponibilidadRequest {
  int32 habitacion_id = 1;
  string fecha_inicio = 2; // "YYYY-MM-DD"
  string fecha_fin    = 3; // "YYYY-MM-DD"
}

message DisponibilidadResponse {
  bool   disponible = 1;
  string mensaje    = 2;
}

message BloqueoFechasRequest {
  int32  habitacion_id = 1;
  string fecha_inicio  = 2;
  string fecha_fin     = 3;
}

message BloqueoFechasResponse {
  bool   exito   = 1;
  string mensaje = 2;
}
```

El servicio gRPC en Alojamientos se registra con `GrpcWeb` habilitado:
```csharp
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.MapGrpcService<CalendarioGrpcService>().EnableGrpcWeb();
```

### 5.2 Event Bus — RabbitMQ + MassTransit

**Dirección:** Facturación publica → Reservas consume  
**Evento:** `FacturaPagadaEvent` (definido en `Shared/Shared.Kernel`)  
**Broker en producción:** CloudAMQP (`amqps://user:pass@host/vhost`)

Configuración en ambos microservicios:

```csharp
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<FacturaPagadaConsumer>(); // solo en Reservas
    x.UsingRabbitMq((context, cfg) =>
    {
        var rmqUrl = builder.Configuration.GetConnectionString("RabbitMQ");
        if (!string.IsNullOrEmpty(rmqUrl))
            cfg.Host(new Uri(rmqUrl));
        else
            cfg.Host("localhost", "/", h => { h.Username("guest"); h.Password("guest"); });
        cfg.ConfigureEndpoints(context);
    });
});
```

---

## 6. Capa Shared

Dos proyectos de librería compartidos, sin lógica de negocio:

### Shared.Kernel

Contiene los eventos del dominio compartidos entre microservicios:
- `FacturaPagadaEvent` — publicado por Facturación, consumido por Reservas.

### Shared.Protos

Contiene el archivo `.proto` del contrato gRPC:
- `Protos/calendario.proto` — contrato `CalendarioGrpc`.

El proyecto `Shared.Protos.csproj` referencia el paquete `Grpc.AspNetCore` y genera los stubs de C# en tiempo de build. Tanto Alojamientos (servidor) como Reservas (cliente gRPC) referencian este proyecto.

---

## 7. Bases de datos

Cuatro bases de datos PostgreSQL **completamente independientes**, una por microservicio. No hay foreign keys entre bases de datos; las referencias cruzadas son "referencias lógicas" por ID.

Todas usan:
- `EF Core` con `UseLowerCaseNamingConvention()` (nombres de tabla y columna en minúsculas en PostgreSQL)
- Triggers automáticos para actualizar `FechaModificacion`
- Stored procedures para operaciones complejas

### DB_Usuarios (Script 01)

| Tabla | Columnas clave |
|---|---|
| `Localizaciones` | `LocalizacionId`, `Area` (POLYGON), `Descripcion` |
| `Usuarios` | `UsuarioId`, `Rol`, `Email`, `PasswordHash`, `NombreCompleto`, `Estado` |
| `Clientes` | `ClienteId`, `UsuarioId` (FK), `Cedula`, `FotoUrl`, `Telefono`, `Domicilio`, `TotalReservas` |

SP: `sp_registrar_cliente(nombre, email, cedula, telefono, domicilio, usuarioId)`

### DB_Alojamientos (Script 02)

| Tabla | Columnas clave |
|---|---|
| `TiposAlojamiento` | `TipoAlojamientoId`, `Nombre`, `Descripcion` |
| `Alojamientos` | `AlojamientoId`, `SocioId`*, `TipoAlojamientoId`, `Ciudad`, `Nombre`, `Coordenadas` (POINT), `Estrellas`, `CalificacionPromedio`, `Estado` |
| `AlojamientoFotos` | `FotoId`, `AlojamientoId`, `Url`, `Orden` |
| `Habitaciones` | `HabitacionId`, `AlojamientoId`, `Nombre`, `CapacidadAdultos`, `PrecioNoche`, `SuperficieM2` |
| `CalendarioDisponibilidad` | `CalendarioId`, `HabitacionId`, fechas bloqueadas, tipo bloqueo |

`*` Referencia lógica a `DB_Usuarios.Usuarios`

### DB_Reservas (Script 03)

| Tabla | Columnas clave |
|---|---|
| `Descuentos` | `DescuentoId`, `Codigo`, `Porcentaje`, `Activo` |
| `Reservas` | `ReservaId`, `ClienteId`*, `AlojamientoId`*, `FechaCheckIn`, `FechaCheckOut`, `Estado`, `MontoTotal` |
| `ReservaDetalleHabitacion` | `DetalleId`, `ReservaId`, `HabitacionId`* |

Función SQL: `fn_calcular_noches(DATE, DATE)`  
SP: `sp_asignar_codigo_reserva(INT)`

### DB_Facturacion (Script 04)

| Tabla | Columnas clave |
|---|---|
| `MetodosPagoCliente` | `MetodoPagoId`, `Tipo` (DEBITO / CREDITO / EnSitio) |
| `Facturas` | `FacturaId`, `ReservaId`*, `MetodoPagoId`, `Monto`, `Estado`, `FechaPago` |
| `DetalleFacturas` | `DetalleFacturaId`, `FacturaId`, `Descripcion`, `Cantidad`, `PrecioUnitario` |
| `AuditoriaGeneral` | `AuditoriaId`, `NombreTabla`, `Operacion`, timestamps de auditoría |

SP: `sp_registrar_factura_completa(reservaId, metodoPagoId, monto, descripcion)`

---

## 8. Servicios externos

### Cloudinary

Almacenamiento de imágenes de alojamientos. Las fotos se suben directamente desde el frontend Angular usando un preset `unsigned` (`locushome_uploads`). El backend también puede subir imágenes via `CloudinaryUploadService` en el microservicio Alojamientos.

Cloud name: `dzxkufyfp`

### CloudAMQP

Instancia gestionada de RabbitMQ en la nube. Se configura via connection string `amqps://...` en `appsettings.json` o variable de entorno. Fallback a `localhost:5672` si no está configurado.

### Render.com

Plataforma de despliegue de todos los servicios:

| Servicio | URL en Render |
|---|---|
| API Gateway | `israel-apigateway.onrender.com` |
| Usuarios API | `israel-usuarios-api.onrender.com` |
| Alojamientos API | `israel-alojamientos-api.onrender.com` |
| Reservas API | `israel-reservas-api.onrender.com` |
| Facturación API | `israel-facturacion-api.onrender.com` |
| Backend monolítico (alt) | `bookingdb-cloud.onrender.com` |
| Frontend | Puerto 5173 vía Nginx |

---

## 9. Infraestructura y despliegue

### Docker Compose (desarrollo local)

```yaml
services:
  api:
    build: { dockerfile: Dockerfile.api }
    ports: ["5028:5028"]
    environment: { ASPNETCORE_ENVIRONMENT: Development }

  frontend:
    build: { dockerfile: Dockerfile.front }
    ports: ["5173:80"]
    depends_on: [api]
```

Solo levanta el API Gateway y el Frontend. Los microservicios individuales tienen sus propios `Dockerfile.*` en `AlojamientoPrototipo/`:

- `Dockerfile.Alojamientos`
- `Dockerfile.Reservas`
- `Dockerfile.Facturacion`
- `Dockerfile.Usuarios`
- `Dockerfile.ApiGateway`

### Nginx (`nginx.conf`)

Sirve el frontend Angular como SPA (`try_files $uri /index.html`) y hace proxy de las llamadas `/api` hacia `bookingdb-cloud.onrender.com` en producción:

```nginx
location / {
    root /usr/share/nginx/html;
    try_files $uri $uri/ /index.html;
}
location /api {
    proxy_pass https://bookingdb-cloud.onrender.com;
    proxy_ssl_server_name on;
}
```

### Variables de entorno relevantes

| Variable | Descripción |
|---|---|
| `ConnectionStrings:ConexionUsuarios` | Connection string PostgreSQL para Usuarios |
| `ConnectionStrings:ConexionAlojamientos` | Connection string PostgreSQL para Alojamientos |
| `ConnectionStrings:ConexionReservas` | Connection string PostgreSQL para Reservas |
| `ConnectionStrings:ConexionFacturacion` | Connection string PostgreSQL para Facturación |
| `ConnectionStrings:RabbitMQ` | URL amqps de CloudAMQP |
| `ASPNETCORE_ENVIRONMENT` | Development / Production |

### Scripts de utilidad

- `migrate_usuarios.ps1` — ejecuta migraciones EF Core para el microservicio Usuarios
- `GenerarDocs.ps1` — genera documentación HTML desde la solución
- `.tmp-bcrypt/` — herramienta auxiliar para generar hashes bcrypt durante desarrollo

---

## 10. Patrones y convenciones

### Clean Architecture

Cada microservicio respeta la separación en capas con dependencias en una sola dirección:

```
API → Business → DataManagement → DataAccess
```

La capa `API` solo conoce `Business`. La capa `Business` solo conoce `DataManagement`. La capa `DataAccess` no depende de ninguna otra capa del microservicio.

### Repository Pattern

`RepositoryBase<T>` provee operaciones CRUD genéricas. Cada entidad tiene su propio `IRepository` con queries específicas. Ejemplo en Usuarios:
- `IClientesRepository` — `GetByEmail`, `GetByCedula`, `GetByUsuarioId`
- `IUsuariosRepository` — `GetByEmail`, autenticación

### Unit of Work

`IUnitOfWork` centraliza la transacción y el `SaveChanges()`. Los servicios de negocio llaman a `_unitOfWork.SaveChangesAsync()` al final de operaciones que modifican estado.

### Mappers explícitos

No se usa AutoMapper. Cada capa tiene sus propios mappers estáticos:
- `Business/Mappers/` — mapea de `DataModel` a `DTO` (respuesta al cliente)
- `DataManagement/Mappers/` — mapea de `Entity` a `DataModel`

### Manejo de errores

`ExceptionHandlingMiddleware` en cada microservicio captura excepciones de dominio (definidas en `Business/Exceptions/`) y las convierte en respuestas HTTP con `ApiErrorResponse`. Las excepciones de dominio típicas son: `*NotFoundException`, `FechasInvalidasException`, `DescuentoInvalidoException`, etc.

### Versionado de API

Todos los controllers usan el prefijo `api/v1/[controller]` en las rutas. La convención está presente en las anotaciones `[Route("api/v1/[controller]")]`.

### Swagger

Habilitado en todos los microservicios incondicionalmente (sin restricción de entorno), por ser un prototipo. El API Gateway también expone un Swagger con el contrato público.

---

*Documentación generada el 5 de junio de 2026 desde el código fuente del repositorio `BackEndLocusHome-main`.*
