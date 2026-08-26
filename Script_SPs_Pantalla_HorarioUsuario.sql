/* ================================================================================
   STORED PROCEDURES PARA LA PANTALLA "Parametrizacion de horario por usuario"
   Base de datos : ReporTarea
   Requisito     : Ejecutar primero Script_Parametrizacion_Horarios_Consolidado.sql
                   (crea R_HorarioLaboral, R_HorarioLaboralDetalle, R_UsuarioHorarioLaboral).

   Mapeo confirmado: R_DetTareasAranda.Id_Responsable = R_Usuarios.Cod_Usuario.

   Se crean 3 SPs:
     1. Sp_RTA_ListarPerfilesHorario    -> combo de perfiles (Id / Valor)
     2. Sp_RTA_ListarUsuariosConHorario -> grilla de usuarios + su horario vigente
     3. Sp_RTA_AsignarHorarioUsuario    -> asigna/cambia horario conservando historial
   ================================================================================ */

USE [ReporTarea];
GO

/* ================================================================================
   1. Sp_RTA_ListarPerfilesHorario
   ================================================================================ */
IF OBJECT_ID(N'dbo.Sp_RTA_ListarPerfilesHorario', N'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarPerfilesHorario;
GO

CREATE PROCEDURE dbo.Sp_RTA_ListarPerfilesHorario
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CONVERT(VARCHAR(10), IdHorarioLaboral) AS Id,
        Nombre                                 AS Valor
    FROM dbo.R_HorarioLaboral
    WHERE Activo = 1
    ORDER BY EsPredeterminado DESC, Nombre;
END;
GO

/* ================================================================================
   2. Sp_RTA_ListarUsuariosConHorario
   Devuelve cada usuario activo con el horario que tiene VIGENTE. Si no tiene
   asignacion abierta, muestra el perfil PREDETERMINADO (EsPredeterminado = 1).
   @Filtro: busca por Nombre, Cod_Usuario o Cedula (vacio = todos).
   ================================================================================ */
IF OBJECT_ID(N'dbo.Sp_RTA_ListarUsuariosConHorario', N'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarUsuariosConHorario;
GO

CREATE PROCEDURE dbo.Sp_RTA_ListarUsuariosConHorario
    @Filtro VARCHAR(100) = ''
AS
BEGIN
    SET NOCOUNT ON;

    SET @Filtro = LTRIM(RTRIM(ISNULL(@Filtro, '')));

    SELECT
        U.Cod_Usuario,
        U.Nom_Usuario,
        ISNULL(U.Cedula, '')        AS Cedula,
        ISNULL(U.Departamento, '')  AS Departamento,
        ISNULL(U.Empresa, '')       AS Empresa,
        COALESCE(HA.IdHorarioLaboral, P.IdHorarioLaboral, 0) AS IdHorarioLaboral,
        COALESCE(HA.Codigo, P.Codigo, '')                    AS CodigoHorario,
        COALESCE(HA.Nombre, P.Nombre, 'Sin perfil')          AS NombreHorario,
        CASE WHEN A.IdHorarioLaboral IS NULL THEN 1 ELSE 0 END AS EsPredeterminado,
        ISNULL(CONVERT(VARCHAR(10), A.FechaDesde, 103), '')  AS FechaDesde
    FROM dbo.R_Usuarios AS U
    OUTER APPLY
    (
        SELECT TOP (1) X.IdHorarioLaboral, X.FechaDesde
        FROM dbo.R_UsuarioHorarioLaboral AS X
        WHERE X.Id_Responsable = U.Cod_Usuario
          AND X.Activo = 1
          AND X.FechaHasta IS NULL
        ORDER BY X.FechaDesde DESC
    ) AS A
    LEFT JOIN dbo.R_HorarioLaboral AS HA
        ON HA.IdHorarioLaboral = A.IdHorarioLaboral
    OUTER APPLY
    (
        SELECT TOP (1) Pre.IdHorarioLaboral, Pre.Codigo, Pre.Nombre
        FROM dbo.R_HorarioLaboral AS Pre
        WHERE Pre.EsPredeterminado = 1
          AND Pre.Activo = 1
        ORDER BY Pre.IdHorarioLaboral
    ) AS P
    WHERE U.Usuario_Estado = 'A'
      AND
      (
          @Filtro = ''
          OR U.Nom_Usuario LIKE '%' + @Filtro + '%'
          OR U.Cod_Usuario LIKE '%' + @Filtro + '%'
          OR ISNULL(U.Cedula, '') LIKE '%' + @Filtro + '%'
      )
    ORDER BY U.Nom_Usuario;
END;
GO

/* ================================================================================
   3. Sp_RTA_AsignarHorarioUsuario
   Asigna un horario a un usuario conservando el historial:
     - Cierra la asignacion abierta anterior (FechaHasta = FechaDesde - 1 dia)
       cuando su FechaDesde es anterior a la nueva.
     - Desactiva asignaciones abiertas cuya FechaDesde sea >= a la nueva
       (evita choque con el CHECK FechaDesde <= FechaHasta).
     - Inserta la nueva asignacion vigente.
   Respuestas: > 0 = Id insertado / OK ; -1 perfil invalido ; -2 usuario inexistente ;
               -3 ya estaba asignado ese mismo perfil/fecha ; -6 fecha invalida.
   ================================================================================ */
IF OBJECT_ID(N'dbo.Sp_RTA_AsignarHorarioUsuario', N'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_AsignarHorarioUsuario;
GO

CREATE PROCEDURE dbo.Sp_RTA_AsignarHorarioUsuario
    @Id_Responsable   VARCHAR(20),
    @IdHorarioLaboral INT,
    @FechaDesde       VARCHAR(10) = NULL,   -- 'yyyy-MM-dd' (NULL = hoy)
    @UsuarioRegistro  VARCHAR(100) = ''
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Fecha DATE;
    DECLARE @NuevoId BIGINT;

    SET @Id_Responsable = LTRIM(RTRIM(ISNULL(@Id_Responsable, '')));

    /* Fecha: si no viene, usa hoy */
    IF @FechaDesde IS NULL OR LTRIM(RTRIM(@FechaDesde)) = ''
        SET @Fecha = CONVERT(DATE, GETDATE());
    ELSE
        SET @Fecha = TRY_CONVERT(DATE, @FechaDesde);

    IF @Fecha IS NULL
    BEGIN
        SELECT -6 AS Respuestas, N'La fecha desde no tiene un formato válido (use yyyy-MM-dd).' AS Mensaje;
        RETURN;
    END;

    /* Validar perfil */
    IF NOT EXISTS
    (
        SELECT 1 FROM dbo.R_HorarioLaboral
        WHERE IdHorarioLaboral = @IdHorarioLaboral AND Activo = 1
    )
    BEGIN
        SELECT -1 AS Respuestas, N'El perfil de horario seleccionado no existe o está inactivo.' AS Mensaje;
        RETURN;
    END;

    /* Validar usuario */
    IF NOT EXISTS
    (
        SELECT 1 FROM dbo.R_Usuarios WHERE Cod_Usuario = @Id_Responsable
    )
    BEGIN
        SELECT -2 AS Respuestas, N'El usuario indicado no existe.' AS Mensaje;
        RETURN;
    END;

    /* Si ya tiene ese mismo perfil vigente desde esa misma fecha, no duplicar */
    IF EXISTS
    (
        SELECT 1
        FROM dbo.R_UsuarioHorarioLaboral
        WHERE Id_Responsable = @Id_Responsable
          AND IdHorarioLaboral = @IdHorarioLaboral
          AND Activo = 1
          AND FechaHasta IS NULL
          AND FechaDesde = @Fecha
    )
    BEGIN
        SELECT -3 AS Respuestas, N'El usuario ya tiene asignado ese mismo horario desde esa fecha.' AS Mensaje;
        RETURN;
    END;

    BEGIN TRY
        BEGIN TRANSACTION;

        /* Cerrar asignaciones abiertas anteriores a la nueva fecha */
        UPDATE dbo.R_UsuarioHorarioLaboral
        SET FechaHasta = DATEADD(DAY, -1, @Fecha)
        WHERE Id_Responsable = @Id_Responsable
          AND Activo = 1
          AND FechaHasta IS NULL
          AND FechaDesde < @Fecha;

        /* Desactivar asignaciones abiertas cuya FechaDesde sea >= a la nueva
           (no se pueden "cerrar" sin violar el CHECK FechaDesde <= FechaHasta) */
        UPDATE dbo.R_UsuarioHorarioLaboral
        SET Activo = 0
        WHERE Id_Responsable = @Id_Responsable
          AND Activo = 1
          AND FechaHasta IS NULL
          AND FechaDesde >= @Fecha;

        /* Insertar la nueva asignacion vigente */
        INSERT INTO dbo.R_UsuarioHorarioLaboral
        (
            Id_Responsable,
            IdHorarioLaboral,
            FechaDesde,
            FechaHasta,
            Activo,
            UsuarioRegistro
        )
        VALUES
        (
            @Id_Responsable,
            @IdHorarioLaboral,
            @Fecha,
            NULL,
            1,
            NULLIF(LTRIM(RTRIM(@UsuarioRegistro)), '')
        );

        SET @NuevoId = CONVERT(BIGINT, SCOPE_IDENTITY());

        COMMIT TRANSACTION;

        SELECT @NuevoId AS Respuestas, N'Horario asignado correctamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        SELECT -99 AS Respuestas,
               N'No se pudo asignar el horario. Detalle: ' + ERROR_MESSAGE() AS Mensaje;
    END CATCH;
END;
GO

PRINT '== SPs de la pantalla de horario por usuario creados ==';
GO
