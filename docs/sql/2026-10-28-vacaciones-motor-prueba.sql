/* ============================================================================
   Vacaciones - prueba del motor con personas ficticias (se deshace sola)
   ReporTarea  |  2026-10-28
   Correr despues de 2026-10-28-vacaciones-esquema.sql y -motor.sql.
   Todo va dentro de BEGIN TRAN ... ROLLBACK: no deja filas. Los resultados se
   guardan en variables de tabla, que el ROLLBACK no borra.
   Imprime PASS / FAIL por comprobacion y termina en VEREDICTO: OK o FALLA.

   La prueba que compara con el Excel de GTH (la de oro) la arma
   generar-carga-vacaciones.py y no se versiona, porque trae datos personales.
   Esta fija cada regla con tres personas ficticias, calculadas a mano al
   2026-03-01. Las fechas elegidas no tienen feriados.

   ZZ-A  ingreso 2015-03-02, 10 anios cumplidos, sin corte
         2025-06-02 a 06-06 VACACIONES 5 dias: termina en viernes, +2   -> 7
         2025-08-04 PERMISO 4 h: 4 / 8                                     -> 0.5
         2025-09-01 VACACIONES 3 dias DADA BAJA                            -> 0
         2025-12-26 VACACIONES 1 dia viernes, Beneficio DOS: sin cargo     -> 1
         Periodos 1-7 vencidos (15x5 + 16 + 17 = 108). El 8 (18 dias) absorbe
         los 8.5 y vence el 2026-03-02: POR VENCER 9.5. Vigentes 9.5+19+20.
         Proporcional del 11 (21 dias): 21 x 364 / 365 = 20.94.
   ZZ-B  ingreso 2025-01-06, 1 anio cumplido
         2025-08-04 a 08-24 VACACIONES 21 dias (con 3 domingos dentro)    -> 21
         Periodo 1 gozado (15), el 2 ya usa 6: ANTICIPO 2.22 - 6 = -3.78.
   ZZ-C  ingreso 2020-05-04, corte 2023-05-04 con saldo 10
         Ajuste = 45 ganados al corte - 10 = 35: periodos 1 y 2 gozados, 3 con 10.
         2019-12-02 antes del ingreso (reingreso)  -> RELACION ANTERIOR
         2023-01-02 antes del corte                -> ANTES DEL CORTE
         Periodo 6 = 16 dias; proporcional 16 x 301 / 365 = 13.19.
   Sin tildes a proposito.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

DECLARE @Fallos INT = 0, @A INT, @B INT, @C INT, @F DATE = '20260301';

DECLARE @RSaldo TABLE (IdColaborador INT, EstadoColaborador VARCHAR(10), FechaCalculo DATE, Cumplidos INT, DiasPorAnio DECIMAL(12,4),
    InicioPeriodo DATE, ProximoAniversario DATE, AjusteCorte DECIMAL(12,4), Disponible DECIMAL(12,4), UsadoEnCurso DECIMAL(12,4),
    Proporcional DECIMAL(12,4), Saldo DECIMAL(12,2), PorVencer DECIMAL(12,4), ProximoVencimiento DATE, Vencidos DECIMAL(12,4),
    TotalGozado DECIMAL(12,4), Estado VARCHAR(12));
DECLARE @RPer TABLE (IdColaborador INT, N INT, Periodo VARCHAR(9), Inicio DATE, Fin DATE, DiasGanados DECIMAL(12,4),
    DiasTomados DECIMAL(12,4), DiasVencidos DECIMAL(12,4), Saldo DECIMAL(12,4), Vence DATE, Estado VARCHAR(20));
DECLARE @RMov TABLE (IdMovimiento INT, IdColaborador INT, Tipo VARCHAR(25), Estado VARCHAR(15), Desde DATE, Hasta DATE,
    Situacion VARCHAR(20), AnioServicio INT, DiasEquivalentes DECIMAL(12,4), DiasBase DECIMAL(12,4),
    DiasFinDeSemana DECIMAL(12,4), DiasDescuenta DECIMAL(12,4), Cruces INT);

BEGIN TRANSACTION;

INSERT INTO dbo.Vac_Colaborador (Cedula, Nombre, FechaIngreso, Estado, Usuario_Registro)
VALUES ('ZZPRUEBA-A', N'ZZ PRUEBA A', '20150302', 'ACTIVO', 'prueba');
SET @A = SCOPE_IDENTITY();
INSERT INTO dbo.Vac_Colaborador (Cedula, Nombre, FechaIngreso, Estado, Usuario_Registro)
VALUES ('ZZPRUEBA-B', N'ZZ PRUEBA B', '20250106', 'ACTIVO', 'prueba');
SET @B = SCOPE_IDENTITY();
INSERT INTO dbo.Vac_Colaborador (Cedula, Nombre, FechaIngreso, Estado, FechaCorte, SaldoCorte, Usuario_Registro)
VALUES ('ZZPRUEBA-C', N'ZZ PRUEBA C', '20200504', 'ACTIVO', '20230504', 10, 'prueba');
SET @C = SCOPE_IDENTITY();

INSERT INTO dbo.Vac_Movimiento (IdColaborador, Tipo, Estado, FechaDesde, FechaHasta, Dias, Horas, Minutos, BeneficioDos, Origen, Orden, Usuario_Registro)
VALUES (@A, 'VACACIONES',        'APROBADO',  '20250602', '20250606', 5,    NULL, NULL, 0, 'SISTEMA', 1, 'prueba'),
       (@A, 'PERMISO POR HORAS', 'APROBADO',  '20250804', '20250804', NULL, 4,    0,    0, 'SISTEMA', 2, 'prueba'),
       (@A, 'VACACIONES',        'DADA BAJA', '20250901', '20250903', 3,    NULL, NULL, 0, 'SISTEMA', 3, 'prueba'),
       (@A, 'VACACIONES',        'APROBADO',  '20251226', '20251226', 1,    NULL, NULL, 1, 'SISTEMA', 4, 'prueba'),
       (@B, 'VACACIONES',        'APROBADO',  '20250804', '20250824', 21,   NULL, NULL, 0, 'SISTEMA', 5, 'prueba'),
       (@C, 'VACACIONES',        'HISTORICO', '20191202', '20191206', 5,    NULL, NULL, 0, 'SISTEMA', 6, 'prueba'),
       (@C, 'VACACIONES',        'APROBADO',  '20230102', '20230111', 10,   NULL, NULL, 0, 'SISTEMA', 7, 'prueba');

INSERT INTO @RMov EXEC dbo.Sp_RTA_VacCalcular @FechaCalculo = @F, @Salida = 'MOVIMIENTOS';
INSERT INTO @RPer EXEC dbo.Sp_RTA_VacCalcular @FechaCalculo = @F, @Salida = 'PERIODOS';
INSERT INTO @RSaldo EXEC dbo.Sp_RTA_VacCalcular @FechaCalculo = @F, @Salida = 'SALDOS';

ROLLBACK;

/* ---------------------------------------------------- movimientos ------- */
IF EXISTS (SELECT 1 FROM @RMov WHERE IdColaborador = @A AND Desde = '20250602' AND DiasFinDeSemana = 2 AND DiasDescuenta = 7)
     PRINT 'PASS 1: termina en viernes, carga sabado y domingo (5 + 2).'
