# Módulos extra por usuario — Plan de implementación

> **Para agentes:** SUB-SKILL REQUERIDA: usar `superpowers:subagent-driven-development` (recomendado) o `superpowers:executing-plans` para ejecutar tarea por tarea. Los pasos usan checkbox (`- [ ]`) para seguimiento.

**Spec:** `docs/superpowers/specs/2026-08-03-modulos-extra-por-usuario-design.md`

**Objetivo:** permitir asignar a un usuario puntual módulos de menú que su perfil no tiene, viendo en pantalla a qué perfil pertenece, sin que nadie pierda accesos.

**Arquitectura:** tabla nueva `R_UsuarioMenu` con excepciones **aditivas**; el SP del sidebar une los menús del perfil con los extras del usuario; pantalla nueva `ParametrizacionMenuUsuario.aspx` con buscador de usuario + árbol de dos niveles donde lo heredado del perfil sale marcado y bloqueado. Mismo patrón de 3 capas + handler `.ashx` que los módulos "Menús por perfil" y "Horario por usuario".

**Stack:** ASP.NET WebForms (.NET Framework), SQL Server 2008+, jQuery, Bootstrap 3, MSBuild 2019.

## Restricciones globales

- **No hay framework de pruebas en el proyecto.** Cada tarea se verifica con `sqlcmd`, con compilación, o en el navegador. Los pasos de verificación son obligatorios, no opcionales.
- **Conexión a la base para verificar:**
  `sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "..."`
  (la credencial vive en `CapaDato/DaoReporTareaAranda.cs:23`).
- **Todos los scripts SQL son idempotentes** y empiezan con `SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; GO`.
- **Los mensajes fijos dentro de SPs van SIN TILDES.** Los mensajes en C# y JS sí llevan tildes.
- **Los archivos `.js` se guardan en UTF-8 CON BOM.** Sin BOM las tildes salen corruptas (ya pasó, ver commit `7690897`).
- **Al tocar un `.js` hay que subir el `?v=` en el `.aspx`** o el navegador sirve la versión cacheada (ya pasó, ver la memoria del proyecto sobre el `?v=3`).
- **Editar solo `CapaDato/DaoMenuDos.cs`.** `CapaNegocio/CapaDato/DaoMenuDos.cs` es una copia huérfana que ningún `.csproj` incluye; modificarla no tiene efecto.
- **Todo archivo nuevo debe agregarse a su `.csproj`** o no compila / no se publica.
- **Convención de estado:** `PerfilMenu.Estado` usa `0 = activo` (heredado, no se cambia). `R_UsuarioMenu.Estado` usa `'A'/'I'`. Los SPs traducen entre ambas.
- **Compilar:** `MSBuild.exe ReporteTareas.sln /p:Configuration=Debug` desde PowerShell. La app corre en **IIS Express de 32 bits** (dependencia `Pechkin` x86).

## Mapa de archivos

| Archivo | Responsabilidad | Tarea |
|---|---|---|
| `docs/superpowers/plans/sql/2026-08-03-fix-combo-perfiles.sql` | `Sp_RTA_ListarPerfiles` lee del catálogo correcto | 1 |
| `docs/superpowers/plans/sql/2026-08-03-modulos-extra-por-usuario.sql` | tabla `R_UsuarioMenu` + 3 SPs de la pantalla | 2 |
| `docs/superpowers/plans/sql/2026-08-03-modulos-extra-sidebar.sql` | `Sp_RTA_ConsultarMenuPerfilUsuario` con extras | 3 |
| `CapaEntidad/EntUsuarioMenuBusqueda.cs`, `EntMenuUsuario.cs` | DTOs | 4 |
| `CapaDato/DaoMenuUsuario.cs` | acceso a los 3 SPs nuevos | 4 |
| `CapaNegocio/NegMenuUsuario.cs` | pass-through | 4 |
| `CapaDato/DaoMenuDos.cs`, `CapaNegocio/NegMenuDos.cs`, `Master.Master.cs` | pasar `Cod_Usuario` al sidebar | 5 |
| `ReporteTareas/Formulario/AdministrarMenuUsuario.ashx(.cs)` | handler JSON | 6 |
| `ReporteTareas/Formulario/ParametrizacionMenuUsuario.aspx(.cs/.designer.cs)` | pantalla | 7 |
| `ReporteTareas/js/parametrizacionMenuUsuario.js` | buscador + árbol + guardar | 8 |
| `docs/superpowers/plans/sql/2026-08-03-modulos-extra-menu.sql` | registro en el menú | 9 |

---

## Tarea 1: Corregir el combo de perfiles

Independiente del resto: arregla un defecto ya en producción y se puede probar sola.

**Archivos:**
- Crear: `docs/superpowers/plans/sql/2026-08-03-fix-combo-perfiles.sql`

**Interfaces:**
- Produce: `Sp_RTA_ListarPerfiles` devolviendo columnas `Id_Perfil, Nombre` (mismos nombres que hoy) desde `dbo.Perfiles`.
- Consume: nada.

- [ ] **Paso 1: Capturar el estado actual (línea base)**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; SELECT 'filas actuales='+CAST(COUNT(*) AS VARCHAR) FROM dbo.R_Perfil WHERE ISNULL(Estado_Logico_Registro,1)=1;"
```

Esperado: `filas actuales=5`. Anotar el número.

- [ ] **Paso 2: Escribir el script**

```sql
/* ============================================================================
   Fix: el combo de perfiles leia de R_Perfil (solo 5 perfiles, nombres que no
   corresponden). El catalogo que mapea con R_Usuarios.Id_Perfil es dbo.Perfiles.
   Se conservan los nombres de columna Id_Perfil / Nombre para no tocar C# ni JS.
   Base: ReporTarea
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('dbo.Sp_RTA_ListarPerfiles','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarPerfiles;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarPerfiles
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        Id_Perfil = IdPerfiles,
        Nombre    = NombrePerfil
    FROM dbo.Perfiles
    WHERE ISNULL(Estado,1) = 1
    ORDER BY NombrePerfil;
END
GO

/* verificacion */
EXEC dbo.Sp_RTA_ListarPerfiles;
GO
```

- [ ] **Paso 3: Ejecutar y verificar**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -i "docs/superpowers/plans/sql/2026-08-03-fix-combo-perfiles.sql"
```

Esperado: **19 filas**, entre ellas `18 | Super Admin` y `19 | Servicios4`. Si salen 5, el script no se aplicó.

- [ ] **Paso 4: Verificar que el contrato de columnas no cambió**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; EXEC dbo.Sp_RTA_ListarPerfiles;" | head -3
```

Esperado: filas con dos columnas, id y nombre. `CapaDato/DaoMenuPerfil.cs:29` lee `dr["Id_Perfil"]` y `dr["Nombre"]`; si algún nombre cambió, la pantalla por perfil se rompe.

- [ ] **Paso 5: Probar en el navegador**

Abrir `ParametrizacionMenuPerfil.aspx`. El combo debe listar 19 perfiles. Elegir **Super Admin** y confirmar que el árbol carga sus 65 ítems.

- [ ] **Paso 6: Commit**

```bash
git add docs/superpowers/plans/sql/2026-08-03-fix-combo-perfiles.sql
git commit -m "fix(perfiles): el combo lee de dbo.Perfiles y no de R_Perfil"
```

---

## Tarea 2: Tabla `R_UsuarioMenu` y sus SPs

**Archivos:**
- Crear: `docs/superpowers/plans/sql/2026-08-03-modulos-extra-por-usuario.sql`

**Interfaces:**
- Produce:
  - `dbo.R_UsuarioMenu(Id_UsuarioMenu, Cod_Usuario, Id_Menu, Estado, Usuario_Registro, Fecha_Registro)`
  - `Sp_RTA_ListarUsuariosMenu @Filtro VARCHAR(100)` → `Cod_Usuario, Nom_Usuario, Cedula, Id_Perfil, NombrePerfil, TotalExtras`
  - `Sp_RTA_ListarMenuUsuario @CodUsuario VARCHAR(50)` → `Id_Menu, Id_MenuPadre, Titulo, Class_Icon, ActivoPerfil, ActivoUsuario`
  - `Sp_RTA_GuardarMenuUsuario @CodUsuario VARCHAR(50), @ExtrasCsv VARCHAR(MAX), @UsuarioRegistro VARCHAR(50)` → `Respuestas INT, Mensaje VARCHAR(300)`
- Consume: nada.

- [ ] **Paso 1: Escribir el script**

```sql
/* ============================================================================
   Modulos extra por usuario — tabla y stored procedures
   Regla: el usuario ve los menus de su perfil MAS estos extras. Nunca menos.
   Base: ReporTarea
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------- 1) Tabla de excepciones aditivas ---------- */
IF OBJECT_ID('dbo.R_UsuarioMenu','U') IS NULL
BEGIN
    CREATE TABLE dbo.R_UsuarioMenu
    (
        Id_UsuarioMenu   INT IDENTITY(1,1) NOT NULL,
        Cod_Usuario      VARCHAR(50)  NOT NULL,
        Id_Menu          INT          NOT NULL,
        Estado           CHAR(1)      NOT NULL CONSTRAINT DF_R_UsuarioMenu_Estado DEFAULT ('A'),
        Usuario_Registro VARCHAR(50)  NOT NULL,
        Fecha_Registro   DATETIME     NOT NULL CONSTRAINT DF_R_UsuarioMenu_Fecha DEFAULT (GETDATE()),
        CONSTRAINT PK_R_UsuarioMenu PRIMARY KEY (Id_UsuarioMenu),
        CONSTRAINT UQ_R_UsuarioMenu UNIQUE (Cod_Usuario, Id_Menu)
    );
