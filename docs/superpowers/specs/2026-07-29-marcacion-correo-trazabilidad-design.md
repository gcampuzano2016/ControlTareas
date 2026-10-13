# Marcación entrada/salida: correo de confirmación y corrección de trazabilidad — Diseño

**Fecha:** 2026-07-29
**Estado:** Aprobado (diseño)
**Rama:** `ProyectoNuevosCambios`.

## Objetivo

Que cada usuario reciba un **correo de confirmación** al registrar su entrada y su salida, y que ese correo confirme algo **cierto**: corregir los tres defectos que hoy hacen que el registro de marcación no sea confiable.

## Contexto y defectos (verificados en código y en datos de producción)

Flujo actual: los botones "Entrada"/"Salida" viven en `Master.Master`; `RegistroLaboral.js` (`RegistrarEvento`) postea a `ObtenerListaTareas.ashx` → `RegistrarEvento` → SP `InsertarModificarEliminarRegistroBiometrico` → tabla `RegistroBiometrico`.

Defectos que este trabajo corrige:

1. **El servidor ignora qué botón se presionó.** `RegistroLaboral.js` envía `Accion: 1|2`, pero `ObtenerListaTareas.ashx.cs:6040` (`RegistrarEvento`) nunca lee ese parámetro: siempre fija `Tipo=1` y deja que el SP deduzca la intención según exista o no fila del día. Quien presiona "Salida" sin haber marcado entrada queda registrado como **entrada**. El bloqueo de botones de `RegistroLaboral.js:159-182` es solo del navegador.
2. **Falla silenciosa.** Si el usuario ya marcó entrada y salida, ninguna rama del SP se ejecuta, `@ESTADO_EJECUCION` queda en `-1`, y el `CASE` de mensajes no tiene `ELSE` → devuelve mensaje `NULL`. El JS solo contempla `estado=="1"` y `estado=="0"`, así que con `-1` **no muestra nada**.
3. **Éxito falso.** En la rama de salida, el `EXISTS` no filtra por `IdProceso` pero el `UPDATE` sí (`WHERE Id_Usuario=@Id_Usuario AND IdProceso=@IdProceso`), y ese `IdProceso` lo envía el cliente. Si llega desfasado o en 0 se actualizan **0 filas** y aun así se responde `EL REGISTRO GUARDADO EXITOSAMENTE`.

Estado medido en `RegistroBiometrico` (17.822 filas, 22/08/2025 → 28/07/2026):

| Hallazgo | Casos |
|---|---|
| Días con registros duplicados (mismo usuario, mismo día) | 31 |
| Filas con entrada sin salida (`1900-01-01`) | 3.065 |
| Filas con salida anterior a la entrada | 5 |
| `FechaRegistro` y `FechaEntrada` en días distintos | 6 |
| Filas con `Observacion` (edición manual) sin rastro de autor | 42 |

La tabla solo tiene PK en `IdProceso`: sin índice único por usuario+día y sin columnas de auditoría.

## Alcance

**Dentro:** correo de confirmación en entrada y salida; corrección de los defectos 1, 2 y 3.

**Fuera (acordado explícitamente):**
- No se limpian los duplicados ni las salidas inválidas ya existentes.
- No se agregan índice único ni columnas de auditoría (quién/IP/fecha de modificación).
- No se cierra el hueco de autorización de `RegistrarEvento2`/`RegistrarEvento3`, que reciben el código de usuario **en texto plano** desde el request (`ObtenerListaTareas.ashx.cs:6112`) y permiten registrar o modificar marcaciones a nombre de cualquier usuario. **Es el hallazgo más grave de la revisión y merece su propio ciclo.**
- La lógica de almuerzo (`FechaAlmorzar`, `FechaRegAlmorzar`) sigue comentada en el SP viejo y sin uso.

## Decisiones de diseño (acordadas)

- **SP y handler nuevos, sin tocar lo compartido.** El SP `InsertarModificarEliminarRegistroBiometrico` lo usan tanto el flujo de botones (`Tipo=1`) como la carga/edición manual (`Tipo=2`) que alimenta reportes. Se deja intacto.
- **El servidor es el único dueño de la decisión.** Recibe `@Accion` explícita, resuelve la fila del día por `Id_Usuario` + fecha, y toma la hora de `GETDATE()`.
- **El `IdProceso` del cliente desaparece del flujo de escritura.** Es lo que elimina el éxito falso.
- **Correo no bloqueante**, disparado solo tras éxito confirmado del SP.

