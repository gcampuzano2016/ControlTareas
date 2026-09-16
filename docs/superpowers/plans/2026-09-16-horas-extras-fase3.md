# Horas Extras — Fase 3: cerrar y auditar

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Que un período de horas extras se pueda cerrar formalmente, que al cerrarse deje de admitir cambios, que reabrirlo exija un perfil distinto del que lo cerró, y que cada cambio de horas quede registrado con quién, cuándo y desde dónde.

**Architecture:** El cierre es un cambio de estado en `HE_Periodo` con dos validaciones y un sello de usuario y fecha. La auditoría se escribe **dentro del mismo procedimiento que guarda la fila**, en la misma transacción implícita, comparando el valor anterior contra el nuevo: escribirla desde C# permitiría que el guardado tuviera éxito y la auditoría no. El bloqueo de edición **ya existe** desde la fase 2 y esta fase lo verifica en vez de construirlo.

**Tech Stack:** ASP.NET WebForms, .NET Framework 4.8 (web) / **4.6.1 (capas)**, SQL Server, jQuery plano, Bootstrap 3, MSTest v1.

**Spec:** `docs/superpowers/specs/2026-09-15-horas-extras-design.md` (autoridad vinculante). Especificación funcional de origen: `Actualizacion/ESPEC_MODULO_HORAS_EXTRAS.md` (fuera de git a propósito; se puede leer).

**Estado de partida:** fases 1 y 2 **en producción** desde el 2026-09-16. Las seis tablas, los 64 colaboradores (62 activos), los cinco procedimientos `Sp_RTA_He*` y la pantalla `HorasExtras.aspx` en el menú de los perfiles 14 y 18. 152 pruebas verdes. **Los binarios de la fase 2 aún no se han desplegado.**

---

## Global Constraints

- **`.NET Framework 4.6.1`** en `CapaEntidad`, `CapaDato`, `CapaNegocio` y `CapaPruebas`: nada de `record`, `init` ni `switch` de expresión. La web es 4.8.
- **`decimal`, nunca `double`.** Redondeo `MidpointRounding.AwayFromZero`.
- **Las capas van `CapaNegocio → CapaDato → CapaEntidad`.** `CapaDato` **no puede** referenciar `CapaNegocio`: es un ciclo y no compila.
- **Sin tildes ni eñes en los `.sql` versionados ni en los comentarios de C#.** El servidor sirve los scripts en windows-1252 y se rompen. Los textos que ve el usuario en `.aspx` y `.js` **sí** llevan tildes correctas.
- **`HorasExtras.aspx` se guarda con BOM UTF-8.** `Web.config:39` declara `requestEncoding="windows-1252"` sin `fileEncoding`: una `.aspx` sin BOM sale con caracteres raros en producción.
- **Parámetros SQL tipados con `SqlDbType` explícito**, nunca `AddWithValue`. Los decimales llevan `Precision`/`Scale`.
- **La identidad sale SIEMPRE de `context.Session`.** El handler declara `IRequiresSessionState`.
- **El repositorio de GitHub es PÚBLICO.** Ningún dato personal en archivos versionados, comentarios ni mensajes de error.
- Compilar con el MSBuild de **VS2019**: `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe`.

## Decisiones tomadas, que NO se reabren

1. **Sin exportación en esta fase.** Decidido por el usuario el 2026-09-16: el formato que espera el proceso de nómina sigue sin definirse (§12.5 funcional) y no se puede adivinar. La exportación será una fase aparte cuando llegue el formato. **No pongas un botón «Exportar» que no exporte.**
2. **Sin validación de topes al cerrar.** Decidido por el usuario. El §6 funcional pide «ninguna fila con advertencia de tope legal sin observación justificativa», pero nadie ha definido a qué umbral mensual se traducen `TopeDiario50` y `TopeSemanal50`, y el módulo guarda un total mensual. Coherente con la fase 2, que ya dejó los topes fuera.
3. **Cerrar: perfiles 14 y 18. Reabrir: sólo el 18.** Decidido por el usuario. Separación de funciones: quien opera el mes puede cerrarlo, pero deshacer un cierre exige un perfil distinto. Es lo más parecido al espíritu de `HE.Cerrar` y `HE.Reabrir` del §8 con los perfiles que este sistema tiene.
4. **La auditoría se escribe en SQL, dentro de `Sp_RTA_HeGuardarFila`.** No desde C#. Si se escribiera desde C# después de guardar, un fallo entre ambos dejaría un cambio sin rastro — y el rastro es justamente lo que el §8 exige que no falte.
5. **El bloqueo de edición ya está construido.** La fase 2 dejó el servidor rechazando escrituras a un período no abierto (`Respuestas = -2`) y la pantalla ocultando el botón de guardar y deshabilitando las celdas. Esta fase lo **verifica**, no lo reescribe.
6. **Sin pantalla para consultar la auditoría.** Se registra, y se consulta por SQL si hace falta. Una pantalla de consulta es otra fase; lo que el §8 exige es que el rastro exista.

---

## Estructura de archivos

| Archivo | Responsabilidad |
|---|---|
| `docs/sql/2026-09-16-horas-extras-fase3.sql` | El `ALTER` de la auditoría, la auditoría dentro de `Sp_RTA_HeGuardarFila`, y los procedimientos de cerrar y reabrir |
| `CapaDato/DaoHorasExtras.cs` | Dos métodos más: `CerrarPeriodo` y `ReabrirPeriodo` |
| `CapaNegocio/NegHorasExtrasPantalla.cs` | La orquestación de ambos, con sus validaciones |
| `CapaPruebas/NegHorasExtrasPantallaTests.cs` | Las pruebas de lo que se puede probar sin base |
| `ReporteTareas/Formulario/AdministrarHorasExtras.ashx.cs` | Dos acciones más, cada una con su comprobación de perfil |
| `ReporteTareas/Formulario/HorasExtras.aspx` | Los botones de cerrar y reabrir, y el indicador de estado |
| `ReporteTareas/js/horasExtras.js` | Su comportamiento |

