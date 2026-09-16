/* ============================================================================
   Correccion: Sp_RTA_GuardarMenuPerfil validaba contra el catalogo equivocado
   ----------------------------------------------------------------------------
   Sintoma: en ParametrizacionMenuPerfil.aspx, asignar modulos a casi cualquier
   perfil devolvia "El perfil indicado no existe."

   Causa: el combo de la pantalla se llena con Sp_RTA_ListarPerfiles, que lee de
   dbo.Perfiles (17 perfiles, ids hasta 19), pero este procedimiento validaba
   contra dbo.R_Perfil, que solo tiene los ids 1..5 y son perfiles DISTINTOS
   (el 1 de Perfiles es 'Tecnico'; el 1 de R_Perfil es 'Tecnico Especialista').
   Cualquier perfil por encima de 5 era rechazado.

   Es una correccion a medias de agosto de 2026: se arreglo el SP que lista para
   que leyera de dbo.Perfiles y se paso por alto el que guarda.

   Se cambia unicamente la linea de validacion. El resto queda igual.
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
ALTER PROCEDURE dbo.Sp_RTA_GuardarMenuPerfil
    @IdPerfil   INT,
    @ActivosCsv VARCHAR(MAX) = ''
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';

    IF ISNULL(@IdPerfil,0) = 0 OR NOT EXISTS (SELECT 1 FROM dbo.Perfiles WHERE IdPerfiles = @IdPerfil)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El perfil indicado no existe.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Activos TABLE (IdMenu INT PRIMARY KEY);

        /* parsear el CSV de enteros via XML-nodes (compatible SQL 2008+) */
        IF ISNULL(LTRIM(RTRIM(@ActivosCsv)),'') <> ''
        BEGIN
            DECLARE @xml XML = CAST('<i>' + REPLACE(@ActivosCsv, ',', '</i><i>') + '</i>' AS XML);
            INSERT INTO @Activos (IdMenu)
            SELECT DISTINCT T.c.value('.', 'INT')
            FROM @xml.nodes('/i') AS T(c)
            WHERE ISNULL(T.c.value('.', 'VARCHAR(20)'),'') <> '';
        END

        /* invariante: agregar el padre de todo hijo activo que no este ya en el set */
        INSERT INTO @Activos (IdMenu)
        SELECT DISTINCT m.Id_MenuPadre
        FROM @Activos a
        JOIN dbo.MenuDos m ON m.Id_Menu = a.IdMenu
        WHERE ISNULL(m.Id_MenuPadre,0) <> 0
          AND m.Id_MenuPadre NOT IN (SELECT IdMenu FROM @Activos);

        /* upsert: existentes -> Estado 0/1 segun set; faltantes activos -> insertar Estado 0 */
        MERGE dbo.PerfilMenu AS tgt
        USING (
            SELECT m.Id_Menu,
                   EsActivo = CASE WHEN a.IdMenu IS NOT NULL THEN 1 ELSE 0 END
            FROM dbo.MenuDos m
            LEFT JOIN @Activos a ON a.IdMenu = m.Id_Menu
        ) AS src
        ON tgt.IdPerfil = @IdPerfil AND tgt.id_Menu = src.Id_Menu
        WHEN MATCHED THEN
            UPDATE SET tgt.Estado = CASE WHEN src.EsActivo = 1 THEN 0 ELSE 1 END
        WHEN NOT MATCHED BY TARGET AND src.EsActivo = 1 THEN
            INSERT (IdPerfil, id_Menu, Estado) VALUES (@IdPerfil, src.Id_Menu, 0);

        SET @Respuestas = 1;
        SET @Mensaje = 'Menus del perfil guardados correctamente.';
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo guardar los menus del perfil: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END

GO
