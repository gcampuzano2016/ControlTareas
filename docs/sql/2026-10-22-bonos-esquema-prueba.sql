/* ============================================================================
   Bonos de empleados - prueba manual del esquema (se deshace sola)
   ReporTarea  |  2026-10-22
   Correr UNA vez, despues de 2026-10-22-bonos-esquema.sql.
   Todo va dentro de BEGIN TRAN ... ROLLBACK: no deja filas. Lo unico que no
   vuelve atras son los contadores IDENTITY de las tablas Bono_* (el proximo
   Id puede saltar algunos numeros); no afecta a nada.
   Imprime PASS / FAIL por comprobacion y termina en VEREDICTO: OK o FALLA.
   Si un procedimiento falla con error, XACT_ABORT deshace todo y el script
   se corta: tambien es FALLA.
   Usa una persona ficticia (ZZPRUEBA) y un tipo ficticio; no lee datos
   reales salvo para saber si ya hay un periodo abierto.
   Sin tildes a proposito.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

DECLARE @Fallos INT = 0;
DECLARE @R TABLE (Respuestas INT);
DECLARE @Rc INT, @IdTipo INT, @IdPeriodo INT, @IdPeriodo2 INT, @PeriodoPropio BIT = 0,
        @B1 INT, @B2 INT, @B3 INT, @Bit0 INT, @Bit1 INT;

/* Conteos antes, para comprobar al final que no quedo nada. */
DECLARE @Antes TABLE (Tabla SYSNAME, Filas INT);
INSERT INTO @Antes
SELECT 'Bono_Tipo', COUNT(*) FROM dbo.Bono_Tipo UNION ALL
SELECT 'Bono_TipoConcepto', COUNT(*) FROM dbo.Bono_TipoConcepto UNION ALL
SELECT 'Bono_Periodo', COUNT(*) FROM dbo.Bono_Periodo UNION ALL
SELECT 'Bono_Detalle', COUNT(*) FROM dbo.Bono_Detalle UNION ALL
SELECT 'Bono_DetalleSemana', COUNT(*) FROM dbo.Bono_DetalleSemana UNION ALL
SELECT 'Bono_Bitacora', COUNT(*) FROM dbo.Bono_Bitacora;

BEGIN TRANSACTION;

SET @Bit0 = (SELECT COUNT(*) FROM dbo.Bono_Bitacora);

/* 1. Tipo de prueba, por semana, SIN conceptos SAP. */
INSERT INTO @R EXEC dbo.Sp_RTA_BonoGuardarTipo
    @IdTipo = 0, @Nombre = N'ZZ Prueba esquema bonos', @Familia = 'Standby', @ValorSugerido = 50,
    @PorSemana = 1, @Activo = 1, @ConceptoDos = NULL, @ConceptoAgility = '', @Usuario = 'prueba';
SELECT @IdTipo = Respuestas FROM @R; DELETE FROM @R;
IF @IdTipo > 0 PRINT 'PASS 1: GuardarTipo crea el tipo de prueba.'
ELSE BEGIN PRINT 'FAIL 1: GuardarTipo deberia devolver un IdTipo > 0.'; SET @Fallos += 1; END

/* 2. Mismo nombre (otro espaciado) -> -1. */
INSERT INTO @R EXEC dbo.Sp_RTA_BonoGuardarTipo
    @IdTipo = 0, @Nombre = N'  ZZ Prueba esquema bonos ', @Familia = 'Otro', @ValorSugerido = NULL,
    @PorSemana = 0, @Activo = 1, @ConceptoDos = NULL, @ConceptoAgility = NULL, @Usuario = 'prueba';
SELECT @Rc = Respuestas FROM @R; DELETE FROM @R;
IF @Rc = -1 PRINT 'PASS 2: GuardarTipo con nombre repetido devuelve -1.'
ELSE BEGIN PRINT 'FAIL 2: GuardarTipo con nombre repetido devolvio ' + ISNULL(CONVERT(VARCHAR(12), @Rc), 'NULL') + '.'; SET @Fallos += 1; END

