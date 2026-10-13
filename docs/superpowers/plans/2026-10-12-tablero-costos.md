# Tablero de Costos — Plan de implementación

> **Para quien ejecute esto:** SUB-SKILL REQUERIDA: usar `superpowers:subagent-driven-development` (recomendado) o `superpowers:executing-plans` para ejecutar tarea por tarea. Los pasos usan casillas (`- [ ]`) para llevar la cuenta.

**Objetivo:** Una pantalla nueva, `TableroCostos.aspx`, que reemplaza el libro `Dashboard_Tareas_Aranda.xlsx`. Muestra las horas de `R_DetTareasAranda` clasificadas en Normal / Extra / Traslado, valoradas en dólares con los salarios de Horas Extras. La ven solo Talento Humano (perfil 14) y Gerencia (perfil 18).

**Arquitectura:**
- **SQL solo trae datos.** Un procedimiento devuelve tres resultados: las actividades del rango, la tarifa de cada responsable y los parámetros vigentes.
- **Las reglas viven en `CapaNegocio`**, en `NegTableroCostos`. Es una clase pura, probada con MSTest, que clasifica, valora, filtra y arma todas las secciones.
- **El handler guarda en sesión** las actividades ya clasificadas del rango y aplica los filtros sobre ellas.
- **La página solo dibuja** lo que devuelve el handler, con Chart.js local.

**Tecnologías:** SQL Server (T-SQL), C# .NET Framework 4.6.1 (C# 7.3), MSTest v1, EPPlus 6.2.7, jQuery 1.9.1, Bootstrap 3, Chart.js (`js/chart.umd.js`, ya está en el proyecto).

**Spec:** `docs/superpowers/specs/2026-10-12-tablero-costos-design.md`

**Referencia funcional:** `C:\ReporteExcel\generar_dashboard.py` y el Excel que genera. Están fuera del repositorio y no se versionan.

### Desviaciones respecto al spec (deliberadas)

1. **Tarifa por persona, no por la primera cuenta.** El script arma la hoja Tarifas por **nombre** de responsable y toma el `Cod_Usuario` de la primera actividad que aparece en el período. Si una persona registra con dos cuentas y solo una está en `Empleados`, el resultado depende del orden de las fechas y cambia con el rango. Aquí, para cada nombre se elige, entre todos sus códigos, **el primero (en orden alfabético) que tiene salario**; si ninguno tiene, el primero que tenga tarifa. Si el cuadre muestra una diferencia contra el Excel, la Task 7 manda revisar primero a esas personas.
2. **«Consultar» vuelve a leer la base.** El spec decía que el procedimiento se llama solo al cambiar Desde/Hasta. Los filtros siguen trabajando sobre la sesión, pero el botón **Consultar** fuerza la relectura, para que quien acaba de completar un salario no tenga que cambiar de rango para verlo.
3. **El aviso de filtros vacíos se generaliza.** El Excel avisa en tres casos fijos. Aquí se avisa cuando **cualquiera** de Mes, Responsable, Empresa o Contrato tiene un valor elegido que no aparece con los demás filtros, y cuando Recargo o Estado HE dejan el resultado vacío. Lleva el mismo texto final: «Elija otro valor o Todos.».
4. **El detalle viaja compacto.** En el JSON, cada actividad del detalle es un arreglo de 13 posiciones en vez de un objeto con nombres. En 24 meses son unas 22.000 filas, y así el JSON pesa cerca de la mitad.

## Restricciones globales

- **Compilar solo con** `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe`, **desde PowerShell**:
  `& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo`
  El MSBuild del PATH falla con errores que despistan. El build trae cientos de warnings anteriores: hay que contar errores, no warnings.
- **Correr las pruebas con:** `& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll` (agregar `/Tests:NegTableroCostosTests` para correr solo las de este módulo).
- **C# 7.3:** nada de `switch` de expresiones, `??=`, `using` sin llaves ni tipos de referencia anulables.
- **Las tres capas se publican como `.exe`:** `CapaEntidad.exe`, `CapaNegocio.exe`, `CapaDato.exe`. Se publican **juntas** con `ReporteTareas.dll`. El 18-09 se subió solo el `.dll` y los correos dejaron de salir.
- **`CapaDato` no puede llamar a `CapaNegocio`.** La web referencia `CapaEntidad` y `CapaNegocio`, nunca `CapaDato`.
- **Codificación:** los `.cs`, `.js`, `.aspx` y `.ashx` nuevos van en **UTF-8 con BOM**. Sin BOM, la `.aspx` sale con caracteres raros en producción porque `Web.config` declara windows-1252. Los `.sql` versionados van **sin BOM y sin tildes**.
- **Cada archivo nuevo se registra en su `.csproj`.** Son proyectos de estilo antiguo: lo que no está en el `.csproj` no se compila ni se publica.
- **Correr un script SQL con** `bash docs/sql/run-sql.sh <archivo.sql>`. Apunta a `ReporTarea`, que es **producción**. **Pedir confirmación explícita al usuario antes de correrlo**, también los de solo lectura.
- **El repositorio es público:** ningún nombre de persona, cliente, salario ni monto real en código, pruebas, scripts ni documentos. Las pruebas usan «PERSONA UNO», «EMPRESA A» y similares.
- **Perfiles autorizados: `{ 14, 18 }`**, declarados en un solo lugar: `AdministrarTableroCostos.PerfilesAutorizados`.
- **Ningún archivo de este plan escribe en `HE_*` ni en `R_DetTareasAranda`.** El tablero solo lee.
- **`?v=` en el `<script>` de `tableroCostos.js`:** empieza en `?v=1` y se sube en cada cambio del `.js` después del primer despliegue. La caché del navegador ya causó un incidente con `?v=3`.
- **Cada commit termina con la línea** `Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>`.
- **Valores verificados el 2026-10-12** (no cambiarlos sin volver a verificar):
  - `HE_Parametro.Clave` usa `Factor50`, `Factor100` y `DiasMes`.
  - `HE_ColaboradorParametro.AplicaHE` es `BIT`, `JornadaHorasDia` es `INT` y `HE_Salario.Origen` toma los valores `Rol` / `Ajuste`.
  - El grupo de menú Nómina es `Id_Menu = 20081`, y la hoja `HorasExtras.aspx` cuelga de él.
  - `PerfilMenu.Estado = '0'` **muestra** la opción.
  - Las fórmulas del Excel aplican estos filtros:
    - Mes, Responsable, Empresa y Contrato: a todas las secciones.
    - Recargo y Estado HE: solo a Extra y Traslado.
    - La tabla **por mes** ignora el filtro de Mes, y el **top 15** ignora el de Responsable.
    - La tabla **por año** aplica todos los filtros.

## Review Focus

1. **Una persona con varias cuentas.** Sus actividades no deben valorarse en $0 por haber registrado con la cuenta que no está en `Empleados`. → Prueba `Clasificar_VariasCuentasMismoNombre_UsaLaQueTieneSalario` en la Task 2.
2. **Filtros que dejan todo vacío, o un rango sin actividades.** No debe haber excepción ni divisiones por cero, y el aviso debe explicar qué pasó. → Pruebas `Armar_SinActividades_NoFallaYPorcentajesEnCero` y `Armar_ElegidoQueNoExisteConLosDemas_AvisaYLoConserva` en la Task 3.
3. **Servidor con cultura es-EC / es-ES.** La descripción de filtros (auditoría y cabecera del Excel) y el nombre del archivo no deben cambiar de formato según la cultura. → `DescribirFiltro_ConCulturaEspanola_NoCambiaElFormato` en la Task 3 y `NombreDeArchivo_ConCulturaEspanola_UsaDigitosFijos` en la Task 5.
4. **Textos con espacios o mayúsculas distintas** en los datos («  EMPRESA A ») y en los filtros que llegan del navegador («empresa b»). El cruce no debe fallar por eso. → `Clasificar_TextosVaciosYEspacios` en la Task 2 y `Armar_FiltroSinDistinguirMayusculas` en la Task 3.
5. **`Det_Tiempo` inválido o nulo y `Det_Estado` nulo.** Deben contar 0 horas y clasificarse como Normal, sin excepción. → `Horas_FormatoInvalido_DaCero` y `Tipo_EstadoNulo_NoFalla` en la Task 2.

---

### Task 1: Base de datos — procedimiento, auditoría y menú

**Archivos:**
- Crear: `docs/sql/2026-10-12-tablero-costos.sql`
- Crear: `docs/sql/2026-10-12-tablero-costos-verificacion.sql`
- Referencia (no se modifica): `docs/sql/2026-09-16-horas-extras-fase5.sql`, de donde sale el bloque que clona el menú.

**Interfaces:**
- Produce:
  - `dbo.Sp_RTA_TableroCostos @Desde DATE, @Hasta DATE`, con **tres** resultados en este orden:
    1. Actividades: `Id_RegDetTareas, Det_Fch_RegDetalleIni, Det_Fch_RegDetalleFin, Det_Tiempo, Id_Responsable, Nom_Usuario, Det_Nom_Empresa, NombreCliente, Det_Id_CompAranda, Det_Num_OrdenServicio, Det_Det_Tarea, Det_Estado, IdDet_EstadoIni, Det_Horas_Extras_Tipo, Det_Horas_Extras_Estado`
    2. Tarifas: `Cod_Usuario, Cargo, JornadaHorasDia, AplicaHE, Monto`, una fila por `Cod_Usuario`.
    3. Parámetros: `Clave, Valor`, para `Factor50`, `Factor100` y `DiasMes`, ordenados por `FechaVigenciaDesde`; el último gana.
  - La tabla `dbo.TC_Auditoria`.
  - `dbo.Sp_RTA_TableroCostosAuditar @CodUsuario VARCHAR(20), @Accion VARCHAR(20), @Filtros NVARCHAR(1000)`, que devuelve una fila con `IdAuditoria INT`.
  - La hoja de menú con `Href = 'TableroCostos.aspx'` bajo el grupo 20081, visible para los perfiles 14 y 18.

- [ ] **Paso 1: Escribir el script** `docs/sql/2026-10-12-tablero-costos.sql` (UTF-8 **sin BOM**, sin tildes)

```sql
/* ============================================================================
   Tablero de Costos (horas y valor de las tareas Aranda)
   ReporTarea  |  2026-10-12
   Spec: docs/superpowers/specs/2026-10-12-tablero-costos-design.md

   PENDIENTE DE EJECUTAR.

   Crea o actualiza:
     1. dbo.Sp_RTA_TableroCostos        (solo lectura: actividades, tarifas, parametros)
     2. dbo.TC_Auditoria                (registro de cada descarga del Excel)
     3. dbo.Sp_RTA_TableroCostosAuditar
     4. La hoja de menu "Tablero de Costos" bajo el grupo Nomina (20081),
        visible SOLO para los perfiles 14 (Talento Humano) y 18 (Gerencia).

   Repetible: correrlo dos veces no duplica la tabla, el menu ni los permisos.
   No escribe en HE_* ni en R_DetTareasAranda.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------------- 1. Sp_RTA_TableroCostos ------ */
IF OBJECT_ID('dbo.Sp_RTA_TableroCostos', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_TableroCostos AS RETURN 0;');
GO

/* Misma consulta que C:\ReporteExcel\generar_dashboard.py (SQL y SQL_TARIFAS).
   El procedimiento no clasifica ni valora: eso vive en CapaNegocio
   (NegTableroCostos), que es lo que se prueba contra el Excel. */
ALTER PROCEDURE dbo.Sp_RTA_TableroCostos
    @Desde DATE,
    @Hasta DATE
AS
BEGIN
    SET NOCOUNT ON;

    /* @Hasta es inclusivo: se compara contra el dia siguiente. */
    DECLARE @HastaExclusivo DATETIME = DATEADD(DAY, 1, CONVERT(DATETIME, @Hasta));

    /* 1. Actividades. El nombre sale de la cuenta mas reciente de ese
          Cod_Usuario: una persona puede tener varias cuentas. */
    SELECT d.Id_RegDetTareas,
           d.Det_Fch_RegDetalleIni,
           d.Det_Fch_RegDetalleFin,
           d.Det_Tiempo,
           LTRIM(RTRIM(CONVERT(VARCHAR(50), d.Id_Responsable))) AS Id_Responsable,
           u.Nom_Usuario,
           d.Det_Nom_Empresa,
           t.NombreCliente,
           d.Det_Id_CompAranda,
           d.Det_Num_OrdenServicio,
           d.Det_Det_Tarea,
           d.Det_Estado,
           d.IdDet_EstadoIni,
           d.Det_Horas_Extras_Tipo,
           d.Det_Horas_Extras_Estado
      FROM dbo.R_DetTareasAranda AS d
      LEFT JOIN dbo.R_TareasAranda AS t
             ON t.Id_RegTareas = d.Id_RegTareas
      OUTER APPLY (SELECT TOP 1 x.Nom_Usuario
                     FROM dbo.R_Usuarios AS x
                    WHERE x.Cod_Usuario = d.Id_Responsable
                    ORDER BY x.Id_Usuario DESC) AS u
     WHERE d.Det_Fch_RegDetalleIni >= @Desde
       AND d.Det_Fch_RegDetalleIni <  @HastaExclusivo
     ORDER BY d.Det_Fch_RegDetalleIni, d.Id_RegDetTareas;

    /* 2. Tarifas: el salario vigente mas reciente. A igual fecha, 'Ajuste'
          gana a 'Rol': cinco personas tienen los dos con la misma vigencia, y
          sin el desempate cobrarian sobre el sueldo anterior al ajuste, en
          silencio. Una fila por Cod_Usuario, prefiriendo la que tiene salario. */
    ;WITH Tarifas AS
    (
        SELECT LTRIM(RTRIM(CONVERT(VARCHAR(50), e.Cod_Usuario))) AS Cod_Usuario,
               cp.Cargo,
               cp.JornadaHorasDia,
               cp.AplicaHE,
               s.Monto,
               ROW_NUMBER() OVER
               (
                   PARTITION BY LTRIM(RTRIM(CONVERT(VARCHAR(50), e.Cod_Usuario)))
                   ORDER BY CASE WHEN s.Monto IS NULL THEN 1 ELSE 0 END,
                            e.IdEmpleado DESC,
                            cp.IdColaboradorParametro DESC
               ) AS Orden
          FROM dbo.Empleados AS e
          LEFT JOIN dbo.HE_ColaboradorParametro AS cp
                 ON cp.IdEmpleado = e.IdEmpleado
                AND cp.Estado = '1'
          OUTER APPLY (SELECT TOP 1 x.Monto
                         FROM dbo.HE_Salario AS x
                        WHERE x.IdEmpleado = e.IdEmpleado
                          AND x.Estado = '1'
                        ORDER BY x.FechaVigenciaDesde DESC,
                                 CASE x.Origen WHEN 'Ajuste' THEN 0 ELSE 1 END,
                                 x.IdSalario DESC) AS s
         WHERE e.Cod_Usuario IS NOT NULL
    )
    SELECT Cod_Usuario, Cargo, JornadaHorasDia, AplicaHE, Monto
      FROM Tarifas
     WHERE Orden = 1;

    /* 3. Parametros vigentes hoy. Si hay dos vigencias de la misma clave, el
          orden deja la mas reciente al final y CapaDato se queda con ella. */
    SELECT p.Clave, p.Valor
      FROM dbo.HE_Parametro AS p
     WHERE p.Clave IN ('Factor50', 'Factor100', 'DiasMes')
       AND p.FechaVigenciaDesde <= CONVERT(DATE, GETDATE())
       AND (p.FechaVigenciaHasta IS NULL OR p.FechaVigenciaHasta >= CONVERT(DATE, GETDATE()))
     ORDER BY p.Clave, p.FechaVigenciaDesde, p.IdParametro;
END
GO

/* ------------------------------------------------- 2. TC_Auditoria -------- */
IF OBJECT_ID('dbo.TC_Auditoria', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TC_Auditoria
    (
        IdAuditoria INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TC_Auditoria PRIMARY KEY,
        CodUsuario  VARCHAR(20)    NOT NULL,
        Accion      VARCHAR(20)    NOT NULL,
        Filtros     NVARCHAR(1000) NULL,
        Fecha       DATETIME       NOT NULL
            CONSTRAINT DF_TC_Auditoria_Fecha DEFAULT (GETDATE())
    );
    PRINT 'TC_Auditoria creada.';
END
GO

/* ----------------------------------- 3. Sp_RTA_TableroCostosAuditar ------- */
IF OBJECT_ID('dbo.Sp_RTA_TableroCostosAuditar', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_TableroCostosAuditar AS RETURN 0;');
GO

/* El handler de descarga llama a este procedimiento ANTES de armar el archivo:
   si el registro falla, no se entrega nada. */
ALTER PROCEDURE dbo.Sp_RTA_TableroCostosAuditar
    @CodUsuario VARCHAR(20),
    @Accion     VARCHAR(20),
    @Filtros    NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.TC_Auditoria (CodUsuario, Accion, Filtros)
    VALUES (@CodUsuario, @Accion, @Filtros);

    SELECT CONVERT(INT, SCOPE_IDENTITY()) AS IdAuditoria;
END
GO

/* ------------------------------------------------------- 4. menu ---------- */
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

/* Se CLONA la fila de HorasExtras.aspx, la hermana que cuelga del grupo
   Nomina: MenuDos tiene 18 columnas, varias NOT NULL con default, y enumerarlas
   a mano es la forma segura de olvidarse de una. Mismo bloque que
   2026-09-16-horas-extras-fase5.sql. */
DECLARE @HrefModelo VARCHAR(512) = 'HorasExtras.aspx';
DECLARE @HrefHoja   VARCHAR(512) = 'TableroCostos.aspx';
DECLARE @TituloHoja VARCHAR(128) = 'Tablero de Costos';
DECLARE @DescHoja   VARCHAR(512) = 'Horas y costos de las tareas Aranda';

DECLARE @IdModelo INT;
DECLARE @IdPadre  INT;
DECLARE @IdHoja   INT;
DECLARE @Orden    INT;

SELECT @IdModelo = Id_Menu, @IdPadre = Id_MenuPadre
  FROM dbo.MenuDos
 WHERE Href = @HrefModelo;

IF @IdModelo IS NULL
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('No existe la fila de menu de HorasExtras.aspx: no hay de donde clonar.', 16, 1);
    RETURN;
END

/* El spec cuelga la pantalla del grupo Nomina (20081, medido el 2026-10-12).
   Si el modelo ya cuelga de otro sitio, el menu cambio y hay que mirarlo antes
   de seguir. */
IF @IdPadre <> 20081
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('HorasExtras.aspx ya no cuelga del grupo 20081. Revisar el menu antes de desplegar.', 16, 1);
    RETURN;
END

DECLARE @cols NVARCHAR(MAX) = STUFF
((
    SELECT ',' + QUOTENAME(c.name)
      FROM sys.columns AS c
     WHERE c.object_id = OBJECT_ID('dbo.MenuDos')
       AND c.is_identity = 0
       AND c.is_computed = 0
       AND c.name <> 'Id_Menu'
     ORDER BY c.column_id
    FOR XML PATH(''), TYPE
).value('.', 'NVARCHAR(MAX)'), 1, 1, '');

DECLARE @EsIdentidad BIT =
    CASE WHEN EXISTS (SELECT 1 FROM sys.columns
                       WHERE object_id = OBJECT_ID('dbo.MenuDos')
                         AND name = 'Id_Menu'
                         AND is_identity = 1)
         THEN 1 ELSE 0 END;

SELECT @IdHoja = Id_Menu
  FROM dbo.MenuDos
 WHERE Href = @HrefHoja;

IF @IdHoja IS NULL
BEGIN
    DECLARE @sql NVARCHAR(MAX);

    IF @EsIdentidad = 1
    BEGIN
        SET @sql =
            N'INSERT INTO dbo.MenuDos (' + @cols + N') ' +
            N'SELECT ' + @cols + N' FROM dbo.MenuDos WHERE Id_Menu = @Modelo; ' +
            N'SET @IdOut = CONVERT(INT, SCOPE_IDENTITY());';

        EXEC sys.sp_executesql @sql,
             N'@Modelo INT, @IdOut INT OUTPUT',
             @Modelo = @IdModelo,
             @IdOut  = @IdHoja OUTPUT;
    END
    ELSE
    BEGIN
        SELECT @IdHoja = ISNULL(MAX(Id_Menu), 0) + 1
          FROM dbo.MenuDos WITH (TABLOCKX, HOLDLOCK);

        SET @sql =
            N'INSERT INTO dbo.MenuDos (' + QUOTENAME('Id_Menu') + N',' + @cols + N') ' +
            N'SELECT @IdNuevo, ' + @cols + N' FROM dbo.MenuDos WHERE Id_Menu = @Modelo;';

        EXEC sys.sp_executesql @sql,
             N'@Modelo INT, @IdNuevo INT',
             @Modelo  = @IdModelo,
             @IdNuevo = @IdHoja;
    END

    SELECT @Orden = ISNULL(MAX(Orden_Opcion), 0) + 1
      FROM dbo.MenuDos
     WHERE Id_MenuPadre = @IdPadre
       AND Id_Menu <> @IdHoja;

    UPDATE dbo.MenuDos
       SET Href              = @HrefHoja,
           Titulo            = @TituloHoja,
           Descripcion       = @DescHoja,
           Id_MenuPadre      = @IdPadre,
           Es_Opcion_de_Menu = 0,
           Orden_Opcion      = @Orden
     WHERE Id_Menu = @IdHoja;

    PRINT 'MenuDos: hoja TableroCostos.aspx creada con Id_Menu = '
        + CONVERT(VARCHAR(20), @IdHoja) + ', bajo el grupo '
        + CONVERT(VARCHAR(20), @IdPadre) + '.';
END
ELSE
BEGIN
    UPDATE dbo.MenuDos
       SET Id_MenuPadre = @IdPadre
     WHERE Id_Menu = @IdHoja
       AND Id_MenuPadre <> @IdPadre;

    PRINT 'MenuDos: la hoja TableroCostos.aspx ya existia (Id_Menu = '
        + CONVERT(VARCHAR(20), @IdHoja) + '). Sin duplicar.';
END

/* Visibilidad SOLO para 14 (Talento Humano) y 18 (Gerencia). Estado = '0'
   MUESTRA: Sp_RTA_ConsultarMenuPerfilUsuario filtra WHERE P.Estado = 0. Los
   demas perfiles no necesitan fila: sin fila con Estado = 0 no ven la opcion.
   La pantalla muestra montos calculados con salarios: la lista es corta a
   proposito y coincide con AdministrarTableroCostos.PerfilesAutorizados. */
DECLARE @Perfiles TABLE (IdPerfil INT PRIMARY KEY);
INSERT INTO @Perfiles (IdPerfil) VALUES (14), (18);

INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
SELECT @IdHoja, P.IdPerfil, CONVERT(DATE, GETDATE()), '0'
  FROM @Perfiles AS P
 WHERE NOT EXISTS
 (
     SELECT 1 FROM dbo.PerfilMenu AS PM
      WHERE PM.id_Menu = @IdHoja
        AND PM.IdPerfil = P.IdPerfil
 );

UPDATE PM
   SET PM.Estado = '0'
  FROM dbo.PerfilMenu AS PM
  JOIN @Perfiles AS P ON P.IdPerfil = PM.IdPerfil
 WHERE PM.id_Menu = @IdHoja
   AND PM.Estado <> '0';

PRINT 'PerfilMenu: visibilidad asegurada para los perfiles 14 y 18.';

COMMIT TRANSACTION;
GO

SET XACT_ABORT OFF;
GO

/* ------------------------------------------------- 5. aserciones ---------- */
/* Un solo batch, sin GO en medio: un GO entre el DECLARE y su ultimo uso hace
   fallar el script con "must declare the scalar variable". RAISERROR 16 no
   aborta el lote, asi que se cuentan los fallos y el veredicto va al final. */
DECLARE @Fallos INT = 0;
DECLARE @IdHojaV INT;
DECLARE @IdPadreV INT;

IF OBJECT_ID('dbo.Sp_RTA_TableroCostos', 'P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_TableroCostos no quedo creado.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.Sp_RTA_TableroCostosAuditar', 'P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_TableroCostosAuditar no quedo creado.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.TC_Auditoria', 'U') IS NULL
BEGIN
    RAISERROR('FALLO: TC_Auditoria no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF (SELECT COUNT(*) FROM dbo.MenuDos WHERE Href = 'TableroCostos.aspx') <> 1
BEGIN
    RAISERROR('FALLO: debe haber exactamente una hoja de menu TableroCostos.aspx.', 16, 1);
    SET @Fallos += 1;
END

SELECT @IdHojaV = Id_Menu, @IdPadreV = Id_MenuPadre
  FROM dbo.MenuDos
 WHERE Href = 'TableroCostos.aspx';

IF @IdPadreV IS NULL OR @IdPadreV <> 20081
BEGIN
    RAISERROR('FALLO: la hoja TableroCostos.aspx no cuelga del grupo 20081.', 16, 1);
    SET @Fallos += 1;
END

IF (SELECT COUNT(DISTINCT IdPerfil) FROM dbo.PerfilMenu
     WHERE id_Menu = @IdHojaV AND Estado = '0' AND IdPerfil IN (14, 18)) <> 2
BEGIN
    RAISERROR('FALLO: los perfiles 14 y 18 no tienen la hoja visible.', 16, 1);
    SET @Fallos += 1;
END

IF EXISTS (SELECT 1 FROM dbo.PerfilMenu
            WHERE id_Menu = @IdHojaV AND Estado = '0' AND IdPerfil NOT IN (14, 18))
BEGIN
    RAISERROR('FALLO: un perfil distinto de 14 y 18 ve el Tablero de Costos.', 16, 1);
    SET @Fallos += 1;
END

IF @Fallos = 0
    PRINT 'VEREDICTO: OK - Tablero de Costos listo en la base.';
ELSE
    RAISERROR('VEREDICTO: FALLO - %d comprobaciones fallaron.', 16, 1, @Fallos);
GO
```

