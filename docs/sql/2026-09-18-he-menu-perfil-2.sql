/* ============================================================================
   Horas extras: mostrarle la pantalla al perfil 2
   ReporTarea  |  2026-09-18

   PENDIENTE DE EJECUTAR.

   ----------------------------------------------------------------------------
   Da de alta al perfil 2 -Supervisor Especialistas, 6 usuarios activos- en las
   dos filas de menu que hacen falta para que "Horas Extras" se dibuje:

     20082  la opcion "Horas Extras"      (Href = HorasExtras.aspx)
     20081  su grupo padre "Nomina"

   Las DOS. Sin la fila del padre la opcion hija no se dibuja, aunque este bien
   registrada: el menu arma primero los grupos que le corresponden al perfil y
   solo despues cuelga de ellos las opciones.

   ----------------------------------------------------------------------------
   OJO con el Estado: la semantica esta INVERTIDA

   En PerfilMenu, Estado = '0' MUESTRA la opcion y '1' la OCULTA. No es un error
   de tipeo ni una convencion olvidada: es como esta hoy en produccion -el perfil
   14 tiene '1' en "Parametros de Horas Extras", que es justamente la que no ve-.
   Escribir '1' aqui creeria estar habilitando y estaria ocultando.

   ----------------------------------------------------------------------------
   Esto es solo la mitad del permiso. La otra mitad es
   AdministrarHorasExtras.PerfilesAutorizados en el codigo: la pantalla se abre
   por URL directa aunque el menu no la muestre, y al reves, esta fila sin el
   perfil en esa lista dibuja una opcion que al pulsarla redirige a
   Principal.aspx. Las dos cosas viajan juntas.

   Idempotente: no duplica filas y corrige el Estado de una que ya exista.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* Que los dos menus existan no es una cortesia: con un Id_Menu equivocado la
   fila se inserta igual -PerfilMenu no tiene clave foranea- y no se muestra
   nada, sin un solo error que lo delate. */
IF NOT EXISTS (SELECT 1 FROM dbo.MenuDos WHERE Id_Menu = 20082 AND Href LIKE '%HorasExtras.aspx%')
BEGIN
    RAISERROR('El menu 20082 no existe o ya no apunta a HorasExtras.aspx. Revisar MenuDos antes de seguir. Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MenuDos WHERE Id_Menu = 20081)
BEGIN
    RAISERROR('El grupo padre 20081 (Nomina) no existe. Revisar MenuDos antes de seguir. Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO

PRINT '== Horas extras: menu para el perfil 2 - inicio ==';
GO

DECLARE @Perfil INT = 2;
DECLARE @Menus TABLE (Id_Menu INT, Etiqueta VARCHAR(50));

INSERT INTO @Menus (Id_Menu, Etiqueta)
VALUES (20081, 'Nomina (grupo padre)'),
       (20082, 'Horas Extras');

DECLARE @Id_Menu INT, @Etiqueta VARCHAR(50), @Mensaje VARCHAR(200);

DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT Id_Menu, Etiqueta FROM @Menus;
OPEN cur;
FETCH NEXT FROM cur INTO @Id_Menu, @Etiqueta;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu
                    WHERE id_Menu = @Id_Menu AND IdPerfil = @Perfil)
    BEGIN
        INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
        VALUES (@Id_Menu, @Perfil, GETDATE(), '0');

        SET @Mensaje = 'Perfil 2 dado de alta en ' + @Etiqueta + ' (' + CAST(@Id_Menu AS VARCHAR(10)) + ').';
        PRINT @Mensaje;
    END
    ELSE
    BEGIN
        /* Ya existe: se fuerza el Estado a '0'. Una fila que quedo en '1'
           -oculta- se leeria como "el perfil ya lo tiene" y el script no haria
           nada, dejando el permiso a medias sin decirlo. */
        UPDATE dbo.PerfilMenu
           SET Estado = '0'
         WHERE id_Menu = @Id_Menu AND IdPerfil = @Perfil AND Estado <> '0';

        SET @Mensaje = 'Perfil 2 ya estaba en ' + @Etiqueta + ' (' + CAST(@Id_Menu AS VARCHAR(10)) + '); Estado asegurado en 0.';
        PRINT @Mensaje;
    END

    FETCH NEXT FROM cur INTO @Id_Menu, @Etiqueta;
END

CLOSE cur;
DEALLOCATE cur;
GO

SET NOEXEC OFF;
GO

PRINT '== Horas extras: menu para el perfil 2 - fin ==';
GO

/* ============================================================================
   VERIFICACION (correr a mano despues del script)

   Esperado: 2 filas, las dos con estado '0'.

SELECT  menu    = pm.id_Menu,
        titulo  = m.Titulo,
        perfil  = pm.IdPerfil,
        estado  = pm.Estado
  FROM  dbo.PerfilMenu pm
  JOIN  dbo.MenuDos m ON m.Id_Menu = pm.id_Menu
 WHERE  pm.IdPerfil = 2
   AND  pm.id_Menu IN (20081, 20082);

   ============================================================================ */