**No se crean archivos nuevos de C#.** La fase añade métodos a los que ya existen: son pocos y pertenecen a la misma responsabilidad que sus vecinos.

---

## Task 1: La auditoría y los procedimientos de cierre

**Files:**
- Create: `docs/sql/2026-09-16-horas-extras-fase3.sql`

**Interfaces:**
- Consumes: `HE_Periodo`, `HE_Detalle`, `HE_DetalleAuditoria` (fase 1, en producción) y `Sp_RTA_HeGuardarFila` (fase 2, en producción).
- Produces: `Sp_RTA_HeCerrarPeriodo`, `Sp_RTA_HeReabrirPeriodo`, y `Sp_RTA_HeGuardarFila` recreado con un parámetro `@Auditar BIT`.

**Contexto que necesitas.**

`HE_DetalleAuditoria` está **en producción** con esta forma: `IdAuditoria INT IDENTITY`, `IdDetalle INT`, `Campo VARCHAR(20)`, `ValorAnterior DECIMAL(9,2)`, `ValorNuevo DECIMAL(9,2)`, `Fecha DATETIME2(0)`, `Usuario VARCHAR(50)`, `Ip VARCHAR(64)`.

Las dos columnas de valor son **decimales**, así que sirven para `Horas50` y `Horas100` y **no** para `Observacion`, que es texto y también es editable. Esa asimetría se detectó al cerrar la fase 2 y se decidió resolverla aquí.

`Sp_RTA_HeGuardarFila` tiene hoy 22 parámetros y devuelve `Respuestas, IdDetalle`. Lo llaman **dos** caminos: `GuardarHoras` (una persona editando) y `AbrirPeriodo` (62 filas de golpe al abrir o reabrir). **Sólo el primero debe auditar**: registrar 62 filas cada vez que alguien abre un período llenaría la auditoría de ruido y enterraría los cambios reales.

- [ ] **Step 1: Escribir el script**

