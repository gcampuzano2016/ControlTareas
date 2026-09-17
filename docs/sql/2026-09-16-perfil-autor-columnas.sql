/* ============================================================================
   Perfil del colaborador: donde anotar QUIEN hizo el cambio
   ReporTarea  |  2026-09-16

   PENDIENTE DE EJECUTAR. Es el paso 1 de la entrega 1; el orden completo esta
   en docs/superpowers/specs/2026-09-16-perfil-edicion-rrhh-design.md.

   ----------------------------------------------------------------------------
   Que hace y por que

   Hasta hoy, en el modulo de perfil el dueno del perfil y el autor del cambio
   eran siempre la misma persona, asi que Usu_Modificacion = Cod_Usuario era
   cierto por construccion. En cuanto Talento Humano pueda corregir el perfil de
   otro, esa columna pasaria a MENTIR: diria que el cambio lo hizo el empleado.

   Las siete tablas Perfil_* ya tienen Usu_Modificacion VARCHAR(50) y no
   necesitan nada. Las dos que este script toca no son de este modulo:

     Emp_CargaFamiliar   no tiene ninguna columna de autor.
     Empleados           tiene Usu_Modificacion, pero es numeric(5) -un
                         correlativo interno de Talento Humano- y no admite un
                         Cod_Usuario. No se convierte: se agrega una al lado.

   Las dos columnas son NULL y no las nombra ningun INSERT existente, asi que
   RRHHEmpleados.aspx sigue funcionando exactamente igual.

   Si alguna de las dos ya existe pero con otro tipo, el script no la toca y se
   detiene diciendo que encontro y que esperaba. Una columna que se llama igual
   pero no admite un Cod_Usuario es peor que una que falta: no se nota hasta
   que falla un guardado en produccion.

   Idempotente.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '== Perfil: columnas de autor - inicio ==';
GO

/* ------------------------------------------------ 1. Emp_CargaFamiliar --- */

/* La comprobacion de existencia de la tabla va DENTRO del mismo IF que la de
   la columna, en un solo lote: un RETURN fuera de un procedimiento solo corta
   el lote actual, no el resto del script, asi que no sirve para detenerlo. Si
   la tabla no existe, este bloque no hace ningun ALTER TABLE y lo dice con un
   PRINT de "omitido", sin prometer una detencion que no ocurre. */
IF OBJECT_ID('dbo.Emp_CargaFamiliar','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('dbo.Emp_CargaFamiliar')
                      AND name = 'Usu_Modificacion')
    BEGIN
        ALTER TABLE dbo.Emp_CargaFamiliar ADD Usu_Modificacion VARCHAR(50) NULL;
        PRINT 'Emp_CargaFamiliar.Usu_Modificacion creada.';
    END
    ELSE
    BEGIN
        /* Que la columna exista no alcanza: lo que importa es que admita un
           Cod_Usuario. El comentario del punto 3 de
           Sp_RTA_PerfilGuardarCargaFamiliar (fase 2) afirma que esta columna
           "es numeric", contradiciendo al punto 1 del mismo comentario, que
           dice que no existe. Nadie puede resolver eso sin mirar la base, asi
           que lo resuelve el script: si la encuentra y no es varchar de 50 o
           mas, se detiene. Sin esto, el IF NOT EXISTS de arriba la daria por
           buena por el solo hecho de llamarse igual, los procedimientos de
           2026-09-16-perfil-autor-procedimientos.sql se crearian sin quejarse
           y fallarian recien al primer guardado de una carga familiar, en
           produccion, al convertir un Cod_Usuario a numero. */
        DECLARE @TipoCargaFam VARCHAR(128), @LargoCargaFam INT;

        SELECT @TipoCargaFam  = t.name,
               @LargoCargaFam = c.max_length
          FROM sys.columns c
          JOIN sys.types   t ON t.user_type_id = c.user_type_id
         WHERE c.object_id = OBJECT_ID('dbo.Emp_CargaFamiliar')
           AND c.name      = 'Usu_Modificacion';

        /* max_length = -1 es varchar(MAX): mas ancha que 50, sirve igual. */
        IF @TipoCargaFam = 'varchar' AND (@LargoCargaFam >= 50 OR @LargoCargaFam = -1)
            PRINT 'Emp_CargaFamiliar.Usu_Modificacion ya existia.';
        ELSE
        BEGIN
            RAISERROR('Emp_CargaFamiliar.Usu_Modificacion ya existe pero es %s de largo %d, y se esperaba varchar de 50 o mas: una columna asi no admite un Cod_Usuario. Revisar a mano que columna es antes de seguir; no se toca nada. Script detenido.', 16, 1, @TipoCargaFam, @LargoCargaFam);
            SET NOEXEC ON;
        END
    END
