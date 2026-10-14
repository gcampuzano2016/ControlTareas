/* ============================================================================
   Tablero de Costos: acceso por usuario
   ReporTarea  |  2026-10-14

   PENDIENTE DE EJECUTAR.

   Crea o actualiza:
     1. dbo.Sp_RTA_TableroCostosAccesoUsuario (solo lectura)

   Hasta ahora el tablero era solo para los perfiles 14 y 18. Con esto tambien
   entra quien tenga la hoja TableroCostos.aspx asignada como modulo extra en
   R_UsuarioMenu (pantalla ParametrizacionMenuUsuario.aspx). Asi se le da el
   tablero a una persona sin abrirlo a todo su perfil, y sin escribir su
   codigo en el programa.

   La comparacion es la misma que usa Sp_RTA_ConsultarMenuPerfilUsuario para
   dibujar los extras del menu: Cod_Usuario de la sesion contra
   R_UsuarioMenu.Cod_Usuario, Estado = 'A'. Si las dos no comparan igual, una
   persona podria ver la opcion y recibir "No tiene permisos", o al reves.

   La hoja se busca por Href, no por Id_Menu: el Id cambia entre ambientes.

   Va ANTES de los binarios: el ReporteTareas.dll nuevo llama a este
   procedimiento para cada usuario que no sea de perfil 14 o 18. Si no existe,
   esos usuarios no entran (el codigo niega ante cualquier error); los
   perfiles 14 y 18 no lo llaman y no se ven afectados.

   Repetible. No escribe en ninguna tabla.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------ 1. Sp_RTA_TableroCostosAccesoUsuario ------ */
IF OBJECT_ID('dbo.Sp_RTA_TableroCostosAccesoUsuario', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_TableroCostosAccesoUsuario AS RETURN 0;');
GO

/* Devuelve una fila con TieneAcceso = 1 o 0. */
ALTER PROCEDURE dbo.Sp_RTA_TableroCostosAccesoUsuario
    @CodUsuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TieneAcceso = CASE WHEN EXISTS (
               SELECT 1
                 FROM dbo.R_UsuarioMenu UM
                INNER JOIN dbo.MenuDos M ON UM.Id_Menu = M.Id_Menu
                WHERE @CodUsuario IS NOT NULL
                  AND UM.Cod_Usuario = @CodUsuario
                  AND UM.Estado = 'A'
                  AND M.Href = 'TableroCostos.aspx')
           THEN 1 ELSE 0 END;
END
GO

/* ------------------------------------------------- 2. aserciones ---------- */
DECLARE @Fallos INT = 0;
DECLARE @r TABLE (TieneAcceso INT);

IF OBJECT_ID('dbo.Sp_RTA_TableroCostosAccesoUsuario', 'P') IS NULL
BEGIN
    RAISERROR('FALLO: Sp_RTA_TableroCostosAccesoUsuario no quedo creado.', 16, 1);
    SET @Fallos += 1;
END
ELSE
BEGIN
    /* Un codigo que no existe no tiene acceso. */
    INSERT @r EXEC dbo.Sp_RTA_TableroCostosAccesoUsuario @CodUsuario = '__no_existe__';
    IF (SELECT TieneAcceso FROM @r) <> 0
    BEGIN
        RAISERROR('FALLO: un usuario inexistente tiene acceso.', 16, 1);
        SET @Fallos += 1;
    END
    DELETE @r;

    /* NULL tampoco. */
    INSERT @r EXEC dbo.Sp_RTA_TableroCostosAccesoUsuario @CodUsuario = NULL;
    IF (SELECT TieneAcceso FROM @r) <> 0
    BEGIN
        RAISERROR('FALLO: un usuario NULL tiene acceso.', 16, 1);
        SET @Fallos += 1;
    END
END

IF (SELECT COUNT(*) FROM dbo.MenuDos WHERE Href = 'TableroCostos.aspx') <> 1
BEGIN
    RAISERROR('FALLO: debe haber exactamente una hoja de menu TableroCostos.aspx.', 16, 1);
    SET @Fallos += 1;
END

IF OBJECT_ID('dbo.R_UsuarioMenu', 'U') IS NULL
BEGIN
    RAISERROR('FALLO: no existe R_UsuarioMenu.', 16, 1);
    SET @Fallos += 1;
END

IF @Fallos = 0
    PRINT 'VEREDICTO: OK - acceso por usuario al Tablero de Costos listo en la base.';
ELSE
    RAISERROR('VEREDICTO: FALLO - %d comprobaciones fallaron.', 16, 1, @Fallos);
GO
