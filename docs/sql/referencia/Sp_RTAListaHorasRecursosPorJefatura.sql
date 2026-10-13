/* ============================================================================
   COPIA DE REFERENCIA - NO CORRER
   Sp_RTAListaHorasRecursosPorJefatura, tal como esta en produccion
   ReporTarea  |  capturado el 2026-09-21

   Este archivo NO es un script de despliegue. Es una foto del procedimiento
   VIGENTE, traido de la base para que quede versionado. No lo corras: no
   cambia nada y solo puede pisar la version buena.

   ----------------------------------------------------------------------------
   Por que esta aca

   docs/sql/2026-09-21-dashboard-aprobacion.sql replica de este procedimiento el
   parseo de Det_Tiempo, el umbral de CumpleJornada y cuatro codigos escritos a
   mano -1314, 1171, 222692 y 1655906-. Esa replica es deliberada: es lo que
   hace que el dashboard y la tabla de APROBADAS den el mismo numero para el
   mismo rango.

   El problema es que hasta hoy la mitad replicada no estaba en el repositorio,
   asi que la advertencia no le podia llegar a nadie: quien tocara este
   procedimiento en la base no tenia forma de enterarse de que el dashboard
   depende de el. Con esta copia, un diff contra la base muestra la divergencia.

   Si cambias el procedimiento de produccion, revisa el SP del dashboard y
   volve a capturar este archivo.

   Creado 2022-09-16, modificado por ultima vez 2024-09-13. 6.905 caracteres.
   ============================================================================ */

--- Sp_RTAListaHorasRecursosPorJefatura 1171,'01/04/2020','15/04/2020'
CREATE PROCEDURE [dbo].[Sp_RTAListaHorasRecursosPorJefatura]
				@IdUsuarioJefe varchar(10),
				@FechaInicio varchar(20),
				@FechaFin varchar(20),
				@idEstado int=0
AS
BEGIN
	SET NOCOUNT ON;
    DECLARE @esJefe INT
	DECLARE @IdUsuarioJefe2 INT
	SELECT @IdUsuarioJefe2=Cod_Jefe_Inm FROM R_Usuarios where Cod_Usuario =@IdUsuarioJefe

	SET @esJefe = dbo.Fn_ConsultaUsuarioEsJefe(@IdUsuarioJefe2)
	declare @query varchar(8000);
	SELECT @query=''