/* 3. Periodo abierto: se abre uno de prueba, o se usa el que ya hay. */
SELECT @IdPeriodo = IdPeriodo FROM dbo.Bono_Periodo WHERE Estado = 'Abierto';
IF @IdPeriodo IS NULL
BEGIN
    INSERT INTO @R EXEC dbo.Sp_RTA_BonoAbrirPeriodo @Usuario = 'prueba';
    SELECT @IdPeriodo = Respuestas FROM @R; DELETE FROM @R;
    SET @PeriodoPropio = 1;
    IF @IdPeriodo > 0 AND EXISTS (SELECT 1 FROM dbo.Bono_Periodo WHERE IdPeriodo = @IdPeriodo AND Estado = 'Abierto')
        PRINT 'PASS 3: AbrirPeriodo abre un periodo de prueba.'
    ELSE BEGIN PRINT 'FAIL 3: AbrirPeriodo deberia devolver el Id de un periodo Abierto.'; SET @Fallos += 1; END
END
ELSE
    PRINT 'AVISO 3: ya habia un periodo abierto (' + CONVERT(VARCHAR(12), @IdPeriodo) + '); la prueba lo usa y todo se deshace al final.';

/* 4. Con uno abierto, abrir otro -> -1. */
INSERT INTO @R EXEC dbo.Sp_RTA_BonoAbrirPeriodo @Usuario = 'prueba';
SELECT @Rc = Respuestas FROM @R; DELETE FROM @R;
IF @Rc = -1 PRINT 'PASS 4: AbrirPeriodo con uno abierto devuelve -1.'
ELSE BEGIN PRINT 'FAIL 4: AbrirPeriodo con uno abierto devolvio ' + ISNULL(CONVERT(VARCHAR(12), @Rc), 'NULL') + '.'; SET @Fallos += 1; END

/* 5. Primer bono: lunes 2026-08-03 y 2026-08-10. */
INSERT INTO @R EXEC dbo.Sp_RTA_BonoGuardar
    @IdBono = 0, @CodUsuario = 'ZZPRUEBA', @Empresa = NULL, @Nombre = NULL, @Identificacion = NULL,
    @Lugar = N'Quito', @IdTipo = @IdTipo, @Semanas = 2, @ValorSugerido = 100, @Valor = 100,
    @MotivoAjuste = NULL, @Observacion = N'prueba', @Origen = 'Jefe', @CodJefe = 'ZZJEFE',
    @Lunes = '2026-08-03,2026-08-10', @EstadoEsperado = NULL, @EstadoNuevo = 'Borrador', @Usuario = 'prueba';
SELECT @B1 = Respuestas FROM @R; DELETE FROM @R;
IF @B1 > 0
   AND (SELECT COUNT(*) FROM dbo.Bono_DetalleSemana WHERE IdBono = @B1) = 2
   AND EXISTS (SELECT 1 FROM dbo.Bono_Detalle WHERE IdBono = @B1 AND IdPeriodo = @IdPeriodo AND Estado = 'Borrador')
    PRINT 'PASS 5: Guardar crea el bono 1 en el periodo abierto con sus 2 lunes.'
ELSE BEGIN PRINT 'FAIL 5: Guardar no creo bien el bono 1 (' + ISNULL(CONVERT(VARCHAR(12), @B1), 'NULL') + ').'; SET @Fallos += 1; END

/* 6. Segundo bono, misma persona, mismo tipo, comparte el 2026-08-10 -> -3. */
INSERT INTO @R EXEC dbo.Sp_RTA_BonoGuardar
    @IdBono = 0, @CodUsuario = 'ZZPRUEBA', @Empresa = NULL, @Nombre = NULL, @Identificacion = NULL,
    @Lugar = N'Quito', @IdTipo = @IdTipo, @Semanas = 2, @ValorSugerido = 100, @Valor = 100,
    @MotivoAjuste = NULL, @Observacion = NULL, @Origen = 'Jefe', @CodJefe = 'ZZJEFE',
    @Lunes = '2026-08-10,2026-08-17', @EstadoEsperado = NULL, @EstadoNuevo = 'Borrador', @Usuario = 'prueba';
SELECT @Rc = Respuestas FROM @R; DELETE FROM @R;
IF @Rc = -3 AND NOT EXISTS (SELECT 1 FROM dbo.Bono_DetalleSemana s JOIN dbo.Bono_Detalle d ON d.IdBono = s.IdBono
                             WHERE d.CodUsuario = 'ZZPRUEBA' AND s.Lunes = '20260817')
    PRINT 'PASS 6: el segundo bono con un lunes repetido devuelve -3 y no escribe.'