```sql
/* ============================================================================
   Horas Extras - fase 3: la auditoria y el cierre del periodo.

   La auditoria se escribe DENTRO de Sp_RTA_HeGuardarFila y no desde C#: si se
   escribiera despues de guardar, un fallo entre las dos operaciones dejaria un
   cambio de horas sin rastro, y el rastro es justo lo que el 8 funcional exige
   que no falte.
   ============================================================================ */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------ 1. la auditoria admite texto --------- */
/* ValorAnterior y ValorNuevo son DECIMAL(9,2) y sirven para Horas50 y Horas100.
   Observacion tambien es editable desde la pantalla y es texto: sin estas dos
   columnas no hay donde registrar su cambio. Se anaden en vez de ensanchar las
   existentes para no perder el tipo de las que si son numericas. */
IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_DetalleAuditoria') AND name = 'TextoAnterior')
BEGIN
    ALTER TABLE dbo.HE_DetalleAuditoria ADD TextoAnterior VARCHAR(400) NULL;
    PRINT 'HE_DetalleAuditoria: columna TextoAnterior agregada.';
END
ELSE
    PRINT 'HE_DetalleAuditoria: TextoAnterior ya existia.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_DetalleAuditoria') AND name = 'TextoNuevo')
BEGIN
    ALTER TABLE dbo.HE_DetalleAuditoria ADD TextoNuevo VARCHAR(400) NULL;
    PRINT 'HE_DetalleAuditoria: columna TextoNuevo agregada.';
END
ELSE
    PRINT 'HE_DetalleAuditoria: TextoNuevo ya existia.';
GO

/* Para responder "que le paso a esta fila" sin recorrer la tabla entera. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
                WHERE object_id = OBJECT_ID('dbo.HE_DetalleAuditoria') AND name = 'IX_HE_DetalleAuditoria_Detalle_Fecha')
BEGIN
    CREATE INDEX IX_HE_DetalleAuditoria_Detalle_Fecha
        ON dbo.HE_DetalleAuditoria (IdDetalle, Fecha DESC);
    PRINT 'HE_DetalleAuditoria: indice por detalle y fecha creado.';
END
GO

/* --------------------------- 2. guardar fila, ahora con auditoria ---------- */
IF OBJECT_ID('dbo.Sp_RTA_HeGuardarFila','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeGuardarFila;
GO
/* Identico al de la fase 2 salvo por @Auditar y el bloque que registra los
   cambios. Lo llaman dos caminos y solo uno audita:
     GuardarHoras  -> @Auditar = 1, es una persona editando una fila
     AbrirPeriodo  -> @Auditar = 0, son 62 filas de golpe al abrir o reabrir;
                      registrarlas enterraria los cambios reales bajo el ruido.

   Respuestas: 0 bien, -1 el periodo no existe, -2 el periodo no esta Abierto.
   Devuelve ademas IdDetalle, que es con lo que se escribe la auditoria. */
CREATE PROCEDURE dbo.Sp_RTA_HeGuardarFila
    @IdPeriodo               INT,
    @IdEmpleado              BIGINT,
    @CedulaSnapshot          VARCHAR(20),
    @NombreSnapshot          VARCHAR(400),
    @EmpresaSnapshot         VARCHAR(120),
    @CargoSnapshot           VARCHAR(200),
    @JornadaHorasDiaSnapshot INT,
    @SalarioBaseSnapshot     DECIMAL(18,2),
    @AplicaHESnapshot        BIT,
    @Divisor                 INT,
    @ValorHoraOrdinaria      DECIMAL(18,6),
    @ValorHora50             DECIMAL(18,6),
    @ValorHora100            DECIMAL(18,6),
    @Horas50                 DECIMAL(9,2),
    @Horas100                DECIMAL(9,2),
    @Total50                 DECIMAL(18,2),
    @Total100                DECIMAL(18,2),
    @TotalHoras              DECIMAL(9,2),
    @TotalHE                 DECIMAL(18,2),
    @Observacion             VARCHAR(400),
    @Usuario                 VARCHAR(50),
    @Ip                      VARCHAR(64),
    @Auditar                 BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Estado VARCHAR(10);
    SELECT @Estado = EstadoPeriodo FROM dbo.HE_Periodo WHERE IdPeriodo = @IdPeriodo;

    IF @Estado IS NULL
    BEGIN
        SELECT Respuestas = -1, IdDetalle = CAST(NULL AS INT);
        RETURN;
    END

    IF @Estado <> 'Abierto'
    BEGIN
        SELECT Respuestas = -2, IdDetalle = CAST(NULL AS INT);
        RETURN;
    END

    DECLARE @IdDetalle INT;

    /* Los valores de ANTES, para poder compararlos. Se leen aunque no se vaya a
       auditar: es una sola lectura de una fila por su indice unico. */
    DECLARE @Horas50Antes DECIMAL(9,2), @Horas100Antes DECIMAL(9,2), @ObsAntes VARCHAR(400);

    SELECT @IdDetalle = IdDetalle, @Horas50Antes = Horas50,
           @Horas100Antes = Horas100, @ObsAntes = Observacion
      FROM dbo.HE_Detalle
     WHERE IdPeriodo = @IdPeriodo AND IdEmpleado = @IdEmpleado;

    IF @IdDetalle IS NULL
    BEGIN
        INSERT INTO dbo.HE_Detalle
            (IdPeriodo, IdEmpleado, CedulaSnapshot, NombreSnapshot, EmpresaSnapshot,
             CargoSnapshot, JornadaHorasDiaSnapshot, SalarioBaseSnapshot, AplicaHESnapshot,
             Divisor, ValorHoraOrdinaria, ValorHora50, ValorHora100,
             Horas50, Horas100, Total50, Total100, TotalHoras, TotalHE,
             Observacion, Usu_Modificacion, Ip_Modificacion)
        VALUES
            (@IdPeriodo, @IdEmpleado, @CedulaSnapshot, @NombreSnapshot, @EmpresaSnapshot,
             @CargoSnapshot, @JornadaHorasDiaSnapshot, @SalarioBaseSnapshot, @AplicaHESnapshot,
             @Divisor, @ValorHoraOrdinaria, @ValorHora50, @ValorHora100,
             @Horas50, @Horas100, @Total50, @Total100, @TotalHoras, @TotalHE,
             @Observacion, @Usuario, @Ip);

        SET @IdDetalle = CAST(SCOPE_IDENTITY() AS INT);
    END
    ELSE
    BEGIN
        UPDATE dbo.HE_Detalle
           SET CedulaSnapshot = @CedulaSnapshot, NombreSnapshot = @NombreSnapshot,
               EmpresaSnapshot = @EmpresaSnapshot, CargoSnapshot = @CargoSnapshot,
               JornadaHorasDiaSnapshot = @JornadaHorasDiaSnapshot,
               SalarioBaseSnapshot = @SalarioBaseSnapshot, AplicaHESnapshot = @AplicaHESnapshot,
               Divisor = @Divisor, ValorHoraOrdinaria = @ValorHoraOrdinaria,
               ValorHora50 = @ValorHora50, ValorHora100 = @ValorHora100,
               Horas50 = @Horas50, Horas100 = @Horas100,
               Total50 = @Total50, Total100 = @Total100,
               TotalHoras = @TotalHoras, TotalHE = @TotalHE,
               Observacion = @Observacion,
               Fec_Modificacion = SYSDATETIME(), Usu_Modificacion = @Usuario, Ip_Modificacion = @Ip
         WHERE IdDetalle = @IdDetalle;

        /* Una fila por campo que de verdad cambio. Los tres INSERT son
           independientes a proposito: si alguien cambia las horas al 50% y la
           observacion en el mismo guardado, son dos hechos distintos y se
           registran por separado. */
        IF @Auditar = 1
        BEGIN
            IF ISNULL(@Horas50Antes, -1) <> ISNULL(@Horas50, -1)
                INSERT INTO dbo.HE_DetalleAuditoria (IdDetalle, Campo, ValorAnterior, ValorNuevo, Usuario, Ip)
                VALUES (@IdDetalle, 'Horas50', @Horas50Antes, @Horas50, @Usuario, @Ip);

            IF ISNULL(@Horas100Antes, -1) <> ISNULL(@Horas100, -1)
                INSERT INTO dbo.HE_DetalleAuditoria (IdDetalle, Campo, ValorAnterior, ValorNuevo, Usuario, Ip)
                VALUES (@IdDetalle, 'Horas100', @Horas100Antes, @Horas100, @Usuario, @Ip);

            IF ISNULL(@ObsAntes, '') <> ISNULL(@Observacion, '')
                INSERT INTO dbo.HE_DetalleAuditoria (IdDetalle, Campo, TextoAnterior, TextoNuevo, Usuario, Ip)
                VALUES (@IdDetalle, 'Observacion', @ObsAntes, @Observacion, @Usuario, @Ip);
        END
    END

    SELECT Respuestas = 0, IdDetalle = @IdDetalle;
END
GO

/* ----------------------------------------- 3. cerrar el periodo ------------ */
IF OBJECT_ID('dbo.Sp_RTA_HeCerrarPeriodo','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeCerrarPeriodo;
GO
/* Respuestas: 0 cerrado, -1 no existe, -2 no estaba Abierto,
               -3 hay filas con valor hora en cero.

   El -3 es la validacion del 6 funcional: una fila cuyo valor hora salio en
   cero es un dato malo -falta el sueldo- y cerrar el mes con ella dejaria
   congelado un pago que nadie calculo. */
CREATE PROCEDURE dbo.Sp_RTA_HeCerrarPeriodo
    @IdPeriodo INT,
    @Usuario   VARCHAR(50),
    @Ip        VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Estado VARCHAR(10);
    SELECT @Estado = EstadoPeriodo FROM dbo.HE_Periodo WHERE IdPeriodo = @IdPeriodo;

    IF @Estado IS NULL
    BEGIN
        SELECT Respuestas = -1, FilasConProblema = 0;
        RETURN;
    END

    IF @Estado <> 'Abierto'
    BEGIN
        SELECT Respuestas = -2, FilasConProblema = 0;
        RETURN;
    END

    /* Solo cuentan las filas de quien SI aplica horas extras: a quien no
       aplica se le calcula el valor hora igual, pero no cobra, y bloquear el
       cierre por su culpa seria bloquearlo para siempre. */
    DECLARE @EnCero INT;
    SELECT @EnCero = COUNT(*)
      FROM dbo.HE_Detalle
     WHERE IdPeriodo = @IdPeriodo AND AplicaHESnapshot = 1 AND ValorHoraOrdinaria = 0;

    IF @EnCero > 0
    BEGIN
        SELECT Respuestas = -3, FilasConProblema = @EnCero;
        RETURN;
    END

    UPDATE dbo.HE_Periodo
       SET EstadoPeriodo = 'Cerrado', FechaCierre = SYSDATETIME(),
           UsuarioCierre = @Usuario, Ip_Modificacion = @Ip
     WHERE IdPeriodo = @IdPeriodo;

    SELECT Respuestas = 0, FilasConProblema = 0;
END
GO

/* --------------------------------------- 4. reabrir el periodo ------------- */
IF OBJECT_ID('dbo.Sp_RTA_HeReabrirPeriodo','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeReabrirPeriodo;
GO
/* Respuestas: 0 reabierto, -1 no existe, -2 no estaba Cerrado.

   Quien puede llamar a esto lo decide la capa web -solo el perfil 18-, no este
   procedimiento: la base no conoce perfiles. Aqui se comprueba solo que el
   periodo este en un estado desde el que reabrir tenga sentido.

   FechaCierre y UsuarioCierre NO se borran: son el rastro de que este mes
   estuvo cerrado alguna vez y quien lo cerro. Borrarlos haria que un periodo
   reabierto fuera indistinguible de uno que nunca se cerro. */
CREATE PROCEDURE dbo.Sp_RTA_HeReabrirPeriodo
    @IdPeriodo INT,
    @Usuario   VARCHAR(50),
    @Ip        VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Estado VARCHAR(10);
    SELECT @Estado = EstadoPeriodo FROM dbo.HE_Periodo WHERE IdPeriodo = @IdPeriodo;

    IF @Estado IS NULL
    BEGIN
        SELECT Respuestas = -1;
        RETURN;
    END

    IF @Estado <> 'Cerrado'
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    UPDATE dbo.HE_Periodo
       SET EstadoPeriodo = 'Abierto', Ip_Modificacion = @Ip
     WHERE IdPeriodo = @IdPeriodo;

    SELECT Respuestas = 0;
END
GO

/* ------------------------------------------------- 5. aserciones ----------- */

DECLARE @Fallos INT = 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_DetalleAuditoria') AND name = 'TextoAnterior')
BEGIN
    RAISERROR('FALLO: HE_DetalleAuditoria.TextoAnterior no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_DetalleAuditoria') AND name = 'TextoNuevo')
BEGIN
    RAISERROR('FALLO: HE_DetalleAuditoria.TextoNuevo no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.Sp_RTA_HeGuardarFila','P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeGuardarFila no quedo creado.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.Sp_RTA_HeCerrarPeriodo','P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeCerrarPeriodo no quedo creado.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.Sp_RTA_HeReabrirPeriodo','P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_HeReabrirPeriodo no quedo creado.', 16, 1);
    SET @Fallos += 1;
END

/* El parametro nuevo tiene que existir Y tener valor por omision, o las
   llamadas de 22 parametros que ya hay en produccion dejarian de funcionar. */
IF NOT EXISTS (SELECT 1 FROM sys.parameters
                WHERE object_id = OBJECT_ID('dbo.Sp_RTA_HeGuardarFila')
                  AND name = '@Auditar' AND has_default_value = 1)
BEGIN
    RAISERROR('FALLO: @Auditar no existe o no tiene valor por omision.', 16, 1);
    SET @Fallos += 1;
END

IF @Fallos > 0
    RAISERROR('FALLO: %d verificaciones no pasaron.', 16, 1, @Fallos);
ELSE
    PRINT 'Horas Extras fase 3: auditoria y cierre listos.';
GO
```

