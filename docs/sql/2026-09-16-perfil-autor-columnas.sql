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

     Emp_CargaFamiliar   tiene Usu_Modificacion, pero es numeric(5). Verificado
                         contra produccion el 2026-09-17: el comentario de la
                         fase 2 que decia "no tiene columna de autor" estaba
                         equivocado, y el que decia "es numeric" tenia razon.

     Empleados           tiene Usu_Modificacion, pero es numeric(5) -un
                         correlativo interno de Talento Humano-.

   Ninguna de las dos admite un Cod_Usuario, y ninguna se convierte: se le
   agrega a cada una Usu_ModificacionCod VARCHAR(50) NULL al lado, y la que ya
   estaba no se toca -sigue recibiendo lo que le escriba el resto del sistema-.

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

/* ------------------------------------------------------------ 0. guarda --- */

/* Antes de tocar nada: si alguna de las dos columnas YA existe pero no es
   varchar de 50 o mas, este script no la toca y se detiene.

   Que la columna exista no alcanza; lo que importa es que admita un
   Cod_Usuario. El nombre elegido, Usu_ModificacionCod, es nuevo en las dos
   tablas, asi que lo normal es que no exista; pero si alguien la creo antes a
   mano con otro tipo, un IF NOT EXISTS por nombre la daria por buena, los
   procedimientos de
   2026-09-16-perfil-autor-procedimientos.sql se crearian sin quejarse y
   fallarian recien al primer guardado de una carga familiar, en produccion, al
   convertir un Cod_Usuario a numero.

   Esta en un lote propio y ANTES del PRINT de inicio, y no dentro de cada
   bloque, por dos razones: asi una corrida detenida no se lee "inicio ... fin"
   como si hubiera sido normal, y asi el mensaje habla de las dos columnas de
   una vez en vez de detenerse en la primera y dejar la segunda sin revisar y
   sin mencionar. */
DECLARE @TipoCargaFam  VARCHAR(128), @LargoCargaFam  INT,
        @TipoEmpleados VARCHAR(128), @LargoEmpleados INT;

/* Cuatro variables mas, que existen SOLO para el texto del mensaje.

   RAISERROR admite como argumentos de sustitucion literales y variables
   locales, nada mas: una llamada a funcion ahi -un ISNULL(), por ejemplo- da
   "Msg 102, Incorrect syntax near 'ISNULL'". Y eso no seria solo un mensaje
   feo: el DECLARE, los dos SELECT, el IF y el SET NOEXEC ON viven todos en el
   MISMO lote, asi que un error de analisis se lleva puesta la guarda entera,
   el script sigue con los ALTER TABLE y una columna del tipo equivocado pasa
   por buena con un PRINT de "ya existia" -exactamente el escenario para el que
   se escribio esta guarda-.

   Se normaliza sobre variables aparte, y no sobre las cuatro de arriba, porque
   el IF de abajo distingue NULL (la columna no existe: caso normal, no hay
   nada que comprobar) de no NULL (existe y hay que mirarle el tipo).
   Normalizar antes del IF borraria esa distincion y la guarda cambiaria de
   sentido. Se llenan DENTRO del BEGIN, cuando el IF ya decidio. */
DECLARE @MsgTipoCargaFam  VARCHAR(128), @MsgLargoCargaFam  INT,
        @MsgTipoEmpleados VARCHAR(128), @MsgLargoEmpleados INT;

SELECT @TipoCargaFam  = t.name,
       @LargoCargaFam = c.max_length
  FROM sys.columns c
  JOIN sys.types   t ON t.user_type_id = c.user_type_id
 WHERE c.object_id = OBJECT_ID('dbo.Emp_CargaFamiliar')
   AND c.name      = 'Usu_ModificacionCod';

SELECT @TipoEmpleados  = t.name,
       @LargoEmpleados = c.max_length
  FROM sys.columns c
  JOIN sys.types   t ON t.user_type_id = c.user_type_id
 WHERE c.object_id = OBJECT_ID('dbo.Empleados')
   AND c.name      = 'Usu_ModificacionCod';