## Arquitectura

### 1. Stored procedure nuevo

```
Sp_RTA_RegistrarMarcacion(@Id_Usuario NUMERIC(6,0), @Accion INT)   -- 1=entrada, 2=salida
  → Respuestas INT, Mensaje VARCHAR(300), FechaHora DATETIME, IdProceso BIGINT
```

La fila del día se identifica por `Id_Usuario` + `CONVERT(DATE, FechaRegistro) = CONVERT(DATE, GETDATE())`, leída con `UPDLOCK, HOLDLOCK` dentro de la transacción para que dos clics simultáneos no puedan crear dos filas.

| Acción | Estado actual | Resultado |
|---|---|---|
| Entrada (1) | sin fila hoy | `INSERT` con `FechaEntrada = GETDATE()`, resto de fechas en `1900-01-01`, `FechaRegistro = GETDATE()`, `Estado = 1` → `Respuestas=1`, *"Entrada registrada a las HH:mm."* |
| Entrada (1) | ya hay fila hoy | `Respuestas=0`, *"Ya registro su entrada hoy a las HH:mm."* |
| Salida (2) | sin fila hoy | `Respuestas=0`, *"Debe registrar primero su entrada."* |
| Salida (2) | fila con `FechaSalida = 1900-01-01` | `UPDATE FechaSalida = GETDATE()` sobre el `IdProceso` resuelto en el servidor → `Respuestas=1`, *"Salida registrada a las HH:mm."* |
| Salida (2) | `FechaSalida` ya registrada | `Respuestas=0`, *"Ya registro su salida hoy a las HH:mm."* |
| Cualquiera | `@Accion` fuera de {1,2} | `Respuestas=0`, *"Accion no valida."* |

Reglas transversales:
- El SP **siempre** devuelve `Respuestas` en 1 o 0 **con `Mensaje` no vacío**. Nunca el `-1` mudo actual.
- `IdProceso` y `FechaHora` viajan en la respuesta para diagnóstico: quedan en `EntMarcacion` y permiten rastrear qué fila se afectó, pero **ningún consumidor los usa** — el JS solo muestra `Mensaje`. No agregar lógica de cliente que dependa de ellos.
- `TRY/CATCH` con `ROLLBACK`; ante excepción devuelve `Respuestas=0` y el detalle del error.
- Toda hora sale de `GETDATE()` de SQL: una sola fuente de tiempo. Hoy se mezcla con `DateTime.Now` del servidor web, origen probable de las 6 filas con días cruzados.
- Mensajes fijos **sin tildes**, por la convención del proyecto.

### 2. Capas C#

- **`CapaEntidad/EntMarcacion.cs`**: `{ int Accion; int Respuestas; string Mensaje; DateTime FechaHora; long IdProceso; }`
- **`CapaDato/DaoMarcacion.cs`**: `RegistrarMarcacion(decimal idUsuario, int accion)` → `EntMarcacion`. Conexión vía `DaoReporTareaAranda`, parámetros tipados, `using` en conexión y comando.
- **`CapaNegocio/NegMarcacion.cs`**: pasarela, mismo patrón que `NegMenuPerfil`.

Los tres archivos se registran con `<Compile Include>` en sus `.csproj`.

### 3. Handler

**`ReporteTareas/Formulario/AdministrarMarcacion.ashx`** (+ `.ashx.cs`), acción `RegistrarMarcacion` con parámetros `{session, Accion}`:

1. `seguridad.Desencripta(campos["session"])` → `NegUsuario.RTAConsultaUsuarioPorCodigo(...)` para obtener `Id_Usuario`, `E_Mail` y `Nom_Usuario`. La identidad **nunca** viene en texto plano.
2. Valida que `Accion` sea 1 o 2; si no, responde rechazo sin tocar la BD.
3. Llama a `NegMarcacion.RegistrarMarcacion(...)`.
4. Si `Respuestas == 1` y `E_Mail` no está vacío, encola el correo (sección 4).
5. Responde `EntRespuesta { estado, mensaje, tipoMensaje }` — `estado="1"` en éxito, `"0"` en rechazo o error.
6. `context.Response.ContentEncoding = Encoding.UTF8` y `Charset = "utf-8"`, como el resto de handlers nuevos.

Se registra en `ReporteTareas.csproj` como `<Content Include>` + `<Compile Include>` con `<DependentUpon>`.