END
GO

/* ---------- 2) Buscar usuarios con su perfil ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarUsuariosMenu','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarUsuariosMenu;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarUsuariosMenu
    @Filtro VARCHAR(100) = ''
AS
BEGIN
    SET NOCOUNT ON;
    SET @Filtro = LTRIM(RTRIM(ISNULL(@Filtro,'')));

    SELECT
        u.Cod_Usuario,
        u.Nom_Usuario,
        Cedula       = ISNULL(u.Cedula,''),
        Id_Perfil    = u.Id_Perfil,
        NombrePerfil = ISNULL(p.NombrePerfil,'Sin perfil'),
        TotalExtras  = ISNULL(x.Total,0)
    FROM dbo.R_Usuarios u
    LEFT JOIN dbo.Perfiles p
        ON p.IdPerfiles = u.Id_Perfil
    OUTER APPLY
    (
        SELECT Total = COUNT(*)
        FROM dbo.R_UsuarioMenu um
        WHERE um.Cod_Usuario = u.Cod_Usuario AND um.Estado = 'A'
    ) x
    WHERE u.Usuario_Estado = 'A'
      AND
      (
          @Filtro = ''
          OR u.Nom_Usuario LIKE '%' + @Filtro + '%'
          OR u.Cod_Usuario LIKE '%' + @Filtro + '%'
          OR ISNULL(u.Cedula,'') LIKE '%' + @Filtro + '%'
      )
    ORDER BY u.Nom_Usuario;
END
GO

/* ---------- 3) Arbol de menus con las dos banderas ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarMenuUsuario','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarMenuUsuario;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarMenuUsuario
    @CodUsuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IdPerfil BIGINT;
    SELECT @IdPerfil = Id_Perfil FROM dbo.R_Usuarios WHERE Cod_Usuario = @CodUsuario;

    SELECT
        m.Id_Menu,
        Id_MenuPadre  = ISNULL(m.Id_MenuPadre,0),
        m.Titulo,
        Class_Icon    = ISNULL(m.Class_Icon,''),
        ActivoPerfil  = CASE WHEN pm.id_Menu IS NOT NULL THEN 1 ELSE 0 END,
        ActivoUsuario = CASE WHEN um.Id_Menu IS NOT NULL THEN 1 ELSE 0 END
    FROM dbo.MenuDos m
    LEFT JOIN dbo.PerfilMenu pm
        ON pm.id_Menu = m.Id_Menu AND pm.IdPerfil = @IdPerfil AND pm.Estado = 0
    LEFT JOIN dbo.R_UsuarioMenu um
        ON um.Id_Menu = m.Id_Menu AND um.Cod_Usuario = @CodUsuario AND um.Estado = 'A'
    ORDER BY ISNULL(m.Id_MenuPadre,0), m.Titulo;
END
GO

/* ---------- 4) Guardar extras (atomico) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_GuardarMenuUsuario','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_GuardarMenuUsuario;
GO
CREATE PROCEDURE dbo.Sp_RTA_GuardarMenuUsuario
    @CodUsuario      VARCHAR(50),
    @ExtrasCsv       VARCHAR(MAX) = '',
    @UsuarioRegistro VARCHAR(50)  = 'SISTEMA'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';

    IF ISNULL(LTRIM(RTRIM(@CodUsuario)),'') = ''
       OR NOT EXISTS (SELECT 1 FROM dbo.R_Usuarios WHERE Cod_Usuario = @CodUsuario)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El usuario indicado no existe.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @IdPerfil BIGINT;
        SELECT @IdPerfil = Id_Perfil FROM dbo.R_Usuarios WHERE Cod_Usuario = @CodUsuario;

        DECLARE @Extras TABLE (IdMenu INT PRIMARY KEY);

        /* parsear el CSV de enteros via XML-nodes (compatible SQL 2008+) */
        IF ISNULL(LTRIM(RTRIM(@ExtrasCsv)),'') <> ''
        BEGIN
            DECLARE @xml XML = CAST('<i>' + REPLACE(@ExtrasCsv, ',', '</i><i>') + '</i>' AS XML);
            INSERT INTO @Extras (IdMenu)
            SELECT DISTINCT T.c.value('.', 'INT')
            FROM @xml.nodes('/i') AS T(c)
            WHERE ISNULL(T.c.value('.', 'VARCHAR(20)'),'') <> '';
        END

        /* solo ids que existan en MenuDos */
        DELETE FROM @Extras
        WHERE IdMenu NOT IN (SELECT Id_Menu FROM dbo.MenuDos);

        /* descartar los que ya vienen del perfil: la tabla guarda solo excepciones reales */
        DELETE FROM @Extras
        WHERE IdMenu IN (SELECT id_Menu FROM dbo.PerfilMenu WHERE IdPerfil = @IdPerfil AND Estado = 0);

        /* invariante hijo=>padre: si el padre no lo da el perfil ni esta en el set, agregarlo.
           Master.Master.cs no dibuja un hijo cuyo padre no exista en el resultado. */
        INSERT INTO @Extras (IdMenu)
        SELECT DISTINCT m.Id_MenuPadre
        FROM @Extras a
        JOIN dbo.MenuDos m ON m.Id_Menu = a.IdMenu
        WHERE ISNULL(m.Id_MenuPadre,0) <> 0
          AND m.Id_MenuPadre NOT IN (SELECT IdMenu FROM @Extras)
          AND NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu pm
                          WHERE pm.IdPerfil = @IdPerfil AND pm.id_Menu = m.Id_MenuPadre AND pm.Estado = 0);

        /* upsert: activar los del set, desactivar el resto de sus filas */
        MERGE dbo.R_UsuarioMenu AS tgt
        USING (
            SELECT IdMenu, EsActivo = 1 FROM @Extras
            UNION
            SELECT um.Id_Menu, EsActivo = 0
            FROM dbo.R_UsuarioMenu um
            WHERE um.Cod_Usuario = @CodUsuario
              AND um.Id_Menu NOT IN (SELECT IdMenu FROM @Extras)
        ) AS src
        ON tgt.Cod_Usuario = @CodUsuario AND tgt.Id_Menu = src.IdMenu
        WHEN MATCHED AND tgt.Estado <> CASE WHEN src.EsActivo = 1 THEN 'A' ELSE 'I' END THEN
            UPDATE SET tgt.Estado           = CASE WHEN src.EsActivo = 1 THEN 'A' ELSE 'I' END,
                       tgt.Usuario_Registro = @UsuarioRegistro,
                       tgt.Fecha_Registro   = GETDATE()
        WHEN NOT MATCHED BY TARGET AND src.EsActivo = 1 THEN
            INSERT (Cod_Usuario, Id_Menu, Estado, Usuario_Registro, Fecha_Registro)
            VALUES (@CodUsuario, src.IdMenu, 'A', @UsuarioRegistro, GETDATE());

        SET @Respuestas = 1;
        SET @Mensaje = 'Modulos del usuario guardados correctamente.';
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo guardar los modulos del usuario: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO
```

- [ ] **Paso 2: Ejecutar el script**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -i "docs/superpowers/plans/sql/2026-08-03-modulos-extra-por-usuario.sql"
```

Esperado: sin errores. Si aparece `Incorrect syntax near 'MERGE'`, falta el `;` de la sentencia anterior.

- [ ] **Paso 3: Verificar la búsqueda de usuarios**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; EXEC dbo.Sp_RTA_ListarUsuariosMenu 'campuzano';"
```

Esperado: al menos una fila, con `NombrePerfil` con texto (no vacío). Verificar también que los usuarios sin perfil válido aparecen:

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM (SELECT * FROM dbo.R_Usuarios u WHERE u.Usuario_Estado='A' AND u.Id_Perfil IN (-1,20,21,41)) z;"
```

Esperado: `6`. Estos 6 deben salir como `Sin perfil` en el SP, no desaparecer.

- [ ] **Paso 4: Verificar el árbol para un usuario real**

Tomar un `Cod_Usuario` de perfil 18 (Super Admin) y ejecutar:

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; DECLARE @c VARCHAR(50); SELECT TOP 1 @c=Cod_Usuario FROM dbo.R_Usuarios WHERE Id_Perfil=18 AND Usuario_Estado='A'; PRINT @c; SELECT ActivoPerfil, Total=COUNT(*) FROM (SELECT * FROM dbo.MenuDos) m CROSS APPLY (SELECT ActivoPerfil=CASE WHEN EXISTS(SELECT 1 FROM dbo.PerfilMenu pm WHERE pm.IdPerfil=18 AND pm.id_Menu=m.Id_Menu AND pm.Estado=0) THEN 1 ELSE 0 END) f GROUP BY ActivoPerfil;"
```

Anotar el `Cod_Usuario` impreso — se reutiliza en los pasos siguientes. Luego:

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; EXEC dbo.Sp_RTA_ListarMenuUsuario '<CodUsuario>';"
```

