/* ============================================================================
   Etapa 5 de CB-GAP-POL-01: los avisos de la recuperacion.

   1. NotificacionRecuperacion                : que se envio y cuando
   2. Sp_RTA_ListarNotificacionesRecuperacion : que toca enviar ahora
   3. Sp_RTA_MarcarNotificacionEnviada        : deja constancia

   ORDEN: despues de 2026-08-25-plan-recuperacion.sql.
   Se puede ejecutar varias veces sin efecto adicional. No modifica ni una fila.
   Base: ReporTarea
   ============================================================================

   Los tres momentos
   -----------------
   SEGUIMIENTO   a los 8 dias de aprobado el permiso, para que el asunto tenga
                 avance y no se descubra al vencer.
   RECORDATORIO  3 dias antes de la fecha maxima de cierre.
   ESCALACION    pasada la fecha maxima sin que nadie confirme.

   Los tres avisan de lo mismo en momentos distintos, que es justo lo que pide la
   especificacion. Van al colaborador y a su jefe inmediato.

   Como se decide, y por que asi
   -----------------------------
   El servicio WinSerEnvioCorreo no es una cola generica: es un bucle cableado
   que consulta las aprobaciones de horas extra pendientes, manda el correo y
   marca la fila. No hay una tabla donde encolar cualquier mensaje.

   Asi que se sigue ese mismo patron en vez de inventar otro: la decision de que
   toca enviar vive en un procedimiento, y el servicio solo envia y marca. Es el
   camino que ya funciona en esta casa y el que menos piezas nuevas agrega.

   La consecuencia operativa importa: estos avisos NO salen solos por desplegar
   el sitio. WinSerEnvioCorreo es un ejecutable aparte, con su propia solucion
   (WinCorreo.sln), que hay que compilar e instalar en el servidor. Mientras eso
   no pase, la tabla y los procedimientos quedan listos y sin usar.

   Por que una tabla de lo enviado
   -------------------------------
   Para no repetir. El servicio sondea cada segundo: sin registro de lo ya
   enviado, un aviso de seguimiento saldria 86.400 veces por dia. La clave unica
   sobre (solicitud, tipo) es lo que lo impide, no la buena voluntad del bucle.
   ============================================================================ */

SET NOCOUNT ON;
GO

/* ------------------------------------------------------- 1. lo ya enviado */
IF OBJECT_ID('dbo.NotificacionRecuperacion') IS NULL
BEGIN
    CREATE TABLE dbo.NotificacionRecuperacion
    (
        IdNotificacion INT IDENTITY(1,1) NOT NULL,
        IdVacaciones   BIGINT       NOT NULL,

        /* SEGUIMIENTO | RECORDATORIO | ESCALACION */
        Tipo           VARCHAR(20)  NOT NULL,

        FechaEnvio     DATETIME2(0) NOT NULL
            CONSTRAINT DF_NotificacionRecuperacion_Fecha DEFAULT (SYSDATETIME()),
        Destinatarios  VARCHAR(300) NULL,

        CONSTRAINT PK_NotificacionRecuperacion PRIMARY KEY (IdNotificacion),

        /* Cada aviso, una sola vez por solicitud. Es la salvaguarda real contra
           el sondeo de cada segundo. */
        CONSTRAINT UQ_NotificacionRecuperacion UNIQUE (IdVacaciones, Tipo)
    );

    PRINT 'NotificacionRecuperacion creada.';
END
ELSE
    PRINT 'NotificacionRecuperacion ya existia.';
GO

/* --------------------------------------------------- 2. que toca enviar */
IF OBJECT_ID('dbo.Sp_RTA_ListarNotificacionesRecuperacion') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarNotificacionesRecuperacion;
GO