- [ ] **Step 2: Comprobar que no lleva tildes ni eñes**

Run: `LC_ALL=C grep -n '[^ -~\t]' docs/sql/2026-09-16-horas-extras-fase3.sql`
Expected: sin salida.

- [ ] **Step 3: Comprobar que el bloque de aserciones es un solo batch**

Run: `grep -n "^GO\s*$\|DECLARE @Fallos\|@Fallos" docs/sql/2026-09-16-horas-extras-fase3.sql`
Expected: el `DECLARE @Fallos` y su último uso están **entre el mismo par de `GO`**. Si hubiera un `GO` en medio, el script entero falla con «must declare the scalar variable». Ya rompió scripts dos veces en este proyecto.

- [ ] **Step 4: Comprobar que el procedimiento recreado no perdió nada**

`Sp_RTA_HeGuardarFila` se recrea entero. Compara su lista de parámetros y su `UPDATE` contra los de `docs/sql/2026-09-16-horas-extras-fase2.sql`: **los 22 parámetros originales tienen que seguir ahí, con los mismos nombres y tipos**, y el `UPDATE` tiene que escribir las mismas 18 columnas de datos. Si falta una, `CapaNegocio.FilaSinCambios` la compararía contra un valor que ya nadie escribe.

Deja constancia en tu reporte de que lo comprobaste y de cuántos parámetros y columnas contaste.

