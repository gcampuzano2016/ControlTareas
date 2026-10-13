/* ============================================================================
   La edición también clasifica el tipo de hora
   ReporTarea  |  2026-09-15

   PENDIENTE DE EJECUTAR. Correr primero en PRUEBAS.

   ----------------------------------------------------------------------------
   Qué cambia y por qué

   Desde el 07-09-2026 crear una actividad clasifica sola: el sábado, el domingo
   y los feriados salen al 100% sin que nadie toque el combo. Editarla no.

   Sp_RTAActualizaDetalleTarea recibía Det_Horas_Extras_Tipo del combo de la
   pantalla y lo guardaba tal cual. Abrir un registro de sábado que estaba
   correcto al 100% y cambiarle cualquier cosa -la descripción, el tipo de
   gasto- lo devolvía al valor del combo, que viene en blanco (0) por defecto.
   Sin error, sin aviso, sin nada en la pantalla.

   Ahora la edición clasifica igual que la creación.

   ----------------------------------------------------------------------------
   El responsable se lee de la fila, no de un parámetro

   Sp_RTAInsertaDetalleTarea_V2 recibe @Id_Responsable de quien lo llama. Acá no
   hace falta: la fila que se está modificando ya lo tiene. Se lee de ahí.

   Eso evita el modo de falla más caro. Si el procedimiento exigiera un
   parámetro nuevo, cualquier camino de edición que no lo mandara rompería toda
   la pantalla con un error de SQL Server -y hay más de uno:
   AdministrarTarea.ashx.cs y AdministrarCargaArchivos.ashx.cs-. Leyéndolo de la
   fila, la firma no cambia y ningún camino se queda afuera.

   ----------------------------------------------------------------------------
   Qué pasa si el rango abarca varios tramos

   Se rechaza con -7 y el mensaje lista los tramos para registrarlos por
   separado, igual que hacía el insert antes del cambio del 26 de agosto.

   No se parte en varias filas como hace el insert, y es a propósito: partir en
   una edición obligaría a borrar la fila y reinsertar, lo que le cambia el
   Id_RegDetTareas. Ese identificador ya está en los archivos adjuntos y en las
   URL encriptadas de los correos de aprobación que se enviaron. Cambiarlo los
   deja apuntando a un registro que ya no existe.

   ----------------------------------------------------------------------------
   El procedimiento ahora devuelve tres columnas

       Respuestas          1 = guardado. Negativo = no se guardó.
       Mensaje             el detalle para la pantalla (los tramos, en el -7).
       TipoHoraCalculado   el tipo que decidió el horario.

   La tercera no es un lujo. El C# decide si manda el correo de autorización
   comparando el tipo nuevo contra el anterior; si el procedimiento cambia el
   tipo por su cuenta y no lo informa, el C# sigue creyendo lo que decía el
   combo y no pide la firma. Quedaría una hora al 100% que nadie autorizó.

   Códigos negativos:
       -5  no existe el registro que se intenta modificar
       -6  fechas inválidas, o fin <= inicio
       -7  cruza la medianoche, o mezcla tipos de hora
       -8  no hay configuración de horario activa para ese día

   ----------------------------------------------------------------------------
   Requisitos

   Necesita FN_RTA_ClasificarTramosHorario, que ya está en producción desde el
   26-08-2026 (Script_Parametrizacion_Horarios_Consolidado.sql, PASO 8).

   ----------------------------------------------------------------------------
   Rollback

   El cuerpo anterior quedó guardado. Para volver atrás basta con reemplazar
   este ALTER por el original: no se tocó ninguna tabla ni ninguna columna.

   ----------------------------------------------------------------------------
   OJO: este script solo no alcanza

   Hay que desplegar también el CapaDato.exe nuevo, que es el que lee las
   columnas Mensaje y TipoHoraCalculado. Si se aplica solo el SQL, la
   clasificación funciona pero el correo de autorización no se dispara cuando el
   tipo lo decide el sistema. Ver el commit que acompaña a este archivo.
   ============================================================================ */

