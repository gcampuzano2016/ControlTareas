/* ============================================================================
   Vacaciones - "Mis vacaciones": cada colaborador ve su propio saldo
   ReporTarea  |  2026-10-29
   Plan: docs/superpowers/plans/2026-10-28-vacaciones-motor-de-saldos.md
   Crea:
     1. Sp_RTA_VacColaboradorDeUsuario   de un Cod_Usuario (el de la sesion)
                                         a su IdColaborador, por la cedula.
   La cedula sale de DOS lugares: R_Usuarios.Cedula y Empleados.Cedula (por
   Empleados.Cod_Usuario). Con solo Empleados casaban 91 de los 95 activos:
   hay filas de Empleados sin Cod_Usuario. Con las dos, 93, y en ningun usuario
   las dos fuentes dan cedulas distintas (medido el 2026-10-29).
   Devuelve TODOS los que casen: si un usuario casa con dos personas -por
   ejemplo porque sus dos cedulas no coinciden-, la pantalla no muestra nada
   en vez de elegir una.
   Solo lee. Repetible. Sin tildes a proposito.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('dbo.Sp_RTA_VacColaboradorDeUsuario', 'P') IS NULL
    EXEC ('CREATE PROCEDURE dbo.Sp_RTA_VacColaboradorDeUsuario AS RETURN 0');
GO
ALTER PROCEDURE dbo.Sp_RTA_VacColaboradorDeUsuario
    @CodUsuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Cod VARCHAR(50) = LTRIM(RTRIM(ISNULL(@CodUsuario, '')));
    IF @Cod = '' RETURN;

    SELECT DISTINCT v.IdColaborador
    FROM dbo.Vac_Colaborador v
    WHERE v.Cedula IN (SELECT LTRIM(RTRIM(u.Cedula)) FROM dbo.R_Usuarios u
                        WHERE LTRIM(RTRIM(u.Cod_Usuario)) = @Cod AND ISNULL(LTRIM(RTRIM(u.Cedula)), '') <> ''
                       UNION
                       SELECT LTRIM(RTRIM(e.Cedula)) FROM dbo.Empleados e
                        WHERE LTRIM(RTRIM(e.Cod_Usuario)) = @Cod AND ISNULL(LTRIM(RTRIM(e.Cedula)), '') <> '');
END
GO

PRINT 'Sp_RTA_VacColaboradorDeUsuario listo.';
GO