- [ ] **Paso 2: Escribir la verificación** `docs/sql/2026-10-12-tablero-costos-verificacion.sql` (UTF-8 **sin BOM**). Es de solo lectura; la prueba de auditoría va dentro de una transacción que se revierte.

```sql
/* ============================================================================
   Verificacion del Tablero de Costos. Solo lectura: lo unico que escribe va
   dentro de una transaccion que se revierte.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
GO

/* 1. El procedimiento devuelve los tres resultados (mirar que salgan tres
      grillas: actividades, tarifas, parametros). Rango corto a proposito. */
EXEC dbo.Sp_RTA_TableroCostos @Desde = '2026-09-01', @Hasta = '2026-09-30';
GO

/* 2. Volumen del rango del Excel de referencia. El Excel del 04-10-2026 tenia
      20.657 actividades; hoy puede haber algunas mas, nunca menos. */
SELECT COUNT(*) AS Actividades_2025_2026
  FROM dbo.R_DetTareasAranda
 WHERE Det_Fch_RegDetalleIni >= '2025-01-01'
   AND Det_Fch_RegDetalleIni <  '2027-01-01';
GO

/* 3. Indices cuya primera columna es Det_Fch_RegDetalleIni. Si no sale
      ninguno, avisar al usuario: el tablero funciona igual, pero cada consulta
      recorre la tabla entera. NO crear el indice desde este plan. */
SELECT i.name AS Indice, i.type_desc
  FROM sys.indexes AS i
  JOIN sys.index_columns AS ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
  JOIN sys.columns AS c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
 WHERE i.object_id = OBJECT_ID('dbo.R_DetTareasAranda')
   AND ic.key_ordinal = 1
   AND c.name = 'Det_Fch_RegDetalleIni';
GO

/* 4. Parametros que leera el tablero. Deben salir Factor50, Factor100 y DiasMes. */
SELECT Clave, Valor, FechaVigenciaDesde, FechaVigenciaHasta
  FROM dbo.HE_Parametro
 WHERE Clave IN ('Factor50', 'Factor100', 'DiasMes')
 ORDER BY Clave, FechaVigenciaDesde;
GO

/* 5. La auditoria devuelve un id, y no deja la fila. */
BEGIN TRANSACTION;
EXEC dbo.Sp_RTA_TableroCostosAuditar
     @CodUsuario = 'VERIFICA',
     @Accion     = 'Descargar',
     @Filtros    = N'verificacion del despliegue';
ROLLBACK TRANSACTION;
GO

/* 6. Quien ve la hoja de menu: deben salir solo 14 y 18. */
SELECT PM.IdPerfil, PM.Estado
  FROM dbo.PerfilMenu AS PM
  JOIN dbo.MenuDos AS M ON M.Id_Menu = PM.id_Menu
 WHERE M.Href = 'TableroCostos.aspx';
GO
```

- [ ] **Paso 3: Pedir confirmación al usuario** antes de correr nada contra `ReporTarea`, que es producción. Mostrarle qué crea: un procedimiento de lectura, una tabla de auditoría, el procedimiento que escribe en ella y una opción de menú visible solo para los perfiles 14 y 18. **No toca `HE_*` ni `R_DetTareasAranda`.**

- [ ] **Paso 4: Correr el script**

Run: `bash docs/sql/run-sql.sh docs/sql/2026-10-12-tablero-costos.sql`
Expected: la última línea es `VEREDICTO: OK - Tablero de Costos listo en la base.`

- [ ] **Paso 5: Correr la verificación** (también con permiso del usuario)

Run: `bash docs/sql/run-sql.sh docs/sql/2026-10-12-tablero-costos-verificacion.sql`
Expected:
- tres grillas para septiembre de 2026;
- un conteo de 2025–2026 de **al menos 20.657**;
- `Factor50`, `Factor100` y `DiasMes` presentes;
- un `IdAuditoria` numérico;
- los perfiles 14 y 18 con `Estado = 0`.

Anotar en el commit si el paso 3 de la verificación no devolvió ningún índice.

- [ ] **Paso 6: Commit**

```bash
git add docs/sql/2026-10-12-tablero-costos.sql docs/sql/2026-10-12-tablero-costos-verificacion.sql
git commit -m "feat(tablero-costos): procedimiento, auditoria de descargas y menu"
```

---

### Task 2: Entidades, clasificación y valoración

**Archivos:**
- Crear: `CapaEntidad/EntTableroCostos.cs`
- Crear: `CapaNegocio/NegTableroCostos.cs`
- Crear: `CapaPruebas/NegTableroCostosTests.cs`
- Modificar: `CapaEntidad/CapaEntidad.csproj`, `CapaNegocio/CapaNegocio.csproj`, `CapaPruebas/CapaPruebas.csproj`

**Interfaces:**
- Produce (las tareas siguientes usan estos nombres tal cual):
  - Entidades: `EntTcActividad`, `EntTcTarifa`, `EntTcDatos`, `EntTcFila`, `EntTcClasificado`, `EntTcFiltro`, `EntTcCelda`, `EntTcIndicadores`, `EntTcResumenFila`, `EntTcEstadoExtra`, `EntTcEstadoTraslado`, `EntTcPeriodo`, `EntTcTop`, `EntTableroCostos`.
  - `NegTableroCostos.Tipo(EntTcActividad) : string`
  - `NegTableroCostos.Recargo(EntTcActividad, string tipo) : string`
  - `NegTableroCostos.EstadoHe(EntTcActividad, string tipo, string recargo) : string`
  - `NegTableroCostos.Horas(string tiempo) : decimal`
  - `NegTableroCostos.Categoria(EntTcFila) : string`
  - `NegTableroCostos.Clasificar(EntTcDatos) : EntTcClasificado`
  - Constantes: `Normal`, `Extra`, `Traslado`, `SinRecargo`, `Recargo50`, `Recargo100`, `Todos`, `CatNormal`, `CatExtra50`, `CatExtra100`, `CatTrasladoSin`, `CatTraslado50`, `CatTraslado100`, `Recargos`, `EstadosHE`.

- [ ] **Paso 1: Escribir las entidades** `CapaEntidad/EntTableroCostos.cs` (UTF-8 con BOM)

```csharp
using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    /*  Entidades del Tablero de Costos (TableroCostos.aspx).
        Spec: docs/superpowers/specs/2026-10-12-tablero-costos-design.md

        [Serializable] en lo que el handler guarda en Session: hoy la sesion es
        InProc y no lo necesita, pero si alguien la pasa a StateServer las
        clases sin el atributo revientan al guardar, lejos de este codigo. */

    /// <summary>Una fila de R_DetTareasAranda tal como la trae Sp_RTA_TableroCostos.</summary>
    [Serializable]
    public class EntTcActividad
    {
        public long IdDetalle { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string Tiempo { get; set; } = "";
        public string CodResponsable { get; set; } = "";
        public string NomResponsable { get; set; } = "";
        public string Empresa { get; set; } = "";
        public string Contrato { get; set; } = "";
        public string Ticket { get; set; } = "";
        public string OrdenServicio { get; set; } = "";
        public string Tarea { get; set; } = "";
        public string Estado { get; set; } = "";
        public int? IdEstadoIni { get; set; }
        /// <summary>Det_Horas_Extras_Tipo: 1 = 50%, 2 = 100%; cualquier otro valor, sin recargo.</summary>
        public int TipoHE { get; set; }
        /// <summary>Det_Horas_Extras_Estado: 1 Solicitado, 2 Aprobado, 3 Rechazado; otro, sin solicitud.</summary>
        public int EstadoHE { get; set; }
    }

    /// <summary>Salario vigente y parametros de un Cod_Usuario (Empleados + HE_*).</summary>
    [Serializable]
    public class EntTcTarifa
    {
        public string CodUsuario { get; set; } = "";
        public string Cargo { get; set; } = "";
        public int? JornadaHorasDia { get; set; }
        public bool? AplicaHE { get; set; }
        public decimal? Salario { get; set; }
    }

    /// <summary>Los tres resultados de Sp_RTA_TableroCostos.</summary>
    public class EntTcDatos
    {
        public List<EntTcActividad> Actividades { get; set; } = new List<EntTcActividad>();
        public List<EntTcTarifa> Tarifas { get; set; } = new List<EntTcTarifa>();
        public Dictionary<string, decimal> Parametros { get; set; } =
            new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Una actividad ya clasificada y valorada. Es la fila del detalle y del Excel.</summary>
    [Serializable]
    public class EntTcFila
    {
        public long IdDetalle { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        /// <summary>"Ene-2025", "Feb-2025"... igual que la columna Periodo del Excel.</summary>
        public string Periodo { get; set; } = "";
        public string Responsable { get; set; } = "";
        public string Empresa { get; set; } = "";
        public string Contrato { get; set; } = "";
        public string Ticket { get; set; } = "";
        public string OrdenServicio { get; set; } = "";
        public string Tarea { get; set; } = "";
        public string Estado { get; set; } = "";
        /// <summary>Normal, Extra o Traslado.</summary>
        public string Tipo { get; set; } = "";
        /// <summary>"50%", "100%", "Sin recargo" (traslados) o "" (normales).</summary>
        public string Recargo { get; set; } = "";
        /// <summary>Aprobado, Solicitado, Rechazado, Sin solicitud, No aplica o "".</summary>
        public string EstadoHE { get; set; } = "";
        public string Tiempo { get; set; } = "";
        public decimal Horas { get; set; }
        public decimal Factor { get; set; }
        public decimal ValorHora { get; set; }
        public decimal Valor { get; set; }
        /// <summary>El responsable no tiene salario: sus horas se valoran en $0.</summary>
        public bool SinSalario { get; set; }
    }

    /// <summary>Lo que el handler guarda en Session por rango de fechas.</summary>
    [Serializable]
    public class EntTcClasificado
    {
        public List<EntTcFila> Filas { get; set; } = new List<EntTcFila>();
        /// <summary>Faltaba algun parametro vigente y se usaron 1,5 / 2 / 30.</summary>
        public bool ParametrosPorDefecto { get; set; }
    }

    /// <summary>Filtros de la pantalla. "" o "Todos" = sin filtro.</summary>
    public class EntTcFiltro
    {
        public string Mes { get; set; } = "";
        public string Responsable { get; set; } = "";
        public string Empresa { get; set; } = "";
        public string Contrato { get; set; } = "";
        public string Recargo { get; set; } = "";
        public string EstadoHE { get; set; } = "";
    }

    public class EntTcCelda
    {
        public decimal Horas { get; set; }
        public decimal Valor { get; set; }
        public int Actividades { get; set; }
    }

    public class EntTcIndicadores
    {
        public EntTcCelda Normal { get; set; } = new EntTcCelda();
        public EntTcCelda Extra { get; set; } = new EntTcCelda();
        public EntTcCelda Traslado { get; set; } = new EntTcCelda();
        public EntTcCelda Total { get; set; } = new EntTcCelda();
    }

    /// <summary>Una fila de "Resumen por tipo y recargo" (las 11 del Excel).</summary>
    public class EntTcResumenFila
    {
        public string Concepto { get; set; } = "";
        public bool EsSubtotal { get; set; }
        public decimal Horas { get; set; }
        public decimal PctHoras { get; set; }
        public decimal Valor { get; set; }
        public decimal PctValor { get; set; }
        public int Actividades { get; set; }
        public decimal ValorPorHora { get; set; }
    }

    /// <summary>Una fila de "Horas extra por estado y recargo".</summary>
    public class EntTcEstadoExtra
    {
        public string Estado { get; set; } = "";
        public decimal Horas50 { get; set; }
        public decimal Horas100 { get; set; }
        public decimal Valor50 { get; set; }
        public decimal Valor100 { get; set; }
        public decimal Total { get; set; }
    }

    /// <summary>Una fila de "Traslado por estado y recargo".</summary>
    public class EntTcEstadoTraslado
    {
        public string Estado { get; set; } = "";
        public decimal HorasSinRecargo { get; set; }
        public decimal Horas50 { get; set; }
        public decimal Horas100 { get; set; }
        public decimal TotalHoras { get; set; }
        public decimal TotalValor { get; set; }
    }

    /// <summary>Una fila de las tablas por año y por mes. H* = horas, V* = valor.</summary>
    public class EntTcPeriodo
    {
        public string Periodo { get; set; } = "";
        public decimal HNormal { get; set; }
        public decimal HExtra50 { get; set; }
        public decimal HExtra100 { get; set; }
        public decimal HTrasladoSin { get; set; }
        public decimal HTraslado50 { get; set; }
        public decimal HTraslado100 { get; set; }
        public decimal HTotal { get; set; }
        public decimal VNormal { get; set; }
        public decimal VExtra50 { get; set; }
        public decimal VExtra100 { get; set; }
        public decimal VTrasladoSin { get; set; }
        public decimal VTraslado50 { get; set; }
        public decimal VTraslado100 { get; set; }
        public decimal VTotal { get; set; }
    }

    /// <summary>Una fila de los top 15 por tipo de hora.</summary>
    public class EntTcTop
    {
        public string Responsable { get; set; } = "";
        public decimal Horas { get; set; }
        public decimal HorasSinRecargo { get; set; }
        public decimal Horas50 { get; set; }
        public decimal Horas100 { get; set; }
        public decimal Valor { get; set; }
    }

    /// <summary>Todo lo que dibuja TableroCostos.aspx.</summary>
    public class EntTableroCostos
    {
        public EntTcIndicadores Indicadores { get; set; } = new EntTcIndicadores();
        public List<EntTcResumenFila> Resumen { get; set; } = new List<EntTcResumenFila>();
        public List<EntTcEstadoExtra> ExtrasPorEstado { get; set; } = new List<EntTcEstadoExtra>();
        public List<EntTcEstadoTraslado> TrasladosPorEstado { get; set; } = new List<EntTcEstadoTraslado>();
        public List<EntTcPeriodo> PorAnio { get; set; } = new List<EntTcPeriodo>();
        public List<EntTcPeriodo> PorMes { get; set; } = new List<EntTcPeriodo>();
        public List<EntTcTop> TopNormal { get; set; } = new List<EntTcTop>();
        public List<EntTcTop> TopExtra { get; set; } = new List<EntTcTop>();
        public List<EntTcTop> TopTraslado { get; set; } = new List<EntTcTop>();
        public List<EntTcFila> Detalle { get; set; } = new List<EntTcFila>();

        public List<string> Meses { get; set; } = new List<string>();
        public List<string> Responsables { get; set; } = new List<string>();
        public List<string> Empresas { get; set; } = new List<string>();
        public List<string> Contratos { get; set; } = new List<string>();

        public List<string> SinSalario { get; set; } = new List<string>();
        /// <summary>"" si no hay nada que avisar.</summary>
        public string Aviso { get; set; } = "";
        public bool ParametrosPorDefecto { get; set; }
    }
}
```

Agregar en `CapaEntidad/CapaEntidad.csproj`, junto a `<Compile Include="EntDashboardAprobacion.cs" />`:

```xml
    <Compile Include="EntTableroCostos.cs" />
```

- [ ] **Paso 2: Escribir las pruebas que fallan** `CapaPruebas/NegTableroCostosTests.cs` (UTF-8 con BOM)

