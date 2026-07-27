/* ============================================================================
   Pagina de inicio y "tipo" por perfil — objetos de BD
   Base: ReporTarea
   ============================================================================ */

/* ---------- 1) Tabla ---------- */
IF OBJECT_ID('dbo.RTA_PerfilInicio','U') IS NULL
BEGIN
    CREATE TABLE dbo.RTA_PerfilInicio
    (
        IdPerfil        INT           NOT NULL,
        Href            VARCHAR(150)  NOT NULL,
        IdTipo          INT           NOT NULL,
        Estado          BIT           NOT NULL CONSTRAINT DF_RTA_PerfilInicio_Estado   DEFAULT(1),
        UsuarioRegistro VARCHAR(100)  NULL,
        FechaRegistro   DATETIME      NOT NULL CONSTRAINT DF_RTA_PerfilInicio_FechaReg DEFAULT(GETDATE()),
        CONSTRAINT PK_RTA_PerfilInicio PRIMARY KEY (IdPerfil)
    );
END
GO

/* ---------- 2) Seed (preserva el comportamiento actual del Login) — idempotente ---------- */
;WITH Semilla(IdPerfil, IdTipo) AS (
    SELECT * FROM (VALUES
        (2,1),(3,1),(4,4),(5,5),(6,6),(7,7),(8,8),(9,9),(10,10),(11,11),
        (12,12),(13,13),(14,14),(15,15),(16,16),(17,17),(18,18),(19,19),
        (20,20),(21,21),(41,41)
    ) AS S(IdPerfil, IdTipo)
)
INSERT INTO dbo.RTA_PerfilInicio (IdPerfil, Href, IdTipo, Estado, UsuarioRegistro)
SELECT s.IdPerfil, 'Principal.aspx', s.IdTipo, 1, 'seed'
FROM Semilla s
WHERE NOT EXISTS (SELECT 1 FROM dbo.RTA_PerfilInicio p WHERE p.IdPerfil = s.IdPerfil);
GO

/* ---------- 3) SP Listar (para la pantalla) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarPerfilInicio','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarPerfilInicio;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarPerfilInicio
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        IdPerfil     = p.Id_Perfil,
        NombrePerfil = p.Nombre,
        Href         = i.Href,
        TituloPagina = m.Titulo,
        IdTipo       = i.IdTipo
    FROM dbo.R_Perfil p
    LEFT JOIN dbo.RTA_PerfilInicio i ON i.IdPerfil = p.Id_Perfil AND i.Estado = 1
    OUTER APPLY (
        SELECT TOP 1 md.Titulo
        FROM dbo.MenuDos md
        WHERE md.Href = i.Href
        ORDER BY md.Id_Menu
    ) m
    WHERE ISNULL(p.Estado_Logico_Registro,1) = 1
    ORDER BY p.Id_Perfil;
END
GO

/* ---------- 4) SP Guardar (upsert, contrato Respuestas/Mensaje) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_GuardarPerfilInicio','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_GuardarPerfilInicio;
GO
CREATE PROCEDURE dbo.Sp_RTA_GuardarPerfilInicio
    @IdPerfil        INT,
    @Href            VARCHAR(150),
    @IdTipo          INT,
    @UsuarioRegistro VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';
    SET @Href = LTRIM(RTRIM(@Href));

    IF ISNULL(@IdPerfil,0) = 0 OR NOT EXISTS (SELECT 1 FROM dbo.R_Perfil WHERE Id_Perfil = @IdPerfil)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El perfil indicado no existe.'; RETURN;
    END
    IF ISNULL(@Href,'') = ''
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Debe indicar la pagina de inicio.'; RETURN;
    END
    /* La pagina de inicio debe ser una pantalla real registrada en MenuDos
       (mismo universo que el desplegable), para evitar redirecciones invalidas. */
    IF NOT EXISTS (SELECT 1 FROM dbo.MenuDos WHERE Href = @Href AND Href NOT LIKE 'Es Men%')
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'La pagina de inicio no es valida.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (SELECT 1 FROM dbo.RTA_PerfilInicio WITH (UPDLOCK, HOLDLOCK) WHERE IdPerfil = @IdPerfil)
            UPDATE dbo.RTA_PerfilInicio
               SET Href = @Href, IdTipo = @IdTipo, Estado = 1,
                   UsuarioRegistro = @UsuarioRegistro, FechaRegistro = GETDATE()
             WHERE IdPerfil = @IdPerfil;
        ELSE
            INSERT INTO dbo.RTA_PerfilInicio (IdPerfil, Href, IdTipo, Estado, UsuarioRegistro, FechaRegistro)
            VALUES (@IdPerfil, @Href, @IdTipo, 1, @UsuarioRegistro, GETDATE());

        SET @Respuestas = 1;
        SET @Mensaje = 'Configuracion guardada correctamente.';
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo guardar la configuracion: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO

/* ---------- 5) SP Obtener (para el Login, siempre 1 fila con fallback) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ObtenerPerfilInicio','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ObtenerPerfilInicio;
GO
CREATE PROCEDURE dbo.Sp_RTA_ObtenerPerfilInicio
    @IdPerfil INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1
        Href   = ISNULL(i.Href, 'Principal.aspx'),
        IdTipo = ISNULL(i.IdTipo, 0)
    FROM (SELECT 1 AS x) d
    LEFT JOIN dbo.RTA_PerfilInicio i ON i.IdPerfil = @IdPerfil AND i.Estado = 1;
END
GO

/* ---------- 6) SP Listar paginas (para el desplegable) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarPaginasMenu','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarPaginasMenu;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarPaginasMenu
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT Titulo, Href
    FROM dbo.MenuDos
    WHERE ISNULL(Href,'') <> '' AND Href NOT LIKE 'Es Men%'
    ORDER BY Titulo;
END
GO