ELSE BEGIN PRINT 'FAIL 6: el segundo bono con un lunes repetido devolvio ' + ISNULL(CONVERT(VARCHAR(12), @Rc), 'NULL') + '.'; SET @Fallos += 1; END

/* 7. Segundo bono con otra semana -> se crea. */
INSERT INTO @R EXEC dbo.Sp_RTA_BonoGuardar
    @IdBono = 0, @CodUsuario = 'ZZPRUEBA', @Empresa = NULL, @Nombre = NULL, @Identificacion = NULL,
    @Lugar = N'Quito', @IdTipo = @IdTipo, @Semanas = 1, @ValorSugerido = 50, @Valor = 50,
    @MotivoAjuste = NULL, @Observacion = NULL, @Origen = 'Jefe', @CodJefe = 'ZZJEFE',
    @Lunes = '2026-08-17', @EstadoEsperado = NULL, @EstadoNuevo = 'Borrador', @Usuario = 'prueba';
SELECT @B2 = Respuestas FROM @R; DELETE FROM @R;
IF @B2 > 0 PRINT 'PASS 7: un bono con otra semana se crea.'
ELSE BEGIN PRINT 'FAIL 7: un bono con otra semana devolvio ' + ISNULL(CONVERT(VARCHAR(12), @B2), 'NULL') + '.'; SET @Fallos += 1; END

/* 8. Cambiar estado con @EstadoEsperado equivocado -> -1. */
INSERT INTO @R EXEC dbo.Sp_RTA_BonoCambiarEstado
    @IdBono = @B1, @EstadoEsperado = 'Enviado', @EstadoNuevo = 'Aprobado', @Usuario = 'prueba';
SELECT @Rc = Respuestas FROM @R; DELETE FROM @R;
IF @Rc = -1 AND EXISTS (SELECT 1 FROM dbo.Bono_Detalle WHERE IdBono = @B1 AND Estado = 'Borrador')
    PRINT 'PASS 8: CambiarEstado con estado esperado equivocado devuelve -1 y no cambia nada.'
ELSE BEGIN PRINT 'FAIL 8: CambiarEstado con estado esperado equivocado devolvio ' + ISNULL(CONVERT(VARCHAR(12), @Rc), 'NULL') + '.'; SET @Fallos += 1; END

/* 9. Guardar (editar) con @EstadoEsperado equivocado -> -1. */
INSERT INTO @R EXEC dbo.Sp_RTA_BonoGuardar
    @IdBono = @B1, @CodUsuario = 'ZZPRUEBA', @Empresa = NULL, @Nombre = NULL, @Identificacion = NULL,
    @Lugar = N'Quito', @IdTipo = @IdTipo, @Semanas = 2, @ValorSugerido = 100, @Valor = 90,
    @MotivoAjuste = N'x', @Observacion = NULL, @Origen = 'Jefe', @CodJefe = 'ZZJEFE',
    @Lunes = '2026-08-03,2026-08-10', @EstadoEsperado = 'Rechazado', @EstadoNuevo = 'Borrador', @Usuario = 'prueba';
SELECT @Rc = Respuestas FROM @R; DELETE FROM @R;
IF @Rc = -1 AND EXISTS (SELECT 1 FROM dbo.Bono_Detalle WHERE IdBono = @B1 AND Valor = 100)
    PRINT 'PASS 9: Guardar con estado esperado equivocado devuelve -1 y no cambia nada.'
ELSE BEGIN PRINT 'FAIL 9: Guardar con estado esperado equivocado devolvio ' + ISNULL(CONVERT(VARCHAR(12), @Rc), 'NULL') + '.'; SET @Fallos += 1; END

/* 10. Enviar (con la copia de la persona) y aprobar el bono 1. */
INSERT INTO @R EXEC dbo.Sp_RTA_BonoCambiarEstado
    @IdBono = @B1, @EstadoEsperado = 'Borrador', @EstadoNuevo = 'Enviado',
    @SnapshotEmpresa = 'dos', @SnapshotNombre = N'PERSONA DE PRUEBA', @SnapshotIdentificacion = '0000', @Usuario = 'prueba';
