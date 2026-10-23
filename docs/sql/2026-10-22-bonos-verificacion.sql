/* ============================================================================
   Bonos de empleados - verificacion (solo lectura)
   Correr antes de publicar (despues de la migracion y del menu) y otra vez
   despues de publicar los binarios. Termina en VEREDICTO: OK o VEREDICTO: FALLA.
   Cada chequeo imprime una linea: OK, FALLA o PENDIENTE. Todo lo que no
   sea [OK] cuenta como falla: el veredicto solo da OK con la migracion y el
   menu ya aplicados.
   ============================================================================ */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

DECLARE @Fallos INT = 0;
DECLARE @Msg NVARCHAR(400);

/* 1. las 6 tablas */
DECLARE @Tablas TABLE (Nombre VARCHAR(60));
INSERT INTO @Tablas VALUES ('Bono_Tipo'), ('Bono_TipoConcepto'), ('Bono_Periodo'), ('Bono_Detalle'), ('Bono_DetalleSemana'), ('Bono_Bitacora');
DECLARE @TablasFalta INT = (SELECT COUNT(*) FROM @Tablas WHERE OBJECT_ID('dbo.' + Nombre, 'U') IS NULL);
IF @TablasFalta = 0 PRINT 'OK        1. las 6 tablas Bono_* existen';
ELSE BEGIN
    SET @Fallos += 1;
    SET @Msg = N'FALLA     1. faltan ' + CONVERT(NVARCHAR(10), @TablasFalta) + N' tablas Bono_*';
    PRINT @Msg;
END

/* 2. los 12 procedimientos */
DECLARE @Procs TABLE (Nombre VARCHAR(60));
INSERT INTO @Procs VALUES ('Sp_RTA_BonoPersonasActivas'), ('Sp_RTA_BonoTipos'), ('Sp_RTA_BonoGuardarTipo'), ('Sp_RTA_BonoPeriodos'),
    ('Sp_RTA_BonoAbrirPeriodo'), ('Sp_RTA_BonoCerrarPeriodo'), ('Sp_RTA_BonoDetalles'), ('Sp_RTA_BonoDePersona'),
    ('Sp_RTA_BonoGuardar'), ('Sp_RTA_BonoCambiarEstado'), ('Sp_RTA_BonoBorrar'), ('Sp_RTA_BonoBitacora');
DECLARE @ProcsFalta INT = (SELECT COUNT(*) FROM @Procs WHERE OBJECT_ID('dbo.' + Nombre, 'P') IS NULL);
IF @ProcsFalta = 0 PRINT 'OK        2. los 12 procedimientos Sp_RTA_Bono* existen';
ELSE BEGIN
    SET @Fallos += 1;
    SET @Msg = N'FALLA     2. faltan ' + CONVERT(NVARCHAR(10), @ProcsFalta) + N' procedimientos Sp_RTA_Bono*';
    PRINT @Msg;
END

IF @TablasFalta <> 0
BEGIN
    SET @Fallos += 1;
    PRINT 'FALLA     3 a 7. no se pueden comprobar: falta el esquema (docs/sql/2026-10-22-bonos-esquema.sql)';
    PRINT 'PENDIENTE 8. menu: no se comprueba sin esquema';
