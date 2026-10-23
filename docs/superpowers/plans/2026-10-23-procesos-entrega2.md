# Procesos entre áreas (Entrega 2) — Plan de implementación

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Que un trabajo que cruza áreas se lleve como un proceso de pasos en orden que avanza solo: al completarse la actividad de un paso se activa el siguiente en la agenda de su responsable, con correo, y al terminar se avisa a quien lo inició.

**Architecture:** Tablas nuevas `AG_Plantilla*` y `AG_Proceso*` y una columna `AG_Actividad.IdProcesoPaso` en ReporTarea. Las reglas (días hábiles, quién hace qué, validaciones, textos de correo) son clases puras de `CapaNegocio` con pruebas. El avance vive en SQL, en la misma transacción que marca el entregable o la «completa»: el handler calcula en C# la fecha de entrega del paso siguiente y se la pasa al procedimiento, que solo la usa si el paso realmente se completa, y devuelve qué se activó para que el handler mande los correos.

**Tech Stack:** ASP.NET WebForms (.NET 4.6.1), C# 7.3, SQL Server 2016, MSTest, jQuery.

**Spec:** `docs/superpowers/specs/2026-10-23-procesos-entrega2-design.md` (aprobado 2026-10-23, commit `d7da090`). Leerlo entero, incluida la sección «Resuelto en la revisión de constructor»: este plan no repite sus reglas.

## Global Constraints

- Rama `ProyectoNuevosCambios`. Commits con la línea `Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>`.
- **El repositorio es público:** ningún documento ni script lleva servidor, usuario, contraseña, nombres o cédulas.
- Prefijos: tablas `AG_*`, procedimientos `Sp_RTA_Agenda*`. Ninguna tabla fuera de `AG_*` se modifica.
- Estados del proceso (texto exacto): `EnCurso`, `Terminado`, `Cancelado`. Del paso: `Pendiente`, `EnCurso`, `Completo`, `Cancelado`.
- Plazo de un paso: entero de **1 a 60** días hábiles. Días hábiles = lunes a viernes que no están en `Feriado` con `Activo = 1`. El conteo empieza el **día siguiente** a la activación.
- **Las actividades de la Entrega 1 sin `IdProcesoPaso` se comportan exactamente igual que hoy**, en SQL, en el handler y en `agenda.js`.
- Activo = `Usuario_Estado = 'A'` y `EstadoUsuario IS NULL` e `ISNULL(IdCliente,0) = 0`, solo en `Sp_RTA_AgendaPersonasActivas` (ya existe).
- Pantallas nuevas en el grupo de menú «Equipo y agenda»: `Procesos.aspx` para todo perfil con usuarios internos activos y el 18; `ParametrizacionPlantillasProcesos.aspx` para los perfiles **10 y 18**. `Agenda.VerTodos` (ya existe) es la llave de «gestionar cualquier proceso».
- Compilar **solo** con el MSBuild de VS2019 desde PowerShell. Las tres capas salen como `.exe`.
- `.cs`, `.aspx` y `.js` nuevos: UTF-8 **con BOM**. `.csproj` en LF (sin `\r` nuevos). Scripts SQL sin BOM y **sin tildes** (textos con tilde por `NCHAR`).
- Todo handler fija `context.Response.ContentEncoding = Encoding.UTF8`. Texto de la base al DOM solo con `.text()`.
- La base local **es la de producción**. Correr cualquier script que escriba exige **autorización explícita del usuario** en ese momento; leer está permitido.

## Review Focus

1. **Dos marcas simultáneas sobre el último entregable de un paso** — el paso siguiente se activa una sola vez (una actividad, un correo). Prueba de concurrencia de dos sesiones en el script de prueba de Task 4.
2. **Una actividad de la Entrega 1 (sin proceso) después del cambio** — guardar, marcar y completar dan exactamente lo mismo que antes. Casos de regresión en el script de prueba de Task 4 y prueba manual en Task 9.
3. **Iniciar con un área sin personas activas** — se rechaza nombrando el área, sin crear nada (prueba `ValidarInicio_AreaSinPersonas` en Task 2).
4. **Plazo que cae en feriado o cruza fin de semana / fin de año** — vence el siguiente día hábil (pruebas de Task 1).
5. **Quien inicia el proceso es responsable de un paso** — no recibe el correo «Te toca un paso» (prueba `DebeAvisarPaso_IniciadorEsResponsable_No` en Task 3).