```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CapaPruebas
{
    /// <summary>
    /// Reglas de C:\ReporteExcel\generar_dashboard.py, que es la referencia del
    /// tablero. Los datos son inventados a proposito: el repositorio es publico.
    /// </summary>
    [TestClass]
    public class NegTableroCostosTests
    {
        private static long _id;

        internal static EntTcActividad Act(string estado = "Asignado", int? estadoIni = null, int tipoHe = 0,
            int estadoHe = 0, string tiempo = "01:00:00", string cod = "C1", string nombre = "PERSONA UNO",
            string empresa = "EMPRESA A", string contrato = "CONTRATO A", int anio = 2026, int mes = 3, int dia = 5)
        {
            _id++;
            return new EntTcActividad
            {
                IdDetalle = _id,
                FechaInicio = new DateTime(anio, mes, dia, 9, 0, 0),
                FechaFin = new DateTime(anio, mes, dia, 10, 0, 0),
                Tiempo = tiempo,
                CodResponsable = cod,
                NomResponsable = nombre,
                Empresa = empresa,
                Contrato = contrato,
                Estado = estado,
                IdEstadoIni = estadoIni,
                TipoHE = tipoHe,
                EstadoHE = estadoHe,
                Ticket = "T-1",
                Tarea = "TAREA DE PRUEBA"
            };
        }

        /// <summary>Parametros como los cargados en HE_Parametro (1,5 / 2 / 30).</summary>
        internal static EntTcDatos Datos()
        {
            EntTcDatos d = new EntTcDatos();
            d.Parametros["Factor50"] = 1.5m;
            d.Parametros["Factor100"] = 2m;
            d.Parametros["DiasMes"] = 30m;
            return d;
        }

        /// <summary>1200 / (8 x 30) = 5 $/h.</summary>
        internal static EntTcTarifa Tarifa(string cod, decimal? salario = 1200m, int? jornada = 8, bool? aplica = true)
        {
            return new EntTcTarifa { CodUsuario = cod, Salario = salario, JornadaHorasDia = jornada, AplicaHE = aplica };
        }

        private static string Tipo(EntTcActividad a) { return NegTableroCostos.Tipo(a); }
        private static string Rec(EntTcActividad a) { return NegTableroCostos.Recargo(a, Tipo(a)); }
        private static string Est(EntTcActividad a) { return NegTableroCostos.EstadoHe(a, Tipo(a), Rec(a)); }

        private static EntTcFila Una(EntTcDatos d)
        {
            return NegTableroCostos.Clasificar(d).Filas.Single();
        }

        // ------------------------------------------------------- tipo

        [TestMethod]
        public void Tipo_EstadoMovilizacionConTilde_EsTraslado()
        {
            Assert.AreEqual(NegTableroCostos.Traslado, Tipo(Act(estado: "Movilización")));
        }

        [TestMethod]
        public void Tipo_EstadoMovilizacionSinTildeConEspaciosYMayusculas_EsTraslado()
        {
            Assert.AreEqual(NegTableroCostos.Traslado, Tipo(Act(estado: "  MOVILIZACION A SITIO ")));
        }

        [TestMethod]
        public void Tipo_EstadoIniDeTraslado_EsTraslado()
        {
            foreach (int id in new[] { 14, 28, 62 })
            {
                Assert.AreEqual(NegTableroCostos.Traslado, Tipo(Act(estadoIni: id)), "IdDet_EstadoIni " + id);
            }
        }

        [TestMethod]
        public void Tipo_OtroEstadoIni_NoEsTraslado()
        {
            Assert.AreEqual(NegTableroCostos.Normal, Tipo(Act(estadoIni: 15)));
        }

        [TestMethod]
        public void Tipo_TrasladoTienePrioridadSobreExtra()
        {
            Assert.AreEqual(NegTableroCostos.Traslado, Tipo(Act(estado: "Movilización", tipoHe: 1)));
        }

        [TestMethod]
        public void Tipo_TipoHe1o2_EsExtra()
        {
            Assert.AreEqual(NegTableroCostos.Extra, Tipo(Act(tipoHe: 1)));
            Assert.AreEqual(NegTableroCostos.Extra, Tipo(Act(tipoHe: 2)));
        }

        [TestMethod]
        public void Tipo_TipoHe0_EsNormalAunqueTengaEstadoHe()
        {
            Assert.AreEqual(NegTableroCostos.Normal, Tipo(Act(tipoHe: 0, estadoHe: 2)));
        }

        [TestMethod]
        public void Tipo_EstadoNulo_NoFalla()
        {
            Assert.AreEqual(NegTableroCostos.Normal, Tipo(Act(estado: null)));
        }

        // ------------------------------------------------ recargo y estado

        [TestMethod]
        public void Recargo_PorTipoYCodigo()
        {
            Assert.AreEqual("50%", Rec(Act(tipoHe: 1)));
            Assert.AreEqual("100%", Rec(Act(tipoHe: 2)));
            Assert.AreEqual("Sin recargo", Rec(Act(estado: "Movilización")));
            Assert.AreEqual("100%", Rec(Act(estado: "Movilización", tipoHe: 2)));
            Assert.AreEqual("", Rec(Act()));
        }

        [TestMethod]
        public void EstadoHe_ConRecargo_TraduceElCatalogo()
        {
            Assert.AreEqual("Solicitado", Est(Act(tipoHe: 1, estadoHe: 1)));
            Assert.AreEqual("Aprobado", Est(Act(tipoHe: 1, estadoHe: 2)));
            Assert.AreEqual("Rechazado", Est(Act(tipoHe: 2, estadoHe: 3)));
            Assert.AreEqual("Sin solicitud", Est(Act(tipoHe: 2, estadoHe: 0)));
            Assert.AreEqual("Sin solicitud", Est(Act(tipoHe: 2, estadoHe: 9)));
            Assert.AreEqual("Aprobado", Est(Act(estado: "Movilización", tipoHe: 1, estadoHe: 2)));
        }

        [TestMethod]
        public void EstadoHe_TrasladoSinRecargo_EsNoAplica()
        {
            Assert.AreEqual("No aplica", Est(Act(estado: "Movilización", estadoHe: 2)));
        }

        [TestMethod]
        public void EstadoHe_Normal_EsVacio()
        {
            Assert.AreEqual("", Est(Act(estadoHe: 2)));
        }

        // ------------------------------------------------------- horas

        [TestMethod]
        public void Horas_ConvierteHhMmSs()
        {
            Assert.AreEqual(1.5m, NegTableroCostos.Horas("01:30:00"));
            Assert.AreEqual(0.3333m, NegTableroCostos.Horas("00:20:00"));
            Assert.AreEqual(25m, NegTableroCostos.Horas("25:00:00"));
            Assert.AreEqual(0.0083m, NegTableroCostos.Horas("00:00:30"));
        }

        [TestMethod]
        public void Horas_FormatoInvalido_DaCero()
        {
            foreach (string t in new[] { null, "", "abc", "1:30", "01:xx:00" })
            {
                Assert.AreEqual(0m, NegTableroCostos.Horas(t), "Tiempo '" + (t ?? "null") + "'");
            }
        }

        // ------------------------------------------------- valoracion

        [TestMethod]
        public void Clasificar_ValorHoraYFactores()
        {
            EntTcDatos d = Datos();
            d.Tarifas.Add(Tarifa("C1"));
            d.Actividades.Add(Act(tiempo: "02:00:00"));
            d.Actividades.Add(Act(tipoHe: 1, tiempo: "02:00:00"));
            d.Actividades.Add(Act(tipoHe: 2, tiempo: "01:00:00"));

            List<EntTcFila> f = NegTableroCostos.Clasificar(d).Filas;

            Assert.AreEqual(5m, f[0].ValorHora);
            Assert.AreEqual(1m, f[0].Factor);
            Assert.AreEqual(10m, f[0].Valor);
            Assert.AreEqual(1.5m, f[1].Factor);
            Assert.AreEqual(15m, f[1].Valor);
            Assert.AreEqual(2m, f[2].Factor);
            Assert.AreEqual(10m, f[2].Valor);
            Assert.IsFalse(f[0].SinSalario);
        }

        [TestMethod]
        public void Clasificar_NoAplicaHE_FactorUno()
        {
            EntTcDatos d = Datos();
            d.Tarifas.Add(Tarifa("C1", aplica: false));
            d.Actividades.Add(Act(tipoHe: 1, tiempo: "02:00:00"));

            EntTcFila f = Una(d);

            Assert.AreEqual(1m, f.Factor);
            Assert.AreEqual(10m, f.Valor);
        }

        [TestMethod]
        public void Clasificar_JornadaDeCuatroHoras_DivideEntre120()
        {
            EntTcDatos d = Datos();
            d.Tarifas.Add(Tarifa("C1", jornada: 4));
            d.Actividades.Add(Act());

            Assert.AreEqual(10m, Una(d).ValorHora);
        }

        [TestMethod]
        public void Clasificar_SinJornadaOJornadaCero_UsaOcho()
        {
            EntTcDatos d = Datos();
            d.Tarifas.Add(Tarifa("C1", jornada: null));
            d.Tarifas.Add(Tarifa("C2", jornada: 0));
            d.Actividades.Add(Act(cod: "C1", nombre: "PERSONA UNO"));
            d.Actividades.Add(Act(cod: "C2", nombre: "PERSONA DOS"));

            List<EntTcFila> f = NegTableroCostos.Clasificar(d).Filas;

            Assert.AreEqual(5m, f[0].ValorHora);
            Assert.AreEqual(5m, f[1].ValorHora);
        }

        [TestMethod]
        public void Clasificar_SinTarifa_ValorCeroYMarcado()
        {
            EntTcDatos d = Datos();
            d.Actividades.Add(Act(cod: "C9", tipoHe: 1));

            EntTcFila f = Una(d);

            Assert.AreEqual(0m, f.Valor);
            Assert.IsTrue(f.SinSalario);
        }

        [TestMethod]
        public void Clasificar_TarifaSinSalario_ValorCeroYMarcado()
        {
            EntTcDatos d = Datos();
            d.Tarifas.Add(Tarifa("C1", salario: null));
            d.Actividades.Add(Act());

            EntTcFila f = Una(d);

            Assert.AreEqual(0m, f.Valor);
            Assert.IsTrue(f.SinSalario);
        }

        [TestMethod]
        public void Clasificar_VariasCuentasMismoNombre_UsaLaQueTieneSalario()
        {
            EntTcDatos d = Datos();
            d.Tarifas.Add(Tarifa("C1", salario: null));
            d.Tarifas.Add(Tarifa("C2"));
            d.Actividades.Add(Act(cod: "C1", nombre: "PERSONA UNO"));
            d.Actividades.Add(Act(cod: "C2", nombre: "PERSONA UNO"));
            d.Actividades.Add(Act(cod: "C3", nombre: "PERSONA UNO"));

            foreach (EntTcFila f in NegTableroCostos.Clasificar(d).Filas)
            {
                Assert.AreEqual(5m, f.ValorHora, "Actividad " + f.IdDetalle);
                Assert.IsFalse(f.SinSalario);
            }
        }

        [TestMethod]
        public void Clasificar_ParametrosFaltantes_UsaLosDelExcelYAvisa()
        {
            EntTcDatos d = new EntTcDatos();
            d.Tarifas.Add(Tarifa("C1"));
            d.Actividades.Add(Act(tipoHe: 1));

            EntTcClasificado c = NegTableroCostos.Clasificar(d);

            Assert.IsTrue(c.ParametrosPorDefecto);
            Assert.AreEqual(1.5m, c.Filas[0].Factor);
            Assert.AreEqual(5m, c.Filas[0].ValorHora);
        }

        [TestMethod]
        public void Clasificar_ParametrosDeLaBase_SeRespetan()
        {
            EntTcDatos d = Datos();
            d.Parametros["Factor50"] = 1.25m;
            d.Parametros["DiasMes"] = 20m;
            d.Tarifas.Add(Tarifa("C1"));
            d.Actividades.Add(Act(tipoHe: 1));

            EntTcClasificado c = NegTableroCostos.Clasificar(d);

            Assert.IsFalse(c.ParametrosPorDefecto);
            Assert.AreEqual(1.25m, c.Filas[0].Factor);
            Assert.AreEqual(7.5m, c.Filas[0].ValorHora);
        }

        [TestMethod]
        public void Clasificar_TextosVaciosYEspacios()
        {
            EntTcDatos d = Datos();
            d.Actividades.Add(Act(cod: " C7 ", nombre: "", empresa: "   ", contrato: null));
            d.Actividades.Add(Act(nombre: " PERSONA UNO ", empresa: " EMPRESA A "));

            List<EntTcFila> f = NegTableroCostos.Clasificar(d).Filas;

            Assert.AreEqual("Cod C7", f[0].Responsable);
            Assert.AreEqual("(Sin empresa)", f[0].Empresa);
            Assert.AreEqual("(Sin contrato)", f[0].Contrato);
            Assert.AreEqual("PERSONA UNO", f[1].Responsable);
            Assert.AreEqual("EMPRESA A", f[1].Empresa);
        }

        [TestMethod]
        public void Clasificar_Periodo()
        {
            EntTcDatos d = Datos();
            d.Actividades.Add(Act(anio: 2026, mes: 3));

            EntTcFila f = Una(d);

            Assert.AreEqual("Mar-2026", f.Periodo);
            Assert.AreEqual(2026, f.Anio);
            Assert.AreEqual(3, f.Mes);
        }

        [TestMethod]
        public void Categoria_SeisCategorias()
        {
            EntTcDatos d = Datos();
            d.Actividades.Add(Act());
            d.Actividades.Add(Act(tipoHe: 1));
            d.Actividades.Add(Act(tipoHe: 2));
            d.Actividades.Add(Act(estado: "Movilización"));
            d.Actividades.Add(Act(estado: "Movilización", tipoHe: 1));
            d.Actividades.Add(Act(estado: "Movilización", tipoHe: 2));

            string[] cats = NegTableroCostos.Clasificar(d).Filas.Select(NegTableroCostos.Categoria).ToArray();

            CollectionAssert.AreEqual(new[]
            {
                "Normal", "Extra 50%", "Extra 100%", "Traslado sin recargo", "Traslado 50%", "Traslado 100%"
            }, cats);
        }
    }
}
```

Agregar en `CapaPruebas/CapaPruebas.csproj`, junto a `<Compile Include="NegReenvioSolicitudTests.cs" />`:

```xml
    <Compile Include="NegTableroCostosTests.cs" />
```

- [ ] **Paso 3: Compilar para ver que falla**

Run: el comando de compilación de las Restricciones globales.
Expected: FAIL con `CS0103: The name 'NegTableroCostos' does not exist in the current context`.

- [ ] **Paso 4: Escribir la clasificación** `CapaNegocio/NegTableroCostos.cs` (UTF-8 con BOM)

```csharp
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Tablero de Costos: la logica de C:\ReporteExcel\generar_dashboard.py
    /// llevada a la aplicacion. El Excel que genera ese script es la referencia:
    /// si el tablero y el Excel no cuadran con los mismos datos, el que esta mal
    /// es el tablero.
    ///
    /// Clase pura en este archivo y en NegTableroCostos.Armar.cs: no toca base,
    /// sesion ni HttpContext. El unico archivo que llama a CapaDato es
    /// NegTableroCostos.Datos.cs.
    /// </summary>
    public static partial class NegTableroCostos
    {
        public const string Normal = "Normal";
        public const string Extra = "Extra";
        public const string Traslado = "Traslado";

        public const string SinRecargo = "Sin recargo";
        public const string Recargo50 = "50%";
        public const string Recargo100 = "100%";

        public const string Todos = "Todos";

        /// <summary>Las seis categorias de los desgloses y los graficos, en el orden del Excel.</summary>
        public const string CatNormal = "Normal";
        public const string CatExtra50 = "Extra 50%";
        public const string CatExtra100 = "Extra 100%";
        public const string CatTrasladoSin = "Traslado sin recargo";
        public const string CatTraslado50 = "Traslado 50%";
        public const string CatTraslado100 = "Traslado 100%";

        public static readonly string[] Recargos = { SinRecargo, Recargo50, Recargo100 };
        public static readonly string[] EstadosHE = { "Aprobado", "Solicitado", "Rechazado", "Sin solicitud", "No aplica" };

        /// <summary>IdDet_EstadoIni que el Excel cuenta como traslado aunque el estado no diga "Movilizacion".</summary>
        private static readonly int[] EstadosIniTraslado = { 14, 28, 62 };

        private static readonly string[] NombresMes =
            { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

        /// <summary>
        /// Una actividad cae en UN solo tipo, en este orden: Traslado, Extra, Normal.
        /// El prefijo "movilizaci" cubre "Movilizacion" con y sin tilde.
        /// </summary>
        public static string Tipo(EntTcActividad a)
        {
            string estado = (a.Estado ?? "").Trim().ToLowerInvariant();
            if (estado.StartsWith("movilizaci", StringComparison.Ordinal)
                || (a.IdEstadoIni.HasValue && Array.IndexOf(EstadosIniTraslado, a.IdEstadoIni.Value) >= 0))
            {
                return Traslado;
            }
            if (a.TipoHE == 1 || a.TipoHE == 2)
            {
                return Extra;
            }
            return Normal;
        }

        public static string Recargo(EntTcActividad a, string tipo)
        {
            if (tipo != Normal && a.TipoHE == 1) { return Recargo50; }
            if (tipo != Normal && a.TipoHE == 2) { return Recargo100; }
            return tipo == Traslado ? SinRecargo : "";
        }

        /// <summary>Catalogo IdTipoCatalogo = 4: 1 Solicitado, 2 Aprobado, 3 Rechazado.</summary>
        public static string EstadoHe(EntTcActividad a, string tipo, string recargo)
        {
            if (recargo == Recargo50 || recargo == Recargo100)
            {
                switch (a.EstadoHE)
                {
                    case 1: return "Solicitado";
                    case 2: return "Aprobado";
                    case 3: return "Rechazado";
                    default: return "Sin solicitud";
                }
            }
            return tipo == Traslado ? "No aplica" : "";
        }

        /// <summary>Det_Tiempo "hh:mm:ss" a horas decimales, a 4 decimales. Formato invalido = 0.</summary>
        public static decimal Horas(string tiempo)
        {
            string[] p = (tiempo ?? "").Trim().Split(':');
            int h, m, s;
            if (p.Length != 3
                || !int.TryParse(p[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out h)
                || !int.TryParse(p[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out m)
                || !int.TryParse(p[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out s))
            {
                return 0m;
            }
            return Math.Round(h + m / 60m + s / 3600m, 4, MidpointRounding.ToEven);
        }

        public static string Categoria(EntTcFila r)
        {
            if (r.Tipo == Extra)
            {
                return r.Recargo == Recargo100 ? CatExtra100 : CatExtra50;
            }
            if (r.Tipo == Traslado)
            {
                if (r.Recargo == Recargo50) { return CatTraslado50; }
                if (r.Recargo == Recargo100) { return CatTraslado100; }
                return CatTrasladoSin;
            }
            return CatNormal;
        }

        /// <summary>
        /// Clasifica y valora cada actividad.
        ///   Valor hora = Salario / (JornadaHorasDia x DiasMes); sin jornada, 8.
        ///   Factor     = Factor50 / Factor100 segun el recargo; 1 si no hay
        ///                recargo o si el colaborador tiene AplicaHE = No.
        ///   Valor $    = Horas x Valor hora x Factor.
        /// </summary>
        public static EntTcClasificado Clasificar(EntTcDatos datos)
        {
            EntTcClasificado c = new EntTcClasificado();
            bool faltan = false;
            decimal factor50 = Parametro(datos.Parametros, "Factor50", 1.5m, ref faltan);
            decimal factor100 = Parametro(datos.Parametros, "Factor100", 2m, ref faltan);
            decimal diasMes = Parametro(datos.Parametros, "DiasMes", 30m, ref faltan);
            c.ParametrosPorDefecto = faltan;

            Dictionary<string, EntTcTarifa> porCodigo = new Dictionary<string, EntTcTarifa>(StringComparer.OrdinalIgnoreCase);
            foreach (EntTcTarifa t in datos.Tarifas)
            {
                string clave = Limpio(t.CodUsuario);
                if (clave != "" && !porCodigo.ContainsKey(clave)) { porCodigo[clave] = t; }
            }
            Dictionary<string, EntTcTarifa> porNombre = TarifasPorNombre(datos.Actividades, porCodigo);

            foreach (EntTcActividad a in datos.Actividades)
            {
                string tipo = Tipo(a);
                string recargo = Recargo(a, tipo);
                string responsable = NombreResponsable(a);

                EntTcTarifa tarifa;
                porNombre.TryGetValue(responsable, out tarifa);

                decimal valorHora = ValorHora(tarifa, diasMes);
                bool aplicaHe = tarifa == null || tarifa.AplicaHE != false;
                decimal factor = !aplicaHe ? 1m
                               : recargo == Recargo50 ? factor50
                               : recargo == Recargo100 ? factor100
                               : 1m;
                decimal horas = Horas(a.Tiempo);

                c.Filas.Add(new EntTcFila
                {
                    IdDetalle = a.IdDetalle,
                    FechaInicio = a.FechaInicio,
                    FechaFin = a.FechaFin,
                    Anio = a.FechaInicio.Year,
                    Mes = a.FechaInicio.Month,
                    Periodo = NombresMes[a.FechaInicio.Month - 1] + "-"
                              + a.FechaInicio.Year.ToString(CultureInfo.InvariantCulture),
                    Responsable = responsable,
                    Empresa = OPorDefecto(a.Empresa, "(Sin empresa)"),
                    Contrato = OPorDefecto(a.Contrato, "(Sin contrato)"),
                    Ticket = Limpio(a.Ticket),
                    OrdenServicio = Limpio(a.OrdenServicio),
                    Tarea = Limpio(a.Tarea),
                    Estado = Limpio(a.Estado),
                    Tipo = tipo,
                    Recargo = recargo,
                    EstadoHE = EstadoHe(a, tipo, recargo),
                    Tiempo = Limpio(a.Tiempo),
                    Horas = horas,
                    Factor = factor,
                    ValorHora = valorHora,
                    Valor = horas * valorHora * factor,
                    SinSalario = valorHora == 0m
                });
            }
            return c;
        }

        /// <summary>
        /// Desviacion deliberada respecto al Excel (ver el plan): el script toma
        /// el Cod_Usuario de la PRIMERA actividad de cada nombre, asi que una
        /// persona con dos cuentas se valora segun el orden de las fechas. Aqui,
        /// entre todos los codigos de ese nombre, gana el primero (alfabetico)
        /// con salario; si ninguno tiene, el primero con tarifa.
        /// </summary>
        private static Dictionary<string, EntTcTarifa> TarifasPorNombre(
            IEnumerable<EntTcActividad> actividades, Dictionary<string, EntTcTarifa> porCodigo)
        {
            Dictionary<string, EntTcTarifa> resultado = new Dictionary<string, EntTcTarifa>();
            foreach (IGrouping<string, EntTcActividad> g in actividades.GroupBy(NombreResponsable))
            {
                List<EntTcTarifa> candidatas = g
                    .Select(a => Limpio(a.CodResponsable))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(cod => cod, StringComparer.Ordinal)
                    .Select(cod =>
                    {
                        EntTcTarifa t;
                        return porCodigo.TryGetValue(cod, out t) ? t : null;
                    })
                    .Where(t => t != null)
                    .ToList();

                resultado[g.Key] = candidatas.FirstOrDefault(t => t.Salario.HasValue && t.Salario.Value > 0m)
                                   ?? candidatas.FirstOrDefault();
            }
            return resultado;
        }

        private static decimal ValorHora(EntTcTarifa t, decimal diasMes)
        {
            if (t == null || !t.Salario.HasValue || t.Salario.Value <= 0m) { return 0m; }
            int jornada = t.JornadaHorasDia.HasValue && t.JornadaHorasDia.Value > 0 ? t.JornadaHorasDia.Value : 8;
            return t.Salario.Value / (jornada * diasMes);
        }

        private static decimal Parametro(Dictionary<string, decimal> p, string clave, decimal porDefecto, ref bool faltan)
        {
            decimal v;
            if (p != null && p.TryGetValue(clave, out v) && v > 0m) { return v; }
            faltan = true;
            return porDefecto;
        }

        private static string NombreResponsable(EntTcActividad a)
        {
            string nombre = Limpio(a.NomResponsable);
            return nombre != "" ? nombre : "Cod " + Limpio(a.CodResponsable);
        }

        private static string Limpio(string s)
        {
            return (s ?? "").Trim();
        }

        private static string OPorDefecto(string s, string porDefecto)
        {
            string v = Limpio(s);
            return v == "" ? porDefecto : v;
        }
    }
}
```

Agregar en `CapaNegocio/CapaNegocio.csproj`, junto a `<Compile Include="NegHeExportacion.cs" />`:

```xml
    <Compile Include="NegTableroCostos.cs" />
```

- [ ] **Paso 5: Compilar y correr las pruebas**

Run: la compilación, y después `vstest.console.exe CapaPruebas\bin\Debug\CapaPruebas.dll /Tests:NegTableroCostosTests`
Expected: compila sin errores y las 26 pruebas pasan. Después correr la suite completa: no debe fallar ninguna prueba anterior.

- [ ] **Paso 6: Commit**

```bash
git add CapaEntidad/EntTableroCostos.cs CapaEntidad/CapaEntidad.csproj CapaNegocio/NegTableroCostos.cs CapaNegocio/CapaNegocio.csproj CapaPruebas/NegTableroCostosTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(tablero-costos): clasificacion y valoracion de actividades"
```

---

### Task 3: Filtros, secciones del tablero y avisos

**Archivos:**
- Crear: `CapaNegocio/NegTableroCostos.Armar.cs`
- Modificar: `CapaPruebas/NegTableroCostosTests.cs` (pruebas nuevas al final de la clase)
- Modificar: `CapaNegocio/CapaNegocio.csproj`

**Interfaces:**
- Consume: todo lo de la Task 2.
- Produce:
  - `NegTableroCostos.Armar(EntTcClasificado datos, EntTcFiltro filtro) : EntTableroCostos`
  - `NegTableroCostos.ValidarRango(DateTime desde, DateTime hasta) : string` (`""` si el rango es válido)
  - `NegTableroCostos.DescribirFiltro(DateTime desde, DateTime hasta, EntTcFiltro filtro) : string`
  - `NegTableroCostos.MesesMaximos = 24`, `NegTableroCostos.TopMaximo = 15`