Esperado: una fila por cada ítem de `MenuDos`; los del perfil con `ActivoPerfil=1`; todos con `ActivoUsuario=0` (aún no hay extras).

- [ ] **Paso 5: Probar que descarta lo que ya da el perfil**

Tomar un `Id_Menu` que el perfil 18 **sí** tiene y guardarlo como extra:

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; DECLARE @m VARCHAR(20); SELECT TOP 1 @m=CAST(id_Menu AS VARCHAR) FROM dbo.PerfilMenu WHERE IdPerfil=18 AND Estado=0; EXEC dbo.Sp_RTA_GuardarMenuUsuario '<CodUsuario>', @m, 'PRUEBA'; SELECT filas=COUNT(*) FROM dbo.R_UsuarioMenu WHERE Cod_Usuario='<CodUsuario>' AND Estado='A';"
```

Esperado: `Respuestas=1` y **`filas=0`** — el SP descartó el ítem porque ya venía del perfil.

- [ ] **Paso 6: Probar la invariante hijo⇒padre**

Buscar un hijo cuyo padre **no** esté en el perfil 18 y guardarlo:

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; DECLARE @h VARCHAR(20); SELECT TOP 1 @h=CAST(m.Id_Menu AS VARCHAR) FROM dbo.MenuDos m WHERE ISNULL(m.Id_MenuPadre,0)<>0 AND NOT EXISTS(SELECT 1 FROM dbo.PerfilMenu pm WHERE pm.IdPerfil=18 AND pm.id_Menu=m.Id_Menu AND pm.Estado=0) AND NOT EXISTS(SELECT 1 FROM dbo.PerfilMenu pm2 WHERE pm2.IdPerfil=18 AND pm2.id_Menu=m.Id_MenuPadre AND pm2.Estado=0); PRINT 'hijo='+@h; EXEC dbo.Sp_RTA_GuardarMenuUsuario '<CodUsuario>', @h, 'PRUEBA'; SELECT Id_Menu, Estado FROM dbo.R_UsuarioMenu WHERE Cod_Usuario='<CodUsuario>' AND Estado='A';"
```

Esperado: **2 filas** activas — el hijo pedido **y su padre**, que el SP agregó solo.

- [ ] **Paso 7: Probar que el set vacío desactiva todo**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; EXEC dbo.Sp_RTA_GuardarMenuUsuario '<CodUsuario>', '', 'PRUEBA'; SELECT activos=COUNT(*) FROM dbo.R_UsuarioMenu WHERE Cod_Usuario='<CodUsuario>' AND Estado='A'; SELECT historicos=COUNT(*) FROM dbo.R_UsuarioMenu WHERE Cod_Usuario='<CodUsuario>';"
```

Esperado: `activos=0` y `historicos=2` — se desactivaron, no se borraron (auditoría intacta).

- [ ] **Paso 8: Probar usuario inexistente**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; EXEC dbo.Sp_RTA_GuardarMenuUsuario 'NOEXISTE_ZZZ', '1', 'PRUEBA';"
```

Esperado: `Respuestas=0` y el mensaje `El usuario indicado no existe.`

- [ ] **Paso 9: Limpiar los datos de prueba**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "DELETE FROM dbo.R_UsuarioMenu WHERE Usuario_Registro='PRUEBA'; SELECT restantes=COUNT(*) FROM dbo.R_UsuarioMenu;"
```

Esperado: `restantes=0`.

- [ ] **Paso 10: Commit**

```bash
git add docs/superpowers/plans/sql/2026-08-03-modulos-extra-por-usuario.sql
git commit -m "feat(db): tabla R_UsuarioMenu y SPs de modulos extra por usuario"
```

---

## Tarea 3: El sidebar une perfil + extras

**Archivos:**
- Crear: `docs/superpowers/plans/sql/2026-08-03-modulos-extra-sidebar.sql`

**Interfaces:**
- Consume: `dbo.R_UsuarioMenu` (Tarea 2).
- Produce: `Sp_RTA_ConsultarMenuPerfilUsuario @tipoPerfil INT = 0, @CodUsuario VARCHAR(50) = NULL` devolviendo las **seis** columnas `id_Menu, Titulo, Estado, Id_MenuPadre, Class_Icon, Href`.

**Por qué las seis columnas importan:** `CapaDato/DaoMenuDos.cs:183-188` lee las seis por nombre. Si falta una, lanza excepción; el `catch` de la línea 195 la traga y devuelve `null`, y `Master.Master.cs:29` revienta al tocar `dtPrincipal`. El resultado sería **el menú en blanco para todos**.

- [ ] **Paso 1: Capturar la línea base ANTES de tocar nada**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; DECLARE @t TABLE (id_Menu INT, Titulo VARCHAR(200), Estado INT, Id_MenuPadre INT, Class_Icon VARCHAR(200), Href VARCHAR(200)); INSERT INTO @t EXEC dbo.Sp_RTA_ConsultarMenuPerfilUsuario 18; SELECT perfil18=COUNT(*) FROM @t; DELETE FROM @t; INSERT INTO @t EXEC dbo.Sp_RTA_ConsultarMenuPerfilUsuario 1; SELECT perfil1=COUNT(*) FROM @t;"
```

Anotar ambos números. Esperado según los datos actuales: `perfil18=65`, `perfil1=42`. **Este es el contrato de no-regresión.**

- [ ] **Paso 2: Escribir el script**

```sql
/* ============================================================================
   Sidebar: el menu ahora es (menus del perfil) UNION (extras del usuario).
   Con @CodUsuario NULL el resultado es identico al de antes de este cambio.
   Se elimina el IF EXISTS original por redundante: si el perfil no tiene filas
   en PerfilMenu el SELECT no devuelve nada igual, y estorbaba para que un
   usuario sin menus de perfil pudiera ver sus extras.
   Base: ReporTarea
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('dbo.Sp_RTA_ConsultarMenuPerfilUsuario','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ConsultarMenuPerfilUsuario;
GO
CREATE PROCEDURE dbo.Sp_RTA_ConsultarMenuPerfilUsuario
    @tipoPerfil AS INT = 0,
    @CodUsuario AS VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* Las seis columnas y su orden son contrato: DaoMenuDos.cs las lee por nombre. */
    SELECT P.id_Menu, M.Titulo, P.Estado, M.Id_MenuPadre, M.Class_Icon, M.Href
    FROM dbo.PerfilMenu P
    INNER JOIN dbo.MenuDos M ON P.id_Menu = M.Id_Menu
    WHERE P.IdPerfil = @tipoPerfil AND P.Estado = 0

    UNION

    SELECT UM.Id_Menu, M.Titulo, 0 AS Estado, M.Id_MenuPadre, M.Class_Icon, M.Href
    FROM dbo.R_UsuarioMenu UM
    INNER JOIN dbo.MenuDos M ON UM.Id_Menu = M.Id_Menu
    WHERE @CodUsuario IS NOT NULL
      AND UM.Cod_Usuario = @CodUsuario
      AND UM.Estado = 'A';

    SET NOCOUNT OFF;
END
GO
```

- [ ] **Paso 3: Ejecutar el script**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -i "docs/superpowers/plans/sql/2026-08-03-modulos-extra-sidebar.sql"
```

- [ ] **Paso 4: Verificar la NO REGRESIÓN (paso crítico)**

Repetir exactamente el comando del Paso 1.

Esperado: **los mismos números** (`perfil18=65`, `perfil1=42`). Si cambiaron, **detenerse y revertir**: hay usuarios que perderían o ganarían menús sin querer.

- [ ] **Paso 5: Verificar que los extras suman**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; DECLARE @c VARCHAR(50); SELECT TOP 1 @c=Cod_Usuario FROM dbo.R_Usuarios WHERE Id_Perfil=18 AND Usuario_Estado='A'; DECLARE @m INT; SELECT TOP 1 @m=Id_Menu FROM dbo.MenuDos WHERE ISNULL(Id_MenuPadre,0)=0 AND Id_Menu NOT IN (SELECT id_Menu FROM dbo.PerfilMenu WHERE IdPerfil=18 AND Estado=0); INSERT INTO dbo.R_UsuarioMenu (Cod_Usuario, Id_Menu, Estado, Usuario_Registro) VALUES (@c, @m, 'A', 'PRUEBA'); DECLARE @t TABLE (id_Menu INT, Titulo VARCHAR(200), Estado INT, Id_MenuPadre INT, Class_Icon VARCHAR(200), Href VARCHAR(200)); INSERT INTO @t EXEC dbo.Sp_RTA_ConsultarMenuPerfilUsuario 18, @c; SELECT conExtra=COUNT(*) FROM @t; DELETE FROM dbo.R_UsuarioMenu WHERE Usuario_Registro='PRUEBA';"
```

Esperado: `conExtra=66` (los 65 del perfil + 1). Al final limpia el dato de prueba.

