/* ============================================================================
   Marcacion de entrada/salida — stored procedure
   Base: ReporTarea
   El servidor es el unico dueno de la decision: recibe @Accion explicita,
   resuelve la fila del dia por Id_Usuario + fecha, y toma la hora de GETDATE().
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('dbo.Sp_RTA_RegistrarMarcacion','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_RegistrarMarcacion;
GO
CREATE PROCEDURE dbo.Sp_RTA_RegistrarMarcacion
    @Id_Usuario NUMERIC(6,0),
    @Accion     INT              -- 1 = entrada, 2 = salida
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuestas INT          = 0;
    DECLARE @Mensaje    VARCHAR(300) = '';
    DECLARE @FechaHora  DATETIME     = NULL;
    DECLARE @IdProceso  BIGINT       = NULL;
    DECLARE @Ahora      DATETIME     = GETDATE();
    DECLARE @Hoy        DATE         = CONVERT(DATE, GETDATE());
    DECLARE @VACIO      DATETIME     = CONVERT(DATETIME, '1900-01-01');

    IF @Accion NOT IN (1, 2)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Accion no valida.',
               FechaHora = NULL, IdProceso = NULL;
        RETURN;
    END

    IF ISNULL(@Id_Usuario, 0) = 0
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Usuario no valido.',
               FechaHora = NULL, IdProceso = NULL;
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @FilaId      BIGINT   = NULL;
        DECLARE @FilaEntrada DATETIME = NULL;
        DECLARE @FilaSalida  DATETIME = NULL;

        /* UPDLOCK + HOLDLOCK: dos clics simultaneos no pueden crear dos filas */
        SELECT TOP 1
               @FilaId      = r.IdProceso,
               @FilaEntrada = r.FechaEntrada,
               @FilaSalida  = r.FechaSalida
        FROM dbo.RegistroBiometrico AS r WITH (UPDLOCK, HOLDLOCK)
        WHERE r.Id_Usuario = @Id_Usuario
          AND CONVERT(DATE, r.FechaRegistro) = @Hoy
        ORDER BY r.IdProceso DESC;

        IF @Accion = 1
        BEGIN
            IF @FilaId IS NOT NULL
            BEGIN
                SET @Respuestas = 0;
                SET @Mensaje = 'Ya registro su entrada hoy a las '
                             + CONVERT(VARCHAR(5), ISNULL(@FilaEntrada, @VACIO), 108) + '.';
            END
            ELSE
            BEGIN
                INSERT INTO dbo.RegistroBiometrico
                    (Id_Usuario, FechaEntrada, FechaAlmorzar, FechaRegAlmorzar,
                     FechaSalida, FechaRegistro, Estado)
                VALUES
                    (@Id_Usuario, @Ahora, @VACIO, @VACIO,
                     @VACIO, @Ahora, 1);

                SET @IdProceso  = SCOPE_IDENTITY();
                SET @FechaHora  = @Ahora;
                SET @Respuestas = 1;
                SET @Mensaje    = 'Entrada registrada a las '
                                + CONVERT(VARCHAR(5), @Ahora, 108) + '.';
            END
        END
        ELSE   /* @Accion = 2 */
        BEGIN
            IF @FilaId IS NULL
            BEGIN
                SET @Respuestas = 0;
                SET @Mensaje = 'Debe registrar primero su entrada.';
            END
            ELSE IF ISNULL(@FilaSalida, @VACIO) > @VACIO
            BEGIN
                SET @Respuestas = 0;
                SET @Mensaje = 'Ya registro su salida hoy a las '
                             + CONVERT(VARCHAR(5), @FilaSalida, 108) + '.';
            END
            ELSE
            BEGIN
                UPDATE dbo.RegistroBiometrico
                   SET FechaSalida = @Ahora
                 WHERE IdProceso = @FilaId;

                SET @IdProceso  = @FilaId;
                SET @FechaHora  = @Ahora;
                SET @Respuestas = 1;
                SET @Mensaje    = 'Salida registrada a las '
                                + CONVERT(VARCHAR(5), @Ahora, 108) + '.';
            END
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @FechaHora  = NULL;
        SET @IdProceso  = NULL;
        SET @Mensaje    = 'No se pudo registrar la marcacion: ' + ERROR_MESSAGE();
    END CATCH

    /* Invariante: Respuestas siempre 0 o 1, Mensaje siempre no vacio */
    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje,
           FechaHora = @FechaHora, IdProceso = @IdProceso;
END
GO