END
ELSE
BEGIN
    /* 3. septiembre 2026: Cerrado, suma 3195,00 */
    DECLARE @Sep DECIMAL(11,2) = (SELECT ISNULL(SUM(d.Valor), 0) FROM dbo.Bono_Detalle d
                                    JOIN dbo.Bono_Periodo p ON p.IdPeriodo = d.IdPeriodo
                                   WHERE p.Anio = 2026 AND p.Mes = 9 AND d.Estado = 'Aprobado');
    DECLARE @SepCerrado INT = (SELECT COUNT(*) FROM dbo.Bono_Periodo WHERE Anio = 2026 AND Mes = 9 AND Estado = 'Cerrado');
    IF @SepCerrado = 0
    BEGIN
        SET @Fallos += 1;
        PRINT 'PENDIENTE 3. septiembre 2026: pendiente: falta la migracion (no existe como periodo Cerrado)';
    END
    ELSE IF @Sep <> 3195.00
    BEGIN
        SET @Fallos += 1;
        SET @Msg = N'FALLA     3. septiembre 2026 suma ' + CONVERT(NVARCHAR(20), @Sep) + N' y debe sumar 3195.00';
        PRINT @Msg;
    END
    ELSE PRINT 'OK        3. septiembre 2026 cerrado y suma 3195.00';

    /* 4. 17 historicos con total */
    DECLARE @Hist INT = (SELECT COUNT(*) FROM dbo.Bono_Periodo
                          WHERE Estado = 'Historico' AND TotalHistorico IS NOT NULL
                            AND Anio * 100 + Mes BETWEEN 202504 AND 202608);
    IF @Hist = 0
    BEGIN
        SET @Fallos += 1;
        PRINT 'PENDIENTE 4. historicos abril 2025 - agosto 2026: pendiente: falta la migracion (0 de 17)';
    END
    ELSE IF @Hist <> 17
    BEGIN
        SET @Fallos += 1;
        SET @Msg = N'FALLA     4. hay ' + CONVERT(NVARCHAR(10), @Hist) + N' periodos historicos con total; se esperaban 17';
        PRINT @Msg;
    END
    ELSE PRINT 'OK        4. 17 periodos historicos con total';

    /* 5. ningun doble pago entre los no rechazados */
    DECLARE @Doble INT = (SELECT COUNT(*) FROM (
        SELECT d.CodUsuario, d.IdTipo, s.Lunes
          FROM dbo.Bono_DetalleSemana s
          JOIN dbo.Bono_Detalle d ON d.IdBono = s.IdBono
         WHERE d.Estado <> 'Rechazado'
         GROUP BY d.CodUsuario, d.IdTipo, s.Lunes HAVING COUNT(*) > 1) AS x);
    IF @Doble = 0 PRINT 'OK        5. ningun doble pago (persona + tipo + semana)';
    ELSE BEGIN
        SET @Fallos += 1;
        SET @Msg = N'FALLA     5. ' + CONVERT(NVARCHAR(10), @Doble) + N' semanas pagadas dos veces';
        PRINT @Msg;
    END

    /* 6. cada tipo activo tiene concepto DOS y AGILITY */
    DECLARE @TiposActivos INT = (SELECT COUNT(*) FROM dbo.Bono_Tipo WHERE Activo = 1);
    DECLARE @SinConcepto INT = (SELECT COUNT(*) FROM dbo.Bono_Tipo t
                                 WHERE t.Activo = 1
                                   AND (SELECT COUNT(*) FROM dbo.Bono_TipoConcepto k
                                         WHERE k.IdTipo = t.IdTipo AND k.Empresa IN ('DOS', 'AGILITY')
                                           AND LTRIM(RTRIM(k.CodigoConcepto)) <> '') <> 2);
    IF @TiposActivos = 0
    BEGIN
        SET @Fallos += 1;
        PRINT 'PENDIENTE 6. conceptos: pendiente: falta la migracion (no hay tipos en el catalogo)';
    END
    ELSE IF @SinConcepto <> 0
    BEGIN
        SET @Fallos += 1;
        SET @Msg = N'FALLA     6. ' + CONVERT(NVARCHAR(10), @SinConcepto) + N' tipos activos sin concepto DOS o AGILITY';
        PRINT @Msg;
    END
    ELSE PRINT 'OK        6. los ' + CONVERT(VARCHAR(10), @TiposActivos) + ' tipos activos tienen concepto DOS y AGILITY';

    /* 7. bonos migrados con sus copias completas */
    DECLARE @Copias INT = (SELECT COUNT(*) FROM dbo.Bono_Detalle
                            WHERE Origen = 'Migracion'
                              AND (ISNULL(Empresa, '') NOT IN ('DOS', 'AGILITY') OR ISNULL(Nombre, N'') = N''
                                   OR ISNULL(Identificacion, '') = '' OR ISNULL(NombreTipo, N'') = N''
                                   OR ISNULL(CodigoConcepto, '') = ''));
    IF @Copias = 0 PRINT 'OK        7. los bonos migrados traen empresa, nombre, identificacion, tipo y concepto';
    ELSE BEGIN
        SET @Fallos += 1;
        SET @Msg = N'FALLA     7. ' + CONVERT(NVARCHAR(10), @Copias) + N' bonos migrados sin alguna de sus copias';
        PRINT @Msg;
    END

    /* 8. menu */
    DECLARE @IdProponer INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'BonosProponer.aspx');
    DECLARE @IdRevisar  INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'BonosRevisar.aspx');
    DECLARE @IdCatalogo INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'BonosCatalogo.aspx');
    DECLARE @IdReporte  INT = (SELECT MIN(Id_Menu) FROM dbo.MenuDos WHERE LTRIM(RTRIM(Href)) = 'BonosReporte.aspx');
    DECLARE @IdGrupo    INT = (SELECT Id_MenuPadre FROM dbo.MenuDos WHERE Id_Menu = @IdProponer);

    IF @IdProponer IS NULL AND @IdRevisar IS NULL AND @IdCatalogo IS NULL AND @IdReporte IS NULL
    BEGIN
        SET @Fallos += 1;
        PRINT 'PENDIENTE 8. menu: pendiente: falta correr 2026-10-22-bonos-menu.sql (comprobacion de visibilidad omitida)';
    END
    ELSE IF @IdProponer IS NULL OR @IdRevisar IS NULL OR @IdCatalogo IS NULL OR @IdReporte IS NULL OR @IdGrupo IS NULL
    BEGIN
        SET @Fallos += 1;
        PRINT 'FALLA     8. menu: faltan hojas o grupo (esperadas 4 hojas con un mismo grupo Bonos)';
    END
    ELSE
    BEGIN
        DECLARE @Todos TABLE (IdPerfil INT PRIMARY KEY);
        INSERT INTO @Todos SELECT DISTINCT Id_Perfil FROM dbo.R_Usuarios
         WHERE Usuario_Estado = 'A' AND EstadoUsuario IS NULL AND ISNULL(IdCliente, 0) = 0 AND Id_Perfil IS NOT NULL;
        IF NOT EXISTS (SELECT 1 FROM @Todos WHERE IdPerfil = 18) INSERT INTO @Todos VALUES (18);

        DECLARE @E TABLE (IdMenu INT, IdPerfil INT);
        INSERT INTO @E SELECT @IdGrupo, IdPerfil FROM @Todos;
        INSERT INTO @E SELECT @IdProponer, IdPerfil FROM @Todos;
        INSERT INTO @E VALUES (@IdRevisar, 14), (@IdRevisar, 18), (@IdCatalogo, 14), (@IdCatalogo, 18), (@IdReporte, 14), (@IdReporte, 18);

        DECLARE @Dif TABLE (Diferencia VARCHAR(20), IdMenu INT, IdPerfil INT);
        ;WITH R AS (SELECT pm.id_Menu, pm.IdPerfil FROM dbo.PerfilMenu AS pm
                     WHERE pm.Estado = '0' AND pm.id_Menu IN (@IdGrupo, @IdProponer, @IdRevisar, @IdCatalogo, @IdReporte))
        INSERT INTO @Dif (Diferencia, IdMenu, IdPerfil)
        SELECT CASE WHEN r.id_Menu IS NULL THEN 'FALTA en el menu' ELSE 'SOBRA en el menu' END,
               ISNULL(e.IdMenu, r.id_Menu), ISNULL(e.IdPerfil, r.IdPerfil)
          FROM @E AS e FULL JOIN R AS r ON r.id_Menu = e.IdMenu AND r.IdPerfil = e.IdPerfil
         WHERE e.IdMenu IS NULL OR r.id_Menu IS NULL;

        IF EXISTS (SELECT 1 FROM @Dif)
        BEGIN
            SET @Fallos += 1;
            PRINT 'FALLA     8. menu: la visibilidad no es la esperada; diferencias abajo';
            SELECT * FROM @Dif ORDER BY IdMenu, IdPerfil;
        END
        ELSE PRINT 'OK        8. menu: grupo Bonos y 4 hojas con la visibilidad esperada (Proponer: todos + 18; resto: 14 y 18)';
    END
END

PRINT '';
SELECT Veredicto = CASE WHEN @Fallos = 0 THEN 'VEREDICTO: OK' ELSE 'VEREDICTO: FALLA' END, Fallos = @Fallos;
IF @Fallos = 0 PRINT 'VEREDICTO: OK'; ELSE PRINT 'VEREDICTO: FALLA (' + CONVERT(VARCHAR(10), @Fallos) + ' comprobaciones)';