SELECT @Rc = Respuestas FROM @R; DELETE FROM @R;
INSERT INTO @R EXEC dbo.Sp_RTA_BonoCambiarEstado
    @IdBono = @B1, @EstadoEsperado = 'Enviado', @EstadoNuevo = 'Aprobado', @Usuario = 'prueba';
SELECT @Rc = ISNULL(@Rc, 0) + Respuestas FROM @R; DELETE FROM @R;
IF @Rc = 0 AND EXISTS (SELECT 1 FROM dbo.Bono_Detalle WHERE IdBono = @B1 AND Estado = 'Aprobado'
                          AND Empresa = 'DOS' AND Nombre = N'PERSONA DE PRUEBA' AND UsuarioRevisa = 'prueba')
    PRINT 'PASS 10: enviar y aprobar devuelven 0; la copia queda con la empresa normalizada.'
ELSE BEGIN PRINT 'FAIL 10: enviar/aprobar el bono 1 no quedo como se esperaba.'; SET @Fallos += 1; END

/* 11. Rechazar el bono 2: un rechazado ya no bloquea su semana. */
INSERT INTO @R EXEC dbo.Sp_RTA_BonoCambiarEstado
    @IdBono = @B2, @EstadoEsperado = 'Borrador', @EstadoNuevo = 'Rechazado', @Motivo = N'semana equivocada', @Usuario = 'prueba';
SELECT @Rc = Respuestas FROM @R; DELETE FROM @R;
INSERT INTO @R EXEC dbo.Sp_RTA_BonoGuardar
    @IdBono = 0, @CodUsuario = 'ZZPRUEBA', @Empresa = NULL, @Nombre = NULL, @Identificacion = NULL,
    @Lugar = N'Quito', @IdTipo = @IdTipo, @Semanas = 1, @ValorSugerido = 50, @Valor = 50,
    @MotivoAjuste = NULL, @Observacion = NULL, @Origen = 'Jefe', @CodJefe = 'ZZJEFE',
    @Lunes = '2026-08-17', @EstadoEsperado = NULL, @EstadoNuevo = 'Borrador', @Usuario = 'prueba';
SELECT @B3 = Respuestas FROM @R; DELETE FROM @R;
IF @Rc = 0 AND @B3 > 0 AND EXISTS (SELECT 1 FROM dbo.Bono_Detalle WHERE IdBono = @B2 AND MotivoRechazo = N'semana equivocada')
    PRINT 'PASS 11: rechazar devuelve 0 y la semana del rechazado se puede volver a usar.'
ELSE BEGIN PRINT 'FAIL 11: rechazo del bono 2 / nuevo bono 3 (' + ISNULL(CONVERT(VARCHAR(12), @B3), 'NULL') + ').'; SET @Fallos += 1; END

/* 12. Volver a enviar el rechazado (su semana ya la tomo el bono 3) -> -3. */
INSERT INTO @R EXEC dbo.Sp_RTA_BonoCambiarEstado
    @IdBono = @B2, @EstadoEsperado = 'Rechazado', @EstadoNuevo = 'Enviado', @Usuario = 'prueba';
SELECT @Rc = Respuestas FROM @R; DELETE FROM @R;
IF @Rc = -3 PRINT 'PASS 12: CambiarEstado hacia un estado no rechazado con un lunes tomado devuelve -3.'
ELSE BEGIN PRINT 'FAIL 12: CambiarEstado con lunes tomado devolvio ' + ISNULL(CONVERT(VARCHAR(12), @Rc), 'NULL') + '.'; SET @Fallos += 1; END

/* 13. Cerrar con un Aprobado cuyo tipo no tiene concepto -> -2, sin cambios. */
INSERT INTO @R EXEC dbo.Sp_RTA_BonoCerrarPeriodo @IdPeriodo = @IdPeriodo, @Usuario = 'prueba';
SELECT @Rc = Respuestas FROM @R; DELETE FROM @R;
IF @Rc = -2 AND EXISTS (SELECT 1 FROM dbo.Bono_Periodo WHERE IdPeriodo = @IdPeriodo AND Estado = 'Abierto')
    PRINT 'PASS 13: CerrarPeriodo con un tipo sin concepto devuelve -2 y el periodo sigue abierto.'