ELSE BEGIN PRINT 'FAIL 1: la solicitud que termina en viernes deberia descontar 7.'; SET @Fallos += 1; END

IF EXISTS (SELECT 1 FROM @RMov WHERE IdColaborador = @A AND Tipo = 'PERMISO POR HORAS' AND DiasDescuenta = 0.5)
     PRINT 'PASS 2: el permiso de 4 horas descuenta 0.5 dias.'
ELSE BEGIN PRINT 'FAIL 2: el permiso de 4 horas deberia descontar 0.5.'; SET @Fallos += 1; END

IF EXISTS (SELECT 1 FROM @RMov WHERE IdColaborador = @A AND Estado = 'DADA BAJA' AND Situacion = 'ANULADO' AND DiasDescuenta = 0)
     PRINT 'PASS 3: lo dado de baja no descuenta.'
ELSE BEGIN PRINT 'FAIL 3: lo dado de baja deberia quedar ANULADO y en 0.'; SET @Fallos += 1; END

IF EXISTS (SELECT 1 FROM @RMov WHERE IdColaborador = @A AND Desde = '20251226' AND DiasDescuenta = 1)
     PRINT 'PASS 4: Beneficio DOS en viernes no carga fin de semana.'
ELSE BEGIN PRINT 'FAIL 4: el Beneficio DOS del viernes deberia descontar 1.'; SET @Fallos += 1; END

