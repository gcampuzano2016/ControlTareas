# Horas Extras: personas fuera del módulo, completar datos y ajustes de sueldo — Plan de implementación

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:**
1. Mostrar en la pantalla, en el aviso de cierre y en el Excel a los empleados activos que tienen horas en el período pero **no están en el módulo** de Horas Extras.
2. Permitir a los perfiles 14 y 18 **completar sus datos** (maestro y sueldo) y **registrar ajustes de sueldo** de quien ya está, con recálculo del período si está abierto.

**Architecture:** Mismo patrón que `2026-09-28-horas-extras-fuera-de-horario.md`.
- Procedimientos SQL que solo leen o solo insertan; nunca actualizan un sueldo existente.
- Una clase pura de `CapaNegocio` para los resúmenes y las validaciones, con pruebas.
- El DAO no transforma nada.
- El handler es la única barrera de perfil.
- La pantalla lo pinta todo con `.text()`.

**Tech Stack:** ASP.NET WebForms (.NET 4.8 en el web, 4.6.1 en las capas), SQL Server, jQuery + Bootstrap 3, EPPlus 6, MSTest v1, `vstest.console.exe`.

**Spec:** el diseño aprobado en conversación el 2026-09-28. Esta sección es su copia fiel y es la autoridad; sobre ella se deciden los conflictos.

> **Diseño aprobado.**
>
> **(1) Personas fuera del módulo.** Entra todo empleado activo que no esté en `HE_ColaboradorParametro` (Estado = '1') y que en el período tenga horas extras en el flujo (solicitadas, aprobadas o rechazadas) o tiempo fuera de su horario.
> - Se muestran nombre, cargo, horas aprobadas al 50% y al 100%, fuera de horario sin aprobar y rechazado. **Sin montos.** El motivo va siempre: «No está en el módulo de Horas Extras: faltan sus datos de RRHH, no se puede calcular el pago».
> - Aparece en un recuadro amarillo encima de la grilla (solo si hay alguien), en una línea del aviso de cierre y en una hoja nueva del Excel, «No están en el módulo».
> - Se calcula con un procedimiento nuevo de solo lectura, con la misma forma que `Sp_RTA_HeFueraDeHorario`, y reutiliza `NegHeFueraDeHorario`. Viaja en la misma consulta que ya hace la pantalla. No agrega a nadie al módulo, no inventa sueldos, no toca tablas ni montos.
>
> **(2) Guardar.**
> - Solo los perfiles **14 y 18**. El perfil 2 no ve los botones y el servidor lo rechaza.
> - **«Completar datos»**, en cada persona del recuadro: empresa (de las existentes en el módulo), cargo (precargado con el de Empleados), jornada de 1 a 12, si aplica horas extras, sueldo y fecha de vigencia. Si la persona ya está en el maestro, se rechaza y se remite al ajuste.
> - **«Registrar ajuste de sueldo»**, en cada fila de la grilla: monto y fecha. Agrega una fila `Origen = 'Ajuste'` y **nunca** borra ni pisa. Un ajuste con la misma fecha que otro ajuste se rechaza.
> - **Después de guardar:** si el período está abierto, se recalcula solo; si está cerrado, no se toca y el mensaje lo dice. Una vigencia posterior al final del período se advierte.
> - **Quién hizo qué:** usuario, fecha e IP en cada fila insertada. No hay borrado ni sobreescritura.
> - **Validaciones del servidor:** sueldo > 0, jornada entre 1 y 12, empresa y fecha obligatorias, empleado activo.
> - **No se crea ninguna tabla.** Nada se escribe en producción sin la aprobación del usuario, y los datos reales los confirma RRHH.

## Global Constraints

- **Nada cambia un monto existente**, salvo el recálculo que el propio usuario provoca al guardar un sueldo en un período abierto, que es el comportamiento de «Recalcular» de siempre.
- **`HE_Salario` solo recibe INSERT**, desde los dos procedimientos nuevos. Jamás `UPDATE` ni `DELETE`.
- `HE_ColaboradorParametro` tiene un índice único por `IdEmpleado`. Si ya existe una fila con `Estado <> '1'`, se **reactiva** con `UPDATE` de esa fila; si no, se inserta una nueva.
- **`Origen`** vale `'Rol'` al completar datos y `'Ajuste'` al registrar un ajuste. Lo exige `CK_HE_Salario_Origen`.
- **Perfiles que editan el maestro:** `{ 14, 18 }`, en un solo sitio: `AdministrarHorasExtras.PerfilesQueEditanMaestro`. La página lo lee para mostrar u ocultar los botones.
- **Mismo cruce de identidad** que `Sp_RTA_HeFueraDeHorario`. Empleado activo = `Empleados.Estado = 'Activo'`.
- **Los minutos se suman primero** y se redondean a horas una sola vez.
- **Codificación:** los `.sql` versionados, sin BOM y sin tildes. `HorasExtras.aspx` y `horasExtras.js`, con BOM. La página pasa a `horasExtras.js?v=9`.
- **Sin datos personales en git.**
- **Build** con el MSBuild de VS2019 desde PowerShell. **Pruebas** con `vstest.console.exe CapaPruebas\bin\Debug\CapaPruebas.dll`.
- **Commits:** solo con los archivos de la tarea. Nunca `Web.config` ni los `bin/` del árbol de trabajo.

