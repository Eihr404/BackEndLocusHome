Aquí tienes el **Plan de Implementación Mejorado para el Reto 3** : 

Fase 1: El Supergrafo (GraphQL) y la Experiencia Móvil 

Tu plan actual asume que seguirás usando solo REST/Ocelot para el frontend. La regla estricta del Reto 3 es migrar a móvil usando GraphQL para optimizar las peticiones. 

- **Paso 1.1: Implementar Apollo Federation (HotChocolate en .NET):** No vas a eliminar tu API Gateway Ocelot (que seguirá sirviendo a tu Angular Admin web), pero debes agregar un **Gateway de GraphQL** . Cada uno de tus microservicios actuales (MS.Catalogo, MS.Alquiler, etc.) debe exponer un "subgrafo". 

- **Paso 1.2: Construir el Supergrafo:** El Gateway de GraphQL unirá estos subgrafos. Cuando la app móvil consulte una reserva, el Supergrafo pedirá los datos del vehículo a Catálogo vía gRPC/REST y los datos de la reserva a Alquiler, armando un solo JSON de respuesta para evitar el _overfetching_ . 

- **Paso 1.3: Desarrollo Móvil (Flutter o React Native):** El marketplace público de _DriveX_ debe migrarse obligatoriamente a una app móvil. **Regla de oro:** Esta app móvil _solo_ debe comunicarse con el Gateway de GraphQL, nunca con los microservicios individuales. 

Fase 2: Sagas, RabbitMQ y Patrones de Fiabilidad 

Tu plan (implementation_planRDA3.md) ya contempla Sagas Coreografiadas con RabbitMQ, lo cual es perfecto, pero le falta la capa de resiliencia exigida. 

- **Paso 2.1: Implementar Idempotencia:** Debes configurar tus consumidores de RabbitMQ para que, si el broker entrega un evento de PagoAprobado dos veces, tu base de datos (Supabase) lo detecte por el ID del mensaje y no procese el pago doblemente. 

- **Paso 2.2: Sagas con Compensación (Rollback):** Tu flujo propuesto es correcto. Si Alquiler emite ReservaCreadaEvent y el Pago falla, se debe emitir un PagoRechazadoEvent. Catálogo debe escuchar este fallo para **liberar el vehículo bloqueado** inmediatamente. 

- **Paso 2.3: Circuit Breaker y Reintentos (Polly):** Para las llamadas entre microservicios, debes instalar la librería _Polly_ en .NET. Si un microservicio cae, el "Cortacircuitos" se abre para evitar fallos en cascada y permitir que el sistema siga operando degradadamente (Fallback). 

- **Paso 2.4: Trazabilidad (Correlation ID):** Mantén tu plan de inyectar el X- Correlation-ID desde el Gateway hacia los metadatos de gRPC y headers de 

RabbitMQ. El profesor evaluará que un log en Supabase MS.Monitoreo tenga el mismo ID desde que inició en el móvil hasta que terminó en la BD. 

Fase 3: Integración SOA / Legacy (El Conector SRI) 

**¡Esta es la omisión más grande de tu plan actual!** El Reto 3 exige demostrar interoperabilidad multi-dominio con tecnologías heterogéneas simulando sistemas legados. 

- **Paso 3.1: Crear el Gateway SOA:** Crea un nuevo proyecto pequeño (o añade un endpoint específico) que actúe como servicio SOAP. 

- **Paso 3.2: Generar Facturación Electrónica (XML):** Cuando el 

   - microservicio de Alquiler confirme un pago exitoso a través de RabbitMQ, este servicio SOA tomará los datos, los transformará obligatoriamente a un documento **XML** y los enviará vía protocolo **SOAP** usando un contrato **WSDL** simulando la comunicación con el SRI o el Ministerio. 

Fase 4: Despliegue Híbrido Definitivo 

En tu plan mencionas migrar de Azure a Kubernetes (Minikube). Aunque Kubernetes es el estándar de la industria, el Reto 3 no exige explícitamente K8s. 