ELSE BEGIN PRINT 'FAIL 13: CerrarPeriodo sin concepto devolvio ' + ISNULL(CONVERT(VARCHAR(12), @Rc), 'NULL') + '.'; SET @Fallos += 1; END

/* 14. Ponerle concepto DOS al tipo (editar) y cerrar. */
INSERT INTO @R EXEC dbo.Sp_RTA_BonoGuardarTipo
    @IdTipo = @IdTipo, @Nombre = N'ZZ Prueba esquema bonos', @Familia = 'Standby', @ValorSugerido = 50,
    @PorSemana = 1, @Activo = 1, @ConceptoDos = ' 1m99 ', @ConceptoAgility = NULL, @Usuario = 'prueba';
SELECT @Rc = Respuestas FROM @R; DELETE FROM @R;
IF @Rc = @IdTipo AND EXISTS (SELECT 1 FROM dbo.Bono_TipoConcepto WHERE IdTipo = @IdTipo AND Empresa = 'DOS' AND CodigoConcepto = '1M99')
    PRINT 'PASS 14: GuardarTipo edita y guarda el concepto normalizado.'
ELSE BEGIN PRINT 'FAIL 14: GuardarTipo no guardo el concepto DOS.'; SET @Fallos += 1; END

INSERT INTO @R EXEC dbo.Sp_RTA_BonoCerrarPeriodo @IdPeriodo = @IdPeriodo, @Usuario = 'prueba';
SELECT @Rc = Respuestas FROM @R; DELETE FROM @R;
IF @Rc = -2 AND @PeriodoPropio = 0
    PRINT 'AVISO 15: el periodo abierto real tiene otros Aprobados sin concepto; se omiten 15 a 17.'
ELSE IF @Rc = 0
     AND EXISTS (SELECT 1 FROM dbo.Bono_Periodo WHERE IdPeriodo = @IdPeriodo AND Estado = 'Cerrado'
                    AND FechaCierre IS NOT NULL AND UsuarioCierre = 'prueba')
     AND EXISTS (SELECT 1 FROM dbo.Bono_Detalle WHERE IdBono = @B1 AND IdPeriodo = @IdPeriodo
                    AND NombreTipo = N'ZZ Prueba esquema bonos' AND CodigoConcepto = '1M99')
     AND EXISTS (SELECT 1 FROM dbo.Bono_Detalle WHERE IdBono = @B2 AND IdPeriodo IS NULL AND Estado = 'Rechazado')
     AND EXISTS (SELECT 1 FROM dbo.Bono_Detalle WHERE IdBono = @B3 AND IdPeriodo IS NULL AND Estado = 'Borrador'
                    AND NombreTipo IS NULL)
BEGIN
    PRINT 'PASS 15: CerrarPeriodo cierra, copia NombreTipo/CodigoConcepto al Aprobado y deja los demas sin periodo.';

    /* 16. Cerrar otra vez -> -1; editar un bono del mes cerrado -> -2. */
    INSERT INTO @R EXEC dbo.Sp_RTA_BonoCerrarPeriodo @IdPeriodo = @IdPeriodo, @Usuario = 'prueba';
    SELECT @Rc = Respuestas FROM @R; DELETE FROM @R;
    DECLARE @Rc2 INT;
    INSERT INTO @R EXEC dbo.Sp_RTA_BonoCambiarEstado
        @IdBono = @B1, @EstadoEsperado = 'Aprobado', @EstadoNuevo = 'Rechazado', @Motivo = N'x', @Usuario = 'prueba';
    SELECT @Rc2 = Respuestas FROM @R; DELETE FROM @R;
    IF @Rc = -1 AND @Rc2 = -2 PRINT 'PASS 16: cerrar dos veces devuelve -1 y tocar un bono del mes cerrado devuelve -2.'
    ELSE BEGIN PRINT 'FAIL 16: cerrar otra vez devolvio ' + ISNULL(CONVERT(VARCHAR(12), @Rc), 'NULL')
                     + ', tocar el cerrado devolvio ' + ISNULL(CONVERT(VARCHAR(12), @Rc2), 'NULL') + '.'; SET @Fallos += 1; END

    /* 17. Abrir el mes siguiente engancha los pendientes. */
    INSERT INTO @R EXEC dbo.Sp_RTA_BonoAbrirPeriodo @Usuario = 'prueba';
    SELECT @IdPeriodo2 = Respuestas FROM @R; DELETE FROM @R;
    IF @IdPeriodo2 > 0
       AND EXISTS (SELECT 1 FROM dbo.Bono_Periodo p1 JOIN dbo.Bono_Periodo p2 ON p2.IdPeriodo = @IdPeriodo2
                    WHERE p1.IdPeriodo = @IdPeriodo
                      AND p2.Anio * 12 + p2.Mes = p1.Anio * 12 + p1.Mes + 1)
       AND (SELECT COUNT(*) FROM dbo.Bono_Detalle WHERE IdBono IN (@B2, @B3) AND IdPeriodo = @IdPeriodo2) = 2
       AND NOT EXISTS (SELECT 1 FROM dbo.Bono_Detalle WHERE IdPeriodo IS NULL)
        PRINT 'PASS 17: AbrirPeriodo abre el mes siguiente y engancha los pendientes.'
    ELSE BEGIN PRINT 'FAIL 17: AbrirPeriodo despues del cierre devolvio ' + ISNULL(CONVERT(VARCHAR(12), @IdPeriodo2), 'NULL') + '.'; SET @Fallos += 1; END