## Review Focus

1. **Doble clic en «Guardar».** No debe insertar dos veces. El botón se deshabilita durante la petición, y el procedimiento rechaza una persona que ya está en el maestro y un ajuste repetido. → Tarea 4 (JS) y Tarea 1 (códigos -2).
2. **Guardar con el período cerrado.** No recalcula nada y lo dice. → Tarea 3, prueba `MensajeTrasGuardar_PeriodoCerrado`.
3. **Vigencia posterior al fin del período.** Se guarda, y el mensaje advierte que no aplica a este período. → Tarea 3, prueba `MensajeTrasGuardar_VigenciaPosterior`.
4. **Sueldo con coma decimal** («1.850,50» o «1850,5»). Se interpreta bien o se rechaza con un mensaje claro, nunca como 185050. → Tarea 3, pruebas de `LeerMonto`.
5. **Un perfil 2 que llama directo al handler.** Recibe «No tiene permisos» y no se escribe nada. → Tarea 4, `EstaEn(PerfilesQueEditanMaestro)`, revisado en la verificación manual.

---

## Tareas

### Task 1: SQL

**Create:** `docs/sql/2026-09-28-horas-extras-fuera-del-modulo.sql`, con tres procedimientos y su verificación.

- `Sp_RTA_HeFueraDelModulo(@FechaInicio DATE, @FechaFin DATE)`, de **solo lectura**, con tres conjuntos:
  1. **Personas:** `IdEmpleado, Nombre, Cargo` (`Empleados.PuestoTrabajo`).
  2. **Segmentos:** mismas columnas que el conjunto 1 de `Sp_RTA_HeFueraDeHorario`.
  3. **Pagados:** `IdEmpleado, Inicio, Fin, TipoRegistrado`.

  El universo es: empleados activos, fuera del maestro activo, con algún tramo del rango en el flujo (estado 1, 2 o 3) o con segmento fuera de horario.
- `Sp_RTA_HeAgregarColaborador(@IdEmpleado BIGINT, @Empresa VARCHAR(120), @Cargo VARCHAR(200), @JornadaHorasDia INT, @AplicaHE BIT, @Monto DECIMAL(18,2), @FechaVigenciaDesde DATE, @Usuario VARCHAR(50), @Ip VARCHAR(64))`. Devuelve `Respuestas`:
  - `0`: guardado;
  - `-1`: el empleado no existe o no está activo;
  - `-2`: ya está en el maestro activo;
  - `-3`: datos inválidos.

  Hace el maestro y el sueldo en **una sola transacción**.
- `Sp_RTA_HeAgregarAjusteSalario(@IdEmpleado BIGINT, @Monto DECIMAL(18,2), @FechaVigenciaDesde DATE, @Observacion VARCHAR(400), @Usuario VARCHAR(50), @Ip VARCHAR(64))`. Devuelve `Respuestas`:
  - `0`: guardado;
  - `-1`: no está en el maestro activo;
  - `-2`: ya hay un ajuste activo con esa fecha;
  - `-3`: monto o fecha inválidos.
- **Verificación con `@Fallos`**, sin escribir nada:
  - los tres objetos existen;
  - `Sp_RTA_HeFueraDelModulo` corre sobre el último período;
  - `Sp_RTA_HeAgregarColaborador` con `IdEmpleado = -1` devuelve -1;
  - `Sp_RTA_HeAgregarAjusteSalario` con monto 0 devuelve -3.
- **Antes de commitear:** se ejecuta como consulta suelta el cuerpo del primer procedimiento, sin crear nada. Expected: Bajaña y Altamirano, y nadie más.

### Task 2: Entidades y resumen puro (TDD)

- `EntHeIntervaloPagado.TipoRegistrado` (int, 0 por omisión).
- `EntHeFueraDelModuloPersona { IdEmpleado, Nombre, Cargo, MinutosAprobados50, MinutosAprobados100, MinutosSinAprobar50, MinutosSinAprobar100, MinutosRechazado50, MinutosRechazado100 }`, con los calculados `HorasAprobadas50`, `HorasAprobadas100`, `HorasSinAprobar` y `HorasRechazado`.
- `EntHeFueraDeHorario.FueraDelModulo` (`List<EntHeFueraDelModuloPersona>`), más los calculados `PersonasFueraDelModulo` y `TotalHorasAprobadasFueraDelModulo`.
- `NegHeFueraDeHorario.ResumirFueraDelModulo(IList<EntHeFueraDelModuloPersona> personas, IList<EntHeSegmentoFuera> segmentos, IList<EntHeIntervaloPagado> pagados)`:
  - **Aprobados:** la unión, por minutos, de los pagados de cada persona, separada por `TipoRegistrado`. Si un minuto aparece en un tramo al 100% y en otro al 50%, gana el 100%.
  - **Sin aprobar y rechazado:** con `Calcular`.
  - Devuelve solo las personas con algún minuto, ordenadas por nombre.

  Pruebas:
  - aprobados al 100%;
  - aprobados superpuestos contados una vez;
  - 100% gana a 50%;
  - sin aprobar y rechazado;
  - persona sin nada, que no aparece;
  - listas nulas;
  - orden por nombre.

