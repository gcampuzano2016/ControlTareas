# Perfil del colaborador — Fase 3a · Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Que el colaborador le ponga cara a su perfil, adjunte los respaldos de lo que declaró en la hoja de vida, y se lleve todo en un PDF.

**Architecture:** Se extiende el módulo de las fases 1 y 2 sin cambiar su forma. Las dos tablas (`Perfil_Foto`, `Perfil_Documento`) ya existen en producción desde la fase 1 y el séptimo result set de `Sp_RTA_PerfilColaborador` ya devuelve los documentos: falta leerlos, escribirlos y pintarlos. La foto entra como **noveno** result set, por el final, respetando el contrato posicional. La subida de archivos **no** reusa `CargaArchivos.ashx` y la descarga **no** es un enlace estático: las dos decisiones están razonadas abajo. La lógica que puede fallar en silencio —qué extensión se acepta, qué HTML se escapa— se extrae a funciones puras en `CapaNegocio` para probarla sin base de datos ni servidor, igual que en las fases anteriores.

**Tech Stack:** ASP.NET WebForms (.NET Framework 4.8 en el web, 4.6.1 en las capas), SQL Server, jQuery + Bootstrap 3, PdfSharp 1.32 + HtmlRenderer.PdfSharp 1.5 (ya referenciados por el proyecto web), MSTest v1 (ensamblado de VS2019, sin NuGet), `vstest.console.exe`.

**Spec:** `docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md`

**Fases anteriores:**
- `docs/superpowers/plans/2026-09-14-perfil-colaborador-fase1.md` (completa, desplegada en producción)
- `docs/superpowers/plans/2026-09-14-perfil-colaborador-fase2.md` (completa, SQL aplicado en producción, binarios pendientes de despliegue por el usuario)

**Fase hermana, que NO entra aquí:** la vista de jefatura (fase 3b). Es la única parte del módulo donde una persona ve datos de otra y merece su propio ciclo de revisión. Este plan no toca `ListaEquipo` ni `PerfilEquipo`.

## Global Constraints

- **Rama:** `ProyectoNuevosCambios`. No commitear ni publicar sin que el usuario lo pida.
- **Compilar con el MSBuild de VS2019**, nunca el del PATH: `"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"`. En Git Bash hace falta `MSYS_NO_PATHCONV=1` y `-p:` en vez de `/p:`.
- **`Cod_Usuario` sale de `context.Session["Cod_Usuario"]`. Nunca del payload, nunca del formulario, nunca de la query string.**
- **Todo procedimiento de escritura lleva la guarda `@CodigoRepetido`** con `RETURN;` real. Un `SELECT` de retorno no interrumpe la ejecución en T-SQL.
- **La validación vive en el servidor.** El handler es alcanzable por HTTP; una validación que sólo esté en el navegador no es una validación.
- **Nada se inserta en la página construyendo HTML.** Todo con `.text()` o `.attr()`: estos datos los teclea el propio usuario, incluido el nombre de un archivo.
- **El nombre del archivo en disco no lo elige el cliente.** Lo arma el servidor.
- **El script SQL corre antes que los binarios**, es idempotente, y arranca con `SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;` — no los toques.
- **El SQL contra producción lo ejecuta el controlador de la sesión, no el implementador.** Escribe el script, avisa, y commitea después de que te devuelvan la salida.
- **`miPerfil.js` cambia: el `?v=` sube de 5 a 6** en `MiPerfil.aspx`. Lo hace la tarea 4 y **ninguna tarea posterior lo vuelve a subir.**
- Comentarios de código en español **sin tildes**; el texto de cara al usuario sí las lleva, **y habla de usted**, como el resto del sistema.
- **Commitear con rutas explícitas de archivo.** Nunca `git add` de directorio: el repositorio versiona carpetas `bin/`.
- **Todo commit termina con `Co-Authored-By: <tu modelo> <noreply@anthropic.com>`**, como los de las fases anteriores. Los `git commit -m` de este plan traen sólo el asunto para no repetirlo diez veces; el cuerpo y esa línea los pones tú, y el cuerpo explica **por qué**, no qué archivos tocaste — eso ya lo dice el diff.
- Estilo SQL de la casa: `INT IDENTITY(1,1)`, constraints con nombre, `DATETIME2(0)` con `SYSDATETIME()`, **sin claves foráneas**.
- **No toques `Web.config`.** `maxRequestLength` ya está en 204800 KB (200 MB); el tope de 5 MB por documento es nuestro y se aplica en código.

## Lo que ya está montado

No lo vuelvas a construir:

| Pieza | Estado |
|---|---|
| `Perfil_Foto` (`Cod_Usuario` PK, `FotoBase64 VARCHAR(MAX)`, `FotoTipo`) | Creada en producción, vacía |
| `Perfil_Documento` (`IdDocumento`, `Cod_Usuario`, `Origen`, `IdOrigen`, `NombreArchivo`, `NombreArchivoCodigo`, `Ruta`, `Estado`) | Creada en producción, vacía |
| `Sp_RTA_PerfilColaborador` | Devuelve **ocho** conjuntos; el **7º ya es el de documentos**, con la guarda aplicada y las columnas correctas |
| `DaoPerfil.CargarPerfil` | Lee 1-6 y 8; **salta el 7 con un `dr.NextResult()` pelado** |
| `DaoPerfil.EjecutarEscritura` / `RespuestaDe` | Centralizan la escritura y la traducción de `0 / -1 / -2` |
| `AdministrarPerfil.ashx` | 12 acciones JSON, `IRequiresSessionState`, identidad siempre de la sesión |
| `MiPerfil.aspx` | Seis pestañas, `MostrarMensaje` y el modal informativo funcionando |
| `NegPerfilCampos` | 8 funciones públicas, 70 pruebas pasando en `CapaPruebas` |
| PdfSharp + HtmlRenderer | Referenciados en `ReporteTareas.csproj` (líneas 67-71, 107-111). `clases/PdfLista.cs` es el precedente vivo |

## Cinco decisiones tomadas antes de escribir el plan

Las tres primeras se **apartan de la letra del spec**. Se documentan aquí para que el revisor las juzgue como decisiones y no como descuidos; la tarea 10 las lleva al spec.

**1. La subida NO reusa `CargaArchivos.ashx`. El spec decía que sí.**

El spec dice: *«Los documentos van por `multipart` reusando `CargaArchivos.ashx`, que es donde ya vive la subida real.»* El terreno lo desmiente: `CargaArchivos` se declara `public class CargaArchivos : IHttpHandler` — **sin `IRequiresSessionState`**. Ahí `context.Session` es `null`, y por eso ese handler saca la identidad de un campo `session` cifrado que **manda el cliente** (`seguridad.Desencripta(context.Request.Form.Get("session"))`).

Todo este módulo descansa sobre lo contrario: la identidad sale de la sesión, nunca del cliente. Reusar ese handler sería entregarle al navegador la decisión de a nombre de quién se sube un archivo.

Así que se reusa **el patrón** (`context.Request.Files`, `postedFile.SaveAs`, `Server.MapPath`) dentro de `AdministrarPerfil.ashx`, que ya es `IRequiresSessionState`. Ni se toca `CargaArchivos.ashx` —está vivo en producción para tareas, contratos y convenios— ni se le agrega `IRequiresSessionState`, que cambiaría el comportamiento de un handler ajeno por una conveniencia nuestra.

**2. Los documentos NO se sirven como archivo estático.**

El patrón de la casa es `<a href='../descargas/<nombre>'>`: el archivo queda servido por IIS, sin ninguna comprobación de a quién pertenece. Para el adjunto de una tarea eso es defendible. Para la partida de nacimiento de un hijo no: bastaría acertar el nombre.

Entonces: los archivos se guardan en `~/descargas/perfil/` con un nombre que el servidor genera (`Perfil_<guid>.<ext>`), esa carpeta lleva un `web.config` que le quita a IIS todos los handlers —de modo que un `GET` directo a la carpeta no devuelve nada—, y la descarga pasa por `DescargarPerfil.ashx`, que le pregunta a la base si ese documento es de quien lo pide **antes** de leer el disco.

**3. El CV no se escribe en disco.**

El spec dice *«genera el PDF con PdfSharp/HtmlRenderer en `~/descargas/` con nombre `CV_<Cod_Usuario>_<timestamp>.pdf` y devuelve la ruta»*. Se genera en memoria y se escribe directo en la respuesta. Es menos código, no deja archivos que limpiar, y sobre todo no deja en una carpeta servida por IIS un PDF con la cédula, la fecha de nacimiento y el domicilio de una persona. El nombre del spec se conserva, pero como nombre de la descarga, no de un archivo en el servidor.

**4. La foto entra como result set 9, no como columnas de la cabecera.**

`Sp_RTA_PerfilColaborador` hay que recrearlo de todos modos para agregarla. Ponerla en el conjunto 1 obligaría a reescribir el `SELECT` de la cabecera, que es justo el que lleva el `LEFT JOIN` que hace entrar a los 119 sin ficha y el `TRY_CONVERT(date, ..., 103)` del que depende la edad del 60% de la gente. Añadir un conjunto al final deja ese `SELECT` **byte a byte igual** y el diff queda puramente aditivo. Es además el contrato que el módulo ya escribió dos veces: se amplía por el final.

**5. El borrado de un documento es lógico y no borra el archivo del disco.**

`Estado = '0'`, como las otras seis tablas del módulo. El archivo queda: `DescargarPerfil.ashx` sólo entrega documentos con `Estado = '1'`, así que deja de ser alcanzable. Borrar el archivo físico agregaría un modo de fallo (la base dice que se borró, el disco falló) a cambio de espacio que no es problema. La casa tampoco borra.

## Estructura de archivos

**Se crean:**

| Archivo | Responsabilidad |
|---|---|
| `docs/sql/2026-09-15-perfil-colaborador-fase3a.sql` | 5 procedimientos nuevos + `Sp_RTA_PerfilColaborador` recreado con el conjunto 9 |
| `CapaEntidad/EntPerfilFoto.cs` | La foto: `Base64` y `Tipo` de ida, `DataUri` de vuelta |
| `CapaEntidad/EntPerfilDocumento.cs` | Un respaldo colgado de una certificación o de una carga familiar |
| `CapaNegocio/NegPerfilCv.cs` | Arma el HTML del CV. Función pura, sin PdfSharp, sin `System.Web` |
| `ReporteTareas/clases/PdfHojaVida.cs` | HTML → `byte[]`. Lo único que sabe de PdfSharp |
| `ReporteTareas/Formulario/DescargarPerfil.ashx` + `.ashx.cs` | Las dos descargas (documento y CV) con la guarda de propiedad |
| `ReporteTareas/descargas/perfil/web.config` | Le quita a IIS los handlers de esa carpeta |
| `CapaPruebas/NegPerfilCvTests.cs` | Pruebas del HTML del CV |

**Se modifican:**

| Archivo | Cambio |
|---|---|
| `CapaEntidad/EntPerfilCompleto.cs` | Propiedades `Foto` y `Documentos` |
| `CapaDato/DaoPerfil.cs` | Lee el conjunto 7 y el 9; cinco métodos de escritura y lectura nuevos |
| `CapaNegocio/NegPerfil.cs` | Fachada de los cinco métodos nuevos |
| `CapaNegocio/NegPerfilCampos.cs` | `ValidarFoto` y `ValidarDocumento` |
| `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs` | Acciones `GuardarFoto`, `EliminarFoto`, `EliminarDocumento` y la rama `multipart` |
| `ReporteTareas/Formulario/MiPerfil.aspx` | Foto en la barra lateral, columna de respaldos, botón del CV, `?v=6` |
| `ReporteTareas/js/miPerfil.js` | Foto, documentos y el enlace del CV |
| `ReporteTareas/ReporteTareas.csproj` | Los archivos nuevos del proyecto web |
| `CapaPruebas/CapaPruebas.csproj` | `NegPerfilCvTests.cs` |
| `CapaPruebas/NegPerfilCamposTests.cs` | Pruebas de `ValidarFoto` y `ValidarDocumento` |
| `docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md` | Las tres desviaciones y la fase marcada |

---

### Task 1: El script SQL de la fase 3a

**Files:**
- Create: `docs/sql/2026-09-15-perfil-colaborador-fase3a.sql`
- Leer como referencia: `docs/sql/2026-09-14-perfil-colaborador-fase2.sql` (líneas 501-670: el texto vigente de `Sp_RTA_PerfilColaborador`)

**Interfaces:**
- Produces: `Sp_RTA_PerfilGuardarFoto` (`@Cod_Usuario`, `@FotoBase64`, `@FotoTipo`, `@Ip`) → `Respuestas`; `Sp_RTA_PerfilEliminarFoto` (`@Cod_Usuario`, `@Ip`) → `Respuestas`; `Sp_RTA_PerfilGuardarDocumento` (`@Cod_Usuario`, `@Origen`, `@IdOrigen`, `@NombreArchivo`, `@NombreArchivoCodigo`, `@Ruta`, `@Ip`) → `Respuestas`, `IdDocumento`; `Sp_RTA_PerfilEliminarDocumento` (`@Cod_Usuario`, `@IdDocumento`, `@Ip`) → `Respuestas`; `Sp_RTA_PerfilDocumentoArchivo` (`@Cod_Usuario`, `@IdDocumento`) → 0 o 1 fila con `NombreArchivo`, `NombreArchivoCodigo`, `Ruta`; y `Sp_RTA_PerfilColaborador` con un **noveno** result set `FotoBase64, FotoTipo`.

**No ejecutes este script.** Cuando lo tengas escrito y commiteado, dilo en el reporte: lo corre el controlador de la sesión contra producción y te devuelve la salida.

- [ ] **Step 1: Escribir la cabecera del script**

Crea `docs/sql/2026-09-15-perfil-colaborador-fase3a.sql`:

```sql
/* ============================================================================
   Perfil del colaborador - FASE 3a
   Foto de perfil, documentos de respaldo y lo que el CV necesita leer.

   Contenido:
     1. Sp_RTA_PerfilGuardarFoto
     2. Sp_RTA_PerfilEliminarFoto
     3. Sp_RTA_PerfilGuardarDocumento
     4. Sp_RTA_PerfilEliminarDocumento
     5. Sp_RTA_PerfilDocumentoArchivo
     6. Sp_RTA_PerfilColaborador recreado con el NOVENO result set (foto)
     7. Aserciones

   Las tablas Perfil_Foto y Perfil_Documento ya existen: las creo la fase 1.
   Este script no crea ni altera ninguna tabla.

   Idempotente: se puede correr dos veces sin dano.
   ============================================================================ */

SET NOCOUNT ON;
GO
/* QUOTED_IDENTIFIER y ANSI_NULLS explicitos y dentro del script, no como
   parametro de sqlcmd: DESPLIEGUE.md dice que esto lo corre una persona a
   mano, y sqlcmd los deja apagados por omision. En la fase 1 eso aborto el
   script a media ejecucion (error 1934). */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
```

- [ ] **Step 2: Los dos procedimientos de la foto**

Añade al script:

```sql
/* ------------------------------------------ 1. Sp_RTA_PerfilGuardarFoto --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarFoto','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarFoto;
GO

/* Una fila por persona: si ya tiene foto se reemplaza. No hay historial de
   fotos y no hace falta. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarFoto
    @Cod_Usuario VARCHAR(50),
    @FotoBase64  VARCHAR(MAX),
    @FotoTipo    VARCHAR(50),
    @Ip          VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    /* Cod_Usuario no es unico en R_Usuarios: la PK real es Id_Usuario y hay
       codigos repetidos entre usuarios activos, en un caso entre dos personas
       distintas. Cuando pasa, este procedimiento no escribe: no hay forma de
       saber de quien seria la foto. Mismo criterio que DaoFirmaUsuario. */
    DECLARE @CodigoRepetido BIT = 0;
    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        /* El RETURN es de verdad necesario: en T-SQL un SELECT no interrumpe
           la ejecucion, y sin el se seguiria escribiendo despues de haber
           devuelto -2. */
        SELECT Respuestas = -2;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Perfil_Foto WHERE Cod_Usuario = @Cod_Usuario)
        UPDATE dbo.Perfil_Foto
           SET FotoBase64       = @FotoBase64,
               FotoTipo         = @FotoTipo,
               Fec_Modificacion = SYSDATETIME(),
               Usu_Modificacion = @Cod_Usuario,
               Ip_Modificacion  = @Ip
         WHERE Cod_Usuario = @Cod_Usuario;
    ELSE
        INSERT INTO dbo.Perfil_Foto
              (Cod_Usuario, FotoBase64, FotoTipo, Usu_Modificacion, Ip_Modificacion)
        VALUES (@Cod_Usuario, @FotoBase64, @FotoTipo, @Cod_Usuario, @Ip);

    SELECT Respuestas = 0;
END
GO
PRINT 'Sp_RTA_PerfilGuardarFoto creado.';
GO

/* ----------------------------------------- 2. Sp_RTA_PerfilEliminarFoto --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarFoto','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarFoto;
GO

/* Borrado fisico y no logico, a diferencia del resto del modulo: Perfil_Foto
   no tiene columna Estado -es una fila por persona, no una lista- y guardar
   una foto marcada como borrada no le sirve a nadie. @Ip se recibe por
   simetria con los demas procedimientos aunque una fila que desaparece no
   tenga donde anotarla. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarFoto
    @Cod_Usuario VARCHAR(50),
    @Ip          VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;
    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    DELETE FROM dbo.Perfil_Foto WHERE Cod_Usuario = @Cod_Usuario;

    IF @@ROWCOUNT = 0
        SELECT Respuestas = -1;
    ELSE
        SELECT Respuestas = 0;
END
GO
PRINT 'Sp_RTA_PerfilEliminarFoto creado.';
GO
```

- [ ] **Step 3: Los tres procedimientos de documentos**

Añade al script:

```sql
/* ------------------------------------- 3. Sp_RTA_PerfilGuardarDocumento --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarDocumento','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarDocumento;
GO

CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarDocumento
    @Cod_Usuario         VARCHAR(50),
    @Origen              VARCHAR(20),
    @IdOrigen            INT,
    @NombreArchivo       VARCHAR(260),
    @NombreArchivoCodigo VARCHAR(260),
    @Ruta                VARCHAR(400),
    @Ip                  VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;
    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2, IdDocumento = 0;
        RETURN;
    END

    /* Un documento cuelga de algo, y ese algo tiene que ser suyo. Sin esta
       comprobacion un POST directo colgaria un archivo de la certificacion de
       otra persona: el handler valida que el IdOrigen sea un numero, pero no
       puede saber de quien es. Aqui si. */
    DECLARE @EsSuyo BIT = 0;

    IF @Origen = 'CERTIFICACION'
       AND EXISTS (SELECT 1 FROM dbo.Perfil_Certificacion
                    WHERE IdCertificacion = @IdOrigen
                      AND Cod_Usuario     = @Cod_Usuario
                      AND Estado          = '1')
        SET @EsSuyo = 1;

    /* Emp_CargaFamiliar la comparte RRHHEmpleados.aspx, que crea filas con
       Cod_Usuario NULL. Exigir Cod_Usuario = @Cod_Usuario deja fuera esas
       filas, que es exactamente lo que se quiere: nadie adjunta un documento
       a una carga que no registro desde su perfil. */
    IF @Origen = 'CARGAFAMILIAR'
       AND EXISTS (SELECT 1 FROM dbo.Emp_CargaFamiliar
                    WHERE IdCargaFam  = @IdOrigen
                      AND Cod_Usuario = @Cod_Usuario
                      AND Estado      = '1')
        SET @EsSuyo = 1;

    IF @EsSuyo = 0
    BEGIN
        SELECT Respuestas = -1, IdDocumento = 0;
        RETURN;
    END

    INSERT INTO dbo.Perfil_Documento
          (Cod_Usuario, Origen, IdOrigen, NombreArchivo, NombreArchivoCodigo,
           Ruta, Usu_Modificacion, Ip_Modificacion)
    VALUES (@Cod_Usuario, @Origen, @IdOrigen, @NombreArchivo, @NombreArchivoCodigo,
            @Ruta, @Cod_Usuario, @Ip);

    SELECT Respuestas = 0, IdDocumento = CONVERT(INT, SCOPE_IDENTITY());
END
GO
PRINT 'Sp_RTA_PerfilGuardarDocumento creado.';
GO

/* ------------------------------------ 4. Sp_RTA_PerfilEliminarDocumento --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarDocumento','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEliminarDocumento;
GO

/* Borrado logico. El archivo se queda en el disco a proposito: desde que
   Estado pasa a '0' deja de ser alcanzable -Sp_RTA_PerfilDocumentoArchivo
   exige '1'-, y borrarlo agregaria un modo de fallo (la base dice borrado, el
   disco fallo) a cambio de espacio que no es un problema. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilEliminarDocumento
    @Cod_Usuario VARCHAR(50),
    @IdDocumento INT,
    @Ip          VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;
    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    IF @CodigoRepetido = 1
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    UPDATE dbo.Perfil_Documento
       SET Estado           = '0',
           Fec_Modificacion = SYSDATETIME(),
           Usu_Modificacion = @Cod_Usuario,
           Ip_Modificacion  = @Ip
     WHERE IdDocumento = @IdDocumento
       AND Cod_Usuario = @Cod_Usuario
       AND Estado      = '1';

    IF @@ROWCOUNT = 0
        SELECT Respuestas = -1;
    ELSE
        SELECT Respuestas = 0;
END
GO
PRINT 'Sp_RTA_PerfilEliminarDocumento creado.';
GO

/* ------------------------------------- 5. Sp_RTA_PerfilDocumentoArchivo --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilDocumentoArchivo','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilDocumentoArchivo;
GO

/* La guarda de la descarga. Devuelve una fila solo si ese documento es de esa
   persona y sigue activo; cero filas en cualquier otro caso, incluido el del
   codigo repetido. El handler no decide nada: si no hay fila, no hay archivo.

   Que no tenga parametro @Ip no es un olvido: es una lectura. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilDocumentoArchivo
    @Cod_Usuario VARCHAR(50),
    @IdDocumento INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;
    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    SELECT NombreArchivo, NombreArchivoCodigo, Ruta
      FROM dbo.Perfil_Documento
     WHERE IdDocumento = @IdDocumento
       AND Cod_Usuario = @Cod_Usuario
       AND Estado      = '1'
       AND @CodigoRepetido = 0;
END
GO
PRINT 'Sp_RTA_PerfilDocumentoArchivo creado.';
GO
```

- [ ] **Step 4: Recrear `Sp_RTA_PerfilColaborador` con el noveno result set**

Copia **literalmente** el bloque de `docs/sql/2026-09-14-perfil-colaborador-fase2.sql` que va de la línea 501 (`/* ----- 5. Sp_RTA_PerfilColaborador (recreado) --- */`) a la línea 670 (el `GO` posterior al `PRINT`), pégalo en el script nuevo, y hazle **exactamente tres** cambios:

1. El comentario del encabezado de la sección: `/* ------------------------- 6. Sp_RTA_PerfilColaborador (recreado) --- */`
2. Inmediatamente **antes** del `END` que cierra el procedimiento —es decir, después del `ORDER BY Fecha_nacimiento DESC, IdCargaFam;` del octavo conjunto— inserta:

```sql

    /* 9. foto de perfil

       Va al final, como fue el 8 en su momento: el Dao recorre los result sets
       POR POSICION y un contrato posicional se amplia por el final. Meterla en
       la cabecera habria obligado a reescribir el SELECT del conjunto 1, que es
       el que lleva el LEFT JOIN de los 119 sin ficha y el TRY_CONVERT con
       estilo 103 del que depende la edad. Este SELECT deja aquel intacto.

       Devuelve cero filas si la persona no subio foto: es el caso normal el
       primer dia y el Dao lo trata como "sin foto", no como error. */
    SELECT FotoBase64, FotoTipo
      FROM dbo.Perfil_Foto
     WHERE Cod_Usuario = @Cod_Usuario
       AND @CodigoRepetido = 0;
```

3. El texto del `PRINT` final: `PRINT 'Sp_RTA_PerfilColaborador recreado con el noveno result set.';`

**Nada más.** Ni reformatees, ni corrijas comentarios, ni "mejores" el `SELECT` de la cabecera: el valor de este paso es que el diff contra el texto de producción sea únicamente lo de arriba.

- [ ] **Step 5: Las aserciones**

Añade al final del script:

```sql
/* ------------------------------------------------------- 7. aserciones --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarFoto','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilGuardarFoto no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarFoto','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilEliminarFoto no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarDocumento','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilGuardarDocumento no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilEliminarDocumento','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilEliminarDocumento no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilDocumentoArchivo','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilDocumentoArchivo no quedo creado.', 16, 1);

/* El noveno result set tiene que estar: el Dao lo lee por posicion y si
   faltara leeria nulos sin quejarse. Se apunta al texto de la consulta -FROM
   mas una columna propia de la tabla- y no a un comentario, por lo mismo que
   la asercion equivalente de la fase 2: un LIKE contra un comentario pasaria
   aunque alguien borrara el SELECT. */
IF NOT EXISTS (SELECT 1 FROM sys.sql_modules m
                JOIN sys.procedures p ON p.object_id = m.object_id
               WHERE p.name = 'Sp_RTA_PerfilColaborador'
                 AND m.definition LIKE '%FROM dbo.Perfil_Foto%'
                 AND m.definition LIKE '%FotoBase64%')
    RAISERROR('FALLO: Sp_RTA_PerfilColaborador no tiene el noveno result set.', 16, 1);

/* El octavo tampoco se puede haber perdido en la recreacion. */
IF NOT EXISTS (SELECT 1 FROM sys.sql_modules m
                JOIN sys.procedures p ON p.object_id = m.object_id
               WHERE p.name = 'Sp_RTA_PerfilColaborador'
                 AND m.definition LIKE '%FROM dbo.Emp_CargaFamiliar%')
    RAISERROR('FALLO: Sp_RTA_PerfilColaborador perdio el octavo result set.', 16, 1);

/* Y el septimo, que es el que esta fase empieza a leer. */
IF NOT EXISTS (SELECT 1 FROM sys.sql_modules m
                JOIN sys.procedures p ON p.object_id = m.object_id
               WHERE p.name = 'Sp_RTA_PerfilColaborador'
                 AND m.definition LIKE '%FROM dbo.Perfil_Documento%')
    RAISERROR('FALLO: Sp_RTA_PerfilColaborador perdio el septimo result set.', 16, 1);

PRINT 'Fase 3a: script terminado.';
GO
```

- [ ] **Step 6: Verificar el diff contra el texto de producción**

Comprueba que sólo cambiaste lo que dijiste. Desde la raíz del repositorio, en Git Bash:

```bash
sed -n '501,670p' docs/sql/2026-09-14-perfil-colaborador-fase2.sql > /tmp/sp-fase2.sql
grep -n "Sp_RTA_PerfilColaborador (recreado)" docs/sql/2026-09-15-perfil-colaborador-fase3a.sql
```

Con el número de línea que devuelve el `grep`, extrae el mismo bloque del script nuevo a `/tmp/sp-fase3a.sql` y compáralos:

```bash
diff /tmp/sp-fase2.sql /tmp/sp-fase3a.sql
```

Esperado: **sólo** las tres diferencias del Step 4 — el comentario del encabezado, el bloque del conjunto 9, y el texto del `PRINT`. Si aparece cualquier otra línea, deshazla. Pega la salida del `diff` en tu reporte.

- [ ] **Step 7: Commit**

```bash
git add docs/sql/2026-09-15-perfil-colaborador-fase3a.sql
git commit -m "feat(perfil): script SQL de la fase 3a - foto, documentos y el noveno result set"
```

- [ ] **Step 8: Avisar**

En el reporte, escribe textualmente: **«El script `docs/sql/2026-09-15-perfil-colaborador-fase3a.sql` está listo y commiteado. No lo ejecuté. Hace falta correrlo contra producción antes de la tarea 2.»** Incluye la salida del `diff` del Step 6.

---

### Task 2: Entidades y lectura de los conjuntos 7 y 9

**Files:**
- Create: `CapaEntidad/EntPerfilFoto.cs`, `CapaEntidad/EntPerfilDocumento.cs`
- Modify: `CapaEntidad/EntPerfilCompleto.cs`, `CapaDato/DaoPerfil.cs:130-152`
- Modify: `CapaEntidad/CapaEntidad.csproj` (los dos `Compile Include` nuevos)

**Interfaces:**
- Consumes: de la tarea 1, el noveno result set de `Sp_RTA_PerfilColaborador` (`FotoBase64`, `FotoTipo`) y el séptimo, que ya existía (`IdDocumento`, `Origen`, `IdOrigen`, `NombreArchivo`, `NombreArchivoCodigo`, `Ruta`).
- Produces: `EntPerfilFoto` con `string Base64`, `string Tipo`, `string DataUri`. `EntPerfilDocumento` con `int IdDocumento`, `string Origen`, `int IdOrigen`, `string NombreArchivo`, `string NombreArchivoCodigo`, `string Ruta`. `EntPerfilCompleto.Foto` (`EntPerfilFoto`) y `EntPerfilCompleto.Documentos` (`List<EntPerfilDocumento>`), ambas inicializadas en el constructor.

**Requisito previo:** el script de la tarea 1 tiene que estar aplicado en producción. Si no lo está, di **BLOCKED** y no sigas: sin el noveno conjunto, la lectura no tiene qué leer.

- [ ] **Step 1: Crear `EntPerfilFoto`**

Crea `CapaEntidad/EntPerfilFoto.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>
    /// La foto de perfil. Base64 y tipo son el camino de ida -lo que el
    /// navegador manda al guardar-; DataUri es el de vuelta -lo que el
    /// navegador pone en el src de una imagen-. Mismo molde que la firma
    /// guardada, DaoFirmaUsuario.
    /// </summary>
    public class EntPerfilFoto
    {
        /// <summary>Solo el payload, sin el prefijo "data:...;base64,". De ida.</summary>
        public string Base64 { get; set; } = "";

        /// <summary>"image/jpeg" o "image/png". De ida.</summary>
        public string Tipo { get; set; } = "";

        /// <summary>
        /// data:&lt;tipo&gt;;base64,&lt;payload&gt;, listo para el src de una imagen.
        /// Cadena vacia si la persona no tiene foto.
        ///
        /// Al leer se llena SOLO esta y no Base64 ni Tipo: las tres juntas
        /// duplicarian el tamanio del JSON del perfil por nada, y una foto de
        /// 256x256 ya son unos 25 KB de base64.
        /// </summary>
        public string DataUri { get; set; } = "";
    }
}
```

- [ ] **Step 2: Crear `EntPerfilDocumento`**

Crea `CapaEntidad/EntPerfilDocumento.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>
    /// Un archivo de respaldo. Cuelga de una certificacion o de una carga
    /// familiar, y de cual lo dicen Origen e IdOrigen: una sola tabla para los
    /// dos casos, con el mismo codigo para subir, listar y quitar.
    /// </summary>
    public class EntPerfilDocumento
    {
        public int IdDocumento { get; set; }

        /// <summary>"CERTIFICACION" o "CARGAFAMILIAR". En mayusculas, siempre.</summary>
        public string Origen { get; set; } = "";

        /// <summary>IdCertificacion o IdCargaFam, segun Origen.</summary>
        public int IdOrigen { get; set; }

        /// <summary>El nombre con el que la persona subio el archivo. Se muestra.</summary>
        public string NombreArchivo { get; set; } = "";

        /// <summary>
        /// El nombre con el que quedo en el disco, que lo arma el servidor.
        ///
        /// Al leer el perfil se deja VACIA a proposito: esa lista viaja al
        /// navegador y el nombre en disco no tiene nada que hacer alli. La
        /// descarga la pide aparte, con DaoPerfil.ObtenerDocumento.
        /// </summary>
        public string NombreArchivoCodigo { get; set; } = "";

        /// <summary>
        /// Ruta de la aplicacion, "~/descargas/perfil/", no la fisica: asi el
        /// dato sigue sirviendo si el sitio cambia de carpeta o de servidor.
        /// Se deja vacia al leer el perfil, por lo mismo que NombreArchivoCodigo.
        /// </summary>
        public string Ruta { get; set; } = "";
    }
}
```

- [ ] **Step 3: Registrar las dos clases en `CapaEntidad.csproj`**

Dentro del `<ItemGroup>` que lista los `Compile Include`, junto a los demás `EntPerfil*`, añade:

```xml
    <Compile Include="EntPerfilDocumento.cs" />
    <Compile Include="EntPerfilFoto.cs" />
```

- [ ] **Step 4: Añadir `Foto` y `Documentos` a `EntPerfilCompleto`**

En `CapaEntidad/EntPerfilCompleto.cs`, reemplaza el comentario de la clase por:

```csharp
    /// <summary>
    /// Lo que devuelve una sola llamada a Sp_RTA_PerfilColaborador. La fase 1
    /// llena cabecera, contacto y emergencia; la fase 2 agrego Estudios,
    /// Certificaciones, Experiencia y CargasFamiliares; la fase 3a agrego
    /// Documentos (el septimo conjunto, que hasta ahora se saltaba) y Foto
    /// (el noveno, nuevo).
    /// </summary>
```

Después de la propiedad `CargasFamiliares`, añade:

```csharp
        /// <summary>
        /// Los respaldos de todas las certificaciones y cargas familiares en una
        /// sola lista. La pantalla la reparte por Origen e IdOrigen; asi una sola
        /// consulta sirve a las dos pestanias.
        /// </summary>
        public List<EntPerfilDocumento> Documentos { get; set; }

        /// <summary>La foto. DataUri vacia cuando la persona no subio ninguna.</summary>
        public EntPerfilFoto Foto { get; set; }
```

Y en el constructor, después de `CargasFamiliares = new List<EntPerfilCargaFamiliar>();`:

```csharp
            Documentos = new List<EntPerfilDocumento>();
            Foto = new EntPerfilFoto();
```

- [ ] **Step 5: Leer el conjunto 7 en el Dao**

En `CapaDato/DaoPerfil.cs`, reemplaza el bloque que hoy salta el conjunto 7 —el comentario `/* 7. documentos de respaldo: son de la fase 3...` y el `dr.NextResult();` pelado que le sigue— por:

```csharp
                    /* 7. documentos de respaldo */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Documentos.Add(new EntPerfilDocumento
                            {
                                IdDocumento   = EnteroDe(dr, "IdDocumento"),
                                Origen        = Texto(dr, "Origen"),
                                IdOrigen      = EnteroDe(dr, "IdOrigen"),
                                NombreArchivo = Texto(dr, "NombreArchivo")

                                /* NombreArchivoCodigo y Ruta se dejan vacias a
                                   proposito aunque el conjunto las traiga: esta
                                   lista se serializa entera al navegador y el
                                   nombre del archivo en disco no tiene nada que
                                   hacer alli. La descarga los pide aparte, con
                                   ObtenerDocumento, que ademas comprueba de
                                   quien es el documento. */
                            });
                        }
                    }
```

- [ ] **Step 6: Leer el conjunto 9 en el Dao**

En el mismo método, inmediatamente después del bloque del conjunto 8 (el `if (dr.NextResult())` de cargas familiares) y antes del cierre del `using (SqlDataReader dr ...)`, añade:

```csharp
                    /* 9. foto de perfil. Cero filas es el caso normal: nadie
                       tiene foto el primer dia. */
                    if (dr.NextResult() && dr.Read())
                    {
                        string base64 = Texto(dr, "FotoBase64");

                        if (base64 != "")
                        {
                            string tipo = Texto(dr, "FotoTipo");
                            if (tipo == "") { tipo = "image/jpeg"; }

                            /* Se llena solo DataUri y no Base64 ni Tipo: ver el
                               comentario de EntPerfilFoto. El molde del data URI
                               es el de DaoFirmaUsuario. */
                            perfil.Foto.DataUri = "data:" + tipo + ";base64," + base64;
                        }
                    }
```

- [ ] **Step 7: Compilar**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln -p:Configuration=Debug -v:minimal
```

Esperado: `0 Error(s)`.

- [ ] **Step 8: Correr las pruebas existentes**

```bash
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas/bin/Debug/CapaPruebas.dll
```

Esperado: **70 pruebas, todas pasando**. Esta tarea no agrega ninguna —no hay lógica nueva que probar sin base de datos— pero ninguna puede romperse.

- [ ] **Step 9: Commit**

```bash
git add CapaEntidad/EntPerfilFoto.cs CapaEntidad/EntPerfilDocumento.cs CapaEntidad/EntPerfilCompleto.cs CapaEntidad/CapaEntidad.csproj CapaDato/DaoPerfil.cs
git commit -m "feat(perfil): el perfil trae foto y documentos de respaldo"
```

---

### Task 3: La foto — validación, capa de datos y handler

**Files:**
- Modify: `CapaNegocio/NegPerfilCampos.cs`, `CapaDato/DaoPerfil.cs`, `CapaNegocio/NegPerfil.cs`, `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs`
- Test: `CapaPruebas/NegPerfilCamposTests.cs`

**Interfaces:**
- Consumes: `EntPerfilFoto` (tarea 2); `DaoPerfil.EjecutarEscritura(string, Action<SqlCommand>)` y `DaoPerfil.RespuestaDe(int, string, string)`, ambos privados y ya existentes; `Sp_RTA_PerfilGuardarFoto` y `Sp_RTA_PerfilEliminarFoto` (tarea 1).
- Produces: `NegPerfilCampos.ValidarFoto(EntPerfilFoto) → string` (cadena vacía si es válida, si no el mensaje para el usuario); `NegPerfil.GuardarFoto(string codUsuario, EntPerfilFoto foto, string ip) → EntRespuesta`; `NegPerfil.EliminarFoto(string codUsuario, string ip) → EntRespuesta`; las acciones JSON `GuardarFoto` (parámetros `base64`, `tipo`) y `EliminarFoto` (sin parámetros).

- [ ] **Step 1: Escribir las pruebas que fallan**

En `CapaPruebas/NegPerfilCamposTests.cs`, antes del cierre de la clase, añade:

```csharp
        /* ---------------------------------------------------------- foto ---- */

        /* Un PNG de 1x1 real, en base64. Sirve de "foto valida" en las pruebas
           sin depender de ningun archivo. */
        private const string PngDeUnPixel =
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";

        [TestMethod]
        public void ValidarFoto_JpegValido_NoDaError()
        {
            EntPerfilFoto foto = new EntPerfilFoto { Base64 = PngDeUnPixel, Tipo = "image/jpeg" };
            Assert.AreEqual("", NegPerfilCampos.ValidarFoto(foto));
        }

        [TestMethod]
        public void ValidarFoto_PngValido_NoDaError()
        {
            EntPerfilFoto foto = new EntPerfilFoto { Base64 = PngDeUnPixel, Tipo = "image/png" };
            Assert.AreEqual("", NegPerfilCampos.ValidarFoto(foto));
        }

        [TestMethod]
        public void ValidarFoto_Nula_DaError()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarFoto(null));
        }

        [TestMethod]
        public void ValidarFoto_SinContenido_DaError()
        {
            EntPerfilFoto foto = new EntPerfilFoto { Base64 = "", Tipo = "image/jpeg" };
            Assert.AreNotEqual("", NegPerfilCampos.ValidarFoto(foto));
        }

        /// <summary>
        /// Un SVG puede traer JavaScript adentro. Si algun dia se sirviera como
        /// archivo en vez de como data URI, seria XSS almacenado. La lista es
        /// blanca: solo JPEG y PNG.
        /// </summary>
        [TestMethod]
        public void ValidarFoto_TipoSvg_DaError()
        {
            EntPerfilFoto foto = new EntPerfilFoto { Base64 = PngDeUnPixel, Tipo = "image/svg+xml" };
            Assert.AreNotEqual("", NegPerfilCampos.ValidarFoto(foto));
        }

        [TestMethod]
        public void ValidarFoto_TipoVacio_DaError()
        {
            EntPerfilFoto foto = new EntPerfilFoto { Base64 = PngDeUnPixel, Tipo = "" };
            Assert.AreNotEqual("", NegPerfilCampos.ValidarFoto(foto));
        }

        /// <summary>
        /// El navegador manda solo el payload. Si llega el data URI entero, el
        /// base64 guardado quedaria con el prefijo dentro y la imagen no se
        /// veria nunca, sin ningun error que lo delate.
        /// </summary>
        [TestMethod]
        public void ValidarFoto_ConPrefijoDataUri_DaError()
        {
            EntPerfilFoto foto = new EntPerfilFoto
            {
                Base64 = "data:image/jpeg;base64," + PngDeUnPixel,
                Tipo   = "image/jpeg"
            };
            Assert.AreNotEqual("", NegPerfilCampos.ValidarFoto(foto));
        }

        [TestMethod]
        public void ValidarFoto_Base64Invalido_DaError()
        {
            EntPerfilFoto foto = new EntPerfilFoto { Base64 = "esto no es base64 %%%", Tipo = "image/png" };
            Assert.AreNotEqual("", NegPerfilCampos.ValidarFoto(foto));
        }

        /// <summary>
        /// El navegador reduce la foto a 256x256 antes de mandarla, pero el
        /// handler es alcanzable por HTTP directo y la columna es VARCHAR(MAX):
        /// sin tope, un POST podria dejar megabytes en la fila que se lee en
        /// cada carga del perfil.
        /// </summary>
        [TestMethod]
        public void ValidarFoto_DemasiadoGrande_DaError()
        {
            EntPerfilFoto foto = new EntPerfilFoto
            {
                Base64 = new string('A', 500001),
                Tipo   = "image/jpeg"
            };
            Assert.AreNotEqual("", NegPerfilCampos.ValidarFoto(foto));
        }
```

- [ ] **Step 2: Correr las pruebas y verificar que fallan**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas/CapaPruebas.csproj -p:Configuration=Debug -v:minimal
```

Esperado: **no compila**, con `'NegPerfilCampos' no contiene una definición para 'ValidarFoto'`.

- [ ] **Step 3: Escribir `ValidarFoto`**

En `CapaNegocio/NegPerfilCampos.cs`, antes del cierre de la clase, añade:

```csharp
        /// <summary>
        /// Los unicos tipos de imagen que se aceptan. Lista blanca y no negra:
        /// un SVG puede traer JavaScript adentro, y con una lista negra la
        /// pregunta pasa a ser "de que nos acordamos de prohibir".
        /// </summary>
        private static readonly string[] TiposDeFotoValidos = { "image/jpeg", "image/png" };

        /// <summary>
        /// Longitud maxima del base64 de la foto. 500 000 caracteres son unos
        /// 366 KB de imagen: el navegador manda alrededor de 25 KB -reduce a
        /// 256x256 antes de subir- asi que esto es un techo, no un limite de uso.
        /// Se mide sobre el texto y antes de decodificar para no gastar memoria
        /// decodificando lo que se va a rechazar.
        /// </summary>
        private const int LargoMaximoFoto = 500000;

        /// <summary>
        /// Valida la foto que llega del navegador. Cadena vacia si esta bien.
        ///
        /// Va en el servidor y no solo en el navegador porque AdministrarPerfil.ashx
        /// es alcanzable por HTTP directo: lo que el canvas del navegador garantiza
        /// -que sea un JPEG de 256x256- no lo garantiza nadie para un POST hecho a
        /// mano.
        /// </summary>
        public static string ValidarFoto(EntPerfilFoto foto)
        {
            if (foto == null) { return "No se recibió la foto."; }

            string tipo = (foto.Tipo ?? "").Trim().ToLowerInvariant();
            bool tipoValido = false;

            for (int i = 0; i < TiposDeFotoValidos.Length; i++)
            {
                if (TiposDeFotoValidos[i] == tipo) { tipoValido = true; }
            }

            if (!tipoValido)
            {
                return "La foto debe ser una imagen JPG o PNG.";
            }

            string base64 = (foto.Base64 ?? "").Trim();

            if (base64 == "")
            {
                return "No se recibió el contenido de la foto.";
            }

            /* El navegador manda solo el payload. Si llega el data URI completo,
               guardarlo dejaria el prefijo dentro del base64 y la imagen no se
               veria nunca sin que nada avise. */
            if (base64.StartsWith("data:"))
            {
                return "El formato de la foto no es el esperado.";
            }

            if (base64.Length > LargoMaximoFoto)
            {
                return "La foto es demasiado grande. Use una imagen más liviana.";
            }

            /* Que decodifique es la prueba de que es base64 de verdad. Sin esto,
               cualquier texto quedaria guardado como si fuera una imagen y el
               fallo aparecería recien en el navegador de la persona. */
            try
            {
                Convert.FromBase64String(base64);
            }
            catch (FormatException)
            {
                return "El formato de la foto no es el esperado.";
            }

            return "";
        }
```

- [ ] **Step 4: Correr las pruebas y verificar que pasan**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas/CapaPruebas.csproj -p:Configuration=Debug -v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas/bin/Debug/CapaPruebas.dll
```

Esperado: **79 pruebas, todas pasando** (70 de antes + 9 nuevas). Cuenta las que informa `vstest` y contrástalas con ese número; si no coinciden, dilo en el reporte en vez de ajustar el número.

- [ ] **Step 5: Las dos escrituras en el Dao**

En `CapaDato/DaoPerfil.cs`, después de `EliminarCargaFamiliar` y antes de los helpers privados (`Texto`, `EnteroDe`…), añade:

```csharp
        /// <summary>
        /// Guarda o reemplaza la foto. Una fila por persona: no hay historial.
        /// </summary>
        public static EntRespuesta GuardarFoto(string codUsuario, EntPerfilFoto foto, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarFoto", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                /* -1 es el largo que SqlDbType.VarChar usa para VARCHAR(MAX). Sin
                   el, el parametro se trunca a 8000 caracteres y la foto llega
                   cortada: se guarda sin error y no se ve. */
                cmd.Parameters.Add("@FotoBase64",  SqlDbType.VarChar, -1).Value = foto.Base64;
                cmd.Parameters.Add("@FotoTipo",    SqlDbType.VarChar, 50).Value = foto.Tipo;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Su foto se actualizó.", "No se pudo guardar la foto.");
        }

        /// <summary>
        /// Quita la foto. Borrado fisico -Perfil_Foto no tiene columna Estado-,
        /// a diferencia del resto del modulo.
        /// </summary>
        public static EntRespuesta EliminarFoto(string codUsuario, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarFoto", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Su foto se quitó.", "No tenía ninguna foto guardada.");
        }
```

- [ ] **Step 6: La fachada en `NegPerfil`**

En `CapaNegocio/NegPerfil.cs`, después de `EliminarCargaFamiliar`, añade:

```csharp
        /// <summary>Guarda o reemplaza la foto del usuario de la sesion.</summary>
        public static EntRespuesta GuardarFoto(string codUsuario, EntPerfilFoto foto, string ip)
        {
            return DaoPerfil.GuardarFoto(codUsuario, foto, ip);
        }

        /// <summary>Quita la foto del usuario de la sesion.</summary>
        public static EntRespuesta EliminarFoto(string codUsuario, string ip)
        {
            return DaoPerfil.EliminarFoto(codUsuario, ip);
        }
```

- [ ] **Step 7: Las dos acciones del handler**

En `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs`, dentro de `ProcessRequest`, después del bloque `if (Action == "EliminarCargaFamiliar")`, añade:

```csharp
                if (Action == "GuardarFoto")
                {
                    existAction = true;
                    responseAction.Append(GuardarFoto(context, parametros[0]["parameters"]));
                }

                if (Action == "EliminarFoto")
                {
                    existAction = true;
                    responseAction.Append(EliminarFoto(context));
                }
```

Y después del método `EliminarCargaFamiliar`, añade:

```csharp
        /// <summary>
        /// Guarda la foto del usuario de la sesion.
        ///
        /// El navegador reduce la imagen a 256x256 y la manda en base64 por el
        /// mismo canal JSON que todo lo demas, en vez de por multipart: son unos
        /// 25 KB y no hay archivo que guardar en disco -la foto vive en la base,
        /// como la firma-, asi que un multipart solo agregaria un camino mas.
        /// </summary>
        private string GuardarFoto(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                EntPerfilFoto foto = new EntPerfilFoto
                {
                    Base64 = Texto(campos, "base64", ""),
                    Tipo   = Texto(campos, "tipo", "")
                };

                string error = NegPerfilCampos.ValidarFoto(foto);
                if (error != "") { return responseMessage("0", error, "warning"); }

                return ToJson(NegPerfil.GuardarFoto(codUsuario, foto, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar la foto. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Quita la foto. No recibe parametros: solo se puede quitar la propia.
        /// </summary>
        private string EliminarFoto(HttpContext context)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                return ToJson(NegPerfil.EliminarFoto(codUsuario, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al quitar la foto. " + ex.Message, "danger");
            }
        }
```

- [ ] **Step 8: Compilar y correr las pruebas**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln -p:Configuration=Debug -v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas/bin/Debug/CapaPruebas.dll
```

Esperado: `0 Error(s)` y **79 pruebas pasando**.

- [ ] **Step 9: Commit**

```bash
git add CapaNegocio/NegPerfilCampos.cs CapaNegocio/NegPerfil.cs CapaDato/DaoPerfil.cs ReporteTareas/Formulario/AdministrarPerfil.ashx.cs CapaPruebas/NegPerfilCamposTests.cs
git commit -m "feat(perfil): guardar y quitar la foto de perfil"
```

---

### Task 4: La foto en la pantalla

**Files:**
- Modify: `ReporteTareas/Formulario/MiPerfil.aspx:4` (el `?v=`) y la barra lateral (líneas 23-35)
- Modify: `ReporteTareas/js/miPerfil.js`

**Interfaces:**
- Consumes: la acción JSON `GuardarFoto` (parámetros `base64`, `tipo`) y `EliminarFoto` (tarea 3); `respuesta.Foto.DataUri` de `CargarPerfil` (tarea 2); las funciones ya existentes `PostPerfil(action, parameters, onSuccess)`, `MostrarMensaje(mensaje, tipo)`, `CargarPerfil()` e `Iniciales(nombre)`.
- Produces: `PintarFoto(foto)`, `RecortarCuadrado(imagen)`, `EnviarFoto(dataUri)`, `QuitarFoto()`.

- [ ] **Step 1: Subir la versión del JavaScript**

En `ReporteTareas/Formulario/MiPerfil.aspx`, línea 4, cambia `?v=5` por `?v=6`:

```aspx
    <script src="../js/miPerfil.js?v=6" type="text/javascript"></script>
```

Sin esto, los navegadores que ya cargaron la pantalla siguen usando el JavaScript viejo y la foto no aparece para nadie hasta que alguien vacíe su caché. **Ninguna tarea posterior vuelve a tocar este número.**

- [ ] **Step 2: La foto en la barra lateral**

En `ReporteTareas/Formulario/MiPerfil.aspx`, reemplaza el `<div id="perfilAvatar">` y lo que le sigue hasta el `<hr />` por:

```aspx
                        <!-- Dos representaciones excluyentes: la foto si la hay,
                             y si no las iniciales, que es lo que la fase 1 dejo. -->
                        <img id="perfilFoto" alt="Foto de perfil"
                             style="display: none; width: 96px; height: 96px; margin: 0 auto 12px;
                                    border-radius: 50%; object-fit: cover" />
                        <div id="perfilAvatar"
                             style="width: 96px; height: 96px; margin: 0 auto 12px; border-radius: 50%;
                                    background: #750202; color: #fff; font-size: 34px; line-height: 96px;">–</div>
                        <h4 id="perfilNombre" style="margin: 0 0 4px">–</h4>
                        <p id="perfilCargo" class="text-muted" style="margin: 0 0 10px">–</p>

                        <!-- El input va oculto y lo dispara el boton: el control de
                             archivo nativo no se puede estilar y desentona. -->
                        <input type="file" id="inFoto" accept="image/jpeg,image/png" style="display: none" />
                        <p style="margin: 0 0 10px">
                            <button type="button" class="btn btn-default btn-xs" onclick="ElegirFoto()">
                                <i class="fa fa-camera"></i> Cambiar foto
                            </button>
                            <button type="button" id="btnQuitarFoto" class="btn btn-default btn-xs"
                                    style="display: none" onclick="QuitarFoto()">
                                <i class="fa fa-trash"></i> Quitar
                            </button>
                        </p>

                        <span id="perfilArea" class="label label-primary">–</span>
                        <span id="perfilCiudad" class="label label-default">–</span>
                        <hr />
```

- [ ] **Step 3: Pintar la foto al cargar el perfil**

En `ReporteTareas/js/miPerfil.js`, dentro de `CargarPerfil`, después de `PintarCargasFamiliares(respuesta.CargasFamiliares);`, añade:

```js
        PintarFoto(respuesta.Foto);
```

Y después de la función `PintarCabecera`, añade:

```js
/* La foto y las iniciales son excluyentes: si hay foto, las iniciales sobran.
   Esta funcion corre despues de PintarCabecera, que es la que escribe las
   iniciales, asi que el orden de las dos llamadas importa. */
function PintarFoto(foto) {
    if (foto && foto.DataUri) {
        $("#perfilFoto").attr("src", foto.DataUri).show();
        $("#perfilAvatar").hide();
        $("#btnQuitarFoto").show();
    } else {
        $("#perfilFoto").hide().removeAttr("src");
        $("#perfilAvatar").show();
        $("#btnQuitarFoto").hide();
    }
}
```

- [ ] **Step 4: Elegir, reducir y enviar la foto**

En `ReporteTareas/js/miPerfil.js`, al final del archivo, añade:

```js
/* ------------------------------------------------------------------ foto -- */

/* 256 es el doble de los 96 px con los que se muestra: se ve nitida en
   pantallas de densidad doble y sigue pesando unos 25 KB en base64. */
var FOTO_LADO = 256;

function ElegirFoto() {
    $("#inFoto").click();
}

$(document).on("change", "#inFoto", function () {
    var archivo = this.files && this.files[0];

    /* Se limpia el input antes de nada: si no, volver a elegir el mismo archivo
       no dispara "change" y el usuario cree que el boton dejo de funcionar. */
    this.value = "";

    if (!archivo) { return; }

    if (archivo.type !== "image/jpeg" && archivo.type !== "image/png") {
        MostrarMensaje("La foto debe ser una imagen JPG o PNG.", "warning");
        return;
    }

    var lector = new FileReader();

    lector.onload = function (e) {
        var imagen = new Image();
        imagen.onload = function () { EnviarFoto(RecortarCuadrado(imagen)); };
        imagen.onerror = function () {
            MostrarMensaje("No pudimos leer esa imagen. Pruebe con otra.", "warning");
        };
        imagen.src = e.target.result;
    };

    lector.onerror = function () {
        MostrarMensaje("No pudimos leer ese archivo. Intente nuevamente.", "warning");
    };

    lector.readAsDataURL(archivo);
});

/* Recorta el cuadrado central y reduce a 256x256.
   Se hace en el navegador y no en el servidor porque lo que viaja es el
   resultado: la foto de 4 MB de un telefono sale de la maquina convertida en
   unos 25 KB. El servidor igual valida lo que recibe -es alcanzable por HTTP
   directo- pero no tiene que cargar con la imagen original. */
function RecortarCuadrado(imagen) {
    var lado = Math.min(imagen.width, imagen.height);
    var x = (imagen.width - lado) / 2;
    var y = (imagen.height - lado) / 2;

    var lienzo = document.createElement("canvas");
    lienzo.width = FOTO_LADO;
    lienzo.height = FOTO_LADO;
    lienzo.getContext("2d").drawImage(imagen, x, y, lado, lado, 0, 0, FOTO_LADO, FOTO_LADO);

    /* Siempre JPEG, aunque el original sea PNG: una foto de una persona pesa
       mucho menos en JPEG y la transparencia no aporta nada en un avatar. */
    return lienzo.toDataURL("image/jpeg", 0.85);
}

function EnviarFoto(dataUri) {
    /* El servidor guarda solo el payload; el prefijo "data:image/jpeg;base64,"
       lo vuelve a armar al leer. Mandar el data URI entero lo rechaza
       ValidarFoto a proposito. */
    var coma = dataUri.indexOf(",");

    PostPerfil("GuardarFoto", { base64: dataUri.substring(coma + 1), tipo: "image/jpeg" },
        function (respuesta) {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            if (respuesta.estado === "1") { CargarPerfil(); }
        });
}

function QuitarFoto() {
    PostPerfil("EliminarFoto", {}, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") { CargarPerfil(); }
    });
}
```

- [ ] **Step 5: Verificar que todos los selectores existen**

Los identificadores que toca el JavaScript nuevo son `#perfilFoto`, `#perfilAvatar`, `#btnQuitarFoto` e `#inFoto`. Compruébalo:

```bash
for id in perfilFoto perfilAvatar btnQuitarFoto inFoto; do
  echo -n "$id -> "; grep -c "id=\"$id\"" ReporteTareas/Formulario/MiPerfil.aspx
done
```

Esperado: `1` para los cuatro. Un `0` es un selector que no resuelve y que no da ningún error en el navegador: la función simplemente no hace nada. Pega la salida en el reporte.

- [ ] **Step 6: Compilar**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln -p:Configuration=Debug -v:minimal
```

Esperado: `0 Error(s)`.

- [ ] **Step 7: Commit**

```bash
git add ReporteTareas/Formulario/MiPerfil.aspx ReporteTareas/js/miPerfil.js
git commit -m "feat(perfil): la foto se cambia y se quita desde la pantalla"
```

---

### Task 5: Documentos — validación, capa de datos y subida

**Files:**
- Modify: `CapaNegocio/NegPerfilCampos.cs`, `CapaDato/DaoPerfil.cs`, `CapaNegocio/NegPerfil.cs`, `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs`
- Test: `CapaPruebas/NegPerfilCamposTests.cs`

**Interfaces:**
- Consumes: `EntPerfilDocumento` (tarea 2); `Sp_RTA_PerfilGuardarDocumento` y `Sp_RTA_PerfilEliminarDocumento` (tarea 1); `DaoPerfil.EjecutarEscritura` y `DaoPerfil.RespuestaDe`.
- Produces: `NegPerfilCampos.ValidarDocumento(string origen, int idOrigen, string nombreArchivo, long tamanoBytes) → string`; `NegPerfil.GuardarDocumento(string codUsuario, EntPerfilDocumento doc, string ip) → EntRespuesta`; `NegPerfil.EliminarDocumento(string codUsuario, int idDocumento, string ip) → EntRespuesta`; la rama `multipart` del handler (campos de formulario `origen`, `idOrigen` y un archivo) y la acción JSON `EliminarDocumento` (parámetro `idDocumento`).

- [ ] **Step 1: Escribir las pruebas que fallan**

En `CapaPruebas/NegPerfilCamposTests.cs`, antes del cierre de la clase, añade:

```csharp
        /* ----------------------------------------------------- documentos ---- */

        [TestMethod]
        public void ValidarDocumento_PdfDeCertificacion_NoDaError()
        {
            Assert.AreEqual("", NegPerfilCampos.ValidarDocumento("CERTIFICACION", 7, "titulo.pdf", 120000));
        }

        [TestMethod]
        public void ValidarDocumento_JpgDeCargaFamiliar_NoDaError()
        {
            Assert.AreEqual("", NegPerfilCampos.ValidarDocumento("CARGAFAMILIAR", 3, "partida.jpg", 90000));
        }

        /// <summary>
        /// La extension se compara en minusculas. Los telefonos suben ".JPG" en
        /// mayusculas mas seguido de lo que parece.
        /// </summary>
        [TestMethod]
        public void ValidarDocumento_ExtensionEnMayusculas_NoDaError()
        {
            Assert.AreEqual("", NegPerfilCampos.ValidarDocumento("CERTIFICACION", 7, "TITULO.PDF", 120000));
        }

        [TestMethod]
        public void ValidarDocumento_OrigenDesconocido_DaError()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarDocumento("EMPLEADOS", 7, "titulo.pdf", 1000));
        }

        [TestMethod]
        public void ValidarDocumento_OrigenVacio_DaError()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarDocumento("", 7, "titulo.pdf", 1000));
        }

        [TestMethod]
        public void ValidarDocumento_SinIdOrigen_DaError()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarDocumento("CERTIFICACION", 0, "titulo.pdf", 1000));
        }

        /// <summary>
        /// La razon por la que esta lista es blanca y no negra. Un .aspx en una
        /// carpeta del sitio es codigo que el servidor ejecuta: aceptarlo no es
        /// un archivo raro, es entregar el servidor. Esta prueba existe para que
        /// nadie convierta la lista blanca en una negra "para ser mas flexibles".
        /// </summary>
        [TestMethod]
        public void ValidarDocumento_ExtensionAspx_DaError()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarDocumento("CERTIFICACION", 7, "malo.aspx", 1000));
        }

        [TestMethod]
        public void ValidarDocumento_ExtensionExe_DaError()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarDocumento("CERTIFICACION", 7, "malo.exe", 1000));
        }

        /// <summary>
        /// "titulo.pdf.aspx" tiene que mirarse por la ULTIMA extension, que es la
        /// que decide como lo trata el servidor.
        /// </summary>
        [TestMethod]
        public void ValidarDocumento_DobleExtension_DaError()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarDocumento("CERTIFICACION", 7, "titulo.pdf.aspx", 1000));
        }

        [TestMethod]
        public void ValidarDocumento_SinExtension_DaError()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarDocumento("CERTIFICACION", 7, "titulo", 1000));
        }

        [TestMethod]
        public void ValidarDocumento_NombreVacio_DaError()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarDocumento("CERTIFICACION", 7, "", 1000));
        }

        [TestMethod]
        public void ValidarDocumento_ArchivoVacio_DaError()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarDocumento("CERTIFICACION", 7, "titulo.pdf", 0));
        }

        [TestMethod]
        public void ValidarDocumento_MasDeCincoMegas_DaError()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarDocumento("CERTIFICACION", 7, "titulo.pdf", 5242881));
        }

        [TestMethod]
        public void ValidarDocumento_CincoMegasExactos_NoDaError()
        {
            Assert.AreEqual("", NegPerfilCampos.ValidarDocumento("CERTIFICACION", 7, "titulo.pdf", 5242880));
        }
```

- [ ] **Step 2: Correr las pruebas y verificar que fallan**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas/CapaPruebas.csproj -p:Configuration=Debug -v:minimal
```

Esperado: **no compila**, con `'NegPerfilCampos' no contiene una definición para 'ValidarDocumento'`.

- [ ] **Step 3: Escribir `ValidarDocumento`**

En `CapaNegocio/NegPerfilCampos.cs`, antes del cierre de la clase, añade:

```csharp
        /// <summary>
        /// De que puede colgar un documento. Los mismos dos valores que espera
        /// Sp_RTA_PerfilGuardarDocumento; si algun dia se agrega un tercero, van
        /// juntos o el procedimiento lo rechazara con -1 sin explicar por que.
        /// </summary>
        private static readonly string[] OrigenesDeDocumentoValidos = { "CERTIFICACION", "CARGAFAMILIAR" };

        /// <summary>
        /// Extensiones aceptadas. Lista BLANCA, y tiene que seguir siendolo: un
        /// .aspx o un .ashx dentro de una carpeta del sitio es codigo que el
        /// servidor ejecuta. Con una lista negra la pregunta pasa a ser de que
        /// nos acordamos de prohibir, y basta olvidar una para entregar el
        /// servidor.
        /// </summary>
        private static readonly string[] ExtensionesDeDocumentoValidas = { "pdf", "jpg", "jpeg", "png" };

        /// <summary>5 MB. Web.config permite 200, pero un respaldo escaneado no los necesita.</summary>
        private const long TamanoMaximoDocumento = 5242880;

        /// <summary>
        /// Valida un documento de respaldo antes de guardarlo. Cadena vacia si
        /// esta bien.
        ///
        /// Lo que esta funcion NO valida, porque no puede: que el IdOrigen sea de
        /// quien sube el archivo. Eso lo comprueba Sp_RTA_PerfilGuardarDocumento
        /// contra la base y devuelve -1 si no lo es.
        /// </summary>
        public static string ValidarDocumento(string origen, int idOrigen, string nombreArchivo, long tamanoBytes)
        {
            string origenNormalizado = (origen ?? "").Trim().ToUpperInvariant();
            bool origenValido = false;

            for (int i = 0; i < OrigenesDeDocumentoValidos.Length; i++)
            {
                if (OrigenesDeDocumentoValidos[i] == origenNormalizado) { origenValido = true; }
            }

            if (!origenValido)
            {
                return "No se pudo determinar a qué registro corresponde el documento.";
            }

            if (idOrigen <= 0)
            {
                return "No se pudo determinar a qué registro corresponde el documento.";
            }

            string nombre = (nombreArchivo ?? "").Trim();

            if (nombre == "")
            {
                return "No se recibió el archivo.";
            }

            /* GetExtension devuelve la ULTIMA, que es la que decide como trata el
               servidor al archivo: "titulo.pdf.aspx" es un .aspx. */
            string extension = Path.GetExtension(nombre).TrimStart('.').ToLowerInvariant();
            bool extensionValida = false;

            for (int i = 0; i < ExtensionesDeDocumentoValidas.Length; i++)
            {
                if (ExtensionesDeDocumentoValidas[i] == extension) { extensionValida = true; }
            }

            if (!extensionValida)
            {
                return "El documento debe ser un PDF o una imagen JPG o PNG.";
            }

            if (tamanoBytes <= 0)
            {
                return "El archivo está vacío.";
            }

            if (tamanoBytes > TamanoMaximoDocumento)
            {
                return "El documento no puede pesar más de 5 MB.";
            }

            return "";
        }
```

Añade `using System.IO;` a los `using` del archivo si no está.

- [ ] **Step 4: Correr las pruebas y verificar que pasan**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas/CapaPruebas.csproj -p:Configuration=Debug -v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas/bin/Debug/CapaPruebas.dll
```

Esperado: **93 pruebas, todas pasando** (79 + 14 nuevas). Contrasta con lo que informe `vstest`.

- [ ] **Step 5: Las escrituras en el Dao**

En `CapaDato/DaoPerfil.cs`, después de `EliminarFoto` y antes de los helpers privados, añade:

```csharp
        /// <summary>
        /// Registra un documento de respaldo.
        ///
        /// El procedimiento devuelve -1 cuando el IdOrigen no es de esta persona:
        /// es la unica comprobacion posible de que el documento se cuelga de algo
        /// suyo, porque el handler ve un numero y no sabe de quien es.
        /// </summary>
        public static EntRespuesta GuardarDocumento(string codUsuario, EntPerfilDocumento doc, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarDocumento", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario",         SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@Origen",              SqlDbType.VarChar,  20).Value = doc.Origen;
                cmd.Parameters.Add("@IdOrigen",            SqlDbType.Int).Value          = doc.IdOrigen;
                cmd.Parameters.Add("@NombreArchivo",       SqlDbType.VarChar, 260).Value = doc.NombreArchivo;
                cmd.Parameters.Add("@NombreArchivoCodigo", SqlDbType.VarChar, 260).Value = doc.NombreArchivoCodigo;
                cmd.Parameters.Add("@Ruta",                SqlDbType.VarChar, 400).Value = doc.Ruta;
                cmd.Parameters.Add("@Ip",                  SqlDbType.VarChar,  64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Documento adjuntado.", "No se encontró el registro al que quiere adjuntarlo.");
        }

        /// <summary>Borrado logico de un documento. El archivo se queda en el disco.</summary>
        public static EntRespuesta EliminarDocumento(string codUsuario, int idDocumento, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilEliminarDocumento", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdDocumento", SqlDbType.Int).Value         = idDocumento;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
            });

            return RespuestaDe(r, "Documento quitado.", "No se encontró ese documento.");
        }
```

- [ ] **Step 6: La fachada en `NegPerfil`**

En `CapaNegocio/NegPerfil.cs`, después de `EliminarFoto`, añade:

```csharp
        /// <summary>Registra un documento de respaldo del usuario de la sesion.</summary>
        public static EntRespuesta GuardarDocumento(string codUsuario, EntPerfilDocumento doc, string ip)
        {
            return DaoPerfil.GuardarDocumento(codUsuario, doc, ip);
        }

        /// <summary>Borrado logico de un documento del usuario de la sesion.</summary>
        public static EntRespuesta EliminarDocumento(string codUsuario, int idDocumento, string ip)
        {
            return DaoPerfil.EliminarDocumento(codUsuario, idDocumento, ip);
        }
```

- [ ] **Step 7: La rama `multipart` y la acción de borrado en el handler**

En `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs`, dentro de `ProcessRequest`, después del bloque `if (Action == "EliminarFoto")`, añade:

```csharp
                if (Action == "EliminarDocumento")
                {
                    existAction = true;
                    responseAction.Append(EliminarDocumento(context, parametros[0]["parameters"]));
                }
```

Después, entre el `else if` del JSON y el `else` final, inserta una rama nueva:

```csharp
            else if (context.Request.Files.Count > 0)
            {
                /* La subida de documentos es lo unico que no viaja como JSON: un
                   archivo necesita multipart. Va en ESTE handler y no en
                   CargaArchivos.ashx -que es donde vive la subida del resto del
                   sistema- porque aquel se declara sin IRequiresSessionState: alli
                   context.Session es null y la identidad la manda el cliente en un
                   campo cifrado del formulario. Todo este modulo descansa en lo
                   contrario. */
                responseAction.Append(SubirDocumento(context));
            }
```

Y después del método `EliminarFoto`, añade:

```csharp
        /// <summary>
        /// Recibe un documento de respaldo y lo cuelga de una certificacion o de
        /// una carga familiar.
        ///
        /// El orden importa: primero la base y despues el disco. Si se escribiera
        /// el archivo antes y la base lo rechazara -porque el IdOrigen no es de
        /// esta persona, o porque su codigo esta repetido-, quedaria un archivo
        /// huerfano en el servidor que nada volveria a nombrar.
        /// </summary>
        private string SubirDocumento(HttpContext context)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                string origen = (context.Request.Form.Get("origen") ?? "").Trim().ToUpperInvariant();

                int idOrigen;
                if (!int.TryParse(context.Request.Form.Get("idOrigen"), out idOrigen)) { idOrigen = 0; }

                HttpPostedFile archivo = context.Request.Files.Get(0);

                /* Path.GetFileName descarta cualquier ruta que venga en el nombre.
                   Sin esto, un nombre como "..\..\Formulario\algo.pdf" saldria del
                   directorio previsto. */
                string nombreArchivo = System.IO.Path.GetFileName(archivo.FileName ?? "");

                string error = NegPerfilCampos.ValidarDocumento(origen, idOrigen, nombreArchivo,
                                                                archivo.ContentLength);
                if (error != "") { return responseMessage("0", error, "warning"); }

                string extension = System.IO.Path.GetExtension(nombreArchivo).TrimStart('.').ToLowerInvariant();

                /* El nombre en disco NO lo elige el cliente: lo arma el servidor con
                   un GUID. Asi dos personas que suban "cedula.pdf" no se pisan, y la
                   direccion del archivo no se puede adivinar desde otra sesion. */
                string nombreCodigo = "Perfil_" + Guid.NewGuid().ToString("N") + "." + extension;

                const string rutaApp = "~/descargas/perfil/";
                string carpeta = context.Server.MapPath(rutaApp);

                if (!System.IO.Directory.Exists(carpeta))
                {
                    System.IO.Directory.CreateDirectory(carpeta);
                }

                EntPerfilDocumento doc = new EntPerfilDocumento
                {
                    Origen              = origen,
                    IdOrigen            = idOrigen,
                    NombreArchivo       = nombreArchivo,
                    NombreArchivoCodigo = nombreCodigo,
                    /* Se guarda la ruta de la aplicacion y no la fisica: asi el
                       registro sigue sirviendo si el sitio cambia de carpeta. */
                    Ruta                = rutaApp
                };

                EntRespuesta respuesta = NegPerfil.GuardarDocumento(codUsuario, doc,
                                                                    context.Request.UserHostAddress);

                if (respuesta.estado != "1") { return ToJson(respuesta); }

                archivo.SaveAs(System.IO.Path.Combine(carpeta, nombreCodigo));

                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al subir el documento. " + ex.Message, "danger");
            }
        }

        /// <summary>Borrado logico de un documento del usuario de la sesion.</summary>
        private string EliminarDocumento(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                int id = Convert.ToInt32(Texto(campos, "idDocumento", "0"));

                return ToJson(NegPerfil.EliminarDocumento(codUsuario, id, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al quitar el documento. " + ex.Message, "danger");
            }
        }
```

- [ ] **Step 8: Verificar que la identidad sigue saliendo sólo de la sesión**

```bash
grep -c "CodUsuarioSesion(context)" ReporteTareas/Formulario/AdministrarPerfil.ashx.cs
grep -n "Form.Get(\"codUsuario\")\|Texto(campos, \"codUsuario\"\|QueryString\[\"cod" ReporteTareas/Formulario/AdministrarPerfil.ashx.cs
```

Esperado: el primero devuelve **16** (12 de antes + `GuardarFoto`, `EliminarFoto`, `SubirDocumento`, `EliminarDocumento`); el segundo, **ninguna línea**. Pega las dos salidas en el reporte, y si el primer número no es 16, dilo en vez de ajustarlo.

- [ ] **Step 9: Compilar y correr las pruebas**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln -p:Configuration=Debug -v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas/bin/Debug/CapaPruebas.dll
```

Esperado: `0 Error(s)` y **93 pruebas pasando**.

- [ ] **Step 10: Commit**

```bash
git add CapaNegocio/NegPerfilCampos.cs CapaNegocio/NegPerfil.cs CapaDato/DaoPerfil.cs ReporteTareas/Formulario/AdministrarPerfil.ashx.cs CapaPruebas/NegPerfilCamposTests.cs
git commit -m "feat(perfil): subir y quitar documentos de respaldo"
```

---

### Task 6: La descarga de documentos, con guarda

**Files:**
- Create: `ReporteTareas/Formulario/DescargarPerfil.ashx`, `ReporteTareas/Formulario/DescargarPerfil.ashx.cs`, `ReporteTareas/descargas/perfil/web.config`
- Modify: `CapaDato/DaoPerfil.cs`, `CapaNegocio/NegPerfil.cs`, `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes: `Sp_RTA_PerfilDocumentoArchivo` (tarea 1); `EntPerfilDocumento` (tarea 2).
- Produces: `DaoPerfil.ObtenerDocumento(string codUsuario, int idDocumento) → EntPerfilDocumento` (**`null`** cuando el documento no es de esa persona, no existe, está borrado, o el código está repetido); `NegPerfil.ObtenerDocumento` con la misma firma; y la URL `DescargarPerfil.ashx?doc=<IdDocumento>`. La tarea 9 añade a este mismo handler la rama `?cv=1`.

- [ ] **Step 1: La lectura en el Dao**

En `CapaDato/DaoPerfil.cs`, después de `EliminarDocumento`, añade:

```csharp
        /// <summary>
        /// Los datos de archivo de un documento, SOLO si es de esta persona y
        /// sigue activo. Devuelve null en cualquier otro caso.
        ///
        /// Este null es la guarda de la descarga entera: el handler no decide
        /// nada, pregunta. Devolver un objeto vacio en vez de null seria peor
        /// -habria que acordarse de mirar si viene vacio-, y una excepcion
        /// convertiria en error lo que tambien es el caso normal de un enlace
        /// viejo a un documento ya quitado.
        /// </summary>
        public static EntPerfilDocumento ObtenerDocumento(string codUsuario, int idDocumento)
        {
            EntPerfilDocumento doc = null;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilDocumentoArchivo", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdDocumento", SqlDbType.Int).Value         = idDocumento;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        doc = new EntPerfilDocumento
                        {
                            IdDocumento         = idDocumento,
                            NombreArchivo       = Texto(dr, "NombreArchivo"),
                            NombreArchivoCodigo = Texto(dr, "NombreArchivoCodigo"),
                            Ruta                = Texto(dr, "Ruta")
                        };
                    }
                }
            }

            return doc;
        }
```

- [ ] **Step 2: La fachada en `NegPerfil`**

En `CapaNegocio/NegPerfil.cs`, después de `EliminarDocumento`, añade:

```csharp
        /// <summary>
        /// El documento con sus datos de archivo, o null si no es de esta persona.
        /// </summary>
        public static EntPerfilDocumento ObtenerDocumento(string codUsuario, int idDocumento)
        {
            return DaoPerfil.ObtenerDocumento(codUsuario, idDocumento);
        }
```

- [ ] **Step 3: El `web.config` de la carpeta**

Crea `ReporteTareas/descargas/perfil/web.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
  Esta carpeta guarda respaldos personales: partidas de nacimiento, titulos,
  cedulas. Sin este archivo, IIS los serviria como archivos estaticos a
  cualquiera que acertara el nombre, sin preguntarle nada a nadie.

  <clear /> deja la carpeta sin ningun handler, de modo que IIS no tiene con
  que responder a una peticion directa. Los archivos se siguen leyendo del
  disco desde DescargarPerfil.ashx, que primero le pregunta a la base de quien
  es el documento; ese camino no pasa por los handlers de esta carpeta y no se
  ve afectado.
-->
<configuration>
  <system.webServer>
    <handlers>
      <clear />
    </handlers>
  </system.webServer>
</configuration>
```

- [ ] **Step 4: El handler**

Crea `ReporteTareas/Formulario/DescargarPerfil.ashx` con una sola línea:

```aspx
<%@ WebHandler Language="C#" CodeBehind="DescargarPerfil.ashx.cs" Class="JsonJQueryNetPerfil.DescargarPerfil" %>
```

Crea `ReporteTareas/Formulario/DescargarPerfil.ashx.cs`:

```csharp
using CapaEntidad;
using CapaNegocio;
using System;
using System.IO;
using System.Web;

namespace JsonJQueryNetPerfil
{
    /// <summary>
    /// Las descargas del perfil. Handler aparte de AdministrarPerfil.ashx porque
    /// aquel responde JSON a peticiones POST y esto es un GET que escribe bytes:
    /// meterlos juntos obligaria a que uno de los dos rompiera su propio contrato.
    ///
    /// IRequiresSessionState no es decorativo: sin el, context.Session es null en
    /// un IHttpHandler y no habria identidad con la que comprobar nada.
    ///
    /// La identidad sale SIEMPRE de la sesion. El unico parametro que se acepta de
    /// la query string es que documento se quiere; de quien es lo dice la base.
    /// </summary>
    public class DescargarPerfil : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.Session == null || context.Session["UserLogin"] == null)
            {
                NoDisponible(context, "Su sesión expiró. Vuelva a iniciar sesión.");
                return;
            }

            string codUsuario = CodUsuarioSesion(context);

            if (codUsuario == "")
            {
                NoDisponible(context, "No se pudo identificar al usuario de la sesión.");
                return;
            }

            int idDocumento;
            if (!int.TryParse(context.Request.QueryString["doc"], out idDocumento) || idDocumento <= 0)
            {
                NoDisponible(context, "No se indicó qué documento descargar.");
                return;
            }

            EntregarDocumento(context, codUsuario, idDocumento);
        }

        /// <summary>
        /// Entrega un documento de respaldo.
        ///
        /// ObtenerDocumento devuelve null cuando el documento no es de esta
        /// persona, no existe, ya fue quitado, o su codigo de usuario esta
        /// repetido. Los cuatro casos dan el mismo mensaje a proposito: uno
        /// distinto para "existe pero no es suyo" le confirmaria a quien esta
        /// probando numeros que acerto con uno.
        /// </summary>
        private void EntregarDocumento(HttpContext context, string codUsuario, int idDocumento)
        {
            EntPerfilDocumento doc = NegPerfil.ObtenerDocumento(codUsuario, idDocumento);

            if (doc == null)
            {
                NoDisponible(context, "No se encontró ese documento.");
                return;
            }

            string rutaFisica = context.Server.MapPath(doc.Ruta) + doc.NombreArchivoCodigo;

            if (!File.Exists(rutaFisica))
            {
                NoDisponible(context, "El archivo ya no está disponible en el servidor.");
                return;
            }

            context.Response.Clear();
            context.Response.Buffer = true;
            context.Response.ContentType = TipoDeContenido(doc.NombreArchivoCodigo);
            context.Response.AddHeader("Content-Disposition",
                                       "attachment;filename=" + NombreSeguro(doc.NombreArchivo));
            context.Response.TransmitFile(rutaFisica);
            context.Response.End();
        }

        /// <summary>
        /// El tipo segun la extension. La lista es la misma que acepta
        /// NegPerfilCampos.ValidarDocumento; cualquier otra cosa sale como binario
        /// generico, que el navegador descarga en vez de interpretar.
        /// </summary>
        private static string TipoDeContenido(string nombreArchivo)
        {
            string extension = Path.GetExtension(nombreArchivo ?? "").TrimStart('.').ToLowerInvariant();

            if (extension == "pdf") { return "application/pdf"; }
            if (extension == "png") { return "image/png"; }
            if (extension == "jpg" || extension == "jpeg") { return "image/jpeg"; }

            return "application/octet-stream";
        }

        /// <summary>
        /// El nombre del archivo lo puso el usuario y aca va dentro de una cabecera
        /// HTTP. Un salto de linea ahi le agrega cabeceras a la respuesta; unas
        /// comillas cierran el valor antes de tiempo. Se quitan los tres caracteres
        /// y, si no queda nada, se usa un nombre generico.
        /// </summary>
        private static string NombreSeguro(string nombreArchivo)
        {
            string limpio = (nombreArchivo ?? "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("\"", "")
                .Trim();

            return limpio == "" ? "documento" : limpio;
        }

        /// <summary>De quien es esta descarga. Sale de la sesion, nunca del cliente.</summary>
        private string CodUsuarioSesion(HttpContext context)
        {
            if (context.Session != null && context.Session["Cod_Usuario"] != null)
            {
                return context.Session["Cod_Usuario"].ToString().Trim();
            }
            return "";
        }

        /// <summary>
        /// Lo que ve quien pide algo que no puede tener. Texto plano y no JSON: el
        /// navegador llega aqui por un enlace, no por una llamada de JavaScript, y
        /// un JSON en pantalla no le dice nada a nadie.
        /// </summary>
        private void NoDisponible(HttpContext context, string mensaje)
        {
            context.Response.Clear();
            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.Write(mensaje);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
```

- [ ] **Step 5: Registrar los archivos en el csproj**

En `ReporteTareas/ReporteTareas.csproj`, junto a las entradas de `CargaArchivos.ashx`, añade el contenido:

```xml
    <Content Include="Formulario\DescargarPerfil.ashx" />
    <Content Include="descargas\perfil\web.config" />
```

Y en el `<ItemGroup>` de los `Compile Include`, junto a `Formulario\AdministrarPerfil.ashx.cs`:

```xml
    <Compile Include="Formulario\DescargarPerfil.ashx.cs">
      <DependentUpon>DescargarPerfil.ashx</DependentUpon>
    </Compile>
```

Es la misma forma que tiene `Formulario\AdministrarPerfil.ashx.cs` (línea 1488): `DependentUpon` y nada más, sin `SubType`.

- [ ] **Step 6: Compilar y correr las pruebas**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln -p:Configuration=Debug -v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas/bin/Debug/CapaPruebas.dll
```

Esperado: `0 Error(s)` y **93 pruebas pasando**.

- [ ] **Step 7: Commit**

```bash
git add ReporteTareas/Formulario/DescargarPerfil.ashx ReporteTareas/Formulario/DescargarPerfil.ashx.cs ReporteTareas/descargas/perfil/web.config ReporteTareas/ReporteTareas.csproj CapaDato/DaoPerfil.cs CapaNegocio/NegPerfil.cs
git commit -m "feat(perfil): los documentos se descargan por handler y solo si son suyos"
```

---

### Task 7: Los documentos en la pantalla

**Files:**
- Modify: `ReporteTareas/Formulario/MiPerfil.aspx` (tabla de certificaciones, tabla de cargas familiares, un `<input type="file">` compartido)
- Modify: `ReporteTareas/js/miPerfil.js` (`PintarCertificaciones`, `PintarCargasFamiliares`, funciones nuevas)

**Interfaces:**
- Consumes: `respuesta.Documentos` de `CargarPerfil` —lista de `{ IdDocumento, Origen, IdOrigen, NombreArchivo }`— (tarea 2); la rama `multipart` de `AdministrarPerfil.ashx` y la acción `EliminarDocumento` (tarea 5); la URL `DescargarPerfil.ashx?doc=<id>` (tarea 6); la variable global `_perfil` y las funciones `MostrarMensaje`, `CargarPerfil`.
- Produces: `DocumentosDe(origen, idOrigen)`, `CeldaDocumentos(origen, idOrigen)`, `PedirArchivo(origen, idOrigen)`, `EliminarDocumento(idDocumento)`.

**Ojo con el nombre:** `EliminarDocumento` es a la vez un método privado de C# en el handler y una función global de JavaScript. No es un conflicto —viven en runtimes distintos— y es la misma convención que ya siguen `EliminarCertificacion` y las demás.

- [ ] **Step 1: La columna de respaldos en las dos tablas**

En `ReporteTareas/Formulario/MiPerfil.aspx`, en la tabla de **certificaciones**, cambia la fila del encabezado por:

```aspx
                                            <tr><th>Nombre</th><th>Entidad emisora</th><th style="width:120px">Obtenida</th><th style="width:170px">Respaldo</th><th style="width:70px">Quitar</th></tr>
```

En la tabla de **cargas familiares**, añade el mismo `<th style="width:170px">Respaldo</th>` justo antes de la columna `Quitar` de su encabezado.

Y antes del `<!-- Modal Informativo -->`, añade el input compartido:

```aspx
        <!-- Un solo control de archivo para toda la pantalla. Antes de abrirlo se
             le cuelga con .data() a que fila pertenece: un input por fila serian
             tantos como respaldos pueda tener la persona, creados y destruidos en
             cada repintado. -->
        <input type="file" id="inDocumento" accept=".pdf,.jpg,.jpeg,.png" style="display: none" />
```

- [ ] **Step 2: Las funciones de documentos en el JavaScript**

En `ReporteTareas/js/miPerfil.js`, al final del archivo, añade:

```js
/* ------------------------------------------------------------ documentos -- */

/* Los respaldos vienen todos en una sola lista y la pantalla los reparte: asi
   una unica consulta sirve a las dos pestanias. */
function DocumentosDe(origen, idOrigen) {
    var encontrados = [];

    if (_perfil && _perfil.Documentos) {
        $.each(_perfil.Documentos, function (i, d) {
            if (d.Origen === origen && d.IdOrigen === idOrigen) { encontrados.push(d); }
        });
    }

    return encontrados;
}

/* La celda de respaldos de una fila: los que ya tiene, y el enlace para sumar
   uno mas. Se arma con jQuery y .text() -nunca concatenando HTML- porque el
   nombre del archivo lo escribio la persona al guardarlo en su maquina. */
function CeldaDocumentos(origen, idOrigen) {
    var $celda = $('<td class="text-center"></td>');

    $.each(DocumentosDe(origen, idOrigen), function (i, d) {
        var $fila = $('<div style="margin-bottom:3px"></div>');

        var $enlace = $('<a target="_blank" style="font-size:11px"></a>')
            .attr("href", "DescargarPerfil.ashx?doc=" + d.IdDocumento)
            .attr("title", d.NombreArchivo)
            .text(d.NombreArchivo);

        var $quitar = $('<a href="javascript:void(0)" title="Quitar" style="margin-left:6px">' +
                        '<i class="fa fa-times text-danger"></i></a>')
            .on("click", function () { EliminarDocumento(d.IdDocumento); });

        $celda.append($fila.append($enlace).append($quitar));
    });

    var $adjuntar = $('<a href="javascript:void(0)" style="font-size:11px">' +
                      '<i class="fa fa-paperclip"></i> Adjuntar</a>')
        .on("click", function () { PedirArchivo(origen, idOrigen); });

    return $celda.append($adjuntar);
}

/* Le cuelga al input compartido a que fila pertenece y lo abre. */
function PedirArchivo(origen, idOrigen) {
    $("#inDocumento").data("origen", origen).data("idOrigen", idOrigen).click();
}

$(document).on("change", "#inDocumento", function () {
    var archivo = this.files && this.files[0];
    var origen = $(this).data("origen");
    var idOrigen = $(this).data("idOrigen");

    /* Se limpia antes de nada: si no, elegir dos veces el mismo archivo no
       vuelve a disparar "change". */
    this.value = "";

    if (!archivo) { return; }

    var datos = new FormData();
    datos.append("origen", origen);
    datos.append("idOrigen", idOrigen);
    datos.append("archivo", archivo);

    /* Esta llamada no puede usar PostPerfil: aquella manda JSON y esto es
       multipart. processData y contentType en false son lo que hace que jQuery
       entregue el FormData tal cual y deje que el navegador ponga el boundary. */
    $.ajax({
        type: "POST",
        url: "AdministrarPerfil.ashx",
        data: datos,
        processData: false,
        contentType: false,
        dataType: "json",
        success: function (respuesta) {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            if (respuesta.estado === "1") { CargarPerfil(); }
        },
        error: function () {
            MostrarMensaje("No pudimos subir el archivo. Intente nuevamente.", "danger");
        }
    });
});

function EliminarDocumento(idDocumento) {
    PostPerfil("EliminarDocumento", { idDocumento: idDocumento }, function (respuesta) {
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
        if (respuesta.estado === "1") { CargarPerfil(); }
    });
}
```

- [ ] **Step 3: Pintar la celda en certificaciones**

En `PintarCertificaciones`, dentro del `$.each`, entre la celda de la fecha y la del botón de quitar, añade:

```js
        $fila.append(CeldaDocumentos("CERTIFICACION", c.IdCertificacion));
```

Y en el mensaje de lista vacía de esa misma función, cambia `colspan="4"` por `colspan="5"`: la tabla ahora tiene cinco columnas y con el `colspan` viejo el texto queda desalineado.

- [ ] **Step 4: Pintar la celda en cargas familiares**

En `PintarCargasFamiliares`, dentro del `$.each`, antes de la celda del botón de quitar, añade:

```js
        $fila.append(CeldaDocumentos("CARGAFAMILIAR", c.IdCargaFam));
```

Y en el mensaje de lista vacía de esa misma función, cambia `colspan="4"` por `colspan="5"`.

- [ ] **Step 5: Verificar los selectores y los `colspan`**

```bash
grep -c "id=\"inDocumento\"" ReporteTareas/Formulario/MiPerfil.aspx
grep -n "colspan" ReporteTareas/js/miPerfil.js
grep -c "<th" ReporteTareas/Formulario/MiPerfil.aspx
```

Contrasta, tabla por tabla, que cada `colspan` del JavaScript sea igual al número de `<th>` de su tabla en el `.aspx`. Pega en el reporte la lista de pares (tabla, `<th>`, `colspan`) que verificaste.

- [ ] **Step 6: Compilar**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln -p:Configuration=Debug -v:minimal
```

Esperado: `0 Error(s)`.

- [ ] **Step 7: Commit**

```bash
git add ReporteTareas/Formulario/MiPerfil.aspx ReporteTareas/js/miPerfil.js
git commit -m "feat(perfil): cada certificacion y cada carga familiar lleva sus respaldos"
```

---

### Task 8: El HTML del CV

**Files:**
- Create: `CapaNegocio/NegPerfilCv.cs`, `CapaPruebas/NegPerfilCvTests.cs`
- Modify: `CapaNegocio/CapaNegocio.csproj`, `CapaPruebas/CapaPruebas.csproj`

**Interfaces:**
- Consumes: `EntPerfilCompleto` con `Cabecera`, `Contacto`, `Estudios`, `Certificaciones`, `Experiencia`, `Emergencia` (fases 1 y 2, más la tarea 2).
- Produces: `NegPerfilCv.Construir(EntPerfilCompleto perfil) → string` — el HTML completo del CV, o cadena vacía si `perfil` es `null` o `PerfilEncontrado` es `false`.

**Por qué está en `CapaNegocio` y no junto al PDF:** `CapaPruebas` referencia `CapaEntidad` y `CapaNegocio`, y nada más. Poner el armado del HTML en el proyecto web lo dejaría fuera del alcance de las pruebas, y es justo la parte que puede fallar sin dar la cara: un nombre sin escapar rompe el documento entero y el renderizador no se queja.

- [ ] **Step 1: Escribir las pruebas que fallan**

Crea `CapaPruebas/NegPerfilCvTests.cs`:

```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CapaPruebas
{
    /// <summary>
    /// El HTML del CV. Lo que se prueba aqui es lo que falla en silencio: un
    /// caracter sin escapar no lanza ninguna excepcion -HtmlRenderer no valida
    /// nada- y el PDF sale con la seccion cortada o con el texto en el lugar
    /// equivocado, sin que nadie se entere hasta que alguien mira su CV.
    /// </summary>
    [TestClass]
    public class NegPerfilCvTests
    {
        private static EntPerfilCompleto PerfilMinimo()
        {
            EntPerfilCompleto perfil = new EntPerfilCompleto();
            perfil.PerfilEncontrado = true;
            perfil.Cabecera.NombreCompleto = "Ana Pérez";
            perfil.Cabecera.Cargo = "Analista";
            perfil.Cabecera.Area = "Sistemas";
            return perfil;
        }

        [TestMethod]
        public void Construir_PerfilNulo_DevuelveVacio()
        {
            Assert.AreEqual("", NegPerfilCv.Construir(null));
        }

        /// <summary>
        /// Sin fila de cabecera no se sabe de quien es el perfil. Un CV en blanco
        /// con membrete seria peor que ninguno.
        /// </summary>
        [TestMethod]
        public void Construir_PerfilNoEncontrado_DevuelveVacio()
        {
            EntPerfilCompleto perfil = new EntPerfilCompleto();
            perfil.PerfilEncontrado = false;
            Assert.AreEqual("", NegPerfilCv.Construir(perfil));
        }

        [TestMethod]
        public void Construir_PerfilMinimo_TraeElNombreYEsUnDocumento()
        {
            string html = NegPerfilCv.Construir(PerfilMinimo());

            StringAssert.Contains(html, "Ana P");
            StringAssert.Contains(html, "<html");
            StringAssert.Contains(html, "</html>");
        }

        /// <summary>
        /// La prueba que justifica que esta clase exista. Los nombres los teclea
        /// la propia persona y van sin filtrar a un documento HTML.
        /// </summary>
        [TestMethod]
        public void Construir_NombreConMarcado_LoEscapa()
        {
            EntPerfilCompleto perfil = PerfilMinimo();
            perfil.Cabecera.NombreCompleto = "<b>Ana</b> & Cía";

            string html = NegPerfilCv.Construir(perfil);

            Assert.IsFalse(html.Contains("<b>Ana</b>"), "El marcado del nombre no se escapo.");
            StringAssert.Contains(html, "&lt;b&gt;Ana&lt;/b&gt;");
            StringAssert.Contains(html, "&amp;");
        }

        [TestMethod]
        public void Construir_TextoDeUnaFilaConMarcado_LoEscapa()
        {
            EntPerfilCompleto perfil = PerfilMinimo();
            perfil.Estudios.Add(new EntPerfilEstudio
            {
                Nivel       = "Tercer nivel",
                Institucion = "<script>alert(1)</script>",
                Titulo      = "Ingeniería",
                AnioGraduacion = 2015
            });

            string html = NegPerfilCv.Construir(perfil);

            Assert.IsFalse(html.Contains("<script>"), "El marcado de una fila no se escapo.");
        }

        /// <summary>
        /// Un CV con encabezados de secciones vacias se lee como un formulario a
        /// medio llenar. Las secciones sin filas no se imprimen.
        /// </summary>
        [TestMethod]
        public void Construir_SinEstudios_NoImprimeLaSeccion()
        {
            string html = NegPerfilCv.Construir(PerfilMinimo());
            Assert.IsFalse(html.Contains("Formación académica"));
        }

        [TestMethod]
        public void Construir_ConUnEstudio_ImprimeLaSeccionYElTitulo()
        {
            EntPerfilCompleto perfil = PerfilMinimo();
            perfil.Estudios.Add(new EntPerfilEstudio
            {
                Nivel          = "Tercer nivel",
                Institucion    = "Universidad Central",
                Titulo         = "Ingeniería en Sistemas",
                AnioGraduacion = 2015
            });

            string html = NegPerfilCv.Construir(perfil);

            StringAssert.Contains(html, "Formación académica");
            StringAssert.Contains(html, "Universidad Central");
            StringAssert.Contains(html, "2015");
        }

        [TestMethod]
        public void Construir_ConUnaCertificacion_ImprimeLaSeccion()
        {
            EntPerfilCompleto perfil = PerfilMinimo();
            perfil.Certificaciones.Add(new EntPerfilCertificacion
            {
                Nombre         = "Scrum Master",
                Entidad        = "Scrum Alliance",
                FechaObtencion = "2023-04"
            });

            string html = NegPerfilCv.Construir(perfil);

            StringAssert.Contains(html, "Certificaciones");
            StringAssert.Contains(html, "Scrum Alliance");
        }

        [TestMethod]
        public void Construir_ConExperiencia_ImprimeLaSeccion()
        {
            EntPerfilCompleto perfil = PerfilMinimo();
            perfil.Experiencia.Add(new EntPerfilExperiencia
            {
                Empresa   = "Acme",
                Cargo     = "Desarrollador",
                AnioDesde = 2018,
                AnioHasta = null,
                Funciones = "Mantenimiento"
            });

            string html = NegPerfilCv.Construir(perfil);

            StringAssert.Contains(html, "Experiencia laboral");
            StringAssert.Contains(html, "Acme");
            /* Sin anio de fin, el periodo se lee "2018 - Actual" y no "2018 - ". */
            StringAssert.Contains(html, "Actual");
        }

        /// <summary>
        /// Los contactos de emergencia y las cargas familiares NO van en el CV: un
        /// CV es lo que uno entrega a un tercero, y el telefono de la madre de
        /// alguien no tiene por que salir en el.
        /// </summary>
        [TestMethod]
        public void Construir_ConContactoDeEmergencia_NoLoImprime()
        {
            EntPerfilCompleto perfil = PerfilMinimo();
            perfil.Emergencia.Add(new EntPerfilEmergencia
            {
                IdContacto = 1,
                Nombre     = "Rosa Calderón",
                Parentesco = "Madre",
                Telefono   = "0991234567"
            });

            string html = NegPerfilCv.Construir(perfil);

            Assert.IsFalse(html.Contains("Rosa Calder"), "El contacto de emergencia salio en el CV.");
        }

        [TestMethod]
        public void Construir_ConCargaFamiliar_NoLaImprime()
        {
            EntPerfilCompleto perfil = PerfilMinimo();
            perfil.CargasFamiliares.Add(new EntPerfilCargaFamiliar
            {
                IdCargaFam      = 1,
                Nombre          = "Mateo Suárez",
                Parentesco      = "Hijo/a",
                FechaNacimiento = "2019-05-02"
            });

            string html = NegPerfilCv.Construir(perfil);

            Assert.IsFalse(html.Contains("Mateo Su"), "La carga familiar salio en el CV.");
        }
    }
}
```

- [ ] **Step 2: Registrar la prueba en `CapaPruebas.csproj`**

Junto a `<Compile Include="NegPerfilCamposTests.cs" />`, añade:

```xml
    <Compile Include="NegPerfilCvTests.cs" />
```

- [ ] **Step 3: Correr las pruebas y verificar que fallan**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas/CapaPruebas.csproj -p:Configuration=Debug -v:minimal
```

Esperado: **no compila**, con `El nombre del tipo o del espacio de nombres 'NegPerfilCv' no existe`.

- [ ] **Step 4: Escribir `NegPerfilCv`**

Crea `CapaNegocio/NegPerfilCv.cs`:

```csharp
using CapaEntidad;
using System.Text;

namespace CapaNegocio
{
    /// <summary>
    /// Arma el HTML del CV a partir del perfil ya cargado.
    ///
    /// Vive en esta capa y no junto al generador de PDF por dos razones. La
    /// primera es que CapaPruebas solo referencia CapaEntidad y CapaNegocio: aqui
    /// se puede probar, alla no. La segunda es que asi esta clase no sabe nada de
    /// PdfSharp, y quien lea el HTML que produce no tiene que saber de PDF.
    ///
    /// El marcado va con tablas y estilos en linea, como HtmlSolicitud, y no con
    /// flexbox ni grid: HtmlRenderer entiende un subconjunto chico de CSS y lo
    /// que no entiende lo ignora en silencio, sin error y sin aviso.
    /// </summary>
    public static class NegPerfilCv
    {
        /// <summary>
        /// El CV completo, o cadena vacia si no hay de quien hacerlo.
        /// </summary>
        public static string Construir(EntPerfilCompleto perfil)
        {
            if (perfil == null || !perfil.PerfilEncontrado) { return ""; }

            StringBuilder html = new StringBuilder();

            html.Append("<html><head><meta charset=\"utf-8\" /></head>");
            html.Append("<body style=\"font-family: Arial, sans-serif; font-size: 11pt; color: #222\">");

            Encabezado(html, perfil);
            Datos(html, perfil);
            Estudios(html, perfil);
            Certificaciones(html, perfil);
            Experiencia(html, perfil);

            /* Los contactos de emergencia y las cargas familiares NO van. Un CV se
               entrega a terceros, y el telefono de la madre de alguien no tiene
               por que viajar con el. */

            html.Append("</body></html>");

            return html.ToString();
        }

        private static void Encabezado(StringBuilder html, EntPerfilCompleto perfil)
        {
            html.Append("<div style=\"border-bottom: 2px solid #750202; padding-bottom: 8px; margin-bottom: 14px\">");
            html.Append("<div style=\"font-size: 18pt; font-weight: bold\">");
            html.Append(Escapar(perfil.Cabecera.NombreCompleto));
            html.Append("</div>");

            html.Append("<div style=\"color: #666\">");
            html.Append(Escapar(perfil.Cabecera.Cargo));

            if (perfil.Cabecera.Area != "")
            {
                html.Append(" &middot; ");
                html.Append(Escapar(perfil.Cabecera.Area));
            }

            html.Append("</div></div>");
        }

        /// <summary>
        /// Los datos de contacto, en dos columnas. Se omite cada linea que no
        /// tenga dato en vez de imprimir la etiqueta con un guion: el CV de los
        /// 119 sin ficha de empleado saldria lleno de guiones.
        /// </summary>
        private static void Datos(StringBuilder html, EntPerfilCompleto perfil)
        {
            html.Append("<table style=\"width: 100%; font-size: 10pt; margin-bottom: 16px\">");

            Dato(html, "Cédula", perfil.Cabecera.Cedula);
            Dato(html, "Fecha de nacimiento", perfil.Cabecera.FechaNacTexto);
            Dato(html, "Ciudad", perfil.Cabecera.Ciudad);
            Dato(html, "Correo", perfil.Contacto.CorreoPersonal != ""
                                     ? perfil.Contacto.CorreoPersonal
                                     : perfil.Cabecera.CorreoNotificacion);
            Dato(html, "Teléfono", perfil.Contacto.TelefonoPersonal);
            Dato(html, "Domicilio", perfil.Contacto.Direccion);

            html.Append("</table>");
        }

        private static void Dato(StringBuilder html, string etiqueta, string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) { return; }

            html.Append("<tr><td style=\"width: 160px; color: #666; padding: 2px 0\">");
            html.Append(Escapar(etiqueta));
            html.Append("</td><td style=\"padding: 2px 0\">");
            html.Append(Escapar(valor));
            html.Append("</td></tr>");
        }

        private static void Estudios(StringBuilder html, EntPerfilCompleto perfil)
        {
            if (perfil.Estudios.Count == 0) { return; }

            Titulo(html, "Formación académica");
            html.Append("<table style=\"width: 100%; font-size: 10pt; margin-bottom: 16px\">");

            foreach (EntPerfilEstudio e in perfil.Estudios)
            {
                html.Append("<tr><td style=\"padding: 3px 0\"><b>");
                html.Append(Escapar(e.Titulo));
                html.Append("</b><br /><span style=\"color: #666\">");
                html.Append(Escapar(e.Institucion));

                if (e.Nivel != "")
                {
                    html.Append(" &middot; ");
                    html.Append(Escapar(e.Nivel));
                }

                html.Append("</span></td><td style=\"width: 70px; text-align: right; color: #666\">");
                html.Append(e.AnioGraduacion.HasValue ? e.AnioGraduacion.Value.ToString() : "");
                html.Append("</td></tr>");
            }

            html.Append("</table>");
        }

        private static void Certificaciones(StringBuilder html, EntPerfilCompleto perfil)
        {
            if (perfil.Certificaciones.Count == 0) { return; }

            Titulo(html, "Certificaciones");
            html.Append("<table style=\"width: 100%; font-size: 10pt; margin-bottom: 16px\">");

            foreach (EntPerfilCertificacion c in perfil.Certificaciones)
            {
                html.Append("<tr><td style=\"padding: 3px 0\"><b>");
                html.Append(Escapar(c.Nombre));
                html.Append("</b><br /><span style=\"color: #666\">");
                html.Append(Escapar(c.Entidad));
                html.Append("</span></td><td style=\"width: 70px; text-align: right; color: #666\">");
                html.Append(Escapar(c.FechaObtencion));
                html.Append("</td></tr>");
            }

            html.Append("</table>");
        }

        private static void Experiencia(StringBuilder html, EntPerfilCompleto perfil)
        {
            if (perfil.Experiencia.Count == 0) { return; }

            Titulo(html, "Experiencia laboral");
            html.Append("<table style=\"width: 100%; font-size: 10pt; margin-bottom: 16px\">");

            foreach (EntPerfilExperiencia x in perfil.Experiencia)
            {
                html.Append("<tr><td style=\"padding: 3px 0\"><b>");
                html.Append(Escapar(x.Cargo));
                html.Append("</b><br /><span style=\"color: #666\">");
                html.Append(Escapar(x.Empresa));
                html.Append("</span>");

                if (x.Funciones != "")
                {
                    html.Append("<br /><span style=\"font-size: 9pt\">");
                    html.Append(Escapar(x.Funciones));
                    html.Append("</span>");
                }

                html.Append("</td><td style=\"width: 110px; text-align: right; color: #666\">");
                html.Append(Escapar(Periodo(x.AnioDesde, x.AnioHasta)));
                html.Append("</td></tr>");
            }

            html.Append("</table>");
        }

        /// <summary>
        /// "2018 - 2021", "2018 - Actual" cuando sigue ahi, o vacio si no se sabe
        /// cuando empezo. Sin anio de fin, un "2018 - " a secas se lee como un
        /// dato cortado.
        /// </summary>
        private static string Periodo(int? desde, int? hasta)
        {
            if (!desde.HasValue) { return ""; }

            return desde.Value.ToString() + " - " +
                   (hasta.HasValue ? hasta.Value.ToString() : "Actual");
        }

        private static void Titulo(StringBuilder html, string texto)
        {
            html.Append("<div style=\"font-size: 12pt; font-weight: bold; color: #750202; ");
            html.Append("border-bottom: 1px solid #ddd; margin-bottom: 6px\">");
            html.Append(Escapar(texto));
            html.Append("</div>");
        }

        /// <summary>
        /// Escapa el texto que va al HTML.
        ///
        /// Se escribe a mano en vez de usar HttpUtility.HtmlEncode porque
        /// CapaNegocio no referencia System.Web y no vale la pena que empiece a
        /// hacerlo por una funcion de cinco reemplazos. Los cinco caracteres son
        /// los que importan: el ampersand va primero, o volveria a escapar lo que
        /// escaparon los demas.
        /// </summary>
        private static string Escapar(string texto)
        {
            if (string.IsNullOrEmpty(texto)) { return ""; }

            return texto
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }
    }
}
```

- [ ] **Step 5: Registrar la clase en `CapaNegocio.csproj`**

Junto a `<Compile Include="NegPerfilCampos.cs" />`, añade:

```xml
    <Compile Include="NegPerfilCv.cs" />
```

- [ ] **Step 6: Correr las pruebas y verificar que pasan**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas/CapaPruebas.csproj -p:Configuration=Debug -v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas/bin/Debug/CapaPruebas.dll
```

Esperado: **104 pruebas, todas pasando** (93 + 11 nuevas). Contrasta con el número que informe `vstest`; si difiere, dilo.

- [ ] **Step 7: Commit**

```bash
git add CapaNegocio/NegPerfilCv.cs CapaNegocio/CapaNegocio.csproj CapaPruebas/NegPerfilCvTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(perfil): el HTML de la hoja de vida, probado"
```

---

### Task 9: El PDF del CV y su descarga

**Files:**
- Create: `ReporteTareas/clases/PdfHojaVida.cs`
- Modify: `ReporteTareas/Formulario/DescargarPerfil.ashx.cs`, `ReporteTareas/Formulario/MiPerfil.aspx`, `ReporteTareas/ReporteTareas.csproj`

**Interfaces:**
- Consumes: `NegPerfilCv.Construir(EntPerfilCompleto) → string` (tarea 8); `NegPerfil.CargarPerfil(string) → EntPerfilCompleto` (fase 1); el handler `DescargarPerfil.ashx` y sus helpers privados `CodUsuarioSesion` y `NoDisponible` (tarea 6).
- Produces: `ReporteTareas.clases.PdfHojaVida.Generar(string html) → byte[]`; la URL `DescargarPerfil.ashx?cv=1`.

**El generador es PdfSharp/HtmlRenderer, nunca Pechkin.** `clases/PDF.cs` usa Pechkin sobre wkhtmltopdf: según sus propios comentarios es una compilación de 32 bits que no carga en un grupo de aplicaciones de 64 bits y **falla una de cada dos veces en el servidor** —tiene un bucle de dos intentos puesto para tapar eso—. `clases/PdfLista.cs` es el precedente correcto y usa las mismas dos referencias que esta tarea.

- [ ] **Step 1: El generador**

Crea `ReporteTareas/clases/PdfHojaVida.cs`:

```csharp
using PdfSharp;
using PdfSharp.Pdf;
using System.IO;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace ReporteTareas.clases
{
    /// <summary>
    /// HTML a PDF, y nada mas. Lo unico de todo el modulo de perfil que sabe de
    /// PdfSharp.
    ///
    /// Se usa PdfSharp + HtmlRenderer y no Pechkin -clases/PDF.cs- porque aquel
    /// es puramente gestionado: sin DLL nativa, sin el problema de 32 contra 64
    /// bits, y sin los fallos intermitentes que obligaron a poner un bucle de
    /// reintentos alrededor del otro. Es el mismo camino que ya usa
    /// clases/PdfLista.cs.
    ///
    /// A cambio, HtmlRenderer entiende un subconjunto chico de CSS: por eso el
    /// HTML que llega aqui va con tablas y estilos en linea.
    /// </summary>
    public static class PdfHojaVida
    {
        /// <summary>
        /// El PDF en memoria. No se escribe en disco a proposito: un CV lleva la
        /// cedula, la fecha de nacimiento y el domicilio de una persona, y un
        /// archivo en una carpeta servida por IIS es un archivo que alguien mas
        /// puede pedir. Ademas no hay nada que limpiar despues.
        /// </summary>
        public static byte[] Generar(string html)
        {
            using (MemoryStream memoria = new MemoryStream())
            {
                PdfDocument documento = PdfGenerator.GeneratePdf(html, PageSize.A4);

                /* false: que no cierre el stream, que lo cierra el using y de el
                   todavia hay que sacar los bytes. */
                documento.Save(memoria, false);

                return memoria.ToArray();
            }
        }
    }
}
```

- [ ] **Step 2: Registrar la clase en el csproj**

En `ReporteTareas/ReporteTareas.csproj`, junto a `<Compile Include="clases\PdfLista.cs" />`, añade:

```xml
    <Compile Include="clases\PdfHojaVida.cs" />
```

- [ ] **Step 3: La rama del CV en el handler**

En `ReporteTareas/Formulario/DescargarPerfil.ashx.cs`, dentro de `ProcessRequest`, entre la comprobación de `codUsuario` y la lectura de `doc`, inserta:

```csharp
            if (context.Request.QueryString["cv"] == "1")
            {
                EntregarCv(context, codUsuario);
                return;
            }
```

Después del método `EntregarDocumento`, añade:

```csharp
        /// <summary>
        /// Genera y entrega el CV de quien lo pide.
        ///
        /// SOLO el propio. No hay parametro que diga de quien es el CV, y no es un
        /// descuido: una jefatura consulta el perfil de su equipo, no se descarga
        /// sus hojas de vida.
        /// </summary>
        private void EntregarCv(HttpContext context, string codUsuario)
        {
            EntPerfilCompleto perfil = NegPerfil.CargarPerfil(codUsuario);

            if (!perfil.PerfilEncontrado)
            {
                NoDisponible(context, "No pudimos identificar su perfil de forma única. " +
                                      "Escriba a Talento Humano para que corrijan su código de usuario.");
                return;
            }

            byte[] pdf = PdfHojaVida.Generar(NegPerfilCv.Construir(perfil));

            /* El codigo de usuario va al nombre del archivo y de ahi a una
               cabecera HTTP: se deja solo lo alfanumerico. */
            string nombre = "CV_" + SoloAlfanumerico(codUsuario) + "_" +
                            DateTime.Now.ToString("yyyyMMdd_HHmmss",
                                System.Globalization.CultureInfo.InvariantCulture) + ".pdf";

            context.Response.Clear();
            context.Response.Buffer = true;
            context.Response.ContentType = "application/pdf";
            context.Response.AddHeader("Content-Disposition", "attachment;filename=" + nombre);
            context.Response.BinaryWrite(pdf);
            context.Response.End();
        }

        private static string SoloAlfanumerico(string texto)
        {
            System.Text.StringBuilder limpio = new System.Text.StringBuilder();

            foreach (char c in texto ?? "")
            {
                if (char.IsLetterOrDigit(c)) { limpio.Append(c); }
            }

            return limpio.Length == 0 ? "perfil" : limpio.ToString();
        }
```

Y añade a los `using` del archivo:

```csharp
using ReporteTareas.clases;
```

- [ ] **Step 4: El botón en la pantalla**

En `ReporteTareas/Formulario/MiPerfil.aspx`, en la barra lateral, después del `<hr />` y del párrafo de la edad, añade:

```aspx
                        <hr />
                        <!-- Enlace y no llamada de JavaScript: la respuesta es un
                             archivo, no JSON, y un <a> con target es lo que el
                             navegador ya sabe manejar. -->
                        <a href="DescargarPerfil.ashx?cv=1" target="_blank"
                           class="btn btn-primary btn-block btn-sm">
                            <i class="fa fa-file-pdf-o"></i> Descargar mi hoja de vida
                        </a>
```

- [ ] **Step 5: Compilar y correr las pruebas**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln -p:Configuration=Debug -v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas/bin/Debug/CapaPruebas.dll
```

Esperado: `0 Error(s)` y **104 pruebas pasando**.

- [ ] **Step 6: Verificar que no se coló Pechkin**

```bash
grep -rn "Pechkin\|wkhtmltopdf\|clases.PDF\b" ReporteTareas/clases/PdfHojaVida.cs ReporteTareas/Formulario/DescargarPerfil.ashx.cs
```

Esperado: **ninguna línea**. Pega la salida (vacía) en el reporte.

- [ ] **Step 7: Compilar en Release**

El despliegue sale de Release y la fase 1 descubrió tarde que compilar sólo en Debug no prueba nada sobre lo que se publica.

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln -p:Configuration=Release -v:minimal
```

Esperado: `0 Error(s)`.

- [ ] **Step 8: Commit**

```bash
git add ReporteTareas/clases/PdfHojaVida.cs ReporteTareas/Formulario/DescargarPerfil.ashx.cs ReporteTareas/Formulario/MiPerfil.aspx ReporteTareas/ReporteTareas.csproj
git commit -m "feat(perfil): la hoja de vida se descarga en PDF"
```

---

### Task 10: Cerrar la fase

**Files:**
- Modify: `docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md`
- Create: `docs/superpowers/plans/2026-09-15-perfil-colaborador-fase3a-verificacion.md`

Esta tarea no toca código. Existe porque tres decisiones de esta fase se apartan de la letra del spec, y un spec que dice una cosa mientras el código hace otra es peor que no tenerlo: el próximo que lo lea va a "arreglar" el código para que le calce.

- [ ] **Step 1: Corregir la afirmación sobre `CargaArchivos.ashx` en el spec**

En `docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md`, en la sección «Acciones del handler», reemplaza:

> Los documentos van por `multipart` reusando `CargaArchivos.ashx`, que es donde ya vive la subida real.

por:

```markdown
Los documentos van por `multipart` en una rama de `AdministrarPerfil.ashx`, no
en `CargaArchivos.ashx`. Este diseño decía lo contrario y la fase 3a lo corrigió:
`CargaArchivos` se declara sin `IRequiresSessionState`, así que ahí
`context.Session` es `null` y la identidad se la manda el cliente en un campo
cifrado del formulario. Todo este módulo descansa en lo contrario. Se reusa el
patrón —`Request.Files`, `SaveAs`, `MapPath`— y no el handler.

Los archivos se guardan en `~/descargas/perfil/` con un nombre que genera el
servidor, esa carpeta lleva un `web.config` con `<handlers><clear /></handlers>`
para que IIS no los sirva directamente, y la descarga pasa por
`DescargarPerfil.ashx`, que le pregunta a la base de quién es el documento antes
de leer el disco. El patrón de la casa —`<a href="../descargas/...">`— sirve para
el adjunto de una tarea, no para la partida de nacimiento de un hijo.
```

- [ ] **Step 2: Corregir la sección «El CV» del spec**

Reemplaza el párrafo que dice *«genera el PDF con PdfSharp/HtmlRenderer en `~/descargas/` con nombre `CV_<Cod_Usuario>_<timestamp>.pdf` y devuelve la ruta»* por:

```markdown
`DescargarPerfil.ashx?cv=1` arma el HTML en el servidor con los mismos datos que
ya carga el perfil, genera el PDF con **PdfSharp/HtmlRenderer** en memoria y lo
escribe directo en la respuesta, con el nombre de descarga
`CV_<Cod_Usuario>_<timestamp>.pdf`. Este diseño decía que se escribiera en
`~/descargas/`; la fase 3a decidió no hacerlo: un CV lleva la cédula, la fecha de
nacimiento y el domicilio de una persona, y un archivo en una carpeta servida por
IIS es un archivo que alguien más puede pedir. El marcado va con tablas y estilos
en línea, como `HtmlSolicitud`, por las limitaciones de CSS del renderizador.

El armado del HTML vive en `CapaNegocio.NegPerfilCv` y el renderizado en
`ReporteTareas.clases.PdfHojaVida`: el corte no es decorativo, es lo que permite
probar el escapado del HTML sin base de datos ni servidor.
```

- [ ] **Step 3: Anotar la foto y marcar la fase**

En la sección «Flujo de carga», donde dice que el procedimiento devuelve **ocho** result sets, cambia a **nueve** y añade `foto` al final de la enumeración.

Y en la sección «Fases», reemplaza el párrafo de la fase 3 por:

```markdown
**Fase 3a — lo visual y lo propio.** [COMPLETA] Foto de perfil, documentos de
respaldo y CV en PDF. La foto entra como noveno result set, por el final, por la
misma razón que las cargas familiares entraron como octavo: el contrato es
posicional y se amplía por el final.

**Fase 3b — lo compartido.** La vista de jefatura. Se separó de la 3a a
propósito: es la única parte del módulo donde una persona ve datos de otra, y
merece su propio ciclo de revisión en vez de compartirlo con un generador de PDF.
La verificación pendiente de este diseño —«que un `codUsuario` que no es
subordinado devuelva vacío»— le corresponde a esa fase.
```

- [ ] **Step 4: Escribir la lista de comprobación manual**

Crea `docs/superpowers/plans/2026-09-15-perfil-colaborador-fase3a-verificacion.md`:

```markdown
# Fase 3a — comprobación manual

Las pruebas automatizadas cubren lo que se puede probar sin base de datos ni
sesión: el escapado del HTML del CV, qué extensión se acepta, qué imagen se
acepta. Todo lo demás —la sesión, el disco, IIS— se comprueba a mano.

El entorno de desarrollo no permite iniciar sesión: el login va por Active
Directory. Esta lista la corre el usuario después de desplegar.

## Antes

- [ ] El script `docs/sql/2026-09-15-perfil-colaborador-fase3a.sql` está aplicado
      en producción, sin `RAISERROR` en la salida.
- [ ] Los binarios de la **fase 2** y los de esta están desplegados. La fase 2
      quedó con el SQL aplicado y los binarios sin publicar.
- [ ] La carpeta `descargas/perfil/` existe en el servidor **con su `web.config`
      dentro**. Si se publicó sin sincronizar y el archivo no viajó, cópialo a
      mano: sin él, los documentos quedan servidos por IIS a quien acierte el
      nombre.

## Foto

- [ ] Subir un JPG grande de un teléfono: la foto aparece redonda en la barra
      lateral y las iniciales desaparecen.
- [ ] Recargar la pantalla: la foto sigue ahí.
- [ ] Subir un PNG: también funciona, y se guarda convertido a JPEG.
- [ ] Elegir **el mismo archivo dos veces seguidas**: la segunda vez también
      responde. (El input se limpia en cada `change` justo por esto.)
- [ ] «Quitar»: vuelven las iniciales.

## Documentos

- [ ] Adjuntar un PDF a una certificación: aparece en su fila, con su nombre.
- [ ] Hacer clic en el nombre: el archivo se descarga y se abre.
- [ ] Adjuntar un JPG a una carga familiar: igual.
- [ ] Dos personas distintas suben un archivo llamado igual: los dos se
      descargan correctos. (El nombre en disco lleva un GUID.)
- [ ] Intentar adjuntar un `.exe` o un `.zip`: lo rechaza con un mensaje.
- [ ] Quitar un documento: desaparece de la fila.
- [ ] **Pedir el documento de otra persona.** Con un `IdDocumento` que no sea
      suyo, abrir `DescargarPerfil.ashx?doc=<ese id>`: tiene que responder
      «No se encontró ese documento», no el archivo.
- [ ] **Pedir el archivo por su ruta directa.** Copiar el nombre en disco de un
      documento —se ve en `Perfil_Documento.NombreArchivoCodigo`— y abrir
      `<sitio>/descargas/perfil/<ese nombre>`: IIS **no** debe entregarlo.

## CV

- [ ] «Descargar mi hoja de vida» con el perfil lleno: el PDF trae nombre,
      cargo, datos, formación, certificaciones y experiencia.
- [ ] Con el perfil vacío —alguien de los 119 sin ficha, sin estudios ni
      experiencia—: el PDF sale igual, con el encabezado y sin secciones vacías.
- [ ] El PDF **no** trae contactos de emergencia ni cargas familiares.
- [ ] Alguien con una experiencia sin año de fin: el período dice «Actual».
- [ ] Generarlo **dos veces seguidas**: las dos funcionan. (Es la prueba de que
      no estamos con Pechkin, que falla una de cada dos.)

## Los casos que dictaron los datos

- [ ] Uno **de los 4 con `Cod_Usuario` repetido**: no puede subir foto ni
      documentos, y el CV le dice que escriba a Talento Humano. No ve datos de
      la otra persona.
- [ ] Uno **de los 119 sin ficha de empleado**: sube foto y documentos con
      normalidad. Nada de esto depende de `Empleados`.
```

- [ ] **Step 5: Commit**

```bash
git add docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md docs/superpowers/plans/2026-09-15-perfil-colaborador-fase3a-verificacion.md
git commit -m "docs(perfil): el spec dice lo que la fase 3a hizo, y la comprobacion manual"
```

---

## Lo que queda para el usuario

Ni el plan ni sus implementadores hacen nada de esto:

1. **Correr el script SQL** contra producción, antes de publicar los binarios.
2. **Publicar con `FolderProfile` (Release) y copiar SIN sincronizar.** Un `robocopy /MIR` borra `connections.config` y `appsettings.config` del servidor y el sitio no arranca.
3. **Commitear el paquete regenerado de `obj/Release/Package/PackageTmp`**, que el repositorio versiona a propósito. Se olvidó en la fase 1 y en la fase 2.
4. **Verificar que `descargas/perfil/web.config` llegó al servidor.** Es lo único de esta fase cuya ausencia no da error y sí deja archivos personales al alcance de cualquiera.
5. **Correr la lista de comprobación manual** de `2026-09-15-perfil-colaborador-fase3a-verificacion.md`.
6. Sigue pendiente de la fase 2: avisarle a Talento Humano que no verá las cargas familiares autogestionadas, y entregarles el reporte de excepciones.
