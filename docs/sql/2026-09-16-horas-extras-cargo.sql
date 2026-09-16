/* ============================================================================
   El cargo del colaborador.

   La grilla de la fase 2 lo muestra y HE_Detalle tiene CargoSnapshot, pero no
   habia de donde leerlo: Empleados no tiene esa columna, y la de R_Usuarios
   exige unir por cedula -que seis usuarios activos comparten-. La plantilla si
   lo trae, lleno en las 64 filas, asi que se le da destino aqui.
   ============================================================================ */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

DECLARE @Fallos INT = 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_ColaboradorParametro') AND name = 'Cargo')
BEGIN
    ALTER TABLE dbo.HE_ColaboradorParametro ADD Cargo VARCHAR(200) NULL;
    PRINT 'Columna Cargo agregada.';
END
ELSE
    PRINT 'Columna Cargo ya existia.';

IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.HE_ColaboradorParametro') AND name = 'Cargo')
BEGIN
    RAISERROR('FALLO: la columna Cargo no quedo creada.', 16, 1);
    SET @Fallos += 1;
END

IF @Fallos > 0
    RAISERROR('FALLO: %d verificaciones no pasaron.', 16, 1, @Fallos);
ELSE
    PRINT 'Horas Extras: la columna Cargo esta lista. Falta volver a correr la carga para llenarla.';
GO
