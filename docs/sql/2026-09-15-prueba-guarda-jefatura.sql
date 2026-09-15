/* ============================================================================
   Demostracion de la guarda de jefatura. NO MODIFICA NADA.

   Todo corre dentro de una transaccion que se revierte al final. El script
   elige por si mismo un jefe real y uno de sus subordinados, y despues un
   tercero que NO le reporta, y comprueba que:

     1. Pedir el perfil de su propio subordinado devuelve una fila de cabecera.
     2. Pedir el perfil de alguien que no es suyo devuelve CERO filas.
     3. La lista de equipo del jefe no contiene a ese tercero.

   Es la verificacion numero 3 del diseno, que no se puede hacer con una
   prueba unitaria porque la guarda vive en SQL y necesita datos.
   ============================================================================ */

SET NOCOUNT ON;
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Jefe VARCHAR(50), @Suyo VARCHAR(50), @Ajeno VARCHAR(50);

    /* Se excluyen los codigos repetidos en los DOS extremos. Sin esto, el TOP 1
       elige al primero por orden alfabetico, que resulta ser uno de los usuarios
       con Cod_Usuario duplicado: el procedimiento se niega con razon -no puede
       saber de cual de dos personas son los datos- y el bloque de abajo sale
       vacio, como si la funcionalidad estuviera rota. */
    SELECT TOP 1 @Jefe = LTRIM(RTRIM(s.Cod_Jefe_Inm)), @Suyo = LTRIM(RTRIM(s.Cod_Usuario))
      FROM dbo.R_Usuarios s
      JOIN dbo.R_Usuarios u ON LTRIM(RTRIM(u.Cod_Usuario)) = LTRIM(RTRIM(s.Cod_Jefe_Inm))
                           AND ISNULL(u.EstadoUsuario,0) = 0
     WHERE ISNULL(s.EstadoUsuario,0) = 0
       AND (SELECT COUNT(*) FROM dbo.R_Usuarios r
             WHERE r.Cod_Usuario = s.Cod_Usuario AND ISNULL(r.EstadoUsuario,0) = 0) = 1
       AND (SELECT COUNT(*) FROM dbo.R_Usuarios r
             WHERE r.Cod_Usuario = s.Cod_Jefe_Inm AND ISNULL(r.EstadoUsuario,0) = 0) = 1
     ORDER BY s.Cod_Usuario;

    /* Alguien activo que NO le reporta a ese jefe y que no es el jefe mismo. */
    SELECT TOP 1 @Ajeno = LTRIM(RTRIM(x.Cod_Usuario))
      FROM dbo.R_Usuarios x
     WHERE ISNULL(x.EstadoUsuario,0) = 0
       AND LTRIM(RTRIM(ISNULL(x.Cod_Jefe_Inm,''))) <> @Jefe
       AND LTRIM(RTRIM(x.Cod_Usuario)) <> @Jefe
       AND (SELECT COUNT(*) FROM dbo.R_Usuarios r
             WHERE r.Cod_Usuario = x.Cod_Usuario AND ISNULL(r.EstadoUsuario,0) = 0) = 1
     ORDER BY x.Cod_Usuario;

    PRINT 'Jefe elegido, un subordinado suyo y un tercero ajeno. No se imprimen los codigos.';

    /* Por que aqui no se captura la salida de Sp_RTA_PerfilEquipo con
       INSERT ... EXEC: ese procedimiento devuelve SEIS result sets de formas
       distintas, y INSERT ... EXEC intenta meterlos todos en la misma tabla y
       falla. Por eso la guarda se saco a Fn_RTA_EsSubordinado: asi esta
       demostracion llama a la MISMA funcion que decide dentro del procedimiento,
       y no a una copia de su logica que podria divergir.

       Sp_RTA_PerfilEquipoLista si devuelve un unico result set, asi que ese si se
       captura de verdad. */

    /* Que cubren OK 1 y OK 2, y que no: aseveran sobre Fn_RTA_EsSubordinado, que
       es la condicion de "le reporta a este jefe". El procedimiento aplica ADEMAS
       una guarda de codigo de usuario repetido que esta funcion no conoce, asi que
       un OK aqui no garantiza por si solo que el procedimiento entregue datos. Por
       eso las dos llamadas de abajo se miran: son la unica forma de ver el
       comportamiento completo, porque un procedimiento de seis result sets de
       formas distintas no se puede capturar con INSERT ... EXEC. */

    /* --- 1. su propio subordinado: la guarda TIENE que decir que si --- */
    IF dbo.Fn_RTA_EsSubordinado(@Jefe, @Suyo) = 1
        PRINT 'OK 1: la guarda reconoce a su propio subordinado.';
    ELSE
        RAISERROR('FALLO 1: la guarda no reconoce a un subordinado real.', 16, 1);

    /* --- 2. alguien ajeno: la guarda TIENE que decir que no --- */
    IF dbo.Fn_RTA_EsSubordinado(@Jefe, @Ajeno) = 0
        PRINT 'OK 2: la guarda niega a quien no le reporta a este jefe.';
    ELSE
        RAISERROR('FALLO 2: la guarda de jefatura NO bloqueo a un usuario ajeno.', 16, 1);

    /* --- 2b. y el procedimiento completo, para verlo con los ojos ---
       Estas dos llamadas no se pueden aseverar desde T-SQL por lo dicho arriba,
       pero sqlcmd imprime sus result sets: la primera tiene que mostrar una
       cabecera con datos y la segunda, seis conjuntos vacios. Quien corra el
       script lo comprueba mirando. */
    PRINT '--- perfil de su propio subordinado (debe traer datos) ---';
    EXEC dbo.Sp_RTA_PerfilEquipo @Cod_Jefe = @Jefe, @Cod_Usuario = @Suyo;

    PRINT '--- perfil de alguien ajeno (los seis conjuntos deben venir vacios) ---';
    EXEC dbo.Sp_RTA_PerfilEquipo @Cod_Jefe = @Jefe, @Cod_Usuario = @Ajeno;

    /* --- 3. la lista del equipo no contiene al tercero --- */
    CREATE TABLE #Lista (CodUsuario VARCHAR(50), NombreCompleto VARCHAR(400),
                         Cargo VARCHAR(400), Area VARCHAR(400), Ciudad VARCHAR(400));

    INSERT INTO #Lista
    EXEC dbo.Sp_RTA_PerfilEquipoLista @Cod_Jefe = @Jefe, @Filtro = '';

    IF (SELECT COUNT(*) FROM #Lista WHERE CodUsuario = @Ajeno) = 0
        PRINT 'OK 3: la lista del equipo no contiene a quien no le reporta.';
    ELSE
        RAISERROR('FALLO 3: la lista del equipo incluyo a alguien que no es subordinado.', 16, 1);

    IF (SELECT COUNT(*) FROM #Lista WHERE CodUsuario = @Suyo) = 1
        PRINT 'OK 4: la lista del equipo si contiene al subordinado propio.';
    ELSE
        RAISERROR('FALLO 4: la lista del equipo no contiene a un subordinado real de este jefe.', 16, 1);

    IF (SELECT COUNT(*) FROM #Lista) > 0
        PRINT 'OK 5: la lista del equipo devolvio al menos una persona.';
    ELSE
        RAISERROR('FALLO 5: la lista del equipo de un jefe real vino vacia.', 16, 1);

    DROP TABLE #Lista;

    ROLLBACK TRANSACTION;
    PRINT 'Transaccion revertida. La base quedo exactamente como estaba.';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'Error, la transaccion quedo revertida: ' + ERROR_MESSAGE();
END CATCH
GO