END
ELSE BEGIN PRINT 'FAIL 15: CerrarPeriodo con conceptos completos devolvio ' + ISNULL(CONVERT(VARCHAR(12), @Rc), 'NULL') + ' o no dejo los datos esperados.'; SET @Fallos += 1; END

/* 18. Borrar: estado esperado equivocado -> -1; correcto -> 0. */
INSERT INTO @R EXEC dbo.Sp_RTA_BonoBorrar @IdBono = @B3, @EstadoEsperado = 'Enviado', @Usuario = 'prueba';
SELECT @Rc = Respuestas FROM @R; DELETE FROM @R;
DECLARE @Rc3 INT;
INSERT INTO @R EXEC dbo.Sp_RTA_BonoBorrar @IdBono = @B3, @EstadoEsperado = 'Borrador', @Usuario = 'prueba';
SELECT @Rc3 = Respuestas FROM @R; DELETE FROM @R;
IF @Rc = -1 AND @Rc3 = 0
   AND NOT EXISTS (SELECT 1 FROM dbo.Bono_Detalle WHERE IdBono = @B3)
   AND NOT EXISTS (SELECT 1 FROM dbo.Bono_DetalleSemana WHERE IdBono = @B3)
    PRINT 'PASS 18: Borrar con estado equivocado devuelve -1; con el correcto borra el bono y sus lunes.'
ELSE BEGIN PRINT 'FAIL 18: Borrar devolvio ' + ISNULL(CONVERT(VARCHAR(12), @Rc), 'NULL') + ' / ' + ISNULL(CONVERT(VARCHAR(12), @Rc3), 'NULL') + '.'; SET @Fallos += 1; END

/* 19. Bitacora: los procedimientos que escriben dejaron su fila; la suelta tambien. */
EXEC dbo.Sp_RTA_BonoBitacora @IdBono = @B1, @IdPeriodo = @IdPeriodo, @Accion = 'Correo',
     @ValorAnterior = NULL, @ValorNuevo = 'prueba@ejemplo', @Detalle = N'error de prueba', @Usuario = 'prueba';
SET @Bit1 = (SELECT COUNT(*) FROM dbo.Bono_Bitacora);
IF EXISTS (SELECT 1 FROM dbo.Bono_Bitacora WHERE IdBono = @B1 AND Accion = 'Crear')
   AND EXISTS (SELECT 1 FROM dbo.Bono_Bitacora WHERE IdBono = @B1 AND Accion = 'Enviar')
   AND EXISTS (SELECT 1 FROM dbo.Bono_Bitacora WHERE IdBono = @B1 AND Accion = 'Aprobar')
   AND EXISTS (SELECT 1 FROM dbo.Bono_Bitacora WHERE IdBono = @B2 AND Accion = 'Rechazar')
   AND EXISTS (SELECT 1 FROM dbo.Bono_Bitacora WHERE IdBono = @B3 AND Accion = 'Borrar')
   AND EXISTS (SELECT 1 FROM dbo.Bono_Bitacora WHERE IdBono = @B1 AND Accion = 'Correo')
   AND EXISTS (SELECT 1 FROM dbo.Bono_Bitacora WHERE (IdPeriodo = @IdPeriodo AND Accion = 'Cerrar') OR @PeriodoPropio = 0)
   AND EXISTS (SELECT 1 FROM dbo.Bono_Bitacora WHERE Accion = 'Catalogo' AND ValorNuevo = N'ZZ Prueba esquema bonos')
   AND NOT EXISTS (SELECT 1 FROM dbo.Bono_Bitacora WHERE Fecha IS NULL)
    PRINT 'PASS 19: la bitacora tiene una fila por accion (' + CONVERT(VARCHAR(12), @Bit1 - @Bit0) + ' filas nuevas).'