- [ ] **Paso 1: Agregar las pruebas que fallan** al final de la clase `NegTableroCostosTests`

```csharp
        // ===================================================== Armar (Task 3)

        /*  Juego de datos de las pruebas de Armar. Tarifas: C1 = 5 $/h, C2 = 10 $/h,
            C3 sin salario.
              UNO  EMPRESA A  Mar-2026  Normal               2 h   $10
              UNO  EMPRESA A  Mar-2026  Extra 50% Aprobado   1 h   $7,50
              UNO  EMPRESA A  Abr-2026  Extra 100% Solicit.  1 h   $10
              DOS  EMPRESA B  Mar-2026  Traslado sin recargo 1 h   $10
              DOS  EMPRESA B  Abr-2026  Traslado 50% Aprob.  2 h   $30
              DOS  EMPRESA B  Abr-2026  Normal               3 h   $30
              TRES EMPRESA A  Dic-2025  Normal               1 h   $0
            Totales: Normal 6 h $40 (3) | Extra 2 h $17,50 (2) | Traslado 3 h $40 (2)
                     Total 11 h $97,50 (7) */
        private static EntTcClasificado Base()
        {
            EntTcDatos d = Datos();
            d.Tarifas.Add(Tarifa("C1", salario: 1200m));
            d.Tarifas.Add(Tarifa("C2", salario: 2400m));
            d.Actividades.Add(Act(tiempo: "02:00:00", cod: "C1", nombre: "PERSONA UNO", empresa: "EMPRESA A", mes: 3));
            d.Actividades.Add(Act(tipoHe: 1, estadoHe: 2, cod: "C1", nombre: "PERSONA UNO", empresa: "EMPRESA A", mes: 3));
            d.Actividades.Add(Act(tipoHe: 2, estadoHe: 1, cod: "C1", nombre: "PERSONA UNO", empresa: "EMPRESA A", mes: 4));
            d.Actividades.Add(Act(estado: "Movilización", cod: "C2", nombre: "PERSONA DOS", empresa: "EMPRESA B", mes: 3));
            d.Actividades.Add(Act(estado: "Movilización", tipoHe: 1, estadoHe: 2, tiempo: "02:00:00", cod: "C2", nombre: "PERSONA DOS", empresa: "EMPRESA B", mes: 4));
            d.Actividades.Add(Act(tiempo: "03:00:00", cod: "C2", nombre: "PERSONA DOS", empresa: "EMPRESA B", mes: 4));
            d.Actividades.Add(Act(cod: "C3", nombre: "PERSONA TRES", empresa: "EMPRESA A", anio: 2025, mes: 12));
            return NegTableroCostos.Clasificar(d);
        }

        private static EntTableroCostos Armar(EntTcFiltro f = null)
        {
            return NegTableroCostos.Armar(Base(), f ?? new EntTcFiltro());
        }

        private static void Celda(EntTcCelda c, decimal horas, decimal valor, int actividades, string que)
        {
            Assert.AreEqual(horas, c.Horas, que + ": horas");
            Assert.AreEqual(valor, c.Valor, que + ": valor");
            Assert.AreEqual(actividades, c.Actividades, que + ": actividades");
        }

        private static EntTcResumenFila Resumen(EntTableroCostos t, string concepto)
        {
            return t.Resumen.Single(r => r.Concepto == concepto);
        }

        [TestMethod]
        public void Armar_SinFiltros_Indicadores()
        {
            EntTableroCostos t = Armar();

            Celda(t.Indicadores.Normal, 6m, 40m, 3, "Normal");
            Celda(t.Indicadores.Extra, 2m, 17.5m, 2, "Extra");
            Celda(t.Indicadores.Traslado, 3m, 40m, 2, "Traslado");
            Celda(t.Indicadores.Total, 11m, 97.5m, 7, "Total");
            Assert.AreEqual("", t.Aviso);
        }

        [TestMethod]
        public void Armar_TodosEsIgualASinFiltro()
        {
            EntTableroCostos t = Armar(new EntTcFiltro
            {
                Mes = "Todos", Responsable = "Todos", Empresa = "Todos", Contrato = "Todos", Recargo = "Todos", EstadoHE = "Todos"
            });

            Celda(t.Indicadores.Total, 11m, 97.5m, 7, "Total");
            Assert.AreEqual("", t.Aviso);
        }

        [TestMethod]
        public void Armar_FiltroRecargo_NoAfectaALasNormales()
        {
            EntTableroCostos t = Armar(new EntTcFiltro { Recargo = "50%" });

            Celda(t.Indicadores.Normal, 6m, 40m, 3, "Normal");
            Celda(t.Indicadores.Extra, 1m, 7.5m, 1, "Extra");
            Celda(t.Indicadores.Traslado, 2m, 30m, 1, "Traslado");
        }

        [TestMethod]
        public void Armar_FiltroEstadoHe_NoAfectaALasNormales()
        {
            EntTableroCostos t = Armar(new EntTcFiltro { EstadoHE = "Aprobado" });

            Celda(t.Indicadores.Normal, 6m, 40m, 3, "Normal");
            Celda(t.Indicadores.Extra, 1m, 7.5m, 1, "Extra");
            Celda(t.Indicadores.Traslado, 2m, 30m, 1, "Traslado");
        }

        [TestMethod]
        public void Armar_FiltroResponsable_ElTopLoIgnora()
        {
            EntTableroCostos t = Armar(new EntTcFiltro { Responsable = "PERSONA UNO" });

            Celda(t.Indicadores.Total, 4m, 27.5m, 3, "Total");
            CollectionAssert.AreEqual(new[] { "PERSONA DOS", "PERSONA UNO", "PERSONA TRES" },
                                      t.TopNormal.Select(x => x.Responsable).ToArray());
        }

        [TestMethod]
        public void Armar_FiltroMes_LaTablaPorMesLoIgnoraYLaPorAnioNo()
        {
            EntTableroCostos t = Armar(new EntTcFiltro { Mes = "Mar-2026" });

            Celda(t.Indicadores.Total, 4m, 27.5m, 3, "Total");
            Assert.AreEqual(6m, t.PorMes.Single(p => p.Periodo == "Abr-2026").HTotal);
            Assert.AreEqual(4m, t.PorAnio.Single(p => p.Periodo == "2026").HTotal);
            Assert.AreEqual(0m, t.PorAnio.Single(p => p.Periodo == "2025").HTotal);
        }

        [TestMethod]
        public void Armar_FiltroSinDistinguirMayusculas()
        {
            EntTableroCostos t = Armar(new EntTcFiltro { Empresa = "  empresa b " });

            Celda(t.Indicadores.Traslado, 3m, 40m, 2, "Traslado");
            Assert.AreEqual("", t.Aviso);
        }

        [TestMethod]
        public void Armar_Resumen_OnceFilasEnElOrdenDelExcel()
        {
            EntTableroCostos t = Armar();

            CollectionAssert.AreEqual(new[]
            {
                "Normal", "Extra 50%", "Extra 100%", "Subtotal Horas Extras",
                "Traslado sin recargo", "Traslado 50%", "Traslado 100%", "Subtotal Traslado",
                "Total", "Total al 50% (Extra + Traslado)", "Total al 100% (Extra + Traslado)"
            }, t.Resumen.Select(r => r.Concepto).ToArray());
        }

        [TestMethod]
        public void Armar_Resumen_SubtotalesPorcentajesYValorPorHora()
        {
            EntTableroCostos t = Armar();

            Assert.AreEqual(2m, Resumen(t, "Subtotal Horas Extras").Horas);
            Assert.AreEqual(3m, Resumen(t, "Subtotal Traslado").Horas);
            Assert.AreEqual(3m, Resumen(t, "Total al 50% (Extra + Traslado)").Horas);
            Assert.AreEqual(37.5m, Resumen(t, "Total al 50% (Extra + Traslado)").Valor);
            Assert.AreEqual(1m, Resumen(t, "Total al 100% (Extra + Traslado)").Horas);
            Assert.AreEqual(1m, Resumen(t, "Total").PctHoras);
            Assert.AreEqual(1m, Resumen(t, "Total").PctValor);
            Assert.AreEqual(6m / 11m, Resumen(t, "Normal").PctHoras);
            Assert.AreEqual(40m / 6m, Resumen(t, "Normal").ValorPorHora);
            Assert.IsTrue(Resumen(t, "Subtotal Traslado").EsSubtotal);
            Assert.IsFalse(Resumen(t, "Normal").EsSubtotal);
        }

        [TestMethod]
        public void Armar_SinActividades_NoFallaYPorcentajesEnCero()
        {
            EntTableroCostos t = NegTableroCostos.Armar(new EntTcClasificado(), new EntTcFiltro());

            Assert.AreEqual(11, t.Resumen.Count);
            Assert.IsTrue(t.Resumen.All(r => r.PctHoras == 0m && r.PctValor == 0m && r.ValorPorHora == 0m));
            Assert.AreEqual(0, t.Indicadores.Total.Actividades);
            Assert.AreEqual(0, t.Detalle.Count);
            Assert.AreEqual("", t.Aviso);
        }

        [TestMethod]
        public void Armar_ExtrasPorEstado()
        {
            EntTableroCostos t = Armar();

            CollectionAssert.AreEqual(new[] { "Aprobado", "Solicitado", "Rechazado", "Sin solicitud", "Total" },
                                      t.ExtrasPorEstado.Select(e => e.Estado).ToArray());
            EntTcEstadoExtra aprobado = t.ExtrasPorEstado[0];
            Assert.AreEqual(1m, aprobado.Horas50);
            Assert.AreEqual(7.5m, aprobado.Valor50);
            Assert.AreEqual(0m, aprobado.Horas100);
            EntTcEstadoExtra solicitado = t.ExtrasPorEstado[1];
            Assert.AreEqual(1m, solicitado.Horas100);
            Assert.AreEqual(10m, solicitado.Valor100);
            Assert.AreEqual(17.5m, t.ExtrasPorEstado[4].Total);
        }

        [TestMethod]
        public void Armar_TrasladosPorEstado_IncluyeNoAplica()
        {
            EntTableroCostos t = Armar();

            CollectionAssert.AreEqual(new[] { "Aprobado", "Solicitado", "Rechazado", "Sin solicitud", "No aplica", "Total" },
                                      t.TrasladosPorEstado.Select(e => e.Estado).ToArray());
            Assert.AreEqual(2m, t.TrasladosPorEstado[0].Horas50);
            Assert.AreEqual(1m, t.TrasladosPorEstado[4].HorasSinRecargo);
            Assert.AreEqual(3m, t.TrasladosPorEstado[5].TotalHoras);
            Assert.AreEqual(40m, t.TrasladosPorEstado[5].TotalValor);
        }

        [TestMethod]
        public void Armar_PorAnioYPorMes_LlevanFilaTotal()
        {
            EntTableroCostos t = Armar();

            CollectionAssert.AreEqual(new[] { "2025", "2026", "Total" }, t.PorAnio.Select(p => p.Periodo).ToArray());
            CollectionAssert.AreEqual(new[] { "Dic-2025", "Mar-2026", "Abr-2026", "Total" }, t.PorMes.Select(p => p.Periodo).ToArray());
            EntTcPeriodo total = t.PorAnio.Last();
            Assert.AreEqual(11m, total.HTotal);
            Assert.AreEqual(97.5m, total.VTotal);
            Assert.AreEqual(6m, total.HNormal);
            Assert.AreEqual(1m, total.HExtra50);
            Assert.AreEqual(1m, total.HExtra100);
            Assert.AreEqual(1m, total.HTrasladoSin);
            Assert.AreEqual(2m, total.HTraslado50);
            Assert.AreEqual(0m, total.HTraslado100);
        }

        [TestMethod]
        public void Armar_Top_OrdenadoPorValorYMaximoQuince()
        {
            EntTcDatos d = Datos();
            for (int i = 1; i <= 16; i++)
            {
                string cod = "P" + i.ToString("00", CultureInfo.InvariantCulture);
                d.Tarifas.Add(Tarifa(cod));
                d.Actividades.Add(Act(cod: cod, nombre: "PERSONA " + cod, tiempo: i.ToString("00", CultureInfo.InvariantCulture) + ":00:00"));
            }

            EntTableroCostos t = NegTableroCostos.Armar(NegTableroCostos.Clasificar(d), new EntTcFiltro());

            Assert.AreEqual(NegTableroCostos.TopMaximo, t.TopNormal.Count);
            Assert.AreEqual("PERSONA P16", t.TopNormal[0].Responsable);
            Assert.AreEqual(80m, t.TopNormal[0].Valor);
            Assert.IsFalse(t.TopNormal.Any(x => x.Responsable == "PERSONA P01"));
        }

        [TestMethod]
        public void Armar_TopExtraYTraslado_DesglosanRecargo()
        {
            EntTableroCostos t = Armar();

            EntTcTop extra = t.TopExtra.Single();
            Assert.AreEqual("PERSONA UNO", extra.Responsable);
            Assert.AreEqual(1m, extra.Horas50);
            Assert.AreEqual(1m, extra.Horas100);
            Assert.AreEqual(17.5m, extra.Valor);

            EntTcTop traslado = t.TopTraslado.Single();
            Assert.AreEqual(1m, traslado.HorasSinRecargo);
            Assert.AreEqual(2m, traslado.Horas50);
            Assert.AreEqual(40m, traslado.Valor);
        }

        [TestMethod]
        public void Armar_Listas_SonDependientesYMesesCronologicos()
        {
            EntTableroCostos todo = Armar();
            CollectionAssert.AreEqual(new[] { "Dic-2025", "Mar-2026", "Abr-2026" }, todo.Meses.ToArray());

            EntTableroCostos t = Armar(new EntTcFiltro { Responsable = "PERSONA DOS" });
            CollectionAssert.AreEqual(new[] { "EMPRESA B" }, t.Empresas.ToArray());
            CollectionAssert.AreEqual(new[] { "PERSONA DOS", "PERSONA TRES", "PERSONA UNO" }, t.Responsables.ToArray());
        }

        [TestMethod]
        public void Armar_ElegidoQueNoExisteConLosDemas_AvisaYLoConserva()
        {
            EntTableroCostos t = Armar(new EntTcFiltro { Empresa = "EMPRESA B", Responsable = "PERSONA UNO" });

            Assert.AreEqual(0, t.Indicadores.Total.Actividades);
            StringAssert.StartsWith(t.Aviso, "⚠ ");
            StringAssert.Contains(t.Aviso, "El responsable elegido");
            StringAssert.Contains(t.Aviso, "La empresa elegida");
            StringAssert.EndsWith(t.Aviso, "Elija otro valor o Todos.");
            CollectionAssert.Contains(t.Responsables, "PERSONA UNO");
            CollectionAssert.Contains(t.Empresas, "EMPRESA B");
        }

        /* Con Mes/Responsable/Empresa/Contrato elegidos, el aviso de la lista
           dependiente sale primero. "Ninguna actividad" queda para cuando solo
           Recargo o Estado HE vacian el resultado: como las Normales nunca se
           filtran por recargo, eso pasa solo si no hay Normales. */
        [TestMethod]
        public void Armar_RecargoQueVaciaElResultado_Avisa()
        {
            EntTcDatos d = Datos();
            d.Tarifas.Add(Tarifa("C1"));
            d.Actividades.Add(Act(tipoHe: 1));

            EntTableroCostos t = NegTableroCostos.Armar(NegTableroCostos.Clasificar(d), new EntTcFiltro { Recargo = "100%" });

            Assert.AreEqual(0, t.Indicadores.Total.Actividades);
            StringAssert.Contains(t.Aviso, "Ninguna actividad cumple los filtros elegidos.");
        }

        [TestMethod]
        public void Armar_SinSalario_SoloDelResultadoFiltrado()
        {
            CollectionAssert.AreEqual(new[] { "PERSONA TRES" }, Armar().SinSalario.ToArray());
            Assert.AreEqual(0, Armar(new EntTcFiltro { Responsable = "PERSONA UNO" }).SinSalario.Count);
        }

        [TestMethod]
        public void Armar_Detalle_OrdenadoPorFecha()
        {
            EntTableroCostos t = Armar();

            Assert.AreEqual(7, t.Detalle.Count);
            Assert.AreEqual("PERSONA TRES", t.Detalle[0].Responsable);
            Assert.IsTrue(t.Detalle.Zip(t.Detalle.Skip(1), (a, b) => a.FechaInicio <= b.FechaInicio).All(x => x));
        }

        [TestMethod]
        public void Armar_ParametrosPorDefecto_SePropaga()
        {
            EntTcDatos d = new EntTcDatos();
            d.Actividades.Add(Act());

            Assert.IsTrue(NegTableroCostos.Armar(NegTableroCostos.Clasificar(d), new EntTcFiltro()).ParametrosPorDefecto);
            Assert.IsFalse(Armar().ParametrosPorDefecto);
        }

        // ----------------------------------------------- rango y descripcion

        [TestMethod]
        public void ValidarRango_DosAniosCalendario_EsValido()
        {
            Assert.AreEqual("", NegTableroCostos.ValidarRango(new DateTime(2025, 1, 1), new DateTime(2026, 12, 31)));
        }

        [TestMethod]
        public void ValidarRango_HastaAnteriorADesde_Rechaza()
        {
            Assert.AreNotEqual("", NegTableroCostos.ValidarRango(new DateTime(2026, 3, 1), new DateTime(2026, 2, 28)));
        }

        [TestMethod]
        public void ValidarRango_MasDeVeinticuatroMeses_Rechaza()
        {
            StringAssert.Contains(NegTableroCostos.ValidarRango(new DateTime(2025, 1, 1), new DateTime(2027, 1, 1)), "24 meses");
        }

        [TestMethod]
        public void DescribirFiltro_ConCulturaEspanola_NoCambiaElFormato()
        {
            CultureInfo antes = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
                string s = NegTableroCostos.DescribirFiltro(new DateTime(2025, 1, 1), new DateTime(2026, 12, 31),
                                                            new EntTcFiltro { Empresa = " EMPRESA B ", Recargo = "Todos" });

                Assert.AreEqual("Desde 01/01/2025 · Hasta 31/12/2026 · Mes: Todos · Responsable: Todos · "
                              + "Empresa: EMPRESA B · Contrato: Todos · Recargo: Todos · Estado HE: Todos", s);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = antes;
            }
        }
```

- [ ] **Paso 2: Compilar para ver que falla**

Expected: FAIL con `CS0117: 'NegTableroCostos' does not contain a definition for 'Armar'`.

- [ ] **Paso 3: Implementar** `CapaNegocio/NegTableroCostos.Armar.cs` (UTF-8 con BOM)