### Task 3: Validaciones, mensajes y orquestación (TDD)

`CapaNegocio/NegHeMaestroPantalla.cs`:

- `static bool LeerMonto(string texto, out decimal monto)` acepta:
  - `"1850"`, `"1850.5"`, `"1850,5"`: coma o punto como separador decimal;
  - `"1.850,50"` y `"1,850.50"`: separador de miles y el último separador como decimal.

  Rechaza lo vacío, los negativos y el cero.
- `static string ValidarColaborador(string empresa, int jornada, decimal monto, DateTime vigencia)` y `static string ValidarAjuste(decimal monto, DateTime vigencia)`: devuelven `null` si todo está bien, o el mensaje del error.
- `static string MensajeTrasGuardar(bool periodoAbierto, DateTime vigencia, DateTime finPeriodo)`: el texto después de guardar, que cubre período abierto, período cerrado y vigencia posterior.
- `EntRespuesta AgregarColaborador(...)` y `EntRespuesta AgregarAjusteSalario(...)`:
  1. validar;
  2. llamar al DAO;
  3. traducir el código a un mensaje;
  4. si hay período y está abierto, `NegHorasExtrasPantalla.AbrirPeriodo(inicio, fin, usuario, ip)`;
  5. si no, `CargarPantalla`;
  6. combinar los mensajes.
- **DAO:** `LeerFueraDelModulo`, `AgregarColaborador` y `AgregarAjusteSalario`.
- **`NegHorasExtrasPantalla.CalcularFueraDeHorario`** llena también `FueraDelModulo`, en el mismo `try`: si falla algo, `Calculado = false`.

### Task 4: Handler, página y pantalla

- **Handler:**
  - `PerfilesQueEditanMaestro = { 14, 18 }` (`internal`);
  - las acciones `AgregarColaborador` y `AgregarAjusteSalario`, cada una con `EstaEn(context, PerfilesQueEditanMaestro)`;
  - los parámetros `idPeriodo, idEmpleado, empresa, cargo, jornada, aplicaHE, monto (texto), vigencia (yyyy-MM-dd), observacion`.
- **Página:**
  - `PuedeEditarMaestro` y `var HE_PUEDE_EDITAR_MAESTRO`;
  - el recuadro `#recuadroFueraDelModulo`, encima de la tabla y oculto al inicio;
  - las ventanas `#modalCompletarDatos` y `#modalAjusteSalario`;
  - `?v=9`.
- **JS:**
  - `PintarFueraDelModulo()` se llama desde `PintarFueraDeHorario`;
  - el botón «Completar datos» y el botón de ajuste junto al nombre, solo con `HE_PUEDE_EDITAR_MAESTRO`;
  - los formularios: la empresa se llena con los valores distintos de `EmpresaSnapshot` de la grilla; el cargo viene precargado; se deshabilita durante la petición;
  - al responder, `PintarPantalla(r.resultado)`;
  - `TextoAvisoFueraDeHorario` suma la línea de los que no están en el módulo.

### Task 5: Excel (TDD)

Hoja nueva `NombreHojaFueraDelModulo = "No están en el módulo"`, con estas columnas:
- ID Empleado, Nombre, Cargo;
- Aprobadas 50% (h), Aprobadas 100% (h);
- Fuera de horario sin aprobar (h), Rechazado (h);
- Motivo.

Sin cálculo, lleva el mismo mensaje que la hoja «Fuera de horario». Las pruebas que cuentan hojas pasan de 3 a 4.

### Task 6: Validación con datos reales, despliegue y paquete

- `Actualizacion/validar-fuera-de-horario.ps1` ejecuta también el cuerpo del procedimiento nuevo. Expected: **Bajaña con 11,00 h aprobadas al 100%** y **Altamirano con 2,75 h al 50% + 0,23 h al 100% sin aprobar**, y nadie más.
- `DESPLIEGUE.md` §13, y el paquete regenerado y comprobado.

### Task 7: Publicar y cargar datos (con aprobación explícita)

Ejecutar el script SQL y publicar los binarios. La carga de Bajaña, de Altamirano y del ajuste de Apunte la hace el usuario desde la pantalla, con los datos que confirme RRHH.