- [ ] **Paso 6: Verificar que no duplica**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; DECLARE @c VARCHAR(50); SELECT TOP 1 @c=Cod_Usuario FROM dbo.R_Usuarios WHERE Id_Perfil=18 AND Usuario_Estado='A'; DECLARE @m INT; SELECT TOP 1 @m=id_Menu FROM dbo.PerfilMenu WHERE IdPerfil=18 AND Estado=0; INSERT INTO dbo.R_UsuarioMenu (Cod_Usuario, Id_Menu, Estado, Usuario_Registro) VALUES (@c, @m, 'A', 'PRUEBA'); DECLARE @t TABLE (id_Menu INT, Titulo VARCHAR(200), Estado INT, Id_MenuPadre INT, Class_Icon VARCHAR(200), Href VARCHAR(200)); INSERT INTO @t EXEC dbo.Sp_RTA_ConsultarMenuPerfilUsuario 18, @c; SELECT total=COUNT(*) FROM @t; DELETE FROM dbo.R_UsuarioMenu WHERE Usuario_Registro='PRUEBA';"
```

Esperado: `total=65` — el `UNION` colapsó el duplicado.

- [ ] **Paso 7: Commit**

```bash
git add docs/superpowers/plans/sql/2026-08-03-modulos-extra-sidebar.sql
git commit -m "feat(db): el sidebar une menus del perfil con los extras del usuario"
```

---

## Tarea 4: Capas C# de la pantalla nueva

**Archivos:**
- Crear: `CapaEntidad/EntUsuarioMenuBusqueda.cs`, `CapaEntidad/EntMenuUsuario.cs`
- Crear: `CapaDato/DaoMenuUsuario.cs`
- Crear: `CapaNegocio/NegMenuUsuario.cs`
- Modificar: `CapaEntidad/CapaEntidad.csproj`, `CapaDato/CapaDato.csproj`, `CapaNegocio/CapaNegocio.csproj`

**Interfaces:**
- Consume: los 3 SPs de la Tarea 2.
- Produce:
  - `NegMenuUsuario.ListarUsuarios(string filtro)` → `List<EntUsuarioMenuBusqueda>`
  - `NegMenuUsuario.ListarMenuUsuario(string codUsuario)` → `List<EntMenuUsuario>`
  - `NegMenuUsuario.GuardarMenuUsuario(string codUsuario, string extrasCsv, string usuarioRegistro)` → `EntRespuesta`

- [ ] **Paso 1: Crear las entidades**

`CapaEntidad/EntUsuarioMenuBusqueda.cs`:

```csharp
namespace CapaEntidad
{
    public class EntUsuarioMenuBusqueda
    {
        public string Cod_Usuario { get; set; }
        public string Nom_Usuario { get; set; }
        public string Cedula { get; set; }
        public long Id_Perfil { get; set; }
        public string NombrePerfil { get; set; }
        public int TotalExtras { get; set; }
    }
}
```

`CapaEntidad/EntMenuUsuario.cs`:

```csharp
namespace CapaEntidad
{
    public class EntMenuUsuario
    {
        public int Id_Menu { get; set; }
        public int Id_MenuPadre { get; set; }
        public string Titulo { get; set; }
        public string Class_Icon { get; set; }
        public int ActivoPerfil { get; set; }
        public int ActivoUsuario { get; set; }
    }
}
```

- [ ] **Paso 2: Registrar las entidades en `CapaEntidad/CapaEntidad.csproj`**

Junto a `<Compile Include="EntMenuPerfil.cs" />` (línea 75) agregar:

```xml
    <Compile Include="EntMenuUsuario.cs" />
    <Compile Include="EntUsuarioMenuBusqueda.cs" />
```

- [ ] **Paso 3: Crear `CapaDato/DaoMenuUsuario.cs`**

```csharp
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoMenuUsuario
    {
        /// <summary>Busca usuarios activos por nombre, código o cédula, con su perfil.</summary>
        public static List<EntUsuarioMenuBusqueda> ListarUsuarios(string filtro)
        {
            List<EntUsuarioMenuBusqueda> lista = new List<EntUsuarioMenuBusqueda>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarUsuariosMenu", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Filtro", SqlDbType.VarChar, 100).Value = filtro ?? string.Empty;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntUsuarioMenuBusqueda()
                        {
                            Cod_Usuario = dr["Cod_Usuario"].ToString(),
                            Nom_Usuario = dr["Nom_Usuario"].ToString(),
                            Cedula = dr["Cedula"].ToString(),
                            Id_Perfil = Convert.ToInt64(dr["Id_Perfil"].ToString()),
                            NombrePerfil = dr["NombrePerfil"].ToString(),
                            TotalExtras = Convert.ToInt32(dr["TotalExtras"].ToString())
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>Lista todos los menús con su origen (perfil / extra) para el usuario.</summary>
        public static List<EntMenuUsuario> ListarMenuUsuario(string codUsuario)
        {
            List<EntMenuUsuario> lista = new List<EntMenuUsuario>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarMenuUsuario", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CodUsuario", SqlDbType.VarChar, 50).Value = codUsuario ?? string.Empty;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntMenuUsuario()
                        {
                            Id_Menu = Convert.ToInt32(dr["Id_Menu"].ToString()),
                            Id_MenuPadre = Convert.ToInt32(dr["Id_MenuPadre"].ToString()),
                            Titulo = dr["Titulo"].ToString(),
                            Class_Icon = dr["Class_Icon"].ToString(),
                            ActivoPerfil = Convert.ToInt32(dr["ActivoPerfil"].ToString()),
                            ActivoUsuario = Convert.ToInt32(dr["ActivoUsuario"].ToString())
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>Guarda (atómico) los módulos extra del usuario; el SP agrega los padres.</summary>
        public static EntRespuesta GuardarMenuUsuario(string codUsuario, string extrasCsv, string usuarioRegistro)
        {
            EntRespuesta respuesta = new EntRespuesta()
            {
                estado = "0",
                resultado = "0",
                tipoMensaje = "danger",
                mensaje = ""
            };

            try
            {
                DaoReporTareaAranda conexion = new DaoReporTareaAranda();

                using (SqlConnection cnx = conexion.conectar())
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_GuardarMenuUsuario", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@CodUsuario", SqlDbType.VarChar, 50).Value = codUsuario ?? string.Empty;
                    cmd.Parameters.Add("@ExtrasCsv", SqlDbType.VarChar, -1).Value = extrasCsv ?? string.Empty;
                    cmd.Parameters.Add("@UsuarioRegistro", SqlDbType.VarChar, 50).Value = usuarioRegistro ?? "SISTEMA";

                    cnx.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            int respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());
                            respuesta.resultado = respuestaSP.ToString();
                            respuesta.mensaje = dr["Mensaje"].ToString();

                            if (respuestaSP > 0)
                            {
                                respuesta.estado = "1";
                                respuesta.tipoMensaje = "success";
                            }
                            else
                            {
                                respuesta.estado = "0";
                                respuesta.tipoMensaje = "warning";
                            }
                        }
                        else
                        {
                            respuesta.mensaje = "El procedimiento no devolvió información.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.estado = "0";
                respuesta.resultado = "0";
                respuesta.tipoMensaje = "danger";
                respuesta.mensaje = "Ocurrió un error al guardar los módulos del usuario. Detalle: " + ex.Message;
            }

            return respuesta;
        }
    }
}
```

- [ ] **Paso 4: Registrar el DAO en `CapaDato/CapaDato.csproj`**

Junto a `<Compile Include="DaoMenuPerfil.cs" />` agregar:

```xml
    <Compile Include="DaoMenuUsuario.cs" />
```

- [ ] **Paso 5: Crear `CapaNegocio/NegMenuUsuario.cs`**

```csharp
using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegMenuUsuario
    {
        public static List<EntUsuarioMenuBusqueda> ListarUsuarios(string filtro)
        {
            return DaoMenuUsuario.ListarUsuarios(filtro);
        }

        public static List<EntMenuUsuario> ListarMenuUsuario(string codUsuario)
        {
            return DaoMenuUsuario.ListarMenuUsuario(codUsuario);
        }

        public static EntRespuesta GuardarMenuUsuario(string codUsuario, string extrasCsv, string usuarioRegistro)
        {
            return DaoMenuUsuario.GuardarMenuUsuario(codUsuario, extrasCsv, usuarioRegistro);
        }
    }
}
```

- [ ] **Paso 6: Registrar el negocio en `CapaNegocio/CapaNegocio.csproj`**

Junto a `<Compile Include="NegMenuPerfil.cs" />` (línea 201) agregar:

```xml
    <Compile Include="NegMenuUsuario.cs" />
```

- [ ] **Paso 7: Compilar**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```

Esperado: `0 Error(s)`. Si dice `no se encontró el tipo o el espacio de nombres 'EntUsuarioMenuBusqueda'`, falta el `<Compile Include=...>` del Paso 2.

- [ ] **Paso 8: Commit**