if(@IdUsuarioJefe='1314' OR @IdUsuarioJefe='1171' OR @IdUsuarioJefe= '222692' OR @IdUsuarioJefe= '1655906')
	BEGIN	

	SELECT @query = @query + ' SELECT ' +char(13)
	SELECT @query = @query + '	a.Id_Responsable AS ''Id'' '+char(13)
	SELECT @query = @query + ', u.Nom_Usuario AS ''NombreResponsable''' +char(13)
	SELECT @query = @query + ', FORMAT(a.Det_Fch_RegDetalleIni, ''yyyy-MM-dd'') AS ''FechaTareas''' +char(13)
			--,(sum(CONVERT(INT,SUBSTRING(a.Det_Tiempo,1,2)))*60+SUM(CONVERT(INT,SUBSTRING(a.Det_Tiempo,4,2)))) AS 'TIEMPO EN MINUTOS'
	SELECT @query = @query + ', (SELECT top 1 Descripcion FROM Catalogo WHERE IdTipoCatalogo = ''8'' AND IdExterno = DATEPART(weekday, FORMAT(a.Det_Fch_RegDetalleIni, ''yyyy-MM-dd''))) AS ''Dia'' ' +char(13)
	SELECT @query = @query + ',dbo.Fn_RTA_SumaTotalHoras(a.Id_Responsable, FORMAT(a.Det_Fch_RegDetalleIni, ''dd-MM-yyyy''),FORMAT(a.Det_Fch_RegDetalleIni, ''dd-MM-yyyy'')) AS ''TotalHorasDiarias'' ' +char(13)
	SELECT @query = @query + ',CASE '+char(13)  
	SELECT @query = @query + ' WHEN ((sum(CONVERT(INT,SUBSTRING(ISNULL(a.Det_Tiempo, 0),1,2)))*60+SUM(CONVERT(INT,SUBSTRING(ISNULL(a.Det_Tiempo, 0),4,2))))/60) <  8 THEN ''NO''  '+char(13)  
	SELECT @query = @query + ' ELSE ''SI'' '+char(13)   
	SELECT @query = @query + ' END  AS ''CumpleJornada'' '+char(13)   
	SELECT @query = @query + ',CASE '+char(13)    
	SELECT @query = @query + ' WHEN ((sum(CONVERT(INT,SUBSTRING(ISNULL(a.Det_Tiempo, 0),1,2)))*60+SUM(CONVERT(INT,SUBSTRING(ISNULL(a.Det_Tiempo, 0),4,2))))/60) <  8 THEN ''#F2DEDE'' '+char(13)    
	SELECT @query = @query + ' ELSE ''#DFF0D8'' '+char(13)    
	SELECT @query = @query + ' END  AS ''BgColorJornada'' '+char(13)  
	SELECT @query = @query + ',LTRIM(RTRIM(' + CONVERT(CHAR(18),@esJefe) + '))  as ''Seleccionar'' '+char(13)  
	SELECT @query = @query + ' FROM R_DetTareasAranda a '+char(13)  
	SELECT @query = @query + ' INNER JOIN R_Usuarios u ON a.Id_Responsable = u.Cod_Usuario '+char(13)  
	SELECT @query = @query + '	WHERE  '+char(13)  
	SELECT @query = @query + ' CONVERT(date,a.Det_Fch_RegDetalleIni,105) >= CONVERT(DATE,''' + CONVERT(CHAR(10),@FechaInicio) + ''',105) ' + CHAR(13)
	SELECT @query = @query + ' AND CONVERT(date,a.Det_Fch_RegDetalleIni,105) <= CONVERT(DATE,''' + CONVERT(CHAR(10),@FechaFin) + ''',105) ' + CHAR(13)
	IF(@idEstado > 0)
			BEGIN
				SELECT @query = @query + ' AND a.Det_Aprobacion_Tarea_Estado =LTRIM(RTRIM(' + CONVERT(CHAR(18),@idEstado) + ')) ' + CHAR(13)
			END	
	--convert(date,a.Det_Fch_RegDetalleIni,105) >= convert(date,@FechaInicio,105)
	--AND convert(date,a.Det_Fch_RegDetalleIni,105) <=  convert(date,@FechaFin,105)
	SELECT @query = @query + ' GROUP BY '+char(13)  
	SELECT @query = @query + ' a.Id_Responsable, u.Nom_Usuario '+char(13)  
	SELECT @query = @query + ', FORMAT(a.Det_Fch_RegDetalleIni, ''yyyy-MM-dd'') '+char(13)  
	SELECT @query = @query + ', FORMAT(a.Det_Fch_RegDetalleIni, ''dd-MM-yyyy'') '+char(13)  
	SELECT @query = @query + '  ORDER BY '+char(13)   
	SELECT @query = @query + '	u.Nom_Usuario '+char(13) 
	SELECT @query = @query + ', FORMAT(a.Det_Fch_RegDetalleIni, ''yyyy-MM-dd''); '+char(13)  
	EXEC(@query)
	PRINT @query

	END
ELSE 
	BEGIN	

	SELECT @query = @query + '	SELECT ' +char(13) 
	SELECT @query = @query + '	a.Id_Responsable AS ''Id'' ' +char(13) 
	SELECT @query = @query + ', u.Nom_Usuario AS ''NombreResponsable'' ' +char(13) 
	SELECT @query = @query + ', FORMAT(a.Det_Fch_RegDetalleIni, ''yyyy-MM-dd'') AS ''FechaTareas'' ' +char(13)
			--,(sum(CONVERT(INT,SUBSTRING(a.Det_Tiempo,1,2)))*60+SUM(CONVERT(INT,SUBSTRING(a.Det_Tiempo,4,2)))) AS 'TIEMPO EN MINUTOS'
	SELECT @query = @query + ', (SELECT top 1 Descripcion FROM Catalogo WHERE IdTipoCatalogo = ''8'' AND IdExterno = DATEPART(weekday, FORMAT(a.Det_Fch_RegDetalleIni, ''yyyy-MM-dd''))) AS ''Dia'' ' +char(13) 
	SELECT @query = @query + ',dbo.Fn_RTA_SumaTotalHoras(a.Id_Responsable, FORMAT(a.Det_Fch_RegDetalleIni, ''dd-MM-yyyy''),FORMAT(a.Det_Fch_RegDetalleIni, ''dd-MM-yyyy'')) AS ''TotalHorasDiarias'' ' +char(13) 
	SELECT @query = @query + ',CASE ' +char(13)   
	SELECT @query = @query + 'WHEN ((sum(CONVERT(INT,SUBSTRING(ISNULL(a.Det_Tiempo, 0),1,2)))*60+SUM(CONVERT(INT,SUBSTRING(ISNULL(a.Det_Tiempo, 0),4,2))))/60) <  8 THEN ''NO'' ' +char(13) 
	SELECT @query = @query + 'ELSE ''SI'' ' +char(13)    
	SELECT @query = @query + 'END  AS ''CumpleJornada''  ' +char(13)   
	SELECT @query = @query + ',CASE ' +char(13)    
	SELECT @query = @query + 'WHEN ((sum(CONVERT(INT,SUBSTRING(ISNULL(a.Det_Tiempo, 0),1,2)))*60+SUM(CONVERT(INT,SUBSTRING(ISNULL(a.Det_Tiempo, 0),4,2))))/60) <  8 THEN ''#F2DEDE''  ' +char(13)
	SELECT @query = @query + 'ELSE ''#DFF0D8'' ' +char(13)      
	SELECT @query = @query + 'END  AS ''BgColorJornada'' ' +char(13)   
	SELECT @query = @query + ',LTRIM(RTRIM(' + CONVERT(CHAR(18),@esJefe) + ')) as ''Seleccionar'' ' +char(13)   
	SELECT @query = @query + 'FROM R_DetTareasAranda a ' +char(13) 
	SELECT @query = @query + 'INNER JOIN R_Usuarios u ON a.Id_Responsable = u.Cod_Usuario ' +char(13)
	SELECT @query = @query + '	WHERE ' +char(13)
	SELECT @query = @query + ' ( a.Id_Responsable =LTRIM(RTRIM(' + CONVERT(CHAR(18),@IdUsuarioJefe) + ')) OR u.Cod_Jefe_Inm =LTRIM(RTRIM(' + CONVERT(CHAR(18),@IdUsuarioJefe) + '))) ' + CHAR(13)

	SELECT @query = @query + ' AND CONVERT(date,a.Det_Fch_RegDetalleIni,105) >= CONVERT(DATE,''' + CONVERT(CHAR(10),@FechaInicio) + ''',105) ' + CHAR(13)
	SELECT @query = @query + ' AND CONVERT(date,a.Det_Fch_RegDetalleIni,105) <= CONVERT(DATE,''' + CONVERT(CHAR(10),@FechaFin) + ''',105) ' + CHAR(13)
	IF(@idEstado > 0)
			BEGIN
				SELECT @query = @query + ' AND a.Det_Aprobacion_Tarea_Estado =LTRIM(RTRIM(' + CONVERT(CHAR(18),@idEstado) + ')) ' + CHAR(13)
			END	
	SELECT @query = @query + ' GROUP BY '+char(13)  
	SELECT @query = @query + ' a.Id_Responsable, u.Nom_Usuario '+char(13)  
	SELECT @query = @query + ', FORMAT(a.Det_Fch_RegDetalleIni, ''yyyy-MM-dd'') '+char(13)  
	SELECT @query = @query + ', FORMAT(a.Det_Fch_RegDetalleIni, ''dd-MM-yyyy'') '+char(13)  
	SELECT @query = @query + '  ORDER BY '+char(13)   
	SELECT @query = @query + '	u.Nom_Usuario '+char(13) 
	SELECT @query = @query + ', FORMAT(a.Det_Fch_RegDetalleIni, ''yyyy-MM-dd''); '+char(13) 
	
	EXEC(@query)
	PRINT @query

	END

	
	SET NOCOUNT OFF;	
END
