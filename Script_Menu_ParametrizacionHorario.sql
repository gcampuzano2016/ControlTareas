/* ================================================================================
   REGISTRO DE LA PANTALLA EN EL MENU
   Pantalla : ParametrizacionHorarioUsuario.aspx  (Parametrización de horario por usuario)
   Tablas   : dbo.MenuDos (opciones de menú) + dbo.PerfilMenu (visibilidad por perfil)
   Base     : ReporTarea

   - Idempotente: si la opción ya existe (mismo Href) no la duplica y solo completa
     los enlaces de perfil que falten.
   - Id_Menu (MenuDos) y id_MenuPerfil (PerfilMenu) son IDENTITY: se usa SCOPE_IDENTITY.
   ================================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

/* -------------------------------------------------------------------------------
   PARAMETROS A AJUSTAR
   ------------------------------------------------------------------------------- */

/* Menú padre donde aparecerá la opción. Opciones disponibles:
      20    = "Tareas"                (por defecto)
      2     = "Actualizaciones Tareas"
      20042 = "Manejo de Perfiles"    (administración)
   Cambie SOLO este valor si desea colgarla de otro grupo.                          */
DECLARE @IdMenuPadre   BIGINT = 20;

DECLARE @Href          VARCHAR(512) = 'ParametrizacionHorarioUsuario.aspx';
DECLARE @Titulo        VARCHAR(128) = 'Parametrización de Horario';
DECLARE @Descripcion   VARCHAR(512) = 'Asignación de horario laboral por usuario';
DECLARE @Icono         VARCHAR(128) = 'fa fa-sliders';
DECLARE @UsuarioCrea   VARCHAR(16)  = 'admin';
DECLARE @IpCrea        VARCHAR(32)  = '127.0.0.1';

/* Perfiles que verán la opción habilitada (Estado='1').
   Perfiles existentes en el sistema: 1, 2, 18, 19.                                  */

/* -------------------------------------------------------------------------------
   1. Insertar la opción en MenuDos (si no existe)
   ------------------------------------------------------------------------------- */

DECLARE @IdMenu   BIGINT;
DECLARE @Orden    INT;

SELECT @IdMenu = Id_Menu
FROM dbo.MenuDos
WHERE Href = @Href
  AND Estado_Logico_Registro = 1;

IF @IdMenu IS NULL
BEGIN
    SELECT @Orden = ISNULL(MAX(Orden_Opcion), 0) + 1
    FROM dbo.MenuDos
    WHERE Id_MenuPadre = @IdMenuPadre;

    INSERT INTO dbo.MenuDos
    (
        Id_MenuPadre,
        Es_Opcion_de_Menu,
        Href,
        Class_Opcion,
        Class_Icon,
        Titulo,
        Descripcion,
        Es_Opcion_Publica,
        Orden_Opcion,
        Estado_Registro,
        Fecha_Creacion,
        Id_Usuario_Creacion,
        Ip_Creacion,
        Estado_Logico_Registro
    )
    VALUES
    (
        @IdMenuPadre,
        0,                 -- 0 = opción/página (no es cabecera de grupo)
        @Href,
        NULL,
        @Icono,
        @Titulo,
        @Descripcion,
        0,                 -- no es pública
        @Orden,
        1,                 -- Estado_Registro activo
        GETDATE(),
        @UsuarioCrea,
        @IpCrea,
        1                  -- Estado_Logico_Registro activo
    );

    SET @IdMenu = CONVERT(BIGINT, SCOPE_IDENTITY());

    PRINT 'MenuDos: opción creada con Id_Menu = ' + CONVERT(VARCHAR(20), @IdMenu)
        + ' bajo el menú padre ' + CONVERT(VARCHAR(20), @IdMenuPadre) + '.';
END
ELSE
    PRINT 'MenuDos: la opción ya existía (Id_Menu = ' + CONVERT(VARCHAR(20), @IdMenu) + '). Sin duplicar.';

/* -------------------------------------------------------------------------------
   2. Habilitar la opción para los perfiles 1, 2, 18 y 19 (solo los que falten)
   IMPORTANTE: en PerfilMenu, Estado = '0' MUESTRA la opción y Estado = '1' la OCULTA
   (el SP Sp_RTA_ConsultarMenuPerfilUsuario filtra por P.Estado = 0).
   Además, la opción solo se ve si su grupo padre también es visible para el perfil.
   ------------------------------------------------------------------------------- */

DECLARE @Perfiles TABLE (IdPerfil INT PRIMARY KEY);
INSERT INTO @Perfiles (IdPerfil) VALUES (1), (2), (18), (19);

INSERT INTO dbo.PerfilMenu (id_Menu, IdPerfil, FechaRegistro, Estado)
SELECT CONVERT(INT, @IdMenu), P.IdPerfil, CONVERT(DATE, GETDATE()), '0'
FROM @Perfiles AS P
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.PerfilMenu AS PM
    WHERE PM.id_Menu = CONVERT(INT, @IdMenu)
      AND PM.IdPerfil = P.IdPerfil
);

PRINT 'PerfilMenu: enlaces de perfil asegurados (1, 2, 18, 19) con Estado = 0 (visible).';

COMMIT TRANSACTION;
GO

/* -------------------------------------------------------------------------------
   VERIFICACION (opcional)
   ------------------------------------------------------------------------------- */
SELECT M.Id_Menu, M.Id_MenuPadre, M.Titulo, M.Href, M.Class_Icon, M.Orden_Opcion,
       PM.IdPerfil, PM.Estado
FROM dbo.MenuDos AS M
LEFT JOIN dbo.PerfilMenu AS PM ON PM.id_Menu = M.Id_Menu
WHERE M.Href = 'ParametrizacionHorarioUsuario.aspx'
ORDER BY PM.IdPerfil;
GO