ELSE BEGIN PRINT 'FAIL 19: faltan filas en Bono_Bitacora.'; SET @Fallos += 1; END

/* 20. Lecturas: corren sin error (sus resultados se descartan). */
DECLARE @Per TABLE (IdPeriodo INT, Anio INT, Mes INT, Estado VARCHAR(10), TotalHistorico DECIMAL(11,2),
                    FechaApertura DATETIME, FechaCierre DATETIME, UsuarioApertura VARCHAR(50),
                    UsuarioCierre VARCHAR(50), TotalAprobado DECIMAL(11,2));
INSERT INTO @Per EXEC dbo.Sp_RTA_BonoPeriodos;
DECLARE @Pers TABLE (CodUsuario VARCHAR(50), Nombre VARCHAR(100), Correo VARCHAR(100), CodJefe VARCHAR(100),
                     Empresa VARCHAR(50), CodSap VARCHAR(50), Cedula VARCHAR(32), IdPerfil INT);
INSERT INTO @Pers EXEC dbo.Sp_RTA_BonoPersonasActivas;
DECLARE @NPers INT = (SELECT COUNT(*) FROM @Pers);
IF (SELECT TotalAprobado FROM @Per WHERE IdPeriodo = @IdPeriodo) >= 100
   AND (SELECT COUNT(*) FROM @Pers) = (SELECT COUNT(DISTINCT CodUsuario) FROM @Pers)
    PRINT 'PASS 20: Periodos trae TotalAprobado y PersonasActivas no repite CodUsuario ('
          + CONVERT(VARCHAR(12), @NPers) + ' personas).'
ELSE BEGIN PRINT 'FAIL 20: Periodos o PersonasActivas no devolvieron lo esperado.'; SET @Fallos += 1; END

ROLLBACK TRANSACTION;

/* 21. No quedo nada. */
DECLARE @Despues TABLE (Tabla SYSNAME, Filas INT);
INSERT INTO @Despues
SELECT 'Bono_Tipo', COUNT(*) FROM dbo.Bono_Tipo UNION ALL
SELECT 'Bono_TipoConcepto', COUNT(*) FROM dbo.Bono_TipoConcepto UNION ALL
SELECT 'Bono_Periodo', COUNT(*) FROM dbo.Bono_Periodo UNION ALL
SELECT 'Bono_Detalle', COUNT(*) FROM dbo.Bono_Detalle UNION ALL
SELECT 'Bono_DetalleSemana', COUNT(*) FROM dbo.Bono_DetalleSemana UNION ALL
SELECT 'Bono_Bitacora', COUNT(*) FROM dbo.Bono_Bitacora;
IF @@TRANCOUNT = 0
   AND NOT EXISTS (SELECT 1 FROM @Antes a JOIN @Despues d ON d.Tabla = a.Tabla WHERE d.Filas <> a.Filas)
   AND NOT EXISTS (SELECT 1 FROM dbo.Bono_Detalle WHERE CodUsuario = 'ZZPRUEBA')
   AND NOT EXISTS (SELECT 1 FROM dbo.Bono_Tipo WHERE Nombre = N'ZZ Prueba esquema bonos')
    PRINT 'PASS 21: el ROLLBACK no dejo ninguna fila.'
ELSE BEGIN PRINT 'FAIL 21: quedaron filas o una transaccion abierta.'; SET @Fallos += 1; END

IF @Fallos = 0
    PRINT 'VEREDICTO: OK';
ELSE
    PRINT 'VEREDICTO: FALLA (' + CONVERT(VARCHAR(10), @Fallos) + ' comprobaciones). Revisar arriba.';
GO
