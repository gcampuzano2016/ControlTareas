/* ============================================================================
   Usuarios: cambiar el perfil de un usuario ya creado
   ReporTarea  |  2026-09-21

   PENDIENTE DE EJECUTAR.

   ----------------------------------------------------------------------------
   Hasta hoy el perfil no se editaba desde ninguna pantalla:
   ParametrizacionUsuarios.aspx lo muestra en un campo deshabilitado y
   Sp_RTA_ActualizarUsuario escribe once columnas, ninguna de ellas Id_Perfil.
   Esto lo habilita, pero SOLO por su propia via y con sus propias reglas.

   Por que un procedimiento aparte y no un parametro mas en
   Sp_RTA_ActualizarUsuario: aquel lo puede llamar cualquier usuario con sesion
   -AdministrarUsuarios.ashx no comprueba perfil- y lo que escribe son once
   campos inofensivos. El perfil no es uno mas: es el que da acceso a todo. Si
   entrara por ahi, cualquiera de los 232 usuarios podria ponerse el 18 con una
   peticion directa. Separado, la guarda de perfil 18 vive en el handler sobre
   una accion que no hace ninguna otra cosa.

   ----------------------------------------------------------------------------
   Convencion de esta pantalla, distinta a la de horas extras:
   Respuestas = 1 es EXITO y 0 es error, y el texto va en Mensaje. Se respeta
   para que AdministrarUsuarios.ashx no tenga dos criterios conviviendo.

   ----------------------------------------------------------------------------
   El catalogo es dbo.Perfiles y no dbo.R_Perfil. No es indistinto: en esta base
   conviven los dos y no dicen lo mismo -el perfil 2 es "Servicios2" en Perfiles
   y "Supervisor Especialistas" en R_Perfil-. Manda Perfiles porque es contra el
   que une Sp_RTA_ListarUsuariosAdmin, o sea el nombre que la pantalla muestra, y
   porque R_Perfil solo cubre 5 de los 21 perfiles que la gente tiene hoy.

   Hay 4 valores de Id_Perfil en uso -20, 21, 41 y -1, entre 6 usuarios- que no
   estan en NINGUN catalogo. Este procedimiento no los ofrece como destino, pero
   si permite sacar a alguien de ahi: valida el perfil NUEVO, nunca el viejo.

   Idempotente: DROP y CREATE.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('dbo.R_UsuarioBitacora','U') IS NULL
BEGIN
    RAISERROR('No existe dbo.R_UsuarioBitacora: sin ella el cambio de perfil quedaria sin rastro. Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO

PRINT '== Usuarios: cambiar el perfil - inicio ==';
GO

IF OBJECT_ID('dbo.Sp_RTA_CambiarPerfilUsuario','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_CambiarPerfilUsuario;
GO

CREATE PROCEDURE dbo.Sp_RTA_CambiarPerfilUsuario
    @Id_Usuario      NUMERIC(6),
    @Id_Perfil       BIGINT,
    @UsuarioRegistro VARCHAR(50) = 'SISTEMA'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';
    DECLARE @PerfilViejo BIGINT, @CodObjetivo VARCHAR(50);

    SELECT @PerfilViejo = Id_Perfil,
           @CodObjetivo = LTRIM(RTRIM(ISNULL(Cod_Usuario,'')))
      FROM dbo.R_Usuarios
     WHERE Id_Usuario = @Id_Usuario;

    IF @PerfilViejo IS NULL AND NOT EXISTS (SELECT 1 FROM dbo.R_Usuarios WHERE Id_Usuario = @Id_Usuario)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El usuario indicado no existe.'; RETURN;
    END

    /* Nadie cambia su propio perfil. Se comprueba tambien en el handler, donde
       vive la sesion; se repite aqui porque este procedimiento es llamable
       desde SSMS y porque el dano -el ultimo Super Admin quitandose el perfil y
       dejando el sistema sin quien administre- no tiene vuelta atras desde la
       aplicacion.

       La comparacion es por Cod_Usuario y no por Id_Usuario porque es lo que el
       handler manda en @UsuarioRegistro; ver UsuarioSesion(). */
    IF @CodObjetivo <> '' AND @CodObjetivo = LTRIM(RTRIM(ISNULL(@UsuarioRegistro,'')))
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'No puede cambiar su propio perfil. Pídaselo a otro administrador.'; RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Perfiles WHERE IdPerfiles = @Id_Perfil AND ISNULL(Estado,1) = 1)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El perfil indicado no existe o está inactivo.'; RETURN;
    END

    /* Mismo criterio que Sp_RTA_ActualizarUsuario: si no hay cambio real no se
       escribe ni se audita, para que la bitacora no se llene de filas que no
       dicen nada. */
    IF @PerfilViejo = @Id_Perfil
    BEGIN
        SELECT Respuestas = 1, Mensaje = 'El usuario ya tenía ese perfil. No hubo cambios que guardar.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        /* Los nombres se resuelven para la bitacora. El viejo puede no estar en
           el catalogo -hay 4 valores en uso que no estan en ninguno- y por eso
           lleva ISNULL: perder el rastro de DE DONDE salio la persona seria
           justo lo que esta bitacora existe para evitar. */
        DECLARE @NomViejo VARCHAR(100), @NomNuevo VARCHAR(100);

        SELECT @NomViejo = ISNULL((SELECT TOP 1 NombrePerfil FROM dbo.Perfiles WHERE IdPerfiles = @PerfilViejo),
                                  '(fuera del catalogo)');
        SELECT @NomNuevo = (SELECT TOP 1 NombrePerfil FROM dbo.Perfiles WHERE IdPerfiles = @Id_Perfil);

        DECLARE @Detalle VARCHAR(2000);
        SET @Detalle = 'Perfil: [' + CAST(ISNULL(@PerfilViejo,0) AS VARCHAR(20)) + ' - ' + @NomViejo + ']'
                     + ' -> [' + CAST(@Id_Perfil AS VARCHAR(20)) + ' - ' + @NomNuevo + ']';

        UPDATE dbo.R_Usuarios
           SET Id_Perfil = @Id_Perfil
         WHERE Id_Usuario = @Id_Usuario;

        INSERT INTO dbo.R_UsuarioBitacora (Id_Usuario, Accion, Detalle, Usuario_Registro)
        VALUES (@Id_Usuario, 'CAMBIO PERFIL', LEFT(@Detalle,2000), @UsuarioRegistro);

        SET @Respuestas = 1;
        SET @Mensaje = 'Perfil actualizado. La persona lo verá al volver a iniciar sesión.';
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo cambiar el perfil: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO

PRINT 'Sp_RTA_CambiarPerfilUsuario actualizado.';
GO

SET NOEXEC OFF;
GO

PRINT '== Usuarios: cambiar el perfil - fin ==';
GO

/* ============================================================================
   VERIFICACION (correr a mano despues del script)

   Esperado: 1 fila.

SELECT nombre = name, creado = CONVERT(VARCHAR(20), modify_date, 120)
  FROM sys.procedures
 WHERE name = 'Sp_RTA_CambiarPerfilUsuario';

   Y despues del primer cambio real, el rastro:

SELECT TOP 10 Id_Usuario, Accion, Detalle, Usuario_Registro, Fecha_Registro
  FROM dbo.R_UsuarioBitacora
 WHERE Accion = 'CAMBIO PERFIL'
 ORDER BY Id_Bitacora DESC;

   ============================================================================ */