### 4. Correo

Método nuevo en `ReporteTareas/clases/EnvioCorreoHelper.cs`, con el mismo molde que `EnvioCorreoPermiso`:

```csharp
public bool EnvioCorreoMarcacion(string correoDestino, string nombreUsuario,
                                 int accion, DateTime fechaHora)
```

Lee los parámetros SMTP con `NegParametrosConfiguracion.RTA_ValorParametroConfiguracion(...)` (`smtpAddress`, `emailFrom`, `emailFromName`, `password`, `portNumber`, `enableSSL`) y delega en `EnviarCorreo(...)`. **No se agrega configuración nueva.**

- **Asunto:** `Registro de entrada - 29/07/2026 08:23` (o `Registro de salida - ...`).
- **Cuerpo:** nombre del usuario, tipo de marcación, fecha y hora tal como quedaron guardadas, y una línea final indicando que si no reconoce el registro contacte a RRHH. Sin adjuntos y sin datos sensibles.
- La hora del correo es la que **devolvió el SP**, no una calculada aparte.

**Envío no bloqueante**, con tres condiciones obligatorias:

1. Los datos del correo se **capturan en variables locales antes** de encolar. En el hilo del `ThreadPool` no existe `HttpContext.Current`; nada puede leerse de la sesión ahí.
2. El delegado va envuelto en un `try/catch` **total**. Una excepción sin capturar en un hilo del `ThreadPool` tumba el worker process de ASP.NET.
3. Solo se encola si `Respuestas == 1` y `E_Mail` no está vacío.

### 5. Frontend

`ReporteTareas/js/RegistroLaboral.js`:
- `RegistrarEvento(Accion)` apunta a `AdministrarMarcacion.ashx` y **deja de enviar `IdProceso`**.
- El manejo de respuesta ya existente sirve tal cual: `estado=="1"` → `MensajeCorrecto`, `estado=="0"` → `MensajeIncorrecto`. Como el SP nuevo siempre devuelve 1 o 0 con mensaje, la falla silenciosa desaparece sin cambiar esa lógica.
- `ConsultarEvento` **no se toca**: es solo lectura y sigue en `ObtenerListaTareas.ashx`.
- El deshabilitado de botones en cliente se conserva como comodidad; la regla real ahora la aplica el servidor.

## Manejo de errores

| Situación | Qué ve el usuario | Qué pasa con el registro |
|---|---|---|
| SMTP caído o lento | Confirmación normal | **Se guarda igual**; el fallo se traga en el hilo aparte |
| Usuario sin `E_Mail` | Confirmación normal | Se guarda; no se intenta enviar |
| Marcación rechazada por regla | Mensaje concreto del SP | No se guarda nada y **no** sale correo |
| BD caída o error SQL | Mensaje de error real | No se guarda; ya no hay silencio |

## Verificación

El proyecto no tiene framework de pruebas automatizadas. Verificación manual, igual que en los módulos anteriores:

1. **MSBuild `EXIT CODE: 0`** (`ReporteTareas.sln`, `Debug`).
2. **SP por `sqlcmd` dentro de una transacción con `ROLLBACK`** (la BD de desarrollo *es* la de producción), cubriendo los cinco casos de la tabla de reglas: entrada nueva, entrada repetida, salida sin entrada, salida válida, salida repetida.
3. **Confirmar que existen los parámetros SMTP** vía `Sp_RTAConsultaParametroConfiguracion` para `smtpAddress`, `emailFrom`, `portNumber` y `enableSSL`. Quedó pendiente de la sesión de diseño porque la BD dejó de responder; **si falta alguno, el correo no sale** y hay que cargarlo antes de dar por terminado el trabajo.
4. **Navegador** (entorno del usuario, con sesión iniciada): marcar entrada → confirmación en pantalla y correo recibido con la hora correcta; volver a presionar Entrada → mensaje de rechazo; marcar salida → segundo correo; volver a presionar Salida → mensaje de rechazo.

## Notas de despliegue

- La cadena de conexión de `CapaDato/DaoReporTareaAranda.cs` apunta a producción (`192.168.11.14 / ReporTarea`), así que el SP nuevo se crea directamente ahí. Es un objeto nuevo: no altera nada existente hasta que el JS apunte al handler nuevo.
- El orden seguro de despliegue es SP → binarios → `RegistroLaboral.js`. Si el `.js` sale antes que el resto, las marcaciones fallarían con error de red.
