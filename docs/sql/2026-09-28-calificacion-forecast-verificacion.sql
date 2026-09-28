/* Verificacion de 2026-09-28-calificacion-forecast.sql. No deja datos: todo
   va dentro de una transaccion que se revierte. */
USE [ReporTarea];
GO
SET NOCOUNT ON;

/* 1. La lista devuelve dos resultados y solo abiertas */
EXEC dbo.Sp_RTAListaCalificacionForeCast @Idusuario = '';

/* 2. La vigente es la ultima, aun en el mismo segundo */
BEGIN TRANSACTION;
DECLARE @Id BIGINT = (SELECT TOP 1 IdForeCast FROM dbo.ForeCast
                       WHERE Estado = 1 AND Prioridad = 'PIPELINE' ORDER BY IdForeCast DESC);
EXEC dbo.Sp_RTAInsertaCalificacionForeCast @IdForeCast = @Id, @Presupuesto = 5,  @Autoridad = 5,  @Necesidad = 6,  @Usuario = 'verificacion';
EXEC dbo.Sp_RTAInsertaCalificacionForeCast @IdForeCast = @Id, @Presupuesto = 20, @Autoridad = 12, @Necesidad = 14, @Usuario = 'verificacion';
EXEC dbo.Sp_RTAInsertaCalificacionForeCast @IdForeCast = @Id, @Presupuesto = 7,  @Autoridad = 5,  @Necesidad = 6,  @Usuario = 'verificacion';  -- debe rechazar
SELECT TOP 1 Presupuesto, Autoridad, Necesidad FROM dbo.ForeCastCalificacion
 WHERE IdForeCast = @Id ORDER BY FechaRegistro DESC, IdCalificacion DESC;   -- debe ser 20, 12, 14
ROLLBACK TRANSACTION;
SELECT COUNT(*) AS FilasQueQuedaron FROM dbo.ForeCastCalificacion WHERE Usuario = 'verificacion';  -- debe ser 0
GO
