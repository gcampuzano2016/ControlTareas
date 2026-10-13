/* ============================================================================
   Inactivar jefe en el módulo "Ventana de aprobación" — objetos de BD
   Base: ReporTarea
   ============================================================================ */

/* ---------- 1) Tabla de exclusión ---------- */
IF OBJECT_ID('dbo.RTA_JefeExcluidoVentana','U') IS NULL
BEGIN
    CREATE TABLE dbo.RTA_JefeExcluidoVentana
    (
        Id              INT IDENTITY(1,1) NOT NULL,
        MailJefe        VARCHAR(150)      NOT NULL,
        Estado          BIT               NOT NULL CONSTRAINT DF_RTA_JefeExcl_Estado   DEFAULT(1),
        UsuarioRegistro VARCHAR(100)      NULL,
        FechaRegistro   DATETIME          NOT NULL CONSTRAINT DF_RTA_JefeExcl_FechaReg DEFAULT(GETDATE()),
        CONSTRAINT PK_RTA_JefeExcluidoVentana PRIMARY KEY (Id),
        CONSTRAINT UQ_RTA_JefeExcl_MailJefe UNIQUE (MailJefe)
    );
END
GO

/* ---------- 2) SP Listar (modificado: agrega @IncluirInactivos y campo Inactivo) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarJefesVentanaAprobacion','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarJefesVentanaAprobacion;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarJefesVentanaAprobacion
    @Filtro           VARCHAR(150) = '',
    @IncluirInactivos BIT = 0
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
        TieneVentana      = CASE WHEN v.Id IS NOT NULL THEN 1 ELSE 0 END,
        Inactivo          = CASE WHEN e.Id IS NOT NULL THEN 1 ELSE 0 END
    FROM Jefes j
    OUTER APPLY (
        SELECT TOP 1 ru.Nom_Usuario
        FROM dbo.R_Usuarios ru
        WHERE LTRIM(RTRIM(ru.E_Mail)) = j.MailJefe
        ORDER BY ru.Cod_Usuario
    ) u
    LEFT JOIN dbo.RTA_VentanaAprobacionJefe v
        ON v.MailJefe = j.MailJefe AND v.Estado = 1
    LEFT JOIN dbo.RTA_JefeExcluidoVentana e
        ON e.MailJefe = j.MailJefe AND e.Estado = 1
    WHERE (@Filtro = ''
           OR j.MailJefe LIKE '%' + @Filtro + '%'
           OR ISNULL(u.Nom_Usuario,'') LIKE '%' + @Filtro + '%')
      AND (@IncluirInactivos = 1 OR e.Id IS NULL)
    ORDER BY NombreJefe;
END
GO

/* ---------- 3) SP Excluir/Reactivar (upsert del estado) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ExcluirJefeVentana','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ExcluirJefeVentana;
GO
CREATE PROCEDURE dbo.Sp_RTA_ExcluirJefeVentana
    @MailJefe        VARCHAR(150),
    @Excluir         BIT,
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

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (SELECT 1 FROM dbo.RTA_JefeExcluidoVentana WITH (UPDLOCK, HOLDLOCK) WHERE MailJefe = @MailJefe)
        BEGIN
            UPDATE dbo.RTA_JefeExcluidoVentana
               SET Estado = @Excluir,
                   UsuarioRegistro = @UsuarioRegistro,
                   FechaRegistro = GETDATE()
             WHERE MailJefe = @MailJefe;
        END
        ELSE IF @Excluir = 1
        BEGIN
            INSERT INTO dbo.RTA_JefeExcluidoVentana (MailJefe, Estado, UsuarioRegistro, FechaRegistro)
            VALUES (@MailJefe, 1, @UsuarioRegistro, GETDATE());
        END

        SET @Respuestas = 1;
        SET @Mensaje = CASE WHEN @Excluir = 1
                            THEN 'Jefe inactivado en este modulo correctamente.'
                            ELSE 'Jefe reactivado correctamente.' END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo actualizar el estado del jefe: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO
