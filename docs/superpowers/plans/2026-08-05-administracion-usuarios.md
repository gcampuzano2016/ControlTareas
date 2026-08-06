# Administración de usuarios — Plan de implementación

> **Para agentes:** SUB-SKILL REQUERIDA: usar `superpowers:subagent-driven-development` (recomendado) o `superpowers:executing-plans` para ejecutar tarea por tarea. Los pasos usan checkbox (`- [ ]`).

**Spec:** `docs/superpowers/specs/2026-08-05-administracion-usuarios-design.md`

**Objetivo:** una pantalla para ver la tabla `R_Usuarios` y editar los datos de los usuarios ya registrados, más restablecer contraseñas, con bitácora de cambios.

**Arquitectura:** tabla nueva `R_UsuarioBitacora`, cuatro SPs, tres capas C#, handler `.ashx` que exige sesión, pantalla `.aspx` + JS. Mismo patrón que los módulos "Menús por perfil" y "Módulos extra por usuario".

**Stack:** ASP.NET WebForms (.NET Framework 4.6.1), SQL Server, jQuery, Bootstrap 3, MSBuild 2019.

## Restricciones globales

- **No hay framework de pruebas.** Cada tarea se verifica con `sqlcmd`, con compilación o en el navegador. Los pasos de verificación son obligatorios.
- **Conexión:** `sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 30 -h -1 -W -Q "..."`
  La conectividad **es intermitente**: si falla, reintenta antes de reportar BLOCKED.
- **MIDE, no confíes en los números de este plan.** Los conteos que aparecen aquí son de referencia y pueden estar desactualizados. Captura siempre la línea base real antes de comparar. (En el plan anterior dos números del plan estaban mal y el implementador acertó al medir.)
- **Scripts SQL idempotentes**, empiezan con `SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; GO`.
- **Mensajes fijos dentro de SPs: SIN TILDES.** En C# y JS **sí** llevan tildes.
- **`.js` y `.aspx` en UTF-8 CON BOM.** La herramienta de escritura **no pone BOM por defecto**: hay que reescribir el archivo explícitamente con `UTF8Encoding($true)` y verificar los bytes `EF BB BF` **después** de commitear.
- **Todo archivo nuevo va a su `.csproj`** o no compila ni se publica.
- **Este repositorio VERSIONA los binarios** (`bin/`, `obj/`). Van en un commit aparte del código, al final (Tarea 8). No incluir `.suo` ni `.user`.
- **Hay trabajo en curso ajeno sin commitear** (`CapaDato/DaoMarcacion.cs`, `CapaDato/DaoTareas.cs`, `CapaNegocio/NegMarcacion.cs`, `ReporteTareas/clases/EnvioCorreoHelper.cs`, `ReporteTareas/js/RegistroLaboral.js` y otros). **No commitear nada de eso.** Usar `git add` con rutas explícitas, nunca `git add -A`.
- **Nunca escribir un `try/catch` que envuelva un bucle completo** de validación: un elemento inválido no puede vaciar en silencio una lista ya acumulada (defecto real encontrado en el módulo anterior).
- **Compilar:** `MSBuild.exe ReporteTareas.sln /p:Configuration=Debug`. Con `/v:minimal` MSBuild no imprime "0 Error(s)"; usar exit code + ausencia de `error CS`/`error MSB` como evidencia.
- **La contraseña nunca se muestra, ni se registra, ni viaja hasheada desde el cliente.**

## Mapa de archivos

| Archivo | Responsabilidad | Tarea |
|---|---|---|
| `docs/superpowers/plans/sql/2026-08-05-usuarios-lectura.sql` | tabla bitácora + 2 SPs de lectura | 1 |
| `docs/superpowers/plans/sql/2026-08-05-usuarios-escritura.sql` | 2 SPs de escritura | 2 |
| `CapaEntidad/EntUsuarioAdmin.cs`, `EntUsuarioBitacora.cs` | DTOs | 3 |
| `CapaDato/DaoUsuarioAdmin.cs` | acceso a los 4 SPs | 3 |
| `CapaNegocio/NegUsuarioAdmin.cs` | pass-through | 3 |
| `ReporteTareas/Formulario/AdministrarUsuarios.ashx(.cs)` | handler JSON | 4 |
| `ReporteTareas/Formulario/ParametrizacionUsuarios.aspx(.cs/.designer.cs)` | pantalla | 5 |
| `ReporteTareas/js/parametrizacionUsuarios.js` | buscador, formulario, modales | 6 |
| `docs/superpowers/plans/sql/2026-08-05-usuarios-menu.sql` | registro en el menú | 7 |
| `bin/`, `obj/` | binarios recompilados | 8 |

---

## Tarea 1: Bitácora y SPs de lectura

**Archivos:** Crear `docs/superpowers/plans/sql/2026-08-05-usuarios-lectura.sql`

**Interfaces producidas:**
- `dbo.R_UsuarioBitacora(Id_Bitacora, Id_Usuario, Accion, Detalle, Usuario_Registro, Fecha_Registro)`
- `Sp_RTA_ListarUsuariosAdmin @Filtro VARCHAR(100)` → `Id_Usuario, Cod_Usuario, Nom_Usuario, Log_Usuario, E_Mail, Cedula, Departamento, Empresa, Cod_Sap, Cod_Jefe_Inm, MailCodJefeInm, Id_Perfil, NombrePerfil, Usuario_Estado`
- `Sp_RTA_ListarBitacoraUsuario @Id_Usuario NUMERIC(5)` → `Accion, Detalle, Usuario_Registro, Fecha_Registro`

- [ ] **Paso 1: Capturar la línea base**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 30 -h -1 -W -Q "SET NOCOUNT ON; SELECT 'usuarios totales='+CAST(COUNT(*) AS VARCHAR) FROM dbo.R_Usuarios; SELECT 'existe bitacora: '+CASE WHEN OBJECT_ID('dbo.R_UsuarioBitacora') IS NULL THEN 'NO' ELSE 'SI' END;"
```

Anota el total (referencia: ~244). Esperado que la bitácora **no** exista todavía.

- [ ] **Paso 2: Escribir el script**

```sql
/* ============================================================================
   Administracion de usuarios — bitacora y SPs de lectura
   Base: ReporTarea
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------- 1) Bitacora de cambios ---------- */
IF OBJECT_ID('dbo.R_UsuarioBitacora','U') IS NULL
BEGIN
    CREATE TABLE dbo.R_UsuarioBitacora
    (
        Id_Bitacora      INT IDENTITY(1,1) NOT NULL,
        Id_Usuario       NUMERIC(5)    NOT NULL,
        Accion           VARCHAR(30)   NOT NULL,
        Detalle          VARCHAR(2000) NOT NULL,
        Usuario_Registro VARCHAR(50)   NOT NULL,
        Fecha_Registro   DATETIME      NOT NULL CONSTRAINT DF_R_UsuarioBitacora_Fecha DEFAULT (GETDATE()),
        CONSTRAINT PK_R_UsuarioBitacora PRIMARY KEY (Id_Bitacora)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_R_UsuarioBitacora_Usuario' AND object_id = OBJECT_ID('dbo.R_UsuarioBitacora'))
    CREATE INDEX IX_R_UsuarioBitacora_Usuario ON dbo.R_UsuarioBitacora (Id_Usuario, Fecha_Registro DESC);
GO

/* ---------- 2) Listado de usuarios (activos e inactivos) ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarUsuariosAdmin','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarUsuariosAdmin;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarUsuariosAdmin
    @Filtro VARCHAR(100) = ''
AS
BEGIN
    SET NOCOUNT ON;
    SET @Filtro = LTRIM(RTRIM(ISNULL(@Filtro,'')));

    SELECT
        u.Id_Usuario,
        u.Cod_Usuario,
        u.Nom_Usuario,
        u.Log_Usuario,
        E_Mail         = ISNULL(u.E_Mail,''),
        Cedula         = ISNULL(u.Cedula,''),
        Departamento   = ISNULL(u.Departamento,''),
        Empresa        = ISNULL(u.Empresa,''),
        Cod_Sap        = ISNULL(u.Cod_Sap,''),
        Cod_Jefe_Inm   = ISNULL(u.Cod_Jefe_Inm,''),
        MailCodJefeInm = ISNULL(u.MailCodJefeInm,''),
        u.Id_Perfil,
        NombrePerfil   = ISNULL(p.NombrePerfil,'Sin perfil'),
        u.Usuario_Estado
    FROM dbo.R_Usuarios u
    LEFT JOIN dbo.Perfiles p ON p.IdPerfiles = u.Id_Perfil
    WHERE
    (
        @Filtro = ''
        OR u.Nom_Usuario LIKE '%' + @Filtro + '%'
        OR u.Cod_Usuario LIKE '%' + @Filtro + '%'
        OR ISNULL(u.Cedula,'') LIKE '%' + @Filtro + '%'
    )
    ORDER BY u.Nom_Usuario;
END
GO

/* ---------- 3) Historial de un usuario ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ListarBitacoraUsuario','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ListarBitacoraUsuario;
GO
CREATE PROCEDURE dbo.Sp_RTA_ListarBitacoraUsuario
    @Id_Usuario NUMERIC(5)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (50)
        b.Accion,
        b.Detalle,
        b.Usuario_Registro,
        Fecha_Registro = CONVERT(VARCHAR(19), b.Fecha_Registro, 120)
    FROM dbo.R_UsuarioBitacora b
    WHERE b.Id_Usuario = @Id_Usuario
    ORDER BY b.Fecha_Registro DESC;
END
GO
```

- [ ] **Paso 3: Ejecutar**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 30 -i "docs/superpowers/plans/sql/2026-08-05-usuarios-lectura.sql"
```

Esperado: sin errores.

- [ ] **Paso 4: Verificar idempotencia**

Ejecuta el mismo comando **una segunda vez**. Esperado: sin errores y sin duplicar el índice.

- [ ] **Paso 5: Verificar el listado**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 30 -h -1 -W -Q "SET NOCOUNT ON; DECLARE @t TABLE (Id_Usuario NUMERIC(5), Cod_Usuario VARCHAR(50), Nom_Usuario VARCHAR(100), Log_Usuario VARCHAR(50), E_Mail VARCHAR(100), Cedula VARCHAR(32), Departamento VARCHAR(128), Empresa VARCHAR(50), Cod_Sap VARCHAR(50), Cod_Jefe_Inm VARCHAR(100), MailCodJefeInm VARCHAR(100), Id_Perfil BIGINT, NombrePerfil VARCHAR(200), Usuario_Estado VARCHAR(1)); INSERT INTO @t EXEC dbo.Sp_RTA_ListarUsuariosAdmin ''; SELECT 'sin filtro='+CAST(COUNT(*) AS VARCHAR) FROM @t; SELECT 'sin perfil='+CAST(COUNT(*) AS VARCHAR) FROM @t WHERE NombrePerfil='Sin perfil'; SELECT 'inactivos='+CAST(COUNT(*) AS VARCHAR) FROM @t WHERE Usuario_Estado<>'A';"
```

Esperado: `sin filtro` coincide con el total del Paso 1; aparecen usuarios `Sin perfil`; **aparecen también los inactivos** (el SP no filtra por estado, es a propósito).

- [ ] **Paso 6: Verificar el filtro**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 30 -h -1 -W -Q "SET NOCOUNT ON; EXEC dbo.Sp_RTA_ListarUsuariosAdmin 'campuzano';"
```

Esperado: solo las filas que coinciden, con `NombrePerfil` poblado.

- [ ] **Paso 7: Verificar la bitácora vacía**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 30 -h -1 -W -Q "SET NOCOUNT ON; DECLARE @id NUMERIC(5); SELECT TOP 1 @id=Id_Usuario FROM dbo.R_Usuarios; EXEC dbo.Sp_RTA_ListarBitacoraUsuario @id; SELECT 'filas en bitacora='+CAST(COUNT(*) AS VARCHAR) FROM dbo.R_UsuarioBitacora;"
```

Esperado: sin filas y `filas en bitacora=0`.

- [ ] **Paso 8: Commit**

```bash
git add docs/superpowers/plans/sql/2026-08-05-usuarios-lectura.sql
git commit -m "feat(db): bitacora de usuarios y SPs de lectura"
```

---

## Tarea 2: SPs de escritura

**Archivos:** Crear `docs/superpowers/plans/sql/2026-08-05-usuarios-escritura.sql`

**Interfaces consumidas:** `dbo.R_UsuarioBitacora` (Tarea 1).
**Interfaces producidas:**
- `Sp_RTA_ActualizarUsuario @Id_Usuario, @Nom_Usuario, @E_Mail, @Cedula, @Departamento, @Empresa, @Cod_Sap, @Cod_Jefe_Inm, @MailCodJefeInm, @UsuarioRegistro` → `Respuestas INT, Mensaje VARCHAR(300)`
- `Sp_RTA_RestablecerPassword @Id_Usuario, @HashMd5, @UsuarioRegistro` → `Respuestas INT, Mensaje VARCHAR(300)`

**Por qué los parámetros se declaran más largos que las columnas:** si un parámetro se declara al ancho exacto de la columna, SQL Server **trunca en silencio** cualquier valor más largo. Declarándolos con holgura y validando `LEN()` dentro del SP, un valor excedido se rechaza con mensaje en vez de guardarse mutilado.

- [ ] **Paso 1: Escribir el script**

```sql
/* ============================================================================
   Administracion de usuarios — SPs de escritura
   Base: ReporTarea
   Los parametros se declaran mas largos que las columnas a proposito: asi un
   valor excedido se rechaza con mensaje en vez de truncarse en silencio.
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------- 1) Actualizar datos del usuario ---------- */
IF OBJECT_ID('dbo.Sp_RTA_ActualizarUsuario','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_ActualizarUsuario;
GO
CREATE PROCEDURE dbo.Sp_RTA_ActualizarUsuario
    @Id_Usuario      NUMERIC(5),
    @Nom_Usuario     VARCHAR(300) = '',
    @E_Mail          VARCHAR(300) = '',
    @Cedula          VARCHAR(300) = '',
    @Departamento    VARCHAR(300) = '',
    @Empresa         VARCHAR(300) = '',
    @Cod_Sap         VARCHAR(300) = '',
    @Cod_Jefe_Inm    VARCHAR(300) = '',
    @MailCodJefeInm  VARCHAR(300) = '',
    @UsuarioRegistro VARCHAR(50)  = 'SISTEMA'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';

    /* normalizar entradas */
    SET @Nom_Usuario    = LTRIM(RTRIM(ISNULL(@Nom_Usuario,'')));
    SET @E_Mail         = LTRIM(RTRIM(ISNULL(@E_Mail,'')));
    SET @Cedula         = LTRIM(RTRIM(ISNULL(@Cedula,'')));
    SET @Departamento   = LTRIM(RTRIM(ISNULL(@Departamento,'')));
    SET @Empresa        = LTRIM(RTRIM(ISNULL(@Empresa,'')));
    SET @Cod_Sap        = LTRIM(RTRIM(ISNULL(@Cod_Sap,'')));
    SET @Cod_Jefe_Inm   = LTRIM(RTRIM(ISNULL(@Cod_Jefe_Inm,'')));
    SET @MailCodJefeInm = LTRIM(RTRIM(ISNULL(@MailCodJefeInm,'')));

    IF NOT EXISTS (SELECT 1 FROM dbo.R_Usuarios WHERE Id_Usuario = @Id_Usuario)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El usuario indicado no existe.'; RETURN;
    END

    IF @Nom_Usuario = ''
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El nombre del usuario es obligatorio.'; RETURN;
    END

    /* longitudes reales de las columnas */
    IF LEN(@Nom_Usuario) > 100 OR LEN(@E_Mail) > 100 OR LEN(@Cedula) > 32
       OR LEN(@Departamento) > 128 OR LEN(@Empresa) > 50 OR LEN(@Cod_Sap) > 50
       OR LEN(@Cod_Jefe_Inm) > 100 OR LEN(@MailCodJefeInm) > 100
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'Alguno de los datos excede el largo permitido.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @oNom VARCHAR(100), @oMail VARCHAR(100), @oCed VARCHAR(32),
                @oDep VARCHAR(128), @oEmp VARCHAR(50), @oSap VARCHAR(50),
                @oJefe VARCHAR(100), @oMailJefe VARCHAR(100);

        SELECT @oNom      = ISNULL(Nom_Usuario,''),
               @oMail     = ISNULL(E_Mail,''),
               @oCed      = ISNULL(Cedula,''),
               @oDep      = ISNULL(Departamento,''),
               @oEmp      = ISNULL(Empresa,''),
               @oSap      = ISNULL(Cod_Sap,''),
               @oJefe     = ISNULL(Cod_Jefe_Inm,''),
               @oMailJefe = ISNULL(MailCodJefeInm,'')
        FROM dbo.R_Usuarios WHERE Id_Usuario = @Id_Usuario;

        DECLARE @Detalle VARCHAR(2000) = '';
        IF @oNom      <> @Nom_Usuario    SET @Detalle = @Detalle + 'Nombre: [' + @oNom + '] -> [' + @Nom_Usuario + ']; ';
        IF @oMail     <> @E_Mail         SET @Detalle = @Detalle + 'Correo: [' + @oMail + '] -> [' + @E_Mail + ']; ';
        IF @oCed      <> @Cedula         SET @Detalle = @Detalle + 'Cedula: [' + @oCed + '] -> [' + @Cedula + ']; ';
        IF @oDep      <> @Departamento   SET @Detalle = @Detalle + 'Departamento: [' + @oDep + '] -> [' + @Departamento + ']; ';
        IF @oEmp      <> @Empresa        SET @Detalle = @Detalle + 'Empresa: [' + @oEmp + '] -> [' + @Empresa + ']; ';
        IF @oSap      <> @Cod_Sap        SET @Detalle = @Detalle + 'Cod SAP: [' + @oSap + '] -> [' + @Cod_Sap + ']; ';
        IF @oJefe     <> @Cod_Jefe_Inm   SET @Detalle = @Detalle + 'Jefe inmediato: [' + @oJefe + '] -> [' + @Cod_Jefe_Inm + ']; ';
        IF @oMailJefe <> @MailCodJefeInm SET @Detalle = @Detalle + 'Correo jefe: [' + @oMailJefe + '] -> [' + @MailCodJefeInm + ']; ';

        IF @Detalle = ''
        BEGIN
            COMMIT TRANSACTION;
            SELECT Respuestas = 1, Mensaje = 'No hubo cambios que guardar.'; RETURN;
        END

        UPDATE dbo.R_Usuarios
        SET Nom_Usuario    = @Nom_Usuario,
            E_Mail         = NULLIF(@E_Mail,''),
            Cedula         = NULLIF(@Cedula,''),
            Departamento   = NULLIF(@Departamento,''),
            Empresa        = NULLIF(@Empresa,''),
            Cod_Sap        = NULLIF(@Cod_Sap,''),
            Cod_Jefe_Inm   = NULLIF(@Cod_Jefe_Inm,''),
            MailCodJefeInm = NULLIF(@MailCodJefeInm,'')
        WHERE Id_Usuario = @Id_Usuario;

        INSERT INTO dbo.R_UsuarioBitacora (Id_Usuario, Accion, Detalle, Usuario_Registro)
        VALUES (@Id_Usuario, 'EDICION', LEFT(@Detalle,2000), @UsuarioRegistro);

        SET @Respuestas = 1;
        SET @Mensaje = 'Datos del usuario guardados correctamente.';
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo guardar los datos del usuario: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO

/* ---------- 2) Restablecer contrasena ---------- */
IF OBJECT_ID('dbo.Sp_RTA_RestablecerPassword','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_RestablecerPassword;
GO
CREATE PROCEDURE dbo.Sp_RTA_RestablecerPassword
    @Id_Usuario      NUMERIC(5),
    @HashMd5         VARCHAR(50) = '',
    @UsuarioRegistro VARCHAR(50) = 'SISTEMA'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Respuestas INT = 0, @Mensaje VARCHAR(300) = '';

    SET @HashMd5 = LTRIM(RTRIM(ISNULL(@HashMd5,'')));

    IF NOT EXISTS (SELECT 1 FROM dbo.R_Usuarios WHERE Id_Usuario = @Id_Usuario)
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'El usuario indicado no existe.'; RETURN;
    END

    /* Defensa en profundidad: el login compara el hash tal cual. Si aqui llegara
       una contrasena en claro, el usuario quedaria sin poder entrar. */
    IF LEN(@HashMd5) <> 32 OR @HashMd5 LIKE '%[^0-9a-fA-F]%'
    BEGIN
        SELECT Respuestas = 0, Mensaje = 'La clave recibida no tiene el formato esperado.'; RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.R_Usuarios SET Pass_Usuario = @HashMd5 WHERE Id_Usuario = @Id_Usuario;

        /* Nunca se registra la contrasena ni el hash. */
        INSERT INTO dbo.R_UsuarioBitacora (Id_Usuario, Accion, Detalle, Usuario_Registro)
        VALUES (@Id_Usuario, 'RESET_PASSWORD', 'Contrasena restablecida', @UsuarioRegistro);

        SET @Respuestas = 1;
        SET @Mensaje = 'Contrasena restablecida correctamente.';
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Respuestas = 0;
        SET @Mensaje = 'No se pudo restablecer la contrasena: ' + ERROR_MESSAGE();
    END CATCH

    SELECT Respuestas = @Respuestas, Mensaje = @Mensaje;
END
GO
```

- [ ] **Paso 2: Ejecutar**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 30 -i "docs/superpowers/plans/sql/2026-08-05-usuarios-escritura.sql"
```

- [ ] **Paso 3: Elegir un usuario de prueba y GUARDAR SUS VALORES ORIGINALES**

Esto es obligatorio: al final hay que dejarlo exactamente como estaba.

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 30 -h -1 -W -Q "SET NOCOUNT ON; SELECT TOP 1 'Id='+CAST(Id_Usuario AS VARCHAR)+' Nom=['+ISNULL(Nom_Usuario,'')+'] Mail=['+ISNULL(E_Mail,'')+'] Ced=['+ISNULL(Cedula,'')+'] Dep=['+ISNULL(Departamento,'')+'] Emp=['+ISNULL(Empresa,'')+'] Sap=['+ISNULL(Cod_Sap,'')+'] Jefe=['+ISNULL(Cod_Jefe_Inm,'')+'] MailJefe=['+ISNULL(MailCodJefeInm,'')+'] Pass_len='+CAST(LEN(Pass_Usuario) AS VARCHAR) FROM dbo.R_Usuarios WHERE Usuario_Estado='A' ORDER BY Id_Usuario DESC;"
```

**Pega esa línea completa en el reporte.** Es tu red de seguridad para el Paso 9.

- [ ] **Paso 4: Probar un cambio de un solo campo**

Usa el `Id_Usuario` del paso anterior. Cambia **solo** el departamento, dejando el resto con sus valores originales:

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 30 -h -1 -W -Q "SET NOCOUNT ON; EXEC dbo.Sp_RTA_ActualizarUsuario @Id_Usuario=<ID>, @Nom_Usuario='<NOM_ORIGINAL>', @E_Mail='<MAIL_ORIGINAL>', @Cedula='<CED_ORIGINAL>', @Departamento='PRUEBA_QA', @Empresa='<EMP_ORIGINAL>', @Cod_Sap='<SAP_ORIGINAL>', @Cod_Jefe_Inm='<JEFE_ORIGINAL>', @MailCodJefeInm='<MAILJEFE_ORIGINAL>', @UsuarioRegistro='PRUEBA'; SELECT Accion, Detalle FROM dbo.R_UsuarioBitacora WHERE Id_Usuario=<ID> ORDER BY Id_Bitacora DESC;"
```

Esperado: `Respuestas=1`, y la bitácora con **una sola** entrada que menciona **únicamente** `Departamento`. Si menciona otros campos, la comparación está mal.

- [ ] **Paso 5: Probar que sin cambios no escribe bitácora**

Repite **exactamente** el comando del Paso 4.

Esperado: `Respuestas=1` con mensaje `No hubo cambios que guardar.` y que la bitácora **siga teniendo una sola** entrada.

- [ ] **Paso 6: Probar los rechazos**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 30 -h -1 -W -Q "SET NOCOUNT ON; PRINT '--usuario inexistente--'; EXEC dbo.Sp_RTA_ActualizarUsuario @Id_Usuario=99999, @Nom_Usuario='X'; PRINT '--nombre vacio--'; EXEC dbo.Sp_RTA_ActualizarUsuario @Id_Usuario=<ID>, @Nom_Usuario=''; PRINT '--nombre demasiado largo--'; EXEC dbo.Sp_RTA_ActualizarUsuario @Id_Usuario=<ID>, @Nom_Usuario='<121 caracteres: usa REPLICATE>';"
```

Para el tercero: `DECLARE @L VARCHAR(300) = REPLICATE('A',121);` y pásalo como `@Nom_Usuario=@L`.

Esperado: los tres devuelven `Respuestas=0` con su mensaje, y **ninguno** escribe en la bitácora.

- [ ] **Paso 7: Probar el restablecimiento con hash válido**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 30 -h -1 -W -Q "SET NOCOUNT ON; EXEC dbo.Sp_RTA_RestablecerPassword @Id_Usuario=<ID>, @HashMd5='0123456789abcdef0123456789abcdef', @UsuarioRegistro='PRUEBA'; SELECT 'largo actual='+CAST(LEN(Pass_Usuario) AS VARCHAR) FROM dbo.R_Usuarios WHERE Id_Usuario=<ID>; SELECT TOP 1 Accion, Detalle FROM dbo.R_UsuarioBitacora WHERE Id_Usuario=<ID> ORDER BY Id_Bitacora DESC;"
```

Esperado: `Respuestas=1`, `largo actual=32`, y la bitácora con `RESET_PASSWORD` / `Contrasena restablecida` — **sin** el hash en el detalle.

- [ ] **Paso 8: Probar que rechaza una clave en claro**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 30 -h -1 -W -Q "SET NOCOUNT ON; EXEC dbo.Sp_RTA_RestablecerPassword @Id_Usuario=<ID>, @HashMd5='miclave123', @UsuarioRegistro='PRUEBA'; EXEC dbo.Sp_RTA_RestablecerPassword @Id_Usuario=<ID>, @HashMd5='ZZZZ456789abcdef0123456789abcdef', @UsuarioRegistro='PRUEBA';"
```

Esperado: **ambos** `Respuestas=0` con el mensaje de formato. Esta es la protección que evita dejar a un usuario sin poder entrar.

- [ ] **Paso 9: Restaurar el usuario de prueba y limpiar (OBLIGATORIO)**

Devuelve los ocho campos a sus valores originales del Paso 3, **restaura la contraseña original** y borra las filas de bitácora de prueba:

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 30 -h -1 -W -Q "SET NOCOUNT ON; UPDATE dbo.R_Usuarios SET Departamento=<ORIGINAL O NULL>, Pass_Usuario='<HASH ORIGINAL>' WHERE Id_Usuario=<ID>; DELETE FROM dbo.R_UsuarioBitacora WHERE Usuario_Registro='PRUEBA'; SELECT 'bitacora restante='+CAST(COUNT(*) AS VARCHAR) FROM dbo.R_UsuarioBitacora;"
```

**Importante:** en el Paso 3 solo capturaste el *largo* de la contraseña, no su valor. Antes del Paso 7, captura el valor con
`SELECT Pass_Usuario FROM dbo.R_Usuarios WHERE Id_Usuario=<ID>;` y guárdalo para restaurarlo aquí. No lo pegues en el reporte.

Esperado: `bitacora restante=0` y el usuario idéntico a como estaba.

- [ ] **Paso 10: Commit**

```bash
git add docs/superpowers/plans/sql/2026-08-05-usuarios-escritura.sql
git commit -m "feat(db): SPs de edicion de usuario y restablecimiento de clave"
```

---

## Tarea 3: Capas C#

**Archivos:**
- Crear: `CapaEntidad/EntUsuarioAdmin.cs`, `CapaEntidad/EntUsuarioBitacora.cs`, `CapaDato/DaoUsuarioAdmin.cs`, `CapaNegocio/NegUsuarioAdmin.cs`
- Modificar: los tres `.csproj`

**Interfaces consumidas:** los 4 SPs de las Tareas 1 y 2.
**Interfaces producidas:**
- `NegUsuarioAdmin.ListarUsuarios(string filtro)` → `List<EntUsuarioAdmin>`
- `NegUsuarioAdmin.ActualizarUsuario(EntUsuarioAdmin u, string usuarioRegistro)` → `EntRespuesta`
- `NegUsuarioAdmin.RestablecerPassword(decimal idUsuario, string hashMd5, string usuarioRegistro)` → `EntRespuesta`
- `NegUsuarioAdmin.ListarBitacora(decimal idUsuario)` → `List<EntUsuarioBitacora>`

`Id_Usuario` es `numeric(5)` en la base, por eso viaja como `decimal` en C#.

- [ ] **Paso 1: Crear las entidades**

`CapaEntidad/EntUsuarioAdmin.cs`:

```csharp
namespace CapaEntidad
{
    public class EntUsuarioAdmin
    {
        public decimal Id_Usuario { get; set; }
        public string Cod_Usuario { get; set; }
        public string Nom_Usuario { get; set; }
        public string Log_Usuario { get; set; }
        public string E_Mail { get; set; }
        public string Cedula { get; set; }
        public string Departamento { get; set; }
        public string Empresa { get; set; }
        public string Cod_Sap { get; set; }
        public string Cod_Jefe_Inm { get; set; }
        public string MailCodJefeInm { get; set; }
        public long Id_Perfil { get; set; }
        public string NombrePerfil { get; set; }
        public string Usuario_Estado { get; set; }
    }
}
```

`CapaEntidad/EntUsuarioBitacora.cs`:

```csharp
namespace CapaEntidad
{
    public class EntUsuarioBitacora
    {
        public string Accion { get; set; }
        public string Detalle { get; set; }
        public string Usuario_Registro { get; set; }
        public string Fecha_Registro { get; set; }
    }
}
```

- [ ] **Paso 2: Registrarlas en `CapaEntidad/CapaEntidad.csproj`**

Junto a las otras entradas `<Compile Include="Ent...`:

```xml
    <Compile Include="EntUsuarioAdmin.cs" />
    <Compile Include="EntUsuarioBitacora.cs" />
```

- [ ] **Paso 3: Crear `CapaDato/DaoUsuarioAdmin.cs`**

```csharp
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoUsuarioAdmin
    {
        /// <summary>Lista usuarios (activos e inactivos) con su perfil.</summary>
        public static List<EntUsuarioAdmin> ListarUsuarios(string filtro)
        {
            List<EntUsuarioAdmin> lista = new List<EntUsuarioAdmin>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarUsuariosAdmin", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Filtro", SqlDbType.VarChar, 100).Value = filtro ?? string.Empty;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntUsuarioAdmin()
                        {
                            Id_Usuario = Convert.ToDecimal(dr["Id_Usuario"]),
                            Cod_Usuario = dr["Cod_Usuario"].ToString(),
                            Nom_Usuario = dr["Nom_Usuario"].ToString(),
                            Log_Usuario = dr["Log_Usuario"].ToString(),
                            E_Mail = dr["E_Mail"].ToString(),
                            Cedula = dr["Cedula"].ToString(),
                            Departamento = dr["Departamento"].ToString(),
                            Empresa = dr["Empresa"].ToString(),
                            Cod_Sap = dr["Cod_Sap"].ToString(),
                            Cod_Jefe_Inm = dr["Cod_Jefe_Inm"].ToString(),
                            MailCodJefeInm = dr["MailCodJefeInm"].ToString(),
                            Id_Perfil = Convert.ToInt64(dr["Id_Perfil"]),
                            NombrePerfil = dr["NombrePerfil"].ToString(),
                            Usuario_Estado = dr["Usuario_Estado"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>Historial de cambios de un usuario.</summary>
        public static List<EntUsuarioBitacora> ListarBitacora(decimal idUsuario)
        {
            List<EntUsuarioBitacora> lista = new List<EntUsuarioBitacora>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarBitacoraUsuario", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id_Usuario", SqlDbType.Decimal).Value = idUsuario;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntUsuarioBitacora()
                        {
                            Accion = dr["Accion"].ToString(),
                            Detalle = dr["Detalle"].ToString(),
                            Usuario_Registro = dr["Usuario_Registro"].ToString(),
                            Fecha_Registro = dr["Fecha_Registro"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>Actualiza los ocho campos editables y deja rastro en la bitácora.</summary>
        public static EntRespuesta ActualizarUsuario(EntUsuarioAdmin u, string usuarioRegistro)
        {
            EntRespuesta respuesta = NuevaRespuesta();

            try
            {
                DaoReporTareaAranda conexion = new DaoReporTareaAranda();

                using (SqlConnection cnx = conexion.conectar())
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_ActualizarUsuario", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id_Usuario", SqlDbType.Decimal).Value = u.Id_Usuario;
                    cmd.Parameters.Add("@Nom_Usuario", SqlDbType.VarChar, 300).Value = u.Nom_Usuario ?? string.Empty;
                    cmd.Parameters.Add("@E_Mail", SqlDbType.VarChar, 300).Value = u.E_Mail ?? string.Empty;
                    cmd.Parameters.Add("@Cedula", SqlDbType.VarChar, 300).Value = u.Cedula ?? string.Empty;
                    cmd.Parameters.Add("@Departamento", SqlDbType.VarChar, 300).Value = u.Departamento ?? string.Empty;
                    cmd.Parameters.Add("@Empresa", SqlDbType.VarChar, 300).Value = u.Empresa ?? string.Empty;
                    cmd.Parameters.Add("@Cod_Sap", SqlDbType.VarChar, 300).Value = u.Cod_Sap ?? string.Empty;
                    cmd.Parameters.Add("@Cod_Jefe_Inm", SqlDbType.VarChar, 300).Value = u.Cod_Jefe_Inm ?? string.Empty;
                    cmd.Parameters.Add("@MailCodJefeInm", SqlDbType.VarChar, 300).Value = u.MailCodJefeInm ?? string.Empty;
                    cmd.Parameters.Add("@UsuarioRegistro", SqlDbType.VarChar, 50).Value = usuarioRegistro ?? "SISTEMA";

                    cnx.Open();
                    LeerRespuesta(cmd, respuesta);
                }
            }
            catch (Exception ex)
            {
                respuesta.estado = "0";
                respuesta.resultado = "0";
                respuesta.tipoMensaje = "danger";
                respuesta.mensaje = "Ocurrió un error al guardar los datos del usuario. Detalle: " + ex.Message;
            }

            return respuesta;
        }

        /// <summary>Guarda el hash MD5 de la nueva contraseña. Nunca recibe la clave en claro.</summary>
        public static EntRespuesta RestablecerPassword(decimal idUsuario, string hashMd5, string usuarioRegistro)
        {
            EntRespuesta respuesta = NuevaRespuesta();

            try
            {
                DaoReporTareaAranda conexion = new DaoReporTareaAranda();

                using (SqlConnection cnx = conexion.conectar())
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_RestablecerPassword", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id_Usuario", SqlDbType.Decimal).Value = idUsuario;
                    cmd.Parameters.Add("@HashMd5", SqlDbType.VarChar, 50).Value = hashMd5 ?? string.Empty;
                    cmd.Parameters.Add("@UsuarioRegistro", SqlDbType.VarChar, 50).Value = usuarioRegistro ?? "SISTEMA";

                    cnx.Open();
                    LeerRespuesta(cmd, respuesta);
                }
            }
            catch (Exception ex)
            {
                respuesta.estado = "0";
                respuesta.resultado = "0";
                respuesta.tipoMensaje = "danger";
                respuesta.mensaje = "Ocurrió un error al restablecer la contraseña. Detalle: " + ex.Message;
            }

            return respuesta;
        }

        private static EntRespuesta NuevaRespuesta()
        {
            return new EntRespuesta()
            {
                estado = "0",
                resultado = "0",
                tipoMensaje = "danger",
                mensaje = ""
            };
        }

        /// <summary>Traduce el contrato Respuestas/Mensaje de los SPs a EntRespuesta.</summary>
        private static void LeerRespuesta(SqlCommand cmd, EntRespuesta respuesta)
        {
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
}
```

- [ ] **Paso 4: Registrar el DAO en `CapaDato/CapaDato.csproj`**

```xml
    <Compile Include="DaoUsuarioAdmin.cs" />
```

- [ ] **Paso 5: Crear `CapaNegocio/NegUsuarioAdmin.cs`**

```csharp
using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegUsuarioAdmin
    {
        public static List<EntUsuarioAdmin> ListarUsuarios(string filtro)
        {
            return DaoUsuarioAdmin.ListarUsuarios(filtro);
        }

        public static List<EntUsuarioBitacora> ListarBitacora(decimal idUsuario)
        {
            return DaoUsuarioAdmin.ListarBitacora(idUsuario);
        }

        public static EntRespuesta ActualizarUsuario(EntUsuarioAdmin u, string usuarioRegistro)
        {
            return DaoUsuarioAdmin.ActualizarUsuario(u, usuarioRegistro);
        }

        public static EntRespuesta RestablecerPassword(decimal idUsuario, string hashMd5, string usuarioRegistro)
        {
            return DaoUsuarioAdmin.RestablecerPassword(idUsuario, hashMd5, usuarioRegistro);
        }
    }
}
```

- [ ] **Paso 6: Registrarlo en `CapaNegocio/CapaNegocio.csproj`**

```xml
    <Compile Include="NegUsuarioAdmin.cs" />
```

- [ ] **Paso 7: Compilar**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```

Esperado: exit code 0 y ninguna línea `error CS` / `error MSB`. Si la ruta de MSBuild no existe, búscala y reporta la que usaste.

- [ ] **Paso 8: Commit (solo el código, sin binarios)**

```bash
git add CapaEntidad/EntUsuarioAdmin.cs CapaEntidad/EntUsuarioBitacora.cs CapaEntidad/CapaEntidad.csproj CapaDato/DaoUsuarioAdmin.cs CapaDato/CapaDato.csproj CapaNegocio/NegUsuarioAdmin.cs CapaNegocio/CapaNegocio.csproj
git commit -m "feat(backend): capas de administracion de usuarios"
```

---

## Tarea 4: Handler `AdministrarUsuarios.ashx`

**Archivos:**
- Crear: `ReporteTareas/Formulario/AdministrarUsuarios.ashx`, `AdministrarUsuarios.ashx.cs`
- Modificar: `ReporteTareas/ReporteTareas.csproj`

**Interfaces consumidas:** `NegUsuarioAdmin` (Tarea 3) y `SeguridadAppHelper.SeguridadHelper.GetMd5Hash(string)` de `ReporteTareas/clases/SeguridadHelper.cs`.
**Interfaces producidas:** acciones `BuscarUsuarios {filtro}`, `GuardarUsuario {idUsuario, nombre, correo, cedula, departamento, empresa, codSap, jefe, correoJefe}`, `RestablecerPassword {idUsuario, clave}`, `VerBitacora {idUsuario}`.

**Punto crítico:** el hash se calcula **aquí, en el servidor**, con la misma clase que usa `Login.aspx.cs:101`. El cliente envía la contraseña en claro sobre la sesión ya autenticada; **nunca** envía un hash.

- [ ] **Paso 1: Crear el markup `AdministrarUsuarios.ashx`**

```
<%@ WebHandler Language="C#" CodeBehind="AdministrarUsuarios.ashx.cs" Class="JsonJQueryNetUsuarios.AdministrarUsuarios" %>
```

- [ ] **Paso 2: Crear `AdministrarUsuarios.ashx.cs`**

```csharp
using CapaEntidad;
using CapaNegocio;
using SeguridadAppHelper;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetUsuarios
{
    /// <summary>
    /// Handler de la pantalla "Administración de usuarios".
    /// Acciones: BuscarUsuarios, GuardarUsuario, RestablecerPassword, VerBitacora.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarUsuarios : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            StringBuilder responseAction = new StringBuilder();

            // Sin sesión no se ejecuta nada: es una pantalla de administración.
            if (context.Session == null || context.Session["UserLogin"] == null)
            {
                responseAction.Append(responseMessage("0", "Su sesión expiró. Vuelva a iniciar sesión.", "danger"));
            }
            else if (context.Request.ContentType != null && context.Request.ContentType.Contains("json"))
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

                if (Action == "GuardarUsuario")
                {
                    existAction = true;
                    responseAction.Append(GuardarUsuario(context, parameters));
                }

                if (Action == "RestablecerPassword")
                {
                    existAction = true;
                    responseAction.Append(RestablecerPassword(context, parameters));
                }

                if (Action == "VerBitacora")
                {
                    existAction = true;
                    responseAction.Append(VerBitacora(parameters));
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
                string filtro = Texto(campos, "filtro");
                return ToJson(NegUsuarioAdmin.ListarUsuarios(filtro));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al buscar usuarios. " + ex.Message, "danger");
            }
        }

        private string VerBitacora(dynamic campos)
        {
            try
            {
                decimal idUsuario = Numero(campos, "idUsuario");
                if (idUsuario <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un usuario.", "warning");
                }

                return ToJson(NegUsuarioAdmin.ListarBitacora(idUsuario));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar el historial. " + ex.Message, "danger");
            }
        }

        private string GuardarUsuario(HttpContext context, dynamic campos)
        {
            try
            {
                decimal idUsuario = Numero(campos, "idUsuario");
                if (idUsuario <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un usuario.", "warning");
                }

                EntUsuarioAdmin u = new EntUsuarioAdmin()
                {
                    Id_Usuario = idUsuario,
                    Nom_Usuario = Texto(campos, "nombre"),
                    E_Mail = Texto(campos, "correo"),
                    Cedula = Texto(campos, "cedula"),
                    Departamento = Texto(campos, "departamento"),
                    Empresa = Texto(campos, "empresa"),
                    Cod_Sap = Texto(campos, "codSap"),
                    Cod_Jefe_Inm = Texto(campos, "jefe"),
                    MailCodJefeInm = Texto(campos, "correoJefe")
                };

                string error = Validar(u);
                if (error != null)
                {
                    return responseMessage("0", error, "warning");
                }

                EntRespuesta respuesta = NegUsuarioAdmin.ActualizarUsuario(u, UsuarioSesion(context));
                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al guardar el usuario. " + ex.Message, "danger");
            }
        }

        private string RestablecerPassword(HttpContext context, dynamic campos)
        {
            try
            {
                decimal idUsuario = Numero(campos, "idUsuario");
                if (idUsuario <= 0)
                {
                    return responseMessage("0", "Debe seleccionar un usuario.", "warning");
                }

                string clave = Texto(campos, "clave");
                if (clave.Length < 6)
                {
                    return responseMessage("0", "La contraseña debe tener al menos 6 caracteres.", "warning");
                }

                // El hash se calcula aquí, con la misma clase que usa el login.
                SeguridadHelper seguridad = new SeguridadHelper();
                string hash = seguridad.GetMd5Hash(clave);

                EntRespuesta respuesta = NegUsuarioAdmin.RestablecerPassword(idUsuario, hash, UsuarioSesion(context));
                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al restablecer la contraseña. " + ex.Message, "danger");
            }
        }

        /// <summary>Validaciones de servidor. Devuelve null si todo está bien.</summary>
        private string Validar(EntUsuarioAdmin u)
        {
            if (u.Nom_Usuario.Length == 0) { return "El nombre del usuario es obligatorio."; }
            if (u.Nom_Usuario.Length > 100) { return "El nombre no puede superar los 100 caracteres."; }
            if (u.E_Mail.Length > 100) { return "El correo no puede superar los 100 caracteres."; }
            if (u.Cedula.Length > 32) { return "La cédula no puede superar los 32 caracteres."; }
            if (u.Departamento.Length > 128) { return "El departamento no puede superar los 128 caracteres."; }
            if (u.Empresa.Length > 50) { return "La empresa no puede superar los 50 caracteres."; }
            if (u.Cod_Sap.Length > 50) { return "El código SAP no puede superar los 50 caracteres."; }
            if (u.Cod_Jefe_Inm.Length > 100) { return "El jefe inmediato no puede superar los 100 caracteres."; }
            if (u.MailCodJefeInm.Length > 100) { return "El correo del jefe no puede superar los 100 caracteres."; }
            if (!CorreoValido(u.E_Mail)) { return "El correo no tiene un formato válido."; }
            if (!CorreoValido(u.MailCodJefeInm)) { return "El correo del jefe no tiene un formato válido."; }
            return null;
        }

        /// <summary>Vacío es válido: el correo es opcional.</summary>
        private bool CorreoValido(string correo)
        {
            if (string.IsNullOrEmpty(correo)) { return true; }
            return Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        /// <summary>Quién hace el cambio sale de la sesión, nunca del cliente.</summary>
        private string UsuarioSesion(HttpContext context)
        {
            if (context.Session != null && context.Session["Cod_Usuario"] != null)
            {
                return context.Session["Cod_Usuario"].ToString();
            }
            return "SISTEMA";
        }

        private string Texto(dynamic campos, string clave)
        {
            try
            {
                object valor = campos[clave];
                if (valor == null) { return string.Empty; }
                return Convert.ToString(valor).Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private decimal Numero(dynamic campos, string clave)
        {
            try
            {
                decimal valor;
                if (!decimal.TryParse(Convert.ToString(campos[clave]), out valor)) { return 0; }
                return valor;
            }
            catch
            {
                return 0;
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

- [ ] **Paso 3: Registrar en `ReporteTareas/ReporteTareas.csproj`**

Junto a las entradas de `AdministrarMenuUsuario`:

```xml
    <Content Include="Formulario\AdministrarUsuarios.ashx" />
```

```xml
    <Compile Include="Formulario\AdministrarUsuarios.ashx.cs">
      <DependentUpon>AdministrarUsuarios.ashx</DependentUpon>
      <SubType>Code</SubType>
    </Compile>
```

- [ ] **Paso 4: Compilar**

Mismo comando del Paso 7 de la Tarea 3. Esperado: exit code 0, sin `error CS`.

- [ ] **Paso 5: Verificaciones estáticas (pega la salida)**

Con `grep`, confirma:
- (a) la clase declara `IRequiresSessionState`;
- (b) el chequeo de sesión es lo **primero** de `ProcessRequest`, antes de deserializar el JSON;
- (c) `GetMd5Hash` se llama en el handler y **no** hay ninguna acción que reciba un hash del cliente;
- (d) `Usuario_Registro` sale de `context.Session`;
- (e) el `Class=` del `.ashx` coincide **exactamente** con `namespace.clase` del `.ashx.cs` (si no, es error 500 en runtime que la compilación no detecta).

- [ ] **Paso 6: Verificar que el hash coincide con el del login**

El hash debe ser idéntico al que produce `Login.aspx.cs:101`. Compruébalo con un valor conocido: el MD5 de `abc123` es `e99a18c428cb38d5f260853678922e03`.

```powershell
$md5 = [System.Security.Cryptography.MD5]::Create()
$bytes = $md5.ComputeHash([System.Text.Encoding]::UTF8.GetBytes("abc123"))
-join ($bytes | ForEach-Object { $_.ToString("x2") })
```

Esperado: `e99a18c428cb38d5f260853678922e03`. Confirma además leyendo `ReporteTareas/clases/SeguridadHelper.cs` que `GetMd5Hash` hace exactamente eso (`Encoding.UTF8`, formato `x2`).

- [ ] **Paso 7: Commit**

```bash
git add "ReporteTareas/Formulario/AdministrarUsuarios.ashx" "ReporteTareas/Formulario/AdministrarUsuarios.ashx.cs" ReporteTareas/ReporteTareas.csproj
git commit -m "feat(handler): AdministrarUsuarios.ashx con sesion obligatoria y hash en servidor"
```

---

## Tarea 5: Pantalla `ParametrizacionUsuarios.aspx`

**Archivos:**
- Crear: `ReporteTareas/Formulario/ParametrizacionUsuarios.aspx`, `.aspx.cs`, `.aspx.designer.cs`
- Modificar: `ReporteTareas/ReporteTareas.csproj`

**Interfaces producidas — los `id` del DOM son contrato**, el JS de la Tarea 6 está escrito contra ellos:
`txtBuscar`, `btnBuscar`, `datosTablaUsuarios`, `panelDetalle`, `txtIdUsuarioSel`, `txtCodUsuarioSel`, `txtLoginSel`, `txtPerfilSel`, `txtEstadoSel`, `txtNombre`, `txtCorreo`, `txtCedula`, `txtDepartamento`, `txtEmpresa`, `txtCodSap`, `txtJefe`, `txtCorreoJefe`, `btnGuardar`, `btnRestablecer`, `btnHistorial`, `modalPassword`, `txtClaveNueva`, `txtClaveConfirma`, `btnConfirmarClave`, `modalHistorial`, `datosHistorial`, `modalMensajeInformativo`, `modalMensajeInformativoTipo`, `MensajeInformativo`.
El `.aspx` llama a las funciones globales `BuscarUsuarios()`, `GuardarUsuario()`, `AbrirModalPassword()`, `ConfirmarPassword()` y `VerHistorial()`.

- [ ] **Paso 1: Crear `ParametrizacionUsuarios.aspx` (UTF-8 CON BOM)**

```aspx
<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ParametrizacionUsuarios.aspx.cs" Inherits="ReporteTareas.Formulario.ParametrizacionUsuarios" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/parametrizacionUsuarios.js?v=1" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Administración de usuarios</h3>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-12" style="padding: 0px">
            <div class="panel panel-default">
                <div class="panel-heading">
                    Datos de los usuarios registrados. El código, el login, el perfil y el estado no se editan desde aquí.
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
                            <div class="panel-heading"><h4>Datos del usuario</h4></div>
                            <div class="panel-body">
                                <input type="hidden" id="txtIdUsuarioSel" />
                                <div class="row">
                                    <div class="form-group col-lg-3">
                                        <label>Código:</label>
                                        <input type="text" class="form-control" id="txtCodUsuarioSel" disabled />
                                    </div>
                                    <div class="form-group col-lg-3">
                                        <label>Login:</label>
                                        <input type="text" class="form-control" id="txtLoginSel" disabled />
                                    </div>
                                    <div class="form-group col-lg-3">
                                        <label>Perfil:</label>
                                        <input type="text" class="form-control" id="txtPerfilSel" disabled />
                                    </div>
                                    <div class="form-group col-lg-3">
                                        <label>Estado:</label>
                                        <input type="text" class="form-control" id="txtEstadoSel" disabled />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-6">
                                        <label>Nombre: <span style="color:#a94442">*</span></label>
                                        <input type="text" class="form-control" id="txtNombre" maxlength="100" />
                                    </div>
                                    <div class="form-group col-lg-6">
                                        <label>Correo:</label>
                                        <input type="text" class="form-control" id="txtCorreo" maxlength="100" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-4">
                                        <label>Cédula:</label>
                                        <input type="text" class="form-control" id="txtCedula" maxlength="32" />
                                    </div>
                                    <div class="form-group col-lg-4">
                                        <label>Departamento:</label>
                                        <input type="text" class="form-control" id="txtDepartamento" maxlength="128" />
                                    </div>
                                    <div class="form-group col-lg-4">
                                        <label>Empresa:</label>
                                        <input type="text" class="form-control" id="txtEmpresa" maxlength="50" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="form-group col-lg-4">
                                        <label>Código SAP:</label>
                                        <input type="text" class="form-control" id="txtCodSap" maxlength="50" />
                                    </div>
                                    <div class="form-group col-lg-4">
                                        <label>Jefe inmediato:</label>
                                        <input type="text" class="form-control" id="txtJefe" maxlength="100" />
                                    </div>
                                    <div class="form-group col-lg-4">
                                        <label>Correo del jefe:</label>
                                        <input type="text" class="form-control" id="txtCorreoJefe" maxlength="100" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-lg-12">
                                        <button id="btnGuardar" onclick="GuardarUsuario()" type="button" class="btn btn-success">Guardar</button>
                                        <button id="btnRestablecer" onclick="AbrirModalPassword()" type="button" class="btn btn-warning">Restablecer contraseña</button>
                                        <button id="btnHistorial" onclick="VerHistorial()" type="button" class="btn btn-default">Ver historial</button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal de contraseña -->
        <div class="modal fade" id="modalPassword" tabindex="-1" role="dialog" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title">Restablecer contraseña</h4>
                    </div>
                    <div class="modal-body">
                        <p>La contraseña actual no se puede consultar. Al guardar, se reemplaza por la nueva.</p>
                        <div class="form-group">
                            <label>Nueva contraseña (mínimo 6 caracteres):</label>
                            <input type="password" class="form-control" id="txtClaveNueva" maxlength="50" />
                        </div>
                        <div class="form-group">
                            <label>Confirmar contraseña:</label>
                            <input type="password" class="form-control" id="txtClaveConfirma" maxlength="50" />
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button id="btnConfirmarClave" onclick="ConfirmarPassword()" type="button" class="btn btn-warning">Restablecer</button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal de historial -->
        <div class="modal fade" id="modalHistorial" tabindex="-1" role="dialog" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title">Historial de cambios</h4>
                    </div>
                    <div class="modal-body" style="max-height: 400px; overflow-y: auto">
                        <div id="datosHistorial"></div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Informativo -->
        <div class="modal fade" id="modalMensajeInformativo" tabindex="-1" role="dialog" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3" id="modalMensajeInformativoTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title">Informativo</h4>
                    </div>
                    <div class="modal-body" id="MensajeInformativo"></div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
```

- [ ] **Paso 2: Crear `ParametrizacionUsuarios.aspx.cs`**

```csharp
using ReporteTareas.Controles;
using SeguridadAppHelper;
using System;

namespace ReporteTareas.Formulario
{
    public partial class ParametrizacionUsuarios : System.Web.UI.Page
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

- [ ] **Paso 3: Crear `ParametrizacionUsuarios.aspx.designer.cs`**

```csharp
//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ReporteTareas.Formulario
{
    public partial class ParametrizacionUsuarios
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

```xml
    <Content Include="Formulario\ParametrizacionUsuarios.aspx" />
```

```xml
    <Compile Include="Formulario\ParametrizacionUsuarios.aspx.cs">
      <DependentUpon>ParametrizacionUsuarios.aspx</DependentUpon>
      <SubType>ASPXCodeBehind</SubType>
    </Compile>
    <Compile Include="Formulario\ParametrizacionUsuarios.aspx.designer.cs">
      <DependentUpon>ParametrizacionUsuarios.aspx</DependentUpon>
    </Compile>
```

- [ ] **Paso 5: Compilar y verificar BOM**

Compila (mismo comando). Después:

```powershell
$b=[System.IO.File]::ReadAllBytes("ReporteTareas\Formulario\ParametrizacionUsuarios.aspx"); "{0:X2} {1:X2} {2:X2}" -f $b[0],$b[1],$b[2]
```

Esperado: `EF BB BF`. **Si no, reescribe el archivo con `UTF8Encoding($true)` y vuelve a verificar después de commitear.**

- [ ] **Paso 6: Verificar los `id` del contrato**

Con `grep`, confirma que los 29 `id` listados en "Interfaces producidas" están presentes y escritos igual. Pega el resultado.

- [ ] **Paso 7: Commit**

```bash
git add "ReporteTareas/Formulario/ParametrizacionUsuarios.aspx" "ReporteTareas/Formulario/ParametrizacionUsuarios.aspx.cs" "ReporteTareas/Formulario/ParametrizacionUsuarios.aspx.designer.cs" ReporteTareas/ReporteTareas.csproj
git commit -m "feat(ui): pantalla de administracion de usuarios"
```

---

## Tarea 6: JavaScript

**Archivos:**
- Crear: `ReporteTareas/js/parametrizacionUsuarios.js`
- Modificar: `ReporteTareas/ReporteTareas.csproj`

**Interfaces consumidas:** los `id` del DOM de la Tarea 5 y las acciones del handler de la Tarea 4.

- [ ] **Paso 1: Crear el archivo (UTF-8 CON BOM)**

```javascript
/* ============================================================================
   Pantalla: Administracion de usuarios
   Handler : AdministrarUsuarios.ashx
   La contrasena actual nunca se consulta ni se muestra: solo se restablece,
   y el hash lo calcula el servidor.
   ============================================================================ */

var _usuarios = [];

$(document).ready(function () {
    BuscarUsuarios();
});

function PostUsuario(action, parameters, onSuccess) {
    var datos = JSON.stringify([{ "action": action, "parameters": parameters }]);
    $.ajax({
        type: "POST",
        url: "AdministrarUsuarios.ashx",
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

    PostUsuario("BuscarUsuarios", { "filtro": filtro }, function (respuesta) {
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
    info += "<th style='text-align:center'>Editar</th>";
    info += "<th>Código</th>";
    info += "<th>Nombre</th>";
    info += "<th>Login</th>";
    info += "<th>Perfil</th>";
    info += "<th>Correo</th>";
    info += "<th style='text-align:center'>Estado</th>";
    info += "</tr></thead><tbody>";

    if (lista.length === 0) {
        info += "<tr><td colspan='7' style='text-align:center'>No existen usuarios para esta búsqueda.</td></tr>";
    }

    $.each(lista, function (i, item) {
        var estado = (item.Usuario_Estado === "A")
            ? "<span class='label label-success'>Activo</span>"
            : "<span class='label label-default'>Inactivo</span>";

        info += "<tr role='row'>";
        info += "<td style='text-align:center'>";
        info += "<i class='fa fa-pencil' title='Editar' style='cursor:pointer' onclick='SeleccionarUsuario(" + i + ")'></i>";
        info += "</td>";
        info += "<td>" + Escapar(item.Cod_Usuario) + "</td>";
        info += "<td>" + Escapar(item.Nom_Usuario) + "</td>";
        info += "<td>" + Escapar(item.Log_Usuario) + "</td>";
        info += "<td>" + Escapar(item.NombrePerfil) + "</td>";
        info += "<td>" + Escapar(item.E_Mail) + "</td>";
        info += "<td style='text-align:center'>" + estado + "</td>";
        info += "</tr>";
    });

    info += "</tbody></table>";
    $("#datosTablaUsuarios").html(info);
}

function SeleccionarUsuario(indice) {
    var u = _usuarios[indice];
    if (u == null) { return; }

    $("#txtIdUsuarioSel").val(u.Id_Usuario);
    $("#txtCodUsuarioSel").val(u.Cod_Usuario);
    $("#txtLoginSel").val(u.Log_Usuario);
    $("#txtPerfilSel").val(u.NombrePerfil + " (" + u.Id_Perfil + ")");
    $("#txtEstadoSel").val(u.Usuario_Estado === "A" ? "Activo" : "Inactivo");

    $("#txtNombre").val(u.Nom_Usuario);
    $("#txtCorreo").val(u.E_Mail);
    $("#txtCedula").val(u.Cedula);
    $("#txtDepartamento").val(u.Departamento);
    $("#txtEmpresa").val(u.Empresa);
    $("#txtCodSap").val(u.Cod_Sap);
    $("#txtJefe").val(u.Cod_Jefe_Inm);
    $("#txtCorreoJefe").val(u.MailCodJefeInm);

    $("#panelDetalle").show();
}

/* Vacio es valido: el correo es opcional. */
function CorreoValido(correo) {
    if (correo == null || correo === "") { return true; }
    return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(correo);
}

function GuardarUsuario() {
    var idUsuario = $("#txtIdUsuarioSel").val();
    if (idUsuario == null || idUsuario === "") {
        MostrarMensaje("Debe seleccionar un usuario.", "warning");
        return;
    }

    var nombre = $.trim($("#txtNombre").val());
    if (nombre === "") {
        MostrarMensaje("El nombre del usuario es obligatorio.", "warning");
        return;
    }

    var correo = $.trim($("#txtCorreo").val());
    if (!CorreoValido(correo)) {
        MostrarMensaje("El correo no tiene un formato válido.", "warning");
        return;
    }

    var correoJefe = $.trim($("#txtCorreoJefe").val());
    if (!CorreoValido(correoJefe)) {
        MostrarMensaje("El correo del jefe no tiene un formato válido.", "warning");
        return;
    }

    var datos = {
        "idUsuario": idUsuario,
        "nombre": nombre,
        "correo": correo,
        "cedula": $.trim($("#txtCedula").val()),
        "departamento": $.trim($("#txtDepartamento").val()),
        "empresa": $.trim($("#txtEmpresa").val()),
        "codSap": $.trim($("#txtCodSap").val()),
        "jefe": $.trim($("#txtJefe").val()),
        "correoJefe": correoJefe
    };

    $("#btnGuardar").prop("disabled", true);
    PostUsuario("GuardarUsuario", datos, function (respuesta) {
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

function AbrirModalPassword() {
    var idUsuario = $("#txtIdUsuarioSel").val();
    if (idUsuario == null || idUsuario === "") {
        MostrarMensaje("Debe seleccionar un usuario.", "warning");
        return;
    }
    $("#txtClaveNueva").val("");
    $("#txtClaveConfirma").val("");
    $("#modalPassword").modal("show");
}

function ConfirmarPassword() {
    var idUsuario = $("#txtIdUsuarioSel").val();
    var clave = $("#txtClaveNueva").val();
    var confirma = $("#txtClaveConfirma").val();

    if (clave == null || clave.length < 6) {
        MostrarMensaje("La contraseña debe tener al menos 6 caracteres.", "warning");
        return;
    }
    if (clave !== confirma) {
        MostrarMensaje("Las dos contraseñas no coinciden.", "warning");
        return;
    }

    $("#btnConfirmarClave").prop("disabled", true);
    PostUsuario("RestablecerPassword", { "idUsuario": idUsuario, "clave": clave }, function (respuesta) {
        $("#btnConfirmarClave").prop("disabled", false);
        /* No dejar la clave en el DOM despues de enviarla. */
        $("#txtClaveNueva").val("");
        $("#txtClaveConfirma").val("");
        $("#modalPassword").modal("hide");

        if (respuesta == null) {
            MostrarMensaje("No se recibió respuesta del servidor.", "danger");
            return;
        }
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
    });
}

function VerHistorial() {
    var idUsuario = $("#txtIdUsuarioSel").val();
    if (idUsuario == null || idUsuario === "") {
        MostrarMensaje("Debe seleccionar un usuario.", "warning");
        return;
    }

    PostUsuario("VerBitacora", { "idUsuario": idUsuario }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        var lista = respuesta || [];
        var info = "<table width='100%' class='table table-striped table-bordered'>";
        info += "<thead><tr><th>Fecha</th><th>Acción</th><th>Detalle</th><th>Responsable</th></tr></thead><tbody>";

        if (lista.length === 0) {
            info += "<tr><td colspan='4' style='text-align:center'>Este usuario no tiene cambios registrados.</td></tr>";
        }

        $.each(lista, function (i, item) {
            info += "<tr>";
            info += "<td>" + Escapar(item.Fecha_Registro) + "</td>";
            info += "<td>" + Escapar(item.Accion) + "</td>";
            info += "<td>" + Escapar(item.Detalle) + "</td>";
            info += "<td>" + Escapar(item.Usuario_Registro) + "</td>";
            info += "</tr>";
        });

        info += "</tbody></table>";
        $("#datosHistorial").html(info);
        $("#modalHistorial").modal("show");
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

- [ ] **Paso 2: Verificar el BOM**

```powershell
$b=[System.IO.File]::ReadAllBytes("ReporteTareas\js\parametrizacionUsuarios.js"); "{0:X2} {1:X2} {2:X2}" -f $b[0],$b[1],$b[2]
```

Esperado: `EF BB BF`. Verifícalo también **después** del commit.

- [ ] **Paso 3: Registrar en el `.csproj`**

```xml
    <Content Include="js\parametrizacionUsuarios.js" />
```

- [ ] **Paso 4: Validar la sintaxis y compilar**

```bash
node --check "ReporteTareas/js/parametrizacionUsuarios.js"
```

Esperado: sin salida (válido). Si `node` no está disponible, dilo en el reporte y sigue. Luego compila la solución.

- [ ] **Paso 5: Verificaciones estáticas (pega la salida)**

Confirma con `grep` que:
- (a) las cuatro acciones (`BuscarUsuarios`, `GuardarUsuario`, `RestablecerPassword`, `VerBitacora`) aparecen escritas correctamente;
- (b) existen las funciones globales que llama el `.aspx`: `BuscarUsuarios`, `GuardarUsuario`, `AbrirModalPassword`, `ConfirmarPassword`, `VerHistorial`;
- (c) **no** hay ninguna llamada a MD5 ni a hash en el JS (la clave viaja en claro sobre la sesión y el servidor la hashea);
- (d) los campos de clave se limpian tras enviarlos;
- (e) todos los valores que vienen del servidor pasan por `Escapar` antes de entrar al HTML.

- [ ] **Paso 6: Commit**

```bash
git add ReporteTareas/js/parametrizacionUsuarios.js ReporteTareas/ReporteTareas.csproj
git commit -m "feat(ui): buscador, formulario e historial de usuarios"
```

---

## Tarea 7: Registro en el menú

**Archivos:** Crear `docs/superpowers/plans/sql/2026-08-05-usuarios-menu.sql`

- [ ] **Paso 1: Confirmar el menú padre antes de escribir nada**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 30 -h -1 -W -Q "SET NOCOUNT ON; SELECT 'padre 20042: '+ISNULL((SELECT Titulo FROM dbo.MenuDos WHERE Id_Menu=20042),'NO EXISTE'); SELECT 'ya registrada? '+CAST(COUNT(*) AS VARCHAR) FROM dbo.MenuDos WHERE Href='ParametrizacionUsuarios.aspx';"
```

Esperado: el padre existe ("Manejo de Perfiles") y la pantalla aún no está registrada. **Si el padre no existe, detente y reporta BLOCKED**: el ítem quedaría colgado de un padre inexistente y no se vería en el menú.

- [ ] **Paso 2: Escribir el script**

```sql
/* ============================================================================
   Registro en menu de "Administracion de Usuarios"
   Base: ReporTarea — Padre: 20042 (Manejo de Perfiles) — Perfiles: 2,18,19
   Se excluye el perfil 1 a proposito, igual que los registros anteriores.
   Idempotente.
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

DECLARE @Titulo VARCHAR(200) = 'Administracion de Usuarios';
DECLARE @Href   VARCHAR(200) = 'ParametrizacionUsuarios.aspx';
DECLARE @Icono  VARCHAR(200) = 'fa fa-users';
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

- [ ] **Paso 3: Ejecutar y verificar idempotencia**

Ejecuta el script **dos veces**. La segunda debe devolver el **mismo** `Id_Menu`, y:

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 30 -h -1 -W -Q "SET NOCOUNT ON; SELECT 'en MenuDos='+CAST(COUNT(*) AS VARCHAR) FROM dbo.MenuDos WHERE Href='ParametrizacionUsuarios.aspx';"
```

Esperado: `en MenuDos=1`.

- [ ] **Paso 4: Verificar que no se activó para otros perfiles**

```bash
sqlcmd -S tcp:192.168.11.14,1433 -U sa -P CAfKsUBnD0s -d ReporTarea -l 30 -h -1 -W -Q "SET NOCOUNT ON; DECLARE @m INT; SELECT @m=Id_Menu FROM dbo.MenuDos WHERE Href='ParametrizacionUsuarios.aspx'; SELECT 'perfil '+CAST(IdPerfil AS VARCHAR)+' estado '+CAST(Estado AS VARCHAR) FROM dbo.PerfilMenu WHERE id_Menu=@m ORDER BY IdPerfil;"
```

Esperado: exactamente los perfiles 2, 18 y 19 con `Estado=0`, ninguno más.

- [ ] **Paso 5: Commit**

```bash
git add docs/superpowers/plans/sql/2026-08-05-usuarios-menu.sql
git commit -m "feat(menu): registro de ParametrizacionUsuarios (perfiles 2,18,19)"
```

---

## Tarea 8: Binarios

Este repositorio versiona los binarios; sin este paso, quien despliegue desde el repositorio publica las pantallas nuevas con las DLL viejas y la aplicación falla al abrirlas.

**Archivos:** `*/bin/*`, `*/obj/*`

- [ ] **Paso 1: Recompilar en limpio**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln /p:Configuration=Debug /v:minimal
```

Esperado: exit code 0, sin `error CS` / `error MSB`, y los cuatro proyectos enlazados.

- [ ] **Paso 2: Preparar SOLO los binarios**

```bash
git add -- "*/bin/*" "*/obj/*"
git diff --cached --name-only | grep -E "\.(cs|sql|md|aspx|js|ashx|suo|user)$" | grep -v "/obj/"
```

El segundo comando **debe salir vacío**: si aparece algún archivo fuente, quítalo del índice antes de commitear. **Nunca uses `git add -A`**: hay trabajo en curso ajeno sin commitear que no debe entrar.

- [ ] **Paso 3: Verificar que el trabajo ajeno sigue fuera**

```bash
git status --short | grep -E "DaoMarcacion|DaoTareas|NegMarcacion|EnvioCorreoHelper|RegistroLaboral"
```

Esperado: siguen apareciendo como **modificados y sin commitear**. Si desaparecieron, se colaron en el commit: deshazlo.

- [ ] **Paso 4: Commit**

```bash
git commit -m "build: binarios de administracion de usuarios"
```

- [ ] **Paso 5: Verificar el BOM después de todos los commits**

```powershell
foreach ($f in @("ReporteTareas\js\parametrizacionUsuarios.js","ReporteTareas\Formulario\ParametrizacionUsuarios.aspx")) { $b=[System.IO.File]::ReadAllBytes($f); "{0} -> {1:X2} {2:X2} {3:X2}" -f (Split-Path $f -Leaf), $b[0],$b[1],$b[2] }
```

Esperado: `EF BB BF` en ambos.

---

## Verificación manual pendiente (requiere la aplicación corriendo)

Ningún subagente puede hacer esto: la app necesita **IIS Express de 32 bits**. Va como lote final.

1. Abrir `ParametrizacionUsuarios.aspx`, buscar un usuario y confirmar que la tabla y el formulario cargan con **tildes correctas**.
2. Editar un campo, guardar, volver a buscar y confirmar que el cambio **persistió**.
3. Guardar sin cambiar nada → mensaje "No hubo cambios que guardar".
4. Escribir un correo inválido → se rechaza antes de guardar.
5. **La prueba que importa:** restablecer la contraseña de un usuario de prueba e **iniciar sesión con la nueva**. Es lo único que demuestra que el hash coincide con el que espera `Sp_RTAAutenticaUsuario`. Después, restaurar su contraseña original.
6. Ver historial → aparecen los cambios con el responsable correcto (tu `Cod_Usuario`, no `SISTEMA`).
7. POST al handler **sin sesión** → debe ser rechazado sin tocar la base.

## Notas de despliegue

- Aplicar los scripts SQL **antes** de publicar los binarios, y en este orden: `usuarios-lectura.sql` (crea la bitácora) **antes** que `usuarios-escritura.sql`, que la usa. El `CREATE PROCEDURE` compila igual aunque la tabla no exista, y el error solo aparecería en la primera ejecución real.
- Archivos a subir al servidor: `Formulario\ParametrizacionUsuarios.aspx`, `Formulario\AdministrarUsuarios.ashx`, `js\parametrizacionUsuarios.js` y los binarios de `bin\`.
- El ítem de menú queda activo apenas se aplica el script de la Tarea 7. Si la publicación no es inmediata, desactívalo (`UPDATE dbo.PerfilMenu SET Estado=1 WHERE id_Menu=<IdMenu>`) para que nadie reciba un 404.
