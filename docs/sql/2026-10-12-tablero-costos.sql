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