END
ELSE PRINT 'dbo.Emp_CargaFamiliar no existe. Omitido.';
GO

/* -------------------------------------------------------- 2. Empleados --- */

/* Usu_ModificacionCod, no Usu_Modificacion: esa ya existe y es numeric(5).
   El sufijo Cod dice de que esta hecha -un Cod_Usuario- y evita que alguien
   la confunda con la vieja. */
IF OBJECT_ID('dbo.Empleados','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('dbo.Empleados')
                      AND name = 'Usu_ModificacionCod')
    BEGIN
        ALTER TABLE dbo.Empleados ADD Usu_ModificacionCod VARCHAR(50) NULL;
        PRINT 'Empleados.Usu_ModificacionCod creada.';
    END
    ELSE
    BEGIN
        /* Mismo criterio que arriba, y aca importa mas todavia: al lado vive
           Usu_Modificacion numeric(5). Si alguien alguna vez creo una
           Usu_ModificacionCod con otro tipo, el IF NOT EXISTS la daria por
           buena y Sp_RTA_PerfilGuardarContacto escribiria el autor del cambio
           en una columna que no lo admite. */
        DECLARE @TipoEmpleados VARCHAR(128), @LargoEmpleados INT;

        SELECT @TipoEmpleados  = t.name,
               @LargoEmpleados = c.max_length
          FROM sys.columns c
          JOIN sys.types   t ON t.user_type_id = c.user_type_id
         WHERE c.object_id = OBJECT_ID('dbo.Empleados')
           AND c.name      = 'Usu_ModificacionCod';

        /* max_length = -1 es varchar(MAX): mas ancha que 50, sirve igual. */
        IF @TipoEmpleados = 'varchar' AND (@LargoEmpleados >= 50 OR @LargoEmpleados = -1)
            PRINT 'Empleados.Usu_ModificacionCod ya existia.';
        ELSE
        BEGIN
            RAISERROR('Empleados.Usu_ModificacionCod ya existe pero es %s de largo %d, y se esperaba varchar de 50 o mas: una columna asi no admite un Cod_Usuario. Revisar a mano que columna es antes de seguir; no se toca nada. Script detenido.', 16, 1, @TipoEmpleados, @LargoEmpleados);
            SET NOEXEC ON;
        END
    END
END
ELSE PRINT 'dbo.Empleados no existe. Omitido.';
GO

/* Incondicional: si alguna de las dos guardas de tipo encendio NOEXEC, apagarlo
   aca es lo que evita que el resto de la sesion de SSMS quede sin ejecutar nada
   y parezca que los scripts siguientes "no hacen nada". */
SET NOEXEC OFF;
GO

PRINT '== Perfil: columnas de autor - fin ==';
GO

/* ============================================================================
   VERIFICACION (correr a mano despues del script)

   Esperado: 2 filas, las dos varchar, largo 50, admiteNulo = 1.

SELECT  tabla    = OBJECT_NAME(c.object_id),
        columna  = c.name,
        tipo     = t.name,
        largo    = c.max_length,
        admiteNulo = c.is_nullable
  FROM  sys.columns c
  JOIN  sys.types  t ON t.user_type_id = c.user_type_id
 WHERE (OBJECT_NAME(c.object_id) = 'Emp_CargaFamiliar' AND c.name = 'Usu_Modificacion')
    OR (OBJECT_NAME(c.object_id) = 'Empleados'         AND c.name = 'Usu_ModificacionCod');

   ============================================================================ */
