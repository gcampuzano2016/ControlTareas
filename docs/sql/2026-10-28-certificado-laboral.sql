/* ============================================================================
   Perfil - certificado laboral
   ReporTarea  |  2026-10-28
   Crea:
     1. Perfil_CertificadoLaboral        el historial: una fila por certificado
     2. Sp_RTA_PerfilCertificadoDatos    lo que va en el certificado, en vivo
     3. Sp_RTA_PerfilCertificadoRegistrar
     4. Sp_RTA_PerfilCertificadoListar
     5. Sp_RTA_PerfilCertificadoObtener
   Repetible: correrlo dos veces no duplica nada. No toca tablas existentes.
   Sin claves foraneas, como el resto de la base. Sin tildes a proposito:
   sqlcmd lee el archivo con la pagina de codigos del equipo.

   De donde sale cada dato:
     - nombre, cedula, cargo, empresa y fecha de ingreso: Vac_Colaborador, el
       maestro de personal que GTH carga desde su Excel. Es la unica tabla con
       la fecha de ingreso de todo el personal: Empleados no la tiene y
       CA_empleados solo trae a quienes pasan por la curva de aprendizaje.
     - el sueldo: HE_Salario, el mismo historial que usa horas extras. El
       vigente lo elige NegHorasExtras.SalarioVigente en C#, no este SQL, para
       que haya una sola regla.
     - la ciudad: Empleados.Ciudad.

   Por que el historial guarda una FOTO de los datos y no solo el tipo: un
   certificado ya entregado tiene que poder volver a descargarse identico. Si se
   regenerara con los datos de hoy, el de alguien que cambio de cargo o de
   sueldo diria otra cosa que el papel que ya entrego.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* --------------------------------------- 1. Perfil_CertificadoLaboral ------ */