USE [ReporTarea];
GO

ALTER PROCEDURE [dbo].[Sp_RTAActualizaDetalleTarea]
				 @Id_RegDetTareas numeric(6, 0),
				 @Id_RegTareas bigint,
				 @Det_Num_OrdenServicio varchar(50),
				 @Det_Id_CompAranda varchar(80),
				 @Det_Det_Tarea  varchar(500),
				 @Det_Fch_RegDetalleIni varchar(40),
				 @Det_Fch_RegDetalleFin varchar(40),
				 @Det_Tiempo varchar(8),
				 @Det_Observaciones varchar(512),
				 @Det_Horas_Extras_Tipo int,
				 @IdUsuarioSession varchar(16),
				 @Det_Ip_Modificacion varchar(32),
				 @Cod_CatalogoTareaSap bigint,
				 @IdTipoGasto bigint
				--exec Sp_RTAActualizaDetalleTarea 


AS
BEGIN
	SET NOCOUNT ON;

	--------------------------------------------
	DECLARE @Respuestas int
	DECLARE @Tiempo varchar(10)
	/* --- Agregado 2026-09-15: clasificacion automatica del tipo de hora --- */
	DECLARE @Id_Responsable varchar(20)
	DECLARE @FechaInicio datetime
	DECLARE @FechaFin datetime
	DECLARE @TipoHoraCalculado bigint
	DECLARE @CantidadTipos int
	DECLARE @DetalleTramos nvarchar(max)
	DECLARE @Tramos TABLE
	(
		Orden       INT NOT NULL,
		TramoInicio DATETIME NOT NULL,
		TramoFin    DATETIME NOT NULL,
		TipoHora    BIGINT NOT NULL,
		Descripcion VARCHAR(50) NOT NULL
	)

	/* ================================================================
	   CLASIFICACION AUTOMATICA DEL TIPO DE HORA
	   ----------------------------------------------------------------
	   Antes el tipo llegaba del combo de la pantalla y se guardaba tal
	   cual. Por eso editar un registro de sabado, domingo o feriado
	   borraba el 100% sin ningun aviso: bastaba con cambiar la
	   descripcion o el tipo de gasto.

	   Ahora decide el horario del responsable, igual que hace
	   Sp_RTAInsertaDetalleTarea_V2 al crear.

	   El responsable se lee de la propia fila, no de un parametro. Asi
	   ningun camino de edicion puede quedarse sin mandarlo, y no hay
	   que cambiar la firma que ya usa la capa de datos.
	   ================================================================ */

	SELECT @Id_Responsable = Id_Responsable
	FROM dbo.R_DetTareasAranda
	WHERE Id_RegDetTareas = @Id_RegDetTareas

	IF @Id_Responsable IS NULL
	BEGIN
		SELECT -5 AS Respuestas,
		       N'No se encontro el registro que se intenta modificar.' AS Mensaje,
		       @Det_Horas_Extras_Tipo AS TipoHoraCalculado
		RETURN
	END

	SET @FechaInicio = TRY_CONVERT(datetime, @Det_Fch_RegDetalleIni, 120)
	SET @FechaFin    = TRY_CONVERT(datetime, @Det_Fch_RegDetalleFin, 120)

	IF @FechaInicio IS NULL OR @FechaFin IS NULL
	BEGIN
		SELECT -6 AS Respuestas,
		       N'Las fechas no tienen un formato valido. Utilice yyyy-MM-dd HH:mm:ss.' AS Mensaje,
		       @Det_Horas_Extras_Tipo AS TipoHoraCalculado
		RETURN
	END

	IF @FechaInicio >= @FechaFin
	BEGIN
		SELECT -6 AS Respuestas,
		       N'La fecha final debe ser mayor que la fecha inicial.' AS Mensaje,
		       @Det_Horas_Extras_Tipo AS TipoHoraCalculado
		RETURN
	END

	IF CONVERT(date, @FechaInicio) <> CONVERT(date, @FechaFin)
	BEGIN
		SELECT -7 AS Respuestas,
		       N'El horario cruza la medianoche. Registre cada fecha por separado.' AS Mensaje,
		       @Det_Horas_Extras_Tipo AS TipoHoraCalculado
		RETURN
	END

	INSERT INTO @Tramos (Orden, TramoInicio, TramoFin, TipoHora, Descripcion)
	SELECT Orden, TramoInicio, TramoFin, TipoHora, Descripcion
	FROM dbo.FN_RTA_ClasificarTramosHorario(@Id_Responsable, @FechaInicio, @FechaFin)

	IF NOT EXISTS (SELECT 1 FROM @Tramos)
	BEGIN
		SELECT -8 AS Respuestas,
		       N'No existe una configuracion activa para el dia seleccionado.' AS Mensaje,
		       @Det_Horas_Extras_Tipo AS TipoHoraCalculado
		RETURN
	END

	SELECT @CantidadTipos     = COUNT(DISTINCT TipoHora),
	       @TipoHoraCalculado = MIN(TipoHora)
	FROM @Tramos

	/* Varios tipos en un mismo rango: la edicion no puede partir una
	   fila en tres sin cambiarle el Id_RegDetTareas, y eso rompe los
	   adjuntos y los correos de aprobacion ya enviados. Se rechaza y se
	   pide registrarlos por separado, igual que hacia el insert antes
	   del cambio del 26 de agosto. */
	IF @CantidadTipos > 1
	BEGIN
		SELECT @DetalleTramos =
			STUFF
			(
				(
					SELECT N'; '
					     + CONVERT(varchar(5), T.TramoInicio, 108)
					     + N' a '
					     + CONVERT(varchar(5), T.TramoFin, 108)
					     + N' = '
					     + T.Descripcion
					FROM @Tramos AS T
					ORDER BY T.Orden
					FOR XML PATH(''), TYPE
				).value('.', 'NVARCHAR(MAX)'),
				1, 2, N''
			)

		SELECT -7 AS Respuestas,
		       N'El horario enviado contiene diferentes tipos de horas. '
		       + N'Registre los siguientes tramos por separado: '
		       + @DetalleTramos + N'.' AS Mensaje,
		       @Det_Horas_Extras_Tipo AS TipoHoraCalculado
		RETURN
	END

	/* El combo de la pantalla deja de decidir. La descripcion sale sola
	   del Catalogo mas abajo, porque se busca por este mismo valor. */
	SET @Det_Horas_Extras_Tipo = @TipoHoraCalculado
	/* --- fin del agregado 2026-09-15 --- */
	--------------------------------------------

	-- ------------------------------------------------
	-- Actulizo la fecha de fin para determinar el tiempo de duraciÃ³n de la tarea
	-- ------------------------------------------------
	SELECT @Tiempo = CONVERT(VARCHAR(8), CONVERT(datetime, @Det_Fch_RegDetalleFin, 120) - CAST(CONVERT(VARCHAR(8), CONVERT(datetime, @Det_Fch_RegDetalleFin, 120), 112) + ' ' + CONVERT(VARCHAR(8), CONVERT(datetime, @Det_Fch_RegDetalleIni, 120), 108) AS DATETIME), 108)

	IF(@IdTipoGasto=0)
	BEGIN

			INSERT INTO [dbo].[R_TareasLog]
					   ([Id_RegDetTareas]
					   ,[Id_RegTareas]
					   ,[Det_Num_OrdenServicio]
					   ,[Det_Id_CompAranda]
					   ,[Det_Fch_RegDetalleIni]
					   ,[Det_Fch_RegDetalleFin]
					   ,[Det_EstadoIni]
					   ,[Det_EstadoFin]
					   ,[Det_Tiempo]
					   ,[Det_Nom_Empresa]
					   ,[Det_Det_Tarea]
					   ,[Det_Estado]
					   ,[Id_Responsable]
					   ,[Det_Det_TareaFin]
					   ,[Det_Motivo_Cambio_Estado]
					   ,[IdDet_EstadoFin]


					   ,[IdDet_EstadoIni]
					   ,[Det_Observaciones]
					   ,[Det_Horas_Extras_Estado]
					   ,[Det_Horas_Extras_Tipo]
					   ,[Det_Horas_Extras_Descripcion]
					   ,[Det_Horas_Extras_Envio_Correo]
					   ,[Det_Aprobacion_Tarea_Estado]
					   ,[Det_Horas_Extras_Fecha_Solicitud]
					   ,[Det_Horas_Extras_Fecha_Aprobacion]
					   ,[Det_Fecha_Creacion]
					   ,[Det_Id_Usuario_Creacion]
					   ,[Det_Ip_Creacion]
					   ,[Det_Fecha_Modificacion]
					   ,[Det_Id_Usuario_Modificacion]
					   ,[Det_Ip_Modificacion]
					   ,[Det_Estado_Logico_Registro]
					   ,[Cod_CatalogoTareaSap]
					   ,[Det_Fecha_Aprobacion_Tarea]
					   ,[Det_Id_Usuario_Aprobacion_Tarea]
					   ,[Det_Ip_Aprobacion_Tarea]
					   ,[Det_Aprobacion_Tarea_Estado_QA]
					   ,[Det_Fecha_Aprobacion_Tarea_QA]
					   ,[Det_Id_Usuario_Aprobacion_Tarea_QA]
					   ,[Det_Ip_Aprobacion_Tarea_QA]
					   ,[IdTipoGasto]
					   ,[Det_Fecha_RegistraActividad])
			SELECT 
				   [Id_RegDetTareas]
				  ,[Id_RegTareas]
				  ,[Det_Num_OrdenServicio]
				  ,[Det_Id_CompAranda]
				  ,[Det_Fch_RegDetalleIni]
				  ,[Det_Fch_RegDetalleFin]
				  ,[Det_EstadoIni]
				  ,[Det_EstadoFin]
				  ,[Det_Tiempo]
				  ,[Det_Nom_Empresa]
				  ,[Det_Det_Tarea]
				  ,[Det_Estado]
				  ,[Id_Responsable]
				  ,[Det_Det_TareaFin]
				  ,[Det_Motivo_Cambio_Estado]
				  ,[IdDet_EstadoFin]
				  ,[IdDet_EstadoIni]
				  ,[Det_Observaciones]
				  ,[Det_Horas_Extras_Estado]
				  ,[Det_Horas_Extras_Tipo]
				  ,[Det_Horas_Extras_Descripcion]
				  ,[Det_Horas_Extras_Envio_Correo]
				  ,[Det_Aprobacion_Tarea_Estado]
				  ,[Det_Horas_Extras_Fecha_Solicitud]
				  ,[Det_Horas_Extras_Fecha_Aprobacion]
				  ,[Det_Fecha_Creacion]
				 
 ,[Det_Id_Usuario_Creacion]
				  ,[Det_Ip_Creacion]
				  ,[Det_Fecha_Modificacion]
				  ,[Det_Id_Usuario_Modificacion]
				  ,[Det_Ip_Modificacion]
				  ,[Det_Estado_Logico_Registro]
				  ,[Cod_CatalogoTareaSap]
				  ,[Det_Fecha_Aprobacion_Tarea]
				  ,[Det_Id_Usuario_Aprobacion_Tarea]
				  ,[Det_Ip_Aprobacion_Tarea]
				  ,[Det_Aprobacion_Tarea_Estado_QA]
				  ,[Det_Fecha_Aprobacion_Tarea_QA]
				  ,[Det_Id_Usuario_Aprobacion_Tarea_QA]
				  ,[Det_Ip_Aprobacion_Tarea_QA]
				  ,[IdTipoGasto]
				  ,[Det_Fecha_RegistraActividad]
			  FROM R_DetTareasAranda
			  where Id_RegDetTareas =@Id_RegDetTareas;

	       

			update [dbo].[R_DetTareasAranda]
			set 
				Id_RegTareas = @Id_RegTareas,
				Det_Num_OrdenServicio = @Det_Num_OrdenServicio,
				Det_Id_CompAranda = @Det_Id_CompAranda,
				Det_Det_Tarea = @Det_Det_Tarea,
				Det_Fch_RegDetalleIni = CONVERT(datetime, @Det_Fch_RegDetalleIni, 120),
				Det_Fch_RegDetalleFin = CONVERT(datetime, @Det_Fch_RegDetalleFin, 120),
				Det_Observaciones = @Det_Observaciones,
				Det_Tiempo = @Tiempo,
				Det_Horas_Extras_Tipo = @Det_Horas_Extras_Tipo,
				Det_Horas_Extras_Descripcion = (select Descripcion from Catalogo where IdTipoCatalogo = '7' and IdExterno = @Det_Horas_Extras_Tipo),
				Det_Id_Usuario_Modificacion = @IdUsuarioSession,
				Det_Ip_Modificacion = @Det_Ip_Modificacion,
				Det_Fecha_Modificacion = GETDATE(),
				Cod_CatalogoTareaSap = @Cod_CatalogoTareaSap
			where Id_RegDetTareas = @Id_RegDetTareas
	END
	ELSE
		BEGIN

			INSERT INTO [dbo].[R_TareasLog]
					   ([Id_RegDetTareas]
					   ,[Id_RegTareas]
					   ,[Det_Num_OrdenServicio]
					   ,[Det_Id_CompAranda]
					   ,[Det_Fch_RegDetalleIni]
					   ,[Det_Fch_RegDetalleFin]
					   ,[Det_EstadoI
ni]
					   ,[Det_EstadoFin]
					   ,[Det_Tiempo]
					   ,[Det_Nom_Empresa]
					   ,[Det_Det_Tarea]
					   ,[Det_Estado]
					   ,[Id_Responsable]
					   ,[Det_Det_TareaFin]
					   ,[Det_Motivo_Cambio_Estado]
					   ,[IdDet_EstadoFin]
					   ,[IdDet_EstadoIni]
					   ,[Det_Observaciones]
					   ,[Det_Horas_Extras_Estado]
					   ,[Det_Horas_Extras_Tipo]
					   ,[Det_Horas_Extras_Descripcion]
					   ,[Det_Horas_Extras_Envio_Correo]
					   ,[Det_Aprobacion_Tarea_Estado]
					   ,[Det_Horas_Extras_Fecha_Solicitud]
					   ,[Det_Horas_Extras_Fecha_Aprobacion]
					   ,[Det_Fecha_Creacion]
					   ,[Det_Id_Usuario_Creacion]
					   ,[Det_Ip_Creacion]
					   ,[Det_Fecha_Modificacion]
					   ,[Det_Id_Usuario_Modificacion]
					   ,[Det_Ip_Modificacion]
					   ,[Det_Estado_Logico_Registro]
					   ,[Cod_CatalogoTareaSap]
					   ,[Det_Fecha_Aprobacion_Tarea]
					   ,[Det_Id_Usuario_Aprobacion_Tarea]
					   ,[Det_Ip_Aprobacion_Tarea]
					   ,[Det_Aprobacion_Tarea_Estado_QA]
					   ,[Det_Fecha_Aprobacion_Tarea_QA]
					   ,[Det_Id_Usuario_Aprobacion_Tarea_QA]
					   ,[Det_Ip_Aprobacion_Tarea_QA]
					   ,[IdTipoGasto]
					   ,[Det_Fecha_RegistraActividad])
			SELECT 
				   [Id_RegDetTareas]
				  ,[Id_RegTareas]
				  ,[Det_Num_OrdenServicio]
				  ,[Det_Id_CompAranda]
				  ,[Det_Fch_RegDetalleIni]
				  ,[Det_Fch_RegDetalleFin]
				  ,[Det_EstadoIni]
				  ,[Det_EstadoFin]
				  ,[Det_Tiempo]
				  ,[Det_Nom_Empresa]
				  ,[Det_Det_Tarea]
				  ,[Det_Estado]
				  ,[Id_Responsable]
				  ,[Det_Det_TareaFin]
				  ,[Det_Motivo_Cambio_Estado]
				  ,[IdDet_EstadoFin]
				  ,[IdDet_EstadoIni]
				  ,[Det_Observaciones]
				  ,[Det_Horas_Extras_Estado]
				
  ,[Det_Horas_Extras_Tipo]
				  ,[Det_Horas_Extras_Descripcion]
				  ,[Det_Horas_Extras_Envio_Correo]
				  ,[Det_Aprobacion_Tarea_Estado]
				  ,[Det_Horas_Extras_Fecha_Solicitud]
				  ,[Det_Horas_Extras_Fecha_Aprobacion]
				  ,[Det_Fecha_Creacion]
				  ,[Det_Id_Usuario_Creacion]
				  ,[Det_Ip_Creacion]
				  ,[Det_Fecha_Modificacion]
				  ,[Det_Id_Usuario_Modificacion]
				  ,[Det_Ip_Modificacion]
				  ,[Det_Estado_Logico_Registro]
				  ,[Cod_CatalogoTareaSap]
				  ,[Det_Fecha_Aprobacion_Tarea]
				  ,[Det_Id_Usuario_Aprobacion_Tarea]
				  ,[Det_Ip_Aprobacion_Tarea]
				  ,[Det_Aprobacion_Tarea_Estado_QA]
				  ,[Det_Fecha_Aprobacion_Tarea_QA]
				  ,[Det_Id_Usuario_Aprobacion_Tarea_QA]
				  ,[Det_Ip_Aprobacion_Tarea_QA]
				  ,[IdTipoGasto]
				  ,[Det_Fecha_RegistraActividad]
			  FROM R_DetTareasAranda
			  where Id_RegDetTareas =@Id_RegDetTareas;



			update [dbo].[R_DetTareasAranda]
			set 
				Id_RegTareas = @Id_RegTareas,
				Det_Num_OrdenServicio = @Det_Num_OrdenServicio,
				Det_Id_CompAranda = @Det_Id_CompAranda,
				Det_Det_Tarea = @Det_Det_Tarea,
				Det_Fch_RegDetalleIni = CONVERT(datetime, @Det_Fch_RegDetalleIni, 120),
				Det_Fch_RegDetalleFin = CONVERT(datetime, @Det_Fch_RegDetalleFin, 120),
				Det_Observaciones = @Det_Observaciones,
				Det_Tiempo = @Tiempo,
				Det_Horas_Extras_Tipo = @Det_Horas_Extras_Tipo,
				Det_Horas_Extras_Descripcion = (select Descripcion from Catalogo where IdTipoCatalogo = '7' and IdExterno = @Det_Horas_Extras_Tipo),
				Det_Id_Usuario_Modificacion = @IdUsuarioSession,
				Det_Ip_Modificacion = @Det_Ip_Modificacion,
				Det_Fecha_Modificacion = GETDATE(),
				Cod_CatalogoTareaSap = @Cod_CatalogoTareaSap,
				IdTipoGasto=@IdTipoGasto
			where Id_RegDetTareas = @Id_RegDetTarea
s
		END
	IF(@@ROWCOUNT>=1)
		BEGIN
			SET @Respuestas = 1
		END
	ELSE
		BEGIN
			SET @Respuestas = 0
		END
		
	
	--CLOSE SYMMETRIC KEY CSYMMETRIC
	--------------------------------------------
	SELECT @Respuestas AS Respuestas,
	       CASE WHEN @Respuestas = 1 THEN N''
	            ELSE N'Ocurrio un error al guardar los datos.'
	       END AS Mensaje,
	       @Det_Horas_Extras_Tipo AS TipoHoraCalculado
	--------------------------------------------

	SET NOCOUNT OFF;	
	
END

GO

PRINT 'Sp_RTAActualizaDetalleTarea actualizado: la edicion ya clasifica el tipo de hora.';
GO