```csharp
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Filtros y secciones del tablero, con las mismas reglas que las formulas
    /// del Excel:
    ///   - Mes, Responsable, Empresa y Contrato filtran todo.
    ///   - Recargo y Estado HE filtran SOLO Extra y Traslado.
    ///   - La tabla por mes ignora el filtro de Mes; el top 15 ignora el de
    ///     Responsable; la tabla por anio aplica todos.
    /// </summary>
    public static partial class NegTableroCostos
    {
        public const int MesesMaximos = 24;
        public const int TopMaximo = 15;

        private static readonly string[] EstadosExtra = { "Aprobado", "Solicitado", "Rechazado", "Sin solicitud" };
        private static readonly string[] EstadosTraslado = { "Aprobado", "Solicitado", "Rechazado", "Sin solicitud", "No aplica" };

        /// <summary>Orden de las listas: alfabetico en espanol, sin distinguir mayusculas (la Ñ va despues de la N).</summary>
        private static readonly StringComparer OrdenTexto = StringComparer.Create(CultureInfo.GetCultureInfo("es-ES"), true);

        [Flags]
        private enum Ignorar
        {
            Nada = 0,
            Mes = 1,
            Responsable = 2,
            Empresa = 4,
            Contrato = 8
        }

        public static EntTableroCostos Armar(EntTcClasificado datos, EntTcFiltro filtro)
        {
            if (datos == null) { datos = new EntTcClasificado(); }
            if (filtro == null) { filtro = new EntTcFiltro(); }

            List<EntTcFila> todas = datos.Filas;
            List<EntTcFila> filtradas = todas.Where(r => Pasa(r, filtro, Ignorar.Nada)).ToList();
            List<EntTcFila> sinMes = todas.Where(r => Pasa(r, filtro, Ignorar.Mes)).ToList();
            List<EntTcFila> sinResponsable = todas.Where(r => Pasa(r, filtro, Ignorar.Responsable)).ToList();

            EntTableroCostos t = new EntTableroCostos();
            t.ParametrosPorDefecto = datos.ParametrosPorDefecto;

            t.Indicadores.Normal = Sumar(filtradas.Where(r => r.Tipo == Normal));
            t.Indicadores.Extra = Sumar(filtradas.Where(r => r.Tipo == Extra));
            t.Indicadores.Traslado = Sumar(filtradas.Where(r => r.Tipo == Traslado));
            t.Indicadores.Total = Sumar(filtradas);

            t.Resumen = ArmarResumen(filtradas);
            t.ExtrasPorEstado = ArmarExtrasPorEstado(filtradas);
            t.TrasladosPorEstado = ArmarTrasladosPorEstado(filtradas);

            foreach (int anio in todas.Select(r => r.Anio).Distinct().OrderBy(a => a))
            {
                t.PorAnio.Add(FilaPeriodo(anio.ToString(CultureInfo.InvariantCulture), filtradas.Where(r => r.Anio == anio)));
            }
            t.PorAnio.Add(FilaPeriodo("Total", filtradas));

            foreach (string periodo in PeriodosCronologicos(todas))
            {
                t.PorMes.Add(FilaPeriodo(periodo, sinMes.Where(r => r.Periodo == periodo)));
            }
            t.PorMes.Add(FilaPeriodo("Total", sinMes));

            t.TopNormal = Top(sinResponsable, Normal);
            t.TopExtra = Top(sinResponsable, Extra);
            t.TopTraslado = Top(sinResponsable, Traslado);

            t.Detalle = filtradas.OrderBy(r => r.FechaInicio).ThenBy(r => r.IdDetalle).ToList();

            t.SinSalario = filtradas.Where(r => r.SinSalario)
                                    .Select(r => r.Responsable)
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .OrderBy(n => n, OrdenTexto)
                                    .ToList();

            /* Listas dependientes: cada una ofrece los valores que tienen
               actividades con los DEMAS filtros elegidos. Si el valor elegido no
               esta, se avisa y se agrega igual, para que el combo lo siga
               mostrando en vez de saltar a "Todos" en silencio. */
            List<string> avisos = new List<string>();

            t.Meses = PeriodosCronologicos(todas.Where(r => Pasa(r, filtro, Ignorar.Mes)));
            Revisar(t.Meses, filtro.Mes, "El mes elegido no tiene actividades con los demás filtros. ", avisos);

            t.Responsables = Valores(todas, filtro, Ignorar.Responsable, r => r.Responsable);
            Revisar(t.Responsables, filtro.Responsable, "El responsable elegido no tiene actividades con los demás filtros. ", avisos);

            t.Empresas = Valores(todas, filtro, Ignorar.Empresa, r => r.Empresa);
            Revisar(t.Empresas, filtro.Empresa, "La empresa elegida no tiene actividades con los demás filtros. ", avisos);

            t.Contratos = Valores(todas, filtro, Ignorar.Contrato, r => r.Contrato);
            Revisar(t.Contratos, filtro.Contrato, "El contrato elegido no tiene actividades con los demás filtros. ", avisos);

            bool hayFiltro = Elegido(filtro.Mes) || Elegido(filtro.Responsable) || Elegido(filtro.Empresa)
                          || Elegido(filtro.Contrato) || Elegido(filtro.Recargo) || Elegido(filtro.EstadoHE);
            if (avisos.Count == 0 && hayFiltro && filtradas.Count == 0)
            {
                avisos.Add("Ninguna actividad cumple los filtros elegidos. ");
            }
            t.Aviso = avisos.Count == 0 ? "" : "⚠ " + string.Concat(avisos) + "Elija otro valor o Todos.";

            return t;
        }

        /// <summary>"" si el rango es valido; si no, el mensaje para el usuario.</summary>
        public static string ValidarRango(DateTime desde, DateTime hasta)
        {
            if (hasta.Date < desde.Date)
            {
                return "La fecha Hasta no puede ser anterior a Desde.";
            }
            if (hasta.Date >= desde.Date.AddMonths(MesesMaximos))
            {
                return "El rango no puede superar " + MesesMaximos.ToString(CultureInfo.InvariantCulture) + " meses.";
            }
            return "";
        }

        /// <summary>Texto de los filtros para la auditoria y la cabecera del Excel. Formato fijo, sin depender de la cultura.</summary>
        public static string DescribirFiltro(DateTime desde, DateTime hasta, EntTcFiltro filtro)
        {
            EntTcFiltro f = filtro ?? new EntTcFiltro();
            return "Desde " + desde.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture)
                 + " · Hasta " + hasta.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture)
                 + " · Mes: " + Mostrar(f.Mes)
                 + " · Responsable: " + Mostrar(f.Responsable)
                 + " · Empresa: " + Mostrar(f.Empresa)
                 + " · Contrato: " + Mostrar(f.Contrato)
                 + " · Recargo: " + Mostrar(f.Recargo)
                 + " · Estado HE: " + Mostrar(f.EstadoHE);
        }

        // ----------------------------------------------------- filtros

        private static bool Pasa(EntTcFila r, EntTcFiltro f, Ignorar ignorar)
        {
            if ((ignorar & Ignorar.Mes) == 0 && Elegido(f.Mes) && !Igual(r.Periodo, f.Mes)) { return false; }
            if ((ignorar & Ignorar.Responsable) == 0 && Elegido(f.Responsable) && !Igual(r.Responsable, f.Responsable)) { return false; }
            if ((ignorar & Ignorar.Empresa) == 0 && Elegido(f.Empresa) && !Igual(r.Empresa, f.Empresa)) { return false; }
            if ((ignorar & Ignorar.Contrato) == 0 && Elegido(f.Contrato) && !Igual(r.Contrato, f.Contrato)) { return false; }

            if (r.Tipo != Normal)
            {
                if (Elegido(f.Recargo) && !Igual(r.Recargo, f.Recargo)) { return false; }
                if (Elegido(f.EstadoHE) && !Igual(r.EstadoHE, f.EstadoHE)) { return false; }
            }
            return true;
        }

        private static bool Elegido(string valor)
        {
            return !string.IsNullOrWhiteSpace(valor)
                && !string.Equals(valor.Trim(), Todos, StringComparison.OrdinalIgnoreCase);
        }

        private static bool Igual(string a, string b)
        {
            return string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static string Mostrar(string valor)
        {
            return Elegido(valor) ? valor.Trim() : Todos;
        }

        // ---------------------------------------------------- secciones

        private static EntTcCelda Sumar(IEnumerable<EntTcFila> filas)
        {
            EntTcCelda c = new EntTcCelda();
            foreach (EntTcFila r in filas)
            {
                c.Horas += r.Horas;
                c.Valor += r.Valor;
                c.Actividades++;
            }
            return c;
        }

        private static EntTcCelda Mas(params EntTcCelda[] celdas)
        {
            EntTcCelda c = new EntTcCelda();
            foreach (EntTcCelda x in celdas)
            {
                c.Horas += x.Horas;
                c.Valor += x.Valor;
                c.Actividades += x.Actividades;
            }
            return c;
        }

        private static List<EntTcResumenFila> ArmarResumen(List<EntTcFila> filas)
        {
            Func<string, EntTcCelda> cat = nombre => Sumar(filas.Where(r => Categoria(r) == nombre));

            EntTcCelda normal = cat(CatNormal);
            EntTcCelda extra50 = cat(CatExtra50);
            EntTcCelda extra100 = cat(CatExtra100);
            EntTcCelda trasladoSin = cat(CatTrasladoSin);
            EntTcCelda traslado50 = cat(CatTraslado50);
            EntTcCelda traslado100 = cat(CatTraslado100);
            EntTcCelda subExtra = Mas(extra50, extra100);
            EntTcCelda subTraslado = Mas(trasladoSin, traslado50, traslado100);
            EntTcCelda total = Mas(normal, subExtra, subTraslado);

            return new List<EntTcResumenFila>
            {
                FilaResumen(CatNormal, normal, total, false),
                FilaResumen(CatExtra50, extra50, total, false),
                FilaResumen(CatExtra100, extra100, total, false),
                FilaResumen("Subtotal Horas Extras", subExtra, total, true),
                FilaResumen(CatTrasladoSin, trasladoSin, total, false),
                FilaResumen(CatTraslado50, traslado50, total, false),
                FilaResumen(CatTraslado100, traslado100, total, false),
                FilaResumen("Subtotal Traslado", subTraslado, total, true),
                FilaResumen("Total", total, total, true),
                FilaResumen("Total al 50% (Extra + Traslado)", Mas(extra50, traslado50), total, true),
                FilaResumen("Total al 100% (Extra + Traslado)", Mas(extra100, traslado100), total, true)
            };
        }

        private static EntTcResumenFila FilaResumen(string concepto, EntTcCelda c, EntTcCelda total, bool subtotal)
        {
            return new EntTcResumenFila
            {
                Concepto = concepto,
                EsSubtotal = subtotal,
                Horas = c.Horas,
                PctHoras = Dividir(c.Horas, total.Horas),
                Valor = c.Valor,
                PctValor = Dividir(c.Valor, total.Valor),
                Actividades = c.Actividades,
                ValorPorHora = Dividir(c.Valor, c.Horas)
            };
        }

        private static decimal Dividir(decimal a, decimal b)
        {
            return b == 0m ? 0m : a / b;
        }

        private static List<EntTcEstadoExtra> ArmarExtrasPorEstado(List<EntTcFila> filas)
        {
            List<EntTcFila> extras = filas.Where(r => r.Tipo == Extra).ToList();
            List<EntTcEstadoExtra> resultado = EstadosExtra
                .Select(e => FilaEstadoExtra(e, extras.Where(r => r.EstadoHE == e)))
                .ToList();
            resultado.Add(FilaEstadoExtra("Total", extras));
            return resultado;
        }

        private static EntTcEstadoExtra FilaEstadoExtra(string estado, IEnumerable<EntTcFila> filas)
        {
            EntTcEstadoExtra e = new EntTcEstadoExtra { Estado = estado };
            foreach (EntTcFila r in filas)
            {
                if (r.Recargo == Recargo50) { e.Horas50 += r.Horas; e.Valor50 += r.Valor; }
                if (r.Recargo == Recargo100) { e.Horas100 += r.Horas; e.Valor100 += r.Valor; }
            }
            e.Total = e.Valor50 + e.Valor100;
            return e;
        }

        private static List<EntTcEstadoTraslado> ArmarTrasladosPorEstado(List<EntTcFila> filas)
        {
            List<EntTcFila> traslados = filas.Where(r => r.Tipo == Traslado).ToList();
            List<EntTcEstadoTraslado> resultado = EstadosTraslado
                .Select(e => FilaEstadoTraslado(e, traslados.Where(r => r.EstadoHE == e)))
                .ToList();
            resultado.Add(FilaEstadoTraslado("Total", traslados));
            return resultado;
        }

        private static EntTcEstadoTraslado FilaEstadoTraslado(string estado, IEnumerable<EntTcFila> filas)
        {
            EntTcEstadoTraslado e = new EntTcEstadoTraslado { Estado = estado };
            foreach (EntTcFila r in filas)
            {
                if (r.Recargo == SinRecargo) { e.HorasSinRecargo += r.Horas; }
                if (r.Recargo == Recargo50) { e.Horas50 += r.Horas; }
                if (r.Recargo == Recargo100) { e.Horas100 += r.Horas; }
                e.TotalHoras += r.Horas;
                e.TotalValor += r.Valor;
            }
            return e;
        }

        private static EntTcPeriodo FilaPeriodo(string etiqueta, IEnumerable<EntTcFila> filas)
        {
            EntTcPeriodo p = new EntTcPeriodo { Periodo = etiqueta };
            foreach (EntTcFila r in filas)
            {
                switch (Categoria(r))
                {
                    case CatNormal: p.HNormal += r.Horas; p.VNormal += r.Valor; break;
                    case CatExtra50: p.HExtra50 += r.Horas; p.VExtra50 += r.Valor; break;
                    case CatExtra100: p.HExtra100 += r.Horas; p.VExtra100 += r.Valor; break;
                    case CatTrasladoSin: p.HTrasladoSin += r.Horas; p.VTrasladoSin += r.Valor; break;
                    case CatTraslado50: p.HTraslado50 += r.Horas; p.VTraslado50 += r.Valor; break;
                    case CatTraslado100: p.HTraslado100 += r.Horas; p.VTraslado100 += r.Valor; break;
                }
                p.HTotal += r.Horas;
                p.VTotal += r.Valor;
            }
            return p;
        }

        private static List<EntTcTop> Top(IEnumerable<EntTcFila> filas, string tipo)
        {
            return filas.Where(r => r.Tipo == tipo)
                .GroupBy(r => r.Responsable)
                .Select(g => new EntTcTop
                {
                    Responsable = g.Key,
                    Horas = g.Sum(r => r.Horas),
                    HorasSinRecargo = g.Where(r => r.Recargo == SinRecargo).Sum(r => r.Horas),
                    Horas50 = g.Where(r => r.Recargo == Recargo50).Sum(r => r.Horas),
                    Horas100 = g.Where(r => r.Recargo == Recargo100).Sum(r => r.Horas),
                    Valor = g.Sum(r => r.Valor)
                })
                .OrderByDescending(x => x.Valor)
                .ThenByDescending(x => x.Horas)
                .ThenBy(x => x.Responsable, StringComparer.Ordinal)
                .Take(TopMaximo)
                .ToList();
        }

        // ------------------------------------------------------- listas

        private static List<string> PeriodosCronologicos(IEnumerable<EntTcFila> filas)
        {
            return filas.GroupBy(r => new { r.Anio, r.Mes, r.Periodo })
                        .OrderBy(g => g.Key.Anio)
                        .ThenBy(g => g.Key.Mes)
                        .Select(g => g.Key.Periodo)
                        .ToList();
        }

        private static List<string> Valores(List<EntTcFila> todas, EntTcFiltro f, Ignorar ignorar, Func<EntTcFila, string> campo)
        {
            return todas.Where(r => Pasa(r, f, ignorar))
                        .Select(campo)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(v => v, OrdenTexto)
                        .ToList();
        }

        private static void Revisar(List<string> lista, string elegido, string texto, List<string> avisos)
        {
            if (!Elegido(elegido) || lista.Contains(elegido.Trim(), StringComparer.OrdinalIgnoreCase)) { return; }
            avisos.Add(texto);
            lista.Add(elegido.Trim());
        }
    }
}
```

> El separador `·` y el `⚠` llevan UTF-8 con BOM en el `.cs`. El JSON los escapa como `\uXXXX` y el Excel los guarda en Unicode, así que llegan bien a los dos lados.

Agregar en `CapaNegocio/CapaNegocio.csproj`, junto a `<Compile Include="NegTableroCostos.cs" />`:

```xml
    <Compile Include="NegTableroCostos.Armar.cs" />
```

- [ ] **Paso 4: Compilar y correr las pruebas**

Run: la compilación y `vstest.console.exe CapaPruebas\bin\Debug\CapaPruebas.dll /Tests:NegTableroCostosTests`
Expected: todas pasan (26 de la Task 2 + 25 nuevas). Después correr la suite completa.

- [ ] **Paso 5: Commit**

```bash
git add CapaNegocio/NegTableroCostos.Armar.cs CapaNegocio/CapaNegocio.csproj CapaPruebas/NegTableroCostosTests.cs
git commit -m "feat(tablero-costos): filtros, secciones y avisos del tablero"
```

---

### Task 4: Acceso a datos y auditoría

**Archivos:**
- Crear: `CapaDato/DaoTableroCostos.cs`
- Crear: `CapaNegocio/NegTableroCostos.Datos.cs`
- Modificar: `CapaDato/CapaDato.csproj`, `CapaNegocio/CapaNegocio.csproj`

**Interfaces:**
- Consume: los tres resultados de `Sp_RTA_TableroCostos` y `Sp_RTA_TableroCostosAuditar` (Task 1), y `NegTableroCostos.Clasificar` (Task 2).
- Produce:
  - `DaoTableroCostos.Cargar(DateTime desde, DateTime hasta) : EntTcDatos`
  - `DaoTableroCostos.Auditar(string codUsuario, string accion, string filtros) : int`
  - `NegTableroCostos.Cargar(DateTime desde, DateTime hasta) : EntTcClasificado`
  - `NegTableroCostos.AuditarDescarga(string codUsuario, string filtros) : void`. Lanza una excepción si no puede registrar.

> Sin pruebas unitarias: `DaoReporTareaAranda` lee la cadena de `ConfigurationManager`, que fuera de la web es `null`. Esta capa se prueba en la Task 7, en la pantalla, y con la verificación SQL de la Task 1.

- [ ] **Paso 1: Escribir el DAO** `CapaDato/DaoTableroCostos.cs` (UTF-8 con BOM)

```csharp
using CapaEntidad;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace CapaDato
{
    /// <summary>
    /// Sp_RTA_TableroCostos devuelve tres resultados, en este orden: actividades,
    /// tarifas y parametros. Si falta uno, se lanza una excepcion en vez de
    /// seguir: sin tarifas todo saldria valorado en $0, sin ningun error visible.
    /// </summary>
    public class DaoTableroCostos
    {
        public static EntTcDatos Cargar(DateTime desde, DateTime hasta)
        {
            EntTcDatos d = new EntTcDatos();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_TableroCostos", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.Add("@Desde", SqlDbType.Date).Value = desde.Date;
                cmd.Parameters.Add("@Hasta", SqlDbType.Date).Value = hasta.Date;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        d.Actividades.Add(new EntTcActividad
                        {
                            IdDetalle = Convert.ToInt64(dr["Id_RegDetTareas"], CultureInfo.InvariantCulture),
                            FechaInicio = Convert.ToDateTime(dr["Det_Fch_RegDetalleIni"], CultureInfo.InvariantCulture),
                            FechaFin = FechaNula(dr, "Det_Fch_RegDetalleFin"),
                            Tiempo = Texto(dr, "Det_Tiempo"),
                            CodResponsable = Texto(dr, "Id_Responsable"),
                            NomResponsable = Texto(dr, "Nom_Usuario"),
                            Empresa = Texto(dr, "Det_Nom_Empresa"),
                            Contrato = Texto(dr, "NombreCliente"),
                            Ticket = Texto(dr, "Det_Id_CompAranda"),
                            OrdenServicio = Texto(dr, "Det_Num_OrdenServicio"),
                            Tarea = Texto(dr, "Det_Det_Tarea"),
                            Estado = Texto(dr, "Det_Estado"),
                            IdEstadoIni = EnteroNulo(dr, "IdDet_EstadoIni"),
                            TipoHE = EnteroNulo(dr, "Det_Horas_Extras_Tipo") ?? 0,
                            EstadoHE = EnteroNulo(dr, "Det_Horas_Extras_Estado") ?? 0
                        });
                    }

                    if (!dr.NextResult())
                    {
                        throw new InvalidOperationException("Sp_RTA_TableroCostos no devolvió las tarifas.");
                    }
                    while (dr.Read())
                    {
                        d.Tarifas.Add(new EntTcTarifa
                        {
                            CodUsuario = Texto(dr, "Cod_Usuario"),
                            Cargo = Texto(dr, "Cargo"),
                            JornadaHorasDia = EnteroNulo(dr, "JornadaHorasDia"),
                            AplicaHE = dr["AplicaHE"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(dr["AplicaHE"], CultureInfo.InvariantCulture),
                            Salario = DecimalNulo(dr, "Monto")
                        });
                    }

                    if (!dr.NextResult())
                    {
                        throw new InvalidOperationException("Sp_RTA_TableroCostos no devolvió los parámetros.");
                    }
                    while (dr.Read())
                    {
                        /* Vienen ordenados por vigencia: la mas reciente pisa a la anterior. */
                        d.Parametros[Texto(dr, "Clave")] = Convert.ToDecimal(dr["Valor"], CultureInfo.InvariantCulture);
                    }
                }
            }
            return d;
        }

        /// <summary>Devuelve el IdAuditoria insertado, o 0 si la base no devolvio nada.</summary>
        public static int Auditar(string codUsuario, string accion, string filtros)
        {
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_TableroCostosAuditar", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CodUsuario", SqlDbType.VarChar, 20).Value = codUsuario ?? "";
                cmd.Parameters.Add("@Accion", SqlDbType.VarChar, 20).Value = accion ?? "";
                cmd.Parameters.Add("@Filtros", SqlDbType.NVarChar, 1000).Value = (object)filtros ?? DBNull.Value;
                cnx.Open();

                object v = cmd.ExecuteScalar();
                return v == null || v == DBNull.Value ? 0 : Convert.ToInt32(v, CultureInfo.InvariantCulture);
            }
        }

        private static string Texto(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? "" : Convert.ToString(v, CultureInfo.InvariantCulture).Trim();
        }

        private static int? EnteroNulo(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? (int?)null : Convert.ToInt32(v, CultureInfo.InvariantCulture);
        }

        private static decimal? DecimalNulo(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? (decimal?)null : Convert.ToDecimal(v, CultureInfo.InvariantCulture);
        }

        private static DateTime? FechaNula(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(v, CultureInfo.InvariantCulture);
        }
    }
}
```

Agregar en `CapaDato/CapaDato.csproj`, junto a `<Compile Include="DaoDashboardAprobacion.cs" />`:

```xml
    <Compile Include="DaoTableroCostos.cs" />
```

- [ ] **Paso 2: Escribir el puente** `CapaNegocio/NegTableroCostos.Datos.cs` (UTF-8 con BOM)

```csharp
using CapaDato;
using CapaEntidad;
using System;

namespace CapaNegocio
{
    /// <summary>Unico archivo de NegTableroCostos que toca la base.</summary>
    public static partial class NegTableroCostos
    {
        /// <summary>Largo de TC_Auditoria.Filtros.</summary>
        public const int LargoFiltrosAuditoria = 1000;

        public static EntTcClasificado Cargar(DateTime desde, DateTime hasta)
        {
            return Clasificar(DaoTableroCostos.Cargar(desde, hasta));
        }

        /// <summary>
        /// Registra la descarga ANTES de armar el archivo. Si no puede, lanza una
        /// excepcion y el handler no entrega nada: el Excel lleva valores
        /// calculados con salarios.
        /// </summary>
        public static void AuditarDescarga(string codUsuario, string filtros)
        {
            if (string.IsNullOrWhiteSpace(codUsuario))
            {
                throw new InvalidOperationException("No se puede registrar una descarga sin usuario.");
            }
            string texto = filtros ?? "";
            if (texto.Length > LargoFiltrosAuditoria)
            {
                texto = texto.Substring(0, LargoFiltrosAuditoria);
            }
            int id = DaoTableroCostos.Auditar(codUsuario.Trim(), "Descargar", texto);
            if (id <= 0)
            {
                throw new InvalidOperationException("La base no confirmó el registro de la descarga.");
            }
        }
    }
}
```

Agregar en `CapaNegocio/CapaNegocio.csproj`, junto a `<Compile Include="NegTableroCostos.Armar.cs" />`:

```xml
    <Compile Include="NegTableroCostos.Datos.cs" />
```

- [ ] **Paso 3: Compilar y correr las pruebas**

Expected: compila sin errores y todas las pruebas pasan.

- [ ] **Paso 4: Commit**

```bash
git add CapaDato/DaoTableroCostos.cs CapaDato/CapaDato.csproj CapaNegocio/NegTableroCostos.Datos.cs CapaNegocio/CapaNegocio.csproj
git commit -m "feat(tablero-costos): acceso a datos y auditoria de descargas"
```

---

### Task 5: Exportar a Excel

**Archivos:**
- Crear: `CapaNegocio/NegTableroCostosExportacion.cs`
- Crear: `CapaPruebas/NegTableroCostosExportacionTests.cs`
- Modificar: `CapaNegocio/CapaNegocio.csproj`, `CapaPruebas/CapaPruebas.csproj`

**Interfaces:**
- Consume: `EntTableroCostos` (Task 2), y `NegTableroCostos.Clasificar` / `Armar` en las pruebas.
- Produce:
  - `NegTableroCostosExportacion.Construir(EntTableroCostos tablero, string descripcionFiltros) : byte[]`
  - `NegTableroCostosExportacion.NombreDeArchivo(DateTime desde, DateTime hasta) : string`
  - Constantes `HojaResumen = "Resumen"`, `HojaDetalle = "Detalle"`, `FilaCabeceraResumen = 4` y `ColumnasDetalle`.