CREATE PROCEDURE dbo.Sp_RTA_ListarNotificacionesRecuperacion
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Hoy DATE = CAST(GETDATE() AS DATE);

    /* Base: recuperaciones sin confirmar de permisos que no fueron rechazados.
       Una recuperacion de un permiso rechazado no tiene de que avisar. */
    WITH Pendientes AS
    (
        SELECT r.IdVacaciones,
               r.FechaMaximaCierre,
               FechaAprobacion = CAST(ISNULL(v.FechaAprobacion, r.FechaRegistro) AS DATE),
               FechaPermiso    = CAST(v.FechaDesde AS DATE),
               v.Colaborador,
               v.JefeInmediato,
               v.CodSap,
               Horas = ISNULL(v.Horas,'')
        FROM dbo.PermisoRecuperacion r
        INNER JOIN dbo.Vacaciones v ON v.IdVacaciones = r.IdVacaciones
        WHERE r.Confirmado IS NULL
          AND v.EstadoSolicitud NOT IN ('RECHAZADO', 'RECHAZADO GTH', 'ANULAR')
    )
    SELECT p.IdVacaciones,
           t.Tipo,
           p.Colaborador,
           p.JefeInmediato,
           p.CodSap,
           p.Horas,
           p.FechaPermiso,
           p.FechaMaximaCierre,
           /* Los correos se resuelven aca para que el servicio no tenga que
              saber como se llega de un colaborador a su jefe. */
           CorreoColaborador = ISNULL(uc.E_Mail,''),
           CorreoJefe        = ISNULL(uj.E_Mail,'')
    FROM Pendientes p
    CROSS APPLY
    (
        SELECT Tipo = 'ESCALACION',   Desde = p.FechaMaximaCierre
        UNION ALL
        SELECT 'RECORDATORIO',        DATEADD(DAY, -3, p.FechaMaximaCierre)
        UNION ALL
        SELECT 'SEGUIMIENTO',         DATEADD(DAY,  8, p.FechaAprobacion)
    ) t
    /* OUTER APPLY con TOP 1 y no LEFT JOIN, porque ni Cod_Sap ni Cod_Usuario son
       unicos en R_Usuarios: con JOIN cada fila salia duplicada y el servicio
       mandaria el mismo correo dos veces. Se elige activo primero y el
       Id_Usuario mas alto, igual que en Sp_RTA_SaldoPermisoMensual, para que la
       eleccion sea siempre la misma. */
    OUTER APPLY
    (
        SELECT TOP 1 E_Mail FROM dbo.R_Usuarios u
        WHERE u.Cod_Sap = p.CodSap AND ISNULL(u.E_Mail,'') <> ''
        ORDER BY CASE WHEN u.Usuario_Estado = 'A' THEN 0 ELSE 1 END, u.Id_Usuario DESC
    ) uc
    OUTER APPLY
    (
        SELECT TOP 1 E_Mail FROM dbo.R_Usuarios u
        WHERE u.Cod_Usuario = p.JefeInmediato AND ISNULL(u.E_Mail,'') <> ''
        ORDER BY CASE WHEN u.Usuario_Estado = 'A' THEN 0 ELSE 1 END, u.Id_Usuario DESC
    ) uj
    WHERE @Hoy >= t.Desde
      AND NOT EXISTS (SELECT 1 FROM dbo.NotificacionRecuperacion n
                      WHERE n.IdVacaciones = p.IdVacaciones AND n.Tipo = t.Tipo)
    /* La escalacion primero: si un caso llego vencido sin ningun aviso previo,
       lo urgente es que se sepa que vencio, no el seguimiento de hace semanas. */
    ORDER BY CASE t.Tipo WHEN 'ESCALACION' THEN 1 WHEN 'RECORDATORIO' THEN 2 ELSE 3 END,
             p.FechaMaximaCierre;
END
GO
PRINT 'Sp_RTA_ListarNotificacionesRecuperacion creado.';
GO

/* ------------------------------------------------------- 3. dejar constancia */
IF OBJECT_ID('dbo.Sp_RTA_MarcarNotificacionEnviada') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_MarcarNotificacionEnviada;
GO

CREATE PROCEDURE dbo.Sp_RTA_MarcarNotificacionEnviada
    @IdVacaciones  BIGINT,
    @Tipo          VARCHAR(20),
    @Destinatarios VARCHAR(300) = ''
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO dbo.NotificacionRecuperacion (IdVacaciones, Tipo, Destinatarios)
        VALUES (@IdVacaciones, UPPER(LTRIM(RTRIM(@Tipo))), NULLIF(LTRIM(RTRIM(@Destinatarios)),''));

        SELECT Respuestas = 1, Mensaje = 'Notificacion registrada.';
    END TRY
    BEGIN CATCH
        /* Choque contra la clave unica: alguien ya la marco. No es un error, es
           el caso normal si dos ciclos se pisan. */
        IF ERROR_NUMBER() = 2627 OR ERROR_NUMBER() = 2601
            SELECT Respuestas = 1, Mensaje = 'Esa notificacion ya estaba registrada.';
        ELSE
            SELECT Respuestas = 0, Mensaje = 'No se pudo registrar: ' + ERROR_MESSAGE();
    END CATCH
END
GO
PRINT 'Sp_RTA_MarcarNotificacionEnviada creado.';
GO
