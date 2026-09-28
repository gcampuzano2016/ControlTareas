/* ============================================================================
   Calificacion de oportunidades del Forecast
   ReporTarea  |  2026-09-28

   PENDIENTE DE EJECUTAR.

   Crea:
     1. dbo.ForeCastCalificacion           (respuestas del vendedor, con historial)
     2. dbo.Sp_RTAListaCalificacionForeCast (solo lectura: datos + tasas de exito)
     3. dbo.Sp_RTAInsertaCalificacionForeCast
     4. La opcion de menu, con los mismos perfiles que ven RegistroForeCast.aspx

   La visibilidad NO se reescribe: el SP de lista hace INSERT ... EXEC de
   Sp_RTAListaForeCast y se queda solo con IdForeCast. Esa regla (sucursales
   por usuario escritas en el SP) vive en un unico lugar.

   ATENCION: #Visibles replica las 40 columnas de Sp_RTAListaForeCast. Si
   alguien le agrega una columna a ese SP, este falla con "Column name or
   number of supplied values does not match table definition". Agregar la
   columna aqui tambien.

   ForeCast NO se modifica. Idempotente: la tabla se crea si no existe; los
   SP se reemplazan (DROP y CREATE); el menu no se duplica.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('dbo.Sp_RTAListaForeCast','P') IS NULL
   OR OBJECT_ID('dbo.ForeCastHistorico','U') IS NULL
   OR OBJECT_ID('dbo.PrmPorcentaje','U') IS NULL
BEGIN
    RAISERROR('Falta Sp_RTAListaForeCast, ForeCastHistorico o PrmPorcentaje. Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO

/* 1. Tabla ------------------------------------------------------------------ */
IF OBJECT_ID('dbo.ForeCastCalificacion','U') IS NULL
BEGIN
    CREATE TABLE dbo.ForeCastCalificacion
    (
        IdCalificacion     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ForeCastCalificacion PRIMARY KEY,
        IdForeCast         BIGINT       NOT NULL,
        Presupuesto        TINYINT      NOT NULL,
        Autoridad          TINYINT      NOT NULL,
        Necesidad          TINYINT      NOT NULL,
        DecisorCargo       VARCHAR(150) NULL,
        PropuestaEnviada   BIT          NOT NULL CONSTRAINT DF_ForeCastCalificacion_Propuesta DEFAULT (0),
        AccionComprometida VARCHAR(300) NULL,
        FechaAccion        DATE         NULL,
        Usuario            VARCHAR(50)  NOT NULL,
        FechaRegistro      DATETIME     NOT NULL CONSTRAINT DF_ForeCastCalificacion_Fecha DEFAULT (GETDATE()),
        CONSTRAINT CK_ForeCastCalificacion_Presupuesto CHECK (Presupuesto IN (0,5,12,20)),
        CONSTRAINT CK_ForeCastCalificacion_Autoridad   CHECK (Autoridad   IN (0,5,12,20)),
        CONSTRAINT CK_ForeCastCalificacion_Necesidad   CHECK (Necesidad   IN (0,6,14,20))
    );

    CREATE INDEX IX_ForeCastCalificacion_IdForeCast
        ON dbo.ForeCastCalificacion (IdForeCast, FechaRegistro DESC, IdCalificacion DESC);

    PRINT 'ForeCastCalificacion creada.';
END
ELSE
    PRINT 'ForeCastCalificacion ya existia.';
GO