---

## Mapa de archivos

| Archivo | Responsabilidad |
|---|---|
| `CapaEntidad/EntAgendaProceso.cs` | Entidades de plantillas y procesos |
| `CapaEntidad/EntAgenda.cs` | `EntAgendaActividad` y `EntAgendaItem` ganan los campos de proceso |
| `CapaNegocio/NegAgendaDiasHabiles.cs` | Suma de días hábiles |
| `CapaNegocio/NegAgendaProceso.cs` | Permisos, validaciones y estado de procesos y plantillas |
| `CapaNegocio/NegAgendaCorreo.cs` | Correos «Te toca un paso» y «Proceso terminado» |
| `CapaNegocio/NegAgenda.Datos.cs`, `CapaDato/DaoAgenda.cs` | Llamadas a los procedimientos nuevos y a los que cambian |
| `CapaPruebas/NegAgendaDiasHabilesTests.cs`, `NegAgendaProcesoTests.cs`, `NegAgendaCorreoTests.cs` | Pruebas |
| `docs/sql/2026-10-23-procesos-esquema.sql` | Tablas, columnas, procedimientos nuevos y los de la Entrega 1 que cambian |
| `docs/sql/2026-10-23-procesos-prueba.sql` | Pruebas SQL dentro de `BEGIN TRAN … ROLLBACK` |
| `docs/sql/2026-10-23-procesos-menu.sql`, `docs/sql/2026-10-23-procesos-verificacion.sql` | Menú y verificación |
| `ReporteTareas/clases/Permisos.cs` | Constantes de las 2 pantallas |
| `ReporteTareas/Formulario/AdministrarAgenda.ashx.cs` | Acciones nuevas y cambios en Marcar/Guardar/Agenda |
| `ReporteTareas/Formulario/Procesos.aspx(.cs, .designer.cs)` + `js/procesos.js` | Pantalla «Procesos» |
| `ReporteTareas/Formulario/ParametrizacionPlantillasProcesos.aspx(.cs, .designer.cs)` + `js/parametrizacionPlantillasProcesos.js` | Pantalla «Plantillas de procesos» |
| `ReporteTareas/js/agenda.js`, `ReporteTareas/Formulario/Agenda.aspx` | Etiqueta del proceso, controles ocultos, confirmación al completar |
| `DESPLIEGUE.md` | Sección 23 |

## Comandos que se repiten

**Compilar** (PowerShell):
```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "C:\respaldodisco\Desarrollo\PRY_Sistema ReporteTareas\ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo | Select-String -Pattern " error " | Select-Object -First 20
```

**Probar** (PowerShell; `<Filtro>` = clase de prueba, u omitir `/Tests:`):
```powershell
$v = Get-ChildItem "C:\Program Files (x86)\Microsoft Visual Studio\2019" -Recurse -Filter vstest.console.exe -ErrorAction SilentlyContinue | Select-Object -First 1
& $v.FullName "C:\respaldodisco\Desarrollo\PRY_Sistema ReporteTareas\CapaPruebas\bin\Debug\CapaPruebas.dll" /Tests:<Filtro> | Select-String -Pattern "Correcto|Con errores|Total"
```

**Correr un script SQL** (Git Bash; si escribe, solo con autorización): servidor y usuario de la cadena `Catalog=ReporTarea` de `ReporteTareas/connections.config`, siempre con `sqlcmd -b`.

**BOM** a un archivo nuevo: `python -c "import sys;p=sys.argv[1];d=open(p,'rb').read();open(p,'wb').write(d if d.startswith(b'\xef\xbb\xbf') else b'\xef\xbb\xbf'+d)" "<ruta>"`

---

### Task 1: Entidades y días hábiles

**Files:**
- Create: `CapaEntidad/EntAgendaProceso.cs`, `CapaNegocio/NegAgendaDiasHabiles.cs`, `CapaPruebas/NegAgendaDiasHabilesTests.cs`
- Modify: `CapaEntidad/EntAgenda.cs`, los tres `.csproj`

