/* =============================================================================
   Bitacora de correos de solicitudes (permisos, vacaciones y planificacion).

   Motivo: en ObtenerListaTareas.ashx.cs y RespuestaAprobacion.aspx.cs el
   resultado de cada envio se asignaba a una variable que nadie leia, asi que un
   correo que no salia no dejaba ningun rastro: la pantalla decia "guardado"
   igual. La unica huella era C:\Detalle\Log\<dd-MM-yyyy>\log.txt del servidor
   web, que no siempre esta al alcance.

   Se registra un renglon por cada intento de aviso, incluso cuando no habia a
   quien escribirle (SIN_CORREO). Ese caso es el que hoy desaparece sin ruido.

   Idempotente: se puede correr varias veces.
   Solo agrega objetos nuevos. No modifica ninguna tabla ni procedimiento
   existente, y no toca datos.
   ============================================================================= */

USE ReporTarea;
GO

/* ---------- Tabla ---------- */
IF OBJECT_ID('dbo.RegistroCorreoSolicitud', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RegistroCorreoSolicitud
    (
        IdLog         BIGINT        IDENTITY(1,1) NOT NULL,
        IdVacaciones  BIGINT        NOT NULL,      /* Vacaciones.IdVacaciones */
        TipoAviso     VARCHAR(30)   NOT NULL,      /* ver lista abajo          */
        Destinatario  VARCHAR(400)  NULL,          /* 400: a GTH van varias    */
        Asunto        VARCHAR(200)  NULL,
        Resultado     VARCHAR(20)   NOT NULL,      /* ENVIADO | FALLIDO | SIN_CORREO */
        Mensaje       VARCHAR(500)  NULL,
        FechaRegistro DATETIME      NOT NULL
            CONSTRAINT DF_RegistroCorreoSolicitud_Fecha DEFAULT (GETDATE()),
        CONSTRAINT PK_RegistroCorreoSolicitud PRIMARY KEY CLUSTERED (IdLog)
    );

    /* "Todo lo que paso con la solicitud N". */
    CREATE INDEX IX_RegistroCorreoSolicitud_Solicitud
        ON dbo.RegistroCorreoSolicitud (IdVacaciones, FechaRegistro);

    /* "Que se cayo esta semana". */
    CREATE INDEX IX_RegistroCorreoSolicitud_Resultado_Fecha
        ON dbo.RegistroCorreoSolicitud (Resultado, FechaRegistro);
END
GO

/* TipoAviso, los siete momentos del ciclo:

     SOLICITUD_JEFE           el aviso al jefe al crear o actualizar
     COPIA_COLABORADOR        la copia que recibe quien la pidio
     PLANIFICACION_JEFE       la planificacion anual de vacaciones
     CANCELACION_JEFE         la cancelacion, al jefe
     CANCELACION_COLABORADOR  la cancelacion, al colaborador
     COLABORADOR_APROBADO     el documento firmado que vuelve al colaborador
     TALENTO_HUMANO           el aviso a GTH cuando el jefe aprueba

   No hay tabla de catalogo ni restriccion CHECK a proposito: agregar un momento
   nuevo no deberia obligar a un script de base. El valor lo fija el C#. */

/* ---------- SP de escritura ---------- */
IF OBJECT_ID('dbo.Sp_RTA_RegistrarLogCorreoSolicitud', 'P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_RegistrarLogCorreoSolicitud;
GO

CREATE PROCEDURE dbo.Sp_RTA_RegistrarLogCorreoSolicitud
    @IdVacaciones BIGINT,
    @TipoAviso    VARCHAR(30),
    @Destinatario VARCHAR(400) = NULL,
    @Asunto       VARCHAR(200) = NULL,
    @Resultado    VARCHAR(20),
    @Mensaje      VARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* La bitacora nunca debe tumbar la solicitud ni el envio: si algo falla
       aqui, se traga. Mismo criterio que Sp_RTA_RegistrarLogCorreoMarcacion. */
    BEGIN TRY
        INSERT INTO dbo.RegistroCorreoSolicitud
            (IdVacaciones, TipoAviso, Destinatario, Asunto, Resultado, Mensaje)
        VALUES
            (@IdVacaciones,
             @TipoAviso,
             LEFT(ISNULL(@Destinatario, ''), 400),
             LEFT(ISNULL(@Asunto,       ''), 200),
             @Resultado,
             LEFT(ISNULL(@Mensaje,      ''), 500));
    END TRY
    BEGIN CATCH
        /* silencio deliberado */
    END CATCH
END
GO


/* =============================================================================
   Consultas de diagnostico. No se ejecutan al correr el script.
   =============================================================================

-- Todo lo que se intento avisar sobre una solicitud:
SELECT FechaRegistro, TipoAviso, Destinatario, Resultado, Mensaje
FROM   dbo.RegistroCorreoSolicitud
WHERE  IdVacaciones = 22942
ORDER  BY FechaRegistro;

-- Lo que no salio en la ultima semana, con el nombre del colaborador:
SELECT l.FechaRegistro, l.IdVacaciones, v.Colaborador, l.TipoAviso,
       l.Destinatario, l.Resultado, l.Mensaje
FROM   dbo.RegistroCorreoSolicitud l
LEFT   JOIN dbo.Vacaciones v ON v.IdVacaciones = l.IdVacaciones
WHERE  l.Resultado <> 'ENVIADO'
  AND  l.FechaRegistro >= DATEADD(DAY, -7, GETDATE())
ORDER  BY l.FechaRegistro DESC;

-- Quienes se estan quedando sin aviso por no tener direccion cargada:
SELECT l.TipoAviso, COUNT(*) AS Veces, MAX(l.FechaRegistro) AS Ultima
FROM   dbo.RegistroCorreoSolicitud l
WHERE  l.Resultado = 'SIN_CORREO'
GROUP  BY l.TipoAviso
ORDER  BY Veces DESC;

-- Un jefe concreto: se le esta avisando, y salio bien?
SELECT l.FechaRegistro, l.IdVacaciones, v.Colaborador, l.Resultado, l.Mensaje
FROM   dbo.RegistroCorreoSolicitud l
LEFT   JOIN dbo.Vacaciones v ON v.IdVacaciones = l.IdVacaciones
WHERE  l.TipoAviso = 'SOLICITUD_JEFE'
  AND  l.Destinatario LIKE '%mmejia@dos.com.ec%'
ORDER  BY l.FechaRegistro DESC;

============================================================================= */