/* 2. Lista ------------------------------------------------------------------ */
IF OBJECT_ID('dbo.Sp_RTAListaCalificacionForeCast','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTAListaCalificacionForeCast;
GO

CREATE PROCEDURE dbo.Sp_RTAListaCalificacionForeCast
    @Idusuario VARCHAR(10) = ''
AS
BEGIN
    SET NOCOUNT ON;

    /* Las 40 columnas de Sp_RTAListaForeCast. Solo se usa IdForeCast; el
       resto va como NVARCHAR(MAX) para que ninguna conversion de tipo falle. */
    CREATE TABLE #Visibles
    (
        IdForeCast BIGINT,
        c02 NVARCHAR(MAX), c03 NVARCHAR(MAX), c04 NVARCHAR(MAX), c05 NVARCHAR(MAX),
        c06 NVARCHAR(MAX), c07 NVARCHAR(MAX), c08 NVARCHAR(MAX), c09 NVARCHAR(MAX),
        c10 NVARCHAR(MAX), c11 NVARCHAR(MAX), c12 NVARCHAR(MAX), c13 NVARCHAR(MAX),
        c14 NVARCHAR(MAX), c15 NVARCHAR(MAX), c16 NVARCHAR(MAX), c17 NVARCHAR(MAX),
        c18 NVARCHAR(MAX), c19 NVARCHAR(MAX), c20 NVARCHAR(MAX), c21 NVARCHAR(MAX),
        c22 NVARCHAR(MAX), c23 NVARCHAR(MAX), c24 NVARCHAR(MAX), c25 NVARCHAR(MAX),
        c26 NVARCHAR(MAX), c27 NVARCHAR(MAX), c28 NVARCHAR(MAX), c29 NVARCHAR(MAX),
        c30 NVARCHAR(MAX), c31 NVARCHAR(MAX), c32 NVARCHAR(MAX), c33 NVARCHAR(MAX),
        c34 NVARCHAR(MAX), c35 NVARCHAR(MAX), c36 NVARCHAR(MAX), c37 NVARCHAR(MAX),
        c38 NVARCHAR(MAX), c39 NVARCHAR(MAX), c40 NVARCHAR(MAX)
    );

    INSERT INTO #Visibles
    EXEC dbo.Sp_RTAListaForeCast @Idusuario = @Idusuario;

    /* Resultado 1: oportunidades abiertas y visibles */
    SELECT
        f.IdForeCast,
        f.IdCliente,
        LTRIM(RTRIM(f.Cliente))            AS Cliente,
        LTRIM(RTRIM(f.DetalleProyecto))    AS DetalleProyecto,
        f.Marca,
        f.Prioridad,
        LTRIM(RTRIM(f.GerentedeCuenta))    AS GerentedeCuenta,
        LTRIM(RTRIM(f.Sucursal))           AS Sucursal,
        LTRIM(RTRIM(f.SegmentodeMercado))  AS SegmentodeMercado,
        LTRIM(RTRIM(f.TipoEmpresa))        AS TipoEmpresa,
        f.PVPEstimado,
        f.UtilidadBrutaEstimada,
        f.IdNegocio,
        TRY_CONVERT(DECIMAL(5,2), NULLIF(LTRIM(RTRIM(p.Porcentaje)), '')) AS Porcentaje,
        p.Descripcion                      AS DescripcionNegocio,
        f.FechaFacturacionProyecto         AS FechaCierre,
        f.FechaCobro                       AS FechaModificacion,
        f.Contacto,
        f.LiderProyectoCliente,
        f.Observaciones,
        (SELECT COUNT(DISTINCT h.FechaFacturacionProyecto)
           FROM dbo.ForeCastHistorico h
          WHERE h.IdForeCast = f.IdForeCast)                        AS FechasCierreDistintas,
        (SELECT COUNT(*)
           FROM dbo.ForeCast g
          WHERE g.IdCliente = f.IdCliente
            AND g.Estado = 1
            AND g.Prioridad = 'GANADO')                              AS GanadosCliente,
        (SELECT COUNT(DISTINCT x.Nombre)
           FROM (SELECT NULLIF(LTRIM(RTRIM(c.Contacto)), '') AS Nombre
                   FROM dbo.ForeCast c WHERE c.IdCliente = f.IdCliente
                 UNION ALL
                 SELECT NULLIF(LTRIM(RTRIM(c.LiderProyectoCliente)), '')
                   FROM dbo.ForeCast c WHERE c.IdCliente = f.IdCliente) x) AS ContactosCuenta,
        k.Presupuesto,
        k.Autoridad,
        k.Necesidad,
        k.DecisorCargo,
        k.PropuestaEnviada,
        k.AccionComprometida,
        k.FechaAccion,
        k.Usuario                          AS UsuarioCalificacion,
        k.FechaRegistro                    AS FechaCalificacion
    FROM dbo.ForeCast f
    INNER JOIN (SELECT DISTINCT IdForeCast FROM #Visibles) v ON v.IdForeCast = f.IdForeCast
    LEFT JOIN dbo.PrmPorcentaje p ON p.IdNegocio = f.IdNegocio
    OUTER APPLY (SELECT TOP 1 q.*
                   FROM dbo.ForeCastCalificacion q
                  WHERE q.IdForeCast = f.IdForeCast
                  ORDER BY q.FechaRegistro DESC, q.IdCalificacion DESC) k
    WHERE f.Prioridad IN ('LEAD','PIPELINE','UPSIDE','FORECAST');

    /* Resultado 2: tasas de exito de los ultimos 36 meses (FechaCobro es la
       ultima modificacion; en un cierre coincide con la fecha del cierre). */
    SELECT 'SEGMENTO' AS Dimension,
           LTRIM(RTRIM(SegmentodeMercado)) AS Valor,
           SUM(CASE WHEN Prioridad = 'GANADO'  THEN 1 ELSE 0 END) AS Ganados,
           SUM(CASE WHEN Prioridad = 'PERDIDO' THEN 1 ELSE 0 END) AS Perdidos
      FROM dbo.ForeCast
     WHERE Estado = 1
       AND Prioridad IN ('GANADO','PERDIDO')
       AND FechaCobro >= DATEADD(MONTH, -36, GETDATE())
       AND ISNULL(LTRIM(RTRIM(SegmentodeMercado)), '') <> ''
     GROUP BY LTRIM(RTRIM(SegmentodeMercado))
    UNION ALL
    SELECT 'TIPO',
           LTRIM(RTRIM(TipoEmpresa)),
           SUM(CASE WHEN Prioridad = 'GANADO'  THEN 1 ELSE 0 END),
           SUM(CASE WHEN Prioridad = 'PERDIDO' THEN 1 ELSE 0 END)
      FROM dbo.ForeCast
     WHERE Estado = 1
       AND Prioridad IN ('GANADO','PERDIDO')
       AND FechaCobro >= DATEADD(MONTH, -36, GETDATE())
       AND ISNULL(LTRIM(RTRIM(TipoEmpresa)), '') <> ''
     GROUP BY LTRIM(RTRIM(TipoEmpresa));
END
GO

/* 3. Insertar ---------------------------------------------------------------- */
IF OBJECT_ID('dbo.Sp_RTAInsertaCalificacionForeCast','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTAInsertaCalificacionForeCast;
GO

CREATE PROCEDURE dbo.Sp_RTAInsertaCalificacionForeCast
    @IdForeCast         BIGINT,
    @Presupuesto        TINYINT,
    @Autoridad          TINYINT,
    @Necesidad          TINYINT,
    @DecisorCargo       VARCHAR(150) = NULL,
    @PropuestaEnviada   BIT = 0,
    @AccionComprometida VARCHAR(300) = NULL,
    @FechaAccion        DATE = NULL,
    @Usuario            VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.ForeCast
                    WHERE IdForeCast = @IdForeCast
                      AND Estado = 1
                      AND Prioridad IN ('LEAD','PIPELINE','UPSIDE','FORECAST'))
    BEGIN
        SELECT 0 AS Estado, CONVERT(VARCHAR(200), 'La oportunidad no existe o ya no esta abierta.') AS Mensaje;
        RETURN;
    END

    IF @Presupuesto NOT IN (0,5,12,20) OR @Autoridad NOT IN (0,5,12,20) OR @Necesidad NOT IN (0,6,14,20)
    BEGIN
        SELECT 0 AS Estado, CONVERT(VARCHAR(200), 'Puntaje no permitido.') AS Mensaje;
        RETURN;
    END

    INSERT INTO dbo.ForeCastCalificacion
        (IdForeCast, Presupuesto, Autoridad, Necesidad, DecisorCargo, PropuestaEnviada,
         AccionComprometida, FechaAccion, Usuario)
    VALUES
        (@IdForeCast, @Presupuesto, @Autoridad, @Necesidad, NULLIF(LTRIM(RTRIM(@DecisorCargo)), ''),
         ISNULL(@PropuestaEnviada, 0), NULLIF(LTRIM(RTRIM(@AccionComprometida)), ''), @FechaAccion, @Usuario);

    SELECT 1 AS Estado, CONVERT(VARCHAR(200), 'Calificacion guardada.') AS Mensaje;
END
GO

/* 4. Menu -------------------------------------------------------------------- */
BEGIN TRANSACTION;

DECLARE @IdMenuPadre BIGINT       = 39;   -- grupo "Forecacst" (asi esta escrito en MenuDos)
DECLARE @Href        VARCHAR(512) = 'CalificacionForeCast.aspx';
DECLARE @Titulo      VARCHAR(128) = 'Calificación ForeCast';
DECLARE @Descripcion VARCHAR(512) = 'Puntaje, alertas y siguiente accion de las oportunidades abiertas';
DECLARE @Icono       VARCHAR(128) = 'fa fa-star-half-o';
DECLARE @IdMenu      BIGINT;
DECLARE @Orden       INT;

SELECT @IdMenu = Id_Menu FROM dbo.MenuDos WHERE Href = @Href AND Estado_Logico_Registro = 1;

IF @IdMenu IS NULL
BEGIN
    SELECT @Orden = ISNULL(MAX(Orden_Opcion), 0) + 1 FROM dbo.MenuDos WHERE Id_MenuPadre = @IdMenuPadre;

    INSERT INTO dbo.MenuDos
        (Id_MenuPadre, Es_Opcion_de_Menu, Href, Class_Opcion, Class_Icon, Titulo, Descripcion,
         Es_Opcion_Publica, Orden_Opcion, Estado_Registro, Fecha_Creacion, Id_Usuario_Creacion,
         Ip_Creacion, Estado_Logico_Registro)
    VALUES
        (@IdMenuPadre, 0, @Href, NULL, @Icono, @Titulo, @Descripcion,
         0, @Orden, 1, GETDATE(), 'admin', '127.0.0.1', 1);

    SET @IdMenu = CONVERT(BIGINT, SCOPE_IDENTITY());
    PRINT 'MenuDos: opcion creada con Id_Menu = ' + CONVERT(VARCHAR(20), @IdMenu);
END
ELSE
    PRINT 'MenuDos: la opcion ya existia (Id_Menu = ' + CONVERT(VARCHAR(20), @IdMenu) + ').';

/* Mismos perfiles que VEN RegistroForeCast.aspx. En PerfilMenu, Estado = '0'
   MUESTRA la opcion y Estado = '1' la oculta. */
INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
SELECT DISTINCT CONVERT(INT, @IdMenu), pm.IdPerfil, CONVERT(DATE, GETDATE()), '0'
  FROM dbo.PerfilMenu pm
  JOIN dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
 WHERE m.Href = 'RegistroForeCast.aspx'
   AND m.Estado_Logico_Registro = 1
   AND pm.Estado = '0'
   AND NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu x
                    WHERE x.id_Menu = CONVERT(INT, @IdMenu) AND x.IdPerfil = pm.IdPerfil);

COMMIT TRANSACTION;
GO

SET NOEXEC OFF;
GO

/* Verificacion */
SELECT M.Id_Menu, M.Id_MenuPadre, M.Titulo, M.Href, PM.IdPerfil, PM.Estado
  FROM dbo.MenuDos M
  LEFT JOIN dbo.PerfilMenu PM ON PM.id_Menu = M.Id_Menu
 WHERE M.Href = 'CalificacionForeCast.aspx'
 ORDER BY PM.IdPerfil;
GO