```bash
git add CapaEntidad/EntMenuUsuario.cs CapaEntidad/EntUsuarioMenuBusqueda.cs CapaEntidad/CapaEntidad.csproj CapaDato/DaoMenuUsuario.cs CapaDato/CapaDato.csproj CapaNegocio/NegMenuUsuario.cs CapaNegocio/CapaNegocio.csproj
git commit -m "feat(backend): capas de modulos extra por usuario"
```

---

## Tarea 5: El sidebar pasa el `Cod_Usuario`

Es el cambio de mayor riesgo del plan: si sale mal, nadie ve el menú. Va solo, para poder revertirlo sin arrastrar nada más.

**Archivos:**
- Modificar: `CapaDato/DaoMenuDos.cs:160-206`
- Modificar: `CapaNegocio/NegMenuDos.cs:16-19`
- Modificar: `ReporteTareas/Formulario/Master.Master.cs:25`

**Interfaces:**
- Consume: `Sp_RTA_ConsultarMenuPerfilUsuario @tipoPerfil, @CodUsuario` (Tarea 3).
- Produce: `NegMenuDos.Sp_RTA_ConsultarMenuPerfilUsuario(int tipo, string codUsuario)`.

- [ ] **Paso 1: Modificar el DAO**

En `CapaDato/DaoMenuDos.cs`, cambiar la firma de la línea 160 y agregar el parámetro después de la línea 173:

```csharp
        public static List<EntMenuDos> Sp_RTA_ConsultarMenuPerfilUsuario(int tipo, string codUsuario = null)
```

y justo después de `cmd.Parameters.AddWithValue("@tipoPerfil", tipo);`:

```csharp
                cmd.Parameters.AddWithValue("@CodUsuario",
                    string.IsNullOrEmpty(codUsuario) ? (object)DBNull.Value : codUsuario);
```

El valor por defecto `null` mantiene compilando cualquier llamada existente.

- [ ] **Paso 2: Modificar el negocio**

En `CapaNegocio/NegMenuDos.cs`:

```csharp
        public static List<EntMenuDos> Sp_RTA_ConsultarMenuPerfilUsuario(int tipo, string codUsuario = null)
        {
            return DaoMenuDos.Sp_RTA_ConsultarMenuPerfilUsuario(tipo, codUsuario);
        }
```

- [ ] **Paso 3: Modificar el Master**

En `ReporteTareas/Formulario/Master.Master.cs`, línea 25, cambiar:

```csharp
            menuDos = NegMenuDos.Sp_RTA_ConsultarMenuPerfilUsuario(Idperfil);
```

por:

```csharp
            menuDos = NegMenuDos.Sp_RTA_ConsultarMenuPerfilUsuario(Idperfil, CodUnico);
```

`CodUnico` ya está poblado en la línea 20. **No tocar nada más de este archivo** — en particular las líneas 43-45, donde `view2` se declara pero se usa `view`: funciona porque ambos son el mismo `DefaultView`, y cambiarlo puede romper el render.

- [ ] **Paso 4: Verificar que no se tocó la copia huérfana**

```bash
git status --short CapaNegocio/CapaDato/
```

Esperado: **sin salida**. Si aparece `CapaNegocio/CapaDato/DaoMenuDos.cs`, revertir ese archivo: no compila y confunde.

- [ ] **Paso 5: Compilar**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```

Esperado: `0 Error(s)`.

- [ ] **Paso 6: Probar en el navegador (obligatorio)**

Levantar la app en IIS Express 32 bits, iniciar sesión con un usuario **sin extras** y confirmar que el menú lateral se ve **exactamente igual que antes**. Contar los ítems del sidebar y compararlos con la línea base del Paso 1 de la Tarea 3.

Si el menú sale **vacío**, la causa casi segura es que el SP dejó de devolver alguna de las seis columnas: el `catch` de `DaoMenuDos.cs:195` traga la excepción y devuelve `null`. Revisar el SP de la Tarea 3.

- [ ] **Paso 7: Probar que un extra aparece**

Reemplazar `<TuCodUsuario>` por el `Cod_Usuario` de la cuenta con la que estás probando en el navegador (el mismo que la app guarda en `Session["Cod_Usuario"]`; se obtiene con `SELECT Cod_Usuario FROM dbo.R_Usuarios WHERE Log_Usuario = '<tu login>'`).

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; DECLARE @m INT; SELECT TOP 1 @m=Id_Menu FROM dbo.MenuDos WHERE ISNULL(Id_MenuPadre,0)=0 AND Id_Menu NOT IN (SELECT id_Menu FROM dbo.PerfilMenu WHERE IdPerfil=(SELECT Id_Perfil FROM dbo.R_Usuarios WHERE Cod_Usuario='<TuCodUsuario>') AND Estado=0); INSERT INTO dbo.R_UsuarioMenu (Cod_Usuario, Id_Menu, Estado, Usuario_Registro) VALUES ('<TuCodUsuario>', @m, 'A', 'PRUEBA'); SELECT Id_Menu=@m, Titulo=(SELECT Titulo FROM dbo.MenuDos WHERE Id_Menu=@m);"
```

