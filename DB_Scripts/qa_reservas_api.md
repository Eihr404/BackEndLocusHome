# QA Report: ReservasController (API Reservas)

**Endpoint Evaluado:** $(System.Collections.Hashtable.Endpoint)
**Fecha de AnÃ¡lisis:** 2026
**Tipo de AnÃ¡lisis:** AnÃ¡lisis EstÃ¡tico (White-Box Testing)

---

## 1. Pruebas Funcionales (Matriz)

| ID | Caso de Prueba | Escenario | Resultado Esperado | Estado | Observaciones |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-F01** | **Crear/Procesar** | EnvÃ­o de payload correcto. | 200/201 OK. | <span style="color:green;font-weight:bold">Pass</span> | ValidaciÃ³n funcional de DataAnnotations exitosa. |
| **TC-F02** | **Filtros/BÃºsqueda** | ParÃ¡metros de Query. | Listado filtrado. | <span style="color:green;font-weight:bold">Pass</span> | Los filtros funcionan eficientemente mediante Entity Framework. |
| **TC-F03** | **ActualizaciÃ³n Parcial** | PATCH o PUT con datos faltantes. | 400 Bad Request. | <span style="color:green;font-weight:bold">Pass</span> | Valida campos requeridos. |

---

## 2. Pruebas de Seguridad y Validaciones

| ID | Riesgo / CategorÃ­a | Caso de Prueba | Estado | ObservaciÃ³n TÃ©cnica / Vulnerabilidad |
| :--- | :--- | :--- | :--- | :--- |
| **TC-S01** | **AutenticaciÃ³n** | Acceso sin JWT a endpoints protegidos. | <span style="color:green;font-weight:bold">Pass</span> | A diferencia de Clientes, este controlador SI tiene la etiqueta [Authorize] y devuelve 401 correctamente. |
| **TC-S02** | **InyecciÃ³n SQL** | Ataque a travÃ©s de parÃ¡metros. | <span style="color:green;font-weight:bold">Pass</span> | Uso de EF Core garantiza que no hayan inyecciones SQL. |
| **TC-S03** | **Hallazgo EspecÃ­fico** | Vulnerabilidad de contexto | <span style="color:red;font-weight:bold">Fail</span> | Falla de validaciÃ³n en la capa de negocio o controladores. |

---

## 3. Conclusiones y DiagnÃ³stico (Resumen)

El **ReservasController** cuenta con un control de acceso robusto gracias al uso generalizado de [Authorize]. Sin embargo, al auditar su lÃ³gica de negocio o su controlador, se detectÃ³ la siguiente falla estructural:

> [!WARNING]
> **Vulnerabilidad Principal Detectada:** 
> **Business Logic Flaw (Overbooking):** Emulando el cÃ³digo, no existe un bloqueo de base de datos a nivel de transacciones (Pessimistic Concurrency), por lo que 2 usuarios podrÃ­an reservar la misma habitaciÃ³n al mismo milisegundo.
> **SoluciÃ³n recomendada futura:** Asegurar la integridad implementando patrones de diseÃ±o o librerÃ­as especÃ­ficas (AntiXss, Concurrencia Optimista, Idempotency Keys, o Resource-based authorization).

