/* ============================================================================
   Ventana de aprobación por jefe inmediato — objetos de BD
   Base: ReporTarea
   ============================================================================ */

/* ---------- 1) Tabla ---------- */
IF OBJECT_ID('dbo.RTA_VentanaAprobacionJefe','U') IS NULL
BEGIN
    CREATE TABLE dbo.RTA_VentanaAprobacionJefe
    (
        Id              INT IDENTITY(1,1) NOT NULL,
        MailJefe        VARCHAR(150)      NOT NULL,
        FechaDesde      DATE              NOT NULL,
        FechaHasta      DATE              NOT NULL,
        Estado          BIT               NOT NULL CONSTRAINT DF_RTA_VentAprob_Estado   DEFAULT(1),
        UsuarioRegistro VARCHAR(100)      NULL,
        FechaRegistro   DATETIME          NOT NULL CONSTRAINT DF_RTA_VentAprob_FechaReg DEFAULT(GETDATE()),
        CONSTRAINT PK_RTA_VentanaAprobacionJefe PRIMARY KEY (Id),
        CONSTRAINT UQ_RTA_VentAprob_MailJefe UNIQUE (MailJefe)
    );
END
GO

/* ---------- 2) SP Listar jefes con su ventana ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarJefesVentanaAprobacion','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarJefesVentanaAprobacion;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarJefesVentanaAprobacion
    @Filtro VARCHAR(150) = ''
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Jefes AS (
        SELECT MailJefe = LTRIM(RTRIM(MailCodJefeInm)),
               NumColaboradores = COUNT(*)
        FROM dbo.R_Usuarios
        WHERE ISNULL(MailCodJefeInm,'') <> ''
        GROUP BY LTRIM(RTRIM(MailCodJefeInm))
    )
    SELECT
        j.MailJefe,
        NombreJefe        = ISNULL(u.Nom_Usuario, j.MailJefe),
        j.NumColaboradores,
        FechaDesde        = CONVERT(VARCHAR(10), v.FechaDesde, 23),
        FechaHasta        = CONVERT(VARCHAR(10), v.FechaHasta, 23),
        TieneVentana      = CASE WHEN v.Id IS NOT NULL THEN 1 ELSE 0 END
    FROM Jefes j
    OUTER APPLY (
        SELECT TOP 1 ru.Nom_Usuario
        FROM dbo.R_Usuarios ru
        WHERE ru.E_Mail = j.MailJefe
    ) u
    LEFT JOIN dbo.RTA_VentanaAprobacionJefe v
        ON v.MailJefe = j.MailJefe AND v.Estado = 1
    WHERE (@Filtro = ''
           OR j.MailJefe LIKE '%' + @Filtro + '%'
           OR ISNULL(u.Nom_Usuario,'') LIKE '%' + @Filtro + '%')
    ORDER BY NombreJefe;
END
GO

/* ---------- 3) SP Guardar (upsert una ventana por jefe) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_GuardarVentanaAprobacionJefe','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_GuardarVentanaAprobacionJefe;
GO
CREATE PROCEDURE dbo.Sp_RTA_GuardarVentanaAprobacionJefe
    @MailJefe        VARCHAR(150),
    @FechaDesde      DATE,
    @FechaHasta      DATE,
    @UsuarioRegistro VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';
    SET @MailJefe = LTRIM(RTRIM(@MailJefe));

    IF ISNULL(@MailJefe,'') = ''
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Debe indicar el jefe.'; RETURN;
    END
    IF @FechaHasta < @FechaDesde
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'La fecha hasta no puede ser menor que la fecha desde.'; RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.RTA_VentanaAprobacionJefe WHERE MailJefe = @MailJefe)
    BEGIN
        UPDATE dbo.RTA_VentanaAprobacionJefe
           SET FechaDesde = @FechaDesde,
               FechaHasta = @FechaHasta,
               Estado = 1,
               UsuarioRegistro = @UsuarioRegistro,
               FechaRegistro = GETDATE()
         WHERE MailJefe = @MailJefe;
        SET @Respuestas = 1; SET @Mensaje = 'Ventana de aprobación actualizada correctamente.';
    END
    ELSE
    BEGIN
        INSERT INTO dbo.RTA_VentanaAprobacionJefe (MailJefe, FechaDesde, FechaHasta, Estado, UsuarioRegistro, FechaRegistro)
        VALUES (@MailJefe, @FechaDesde, @FechaHasta, 1, @UsuarioRegistro, GETDATE());
        SET @Respuestas = 1; SET @Mensaje = 'Ventana de aprobación registrada correctamente.';
    END

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO

/* ---------- 4) SP Validar (usado en la aprobación) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ValidarVentanaAprobacionJefe','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ValidarVentanaAprobacionJefe;
GO
CREATE PROCEDURE dbo.Sp_RTA_ValidarVentanaAprobacionJefe
    @MailJefe VARCHAR(150),
    @Fecha    DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @PuedeAprobar BIT = 1, @Mensaje VARCHAR(400) = '';
    DECLARE @Desde DATE, @Hasta DATE;
    SET @MailJefe = LTRIM(RTRIM(@MailJefe));

    SELECT TOP 1 @Desde = FechaDesde, @Hasta = FechaHasta
    FROM dbo.RTA_VentanaAprobacionJefe
    WHERE MailJefe = @MailJefe AND Estado = 1;

    IF @Desde IS NULL
        SET @PuedeAprobar = 1;                 -- sin ventana => permitir (opt-in)
    ELSE IF @Fecha BETWEEN @Desde AND @Hasta
        SET @PuedeAprobar = 1;
    ELSE
    BEGIN
        SET @PuedeAprobar = 0;
        SET @Mensaje = 'No puede aprobar en esta fecha. Su ventana de aprobación es del '
            + CONVERT(VARCHAR(10), @Desde, 103) + ' al ' + CONVERT(VARCHAR(10), @Hasta, 103) + '.';
    END

    SELECT PuedeAprobar = @PuedeAprobar, Mensaje = @Mensaje;
END
GO