- [ ] **Paso 1: Escribir las pruebas que fallan** `CapaPruebas/NegTableroCostosExportacionTests.cs` (UTF-8 con BOM)

```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;
using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace CapaPruebas
{
    /// <summary>
    /// El archivo se arma en memoria: se construye, se vuelve a abrir con EPPlus
    /// y se leen las celdas. Datos inventados: el repositorio es publico.
    /// </summary>
    [TestClass]
    public class NegTableroCostosExportacionTests
    {
        /// <summary>Dos actividades de una persona a 5 $/h: Normal 2 h ($10) y Extra 50% 1 h ($7,50).</summary>
        private static EntTableroCostos Tablero()
        {
            EntTcDatos d = new EntTcDatos();
            d.Parametros["Factor50"] = 1.5m;
            d.Parametros["Factor100"] = 2m;
            d.Parametros["DiasMes"] = 30m;
            d.Tarifas.Add(new EntTcTarifa { CodUsuario = "C1", Salario = 1200m, JornadaHorasDia = 8, AplicaHE = true });
            d.Actividades.Add(new EntTcActividad
            {
                IdDetalle = 1, FechaInicio = new DateTime(2026, 3, 5, 9, 0, 0), FechaFin = new DateTime(2026, 3, 5, 11, 0, 0),
                Tiempo = "02:00:00", CodResponsable = "C1", NomResponsable = "PERSONA DE PRUEBA",
                Empresa = "EMPRESA DE PRUEBA", Contrato = "CONTRATO DE PRUEBA", Estado = "Asignado"
            });
            d.Actividades.Add(new EntTcActividad
            {
                IdDetalle = 2, FechaInicio = new DateTime(2026, 3, 6, 19, 0, 0),
                Tiempo = "01:00:00", CodResponsable = "C1", NomResponsable = "PERSONA DE PRUEBA",
                Empresa = "EMPRESA DE PRUEBA", Contrato = "CONTRATO DE PRUEBA", Estado = "En progreso",
                TipoHE = 1, EstadoHE = 2
            });
            return NegTableroCostos.Armar(NegTableroCostos.Clasificar(d), new EntTcFiltro());
        }

        private static ExcelPackage Abrir(byte[] archivo)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            return new ExcelPackage(new MemoryStream(archivo));
        }

        private static int Columna(string nombre)
        {
            return Array.IndexOf(NegTableroCostosExportacion.ColumnasDetalle, nombre) + 1;
        }

        [TestMethod]
        public void Construir_TieneHojaResumenYDetalle()
        {
            using (ExcelPackage p = Abrir(NegTableroCostosExportacion.Construir(Tablero(), "filtros")))
            {
                Assert.IsNotNull(p.Workbook.Worksheets[NegTableroCostosExportacion.HojaResumen]);
                Assert.IsNotNull(p.Workbook.Worksheets[NegTableroCostosExportacion.HojaDetalle]);
            }
        }

        [TestMethod]
        public void Detalle_CabeceraYUnaFilaPorActividad()
        {
            using (ExcelPackage p = Abrir(NegTableroCostosExportacion.Construir(Tablero(), "filtros")))
            {
                ExcelWorksheet ws = p.Workbook.Worksheets[NegTableroCostosExportacion.HojaDetalle];
                for (int c = 0; c < NegTableroCostosExportacion.ColumnasDetalle.Length; c++)
                {
                    Assert.AreEqual(NegTableroCostosExportacion.ColumnasDetalle[c], ws.Cells[1, c + 1].Text);
                }
                Assert.AreEqual(3, ws.Dimension.End.Row);
            }
        }

        [TestMethod]
        public void Detalle_HorasFactorYValorSonNumeros()
        {
            using (ExcelPackage p = Abrir(NegTableroCostosExportacion.Construir(Tablero(), "filtros")))
            {
                ExcelWorksheet ws = p.Workbook.Worksheets[NegTableroCostosExportacion.HojaDetalle];
                Assert.AreEqual(2m, Convert.ToDecimal(ws.Cells[2, Columna("Horas")].Value, CultureInfo.InvariantCulture));
                Assert.AreEqual(10m, Convert.ToDecimal(ws.Cells[2, Columna("Valor $")].Value, CultureInfo.InvariantCulture));
                Assert.AreEqual(1.5m, Convert.ToDecimal(ws.Cells[3, Columna("Factor")].Value, CultureInfo.InvariantCulture));
                Assert.AreEqual(7.5m, Convert.ToDecimal(ws.Cells[3, Columna("Valor $")].Value, CultureInfo.InvariantCulture));
                Assert.AreEqual("Extra", ws.Cells[3, Columna("Tipo Hora")].Text);
                Assert.AreEqual("Aprobado", ws.Cells[3, Columna("Estado HE")].Text);
            }
        }

        [TestMethod]
        public void Resumen_LlevaLosFiltrosYLasOnceFilas()
        {
            using (ExcelPackage p = Abrir(NegTableroCostosExportacion.Construir(Tablero(), "Desde 01/03/2026 · Hasta 31/03/2026")))
            {
                ExcelWorksheet ws = p.Workbook.Worksheets[NegTableroCostosExportacion.HojaResumen];
                Assert.AreEqual("Desde 01/03/2026 · Hasta 31/03/2026", ws.Cells[2, 1].Text);

                int primera = NegTableroCostosExportacion.FilaCabeceraResumen + 1;
                Assert.AreEqual("Normal", ws.Cells[primera, 1].Text);
                Assert.AreEqual("Total al 100% (Extra + Traslado)", ws.Cells[primera + 10, 1].Text);
                Assert.AreEqual(17.5m, Convert.ToDecimal(ws.Cells[primera + 8, 4].Value, CultureInfo.InvariantCulture));
            }
        }

        [TestMethod]
        public void Construir_SinActividades_NoFalla()
        {
            EntTableroCostos vacio = NegTableroCostos.Armar(new EntTcClasificado(), new EntTcFiltro());

            using (ExcelPackage p = Abrir(NegTableroCostosExportacion.Construir(vacio, "")))
            {
                ExcelWorksheet ws = p.Workbook.Worksheets[NegTableroCostosExportacion.HojaDetalle];
                Assert.AreEqual(NegTableroCostosExportacion.ColumnasDetalle[0], ws.Cells[1, 1].Text);
            }
        }

        [TestMethod]
        public void NombreDeArchivo_ConCulturaEspanola_UsaDigitosFijos()
        {
            CultureInfo antes = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
                Assert.AreEqual("TableroCostos_20250101_20261231.xlsx",
                    NegTableroCostosExportacion.NombreDeArchivo(new DateTime(2025, 1, 1), new DateTime(2026, 12, 31)));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = antes;
            }
        }
    }
}
```

Agregar en `CapaPruebas/CapaPruebas.csproj`, junto a `<Compile Include="NegTableroCostosTests.cs" />`:

```xml
    <Compile Include="NegTableroCostosExportacionTests.cs" />
```

- [ ] **Paso 2: Compilar para ver que falla**

Expected: FAIL con `CS0103: The name 'NegTableroCostosExportacion' does not exist in the current context`.

- [ ] **Paso 3: Implementar** `CapaNegocio/NegTableroCostosExportacion.cs` (UTF-8 con BOM)

```csharp
using CapaEntidad;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CapaNegocio
{
    /// <summary>
    /// El Excel del Tablero de Costos: el resumen por tipo y recargo, y el
    /// detalle de las actividades filtradas, con las columnas de la hoja "Datos"
    /// del Excel original. Se arma en memoria y nunca se escribe a disco, como
    /// NegHeExportacion: lleva valores calculados con salarios.
    /// </summary>
    public static class NegTableroCostosExportacion
    {
        public const string HojaResumen = "Resumen";
        public const string HojaDetalle = "Detalle";
        public const int FilaCabeceraResumen = 4;

        public static readonly string[] ColumnasDetalle =
        {
            "Id Detalle", "Fecha Inicio", "Fecha Fin", "Año", "Mes", "Responsable", "Empresa", "Contrato",
            "Ticket Aranda", "Orden Servicio", "Tarea", "Estado", "Tipo Hora", "Recargo HE", "Estado HE",
            "Tiempo", "Horas", "Factor", "Valor Hora", "Valor $", "Periodo"
        };

        private static readonly string[] ColumnasResumen =
            { "Tipo / Recargo", "Horas", "% horas", "Valor $", "% valor", "Actividades", "$ por hora" };

        public static string NombreDeArchivo(DateTime desde, DateTime hasta)
        {
            return "TableroCostos_"
                 + desde.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "_"
                 + hasta.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ".xlsx";
        }

        public static byte[] Construir(EntTableroCostos tablero, string descripcionFiltros)
        {
            if (tablero == null) { throw new ArgumentNullException("tablero"); }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage paquete = new ExcelPackage())
            {
                EscribirResumen(paquete.Workbook.Worksheets.Add(HojaResumen), tablero, descripcionFiltros ?? "");
                EscribirDetalle(paquete.Workbook.Worksheets.Add(HojaDetalle), tablero.Detalle);
                return paquete.GetAsByteArray();
            }
        }

        private static void EscribirResumen(ExcelWorksheet ws, EntTableroCostos t, string filtros)
        {
            ws.Cells[1, 1].Value = "Tablero de Costos – Tareas Aranda";
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Size = 14;
            ws.Cells[2, 1].Value = filtros;

            Cabecera(ws, FilaCabeceraResumen, ColumnasResumen);

            int fila = FilaCabeceraResumen + 1;
            foreach (EntTcResumenFila r in t.Resumen)
            {
                ws.Cells[fila, 1].Value = r.Concepto;
                ws.Cells[fila, 2].Value = r.Horas;
                ws.Cells[fila, 3].Value = r.PctHoras;
                ws.Cells[fila, 4].Value = r.Valor;
                ws.Cells[fila, 5].Value = r.PctValor;
                ws.Cells[fila, 6].Value = r.Actividades;
                ws.Cells[fila, 7].Value = r.ValorPorHora;
                if (r.EsSubtotal) { ws.Cells[fila, 1, fila, ColumnasResumen.Length].Style.Font.Bold = true; }
                fila++;
            }

            int primera = FilaCabeceraResumen + 1, ultima = fila - 1;
            if (ultima >= primera)
            {
                ws.Cells[primera, 2, ultima, 2].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[primera, 3, ultima, 3].Style.Numberformat.Format = "0.0%";
                ws.Cells[primera, 4, ultima, 4].Style.Numberformat.Format = "\"$\"#,##0.00";
                ws.Cells[primera, 5, ultima, 5].Style.Numberformat.Format = "0.0%";
                ws.Cells[primera, 6, ultima, 6].Style.Numberformat.Format = "#,##0";
                ws.Cells[primera, 7, ultima, 7].Style.Numberformat.Format = "\"$\"#,##0.00";
            }
            ws.Column(1).Width = 38;
            for (int c = 2; c <= ColumnasResumen.Length; c++) { ws.Column(c).Width = 14; }
        }

        private static void EscribirDetalle(ExcelWorksheet ws, List<EntTcFila> filas)
        {
            Cabecera(ws, 1, ColumnasDetalle);

            int fila = 2;
            foreach (EntTcFila r in filas)
            {
                object[] v =
                {
                    r.IdDetalle, r.FechaInicio, r.FechaFin, r.Anio, r.Mes, r.Responsable, r.Empresa, r.Contrato,
                    r.Ticket, r.OrdenServicio, r.Tarea, r.Estado, r.Tipo, r.Recargo, r.EstadoHE,
                    r.Tiempo, r.Horas, r.Factor, r.ValorHora, r.Valor, r.Periodo
                };
                for (int c = 0; c < v.Length; c++) { ws.Cells[fila, c + 1].Value = v[c]; }
                fila++;
            }

            int ultima = fila - 1;
            if (ultima >= 2)
            {
                ws.Cells[2, 2, ultima, 3].Style.Numberformat.Format = "dd/mm/yyyy hh:mm";
                ws.Cells[2, 17, ultima, 17].Style.Numberformat.Format = "0.0000";
                ws.Cells[2, 18, ultima, 18].Style.Numberformat.Format = "0.00";
                ws.Cells[2, 19, ultima, 19].Style.Numberformat.Format = "\"$\"#,##0.0000";
                ws.Cells[2, 20, ultima, 20].Style.Numberformat.Format = "\"$\"#,##0.00";
                ws.Cells[1, 1, ultima, ColumnasDetalle.Length].AutoFilter = true;
            }
            ws.View.FreezePanes(2, 1);

            int[] anchos = { 11, 17, 17, 7, 6, 32, 26, 30, 17, 15, 45, 20, 11, 11, 13, 10, 9, 8, 11, 12, 10 };
            for (int c = 0; c < anchos.Length; c++) { ws.Column(c + 1).Width = anchos[c]; }
        }

        private static void Cabecera(ExcelWorksheet ws, int fila, string[] textos)
        {
            for (int i = 0; i < textos.Length; i++) { ws.Cells[fila, i + 1].Value = textos[i]; }

            ExcelRange r = ws.Cells[fila, 1, fila, textos.Length];
            r.Style.Font.Bold = true;
            r.Style.Font.Color.SetColor(255, 255, 255, 255);
            r.Style.Fill.PatternType = ExcelFillStyle.Solid;
            r.Style.Fill.BackgroundColor.SetColor(255, 0x1F, 0x3A, 0x5F);
        }
    }
}
```

Agregar en `CapaNegocio/CapaNegocio.csproj`, junto a `<Compile Include="NegTableroCostos.Datos.cs" />`:

```xml
    <Compile Include="NegTableroCostosExportacion.cs" />
```

- [ ] **Paso 4: Compilar y correr las pruebas**

Run: la compilación y `vstest.console.exe CapaPruebas\bin\Debug\CapaPruebas.dll /Tests:NegTableroCostosExportacionTests`
Expected: las 6 pruebas pasan. Después correr la suite completa.

- [ ] **Paso 5: Commit**

```bash
git add CapaNegocio/NegTableroCostosExportacion.cs CapaNegocio/CapaNegocio.csproj CapaPruebas/NegTableroCostosExportacionTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(tablero-costos): exportacion a Excel del detalle y el resumen"
```

---

### Task 6: Handlers — consulta (JSON) y descarga

**Archivos:**
- Crear: `ReporteTareas/Formulario/AdministrarTableroCostos.ashx` + `.ashx.cs`
- Crear: `ReporteTareas/Formulario/DescargarTableroCostos.ashx` + `.ashx.cs`
- Modificar: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consume: `NegTableroCostos.Cargar`, `Armar`, `ValidarRango`, `DescribirFiltro` y `AuditarDescarga`, y `NegTableroCostosExportacion.Construir` / `NombreDeArchivo`.
- Produce:
  - **POST** a `AdministrarTableroCostos.ashx` con el cuerpo `[{ "action": "ConsultarTablero", "parameters": { desde, hasta, mes, responsable, empresa, contrato, recargo, estadoHE, refrescar } }]`. Las fechas van en `yyyy-MM-dd` y `refrescar` vale `"1"` o `"0"`. Devuelve `EntRespuesta`, y en `resultado` va el objeto de `ParaPantalla`: las mismas propiedades que `EntTableroCostos`, salvo `Detalle`, donde cada fila es un **arreglo**: `[0 Id, 1 Inicio "dd/MM/yyyy HH:mm", 2 Responsable, 3 Empresa, 4 Contrato, 5 Ticket, 6 Tarea, 7 Estado, 8 Tipo, 9 Recargo, 10 EstadoHE, 11 Horas, 12 Valor]`.
  - **GET** a `DescargarTableroCostos.ashx?desde=&hasta=&mes=&responsable=&empresa=&contrato=&recargo=&estadoHE=`, que devuelve el `.xlsx`.
  - `AdministrarTableroCostos.PerfilesAutorizados` (`internal static readonly int[]`), que usa la página.
- La identidad sale **siempre de la sesión** (`Session["Cod_Usuario"]`, `Session["Id_Perfil"]`), nunca de un parámetro.

- [ ] **Paso 1: Handler JSON**

`ReporteTareas/Formulario/AdministrarTableroCostos.ashx` (UTF-8 con BOM):

```
<%@ WebHandler Language="C#" CodeBehind="AdministrarTableroCostos.ashx.cs" Class="JsonJQueryNetTableroCostos.AdministrarTableroCostos" %>
```

`ReporteTareas/Formulario/AdministrarTableroCostos.ashx.cs` (UTF-8 con BOM):

```csharp
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace JsonJQueryNetTableroCostos
{
    /// <summary>
    /// Handler de TableroCostos.aspx. Accion: ConsultarTablero.
    ///
    /// El control de perfil NO es cosmetico: el menu solo esconde la opcion, y
    /// la pantalla muestra montos calculados con salarios. Esta lista es la
    /// unica; TableroCostos.aspx.cs y DescargarTableroCostos la reusan. Quien
    /// agregue un perfil aqui tiene que agregarlo tambien en PerfilMenu
    /// (docs/sql/2026-10-12-tablero-costos.sql).
    ///
    /// Las actividades clasificadas del rango quedan en Session: cambiar un
    /// filtro no vuelve a la base. "refrescar" = "1" (boton Consultar) fuerza
    /// la relectura.
    /// </summary>
    public class AdministrarTableroCostos : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        internal static readonly int[] PerfilesAutorizados = { 14, 18 };

        private const string ClaveRango = "TableroCostos.Rango";
        private const string ClaveDatos = "TableroCostos.Datos";

        public void ProcessRequest(HttpContext context)
        {
            /* Sin esto los bytes salen en windows-1252 (Web.config) aunque la
               cabecera diga utf-8, y cada tilde llega rota. */
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentType = "application/json";

            string salida;
            if (context.Session == null || context.Session["UserLogin"] == null)
            {
                context.Response.StatusCode = 401;
                salida = Mensaje("0", "Su sesión expiró. Vuelva a iniciar sesión.", "danger");
            }
            else if (!Autorizado(context))
            {
                context.Response.StatusCode = 403;
                salida = Mensaje("0", "No tiene permisos para esta pantalla.", "danger");
            }
            else if (context.Request.ContentType != null && context.Request.ContentType.Contains("json"))
            {
                salida = Despachar(context);
            }
            else
            {
                salida = Mensaje("0", "Petición no válida.", "danger");
            }
            context.Response.Write(salida);
        }

        private string Despachar(HttpContext context)
        {
            try
            {
                string json = new System.IO.StreamReader(context.Request.InputStream).ReadToEnd();
                object[] lista = new JavaScriptSerializer().Deserialize<object[]>(json);
                IDictionary<string, object> peticion = lista != null && lista.Length > 0
                    ? lista[0] as IDictionary<string, object>
                    : null;
                if (peticion == null) { return Mensaje("0", "Petición no válida.", "danger"); }

                string accion = Campo(peticion, "action");
                object parametros;
                peticion.TryGetValue("parameters", out parametros);

                if (accion == "ConsultarTablero") { return Consultar(context, parametros); }
                return Mensaje("0", "La acción solicitada no existe.", "danger");
            }
            catch (Exception ex)
            {
                return Mensaje("0", "Petición no válida. " + ex.Message, "danger");
            }
        }

        private string Consultar(HttpContext context, object p)
        {
            try
            {
                DateTime desde, hasta;
                string error;
                if (!LeerRango(Campo(p, "desde"), Campo(p, "hasta"), out desde, out hasta, out error))
                {
                    return Mensaje("0", error, "warning");
                }

                EntTcClasificado datos = Datos(context, desde, hasta, Campo(p, "refrescar") == "1");
                EntTableroCostos tablero = NegTableroCostos.Armar(datos, LeerFiltro(n => Campo(p, n)));

                return Serializar(new EntRespuesta
                {
                    estado = "1",
                    mensaje = "",
                    tipoMensaje = "success",
                    resultado = ParaPantalla(tablero)
                });
            }
            catch (Exception ex)
            {
                return Mensaje("0", "No se pudo cargar el tablero. " + ex.Message, "danger");
            }
        }

        // ------------------------------------- compartido con la descarga

        internal static bool Autorizado(HttpContext context)
        {
            int idPerfil;
            return int.TryParse(Convert.ToString(context.Session["Id_Perfil"], CultureInfo.InvariantCulture), out idPerfil)
                && Array.IndexOf(PerfilesAutorizados, idPerfil) >= 0;
        }

        internal static bool LeerRango(string desdeTexto, string hastaTexto, out DateTime desde, out DateTime hasta, out string error)
        {
            desde = DateTime.MinValue;
            hasta = DateTime.MinValue;
            if (!DateTime.TryParseExact(desdeTexto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out desde)
                || !DateTime.TryParseExact(hastaTexto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out hasta))
            {
                error = "Indique las fechas Desde y Hasta.";
                return false;
            }
            error = NegTableroCostos.ValidarRango(desde, hasta);
            return error == "";
        }

        internal static EntTcFiltro LeerFiltro(Func<string, string> campo)
        {
            return new EntTcFiltro
            {
                Mes = campo("mes"),
                Responsable = campo("responsable"),
                Empresa = campo("empresa"),
                Contrato = campo("contrato"),
                Recargo = campo("recargo"),
                EstadoHE = campo("estadoHE")
            };
        }

        /// <summary>Las actividades del rango, de la sesion si ya estan; si no, de la base.</summary>
        internal static EntTcClasificado Datos(HttpContext context, DateTime desde, DateTime hasta, bool refrescar)
        {
            string clave = desde.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "|"
                         + hasta.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            EntTcClasificado enSesion = context.Session[ClaveDatos] as EntTcClasificado;
            if (!refrescar && enSesion != null && clave == context.Session[ClaveRango] as string)
            {
                return enSesion;
            }

            EntTcClasificado nuevo = NegTableroCostos.Cargar(desde, hasta);
            context.Session[ClaveRango] = clave;
            context.Session[ClaveDatos] = nuevo;
            return nuevo;
        }

        // ------------------------------------------------------- salida

        /// <summary>
        /// El detalle viaja como arreglos (ver el orden en tableroCostos.js,
        /// constantes D_*): con unas 22.000 filas en 24 meses, repetir los
        /// nombres de propiedad duplicaria el peso del JSON.
        /// </summary>
        private static object ParaPantalla(EntTableroCostos t)
        {
            return new
            {
                t.Indicadores,
                t.Resumen,
                t.ExtrasPorEstado,
                t.TrasladosPorEstado,
                t.PorAnio,
                t.PorMes,
                t.TopNormal,
                t.TopExtra,
                t.TopTraslado,
                t.Meses,
                t.Responsables,
                t.Empresas,
                t.Contratos,
                t.SinSalario,
                t.Aviso,
                t.ParametrosPorDefecto,
                Detalle = t.Detalle.Select(r => new object[]
                {
                    r.IdDetalle,
                    r.FechaInicio.ToString("dd'/'MM'/'yyyy HH:mm", CultureInfo.InvariantCulture),
                    r.Responsable,
                    r.Empresa,
                    r.Contrato,
                    r.Ticket,
                    r.Tarea,
                    r.Estado,
                    r.Tipo,
                    r.Recargo,
                    r.EstadoHE,
                    Math.Round(r.Horas, 4),
                    Math.Round(r.Valor, 2)
                }).ToList()
            };
        }

        private static string Campo(object campos, string nombre)
        {
            IDictionary<string, object> d = campos as IDictionary<string, object>;
            object v;
            if (d == null || !d.TryGetValue(nombre, out v) || v == null) { return ""; }
            return Convert.ToString(v, CultureInfo.InvariantCulture).Trim();
        }

        private static string Mensaje(string estado, string mensaje, string tipo)
        {
            return Serializar(new EntRespuesta { estado = estado, mensaje = mensaje, tipoMensaje = tipo, resultado = "" });
        }

        /// <summary>JavaScriptSerializer escapa las tildes como \uXXXX; MaxJsonLength al maximo por el detalle.</summary>
        private static string Serializar(object o)
        {
            JavaScriptSerializer s = new JavaScriptSerializer();
            s.MaxJsonLength = int.MaxValue;
            return s.Serialize(o);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
```

