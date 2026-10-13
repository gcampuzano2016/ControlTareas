/* ============================================================================
   Datos personales editables: donde anotar quien cambio que, y cuando
   ReporTarea  |  2026-09-17

   YA APLICADO EN PRODUCCION (verificado el 2026-09-21). Es el paso 1; el
   orden completo esta en
   docs/superpowers/specs/2026-09-17-datos-personales-editables-design.md.

   ----------------------------------------------------------------------------
   R_Usuarios no tiene NINGUNA columna de auditoria. Verificado contra
   produccion el 2026-09-17: no hay autor, ni fecha de modificacion, ni IP. Lo
   unico parecido es Fec_Creacion, y es varchar(50).

   Hasta hoy daba igual: a R_Usuarios le escribia AdministrarUsuarios.aspx y
   nadie mas. En cuanto Talento Humano pueda cambiar desde el perfil el jefe
   inmediato o el correo de notificacion -los dos campos que deciden quien
   aprueba las vacaciones de alguien y a donde le llegan los avisos- hace falta
   poder reconstruir quien lo hizo.

   Se agregan las tres, no solo el autor: saber quien cambio el jefe de alguien
   sin saber cuando no sirve para reconstruir nada.

   Las tres son NULL y ningun INSERT existente las nombra, asi que
   AdministrarUsuarios.aspx sigue funcionando exactamente igual.

   Idempotente.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------------------------------ 0. guarda --- */

/* Si Usu_ModificacionCod ya existe pero no admite un Cod_Usuario, este script
   no la toca y se detiene. El nombre es nuevo en esta tabla, asi que lo normal
   es que no exista; pero si alguien la creo a mano con otro tipo, un
   IF NOT EXISTS por nombre la daria por buena y el guardado fallaria recien en
   produccion, al escribir el autor.

   RAISERROR admite como argumentos de sustitucion literales y variables
   locales, nada mas: un ISNULL() ahi da "Msg 102, Incorrect syntax near
   'ISNULL'", y como el DECLARE, el SELECT, el IF y el SET NOEXEC ON viven en el
   MISMO lote, ese error de analisis se llevaria puesta la guarda entera. Por
   eso se normaliza sobre variables aparte, DENTRO del BEGIN, cuando el IF ya
   decidio.

   Solo se comprueba Usu_ModificacionCod. Fec_Modificacion e Ip_Modificacion no
   necesitan guarda: si existieran con otro tipo, el ALTER no se ejecuta -el
   IF NOT EXISTS lo impide- y el procedimiento de la entrega siguiente fallaria
   al compilarse, que es ruidoso y ocurre antes de tocar ningun dato. La del
   autor es distinta porque una columna de texto angosta o numerica aceptaria el
   CREATE y reventaria recien al primer guardado. */
DECLARE @Tipo VARCHAR(128), @Largo INT, @MsgTipo VARCHAR(128), @MsgLargo INT;

SELECT @Tipo = t.name, @Largo = c.max_length
  FROM sys.columns c
  JOIN sys.types   t ON t.user_type_id = c.user_type_id
 WHERE c.object_id = OBJECT_ID('dbo.R_Usuarios')
   AND c.name      = 'Usu_ModificacionCod';

/* max_length = -1 es varchar(MAX): mas ancha que 50, sirve igual. */
IF @Tipo IS NOT NULL AND NOT (@Tipo = 'varchar' AND (@Largo >= 50 OR @Largo = -1))
BEGIN
    SET @MsgTipo  = ISNULL(@Tipo, '(no existe)');
    SET @MsgLargo = ISNULL(@Largo, 0);

    RAISERROR('R_Usuarios.Usu_ModificacionCod ya existe con un tipo que no sirve: es %s de largo %d y se esperaba varchar de 50 o mas, porque va a recibir un Cod_Usuario. No se creo ni se modifico ninguna columna, y el RESTO DE ESTE SCRIPT NO SE EJECUTO. Script detenido.', 16, 1, @MsgTipo, @MsgLargo);
    SET NOEXEC ON;
END
GO

/* Despues de la guarda a proposito: una corrida detenida no llega a imprimir
   "inicio" y se distingue de una normal con solo mirar la salida. */
PRINT '== Datos personales: auditoria de R_Usuarios - inicio ==';
GO

IF OBJECT_ID('dbo.R_Usuarios','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('dbo.R_Usuarios') AND name = 'Usu_ModificacionCod')
    BEGIN
        ALTER TABLE dbo.R_Usuarios ADD Usu_ModificacionCod VARCHAR(50) NULL;
        PRINT 'R_Usuarios.Usu_ModificacionCod creada.';
    END
    ELSE PRINT 'R_Usuarios.Usu_ModificacionCod ya existia.';

    IF NOT EXISTS (SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('dbo.R_Usuarios') AND name = 'Fec_Modificacion')
    BEGIN
        ALTER TABLE dbo.R_Usuarios ADD Fec_Modificacion DATETIME NULL;
        PRINT 'R_Usuarios.Fec_Modificacion creada.';
    END
    ELSE PRINT 'R_Usuarios.Fec_Modificacion ya existia.';

    /* varchar(64) y no 32 como en Empleados: alli el ancho lo impuso una tabla
       que ya existia, aca se elige, y 64 entra una IPv6 completa sin recortar.
       El procedimiento que la escribe hace LEFT(@Ip, 64) igual. */
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('dbo.R_Usuarios') AND name = 'Ip_Modificacion')
    BEGIN
        ALTER TABLE dbo.R_Usuarios ADD Ip_Modificacion VARCHAR(64) NULL;
        PRINT 'R_Usuarios.Ip_Modificacion creada.';
    END
    ELSE PRINT 'R_Usuarios.Ip_Modificacion ya existia.';
END
ELSE PRINT 'dbo.R_Usuarios no existe. Omitido.';
GO

/* Incondicional: si la guarda encendio NOEXEC, apagarlo aca evita que el resto
   de la sesion de SSMS quede sin ejecutar nada y parezca que los scripts
   siguientes "no hacen nada". */
SET NOEXEC OFF;
GO

PRINT '== Datos personales: auditoria de R_Usuarios - fin ==';
GO

/* ============================================================================
   VERIFICACION (correr a mano despues del script)

   Esperado: 3 filas, todas admiteNulo = 1.

SELECT  columna = c.name, tipo = t.name, largo = c.max_length,
        admiteNulo = c.is_nullable
  FROM  sys.columns c
  JOIN  sys.types  t ON t.user_type_id = c.user_type_id
 WHERE  c.object_id = OBJECT_ID('dbo.R_Usuarios')
   AND  c.name IN ('Usu_ModificacionCod','Fec_Modificacion','Ip_Modificacion');

   ============================================================================ */
