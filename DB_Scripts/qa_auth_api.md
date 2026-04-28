# QA Report: AuthController (API Autenticación)

**Endpoint Evaluado:** `/api/v1/auth/login`
**Fecha de Análisis:** 2026
**Tipo de Análisis:** Análisis Estático (White-Box Testing)

---

## 1. Pruebas Funcionales (Matriz)

| ID | Caso de Prueba | Escenario | Resultado Esperado | Estado Code-Review | Observaciones |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-F01** | **Login Exitoso** | Credenciales correctas y cuenta Activa. | 200 OK + Payload con `Token` y `Expiracion` (8 horas). | <span style="color:green;font-weight:bold">Pass</span> | El JWT se genera correctamente leyendo el RolId y mapeando los Claims estándar. |
| **TC-F02** | **Credenciales Inválidas** | Email existe pero la contraseña no coincide. | Error 400. | <span style="color:green;font-weight:bold">Pass</span> | El servicio arroja un `BusinessException("Credenciales inválidas")` capturado por el Middleware. |
| **TC-F03** | **Campos Vacíos** | Payload sin Email o Password. | Error 400. | <span style="color:green;font-weight:bold">Pass</span> | Valorado exitosamente mediante `string.IsNullOrWhiteSpace` que lanza `ValidationException`. |
| **TC-F04** | **Cuenta Inactiva** | Credenciales correctas pero `Estado == false`. | Error 400 indicando inactividad. | <span style="color:green;font-weight:bold">Pass</span> | El código bloquea el acceso si la cuenta fue suspendida. Excelente control. |

---

## 2. Pruebas de Seguridad y Validaciones

| ID | Riesgo / Categoría | Caso de Prueba | Estado Code-Review | Observación Técnica / Vulnerabilidad |
| :--- | :--- | :--- | :--- | :--- |
| **TC-S01** | **Confidencialidad** | Almacenamiento de contraseñas. | <span style="color:green;font-weight:bold">Pass</span> | Contraseñas protegidas mediante Hash Nativo `SHA256` + Base64 en `AuthService`. |
| **TC-S02** | **Ataque Brute Force** | Intentos ilimitados de Login (Fuerza bruta). | <span style="color:red;font-weight:bold">Fail</span> | **CRÍTICO:** No existe Rate Limiting (`[EnableRateLimiting]`) en el controlador ni bloqueo temporal de cuentas (Lockout) en base de datos. |
| **TC-S03** | **Interceptación (MITM)** | Tránsito de datos en texto plano. | <span style="color:green;font-weight:bold">Pass</span> | ASP.NET Core tiene `app.UseHttpsRedirection()` activo globalmente. |
| **TC-S04** | **Data Leak** | Errores en BD arrojan stack trace al cliente. | <span style="color:green;font-weight:bold">Pass</span> | El `ExceptionHandlingMiddleware` intercepta errores no controlados y oculta el Trace en Producción. |

---

## 3. Conclusiones y Diagnóstico (Resumen)

El **AuthController** cumple de forma brillante con los requisitos funcionales básicos. La generación del token y el hash de contraseñas utilizan los estándares modernos de `.NET 8` (`System.Security.Cryptography.SHA256`).

> [!WARNING]
> **Vulnerabilidad Principal Detectada:** 
> El Endpoint `/login` está actualmente expuesto a ataques de diccionario o fuerza bruta. Debido a que no hay límite de peticiones (Rate Limiting), un hacker puede hacer 50,000 requests por minuto intentando adivinar contraseñas de correos expuestos sin que el servidor lo bloquee.
> **Solución recomendada futura:** Implementar un middleware de Rate Limit (`AddRateLimiter`) enfocado al AuthController o añadir una tabla de `IntentosFallidos` en la base de datos.