Cerrar sesión, volver a entrar con ese usuario y confirmar que **el ítem nuevo aparece** en el sidebar. Luego limpiar:

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "DELETE FROM dbo.R_UsuarioMenu WHERE Usuario_Registro='PRUEBA';"
```

- [ ] **Paso 8: Commit**

```bash
git add CapaDato/DaoMenuDos.cs CapaNegocio/NegMenuDos.cs "ReporteTareas/Formulario/Master.Master.cs"
git commit -m "feat(menu): el sidebar incluye los modulos extra del usuario en sesion"
```

---

## Tarea 6: Handler `AdministrarMenuUsuario.ashx`

**Archivos:**
- Crear: `ReporteTareas/Formulario/AdministrarMenuUsuario.ashx`, `AdministrarMenuUsuario.ashx.cs`
- Modificar: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consume: `NegMenuUsuario` (Tarea 4).
- Produce: endpoint POST JSON con formato `[{"action": "...", "parameters": {...}}]` y acciones `BuscarUsuarios`, `ListaMenuUsuario`, `GuardarMenuUsuario`.

- [ ] **Paso 1: Crear el markup `AdministrarMenuUsuario.ashx`**

```
<%@ WebHandler Language="C#" CodeBehind="AdministrarMenuUsuario.ashx.cs" Class="JsonJQueryNetMenuUsuario.AdministrarMenuUsuario" %>
```

- [ ] **Paso 2: Crear `AdministrarMenuUsuario.ashx.cs`**

```csharp
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetMenuUsuario
{
    /// <summary>
    /// Handler de la pantalla "Parametrización de módulos por usuario".
    /// Acciones: BuscarUsuarios, ListaMenuUsuario, GuardarMenuUsuario.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarMenuUsuario : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            StringBuilder responseAction = new StringBuilder();

            if (context.Request.ContentType != null && context.Request.ContentType.Contains("json"))
            {
                var inputStream = new System.IO.StreamReader(context.Request.InputStream);
                var inputJson = inputStream.ReadToEnd();

                JavaScriptSerializer i = new JavaScriptSerializer();
                dynamic parametros = i.Deserialize(inputJson.ToString(), typeof(object));

                var parameters = parametros[0]["parameters"];
                var Action = parametros[0]["action"];
                bool existAction = false;

                if (Action == "BuscarUsuarios")
                {
                    existAction = true;
                    responseAction.Append(BuscarUsuarios(parameters));
                }

                if (Action == "ListaMenuUsuario")
                {
                    existAction = true;
                    responseAction.Append(ListaMenuUsuario(parameters));
                }

                if (Action == "GuardarMenuUsuario")
                {
                    existAction = true;
                    responseAction.Append(GuardarMenuUsuario(context, parameters));
                }

                if (!existAction)
                {
                    responseAction.Append(responseMessage("0", "No existe la acción solicitada.", "danger"));
                }
            }

            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Charset = "utf-8";
            context.Response.Write(responseAction.ToString());
        }

        private string BuscarUsuarios(dynamic campos)
        {
            try
            {
                string filtro = "";
                try { filtro = Convert.ToString(campos["filtro"]); }
                catch { filtro = ""; }

                return ToJson(NegMenuUsuario.ListarUsuarios(filtro));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al buscar usuarios. " + ex.Message, "danger");
            }
        }

        private string ListaMenuUsuario(dynamic campos)
        {
            try
            {
                string codUsuario = "";
                try { codUsuario = Convert.ToString(campos["codUsuario"]); }
                catch { codUsuario = ""; }

                if (string.IsNullOrEmpty(codUsuario))
                {
                    return responseMessage("0", "Debe seleccionar un usuario.", "warning");
                }

                return ToJson(NegMenuUsuario.ListarMenuUsuario(codUsuario));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar los módulos. " + ex.Message, "danger");
            }
        }

        private string GuardarMenuUsuario(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = "";
                try { codUsuario = Convert.ToString(campos["codUsuario"]); }
                catch { codUsuario = ""; }

                if (string.IsNullOrEmpty(codUsuario))
                {
                    return responseMessage("0", "Debe seleccionar un usuario.", "warning");
                }

                // 'extras' es un arreglo de ids (enteros). Se arma un CSV validado.
                List<string> ids = new List<string>();
                try
                {
                    var extras = campos["extras"];
                    if (extras != null)
                    {
                        foreach (var v in extras)
                        {
                            int id = Convert.ToInt32(v);
                            if (id > 0) { ids.Add(id.ToString()); }
                        }
                    }
                }
                catch { ids = new List<string>(); }

                string csv = string.Join(",", ids);

                // Quién asigna sale de la sesión, nunca del cliente.
                string usuarioRegistro = "SISTEMA";
                if (context.Session != null && context.Session["Cod_Usuario"] != null)
                {
                    usuarioRegistro = context.Session["Cod_Usuario"].ToString();
                }

                EntRespuesta respuesta = NegMenuUsuario.GuardarMenuUsuario(codUsuario, csv, usuarioRegistro);
                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al guardar los módulos del usuario. " + ex.Message, "danger");
            }
        }

        private string responseMessage(string estado, string mensaje, string tipoMensaje, string resultado = "")
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta.estado = estado;
            respuesta.mensaje = mensaje;
            respuesta.tipoMensaje = tipoMensaje;
            respuesta.resultado = resultado;

            return ToJson(respuesta);
        }

        private static string ToJson(object obj)
        {
            if (obj == null) return string.Empty;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            return serializer.Serialize(obj);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
```

**Nota:** este handler implementa `IRequiresSessionState` (a diferencia de `AdministrarMenuPerfil`) porque necesita leer `Session["Cod_Usuario"]` para la auditoría. Sin esa interfaz, `context.Session` viene `null` y el registro quedaría siempre como `SISTEMA`.

- [ ] **Paso 3: Registrar en `ReporteTareas/ReporteTareas.csproj`**

Junto a `<Content Include="Formulario\AdministrarMenuPerfil.ashx" />` (línea 1298):

```xml
    <Content Include="Formulario\AdministrarMenuUsuario.ashx" />
```

Junto al `<Compile>` de `AdministrarMenuPerfil.ashx.cs` (línea 1444):

```xml
    <Compile Include="Formulario\AdministrarMenuUsuario.ashx.cs">
      <DependentUpon>AdministrarMenuUsuario.ashx</DependentUpon>
      <SubType>Code</SubType>
    </Compile>
```

- [ ] **Paso 4: Compilar**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```

Esperado: `0 Error(s)`.

- [ ] **Paso 5: Probar el handler por HTTP**

Con la app corriendo y sesión iniciada, desde la consola del navegador (F12) en cualquier página de `/Formulario/`:

```javascript
$.ajax({type:"POST", url:"AdministrarMenuUsuario.ashx",
  data: JSON.stringify([{action:"BuscarUsuarios", parameters:{filtro:"a"}}]),
  contentType:"application/json; charset=utf-8", dataType:"json",
  success: function(r){ console.log(r.length, r[0]); }});
```

Esperado: un arreglo con objetos que traen `Cod_Usuario`, `Nom_Usuario`, `NombrePerfil`, `TotalExtras`. **Verificar que las tildes de los nombres se ven bien** (`MARTÍNEZ`, no `MARTÃNEZ`).

- [ ] **Paso 6: Commit**

```bash
git add "ReporteTareas/Formulario/AdministrarMenuUsuario.ashx" "ReporteTareas/Formulario/AdministrarMenuUsuario.ashx.cs" ReporteTareas/ReporteTareas.csproj
git commit -m "feat(handler): AdministrarMenuUsuario.ashx con identidad por sesion"
```

---

## Tarea 7: Pantalla `ParametrizacionMenuUsuario.aspx`

**Archivos:**
- Crear: `ReporteTareas/Formulario/ParametrizacionMenuUsuario.aspx`, `.aspx.cs`, `.aspx.designer.cs`
- Modificar: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consume: `AdministrarMenuUsuario.ashx` (Tarea 6).
- Produce: los ids del DOM que consume el JS de la Tarea 8: `txtBuscar`, `btnBuscar`, `datosTablaUsuarios`, `panelDetalle`, `txtCodUsuarioSel`, `txtNombreUsuarioSel`, `txtPerfilSel`, `btnGuardar`, `datosArbolMenu`, `modalMensajeInformativo`, `modalMensajeInformativoTipo`, `MensajeInformativo`.

- [ ] **Paso 1: Crear `ParametrizacionMenuUsuario.aspx`**

```aspx
<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ParametrizacionMenuUsuario.aspx.cs" Inherits="ReporteTareas.Formulario.ParametrizacionMenuUsuario" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/parametrizacionMenuUsuario.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Parametrización de módulos por usuario</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">
                    Módulos adicionales a los que ya da el perfil del usuario. Lo heredado del perfil no se puede quitar desde aquí.
                    <div style="display: none">
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                        <asp:TextBox ID="txtLoginUsuario" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                        <asp:TextBox ID="txtIdCliente" runat="server" CssClass="form-control" Enabled="False" Visible="true" />
                    </div>
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="form-group col-lg-5">
                            <label>Buscar usuario (nombre, código o cédula):</label>
                            <input type="text" class="form-control" id="txtBuscar" placeholder="Escriba para filtrar..." onkeypress="if(event.keyCode==13){BuscarUsuarios();return false;}">
                        </div>
                        <div class="form-group col-lg-4" style="padding-top: 25px">
                            <button id="btnBuscar" onclick="BuscarUsuarios()" type="button" class="btn btn-primary">Buscar</button>
                        </div>
                    </div>

                    <div class="col-lg-12" style="padding: 0px">
                        <div class="panel panel-default">
                            <div class="panel-heading"><h4>Usuarios</h4></div>
                            <div class="panel-body" style="height: 260px; overflow-y: auto; overflow-x: auto;">
                                <div id="datosTablaUsuarios" style="padding: 0px"></div>
                            </div>
                        </div>
                    </div>

                    <div class="col-lg-12" style="padding: 0px; display: none" id="panelDetalle">
                        <div class="panel panel-default">
                            <div class="panel-heading"><h4>Módulos del usuario</h4></div>
                            <div class="panel-body">
                                <div class="row">
                                    <input type="hidden" id="txtCodUsuarioSel" />
                                    <div class="form-group col-lg-5">
                                        <label>Usuario:</label>
                                        <input type="text" class="form-control" id="txtNombreUsuarioSel" disabled />
                                    </div>
                                    <div class="form-group col-lg-4">
                                        <label>Perfil:</label>
                                        <input type="text" class="form-control" id="txtPerfilSel" disabled />
                                    </div>
                                    <div class="form-group col-lg-3" style="padding-top: 25px">
                                        <button id="btnGuardar" onclick="GuardarModulos()" type="button" class="btn btn-success">Guardar</button>
                                    </div>
                                </div>
                                <div style="margin-bottom: 8px">
                                    <span class="label label-default">Perfil</span> heredado, no se puede desmarcar &nbsp;
                                    <span class="label label-success">Extra</span> asignado a este usuario
                                </div>
                                <div style="height: 400px; overflow-y: auto; overflow-x: auto;">
                                    <div id="datosArbolMenu" style="padding: 0px"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Informativo -->
        <div class="modal fade" id="modalMensajeInformativo" tabindex="-1" role="dialog" aria-labelledby="modalMensajeInformativoLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3" id="modalMensajeInformativoTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="myModalLabel">Informativo</h4>
                    </div>
                    <div class="modal-body" id="MensajeInformativo"></div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->
    </div>
</asp:Content>
```

**Guardar este archivo en UTF-8 con BOM** (tiene tildes).

- [ ] **Paso 2: Crear `ParametrizacionMenuUsuario.aspx.cs`**

```csharp
using ReporteTareas.Controles;
using SeguridadAppHelper;
using System;

namespace ReporteTareas.Formulario
{
    public partial class ParametrizacionMenuUsuario : System.Web.UI.Page
    {
        #region Variables
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            if (Session["UserLogin"] != null)
            {
                if (!IsPostBack)
                {
                    try
                    {
                        string CodUnico = Session["Cod_Usuario"].ToString();
                        SeguridadHelper seguridad = new SeguridadHelper();
                        txtUsuario.Text = seguridad.Encripta(CodUnico.ToString());
                        txtLoginUsuario.Text = Session["UserLogin"].ToString();

                        if (Session["IdCliente"] != null)
                        {
                            txtIdCliente.Text = Session["IdCliente"].ToString();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
```

- [ ] **Paso 3: Crear `ParametrizacionMenuUsuario.aspx.designer.cs`**

```csharp
//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ReporteTareas.Formulario
{
    public partial class ParametrizacionMenuUsuario
    {
        /// <summary>Control txtUsuario.</summary>
        protected global::System.Web.UI.WebControls.TextBox txtUsuario;

        /// <summary>Control txtLoginUsuario.</summary>
        protected global::System.Web.UI.WebControls.TextBox txtLoginUsuario;

        /// <summary>Control txtIdCliente.</summary>
        protected global::System.Web.UI.WebControls.TextBox txtIdCliente;
    }
}
```

- [ ] **Paso 4: Registrar en `ReporteTareas/ReporteTareas.csproj`**

Junto a `<Content Include="Formulario\ParametrizacionMenuPerfil.aspx" />` (línea 1130):

```xml
    <Content Include="Formulario\ParametrizacionMenuUsuario.aspx" />
```

Junto a los `<Compile>` de `ParametrizacionMenuPerfil` (líneas 1893-1899):

```xml
    <Compile Include="Formulario\ParametrizacionMenuUsuario.aspx.cs">
      <DependentUpon>ParametrizacionMenuUsuario.aspx</DependentUpon>
      <SubType>ASPXCodeBehind</SubType>
    </Compile>
    <Compile Include="Formulario\ParametrizacionMenuUsuario.aspx.designer.cs">
      <DependentUpon>ParametrizacionMenuUsuario.aspx</DependentUpon>
    </Compile>
```

- [ ] **Paso 5: Compilar**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```

Esperado: `0 Error(s)`.

- [ ] **Paso 6: Abrir la pantalla**

Navegar a `/Formulario/ParametrizacionMenuUsuario.aspx`. Esperado: la página carga con el buscador visible, el panel de detalle oculto y **las tildes correctas** en los títulos ("Parametrización de módulos por usuario"). La tabla y el árbol están vacíos: el JS llega en la Tarea 8.

- [ ] **Paso 7: Commit**

```bash
git add "ReporteTareas/Formulario/ParametrizacionMenuUsuario.aspx" "ReporteTareas/Formulario/ParametrizacionMenuUsuario.aspx.cs" "ReporteTareas/Formulario/ParametrizacionMenuUsuario.aspx.designer.cs" ReporteTareas/ReporteTareas.csproj
git commit -m "feat(ui): pantalla de modulos por usuario"
```

---

## Tarea 8: JavaScript de la pantalla

**Archivos:**
- Crear: `ReporteTareas/js/parametrizacionMenuUsuario.js`
- Modificar: `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consume: los ids del DOM de la Tarea 7 y el handler de la Tarea 6.
- Produce: funciones globales `BuscarUsuarios()`, `SeleccionarUsuario(indice)`, `GuardarModulos()`, invocadas desde `onclick`/`onkeypress` del `.aspx`.

- [ ] **Paso 1: Crear el archivo (UTF-8 CON BOM)**

```javascript
/* ============================================================================
   Pantalla: Parametrizacion de modulos por usuario
   Handler : AdministrarMenuUsuario.ashx
   Regla   : el usuario ve los menus de su perfil MAS estos extras.
             Lo heredado del perfil sale marcado y deshabilitado.
   ============================================================================ */

var _usuarios = [];
var _modulos = [];

$(document).ready(function () {
    BuscarUsuarios();
});

function PostMenuUsuario(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);
    $.ajax({
        type: "POST",
        url: "AdministrarMenuUsuario.ashx",
        data: datos,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (respuesta) { onSuccess(respuesta); },
        error: function () {
            MostrarMensaje("La operación está tomando demasiado tiempo o la red está saturada. Intente nuevamente.", "danger");
        }
    });
}

function BuscarUsuarios() {
    var filtro = $("#txtBuscar").val();

    PostMenuUsuario("BuscarUsuarios", { "filtro": filtro }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }
        _usuarios = respuesta || [];
        RenderTablaUsuarios(_usuarios);
        $("#panelDetalle").hide();
    });
}

function RenderTablaUsuarios(lista) {
    var info = "";
    info += "<table width='100%' class='table table-striped table-bordered table-hover dataTable no-footer'>";
    info += "<thead><tr role='row'>";
    info += "<th style='text-align:center'>Módulos</th>";
    info += "<th>Código</th>";
    info += "<th>Nombre</th>";
    info += "<th>Cédula</th>";
    info += "<th>Perfil</th>";
    info += "<th style='text-align:center'>Extras</th>";
    info += "</tr></thead><tbody>";

    if (lista.length === 0) {
        info += "<tr><td colspan='6' style='text-align:center'>No existen usuarios para esta búsqueda.</td></tr>";
    }

    $.each(lista, function (i, item) {
        var extras = (item.TotalExtras > 0)
            ? "<span class='label label-success'>" + item.TotalExtras + "</span>"
            : "";

        info += "<tr role='row'>";
        info += "<td style='text-align:center'>";
        info += "<i class='fa fa-hand-o-right' title='Ver módulos' style='cursor:pointer' onclick='SeleccionarUsuario(" + i + ")'></i>";
        info += "</td>";
        info += "<td>" + Escapar(item.Cod_Usuario) + "</td>";
        info += "<td>" + Escapar(item.Nom_Usuario) + "</td>";
        info += "<td>" + Escapar(item.Cedula) + "</td>";
        info += "<td>" + Escapar(item.NombrePerfil) + "</td>";
        info += "<td style='text-align:center'>" + extras + "</td>";
        info += "</tr>";
    });

    info += "</tbody></table>";
    $("#datosTablaUsuarios").html(info);
}

function SeleccionarUsuario(indice) {
    var item = _usuarios[indice];
    if (item == null) { return; }

    $("#txtCodUsuarioSel").val(item.Cod_Usuario);
    $("#txtNombreUsuarioSel").val(item.Nom_Usuario + " (" + item.Cod_Usuario + ")");
    $("#txtPerfilSel").val(item.NombrePerfil + " (" + item.Id_Perfil + ")");

    PostMenuUsuario("ListaMenuUsuario", { "codUsuario": item.Cod_Usuario }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }
        _modulos = respuesta || [];
        RenderArbol(_modulos);
        $("#panelDetalle").show();
    });
}

function RenderArbol(lista) {
    var padres = $.grep(lista, function (m) { return m.Id_MenuPadre == 0; });

    if (padres.length === 0) {
        $("#datosArbolMenu").html("<p>No existen módulos.</p>");
        return;
    }

    var info = "";
    $.each(padres, function (i, padre) {
        info += "<div style='margin:4px 0'>";
        info += "<label style='font-weight:bold'>";
        info += PintarCheck(padre, "chkPadre", 0);
        info += "<i class='" + EscaparAttr(padre.Class_Icon) + "'></i> " + Escapar(padre.Titulo);
        info += "</label>";
        info += Etiqueta(padre);

        var hijos = $.grep(lista, function (m) { return m.Id_MenuPadre == padre.Id_Menu; });
        $.each(hijos, function (j, hijo) {
            info += "<div style='margin-left:28px'>";
            info += "<label>";
            info += PintarCheck(hijo, "chkHijo", padre.Id_Menu);
            info += "<i class='" + EscaparAttr(hijo.Class_Icon) + "'></i> " + Escapar(hijo.Titulo);
            info += "</label>";
            info += Etiqueta(hijo);
            info += "</div>";
        });
        info += "</div>";
    });

    $("#datosArbolMenu").html(info);
}

/* Heredado del perfil -> marcado y deshabilitado. Extra -> marcado y editable. */
function PintarCheck(item, clase, idPadre) {
    var esPerfil = (item.ActivoPerfil == 1);
    var marcado = (esPerfil || item.ActivoUsuario == 1) ? "checked" : "";
    var bloqueado = esPerfil ? "disabled" : "";
    var evento = (clase === "chkHijo")
        ? " onchange='OnHijoChange(" + idPadre + ")'"
        : " onchange='OnPadreChange(" + item.Id_Menu + ")'";

    var html = "<input type='checkbox' class='chkModulo " + clase + "'";
    html += " data-menu='" + EscaparAttr(String(item.Id_Menu)) + "'";
    if (clase === "chkHijo") {
        html += " data-padre='" + EscaparAttr(String(idPadre)) + "'";
    }
    html += " " + marcado + " " + bloqueado + evento + " /> ";
    return html;
}

function Etiqueta(item) {
    if (item.ActivoPerfil == 1) {
        return " <span class='label label-default'>Perfil</span>";
    }
    if (item.ActivoUsuario == 1) {
        return " <span class='label label-success'>Extra</span>";
    }
    return "";
}

/* Marcar un hijo auto-marca su padre, salvo que el padre ya venga del perfil
   (en ese caso ya esta marcado y deshabilitado). */
function OnHijoChange(idPadre) {
    var algunHijo = $(".chkHijo[data-padre='" + idPadre + "']:checked").length > 0;
    if (algunHijo) {
        $(".chkPadre[data-menu='" + idPadre + "']").not(":disabled").prop("checked", true);
    }
}

/* Desmarcar un padre editable desmarca sus hijos editables. */
function OnPadreChange(idPadre) {
    var padreChecked = $(".chkPadre[data-menu='" + idPadre + "']").is(":checked");
    if (!padreChecked) {
        $(".chkHijo[data-padre='" + idPadre + "']").not(":disabled").prop("checked", false);
    }
}

function GuardarModulos() {
    var codUsuario = $("#txtCodUsuarioSel").val();
    if (codUsuario == null || codUsuario === "") {
        MostrarMensaje("Debe seleccionar un usuario.", "warning");
        return;
    }

    /* Solo viajan los editables marcados: los heredados del perfil no se guardan. */
    var extras = [];
    $(".chkModulo:checked").not(":disabled").each(function () {
        extras.push(parseInt($(this).attr("data-menu"), 10));
    });

    $("#btnGuardar").prop("disabled", true);
    PostMenuUsuario("GuardarMenuUsuario", { "codUsuario": codUsuario, "extras": extras }, function (respuesta) {
        $("#btnGuardar").prop("disabled", false);
        if (respuesta == null) {
            MostrarMensaje("No se recibió respuesta del servidor.", "danger");
            return;
        }
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado == "1") {
            BuscarUsuarios();
        }
    });
}

/* ----------------------------- utilitarios ------------------------------- */

function MostrarMensaje(mensaje, tipo) {
    var color = "#fcf8e3";
    if (tipo == "success") { color = "#dff0d8"; }
    if (tipo == "danger") { color = "#f2dede"; }
    if (tipo == "warning") { color = "#fcf8e3"; }
    $("#modalMensajeInformativoTipo").css("background", color);
    $("#MensajeInformativo").html(mensaje);
    $("#modalMensajeInformativo").modal("show");
}

function Escapar(texto) {
    if (texto == null) { return ""; }
    return $("<div>").text(texto).html();
}

function EscaparAttr(texto) {
    return Escapar(texto).replace(/'/g, "&#39;").replace(/"/g, "&quot;");
}
```

- [ ] **Paso 2: Verificar que el archivo tiene BOM**

```powershell
$bytes = [System.IO.File]::ReadAllBytes("ReporteTareas\js\parametrizacionMenuUsuario.js"); "{0:X2} {1:X2} {2:X2}" -f $bytes[0], $bytes[1], $bytes[2]
```

Esperado: `EF BB BF`. Si no, reescribir el archivo con BOM o las tildes de los mensajes saldrán corruptas.

- [ ] **Paso 3: Registrar en `ReporteTareas/ReporteTareas.csproj`**

Junto a `<Content Include="js\parametrizacionMenuPerfil.js" />` (línea 1389):

```xml
    <Content Include="js\parametrizacionMenuUsuario.js" />
```

- [ ] **Paso 4: Compilar**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```

Esperado: `0 Error(s)`.

- [ ] **Paso 5: Probar el flujo completo en el navegador**

Abrir `/Formulario/ParametrizacionMenuUsuario.aspx` y verificar, en orden:

1. La tabla de usuarios carga sola, con la columna **Perfil** poblada.
2. Escribir un apellido y pulsar **Buscar** → la lista se filtra.
3. Click en la manito de un usuario → aparece el panel con **Usuario** y **Perfil** en solo lectura, y el árbol.
4. Los ítems del perfil salen **marcados, en gris, con la etiqueta "Perfil"**, y **no se pueden desmarcar**.
5. Marcar un módulo hijo cuyo padre esté libre → el padre se auto-marca.
6. **Guardar** → mensaje de éxito, la tabla se recarga y el usuario ahora muestra un contador en la columna **Extras**.
7. Volver a entrar al usuario → los extras salen marcados con la etiqueta **"Extra"**.
8. Desmarcar todos los extras y guardar → el contador vuelve a vacío.
9. Confirmar que las tildes se ven bien en los mensajes del modal.

- [ ] **Paso 6: Verificar la auditoría**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; SELECT TOP 10 Cod_Usuario, Id_Menu, Estado, Usuario_Registro, Fecha_Registro FROM dbo.R_UsuarioMenu ORDER BY Fecha_Registro DESC;"
```

Esperado: `Usuario_Registro` con **tu** `Cod_Usuario` (el de la sesión), no `SISTEMA`. Si sale `SISTEMA`, falta `IRequiresSessionState` en el handler (Tarea 6).

- [ ] **Paso 7: Commit**

```bash
git add ReporteTareas/js/parametrizacionMenuUsuario.js ReporteTareas/ReporteTareas.csproj
git commit -m "feat(ui): buscador y arbol de modulos por usuario"
```

---

## Tarea 9: Registro en el menú y verificación de punta a punta

**Archivos:**
- Crear: `docs/superpowers/plans/sql/2026-08-03-modulos-extra-menu.sql`

**Interfaces:**
- Consume: la pantalla de la Tarea 7.
- Produce: la entrada de `ParametrizacionMenuUsuario.aspx` en `MenuDos` + `PerfilMenu`.

- [ ] **Paso 1: Escribir el script**

```sql
/* ============================================================================
   Registro en menu de "Modulos por Usuario"
   Base: ReporTarea — Padre: 20042 (Manejo de Perfiles) — Perfiles: 2,18,19
   Se excluye el perfil 1 a proposito, igual que en el registro de
   ParametrizacionMenuPerfil.aspx.
   Idempotente.
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

DECLARE @Titulo VARCHAR(200) = 'Modulos por Usuario';
DECLARE @Href   VARCHAR(200) = 'ParametrizacionMenuUsuario.aspx';
DECLARE @Icono  VARCHAR(200) = 'fa fa-user-plus';
DECLARE @Padre  INT          = 20042;
DECLARE @IdMenu INT;

SELECT @IdMenu = Id_Menu FROM dbo.MenuDos WHERE Href = @Href;

IF @IdMenu IS NULL
BEGIN
    INSERT INTO dbo.MenuDos (Titulo, Href, Class_Icon, Id_MenuPadre)
    VALUES (@Titulo, @Href, @Icono, @Padre);
    SET @IdMenu = SCOPE_IDENTITY();
END

DECLARE @Perfiles TABLE (IdPerfil INT);
INSERT INTO @Perfiles (IdPerfil) VALUES (2),(18),(19);

INSERT INTO dbo.PerfilMenu (IdPerfil, id_Menu, Estado)
SELECT p.IdPerfil, m.id_Menu, 0
FROM @Perfiles p
CROSS JOIN (SELECT @IdMenu AS id_Menu UNION SELECT @Padre) m
WHERE NOT EXISTS (SELECT 1 FROM dbo.PerfilMenu pm
                  WHERE pm.IdPerfil = p.IdPerfil AND pm.id_Menu = m.id_Menu);

UPDATE pm SET pm.Estado = 0
FROM dbo.PerfilMenu pm
JOIN @Perfiles p ON p.IdPerfil = pm.IdPerfil
WHERE pm.id_Menu IN (@IdMenu, @Padre);

SELECT Id_Menu = @IdMenu, Href = @Href;
GO
```

- [ ] **Paso 2: Ejecutar**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -i "docs/superpowers/plans/sql/2026-08-03-modulos-extra-menu.sql"
```

Esperado: devuelve el `Id_Menu` asignado.

- [ ] **Paso 3: Verificar idempotencia**

Ejecutar el mismo comando **una segunda vez**. Esperado: el **mismo** `Id_Menu`, sin filas duplicadas:

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; SELECT enMenuDos=COUNT(*) FROM dbo.MenuDos WHERE Href='ParametrizacionMenuUsuario.aspx';"
```

Esperado: `enMenuDos=1`.

- [ ] **Paso 4: Verificación de punta a punta**

1. Cerrar sesión y entrar con un usuario de perfil **18**. Confirmar que bajo **Manejo de Perfiles** aparece **Módulos por Usuario**.
2. Abrir la pantalla y darle a un usuario de prueba (uno de perfil 1, con pocos menús) un módulo extra que su perfil no tenga.
3. Cerrar sesión, entrar **con ese usuario de prueba** y confirmar que el módulo nuevo **aparece** en su menú lateral y que el resto de su menú sigue igual.
4. Entrar con **otro** usuario del mismo perfil 1 y confirmar que **no** ve ese módulo — la excepción es individual.
5. Volver a la pantalla, quitarle el extra al usuario de prueba y guardar. Confirmar que al reingresar ya no lo ve.

- [ ] **Paso 5: Confirmar que no quedaron datos de prueba**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 20 -h -1 -W -Q "SET NOCOUNT ON; SELECT Usuario_Registro, Estado, total=COUNT(*) FROM dbo.R_UsuarioMenu GROUP BY Usuario_Registro, Estado;"
```

Revisar que no haya filas con `Usuario_Registro='PRUEBA'`. Si las hay, borrarlas.

- [ ] **Paso 6: Commit**

```bash
git add docs/superpowers/plans/sql/2026-08-03-modulos-extra-menu.sql
git commit -m "feat(menu): registro de ParametrizacionMenuUsuario (perfiles 2,18,19)"
```

---

## Orden y reversión

Las tareas van en orden: 1 → 9. Las 1, 2 y 3 son solo base de datos y se pueden aplicar antes de compilar nada.

**Punto de no retorno:** la Tarea 5. Hasta la 4 inclusive, nada cambia el comportamiento de la app para los usuarios. Si algo falla después de la 5, revertir es:

```bash
git revert <sha-de-la-tarea-5>
```

y volver a compilar. El SP de la Tarea 3 puede quedarse: con `@CodUsuario` en `NULL` se comporta como el original.

## Notas de despliegue

- Los scripts SQL se aplican **antes** de publicar los binarios: la Tarea 5 llama al SP con un parámetro que solo existe después de la Tarea 3.
- Al publicar, verificar que `parametrizacionMenuUsuario.js` llegó al servidor. Si la pantalla sale sin comportamiento, revisar el `?v=` del `.aspx` y forzar recarga con Ctrl+F5.