/* NULL es el caso normal: la columna no existe todavia y no hay nada que
   comprobar, la crea el bloque que corresponde. max_length = -1 es
   varchar(MAX): mas ancha que 50, sirve igual. */
IF    (@TipoCargaFam  IS NOT NULL AND NOT (@TipoCargaFam  = 'varchar' AND (@LargoCargaFam  >= 50 OR @LargoCargaFam  = -1)))
   OR (@TipoEmpleados IS NOT NULL AND NOT (@TipoEmpleados = 'varchar' AND (@LargoEmpleados >= 50 OR @LargoEmpleados = -1)))
BEGIN
    /* Recien aca, con el IF ya decidido: normalizar antes habria cambiado la
       condicion de arriba, que necesita ver los NULL. */
    SET @MsgTipoCargaFam   = ISNULL(@TipoCargaFam,  '(no existe)');
    SET @MsgLargoCargaFam  = ISNULL(@LargoCargaFam,  0);
    SET @MsgTipoEmpleados  = ISNULL(@TipoEmpleados, '(no existe)');
    SET @MsgLargoEmpleados = ISNULL(@LargoEmpleados, 0);

    RAISERROR('Alguna columna de autor ya existe con un tipo que no sirve. Encontrado: Emp_CargaFamiliar.Usu_ModificacionCod = %s de largo %d y Empleados.Usu_ModificacionCod = %s de largo %d. En las dos se esperaba varchar de 50 o mas, porque van a recibir un Cod_Usuario. Ojo: NO se refiere a Emp_CargaFamiliar.Usu_Modificacion, que existe y es numeric(5) a proposito y no se toca. No se creo ni se modifico ninguna columna, y el RESTO DE ESTE SCRIPT NO SE EJECUTO: revisar a mano que columna es esa y, una vez corregida, volver a correr este script entero. Script detenido.', 16, 1, @MsgTipoCargaFam, @MsgLargoCargaFam, @MsgTipoEmpleados, @MsgLargoEmpleados);
    SET NOEXEC ON;
END
GO

/* Despues de la guarda a proposito: una corrida detenida no llega a imprimir
   "inicio" y se distingue de una normal con solo mirar la salida. */
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
                      AND name = 'Usu_ModificacionCod')
    BEGIN
        ALTER TABLE dbo.Emp_CargaFamiliar ADD Usu_ModificacionCod VARCHAR(50) NULL;
        PRINT 'Emp_CargaFamiliar.Usu_ModificacionCod creada.';
    END
    /* Si llego hasta aca y la columna ya existe, la guarda de arriba ya
       comprobo que es varchar de 50 o mas. */
    ELSE PRINT 'Emp_CargaFamiliar.Usu_ModificacionCod ya existia.';
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
    /* Aca importa mas todavia que la guarda de arriba haya mirado el tipo: al
       lado vive Usu_Modificacion numeric(5), y una Usu_ModificacionCod con el
       tipo equivocado haria que Sp_RTA_PerfilGuardarContacto escribiera el
       autor del cambio en una columna que no lo admite. */
    ELSE PRINT 'Empleados.Usu_ModificacionCod ya existia.';
END
ELSE PRINT 'dbo.Empleados no existe. Omitido.';
GO

/* Incondicional: si la guarda de tipo encendio NOEXEC, apagarlo aca es lo que
   evita que el resto de la sesion de SSMS quede sin ejecutar nada y parezca que
   los scripts siguientes "no hacen nada". */
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
 WHERE (OBJECT_NAME(c.object_id) = 'Emp_CargaFamiliar' AND c.name = 'Usu_ModificacionCod')
    OR (OBJECT_NAME(c.object_id) = 'Empleados'         AND c.name = 'Usu_ModificacionCod');

   ============================================================================ */