- [ ] **Step 5: No ejecutar nada**

**No ejecutes este script contra ninguna base de datos ni abras conexión.** El usuario lo ejecuta en producción. Deja dicho en tu reporte, con estas palabras: *"El script está listo y commiteado. No lo ejecuté. Hace falta correrlo contra producción antes de que el cierre funcione."*

- [ ] **Step 6: Commit**

```bash
git add docs/sql/2026-09-16-horas-extras-fase3.sql
git commit -m "feat(horas-extras): la auditoria de lo editable y los procedimientos de cierre"
```

---

## Task 2: Cerrar y reabrir, en las capas

**Files:**
- Modify: `CapaDato/DaoHorasExtras.cs`
- Modify: `CapaNegocio/NegHorasExtrasPantalla.cs`
- Modify: `CapaPruebas/NegHorasExtrasPantallaTests.cs`

**Interfaces:**
- Consumes: `Sp_RTA_HeCerrarPeriodo`, `Sp_RTA_HeReabrirPeriodo` y el `@Auditar` de `Sp_RTA_HeGuardarFila` (tarea 1).
- Produces:
  - `DaoHorasExtras.CerrarPeriodo(int idPeriodo, string usuario, string ip, out int filasConProblema)` → `int` (Respuestas)
  - `DaoHorasExtras.ReabrirPeriodo(int idPeriodo, string usuario, string ip)` → `int` (Respuestas)
  - `DaoHorasExtras.GuardarFila(int idPeriodo, EntHeFila fila, string usuario, string ip, bool auditar)` → `int`
  - `NegHorasExtrasPantalla.CerrarPeriodo(int idPeriodo, string usuario, string ip)` → `EntRespuesta`
  - `NegHorasExtrasPantalla.ReabrirPeriodo(int idPeriodo, string usuario, string ip)` → `EntRespuesta`

**Contexto que necesitas.**

`GuardarFila` gana un parámetro `auditar`. **Los dos llamadores existentes tienen que pasarlo explícitamente**, y con valores distintos:
- `GuardarHoras` pasa `true` — es una persona editando una fila.
- `AbrirPeriodo` pasa `false` — son 62 filas de golpe; auditarlas enterraría los cambios reales bajo el ruido.

**No le pongas valor por omisión al parámetro de C#.** El valor por omisión existe en el procedimiento SQL para no romper nada que ya esté desplegado, pero en C# un parámetro obligatorio obliga a cada llamador a decidir. Es el mismo criterio que ya se aplicó con `salarioCongelado` en la fase 2, y por el mismo motivo: un campo que se puede olvidar se olvida.

- [ ] **Step 1: Escribir las pruebas primero**

Añade a `CapaPruebas/NegHorasExtrasPantallaTests.cs`:

```csharp
        /// <summary>
        /// El cierre traduce cada codigo del procedimiento a un mensaje distinto.
        /// Un "no se pudo cerrar" generico obligaria a la persona a adivinar si
        /// el problema es suyo, del periodo, o de los datos.
        /// </summary>
        [TestMethod]
        public void MensajeDeCierre_CadaCodigoDiceAlgoDistinto()
        {
            string noExiste = NegHorasExtrasPantalla.MensajeDeCierre(-1, 0);
            string noAbierto = NegHorasExtrasPantalla.MensajeDeCierre(-2, 0);
            string enCero = NegHorasExtrasPantalla.MensajeDeCierre(-3, 7);

            Assert.AreNotEqual(noExiste, noAbierto);
            Assert.AreNotEqual(noAbierto, enCero);
            StringAssert.Contains(enCero, "7", "el mensaje tiene que decir cuantas filas estan mal");
        }

        [TestMethod]
        public void MensajeDeCierre_ConUnaSolaFila_NoDicePluralRaro()
        {
            string uno = NegHorasExtrasPantalla.MensajeDeCierre(-3, 1);

            StringAssert.Contains(uno, "1");
            Assert.IsFalse(uno.Contains("1 filas"), "con una fila el texto no debe decir '1 filas'");
        }
```

- [ ] **Step 2: Correrlas y verlas fallar**

```bash
MSYS_NO_PATHCONV=1 "C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/MSBuild.exe" CapaPruebas/CapaPruebas.csproj -t:Build -p:Configuration=Debug -v:quiet -nologo
```
Expected: **no compila**, con `'NegHorasExtrasPantalla' no contiene una definicion para 'MensajeDeCierre'`.

- [ ] **Step 3: Los dos métodos del DAO**

En `CapaDato/DaoHorasExtras.cs`, siguiendo el molde de `CrearPeriodo` que ya está ahí:

