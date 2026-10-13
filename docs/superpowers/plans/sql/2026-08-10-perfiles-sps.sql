SET QUOTED_IDENTIFIER ON;
GO

-- =============================================================================
-- Sp_RTA_EliminarPerfil
-- Contrato con CapaDato/DaoPerfiles.cs (metodo EliminarPerfil):
--   parametro @IdPerfiles, devuelve una fila con (Respuestas int, Mensaje varchar).
--   La capa trata unicamente el valor 1 como exito.
-- =============================================================================
IF OBJECT_ID('dbo.Sp_RTA_EliminarPerfil', 'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_EliminarPerfil;
GO

CREATE PROCEDURE dbo.Sp_RTA_EliminarPerfil
    @IdPerfiles bigint
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @codigo int;
    DECLARE @usuarios int;

    SELECT @codigo = Codigo
    FROM dbo.Perfiles
    WHERE IdPerfiles = @IdPerfiles;

    IF @codigo IS NULL
    BEGIN
        SELECT 0 AS Respuestas, 'El perfil ya no existe.' AS Mensaje;
        RETURN;
    END

    SELECT @usuarios = COUNT(*)
    FROM dbo.R_Usuarios u
    WHERE u.Id_Perfil = @IdPerfiles
       OR u.Cod_Perfil = @codigo;

    IF @usuarios > 0
    BEGIN
        SELECT 0 AS Respuestas,
               'Este perfil tiene ' + CONVERT(varchar(20), @usuarios) + ' usuarios asignados. Reasignelos antes de eliminarlo.' AS Mensaje;
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM dbo.PerfilMenu       WHERE IdPerfil   = @IdPerfiles;
        DELETE FROM dbo.RTA_PerfilInicio WHERE IdPerfil   = @IdPerfiles;
        DELETE FROM dbo.Perfiles         WHERE IdPerfiles = @IdPerfiles;

        COMMIT TRANSACTION;

        SELECT 1 AS Respuestas, 'El perfil se elimino correctamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT 0 AS Respuestas,
               'No se pudo eliminar el perfil: ' + ERROR_MESSAGE() AS Mensaje;
    END CATCH
END
GO

-- =============================================================================
-- Sp_RTA_ListarPerfilesAdmin
-- Contrato con CapaDato/DaoPerfiles.cs (metodo ListarPerfilesAdmin):
--   parametro @filtro varchar(100), devuelve filas con columnas
--   IdPerfiles, NombrePerfil, Estado, Usuarios (los tres ultimos numericos:
--   la capa hace Convert.ToInt32 sobre ellos).
-- El conteo de usuarios usa el mismo criterio que Sp_RTA_EliminarPerfil
-- (Id_Perfil o Cod_Perfil contra el Codigo del perfil), para que un perfil
-- que el listado muestra con usuarios no sea luego "0 usuarios" para el borrado.
-- Se cuentan todos los usuarios, activos e inactivos.
-- =============================================================================
IF OBJECT_ID('dbo.Sp_RTA_ListarPerfilesAdmin', 'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarPerfilesAdmin;
GO

CREATE PROCEDURE dbo.Sp_RTA_ListarPerfilesAdmin
    @filtro varchar(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.IdPerfiles,
        p.NombrePerfil,
        ISNULL(p.Estado, 0) AS Estado,
        (
            SELECT COUNT(*)
            FROM dbo.R_Usuarios u
            WHERE u.Id_Perfil = p.IdPerfiles
               OR u.Cod_Perfil = p.Codigo
        ) AS Usuarios
    FROM dbo.Perfiles p
    WHERE @filtro = '' OR p.NombrePerfil LIKE '%' + @filtro + '%'
    ORDER BY p.NombrePerfil;
END
GO

-- =============================================================================
-- Sp_RTAInsertaNuevoPerfil
-- Contrato con CapaDato/DaoPerfiles.cs (metodo RTAInsertarNuevoPerfil):
--   parametros @Codigo, @NombrePerfil, @Estado, @Fecha; devuelve una columna
--   Respuestas. La capa trata cualquier valor >= 1 como exito y -1 como
--   error de validacion.
-- =============================================================================
IF OBJECT_ID('dbo.Sp_RTAInsertaNuevoPerfil', 'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTAInsertaNuevoPerfil;
GO

CREATE PROCEDURE dbo.Sp_RTAInsertaNuevoPerfil
    @Codigo int,
    @NombrePerfil varchar(150),
    @Estado int,
    @Fecha date
AS
BEGIN
    SET NOCOUNT ON;

    IF @NombrePerfil IS NULL OR LTRIM(RTRIM(@NombrePerfil)) = ''
    BEGIN
        SELECT -1 AS Respuestas;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Perfiles WHERE NombrePerfil = @NombrePerfil)
    BEGIN
        SELECT -1 AS Respuestas;
        RETURN;
    END

    DECLARE @fechaInsertar date;
    IF @Fecha IS NULL OR @Fecha = '19000101'
        SET @fechaInsertar = GETDATE();
    ELSE
        SET @fechaInsertar = @Fecha;

    -- @Codigo se ignora deliberadamente: el handler que llama a este
    -- procedimiento no lo fija (siempre llega 0), y la columna Codigo es
    -- NOT NULL. La convencion existente es Codigo = IdPerfiles, asi que
    -- se inserta primero y despues se actualiza Codigo con el
    -- IdPerfiles recien generado.
    INSERT INTO dbo.Perfiles (Codigo, NombrePerfil, Estado, Fecha)
    VALUES (0, @NombrePerfil, @Estado, @fechaInsertar);

    DECLARE @nuevoId bigint = CONVERT(bigint, SCOPE_IDENTITY());

    UPDATE dbo.Perfiles
    SET Codigo = @nuevoId
    WHERE IdPerfiles = @nuevoId;

    SELECT @nuevoId AS Respuestas;
END
GO