**Interfaces:**
- Produces:
  - `EntAgendaPlantilla { IdPlantilla:int, Nombre, Descripcion:string, Activa:bool, TieneProcesos:bool, Pasos:List<EntAgendaPlantillaPaso> }`
  - `EntAgendaPlantillaPaso { IdPlantillaPaso, Orden, IdArea:int, Area, Titulo:string, PlazoDias:int, Entregables:string }` (`Entregables` = uno por línea)
  - `EntAgendaProceso { IdProceso:int, Nombre:string, IdPlantilla:int, Plantilla, CodIniciadoPor, Estado:string, FechaInicio:DateTime, FechaFin:DateTime?, MotivoCancelacion:string, Pasos:List<EntAgendaProcesoPaso> }`
  - `EntAgendaProcesoPaso { IdProcesoPaso, IdProceso, Orden, IdArea:int, Area, Color, Titulo:string, PlazoDias:int, Entregables, CodResponsable, Estado:string, FechaActivacion:DateTime?, FechaCompletado:DateTime?, IdActividad:int?, FechaEntrega:DateTime?, ActividadCompleta:bool }`
  - `EntAgendaAvance { IdProcesoPasoActivado:int?, IdProcesoTerminado:int? }` (lo que devuelve un marcado que hizo avanzar un proceso)
  - Constantes `AgendaProcesoEstados { EnCurso, Terminado, Cancelado }` y `AgendaPasoEstados { Pendiente, EnCurso, Completo, Cancelado }`.
  - En `EntAgendaActividad`: `IdProcesoPaso:int?`, `NombreProceso:string`, `EstadoPaso:string`. En `EntAgendaItem`: `IdProceso:int?`, `NombreProceso:string`, `EsPasoDeProceso:bool`, `AvisoAlCompletar:string`.
  - `NegAgendaDiasHabiles.SumarDiasHabiles(DateTime desde, int dias, ISet<DateTime> feriados) : DateTime`; `EsHabil(DateTime d, ISet<DateTime> feriados) : bool`.

- [ ] **Step 1: Entidades** como arriba (cadenas inicializadas en `""`, listas vacías).
- [ ] **Step 2: Pruebas que fallan** (`D(m,d)` = 2026; `F(params DateTime[])` arma el conjunto de feriados):
```csharp
[TestMethod] public void Sumar_UnDiaDesdeMiercoles_Jueves() => Assert.AreEqual(D(10,22), NegAgendaDiasHabiles.SumarDiasHabiles(D(10,21), 1, F()));
[TestMethod] public void Sumar_UnDiaDesdeViernes_Lunes() => Assert.AreEqual(D(10,26), NegAgendaDiasHabiles.SumarDiasHabiles(D(10,23), 1, F()));
[TestMethod] public void Sumar_SaltaFeriado() => Assert.AreEqual(D(10,13), NegAgendaDiasHabiles.SumarDiasHabiles(D(10,8), 2, F(D(10,9))));
[TestMethod] public void Sumar_FeriadoEnLunesTrasViernes_Martes() => Assert.AreEqual(D(10,27), NegAgendaDiasHabiles.SumarDiasHabiles(D(10,23), 1, F(D(10,26))));
[TestMethod] public void Sumar_CruzaAnio() => Assert.AreEqual(new DateTime(2027,1,1), NegAgendaDiasHabiles.SumarDiasHabiles(D(12,31), 1, F()));
[TestMethod] public void Sumar_CruzaAnioConFeriado() => Assert.AreEqual(new DateTime(2027,1,4), NegAgendaDiasHabiles.SumarDiasHabiles(D(12,31), 1, F(new DateTime(2027,1,1))));
[TestMethod] public void Sumar_CincoDias_UnaSemana() => Assert.AreEqual(D(10,28), NegAgendaDiasHabiles.SumarDiasHabiles(D(10,21), 5, F()));
[TestMethod] public void Sumar_DesdeSabado_CuentaDesdeLunes() => Assert.AreEqual(D(10,26), NegAgendaDiasHabiles.SumarDiasHabiles(D(10,24), 1, F()));
[TestMethod] public void Sumar_IgnoraHora() => Assert.AreEqual(D(10,22), NegAgendaDiasHabiles.SumarDiasHabiles(D(10,21).AddHours(23), 1, F()));
[TestMethod] [ExpectedException(typeof(ArgumentOutOfRangeException))] public void Sumar_PlazoCero_Error() => NegAgendaDiasHabiles.SumarDiasHabiles(D(10,21), 0, F());
```
(La clase no lee la base: cada prueba pasa su propio conjunto de feriados.)
- [ ] **Step 3: Correr** `/Tests:NegAgendaDiasHabilesTests` — FAIL. **Step 4: Implementar** (comparar por `.Date`; rango válido de `dias` 1..60). **Step 5: Correr** — PASS; suite completa — PASS. **Step 6: Commit** `feat(procesos): entidades y dias habiles`.