```csharp
        /// <summary>
        /// Cierra el periodo. Respuestas: 0 bien, -1 no existe, -2 no estaba
        /// abierto, -3 hay filas con valor hora en cero -y cuantas, en
        /// filasConProblema-.
        /// </summary>
        public static int CerrarPeriodo(int idPeriodo, string usuario, string ip, out int filasConProblema)
        {
            int resultado = -1;
            int conProblema = 0;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeCerrarPeriodo", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPeriodo", SqlDbType.Int).Value = idPeriodo;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario ?? "";
                cmd.Parameters.Add("@Ip", SqlDbType.VarChar, 64).Value = ip ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        resultado = Convert.ToInt32(dr["Respuestas"]);
                        conProblema = Convert.ToInt32(dr["FilasConProblema"]);
                    }
                }
            }

            filasConProblema = conProblema;
            return resultado;
        }

        /// <summary>
        /// Reabre un periodo cerrado. Respuestas: 0 bien, -1 no existe,
        /// -2 no estaba cerrado.
        ///
        /// Quien puede llamar a esto lo decide la capa web: la base no conoce
        /// perfiles y este metodo no comprueba ninguno.
        /// </summary>
        public static int ReabrirPeriodo(int idPeriodo, string usuario, string ip)
        {
            int resultado = -1;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeReabrirPeriodo", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPeriodo", SqlDbType.Int).Value = idPeriodo;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario ?? "";
                cmd.Parameters.Add("@Ip", SqlDbType.VarChar, 64).Value = ip ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read()) { resultado = Convert.ToInt32(dr["Respuestas"]); }
                }
            }

            return resultado;
        }
```

Y `GuardarFila` gana el parámetro, que **se añade al comando**:

```csharp
        public static int GuardarFila(int idPeriodo, EntHeFila f, string usuario, string ip, bool auditar)
```
con, junto a los demás parámetros:
```csharp
                cmd.Parameters.Add("@Auditar", SqlDbType.Bit).Value = auditar;
```

- [ ] **Step 4: La orquestación**

En `CapaNegocio/NegHorasExtrasPantalla.cs`:

```csharp
        /// <summary>
        /// Traduce el codigo del procedimiento de cierre a algo que una persona
        /// pueda leer. Funcion pura y publica para poder probarla sin base.
        /// </summary>
        public static string MensajeDeCierre(int codigo, int filasConProblema)
        {
            if (codigo == -1) { return "Ese periodo no existe."; }
            if (codigo == -2) { return "Solo se puede cerrar un periodo abierto."; }

            if (codigo == -3)
            {
                string cuantas = filasConProblema == 1
                                 ? "Hay 1 fila"
                                 : "Hay " + filasConProblema.ToString() + " filas";

                return cuantas + " sin sueldo vigente, con el valor hora en cero. "
                     + "Corrija el sueldo de esas personas antes de cerrar: el periodo "
                     + "quedaria congelado con un pago que nadie calculo.";
            }

            return "No se pudo cerrar el periodo.";
        }

        /// <summary>Cierra el periodo y devuelve la pantalla ya en estado cerrado.</summary>
        public static EntRespuesta CerrarPeriodo(int idPeriodo, string usuario, string ip)
        {
            EntRespuesta respuesta = new EntRespuesta();

            int filasConProblema;
            int codigo = DaoHorasExtras.CerrarPeriodo(idPeriodo, usuario, ip, out filasConProblema);

            if (codigo != 0)
            {
                respuesta.estado = "0";
                respuesta.mensaje = MensajeDeCierre(codigo, filasConProblema);
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            EntRespuesta pantalla = CargarPantalla(idPeriodo);

            if (pantalla.estado == "1")
            {
                pantalla.mensaje = "Periodo cerrado. Ya no admite cambios.";
                pantalla.tipoMensaje = "success";
            }

            return pantalla;
        }

        /// <summary>
        /// Reabre un periodo cerrado.
        ///
        /// NO comprueba perfiles: eso es de la capa web, que es la unica que
        /// conoce la sesion. Este metodo asume que quien llega aqui ya tiene
        /// permiso, y por eso el handler tiene que comprobarlo ANTES.
        /// </summary>
        public static EntRespuesta ReabrirPeriodo(int idPeriodo, string usuario, string ip)
        {
            EntRespuesta respuesta = new EntRespuesta();

            int codigo = DaoHorasExtras.ReabrirPeriodo(idPeriodo, usuario, ip);

            if (codigo == -1)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "Ese periodo no existe.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            if (codigo != 0)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "Solo se puede reabrir un periodo cerrado.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            EntRespuesta pantalla = CargarPantalla(idPeriodo);

            if (pantalla.estado == "1")
            {
                pantalla.mensaje = "Periodo reabierto. Vuelve a admitir cambios.";
                pantalla.tipoMensaje = "success";
            }

            return pantalla;
        }
```

- [ ] **Step 5: Actualizar los dos llamadores de `GuardarFila`**

En `AbrirPeriodo`, la llamada pasa `false`. En `GuardarHoras`, `true`. **El compilador te obligará a tocar los dos** — ése es el motivo de no ponerle valor por omisión.

Deja un comentario en cada uno diciendo por qué: en `AbrirPeriodo`, que auditar 62 filas por apertura enterraría los cambios reales; en `GuardarHoras`, que es la edición de una persona y es justo lo que el §8 exige registrar.

- [ ] **Step 6: Correr la suite**

```bash
MSYS_NO_PATHCONV=1 "C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe" "CapaPruebas/bin/Debug/CapaPruebas.dll"
```
Expected: **154 de 154** (152 previas + 2 nuevas). Si el número no cuadra, **dilo en tu reporte en vez de ajustarlo.**

