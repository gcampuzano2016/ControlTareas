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

/* 3. La visibilidad no se reescribe: para un usuario con sucursal filtrada, el
   total del resultado 1 debe coincidir con lo que ve directamente en
   Sp_RTAListaForeCast (filtrado a las etapas abiertas). '879' es un codigo de
   sucursal hard-codeado en Sp_RTAListaForeCast, no un dato personal. Solo
   lectura: tablas temporales, nada se escribe en ForeCast. Con dos resultados
   por EXEC no se puede comparar en la misma consulta, asi que se imprime el
   esperado y se compara a mano contra el total de filas de la EXEC de abajo. */
CREATE TABLE #v
(
    IdForeCast BIGINT,
    c02 NVARCHAR(MAX), c03 NVARCHAR(MAX), c04 NVARCHAR(MAX), c05 NVARCHAR(MAX),
    c06 NVARCHAR(MAX), c07 NVARCHAR(MAX), c08 NVARCHAR(MAX), c09 NVARCHAR(MAX),
    c10 NVARCHAR(MAX), c11 NVARCHAR(MAX), c12 NVARCHAR(MAX), c13 NVARCHAR(MAX),
    c14 NVARCHAR(MAX), c15 NVARCHAR(MAX), c16 NVARCHAR(MAX), c17 NVARCHAR(MAX),
    c18 NVARCHAR(MAX), c19 NVARCHAR(MAX), c20 NVARCHAR(MAX), c21 NVARCHAR(MAX),
    c22 NVARCHAR(MAX), c23 NVARCHAR(MAX), c24 NVARCHAR(MAX), c25 NVARCHAR(MAX),
    c26 NVARCHAR(MAX), c27 NVARCHAR(MAX), c28 NVARCHAR(MAX), c29 NVARCHAR(MAX),
    c30 NVARCHAR(MAX), c31 NVARCHAR(MAX), c32 NVARCHAR(MAX), c33 NVARCHAR(MAX),
    c34 NVARCHAR(MAX), c35 NVARCHAR(MAX), c36 NVARCHAR(MAX), c37 NVARCHAR(MAX),
    c38 NVARCHAR(MAX), c39 NVARCHAR(MAX), c40 NVARCHAR(MAX)
);
INSERT INTO #v
EXEC dbo.Sp_RTAListaForeCast @Idusuario = '879';

SELECT COUNT(*) AS EsperadoParaUsuario879   -- comparar contra el total de filas de la EXEC siguiente
  FROM dbo.ForeCast f
  INNER JOIN (SELECT DISTINCT IdForeCast FROM #v) v ON v.IdForeCast = f.IdForeCast
 WHERE f.Estado = 1
   AND f.Prioridad IN ('LEAD','PIPELINE','UPSIDE','FORECAST');

EXEC dbo.Sp_RTAListaCalificacionForeCast @Idusuario = '879';   -- contar las filas del resultado 1

DROP TABLE #v;
GO