- [ ] **Paso 2: Handler de descarga**

`ReporteTareas/Formulario/DescargarTableroCostos.ashx` (UTF-8 con BOM):

```
<%@ WebHandler Language="C#" CodeBehind="DescargarTableroCostos.ashx.cs" Class="JsonJQueryNetTableroCostos.DescargarTableroCostos" %>
```

`ReporteTareas/Formulario/DescargarTableroCostos.ashx.cs` (UTF-8 con BOM):

```csharp
using CapaEntidad;
using CapaNegocio;
using System;
using System.Web;

namespace JsonJQueryNetTableroCostos
{
    /// <summary>
    /// El Excel del Tablero de Costos. Es un GET que escribe bytes, como
    /// DescargarHorasExtras.ashx.
    ///
    /// La descarga se REGISTRA ANTES de armar el archivo: si TC_Auditoria no
    /// acepta la fila, no se entrega nada. El archivo lleva valores calculados
    /// con salarios.
    ///
    /// Response.End() queda FUERA de los try: End() lanza ThreadAbortException,
    /// y un catch alrededor intentaria escribir con la respuesta ya enviada.
    /// </summary>
    public class DescargarTableroCostos : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.Session == null || context.Session["UserLogin"] == null || context.Session["Cod_Usuario"] == null)
            {
                NoDisponible(context, "Su sesión expiró. Vuelva a iniciar sesión.", 401);
                return;
            }
            if (!AdministrarTableroCostos.Autorizado(context))
            {
                NoDisponible(context, "No tiene permisos para descargar este archivo.", 403);
                return;
            }

            DateTime desde, hasta;
            string error;
            if (!AdministrarTableroCostos.LeerRango(Param(context, "desde"), Param(context, "hasta"), out desde, out hasta, out error))
            {
                NoDisponible(context, error, 400);
                return;
            }

            EntTcFiltro filtro = AdministrarTableroCostos.LeerFiltro(n => Param(context, n));
            string descripcion = NegTableroCostos.DescribirFiltro(desde, hasta, filtro);
            string codUsuario = Convert.ToString(context.Session["Cod_Usuario"]).Trim();

            try
            {
                NegTableroCostos.AuditarDescarga(codUsuario, descripcion);
            }
            catch (Exception ex)
            {
                NoDisponible(context, "No se pudo registrar la descarga y, por seguridad, no se entrega el archivo. " + ex.Message, 500);
                return;
            }

            byte[] archivo;
            try
            {
                EntTcClasificado datos = AdministrarTableroCostos.Datos(context, desde, hasta, false);
                EntTableroCostos tablero = NegTableroCostos.Armar(datos, filtro);
                archivo = NegTableroCostosExportacion.Construir(tablero, descripcion);
            }
            catch (Exception ex)
            {
                NoDisponible(context, "No se pudo generar el archivo. " + ex.Message, 500);
                return;
            }

            context.Response.Clear();
            context.Response.Buffer = true;
            context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            context.Response.AddHeader("Content-Disposition",
                "attachment; filename=" + NegTableroCostosExportacion.NombreDeArchivo(desde, hasta));
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            context.Response.BinaryWrite(archivo);
            context.Response.End();
        }

        private static string Param(HttpContext context, string nombre)
        {
            return (context.Request.QueryString[nombre] ?? "").Trim();
        }

        private static void NoDisponible(HttpContext context, string mensaje, int statusCode)
        {
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.Write(mensaje);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
```

- [ ] **Paso 3: Registrar en `ReporteTareas/ReporteTareas.csproj`**

Junto a `<Content Include="Formulario\DescargarHorasExtras.ashx" />`:

```xml
    <Content Include="Formulario\AdministrarTableroCostos.ashx" />
    <Content Include="Formulario\DescargarTableroCostos.ashx" />
```

Junto al bloque `<Compile Include="Formulario\DescargarHorasExtras.ashx.cs">`:

```xml
    <Compile Include="Formulario\AdministrarTableroCostos.ashx.cs">
      <DependentUpon>AdministrarTableroCostos.ashx</DependentUpon>
    </Compile>
    <Compile Include="Formulario\DescargarTableroCostos.ashx.cs">
      <DependentUpon>DescargarTableroCostos.ashx</DependentUpon>
    </Compile>
```

- [ ] **Paso 4: Compilar y correr las pruebas**

Expected: compila sin errores y todas las pruebas pasan.

- [ ] **Paso 5: Commit**

```bash
git add ReporteTareas/Formulario/AdministrarTableroCostos.ashx ReporteTareas/Formulario/AdministrarTableroCostos.ashx.cs ReporteTareas/Formulario/DescargarTableroCostos.ashx ReporteTareas/Formulario/DescargarTableroCostos.ashx.cs ReporteTareas/ReporteTareas.csproj
git commit -m "feat(tablero-costos): handlers de consulta y descarga auditada"
```

---

### Task 7: La pantalla

**Archivos:**
- Crear: `ReporteTareas/Formulario/TableroCostos.aspx`, `.aspx.cs` y `.aspx.designer.cs`
- Crear: `ReporteTareas/js/tableroCostos.js`
- Modificar: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consume: la acción `ConsultarTablero` y la descarga de la Task 6, con los nombres de propiedad de las entidades de la Task 2. También `AdministrarTableroCostos.PerfilesAutorizados`.
- Produce: `Formulario/TableroCostos.aspx`, que es el `Href` del menú de la Task 1.

- [ ] **Paso 1: La página** `ReporteTareas/Formulario/TableroCostos.aspx` (UTF-8 **con BOM**)

```aspx
<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="TableroCostos.aspx.cs" Inherits="ReporteTareas.Formulario.TableroCostos" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .tc-tarjeta { border: 1px solid #ddd; border-radius: 4px; padding: 10px; text-align: center; margin-bottom: 10px; border-top-width: 4px; }
        .tc-tarjeta .tc-titulo { font-weight: bold; }
        .tc-tarjeta .tc-horas { font-size: 20px; font-weight: bold; }
        .tc-normal { border-top-color: #2F6DB5; }
        .tc-extra { border-top-color: #E07B24; }
        .tc-traslado { border-top-color: #1F9E89; }
        .tc-total { border-top-color: #555; }
        .tc-subtotal td { font-weight: bold; background: #f5f5f5; }
        .tc-grafico { position: relative; height: 300px; }
        .tc-grafico-alto { position: relative; height: 420px; }
        .tc-tabla { font-size: 12px; }
        .tc-tabla td.text-right, .tc-tabla th { white-space: nowrap; }
        .tc-aviso { display: none; }
    </style>
    <script src="../js/chart.umd.js" type="text/javascript"></script>
    <script src="../js/tableroCostos.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Tablero de Costos – Tareas Aranda</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">Filtros</div>
                <div class="panel-body">
                    <div class="row">
                        <div class="form-group col-lg-2"><label>Desde</label><input type="date" id="txtDesde" class="form-control" /></div>
                        <div class="form-group col-lg-2"><label>Hasta</label><input type="date" id="txtHasta" class="form-control" /></div>
                        <div class="form-group col-lg-2"><label>Mes</label><select id="cboMes" class="form-control"><option value="Todos">Todos</option></select></div>
                        <div class="form-group col-lg-3"><label>Responsable</label><select id="cboResponsable" class="form-control"><option value="Todos">Todos</option></select></div>
                        <div class="form-group col-lg-3"><label>Empresa</label><select id="cboEmpresa" class="form-control"><option value="Todos">Todos</option></select></div>
                    </div>
                    <div class="row">
                        <div class="form-group col-lg-4"><label>Contrato</label><select id="cboContrato" class="form-control"><option value="Todos">Todos</option></select></div>
                        <div class="form-group col-lg-2"><label>Recargo <small>(solo Extras y Traslado)</small></label>
                            <select id="cboRecargo" class="form-control">
                                <option value="Todos">Todos</option>
                                <option value="Sin recargo">Sin recargo</option>
                                <option value="50%">50%</option>
                                <option value="100%">100%</option>
                            </select>
                        </div>
                        <div class="form-group col-lg-2"><label>Estado HE <small>(solo Extras y Traslado)</small></label>
                            <select id="cboEstadoHE" class="form-control">
                                <option value="Todos">Todos</option>
                                <option value="Aprobado">Aprobado</option>
                                <option value="Solicitado">Solicitado</option>
                                <option value="Rechazado">Rechazado</option>
                                <option value="Sin solicitud">Sin solicitud</option>
                                <option value="No aplica">No aplica</option>
                            </select>
                        </div>
                        <div class="form-group col-lg-4" style="padding-top: 25px">
                            <button type="button" class="btn btn-primary" id="btnConsultar">Consultar</button>
                            <button type="button" class="btn btn-success" id="btnExcel"><i class="fa fa-file-excel-o"></i> Exportar a Excel</button>
                        </div>
                    </div>
                    <div id="tcError" class="alert alert-danger tc-aviso"></div>
                    <div id="tcAviso" class="alert alert-warning tc-aviso"></div>
                    <div id="tcVacio" class="alert alert-info tc-aviso"></div>
                    <div id="tcParametros" class="alert alert-warning tc-aviso"></div>
                    <div id="tcSinSalario" class="alert alert-warning tc-aviso"></div>
                </div>
            </div>
        </div>

        <div class="col-lg-12">
            <div class="row">
                <div class="col-lg-3"><div class="tc-tarjeta tc-normal"><div class="tc-titulo">Horas Normales</div><div class="tc-horas" id="indNormalHoras">-</div><div id="indNormalValor">-</div><div id="indNormalAct">-</div></div></div>
                <div class="col-lg-3"><div class="tc-tarjeta tc-extra"><div class="tc-titulo">Horas Extras (50% + 100%)</div><div class="tc-horas" id="indExtraHoras">-</div><div id="indExtraValor">-</div><div id="indExtraAct">-</div></div></div>
                <div class="col-lg-3"><div class="tc-tarjeta tc-traslado"><div class="tc-titulo">Traslado (sin recargo + 50% + 100%)</div><div class="tc-horas" id="indTrasladoHoras">-</div><div id="indTrasladoValor">-</div><div id="indTrasladoAct">-</div></div></div>
                <div class="col-lg-3"><div class="tc-tarjeta tc-total"><div class="tc-titulo">Total</div><div class="tc-horas" id="indTotalHoras">-</div><div id="indTotalValor">-</div><div id="indTotalAct">-</div></div></div>
            </div>

            <div class="row">
                <div class="col-lg-6">
                    <div class="panel panel-default"><div class="panel-heading">Resumen por tipo y recargo</div>
                        <div class="panel-body" style="overflow-x: auto"><table id="tblResumen" class="table table-condensed table-bordered tc-tabla"></table></div></div>
                </div>
                <div class="col-lg-6">
                    <div class="panel panel-default"><div class="panel-heading">Horas extra por estado y recargo</div>
                        <div class="panel-body" style="overflow-x: auto"><table id="tblExtras" class="table table-condensed table-bordered tc-tabla"></table></div></div>
                    <div class="panel panel-default"><div class="panel-heading">Traslado por estado y recargo</div>
                        <div class="panel-body" style="overflow-x: auto"><table id="tblTraslados" class="table table-condensed table-bordered tc-tabla"></table></div></div>
                </div>
            </div>

            <div class="row">
                <div class="col-lg-6"><div class="panel panel-default"><div class="panel-heading">Distribución de horas por tipo y recargo</div><div class="panel-body"><div class="tc-grafico"><canvas id="grfDonaHoras"></canvas></div></div></div></div>
                <div class="col-lg-6"><div class="panel panel-default"><div class="panel-heading">Distribución del valor ($) por tipo y recargo</div><div class="panel-body"><div class="tc-grafico"><canvas id="grfDonaValor"></canvas></div></div></div></div>
            </div>

            <div class="panel panel-default"><div class="panel-heading">Horas y valor ($) por año, tipo y recargo</div>
                <div class="panel-body" style="overflow-x: auto"><table id="tblAnio" class="table table-condensed table-bordered tc-tabla"></table></div></div>

            <div class="panel panel-default"><div class="panel-heading">Horas y valor ($) por mes, tipo y recargo <small>(no aplica el filtro de Mes)</small></div>
                <div class="panel-body" style="overflow-x: auto"><table id="tblMes" class="table table-condensed table-bordered tc-tabla"></table></div></div>

            <div class="row">
                <div class="col-lg-6"><div class="panel panel-default"><div class="panel-heading">Horas por mes, tipo y recargo</div><div class="panel-body"><div class="tc-grafico"><canvas id="grfMesHoras"></canvas></div></div></div></div>
                <div class="col-lg-6"><div class="panel panel-default"><div class="panel-heading">Valor ($) por mes, tipo y recargo</div><div class="panel-body"><div class="tc-grafico"><canvas id="grfMesValor"></canvas></div></div></div></div>
            </div>

            <h4>Top 15 responsables por tipo de hora <small>(por valor $; no aplica el filtro de Responsable)</small></h4>
            <div class="row">
                <div class="col-lg-4"><div class="panel panel-default"><div class="panel-heading">Normal</div><div class="panel-body">
                    <div class="tc-grafico-alto"><canvas id="grfTopNormal"></canvas></div>
                    <table id="tblTopNormal" class="table table-condensed table-bordered tc-tabla"></table></div></div></div>
                <div class="col-lg-4"><div class="panel panel-default"><div class="panel-heading">Extra</div><div class="panel-body">
                    <div class="tc-grafico-alto"><canvas id="grfTopExtra"></canvas></div>
                    <table id="tblTopExtra" class="table table-condensed table-bordered tc-tabla"></table></div></div></div>
                <div class="col-lg-4"><div class="panel panel-default"><div class="panel-heading">Traslado</div><div class="panel-body">
                    <div class="tc-grafico-alto"><canvas id="grfTopTraslado"></canvas></div>
                    <table id="tblTopTraslado" class="table table-condensed table-bordered tc-tabla"></table></div></div></div>
            </div>

            <div class="panel panel-default"><div class="panel-heading">Detalle de actividades</div>
                <div class="panel-body" style="overflow-x: auto">
                    <table id="tblDetalle" class="table table-condensed table-striped table-bordered tc-tabla">
                        <thead><tr><th>Inicio</th><th>Responsable</th><th>Empresa</th><th>Contrato</th><th>Ticket</th><th>Tarea</th><th>Estado</th><th>Tipo</th><th>Recargo</th><th>Estado HE</th><th>Horas</th><th>Valor $</th></tr></thead>
                        <tbody></tbody>
                    </table>
                    <button type="button" class="btn btn-default btn-sm" id="btnAnterior">Anterior</button>
                    <span id="lblPagina" style="margin: 0 10px"></span>
                    <button type="button" class="btn btn-default btn-sm" id="btnSiguiente">Siguiente</button>
                </div>
            </div>

            <div class="panel panel-default">
                <div class="panel-heading"><a data-toggle="collapse" href="#tcCriterios">Criterios del tablero</a></div>
                <div id="tcCriterios" class="panel-collapse collapse"><div class="panel-body">
                    <ul>
                        <li><b>Traslado:</b> el estado empieza con «Movilización», o <code>IdDet_EstadoIni</code> es 14, 28 o 62. Tiene prioridad sobre Extra.</li>
                        <li><b>Extra:</b> <code>Det_Horas_Extras_Tipo</code> = 1 (50%) o 2 (100%). Estado HE: 1 Solicitado, 2 Aprobado, 3 Rechazado; otro valor, Sin solicitud.</li>
                        <li><b>Normal:</b> el resto de actividades, en cualquier estado (también las pausas).</li>
                        <li><b>Recargo y Estado HE</b> filtran solo Extras y Traslado. Los traslados sin recargo tienen estado «No aplica».</li>
                        <li><b>Valor hora</b> = salario mensual ÷ (jornada en horas por día × días por mes). <b>Valor $</b> = horas × valor hora × factor.</li>
                        <li><b>Factor:</b> 1,5 (50%) y 2 (100%) para extras y traslados con recargo; 1 para el resto o si el colaborador no aplica horas extra.</li>
                        <li><b>Salario:</b> el vigente más reciente en Horas Extras, aplicado a todo el período. Quien no tiene salario se valora en $0.</li>
                    </ul>
                </div></div>
            </div>
        </div>
    </div>
</asp:Content>
```

- [ ] **Paso 2: El código de la página** `ReporteTareas/Formulario/TableroCostos.aspx.cs` (UTF-8 con BOM)

```csharp
using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Tablero de Costos. No pasa ningun identificador a la pagina: el handler
    /// toma la identidad y el perfil de la sesion, siempre.
    ///
    /// El handler ya rechaza a quien no sea perfil 14 o 18. Este redirect evita
    /// que alguien que escriba la URL a mano vea la pantalla vacia cargarse y
    /// reciba el error recien al pedir los datos. La lista de perfiles es la del
    /// handler, no una copia.
    /// </summary>
    public partial class TableroCostos : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            int idPerfil;
            bool tienePermiso = int.TryParse(Convert.ToString(Session["Id_Perfil"]), out idPerfil)
                                && Array.IndexOf(
                                       JsonJQueryNetTableroCostos.AdministrarTableroCostos.PerfilesAutorizados,
                                       idPerfil) >= 0;

            if (!tienePermiso)
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }
    }
}
```

`ReporteTareas/Formulario/TableroCostos.aspx.designer.cs` (UTF-8 con BOM):

```csharp
namespace ReporteTareas.Formulario
{
    public partial class TableroCostos
    {
        /* Sin controles de servidor: la pagina se dibuja entera desde js/tableroCostos.js. */
    }
}
```

- [ ] **Paso 3: El JavaScript** `ReporteTareas/js/tableroCostos.js` (UTF-8 **con BOM**)

