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

IF OBJECT_ID('dbo.Emp_CargaFamiliar','U') IS NULL
BEGIN
    RAISERROR('dbo.Emp_CargaFamiliar no existe. Script detenido.', 16, 1);
    RETURN;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.Emp_CargaFamiliar')
                  AND name = 'Usu_Modificacion')
BEGIN
    ALTER TABLE dbo.Emp_CargaFamiliar ADD Usu_Modificacion VARCHAR(50) NULL;
    PRINT 'Emp_CargaFamiliar.Usu_Modificacion creada.';
END
ELSE PRINT 'Emp_CargaFamiliar.Usu_Modificacion ya existia.';
GO

/* -------------------------------------------------------- 2. Empleados --- */

IF OBJECT_ID('dbo.Empleados','U') IS NULL
BEGIN
    RAISERROR('dbo.Empleados no existe. Script detenido.', 16, 1);
    RETURN;
END
GO

/* Usu_ModificacionCod, no Usu_Modificacion: esa ya existe y es numeric(5).
   El sufijo Cod dice de que esta hecha -un Cod_Usuario- y evita que alguien
   la confunda con la vieja. */
IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.Empleados')
                  AND name = 'Usu_ModificacionCod')
BEGIN
    ALTER TABLE dbo.Empleados ADD Usu_ModificacionCod VARCHAR(50) NULL;
    PRINT 'Empleados.Usu_ModificacionCod creada.';
END
ELSE PRINT 'Empleados.Usu_ModificacionCod ya existia.';
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