- [ ] **Step 7: La prueba de ida y vuelta**

Cambia `MensajeDeCierre` para que el caso `-3` devuelva el mismo texto que el `-2`, corre las pruebas y comprueba que `MensajeDeCierre_CadaCodigoDiceAlgoDistinto` **falla**. Deshaz el cambio. **Escribe en tu reporte qué dijo el fallo.**

- [ ] **Step 8: Commit**

```bash
git add CapaDato/DaoHorasExtras.cs CapaNegocio/NegHorasExtrasPantalla.cs CapaPruebas/NegHorasExtrasPantallaTests.cs
git commit -m "feat(horas-extras): cerrar y reabrir el periodo, con la auditoria solo en la edicion"
```

---

## Task 3: Los botones, con el perfil que corresponde

**Files:**
- Modify: `ReporteTareas/Formulario/AdministrarHorasExtras.ashx.cs`
- Modify: `ReporteTareas/Formulario/HorasExtras.aspx`
- Modify: `ReporteTareas/js/horasExtras.js`

**Interfaces:**
- Consumes: `NegHorasExtrasPantalla.CerrarPeriodo` y `.ReabrirPeriodo` (tarea 2).
- Produces: las acciones JSON `CerrarPeriodo` y `ReabrirPeriodo`.

**Contexto que necesitas, y es donde está el riesgo de esta tarea.**

El handler ya tiene una comprobación de perfil para entrar a la pantalla:

```csharp
        private static readonly int[] PerfilesAutorizados = { 14, 18 };

        private static bool TienePermiso(HttpContext context)
        {
            int idPerfil;
            if (!int.TryParse(Convert.ToString(context.Session["Id_Perfil"]), out idPerfil)) { return false; }
            return Array.IndexOf(PerfilesAutorizados, idPerfil) >= 0;
        }
```

**Reabrir exige más que eso: sólo el perfil 18.** Es una decisión del usuario y es de separación de funciones — quien opera el mes puede cerrarlo, pero deshacer un cierre formal exige un perfil distinto.

**La comprobación va en el servidor, no en el cliente.** Ocultar el botón es cortesía; lo que impide reabrir es el handler. Si sólo ocultaras el botón, cualquiera de los 8 usuarios del perfil 14 podría reabrir un período con una petición hecha a mano.

- [ ] **Step 1: La segunda comprobación de perfil en el handler**

```csharp
        /// <summary>Talento Humano (14) y Super Admin (18): pueden entrar y cerrar.</summary>
        private static readonly int[] PerfilesAutorizados = { 14, 18 };

        /// <summary>
        /// Solo Super Admin (18) puede reabrir un periodo cerrado. Separacion de
        /// funciones: quien opera el mes puede cerrarlo, pero deshacer un cierre
        /// formal exige otro perfil.
        ///
        /// Se comprueba AQUI y no solo ocultando el boton: ocultarlo es cortesia
        /// para quien no puede, no una barrera para quien no debe.
        /// </summary>
        private static readonly int[] PerfilesQueReabren = { 18 };

        private static bool EstaEn(HttpContext context, int[] perfiles)
        {
            int idPerfil;
            if (!int.TryParse(Convert.ToString(context.Session["Id_Perfil"]), out idPerfil)) { return false; }
            return Array.IndexOf(perfiles, idPerfil) >= 0;
        }
```

`TienePermiso` pasa a ser `EstaEn(context, PerfilesAutorizados)`.

Y la acción de reabrir comprueba lo suyo antes de hacer nada:

```csharp
        private string ReabrirPeriodo(HttpContext context, dynamic p)
        {
            if (!EstaEn(context, PerfilesQueReabren))
            {
                return Mensaje("0", "Su perfil no puede reabrir un período cerrado.", "warning");
            }

            EntRespuesta r = NegHorasExtrasPantalla.ReabrirPeriodo(
                Entero(p["idPeriodo"]), Usuario(context), Ip(context));
            return new JavaScriptSerializer().Serialize(r);
        }
```

Las dos acciones nuevas van envueltas en `try/catch` como las otras cuatro, devolviendo un `EntRespuesta` JSON. **Un handler de esta pantalla sin `try/catch` deja salir la excepción como página de error de ASP.NET, y el cliente sólo ve «no se pudo contactar al servidor».**

- [ ] **Step 2: Los botones en la pantalla**

En la cabecera, junto a «Guardar»:
- **«Cerrar período»**, visible sólo cuando el período está abierto.
- **«Reabrir período»**, visible sólo cuando está cerrado **y** el perfil de la sesión es el 18.

Para lo segundo el cliente necesita saber el perfil. `HorasExtras.aspx.cs` ya lo lee de la sesión —la fase 2 le añadió el `Response.Redirect` para quien no sea 14 ni 18—, así que sólo hay que exponerlo. En el `code-behind`, un campo protegido:

```csharp
        /// <summary>
        /// Solo para que la pantalla decida si ENSEnA el boton de reabrir. La
        /// barrera de verdad esta en AdministrarHorasExtras.ashx.cs: esto es
        /// cortesia para quien no puede, no una barrera para quien no debe.
        /// </summary>
        protected bool PuedeReabrir
        {
            get
            {
                int idPerfil;
                return int.TryParse(Convert.ToString(Session["Id_Perfil"]), out idPerfil) && idPerfil == 18;
            }
        }
```

y en el `.aspx`, dentro del bloque de scripts:

```aspx
        <script type="text/javascript">
            var HE_PUEDE_REABRIR = <%= PuedeReabrir ? "true" : "false" %>;
        </script>
```

El `.js` la usa **sólo para mostrar u ocultar**. La barrera real es la del paso 1.

