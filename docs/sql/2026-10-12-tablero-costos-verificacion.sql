/* ============================================================================
   Verificacion del Tablero de Costos. Solo lectura: lo unico que escribe va
   dentro de una transaccion que se revierte.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
GO

/* 1. El procedimiento devuelve los tres resultados (mirar que salgan tres
      grillas: actividades, tarifas, parametros). Rango corto a proposito. */
EXEC dbo.Sp_RTA_TableroCostos @Desde = '2026-09-01', @Hasta = '2026-09-30';
GO

/* 2. Volumen del rango del Excel de referencia. El Excel del 04-10-2026 tenia
      20.657 actividades; hoy puede haber algunas mas, nunca menos. */
SELECT COUNT(*) AS Actividades_2025_2026
  FROM dbo.R_DetTareasAranda
 WHERE Det_Fch_RegDetalleIni >= '2025-01-01'
   AND Det_Fch_RegDetalleIni <  '2027-01-01';
GO

/* 3. Indices cuya primera columna es Det_Fch_RegDetalleIni. Si no sale
      ninguno, avisar al usuario: el tablero funciona igual, pero cada consulta
      recorre la tabla entera. NO crear el indice desde este plan. */
SELECT i.name AS Indice, i.type_desc
  FROM sys.indexes AS i
  JOIN sys.index_columns AS ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
  JOIN sys.columns AS c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
 WHERE i.object_id = OBJECT_ID('dbo.R_DetTareasAranda')
   AND ic.key_ordinal = 1
   AND c.name = 'Det_Fch_RegDetalleIni';
GO

/* 4. Parametros que leera el tablero. Deben salir Factor50, Factor100 y DiasMes. */
SELECT Clave, Valor, FechaVigenciaDesde, FechaVigenciaHasta
  FROM dbo.HE_Parametro
 WHERE Clave IN ('Factor50', 'Factor100', 'DiasMes')
 ORDER BY Clave, FechaVigenciaDesde;
GO

/* 5. La auditoria devuelve un id, y no deja la fila. */
BEGIN TRANSACTION;
EXEC dbo.Sp_RTA_TableroCostosAuditar
     @CodUsuario = 'VERIFICA',
     @Accion     = 'Descargar',
     @Filtros    = N'verificacion del despliegue';
ROLLBACK TRANSACTION;
GO

/* 6. Quien ve la hoja de menu: deben salir solo 14 y 18. */
SELECT PM.IdPerfil, PM.Estado
  FROM dbo.PerfilMenu AS PM
  JOIN dbo.MenuDos AS M ON M.Id_Menu = PM.id_Menu
 WHERE M.Href = 'TableroCostos.aspx';
GO