IF OBJECT_ID('dbo.Perfil_CertificadoLaboral', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfil_CertificadoLaboral
    (
        IdCertificado  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Perfil_CertificadoLaboral PRIMARY KEY,
        Cod_Usuario    VARCHAR(50)    NOT NULL,   -- de quien es el certificado
        Tipo           VARCHAR(3)     NOT NULL,   -- 'SIN' o 'CON' salario
        Empresa        VARCHAR(20)    NOT NULL,   -- 'DOS' o 'AGILITY'
        Nombre         NVARCHAR(400)  NOT NULL,
        Cedula         VARCHAR(20)    NOT NULL,
        Cargo          NVARCHAR(200)  NOT NULL,
        FechaIngreso   DATE           NOT NULL,
        Salario        DECIMAL(18,2)  NULL,       -- solo en los 'CON'
        Ciudad         NVARCHAR(100)  NOT NULL,
        Fec_Solicitud  DATETIME       NOT NULL CONSTRAINT DF_Perfil_CertificadoLaboral_Fec DEFAULT GETDATE(),
        Usu_Solicita   VARCHAR(50)    NOT NULL,   -- quien lo genero: el dueno o Talento Humano
        Ip_Solicitud   VARCHAR(64)    NULL
    );

    CREATE INDEX IX_Perfil_CertificadoLaboral_Usuario
        ON dbo.Perfil_CertificadoLaboral (Cod_Usuario, Fec_Solicitud DESC);

    PRINT 'Perfil_CertificadoLaboral creada.';
END
GO

/* ---------------------------------- 2. Sp_RTA_PerfilCertificadoDatos ------- */
IF OBJECT_ID('dbo.Sp_RTA_PerfilCertificadoDatos','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilCertificadoDatos;
GO
/* Dos resultados: (1) una fila con los datos del certificado, o ninguna si no
   hay de quien hacerlo; (2) el historial de sueldos de esa persona.

   La guarda de codigo repetido compara EXACTAMENTE igual que el SELECT al que
   protege -mismo campo, sin TRIM, mismo filtro de usuarios activos-. Si la
   guarda contara un conjunto y el SELECT entregara otro, protegeria menos de
   lo que entrega (fue un hallazgo Critical en la fase 3b del perfil).

   Vac_Colaborador se cruza por cedula con OUTER APPLY TOP 1: hoy no hay
   cedulas activas repetidas, pero la tabla no lo impide, y dos filas de la
   misma cedula son la misma persona; un JOIN duplicaria la fila y el Dao lee
   solo la primera. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilCertificadoDatos
    @Cod_Usuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    DECLARE @Datos TABLE
    (
        Cod_Usuario  VARCHAR(50),
        IdEmpleado   BIGINT,
        Cedula       VARCHAR(20),
        Nombre       NVARCHAR(400),
        Cargo        NVARCHAR(200),
        Empresa      VARCHAR(20),
        FechaIngreso DATE,
        Ciudad       NVARCHAR(100),
        EnMaestro    BIT
    );

    INSERT INTO @Datos
    SELECT  u.Cod_Usuario,
            e.IdEmpleado,
            Cedula       = LTRIM(RTRIM(ISNULL(NULLIF(LTRIM(RTRIM(u.Cedula)), ''), e.Cedula))),
            Nombre       = ISNULL(NULLIF(LTRIM(RTRIM(v.Nombre)), ''),
                                  ISNULL(NULLIF(LTRIM(RTRIM(e.Nombre)), ''), u.Nom_Usuario)),
            Cargo        = LTRIM(RTRIM(ISNULL(v.Cargo, ''))),
            Empresa      = UPPER(LTRIM(RTRIM(ISNULL(v.Empresa, '')))),
            v.FechaIngreso,
            Ciudad       = LTRIM(RTRIM(ISNULL(e.Ciudad, ''))),
            EnMaestro    = CASE WHEN v.IdColaborador IS NULL THEN 0 ELSE 1 END
      FROM  dbo.R_Usuarios u
      LEFT JOIN dbo.Empleados e ON e.Cod_Usuario = u.Cod_Usuario
      OUTER APPLY (SELECT TOP 1 vc.IdColaborador, vc.Nombre, vc.Cargo, vc.Empresa, vc.FechaIngreso
                     FROM dbo.Vac_Colaborador vc
                    WHERE LTRIM(RTRIM(vc.Cedula)) = LTRIM(RTRIM(ISNULL(NULLIF(LTRIM(RTRIM(u.Cedula)), ''), e.Cedula)))
                      AND vc.Estado = 'ACTIVO'
                    ORDER BY vc.IdColaborador DESC) v
     WHERE  u.Cod_Usuario = @Cod_Usuario
       AND  ISNULL(u.EstadoUsuario,0) = 0
       AND  @CodigoRepetido = 0;

    SELECT Cod_Usuario, IdEmpleado, Cedula, Nombre, Cargo, Empresa, FechaIngreso, Ciudad, EnMaestro
      FROM @Datos;

    SELECT s.Monto, s.FechaVigenciaDesde, Origen = ISNULL(s.Origen, '')
      FROM dbo.HE_Salario s
      JOIN @Datos d ON d.IdEmpleado = s.IdEmpleado
     WHERE s.Estado = '1'
     ORDER BY s.FechaVigenciaDesde;
END
GO
PRINT 'Sp_RTA_PerfilCertificadoDatos creado.';
GO

/* ------------------------------ 3. Sp_RTA_PerfilCertificadoRegistrar ------- */
IF OBJECT_ID('dbo.Sp_RTA_PerfilCertificadoRegistrar','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilCertificadoRegistrar;
GO
/* Guarda la foto que ya armo la capa de negocio. No vuelve a leer los datos:
   lo que se guarda es exactamente lo que la persona vio en la vista previa. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilCertificadoRegistrar
    @Cod_Usuario  VARCHAR(50),
    @Tipo         VARCHAR(3),
    @Empresa      VARCHAR(20),
    @Nombre       NVARCHAR(400),
    @Cedula       VARCHAR(20),
    @Cargo        NVARCHAR(200),
    @FechaIngreso DATE,
    @Salario      DECIMAL(18,2) = NULL,
    @Ciudad       NVARCHAR(100),
    @Usu_Solicita VARCHAR(50),
    @Ip           VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Tipo NOT IN ('SIN', 'CON')
    BEGIN
        SELECT Respuestas = -1, IdCertificado = CAST(NULL AS INT);
        RETURN;
    END

    INSERT INTO dbo.Perfil_CertificadoLaboral
        (Cod_Usuario, Tipo, Empresa, Nombre, Cedula, Cargo, FechaIngreso,
         Salario, Ciudad, Fec_Solicitud, Usu_Solicita, Ip_Solicitud)
    VALUES
        (@Cod_Usuario, @Tipo, @Empresa, @Nombre, @Cedula, @Cargo, @FechaIngreso,
         CASE WHEN @Tipo = 'CON' THEN @Salario END, @Ciudad, GETDATE(), @Usu_Solicita, @Ip);

    SELECT Respuestas = 0, IdCertificado = CAST(SCOPE_IDENTITY() AS INT);
END
GO
PRINT 'Sp_RTA_PerfilCertificadoRegistrar creado.';
GO

/* --------------------------------- 4. Sp_RTA_PerfilCertificadoListar ------- */
IF OBJECT_ID('dbo.Sp_RTA_PerfilCertificadoListar','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilCertificadoListar;
GO
/* @Cod_Usuario vacio = todos (el historial de Talento Humano). Quien puede
   pedir "todos" lo decide el handler, no este procedimiento. El sueldo NO va
   en la lista: solo dentro del PDF. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilCertificadoListar
    @Cod_Usuario VARCHAR(50) = ''
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  c.IdCertificado, c.Cod_Usuario, c.Tipo, c.Empresa, c.Nombre, c.Cedula,
            c.Cargo, c.Fec_Solicitud, c.Usu_Solicita
      FROM  dbo.Perfil_CertificadoLaboral c
     WHERE  ISNULL(@Cod_Usuario, '') = '' OR c.Cod_Usuario = @Cod_Usuario
     ORDER BY c.Fec_Solicitud DESC, c.IdCertificado DESC;
END
GO
PRINT 'Sp_RTA_PerfilCertificadoListar creado.';
GO

/* -------------------------------- 5. Sp_RTA_PerfilCertificadoObtener ------- */
IF OBJECT_ID('dbo.Sp_RTA_PerfilCertificadoObtener','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilCertificadoObtener;
GO
/* La foto completa de un certificado, para volver a armar su PDF.
   @Cod_Usuario vacio = sin filtro de dueno (Talento Humano). Con codigo, un
   Id ajeno no devuelve nada: el handler ve un numero y no sabe de quien es. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilCertificadoObtener
    @IdCertificado INT,
    @Cod_Usuario   VARCHAR(50) = ''
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  c.IdCertificado, c.Cod_Usuario, c.Tipo, c.Empresa, c.Nombre, c.Cedula,
            c.Cargo, c.FechaIngreso, c.Salario, c.Ciudad, c.Fec_Solicitud, c.Usu_Solicita
      FROM  dbo.Perfil_CertificadoLaboral c
     WHERE  c.IdCertificado = @IdCertificado
       AND  (ISNULL(@Cod_Usuario, '') = '' OR c.Cod_Usuario = @Cod_Usuario);
END
GO
PRINT 'Sp_RTA_PerfilCertificadoObtener creado.';
GO