- **Recomendación:** Ya tienes la infraestructura montada en **Azure Container Apps** con deploy-microservicios.ps1. En lugar de gastar tiempo armando manifiestos de K8s (deployment.yaml), es mejor que despliegues tus nuevos contenedores (RabbitMQ, GraphQL Gateway y el servicio SOAP) directamente como nuevas Azure Container Apps para mantener el ecosistema unificado y asegurar que esté "desplegado públicamente en Internet" como exige la rúbrica. 

**Resumen de la Arquitectura Híbrida a entregar:** Tu documento final mostrará que el **Admin Web (Angular)** usa **REST/Ocelot** , la **App Móvil** usa **GraphQL** , los microservicios hablan internamente con **gRPC** (alto rendimiento), las transacciones asíncronas fluyen por **RabbitMQ** (EDA/Sagas), y la facturación sale por **SOAP/XML** . 

Aquí tienes la documentación inicial y las definiciones estratégicas que debemos armar juntos antes de programar, basadas estrictamente en las exigencias de tus grabaciones: 

1. El Catálogo de Eventos y Topología (Regla de reto3.m4a) 

En el audio del Reto 3, el profesor lee una lista exacta de **"Resumen de orden de decisión de diseño"** que se debe hacer antes de codificar con RabbitMQ. Debemos documentar lo siguiente: 

   - **Catálogo de Eventos:** Definir una tabla exacta con el Evento (ej. ReservaCreadaEvent), quién es el Productor (Microservicio Alquiler) y quiénes son los Consumidores (Microservicio Catálogo y Financiero). 

   - **El Contrato del Mensaje (Payload):** El profesor exige que el mensaje JSON que viaje por RabbitMQ tenga una estructura base estricta que incluya el MessageId, el EventSource y el CorrelationId. 

   - **Topología del Exchange:** El profesor sugiere decidir si usaremos un solo _Exchange_ global (recomendado por simplicidad) o uno por microservicio. 

   - **Políticas de Fiabilidad:** Definir explícitamente que usaremos colas _Dead_ 

   - durables (durable: true), confirmación manual (Manual ACK con _Letter Queues_ para mensajes fallidos) y la estrategia de idempotencia basada en el MessageId. 

2. Diseño del Supergrafo y las "Keys" de GraphQL (graphql2.m4a) 

Para la Federación de Apollo (GraphQL), el profesor aclara que cada microservicio es un "subgrafo" y la magia de unirlos se da a través de llaves y extensiones (@key y extend). 

   - **Definición de Entidades Federadas:** Antes de codificar HotChocolate en C#, debemos documentar cómo se van a relacionar tus tablas separadas. Por ejemplo, documentar que el subgrafo de _Alquiler_ extenderá la entidad Vehiculo usando su ID como llave (@key(fields: "id")) para que el Gateway pueda unir los datos de la reserva con los datos del catálogo sin que las bases de datos estén físicamente conectadas. 

3. El Contrato WSDL para SOA/SRI (soa.m4a y graphql2.m4a) 

Tal como aprendimos en el Reto 1 y 2 con OpenAPI y gRPC (Proto3), el enfoque de la asignatura es _API-First_ . En SOA, **el contrato se escribe antes que el código** . 

- **Definición del WSDL:** El profesor indica que para SOA, "el cliente realmente a dónde se conecta... es al contrato, es decir, el WSDL". Antes de programar el servicio SOAP para la facturación electrónica del SRI, 

debemos generar el documento XML (WSDL) que describe las operaciones (GenerarFactura), los parámetros de entrada y los tipos de datos que enviaremos. 

4. La Arquitectura de Coexistencia (Regla Híbrida) 

En graphql2.m4a, el profesor hace un fuerte énfasis en que "no es que ya no van a hacer REST, no es que ya no van a hacer gRPC, tienen que todo combinarlo". 

- **Mapa de Enrutamiento Híbrido:** Debemos documentar (o tener claro) que tu frontend móvil consumirá el **Gateway GraphQL** (1 solo endpoint), mientras que tu aplicación web administrativa seguirá consumiendo **REST** , los microservicios hablarán entre sí vía **gRPC** de forma síncrona o vía **RabbitMQ** de forma asíncrona, y la facturación saldrá por **SOA** . 