---

### Task 2: Reglas de procesos y plantillas

**Files:** Create `CapaNegocio/NegAgendaProceso.cs`, `CapaPruebas/NegAgendaProcesoTests.cs`; Modify `CapaNegocio.csproj`, `CapaPruebas.csproj`.

**Interfaces:**
- Consumes: Task 1; `NegAgendaEquipo.Normalizar`, `Cadena`, `Codigos` (Entrega 1).
- Produces (`public static class NegAgendaProceso`, todas devuelven `null` = permitido o el mensaje):
  - `PuedeVer(string yo, EntAgendaProceso p, ISet<string> cadena, bool verTodos) : bool` — iniciador, responsable de algún paso, alguno de ellos en `cadena` de `yo`, o `verTodos`.
  - `PuedeGestionar(string yo, EntAgendaProceso p, bool verTodos) : bool` — iniciador o `verTodos`, y el proceso `EnCurso`.
  - `ValidarPlantilla(EntAgendaPlantilla t, ISet<int> areasActivas) : string`
  - `ValidarInicio(string nombre, EntAgendaPlantilla t, IList<EntAgendaProcesoPaso> pasos, IList<EntAgendaPersona> activas) : string` — nombre 1..200; plantilla activa; un paso por paso de plantilla, mismo orden; cada `CodResponsable` activo **y del área del paso**; si un área no tiene personas activas: `"El área «X» no tiene personas activas: no se puede iniciar el proceso."`; título 1..200; plazo 1..60.
  - `PuedeReasignar(EntAgendaProcesoPaso paso, string nuevoResponsable, IList<EntAgendaPersona> activas) : string` — paso `Pendiente` o `EnCurso`; persona activa del área.
  - `PuedeCambiarPlazo(EntAgendaProcesoPaso paso, int plazo) : string` — solo `Pendiente`, 1..60.
  - `PuedeCambiarFecha(EntAgendaProcesoPaso paso, DateTime fecha, DateTime hoy) : string` — solo `EnCurso`; nunca sábado ni domingo; no anterior a hoy.
  - `PuedeCancelar(EntAgendaProceso p, string motivo) : string` — `EnCurso`; motivo 1..400.
  - `ResponsableInactivo(EntAgendaProcesoPaso paso, IList<EntAgendaPersona> activas) : bool` — paso `Pendiente`/`EnCurso` con responsable fuera de `activas`.
  - `AvisoAlCompletar(EntAgendaProceso p, EntAgendaProcesoPaso paso, string nombreSiguiente) : string` — `"Al completar este paso se le avisará a <nombre> para el paso siguiente. No se puede deshacer."` o, si es el último, `"Al completar este paso se terminará el proceso. No se puede deshacer."`.
  - `ResumenAvance(EntAgendaProceso p) : string` — `"n/m pasos completos"`.

- [ ] **Step 1: Pruebas que fallan** — una por regla y caso, con nombres `<Regla>_<Caso>`, al menos: `PuedeVer_Iniciador_Si`, `PuedeVer_ResponsableDeUnPaso_Si`, `PuedeVer_JefeDeUnResponsable_Si`, `PuedeVer_Ajeno_No`, `PuedeVer_VerTodos_Si`, `PuedeGestionar_SoloIniciadorOVerTodos`, `PuedeGestionar_Terminado_No`, `ValidarPlantilla_SinPasos`, `ValidarPlantilla_OrdenNoConsecutivo`, `ValidarPlantilla_AreaInactiva`, `ValidarPlantilla_Plazo0y61`, `ValidarInicio_AreaSinPersonas` (mensaje exacto de arriba), `ValidarInicio_ResponsableDeOtraArea`, `ValidarInicio_ResponsableFueraDeLaCadena_Vale`, `PuedeReasignar_Completo_No`, `PuedeCambiarPlazo_EnCurso_No`, `PuedeCambiarFecha_Sabado_No`, `PuedeCambiarFecha_Pendiente_No`, `PuedeCancelar_SinMotivo`, `ResponsableInactivo_Si`, `AvisoAlCompletar_UltimoPaso`, `AvisoAlCompletar_ConSiguiente`. Los códigos se comparan con `NegAgendaEquipo.Codigos` (espacios y mayúsculas).
- [ ] **Step 2: Correr** — FAIL. **Step 3: Implementar.** **Step 4: Correr** — PASS; suite — PASS. **Step 5: Commit** `feat(procesos): reglas de procesos y plantillas`.