```javascript
/* ==========================================================================
   Tablero de Costos — TableroCostos.aspx
   Handler : AdministrarTableroCostos.ashx  (accion ConsultarTablero)
             DescargarTableroCostos.ashx    (Excel)
   Este archivo NO calcula nada: clasificacion, valoracion, filtros, totales y
   avisos llegan resueltos de NegTableroCostos. Solo dibuja.
   ========================================================================== */

var tcDatos = null;
var tcRango = '';
var tcGraficos = {};
var tcPagina = 0;
var TC_POR_PAGINA = 50;

/* Mismo orden que el Resumen y que las propiedades H* / V* de EntTcPeriodo. */
var TC_CATEGORIAS = ['Normal', 'Extra 50%', 'Extra 100%', 'Traslado sin recargo', 'Traslado 50%', 'Traslado 100%'];
var TC_SUFIJOS = ['Normal', 'Extra50', 'Extra100', 'TrasladoSin', 'Traslado50', 'Traslado100'];
var TC_COLORES = ['#2F6DB5', '#E07B24', '#B85A12', '#1F9E89', '#4CC3AE', '#0F6B5C'];
var TC_COLOR_TIPO = { Normal: '#2F6DB5', Extra: '#E07B24', Traslado: '#1F9E89' };

/* Posiciones de cada fila de tcDatos.Detalle (AdministrarTableroCostos.ParaPantalla). */
var D_INICIO = 1, D_RESPONSABLE = 2, D_EMPRESA = 3, D_CONTRATO = 4, D_TICKET = 5, D_TAREA = 6,
    D_ESTADO = 7, D_TIPO = 8, D_RECARGO = 9, D_ESTADOHE = 10, D_HORAS = 11, D_VALOR = 12;

$(document).ready(function () {
    var hoy = new Date();
    $('#txtDesde').val(hoy.getFullYear() + '-01-01');
    $('#txtHasta').val(FechaIso(hoy));

    $('#cboMes, #cboResponsable, #cboEmpresa, #cboContrato, #cboRecargo, #cboEstadoHE')
        .on('change', function () { Consultar(false); });
    $('#btnConsultar').on('click', function () { Consultar(true); });
    $('#btnExcel').on('click', Descargar);
    $('#btnAnterior').on('click', function () { CambiarPagina(-1); });
    $('#btnSiguiente').on('click', function () { CambiarPagina(1); });

    Consultar(true);
});

function FechaIso(d) {
    var m = d.getMonth() + 1, dia = d.getDate();
    return d.getFullYear() + '-' + (m < 10 ? '0' : '') + m + '-' + (dia < 10 ? '0' : '') + dia;
}

function Parametros() {
    return {
        desde: $('#txtDesde').val() || '',
        hasta: $('#txtHasta').val() || '',
        mes: $('#cboMes').val() || 'Todos',
        responsable: $('#cboResponsable').val() || 'Todos',
        empresa: $('#cboEmpresa').val() || 'Todos',
        contrato: $('#cboContrato').val() || 'Todos',
        recargo: $('#cboRecargo').val() || 'Todos',
        estadoHE: $('#cboEstadoHE').val() || 'Todos'
    };
}

function Mostrar(selector, texto) {
    $(selector).text(texto || '').toggle(!!texto);
}

function Consultar(refrescar) {
    /* Si no se publico chart.umd.js, decirlo aqui y no en la consola. */
    if (typeof Chart === 'undefined') {
        Mostrar('#tcError', 'No se pudo cargar la librería de gráficos. Avise a Sistemas: falta publicar js/chart.umd.js.');
        return;
    }

    /* Un rango nuevo puede no tener el mes, la persona o la empresa elegidos. */
    var rango = $('#txtDesde').val() + '|' + $('#txtHasta').val();
    if (refrescar && rango !== tcRango) {
        $('#cboMes, #cboResponsable, #cboEmpresa, #cboContrato').val('Todos');
    }

    var p = Parametros();
    p.refrescar = refrescar ? '1' : '0';
    $('#btnConsultar').prop('disabled', true).text('Consultando…');

    $.ajax({
        type: 'POST',
        url: 'AdministrarTableroCostos.ashx',
        data: JSON.stringify([{ action: 'ConsultarTablero', parameters: p }]),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (r) {
            if (!r || r.estado !== '1') {
                Mostrar('#tcError', (r && r.mensaje) || 'No se pudo cargar el tablero.');
                return;
            }
            Mostrar('#tcError', '');
            tcDatos = r.resultado;
            tcRango = rango;
            tcPagina = 0;
            Dibujar();
        },
        error: function (x) {
            var r = null;
            try { r = JSON.parse(x.responseText); } catch (e) { r = null; }
            Mostrar('#tcError', (r && r.mensaje) || 'No se pudo comunicar con el servidor.');
        },
        complete: function () {
            $('#btnConsultar').prop('disabled', false).text('Consultar');
        }
    });
}

/* ------------------------------------------------------------ formato */

function Num(n, decimales) {
    var partes = (Number(n) || 0).toFixed(decimales).split('.');
    partes[0] = partes[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    return partes.length > 1 ? partes[0] + ',' + partes[1] : partes[0];
}

function Usd(n) { return '$' + Num(n, 2); }

function Pct(n) { return Num((Number(n) || 0) * 100, 1) + ' %'; }

function Html(s) {
    return $('<div>').text(s == null ? '' : String(s)).html();
}

/* ------------------------------------------------------------ dibujo */

function Dibujar() {
    LlenarCombo('#cboMes', tcDatos.Meses);
    LlenarCombo('#cboResponsable', tcDatos.Responsables);
    LlenarCombo('#cboEmpresa', tcDatos.Empresas);
    LlenarCombo('#cboContrato', tcDatos.Contratos);

    Mostrar('#tcAviso', tcDatos.Aviso);
    Mostrar('#tcVacio', tcDatos.Indicadores.Total.Actividades === 0
        ? 'No hay actividades para el rango y los filtros elegidos.' : '');
    Mostrar('#tcParametros', tcDatos.ParametrosPorDefecto
        ? 'Faltan parámetros vigentes de Horas Extras (Factor50, Factor100 o DiasMes): se usan 1,5 / 2 / 30, como el Excel.' : '');
    Mostrar('#tcSinSalario', tcDatos.SinSalario.length
        ? 'Sin salario en Horas Extras (sus horas se valoran en $0; complételos en Horas Extras): ' + tcDatos.SinSalario.join(', ') + '.'
        : '');

    DibujarIndicadores();
    DibujarTablas();
    DibujarGraficos();
    DibujarDetalle();
}

/* Conserva lo elegido. El servidor ya agrega el valor elegido si no tiene datos. */
function LlenarCombo(id, valores) {
    var combo = $(id), actual = combo.val() || 'Todos';
    combo.empty().append($('<option>').val('Todos').text('Todos'));
    $.each(valores || [], function (i, v) { combo.append($('<option>').val(v).text(v)); });
    combo.val(actual);
    if (combo.val() === null) { combo.val('Todos'); }
}

function DibujarIndicadores() {
    $.each(['Normal', 'Extra', 'Traslado', 'Total'], function (i, k) {
        var c = tcDatos.Indicadores[k];
        $('#ind' + k + 'Horas').text(Num(c.Horas, 2) + ' h');
        $('#ind' + k + 'Valor').text(Usd(c.Valor));
        $('#ind' + k + 'Act').text(Num(c.Actividades, 0) + ' actividades');
    });
}

/* filas: [{ negrita: bool, celdas: [html...] }]. La primera celda ya viene escapada. */
function Tabla(id, cabeceras, filas) {
    var html = '<thead><tr>';
    $.each(cabeceras, function (i, c) { html += '<th>' + Html(c) + '</th>'; });
    html += '</tr></thead><tbody>';
    $.each(filas, function (i, f) {
        html += '<tr' + (f.negrita ? ' class="tc-subtotal"' : '') + '>';
        $.each(f.celdas, function (j, c) { html += '<td' + (j > 0 ? ' class="text-right"' : '') + '>' + c + '</td>'; });
        html += '</tr>';
    });
    $(id).html(html + '</tbody>');
}

function DibujarTablas() {
    Tabla('#tblResumen', ['Tipo / Recargo', 'Horas', '% horas', 'Valor $', '% valor', 'Actividades', '$ por hora'],
        $.map(tcDatos.Resumen, function (r) {
            return { negrita: r.EsSubtotal, celdas: [Html(r.Concepto), Num(r.Horas, 2), Pct(r.PctHoras), Usd(r.Valor), Pct(r.PctValor), Num(r.Actividades, 0), Usd(r.ValorPorHora)] };
        }));

    Tabla('#tblExtras', ['Estado HE', 'Horas 50%', 'Horas 100%', '$ 50%', '$ 100%', 'Total $'],
        $.map(tcDatos.ExtrasPorEstado, function (e) {
            return { negrita: e.Estado === 'Total', celdas: [Html(e.Estado), Num(e.Horas50, 2), Num(e.Horas100, 2), Usd(e.Valor50), Usd(e.Valor100), Usd(e.Total)] };
        }));

    Tabla('#tblTraslados', ['Estado HE', 'Horas sin recargo', 'Horas 50%', 'Horas 100%', 'Total horas', 'Total $'],
        $.map(tcDatos.TrasladosPorEstado, function (e) {
            return { negrita: e.Estado === 'Total', celdas: [Html(e.Estado), Num(e.HorasSinRecargo, 2), Num(e.Horas50, 2), Num(e.Horas100, 2), Num(e.TotalHoras, 2), Usd(e.TotalValor)] };
        }));

    var cabPeriodo = function (primera) {
        var c = [primera];
        $.each(TC_CATEGORIAS, function (i, k) { c.push('Horas ' + k); });
        c.push('Total horas');
        $.each(TC_CATEGORIAS, function (i, k) { c.push('$ ' + k); });
        c.push('Total $');
        return c;
    };
    Tabla('#tblAnio', cabPeriodo('Año'), FilasPeriodo(tcDatos.PorAnio));
    Tabla('#tblMes', cabPeriodo('Mes'), FilasPeriodo(tcDatos.PorMes));

    Tabla('#tblTopNormal', ['Responsable', 'Horas', 'Valor $'],
        $.map(tcDatos.TopNormal, function (t) { return { celdas: [Html(t.Responsable), Num(t.Horas, 0), Usd(t.Valor)] }; }));
    Tabla('#tblTopExtra', ['Responsable', 'Horas 50%', 'Horas 100%', 'Valor $'],
        $.map(tcDatos.TopExtra, function (t) { return { celdas: [Html(t.Responsable), Num(t.Horas50, 0), Num(t.Horas100, 0), Usd(t.Valor)] }; }));
    Tabla('#tblTopTraslado', ['Responsable', 'Sin recargo', '50%', '100%', 'Valor $'],
        $.map(tcDatos.TopTraslado, function (t) { return { celdas: [Html(t.Responsable), Num(t.HorasSinRecargo, 0), Num(t.Horas50, 0), Num(t.Horas100, 0), Usd(t.Valor)] }; }));
}

function FilasPeriodo(lista) {
    return $.map(lista, function (p) {
        var celdas = [Html(p.Periodo)];
        $.each(TC_SUFIJOS, function (i, s) { celdas.push(Num(p['H' + s], 2)); });
        celdas.push(Num(p.HTotal, 2));
        $.each(TC_SUFIJOS, function (i, s) { celdas.push(Usd(p['V' + s])); });
        celdas.push(Usd(p.VTotal));
        return { negrita: p.Periodo === 'Total', celdas: celdas };
    });
}

/* ---------------------------------------------------------- graficos */

function Grafico(id, config) {
    if (tcGraficos[id]) { tcGraficos[id].destroy(); }
    tcGraficos[id] = new Chart(document.getElementById(id), config);
}

function Categorias() {
    return $.grep(tcDatos.Resumen, function (r) { return TC_CATEGORIAS.indexOf(r.Concepto) >= 0; });
}

function Dona(id, campo, formato) {
    var filas = Categorias();
    Grafico(id, {
        type: 'doughnut',
        data: {
            labels: $.map(filas, function (r) { return r.Concepto; }),
            datasets: [{ data: $.map(filas, function (r) { return r[campo]; }), backgroundColor: TC_COLORES }]
        },
        options: {
            maintainAspectRatio: false,
            plugins: {
                legend: { position: 'right' },
                tooltip: { callbacks: { label: function (c) { return c.label + ': ' + formato(c.parsed); } } }
            }
        }
    });
}

function BarrasMes(id, prefijo, formato) {
    var meses = $.grep(tcDatos.PorMes, function (p) { return p.Periodo !== 'Total'; });
    var series = $.map(TC_SUFIJOS, function (s, i) {
        return {
            label: TC_CATEGORIAS[i],
            backgroundColor: TC_COLORES[i],
            data: $.map(meses, function (p) { return p[prefijo + s]; })
        };
    });
    Grafico(id, {
        type: 'bar',
        data: { labels: $.map(meses, function (p) { return p.Periodo; }), datasets: series },
        options: {
            maintainAspectRatio: false,
            scales: { x: { stacked: true }, y: { stacked: true, ticks: { callback: function (v) { return formato(v); } } } },
            plugins: { tooltip: { callbacks: { label: function (c) { return c.dataset.label + ': ' + formato(c.parsed.y); } } } }
        }
    });
}

function BarrasTop(id, lista, color) {
    Grafico(id, {
        type: 'bar',
        data: {
            labels: $.map(lista, function (t) { return t.Responsable; }),
            datasets: [{ label: 'Valor $', backgroundColor: color, data: $.map(lista, function (t) { return t.Valor; }) }]
        },
        options: {
            indexAxis: 'y',
            maintainAspectRatio: false,
            plugins: { legend: { display: false }, tooltip: { callbacks: { label: function (c) { return Usd(c.parsed.x); } } } }
        }
    });
}

function DibujarGraficos() {
    Dona('grfDonaHoras', 'Horas', function (v) { return Num(v, 2) + ' h'; });
    Dona('grfDonaValor', 'Valor', Usd);
    BarrasMes('grfMesHoras', 'H', function (v) { return Num(v, 0); });
    BarrasMes('grfMesValor', 'V', function (v) { return '$' + Num(v, 0); });
    BarrasTop('grfTopNormal', tcDatos.TopNormal, TC_COLOR_TIPO.Normal);
    BarrasTop('grfTopExtra', tcDatos.TopExtra, TC_COLOR_TIPO.Extra);
    BarrasTop('grfTopTraslado', tcDatos.TopTraslado, TC_COLOR_TIPO.Traslado);
}

/* ----------------------------------------------------------- detalle */

function DibujarDetalle() {
    var filas = tcDatos.Detalle, total = filas.length;
    var ini = tcPagina * TC_POR_PAGINA, fin = Math.min(ini + TC_POR_PAGINA, total);
    var html = '';
    for (var i = ini; i < fin; i++) {
        var f = filas[i];
        html += '<tr><td>' + Html(f[D_INICIO]) + '</td><td>' + Html(f[D_RESPONSABLE]) + '</td><td>' + Html(f[D_EMPRESA])
              + '</td><td>' + Html(f[D_CONTRATO]) + '</td><td>' + Html(f[D_TICKET]) + '</td><td>' + Html(f[D_TAREA])
              + '</td><td>' + Html(f[D_ESTADO]) + '</td><td>' + Html(f[D_TIPO]) + '</td><td>' + Html(f[D_RECARGO])
              + '</td><td>' + Html(f[D_ESTADOHE]) + '</td><td class="text-right">' + Num(f[D_HORAS], 2)
              + '</td><td class="text-right">' + Usd(f[D_VALOR]) + '</td></tr>';
    }
    $('#tblDetalle tbody').html(html);
    $('#lblPagina').text(total === 0 ? 'Sin actividades'
        : 'Mostrando ' + Num(ini + 1, 0) + '–' + Num(fin, 0) + ' de ' + Num(total, 0));
    $('#btnAnterior').prop('disabled', tcPagina === 0);
    $('#btnSiguiente').prop('disabled', fin >= total);
}

function CambiarPagina(delta) {
    if (!tcDatos) { return; }
    var ultima = Math.max(0, Math.ceil(tcDatos.Detalle.length / TC_POR_PAGINA) - 1);
    tcPagina = Math.min(ultima, Math.max(0, tcPagina + delta));
    DibujarDetalle();
}

function Descargar() {
    window.location = 'DescargarTableroCostos.ashx?' + $.param(Parametros());
}
```

- [ ] **Paso 4: Registrar en `ReporteTareas/ReporteTareas.csproj`**

Junto a `<Content Include="Formulario\DescargarHorasExtras.ashx" />`:

```xml
    <Content Include="Formulario\TableroCostos.aspx" />
```

Junto a `<Content Include="js\dashboardAprobacion.js" />`:

```xml
    <Content Include="js\tableroCostos.js" />
```

Junto al bloque `<Compile Include="Formulario\HorasExtras.aspx.designer.cs">`:

```xml
    <Compile Include="Formulario\TableroCostos.aspx.cs">
      <DependentUpon>TableroCostos.aspx</DependentUpon>
      <SubType>ASPXCodeBehind</SubType>
    </Compile>
    <Compile Include="Formulario\TableroCostos.aspx.designer.cs">
      <DependentUpon>TableroCostos.aspx</DependentUpon>
    </Compile>
```

Confirmar que `<Content Include="js\chart.umd.js" />` sigue en el `.csproj`. Ya está, en la línea 1156.

- [ ] **Paso 5: Comprobar la codificación**

Run (Git Bash): `for f in ReporteTareas/Formulario/TableroCostos.aspx ReporteTareas/js/tableroCostos.js ReporteTareas/Formulario/AdministrarTableroCostos.ashx.cs CapaNegocio/NegTableroCostos.Armar.cs; do head -c3 "$f" | od -An -tx1; done`
Expected: `ef bb bf` en cada uno.

- [ ] **Paso 6: Compilar y correr las pruebas**

Expected: compila y todas las pruebas pasan.

- [ ] **Paso 7: Probar en el navegador** con IIS Express de **32 bits** (`C:\Program Files (x86)\IIS Express\iisexpress.exe`; el de 64 bits no levanta el sitio). Iniciar sesión con un usuario de perfil 14 o 18 y abrir *Nómina → Tablero de Costos*. Comprobar:
1. **Carga:** las 4 tarjetas, las tablas y los 7 gráficos se dibujan sin errores en la consola.
2. **Filtros:** Responsable cambia todo menos el top 15. Mes cambia todo menos la tabla por mes. Recargo «50%» no cambia la tarjeta de Horas Normales.
3. **Listas dependientes:** con una Empresa elegida, Responsable solo ofrece personas de esa empresa.
4. **Aviso:** elegir un Responsable y después una Empresa donde esa persona no trabajó. Aparece el aviso amarillo y los dos combos conservan lo elegido.
5. **Detalle:** Anterior / Siguiente recorren las páginas, y «Mostrando x–y de N» coincide con el total de actividades.
6. **Excel:** descarga `TableroCostos_<desde>_<hasta>.xlsx`, con las hojas Resumen y Detalle. En la base aparece una fila nueva en `TC_Auditoria`.
7. **Tildes:** «Movilización», «Período», «Año» y los nombres con Ñ se ven bien en la pantalla y en el Excel.
8. **Permisos:** con un usuario de otro perfil, la opción no aparece en el menú, la URL directa redirige a Principal y `DescargarTableroCostos.ashx?desde=2026-01-01&hasta=2026-01-31` responde 403.

- [ ] **Paso 8: Prueba de cuadre contra el Excel.** Es manual y no se versiona.
1. **Pedir al usuario** que regenere el Excel **el mismo día**, en `C:\ReporteExcel`, con `python generar_dashboard.py 2025 2026`. El archivo del 04-10 ya no sirve para comparar, porque desde entonces se registraron actividades nuevas. No correr el script sin su permiso: sobrescribe su archivo.
2. En el tablero: Desde `2025-01-01`, Hasta `2026-12-31`, sin filtros.
3. Comparar las 4 tarjetas y las 11 filas del resumen contra las filas 6–8 y 13–23 de la hoja Dashboard del Excel nuevo. Las **actividades y las horas** deben coincidir exactamente. El **valor** puede diferir solo en personas con varias cuentas (desviación 1 del plan); para encontrarlas, comparar el top 15 y la hoja Tarifas del Excel, persona por persona. Cualquier otra diferencia es un defecto: hay que buscarla antes de seguir.
4. Anotar el resultado en el mensaje del commit. **Sin montos ni nombres**: el repositorio es público.

- [ ] **Paso 9: Commit**

```bash
git add ReporteTareas/Formulario/TableroCostos.aspx ReporteTareas/Formulario/TableroCostos.aspx.cs ReporteTareas/Formulario/TableroCostos.aspx.designer.cs ReporteTareas/js/tableroCostos.js ReporteTareas/ReporteTareas.csproj
git commit -m "feat(tablero-costos): pantalla del tablero con graficos y detalle"
```

---

### Task 8: Despliegue

**Archivos:**
- Modificar: `DESPLIEGUE.md`. Agregar una sección **16** al final; la última hoy es la 15.

- [ ] **Paso 1: Agregar la sección**

```markdown
## 16. Tablero de Costos (Nómina, perfiles 14 y 18)

Lleva a la aplicación el `Dashboard_Tareas_Aranda.xlsx` que se generaba a mano con
`C:\ReporteExcel\generar_dashboard.py`. Spec: `docs/superpowers/specs/2026-10-12-tablero-costos-design.md`.

### 16.1 Base de datos

`docs/sql/2026-10-12-tablero-costos.sql` crea:
- el procedimiento de lectura `Sp_RTA_TableroCostos`;
- la tabla `TC_Auditoria` y el procedimiento `Sp_RTA_TableroCostosAuditar`;
- la hoja de menú *Nómina → Tablero de Costos*, visible **solo** para los perfiles 14 y 18.

Es repetible y **no escribe en `HE_*` ni en `R_DetTareasAranda`**. Va **antes** de los binarios. La última línea debe decir `VEREDICTO: OK`.

Para verificar, correr `docs/sql/2026-10-12-tablero-costos-verificacion.sql`. Es de solo lectura; la prueba de auditoría se revierte.

> **Si se agrega un perfil al tablero:** hay que cambiarlo en **dos** lugares:
> `AdministrarTableroCostos.PerfilesAutorizados` (código) y `PerfilMenu` (base).
> Con solo uno, la persona ve la opción y recibe «No tiene permisos», o tiene permiso y no ve la opción.

### 16.2 Archivos a publicar

Todos juntos. Las capas son `.exe` y van con el `.dll` de la web; publicarlos por separado ya rompió los correos el 18-09.

- `bin\ReporteTareas.dll`, `bin\CapaEntidad.exe`, `bin\CapaNegocio.exe`, `bin\CapaDato.exe`
- `Formulario\TableroCostos.aspx`
- `Formulario\AdministrarTableroCostos.ashx`
- `Formulario\DescargarTableroCostos.ashx`
- `js\tableroCostos.js`
- `js\chart.umd.js`, si el servidor todavía no lo tiene. Lo usa también el dashboard de jefatura.

### 16.3 Verificar después de publicar

1. Con un usuario de perfil 14 o 18, abrir el tablero con Desde `2025-01-01` y Hasta `2026-12-31`: el total de actividades coincide con el paso 2 de la verificación SQL.
2. Descargar el Excel y comprobar que hay una fila nueva en `TC_Auditoria`.
3. Con un usuario de otro perfil, la opción no aparece y la URL directa redirige a Principal.
4. Si alguien no ve cambios del `.js` después de un redespliegue, subir el `?v=` en `TableroCostos.aspx`.
```

- [ ] **Paso 2: Verificación final**

Run (PowerShell):
```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
git status --short
```
Expected:
- compila;
- todas las pruebas pasan;
- `git status` no muestra archivos nuevos de este módulo sin commitear.

Comprobar además que cada archivo nuevo aparece en su `.csproj` con `git grep -n "TableroCostos" -- "*.csproj"`. Deben aparecer:
- `CapaEntidad.csproj`: `EntTableroCostos.cs`.
- `CapaDato.csproj`: `DaoTableroCostos.cs`.
- `CapaNegocio.csproj`: `NegTableroCostos.cs`, `NegTableroCostos.Armar.cs`, `NegTableroCostos.Datos.cs`, `NegTableroCostosExportacion.cs`.
- `CapaPruebas.csproj`: `NegTableroCostosTests.cs`, `NegTableroCostosExportacionTests.cs`.
- `ReporteTareas.csproj`:
  - `Content` de `TableroCostos.aspx`, `AdministrarTableroCostos.ashx`, `DescargarTableroCostos.ashx` y `js\tableroCostos.js`;
  - `Compile` de los dos `.ashx.cs`, de `TableroCostos.aspx.cs` y de `TableroCostos.aspx.designer.cs`.

- [ ] **Paso 3: Commit**

```bash
git add DESPLIEGUE.md
git commit -m "docs(tablero-costos): despliegue del modulo"
```

- [ ] **Paso 4: Paquete de despliegue.** Va **al final**, después del último commit de código; un paquete regenerado antes queda viejo. Publicar con el perfil `FolderProfile.pubxml`. Comprobar que el paquete es el nuevo **buscando algo de este módulo dentro de él**, no solo que los archivos existan:

```powershell
$neg = [System.Reflection.Assembly]::LoadFrom((Resolve-Path "ReporteTareas\obj\Release\Package\PackageTmp\bin\CapaNegocio.exe"))
$null -ne $neg.GetType('CapaNegocio.NegTableroCostos')
Test-Path "ReporteTareas\obj\Release\Package\PackageTmp\js\tableroCostos.js"
```
Expected: `True` y `True`. Commit del paquete regenerado, como en los módulos anteriores.