IF EXISTS (SELECT 1 FROM @RMov WHERE IdColaborador = @C AND Desde = '20191202' AND Situacion = 'RELACION ANTERIOR' AND DiasDescuenta = 0)
   AND EXISTS (SELECT 1 FROM @RMov WHERE IdColaborador = @C AND Desde = '20230102' AND Situacion = 'ANTES DEL CORTE' AND DiasDescuenta = 0)
     PRINT 'PASS 5: lo anterior al reingreso y al corte no descuenta.'
ELSE BEGIN PRINT 'FAIL 5: reingreso o corte mal clasificados.'; SET @Fallos += 1; END

/* -------------------------------------------------------- periodos ------- */
IF (SELECT DiasGanados FROM @RPer WHERE IdColaborador = @A AND N = 1) = 15
   AND (SELECT DiasGanados FROM @RPer WHERE IdColaborador = @A AND N = 6) = 16
   AND (SELECT DiasGanados FROM @RPer WHERE IdColaborador = @A AND N = 11) = 21
     PRINT 'PASS 6: 15 dias por anio y uno mas por cada anio despues del quinto.'
ELSE BEGIN PRINT 'FAIL 6: dias por periodo mal calculados.'; SET @Fallos += 1; END

IF (SELECT COUNT(*) FROM @RPer WHERE IdColaborador = @A AND Estado = 'VENCIDO') = 7
   AND EXISTS (SELECT 1 FROM @RPer WHERE IdColaborador = @A AND N = 8 AND Estado = 'VIGENTE' AND DiasTomados = 8.5 AND Saldo = 9.5 AND Vence = '20260302')
     PRINT 'PASS 7: vencen los periodos 1 a 7; el 8 absorbe lo tomado y vence en el aniversario 11.'
ELSE BEGIN PRINT 'FAIL 7: vencimiento o consumo del periodo mas antiguo mal calculados.'; SET @Fallos += 1; END

/* ---------------------------------------------------------- saldos ------- */
IF EXISTS (SELECT 1 FROM @RSaldo WHERE IdColaborador = @A AND DiasPorAnio = 21 AND Disponible = 48.5 AND Vencidos = 108
           AND Proporcional = 20.94 AND Saldo = 69.44 AND PorVencer = 9.5 AND Estado = 'POR VENCER' AND TotalGozado = 8.5)
     PRINT 'PASS 8: saldo de ZZ-A = 48.5 disponibles + 20.94 proporcional = 69.44, POR VENCER.'
ELSE BEGIN PRINT 'FAIL 8: saldo de ZZ-A distinto de lo esperado.'; SET @Fallos += 1; SELECT * FROM @RSaldo WHERE IdColaborador = @A; END

IF EXISTS (SELECT 1 FROM @RSaldo WHERE IdColaborador = @B AND Disponible = 0 AND UsadoEnCurso = 6 AND Proporcional = 2.22
           AND Saldo = -3.78 AND Estado = 'ANTICIPO')
     PRINT 'PASS 9: ZZ-B queda en anticipo: 2.22 - 6 = -3.78.'
ELSE BEGIN PRINT 'FAIL 9: saldo de ZZ-B distinto de lo esperado.'; SET @Fallos += 1; SELECT * FROM @RSaldo WHERE IdColaborador = @B; END

IF EXISTS (SELECT 1 FROM @RSaldo WHERE IdColaborador = @C AND AjusteCorte = 35 AND Disponible = 40 AND Proporcional = 13.19
           AND Saldo = 53.19 AND PorVencer = 10 AND Estado = 'POR VENCER')
     PRINT 'PASS 10: ZZ-C parte del saldo de corte: 10 + 15 + 15 + 13.19 = 53.19.'
ELSE BEGIN PRINT 'FAIL 10: saldo de ZZ-C distinto de lo esperado.'; SET @Fallos += 1; SELECT * FROM @RSaldo WHERE IdColaborador = @C; END

PRINT CASE WHEN @Fallos = 0 THEN 'VEREDICTO: OK' ELSE 'VEREDICTO: FALLA (' + CONVERT(VARCHAR(10), @Fallos) + ')' END;