---

### Task 3: Correos de procesos

**Files:** Modify `CapaNegocio/NegAgendaCorreo.cs`, `CapaPruebas/NegAgendaCorreoTests.cs`.

**Interfaces:**
- Produces: `DebeAvisarPaso(EntAgendaProceso p, EntAgendaProcesoPaso paso) : bool` (falso si el responsable es el iniciador); `AsuntoPaso(EntAgendaProceso p, EntAgendaProcesoPaso paso) : string` = `"Te toca un paso: <título> — <proceso>"`; `CuerpoPaso(p, paso, string nombreResponsable, string nombreIniciador, DateTime fechaEntrega, string urlSitio) : string`; `AsuntoTerminado(EntAgendaProceso p) : string` = `"Proceso terminado: <proceso>"`; `CuerpoTerminado(p, IDictionary<string,string> nombres, string urlSitio) : string` (tabla de pasos: orden, título, responsable, fecha completado).
- [ ] **Step 1: Pruebas que fallan:** `DebeAvisarPaso_IniciadorEsResponsable_No`, `DebeAvisarPaso_OtroResponsable_Si`, `AsuntoPaso_Formato`, `AsuntoPaso_SinSaltosDeLinea` (títulos con `\r\n` quedan en una línea), `CuerpoPaso_EscapaHtmlYTildes`, `CuerpoPaso_TraeFechaYEnlace`, `AsuntoTerminado_Formato`, `CuerpoTerminado_ListaTodosLosPasos`. Mismo escape y estilo que `Cuerpo` de la Entrega 1.
- [ ] **Step 2: Correr** — FAIL. **Step 3: Implementar.** **Step 4: Correr** — PASS. **Step 5: Commit** `feat(procesos): correos de pasos y procesos`.

---

### Task 4: Esquema y procedimientos

**Files:** Create `docs/sql/2026-10-23-procesos-esquema.sql`, `docs/sql/2026-10-23-procesos-prueba.sql`.

**Interfaces:**
- Produces: tablas del spec §1 (`AG_Plantilla`, `AG_PlantillaPaso`, `AG_Proceso`, `AG_ProcesoPaso`), `AG_Actividad.IdProcesoPaso` (FK, índice), `AG_CorreoBitacora.IdActividad` NULL + `IdProceso` (FK) + `CHECK` de al menos uno. Procedimientos (columnas devueltas = nombres de propiedad de Task 1):

