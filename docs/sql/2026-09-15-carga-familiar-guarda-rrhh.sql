/* ============================================================================
   Modulo: Talento Humano (RRHHEmpleados.aspx) — guarda contra el perfil
   Spec  : docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md
   Motivo: revision final de la fase 2 de "Perfil del colaborador" (C1b)

   Sp_RTAInsUpdCargaFam valida duplicados asi:

       SELECT @CONTADOR = COUNT(*) FROM Emp_CargaFamiliar WHERE Nombre = @Nombre

   Ese COUNT cuenta tambien las filas que crea el perfil del colaborador
   (Sp_RTA_PerfilGuardarCargaFamiliar), que tienen Cod_Usuario NOT NULL e
   IdEmpleado NULO. Sin este arreglo, una carga familiar registrada desde el
   perfil bloquearia a Talento Humano dar de alta a otra persona distinta con
   ese mismo nombre, con el mensaje "REGISTRO YA EXISTE, NO PUEDE INGRESAR
   DATOS DUPLICADOS" por una fila que esa pantalla no puede ver ni explicar.

   El arreglo, autorizado expresamente por el usuario: se agrega
   "AND Cod_Usuario IS NULL" a ese COUNT. Es inocuo para Talento Humano -toda
   fila que su pantalla crea o edita tiene Cod_Usuario nulo, asi que el COUNT
   sigue viendo exactamente lo mismo que hoy- y dejar de contar las filas del
   perfil, que es lo unico que cambia.

   Se toca solo esa linea. El resto del procedimiento -mayusculas, sangria,
   los dos espacios sueltos, todo- se deja tal cual estaba: se leyo desde
   sys.sql_modules antes de escribir este script para no introducir ninguna
   otra diferencia.

   No se toca Sp_RTACambiarEstadoCargaFam (el toggle por nombre que borra
   cargas familiares): ese arreglo va por otro lado -Emp_CargaFamiliar.Estado
   en '1'/'0' en vez de 'Activo'/'Inactivo' para las filas del perfil, en
   docs/sql/2026-09-14-perfil-colaborador-fase2.sql- y no requiere tocar el
   procedimiento de RRHH.

   Se puede ejecutar varias veces sin efecto adicional.
   Base: ReporTarea
   ============================================================================ */

SET NOCOUNT ON;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('dbo.Sp_RTAInsUpdCargaFam','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTAInsUpdCargaFam;
GO

/*=========================================================================================
DESCRIPCION: PROCEDIMIENTO PARA REALIZAR EL INGRESO Y ACTUALIZACION DE EMP_CARGAFAMILIAR
AUTOR: LEONARDO SALAZAR
FECHA: 08/04/2023
===========================================================================================*/


CREATE PROCEDURE [dbo].[Sp_RTAInsUpdCargaFam]
	 @IDCARGAFAM [BIGINT]
	,@IdEmpleado [BIGINT]
	,@Parentesco VARCHAR(100)
	,@Nombre VARCHAR(100)
	,@Fecha_nacimiento [datetime]
	,@OPERACION INT

AS
BEGIN
	SET NOCOUNT ON

	DECLARE @ERROR INT
	       ,@MENSAJE VARCHAR(200)
		   ,@CONTADOR INT

    SELECT @ERROR=0
	      ,@MENSAJE='REGISTRO GUARDADO EXITOSAMENTE'
		  ,@CONTADOR=0

    IF @OPERACION=1
	BEGIN
		SELECT @CONTADOR=COUNT(*) FROM Emp_CargaFamiliar WHERE Nombre = @Nombre AND Cod_Usuario IS NULL
		IF @CONTADOR =0
		BEGIN
			BEGIN TRAN INSCARGA
			INSERT INTO Emp_CargaFamiliar
            ([IDEMPLEADO]
           ,[PARENTESCO]
           ,[NOMBRE]
           ,[FECHA_NACIMIENTO])
			SELECT @IDEMPLEADO
           ,@PARENTESCO
           ,@NOMBRE
           ,@FECHA_NACIMIENTO

		   SELECT @IDCARGAFAM =@@IDENTITY

		   IF (@@ERROR <>0)
		   BEGIN
			   SELECT @ERROR=2
			   ,@MENSAJE='NO SE PUDO INSERTAR REGISTRO'
			   ROLLBACK TRAN INSCARGA
		   END
		   ELSE
		   BEGIN
			   COMMIT TRAN INSCARGA
		   END
		END
		ELSE
		BEGIN
			SELECT @ERROR=1
			      ,@MENSAJE='REGISTRO YA EXISTE, NO PUEDE INGRESAR DATOS DUPLICADOS'
		END
	END
	IF @OPERACION=2
	BEGIN
		    begin tran UPDCARGA
			UPDATE Emp_CargaFamiliar
			SET IDEMPLEADO = @IDEMPLEADO
				,NOMBRE = @NOMBRE
				,PARENTESCO = @PARENTESCO
				,FECHA_NACIMIENTO = @FECHA_NACIMIENTO
			WHERE IdCargaFam = @IDCARGAFAM

			IF @@ERROR <> 0
			BEGIN
				SELECT @ERROR=3
				     , @MENSAJE='REGISTRO NO PUDO SER ACTUALIZADO'
			    ROLLBACK TRAN UPDCARGA
			END
			ELSE
			BEGIN
				COMMIT TRAN UPDCARGA
			END
	END
	SELECT @ERROR ERROR, @MENSAJE MENSAJE, @ERROR as Respuestas

END
GO
PRINT 'Sp_RTAInsUpdCargaFam recreado con la guarda Cod_Usuario IS NULL.';
GO

/* ------------------------------------------------------- aserciones ------- */

IF OBJECT_ID('dbo.Sp_RTAInsUpdCargaFam','P') IS NULL
    RAISERROR('FALLO: Sp_RTAInsUpdCargaFam no quedo creado.', 16, 1);

IF NOT EXISTS (SELECT 1 FROM sys.sql_modules m
                JOIN sys.procedures p ON p.object_id = m.object_id
               WHERE p.name = 'Sp_RTAInsUpdCargaFam'
                 AND m.definition LIKE '%WHERE Nombre = @Nombre AND Cod_Usuario IS NULL%')
    RAISERROR('FALLO: Sp_RTAInsUpdCargaFam no tiene la guarda Cod_Usuario IS NULL.', 16, 1);

PRINT 'Aserciones OK.';
GO

PRINT 'Script 2026-09-15-carga-familiar-guarda-rrhh completado.';
GO