**Ninguno de los dos botones hace nada sin confirmación**, y las dos confirmaciones dicen lo que va a pasar, no «¿está seguro?»:
- Cerrar: que el período dejará de admitir cambios.
- Reabrir: que el período volverá a admitir cambios y que quedará registrado quién lo reabrió.

Usa el modal de la casa, el mismo patrón que el de confirmar el pegado.

- [ ] **Step 3: El indicador de estado**

El §5.1 funcional pide color: verde `ABIERTO`, gris `CERRADO`, rojo `ANULADO`. Los tres estados existen en `EntHePeriodo.EstadoPeriodo`. **No inventes estilos nuevos**: usa las variables de `css/dos-tema.css`, como hace el resto de la pantalla.

Cuando está cerrado, además del estado en gris, **que se vea de un vistazo quién lo cerró y cuándo** — `EntHePeriodo.UsuarioCierre` y `FechaCierre` ya viajan en el payload desde la fase 2 y hoy no se muestran.

- [ ] **Step 4: Comprobar el BOM**

```bash
head -c 3 ReporteTareas/Formulario/HorasExtras.aspx | od -An -tx1
```
Expected: `ef bb bf`. Sin eso la página sale con caracteres raros en producción; ya pasó con `MiPerfil.aspx`.

- [ ] **Step 5: Compilar**

```bash
MSYS_NO_PATHCONV=1 "C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/MSBuild.exe" ReporteTareas/ReporteTareas.csproj -t:Build -p:Configuration=Debug -v:quiet -nologo
```

- [ ] **Step 6: Commit**

```bash
git add ReporteTareas/Formulario/AdministrarHorasExtras.ashx.cs ReporteTareas/Formulario/HorasExtras.aspx ReporteTareas/js/horasExtras.js
git commit -m "feat(horas-extras): cerrar y reabrir desde la pantalla, con el perfil que corresponde"
```

---

## Task 4: Verificar que el bloqueo de edición es real

**Files:**
- Modify: `CapaPruebas/NegHorasExtrasPantallaTests.cs` (si hay algo probable sin base)

**Por qué existe esta tarea y por qué no construye nada.**

La fase 2 dejó el bloqueo hecho: el servidor devuelve `-2` para un período no abierto y la pantalla oculta el botón de guardar y deshabilita las celdas. Esta fase **no lo reescribe: lo audita**, porque ahora es la barrera que sostiene un cierre formal y no sólo una cortesía.

Tu entregable es un **informe**, no código, salvo que encuentres un hueco.

- [ ] **Step 1: Recorrer los caminos de escritura**

Enumera **todos** los caminos por los que un dato de `HE_Detalle` puede cambiar, y di para cada uno qué pasa si el período está cerrado. Como mínimo: `GuardarHoras`, `AbrirPeriodo`, `CerrarPeriodo`, `ReabrirPeriodo` y cualquier otro que encuentres.

Para cada uno: ¿la comprobación está en el procedimiento, en `CapaNegocio`, o en las dos? Una que esté sólo en `CapaNegocio` se puede saltar con una petición hecha a mano.

- [ ] **Step 2: Los dos casos que más me preocupan**

1. **Cerrar un período mientras alguien tiene la pantalla abierta.** Esa persona tiene una grilla que cree abierta. Si pulsa Guardar sobre 40 filas sucias, ¿qué ve? ¿Se entera de que el período se cerró, o recibe 40 mensajes?
2. **Reabrir y volver a cerrar.** ¿Queda `FechaCierre` del primer cierre o del segundo? ¿Se puede distinguir un período que nunca se cerró de uno que se cerró y se reabrió?

- [ ] **Step 3: Escribir el informe**

En `.superpowers/sdd/<carpeta de esta fase>/task-4-report.md`: la tabla de caminos, la respuesta a los dos casos, y **cualquier hueco con el caso concreto que lo dispara**. Si no hay ninguno, dilo — un informe honesto que dice «está cerrado» vale más que un hallazgo inventado.

- [ ] **Step 4: Sólo si encontraste un hueco, arreglarlo y commitear**

Con su prueba si es alcanzable sin base de datos. Si no lo es, dilo en el informe en vez de forzar una prueba artificial.

---

## Cierre de la fase

1. **Regenerar el paquete de despliegue, y hacerlo AL FINAL.** El repo versiona `ReporteTareas/bin/` y `obj/Release/Package/PackageTmp` a propósito. Este paquete se ha quedado atrás cinco veces en este proyecto, y **no por olvidarlo: por regenerarlo antes del último commit de código**. Comprueba que contiene algo escrito en ese último commit, no que los archivos existan.
2. **Orden de ejecución en producción:** `docs/sql/2026-09-16-horas-extras-fase3.sql`, y después los binarios. Los scripts van siempre antes (`DESPLIEGUE.md`).
3. **Comprobación manual mínima**, y la primera es la que importa:
   - Con un usuario del **perfil 14**: abrir un período, digitar horas, guardar, y comprobar en `HE_DetalleAuditoria` que quedó una fila con el valor anterior, el nuevo, el usuario y la IP. Cerrar el período y comprobar que la grilla queda de sólo lectura. **Comprobar que el botón de reabrir NO aparece.**
   - Con un usuario del **perfil 18**: reabrir el período y comprobar que vuelve a admitir cambios.
   - Volver a abrir el período desde el botón «Abrir/actualizar» y comprobar que **NO** se generaron 62 filas de auditoría.

**Lo que esta fase deja pendiente, a propósito:** la exportación para nómina, que espera el formato; y el umbral mensual de los topes, que espera a RRHH. Ninguna de las dos es un olvido.