| Procedimiento | Qué hace | Devuelve |
|---|---|---|
| `Sp_RTA_AgendaFeriados` | `Fecha` de `Feriado` con `Activo = 1` desde hoy − 30 días | fechas |
| `Sp_RTA_AgendaPlantillas` | plantillas con `TieneProcesos`; 2.º conjunto: pasos con nombre de área | — |
| `Sp_RTA_AgendaGuardarPlantilla` | `@IdPlantilla` (0 = nueva), `@Nombre`, `@Descripcion`, `@Activa`, `@Pasos` XML (`<p o="" a="" t="" d="" e=""/>`), `@Usuario`; reemplaza los pasos | id, `-1` nombre repetido |
| `Sp_RTA_AgendaProcesos` | todos los procesos (la visibilidad la decide C#); 2.º conjunto: pasos con área, color, `FechaEntrega` y `ActividadCompleta` de su actividad | — |
| `Sp_RTA_AgendaIniciarProceso` | `@Nombre`, `@IdPlantilla`, `@Pasos` XML (orden, área, título, plazo, entregables, responsable), `@FechaEntregaPrimero`, `@Usuario`; crea proceso, pasos `Pendiente`, activa el 1.º (actividad + entregables, `Cod_AsignadoPor = @Usuario`) | `IdProceso`, `IdProcesoPasoActivado` |
| `Sp_RTA_AgendaReasignarPaso` | `@IdProcesoPaso`, `@EstadoEsperado`, `@CodResponsable`, `@Usuario`; si está en curso mueve la actividad | `0` / `-1` cambió |
| `Sp_RTA_AgendaCambiarPasoPendiente` | `@IdProcesoPaso`, `@Titulo`, `@PlazoDias`, `@Entregables`, `@Usuario` (solo `Pendiente`) | `0` / `-1` |
| `Sp_RTA_AgendaCambiarFechaPaso` | `@IdProcesoPaso`, `@Fecha`, `@Usuario` (solo `EnCurso`, cambia `FechaEntrega` de su actividad) | `0` / `-1` |
| `Sp_RTA_AgendaCancelarProceso` | `@IdProceso`, `@Motivo`, `@Usuario`; pasos no completos → `Cancelado`, anula la actividad en curso | `0` / `-1` no en curso |

  **Cambios a la Entrega 1** (con `SET NOEXEC` de versión y el cuerpo anterior de cada uno comentado al final para la reversa):
  - `Sp_RTA_AgendaActividades` y `Sp_RTA_AgendaActividad`: agregan `IdProcesoPaso`, `NombreProceso`, `EstadoPaso`.
  - `Sp_RTA_AgendaGuardarActividad`: si la actividad existente tiene `IdProcesoPaso`, devuelve `-9` y no toca nada.
  - `Sp_RTA_AgendaMarcarEntregable` y `Sp_RTA_AgendaMarcarCompleta`: parámetro nuevo `@FechaEntregaSiguiente DATE = NULL`; rechazan (`-9`) si el paso de la actividad está `Completo` o `Cancelado`; tras `Sp_RTA_AgendaRecalcular`, si la actividad es de un paso `EnCurso` y quedó completa: paso → `Completo`, siguiente → `EnCurso` con su actividad (`FechaEntrega = @FechaEntregaSiguiente`; si viene NULL, `-8` y no avanza) o proceso → `Terminado`. Todo con `UPDLOCK, HOLDLOCK` sobre la fila del proceso, dentro de una transacción. **Devuelven** una fila: `Respuestas`, `IdProcesoPasoActivado`, `IdProcesoTerminado` (antes no devolvían nada: el DAO actual las llama sin leer, sigue funcionando hasta que se publiquen los binarios).
  - `Sp_RTA_AgendaRegistrarCorreo`: `@IdProceso INT = NULL` nuevo; `@IdActividad` admite NULL.

- [ ] **Step 1: Escribir el esquema** con el patrón de `2026-10-15-agenda-esquema.sql` y bloque `@Fallos` → `VEREDICTO: OK`.
- [ ] **Step 2: Escribir la prueba** (todo en `BEGIN TRAN … ROLLBACK`, `SET XACT_ABORT ON`, `PASS/FAIL` por caso y conteo final de filas sin cambios): plantilla de 3 pasos; iniciar → paso 1 `EnCurso` con actividad, pasos 2–3 sin actividad; marcar el último entregable con fecha → paso 2 activo con su fecha; marcar con `@FechaEntregaSiguiente` NULL → `-8` sin avanzar; desmarcar un entregable de un paso completo → `-9`; guardar una actividad de paso → `-9`; reasignar en curso → la actividad cambia de responsable y conserva sus entregables hechos; cancelar → actividad anulada; terminar → `IdProcesoTerminado`. **Regresión Entrega 1:** una actividad sin proceso: guardar, marcar, desmarcar, completar y anular producen las mismas filas que la versión anterior (comparar contra el resultado esperado escrito en el script).
- [ ] **Step 3: Probar en una base local desechable** (LocalDB): esquema dos veces → OK; prueba → todos PASS. Prueba de concurrencia: dos sesiones marcan a la vez el último entregable → un solo paso activado. **No correr nada contra producción.**
- [ ] **Step 4: Commit** `feat(procesos): esquema y procedimientos`. Luego el controlador pide autorización al usuario para correr en producción: esquema dos veces y prueba una vez.

---

### Task 5: Datos

**Files:** Modify `CapaDato/DaoAgenda.cs`, `CapaNegocio/NegAgenda.Datos.cs`.

**Interfaces:**
- Produces (`NegAgenda`, parcial): `Feriados() : HashSet<DateTime>`, `Plantillas() : List<EntAgendaPlantilla>`, `GuardarPlantilla(EntAgendaPlantilla t, string usuario) : int`, `Procesos() : List<EntAgendaProceso>`, `IniciarProceso(string nombre, int idPlantilla, IList<EntAgendaProcesoPaso> pasos, DateTime fechaPrimero, string usuario, out int idPasoActivado) : int`, `ReasignarPaso(int idPaso, string estadoEsperado, string codResponsable, string usuario) : int`, `CambiarPasoPendiente(EntAgendaProcesoPaso paso, string usuario) : int`, `CambiarFechaPaso(int idPaso, DateTime fecha, string usuario) : int`, `CancelarProceso(int idProceso, string motivo, string usuario) : int`; `MarcarEntregable` y `MarcarCompleta` ganan `DateTime? fechaSiguiente` y devuelven `EntAgendaAvance` (con `Respuestas` < 0 → excepción controlada o código, según el patrón actual del DAO); `RegistrarCorreo` gana `int? idProceso`.
- [ ] **Step 1:** Métodos en `DaoAgenda` (XML de pasos con `XElement`, escapado por el framework). **Step 2:** Puente en `NegAgenda.Datos.cs`. **Step 3:** Compilar; suite — PASS. Con el esquema ya en producción, verificar con un arnés o consulta de solo lectura que `Plantillas()` y `Procesos()` leen sin error. **Step 4: Commit** `feat(procesos): datos`.

---

### Task 6: Handler y permisos

**Files:** Modify `ReporteTareas/Formulario/AdministrarAgenda.ashx.cs`, `ReporteTareas/clases/Permisos.cs` (`Procesos = "Procesos.aspx"`, `ParametrizacionPlantillasProcesos = "ParametrizacionPlantillasProcesos.aspx"`).

**Interfaces:**
- Produces: acciones JSON nuevas (mismo sobre `{estado, mensaje, aviso, resultado}` de la Entrega 1):
  - pantalla `Procesos`: `ListarProcesos` (solo los visibles por `NegAgendaProceso.PuedeVer`; cada paso con `responsableInactivo`, cada proceso con `puedeGestionar`), `DatosIniciar` (plantillas activas y personas activas por área), `IniciarProceso`, `ReasignarPaso`, `CambiarPasoPendiente`, `CambiarFechaPaso`, `CancelarProceso`.
  - pantalla `ParametrizacionPlantillasProcesos`: `ListarPlantillas`, `GuardarPlantilla`.
  - **Cambios:** `MarcarEntregable`/`MarcarCompleta` calculan `fechaSiguiente` con `NegAgendaDiasHabiles.SumarDiasHabiles(DateTime.Today, plazo del paso siguiente, Feriados())` cuando la actividad es de un paso, y tras el marcado mandan «Te toca un paso» (si `DebeAvisarPaso`) o «Proceso terminado», registrando cada intento con `RegistrarCorreo` (un fallo solo agrega `aviso`). `Agenda` llena en cada `EntAgendaItem` los campos de proceso; para un paso: `PuedeEditar = false` y `AvisoAlCompletar` con `NegAgendaProceso.AvisoAlCompletar`. `GuardarActividad`/`Anular` rechazan actividades de pasos con «Esta actividad es un paso de un proceso: se gestiona desde Procesos.». Códigos `-1` → «El proceso cambió; recargue la pantalla.»; `-8`/`-9` → mensaje claro.
  - `IniciarProceso` y `ReasignarPaso` (en curso) mandan «Te toca un paso» al responsable activado.
- [ ] **Step 1:** Implementar; cada acción comprueba `Permisos.PuedeEntrar` de su pantalla antes de todo; identidad solo de `Session["Cod_Usuario"]`. **Step 2:** Compilar; suite — PASS. **Step 3:** Dejar en el reporte el contrato JSON de cada acción (campos de entrada y salida): lo consumen las Tasks 7–9. **Step 4: Commit** `feat(procesos): handler y permisos`.

---

### Task 7: Pantalla «Plantillas de procesos»

**Files:** Create `ParametrizacionPlantillasProcesos.aspx(.cs, .designer.cs)`, `js/parametrizacionPlantillasProcesos.js`; Modify `ReporteTareas.csproj`.
- [ ] **Step 1:** Estructura de `ParametrizacionAreasAgenda.aspx`. Lista de plantillas (nombre, n.º de pasos, activa, «tiene procesos»); editor: nombre, descripción, activa, pasos con área (catálogo activo, con su color), título, plazo, entregables (textarea, uno por línea), agregar/quitar/subir/bajar. Sin botón borrar plantilla.
- [ ] **Step 2:** BOM, compilar. **Step 3 (manual o pendiente para el usuario si el login AD lo impide):** crear una plantilla de 3 pasos en áreas distintas; editarla; desactivarla. **Step 4: Commit** `feat(procesos): pantalla de plantillas`.

---

### Task 8: Pantalla «Procesos»

**Files:** Create `Procesos.aspx(.cs, .designer.cs)`, `js/procesos.js`; Modify `ReporteTareas.csproj`.
- [ ] **Step 1:** Spec §4.1: filtros (estado, por defecto «En curso»; área), tarjetas con la línea de pasos (completo verde, en curso resaltado, pendiente gris, cancelado tachado, atrasado rojo, «Responsable inactivo — reasignar» en rojo para quien puede gestionar), y para quien puede gestionar: reasignar (personas activas del área), cambiar plazo (pendiente) o fecha (en curso; sin sábados ni domingos), cancelar con motivo (diálogo propio). «Iniciar un proceso»: plantilla, nombre, por paso responsable/título/plazo/entregables; botones deshabilitados en vuelo.
- [ ] **Step 2:** BOM, compilar. **Step 3 (manual o pendiente):** ciclo del spec «Pruebas en navegador». **Step 4: Commit** `feat(procesos): pantalla procesos`.

---

### Task 9: Agenda con pasos de procesos

**Files:** Modify `ReporteTareas/js/agenda.js`, `ReporteTareas/Formulario/Agenda.aspx`.
- [ ] **Step 1:** Si `esPasoDeProceso`: etiqueta con `nombreProceso` y enlace a `Procesos.aspx`; no mostrar «Editar» ni «Anular»; antes de marcar el último entregable pendiente o «Marcar como completa», diálogo propio con `avisoAlCompletar` (Aceptar / Cancelar). En un paso `Completo`, casillas deshabilitadas. **Nada cambia para las demás actividades.**
- [ ] **Step 2:** BOM se conserva; compilar. **Step 3 (manual o pendiente):** una actividad normal se ve y se usa igual que antes (regresión); un paso muestra la etiqueta, pide confirmación y al aceptar activa el siguiente. **Step 4: Commit** `feat(procesos): pasos de procesos en la agenda`.

---

### Task 10: Menú, verificación, despliegue y paquete

**Files:** Create `docs/sql/2026-10-23-procesos-menu.sql`, `docs/sql/2026-10-23-procesos-verificacion.sql`; Modify `DESPLIEGUE.md` (§23), paquete `PackageTmp`.
- [ ] **Step 1: Menú** con el patrón de `2026-10-15-agenda-menu.sql`: dos hojas en el grupo «Equipo y agenda» existente (Procesos, «Plantillas de procesos»), visibilidad de Global Constraints, reversa comentada. No se corre en esta tarea.
- [ ] **Step 2: Verificación** (solo lectura, `@Fallos`): tablas, columnas y procedimientos; `AG_CorreoBitacora` con su `CHECK`; ninguna actividad con `IdProcesoPaso` apuntando a un paso inexistente; ningún paso `EnCurso` sin actividad ni con dos; menú esperado (`PENDIENTE` cuenta como falla).
- [ ] **Step 3: DESPLIEGUE §23**: esquema (con autorización) → binarios juntos + `Procesos.aspx`, `ParametrizacionPlantillasProcesos.aspx`, `procesos.js`, `parametrizacionPlantillasProcesos.js`, `Agenda.aspx`, `agenda.js` → menú → verificación. Sin `Web.config`. Las plantillas de ejemplo de la maqueta como sugerencia (spec §5). Lista de pruebas en producción y reversa (binarios juntos + reversa del menú + cuerpos anteriores comentados de los procedimientos de la Entrega 1).
- [ ] **Step 4: Paquete** (último paso): target `Package`; comprobar por reflexión que `PackageTmp\bin\CapaNegocio.exe` contiene `CapaNegocio.NegAgendaProceso` y que las `.aspx`/`.js` están en `PackageTmp`.
- [ ] **Step 5: Commits** `feat(procesos): menu, verificacion y despliegue (seccion 23)` y `build(procesos): paquete`.
