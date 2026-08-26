/* =============================================================================
   Bitacora de correos de marcacion.

   Motivo: el envio del correo de entrada/salida es fire-and-forget en un hilo
   del ThreadPool. Hasta ahora, si el correo no salia no quedaba ningun rastro
   y era imposible saber a posteriori que paso con un usuario concreto.

   Se registra SIEMPRE un renglon por marcacion exitosa, incluso cuando no se
   intenta enviar nada porque el usuario no tiene correo cargado.
   ============================================================================= */

USE ReporTarea;
GO

/* ---------- Tabla ---------- */
IF OBJECT_ID('dbo.RegistroCorreoMarcacion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RegistroCorreoMarcacion
    (
        IdLog         BIGINT        IDENTITY(1,1) NOT NULL,
        IdProceso     BIGINT        NULL,          /* RegistroBiometrico.IdProceso */
        Id_Usuario    NUMERIC(6,0)  NOT NULL,
        Accion        INT           NOT NULL,      /* 1 = entrada, 2 = salida   */
        Destinatario  VARCHAR(200)  NULL,
        Resultado     VARCHAR(20)   NOT NULL,      /* ENVIADO | FALLIDO | SIN_CORREO */
        Mensaje       VARCHAR(500)  NULL,
        FechaRegistro DATETIME      NOT NULL CONSTRAINT DF_RegistroCorreoMarcacion_Fecha DEFAULT (GETDATE()),
        CONSTRAINT PK_RegistroCorreoMarcacion PRIMARY KEY CLUSTERED (IdLog)
    );

    CREATE INDEX IX_RegistroCorreoMarcacion_Usuario_Fecha
        ON dbo.RegistroCorreoMarcacion (Id_Usuario, FechaRegistro);

    CREATE INDEX IX_RegistroCorreoMarcacion_Resultado_Fecha
        ON dbo.RegistroCorreoMarcacion (Resultado, FechaRegistro);
END
GO

/* ---------- SP de escritura ---------- */
IF OBJECT_ID('dbo.Sp_RTA_RegistrarLogCorreoMarcacion', 'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_RegistrarLogCorreoMarcacion;
GO

CREATE PROCEDURE dbo.Sp_RTA_RegistrarLogCorreoMarcacion
    @IdProceso    BIGINT       = NULL,
    @Id_Usuario   NUMERIC(6,0),
    @Accion       INT,
    @Destinatario VARCHAR(200) = NULL,
    @Resultado    VARCHAR(20),
    @Mensaje      VARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* La bitacora nunca debe tumbar la marcacion: si algo falla aqui, se traga. */
    BEGIN TRY
        INSERT INTO dbo.RegistroCorreoMarcacion
            (IdProceso, Id_Usuario, Accion, Destinatario, Resultado, Mensaje)
        VALUES
            (@IdProceso, @Id_Usuario, @Accion,
             LEFT(ISNULL(@Destinatario, ''), 200),
             @Resultado,
             LEFT(ISNULL(@Mensaje, ''), 500));
    END TRY
    BEGIN CATCH
        /* silencio deliberado */
    END CATCH
END
GO

/* ---------- Consulta de diagnostico ----------
   Quien marco y NO recibio correo, en los ultimos 7 dias:

   SELECT l.FechaRegistro, u.Cod_Usuario, u.Nom_Usuario, l.Accion,
          l.Destinatario, l.Resultado, l.Mensaje
   FROM dbo.RegistroCorreoMarcacion l
   LEFT JOIN dbo.R_Usuarios u ON u.Id_Usuario = l.Id_Usuario
   WHERE l.Resultado <> 'ENVIADO'
     AND l.FechaRegistro >= DATEADD(day, -7, CONVERT(date, GETDATE()))
   ORDER BY l.FechaRegistro DESC;
------------------------------------------------ */
