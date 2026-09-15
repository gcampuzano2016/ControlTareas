# Perfil del colaborador — Fase 3b · Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Que una jefatura consulte —sin editar— lo indispensable de las personas que le reportan, y sólo de ellas.

**Architecture:** Es la única parte del módulo donde alguien accede a datos de otra persona, así que la restricción se construye en dos capas que se refuerzan: la **guarda** vive en el procedimiento almacenado —un `codUsuario` que no es subordinado devuelve cero filas, no una respuesta vacía negociada en C#—, y el **recorte de campos** vive en una entidad nueva que sencillamente no tiene dónde poner una cédula. Lo que no está en la clase no se puede enviar aunque alguien lo intente.

**Tech Stack:** ASP.NET WebForms (.NET Framework 4.8 en el web, 4.6.1 en las capas), SQL Server, jQuery + Bootstrap 3, MSTest v1 (ensamblado de VS2019, sin NuGet), `vstest.console.exe`.

**Spec:** `docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md`

**Fases anteriores:**
- Fase 1 (`2026-09-14-perfil-colaborador-fase1.md`) — completa y desplegada
- Fase 2 (`2026-09-14-perfil-colaborador-fase2.md`) — completa, binarios pendientes de desplegar
- Fase 3a (`2026-09-15-perfil-colaborador-fase3a.md`) — completa, binarios pendientes de desplegar

## Global Constraints

- **Rama:** `ProyectoNuevosCambios`. No commitear ni publicar sin que el usuario lo pida.
- **Compilar con el MSBuild de VS2019**, nunca el del PATH: `"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"`. En Git Bash hace falta `MSYS_NO_PATHCONV=1` y `-p:` en vez de `/p:`.
- **La identidad del JEFE sale de `context.Session["Cod_Usuario"]`. Nunca del payload.** El `codUsuario` del subordinado **sí** viene del cliente: es el único parámetro de todo el módulo que lo hace, y por eso la guarda del procedimiento es obligatoria y no opcional.
- **Todo procedimiento lleva la guarda `@CodigoRepetido`** con un `RETURN;` real. Un `SELECT` de retorno no interrumpe la ejecución en T-SQL.
- **Lo que la jefatura puede ver se decide en el SP, no en la vista.** Un campo que no debe verse no se `SELECT`ea; no se envía y se oculta con CSS.
- **Nada se inserta en la página construyendo HTML con datos.** Todo con `.text()` o `.attr()`. Aquí no es teoría: el nombre y el cargo que se pintan son de **otra persona**, y esta pantalla los muestra a terceros.
- **El script SQL corre antes que los binarios**, es idempotente, y arranca con `SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;` — no los toques.
- **El SQL contra producción lo ejecuta el controlador de la sesión, no el implementador.** Escribe el script, avisa, y commitea después de que te devuelvan la salida.
- **`miPerfil.js` cambia: el `?v=` sube de 6 a 7** en `MiPerfil.aspx`. Lo hace la tarea 5 y **ninguna otra tarea lo toca**.
- Comentarios de código en español **sin tildes**; el texto de cara al usuario sí las lleva, **y habla de usted**. Esta distinción ya causó dos hallazgos en la fase 3a, uno en cada dirección.
- **Commitear con rutas explícitas de archivo.** Nunca `git add` de directorio: el repositorio versiona carpetas `bin/`.
- **El commit termina con `Co-Authored-By: <tu modelo> <noreply@anthropic.com>`**, y su cuerpo explica **por qué**, no qué archivos tocaste.
- Estilo SQL de la casa: constraints con nombre, `DATETIME2(0)` con `SYSDATETIME()`, **sin claves foráneas**.

## Terreno verificado en producción (2026-09-15)

No lo vuelvas a consultar; está medido:

| Dato | Valor |
|---|---|
| Jefes con al menos un subordinado activo que resuelve a un usuario real | **22** |
| Equipo más grande | **49** reportes directos |
| Jefes que son a su vez subordinados de alguien | 22 (los mismos: el árbol está conectado) |
| Personas que son jefe de sí mismas | **0** |
| `EsJefe` en el conjunto 1 de `Sp_RTA_PerfilColaborador` | **ya existe** desde la fase 1 |

`Cod_Jefe_Inm` y `Cod_Usuario` se comparan **siempre** con `LTRIM(RTRIM(...))` en los dos lados: así lo hace el cálculo de `EsJefe` que ya está en producción, porque los valores traen relleno. Una comparación sin recortar devuelve equipos vacíos sin dar ningún error.

## Lo que ya está montado

| Pieza | Estado |
|---|---|
| `EsJefe` en la cabecera del perfil | Ya viaja al navegador en cada `CargarPerfil` |
| `AdministrarPerfil.ashx` | 16 acciones JSON, identidad siempre de la sesión |
| `MiPerfil.aspx` | Seis pestañas, `MostrarMensaje` y el modal funcionando |
| `PostPerfil(action, parameters, onSuccess)` | El cliente JSON del módulo |
| Patrón de buscador | `ParametrizacionHorarioUsuario.aspx:31-40` y `js/parametrizacionHorarioUsuario.js:76` — filtro por servidor, `{filtro: "..."}` |
| Patrón de parámetro de filtro | `@Filtro VARCHAR(100)`, y en C# `filtro ?? string.Empty` (ver `DaoMenuUsuario.ListarUsuarios`) |

## Cuatro decisiones tomadas antes de escribir el plan

**1. Una entidad nueva y recortada, no `EntPerfilCompleto` con campos sin llenar.**

Reusar `EntPerfilCompleto` y limitarse a no llenar `Cedula`, `Contacto` o `CargasFamiliares` sería una restricción que depende de que nadie, nunca, rellene esas propiedades por descuido o por "completitud". `EntPerfilEquipo` no tiene esas propiedades: no hay dónde poner una cédula. Es el mismo argumento con el que el diseño justificó que `EntPerfilContacto` tuviera exactamente cuatro propiedades — «es la lista blanca; un campo que no está acá no se puede guardar, porque no hay dónde ponerlo». Aquí es al revés y por el mismo motivo: no se puede enviar.

La tarea 2 añade además una prueba de reflexión que **falla** el día que alguien le agregue a esa clase una propiedad prohibida. Es la única forma de que esta decisión siga viva dentro de un año.

**2. Los documentos de respaldo NO se muestran a la jefatura.**

La matriz de permisos concede «Formación, certificaciones, experiencia: ve», y no dice nada de los documentos. La regla que encabeza esa misma matriz es «lo que no está en la tabla, no se muestra ni se envía», así que la ausencia decide: no se envían. Y hay una razón concreta: el respaldo de una certificación suele ser un escaneo que lleva la cédula impresa, justo el campo que la matriz le niega a la jefatura. Un documento adjunto sería la puerta trasera de la restricción.

**3. Cero filas es la respuesta, y la da la base.**

`Sp_RTA_PerfilEquipo` recibe el código del jefe **y** el del subordinado, y su `WHERE` exige que el segundo le reporte al primero. Si no, devuelve cero filas en los seis conjuntos. El handler no compara nada: no tiene con qué. Esto importa porque el `codUsuario` del subordinado es el único dato de identidad que este módulo acepta del cliente en toda su superficie.

**4. Sin jerarquía recursiva, y el equipo se calcula al pedirlo.**

Un jefe de jefe no ve a los nietos: el diseño lo pone fuera de alcance y el dato disponible es el jefe inmediato. Y la lista del equipo no se cachea ni viaja en `CargarPerfil`: se pide cuando se abre la pestaña. Con 49 reportes como máximo, la consulta es barata, y así un cambio de jefatura se ve al recargar en vez de quedar pegado en la sesión.

## Estructura de archivos

**Se crean:**

| Archivo | Responsabilidad |
|---|---|
| `docs/sql/2026-09-15-perfil-colaborador-fase3b.sql` | `Sp_RTA_PerfilEquipoLista` y `Sp_RTA_PerfilEquipo` |
| `docs/sql/2026-09-15-prueba-guarda-jefatura.sql` | La demostración de la guarda, dentro de una transacción revertida |
| `CapaEntidad/EntPerfilEquipoItem.cs` | Una fila de la lista del equipo |
| `CapaEntidad/EntPerfilEquipoCabecera.cs` | La cabecera recortada de un subordinado |
| `CapaEntidad/EntPerfilEquipo.cs` | Lo que devuelve consultar a un subordinado |
| `CapaPruebas/EntPerfilEquipoTests.cs` | La prueba de reflexión que prohíbe los campos vetados |

**Se modifican:**

| Archivo | Cambio |
|---|---|
| `CapaEntidad/CapaEntidad.csproj` | Los tres `Compile Include` nuevos |
| `CapaDato/DaoPerfil.cs` | `ListaEquipo` y `PerfilEquipo` |
| `CapaNegocio/NegPerfil.cs` | Fachada de los dos |
| `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs` | Acciones `ListaEquipo` y `PerfilEquipo` |
| `ReporteTareas/Formulario/MiPerfil.aspx` | Séptima pestaña, buscador, lista, panel de detalle, `?v=7` |
| `ReporteTareas/js/miPerfil.js` | Pintado del equipo y del subordinado |
| `CapaPruebas/CapaPruebas.csproj` | `EntPerfilEquipoTests.cs` |
| `docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md` | Fase 3b marcada completa, verificación al día |

---

### Task 1: El script SQL de la fase 3b

**Files:**
- Create: `docs/sql/2026-09-15-perfil-colaborador-fase3b.sql`
- Referencia de estilo: `docs/sql/2026-09-15-perfil-colaborador-fase3a.sql`

**Interfaces:**
- Produces: `Sp_RTA_PerfilEquipoLista` (`@Cod_Jefe VARCHAR(50)`, `@Filtro VARCHAR(100)`) → una tabla con `CodUsuario`, `NombreCompleto`, `Cargo`, `Area`, `Ciudad`; y `Sp_RTA_PerfilEquipo` (`@Cod_Jefe VARCHAR(50)`, `@Cod_Usuario VARCHAR(50)`) → **seis** result sets en este orden: cabecera recortada, contactos de emergencia, estudios, certificaciones, experiencia, foto.

**No ejecutes este script.** Cuando esté escrito y commiteado, dilo en el reporte: lo corre el controlador de la sesión.

- [ ] **Step 1: La cabecera del script**

Crea `docs/sql/2026-09-15-perfil-colaborador-fase3b.sql`:

```sql
/* ============================================================================
   Perfil del colaborador - FASE 3b
   La vista de jefatura: una jefatura consulta -sin editar- lo indispensable
   de las personas que le reportan.

   Contenido:
     1. Sp_RTA_PerfilEquipoLista
     2. Sp_RTA_PerfilEquipo
     3. Aserciones

   Esta es la unica parte del modulo donde alguien lee datos de otra persona.
   La guarda vive aqui y no en C#: los dos procedimientos exigen que el
   subordinado le reporte al jefe, y devuelven cero filas si no.

   No crea ni altera ninguna tabla.
   Idempotente: se puede correr dos veces sin dano.
   ============================================================================ */

SET NOCOUNT ON;
GO
/* QUOTED_IDENTIFIER y ANSI_NULLS explicitos y dentro del script: DESPLIEGUE.md
   dice que esto lo corre una persona a mano y sqlcmd los deja apagados por
   omision. En la fase 1 eso aborto el script a media ejecucion (error 1934). */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
```

- [ ] **Step 2: `Sp_RTA_PerfilEquipoLista`**

Añade al script:

```sql
/* ----------------------------------------- 1. Sp_RTA_PerfilEquipoLista --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilEquipoLista','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEquipoLista;
GO

/* Quien le reporta directamente a esta persona. Sin recursion: un jefe de jefe
   no ve a los nietos, que es lo que dice el diseno y lo que permite el dato
   disponible -Cod_Jefe_Inm es el jefe inmediato y nada mas-.

   El equipo mas grande de la empresa es de 49 personas, asi que no hay
   paginacion: sobra con filtrar por texto en el servidor. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilEquipoLista
    @Cod_Jefe VARCHAR(50),
    @Filtro   VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;
    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Jefe AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    DECLARE @F VARCHAR(100) = LTRIM(RTRIM(ISNULL(@Filtro, '')));

    /* LTRIM(RTRIM(...)) en los DOS lados de la comparacion, siempre. Los
       valores de Cod_Jefe_Inm traen relleno y una comparacion cruda devuelve
       equipos vacios sin dar ningun error. Es la misma forma que usa el
       calculo de EsJefe que ya esta en produccion. */
    SELECT  CodUsuario     = LTRIM(RTRIM(u.Cod_Usuario)),
            NombreCompleto = ISNULL(NULLIF(LTRIM(RTRIM(e.Nombre)), ''), u.Nom_Usuario),
            Cargo          = ISNULL(NULLIF(LTRIM(RTRIM(e.PuestoTrabajo)), ''), u.Cargo),
            Area           = ISNULL(NULLIF(LTRIM(RTRIM(e.AreaTrabajo)), ''), u.Departamento),
            Ciudad         = e.Ciudad
      FROM  dbo.R_Usuarios u
      LEFT JOIN dbo.Empleados e ON e.Cod_Usuario = u.Cod_Usuario
     WHERE  LTRIM(RTRIM(u.Cod_Jefe_Inm)) = LTRIM(RTRIM(@Cod_Jefe))
       AND  ISNULL(u.EstadoUsuario, 0) = 0
       AND  @CodigoRepetido = 0
       AND  (@F = ''
             OR ISNULL(NULLIF(LTRIM(RTRIM(e.Nombre)), ''), u.Nom_Usuario) LIKE '%' + @F + '%'
             OR LTRIM(RTRIM(u.Cod_Usuario)) LIKE '%' + @F + '%'
             OR ISNULL(NULLIF(LTRIM(RTRIM(e.PuestoTrabajo)), ''), u.Cargo) LIKE '%' + @F + '%')
     ORDER BY NombreCompleto;
END
GO
PRINT 'Sp_RTA_PerfilEquipoLista creado.';
GO
```

- [ ] **Step 3: `Sp_RTA_PerfilEquipo`**

Añade al script:

```sql
/* ------------------------------------------- 2. Fn_RTA_EsSubordinado --- */

IF OBJECT_ID('dbo.Fn_RTA_EsSubordinado','FN') IS NOT NULL
    DROP FUNCTION dbo.Fn_RTA_EsSubordinado;
GO

/* La guarda, en una funcion y no escrita a mano dentro del procedimiento.

   El motivo es que se pueda PROBAR. La verificacion numero 3 del diseno -"que
   un codUsuario que no es subordinado devuelva vacio"- no se puede cubrir con
   una prueba unitaria, y contra la base tampoco se puede capturar la salida de
   Sp_RTA_PerfilEquipo con INSERT ... EXEC, porque devuelve seis result sets de
   formas distintas y ese INSERT intenta meterlos todos en la misma tabla.

   Sacando la condicion aqui, la demostracion llama exactamente a la MISMA
   funcion que usa el procedimiento, en vez de a una copia de su logica que
   podria divergir sin que nadie se entere. Una prueba que verifica un duplicado
   del codigo no verifica el codigo.

   LTRIM(RTRIM(...)) en los dos lados: Cod_Jefe_Inm trae relleno. */
CREATE FUNCTION dbo.Fn_RTA_EsSubordinado
(
    @Cod_Jefe    VARCHAR(50),
    @Cod_Usuario VARCHAR(50)
)
RETURNS BIT
AS
BEGIN
    DECLARE @EsSubordinado BIT = 0;

    IF EXISTS (SELECT 1 FROM dbo.R_Usuarios s
                WHERE LTRIM(RTRIM(s.Cod_Usuario))  = LTRIM(RTRIM(@Cod_Usuario))
                  AND LTRIM(RTRIM(s.Cod_Jefe_Inm)) = LTRIM(RTRIM(@Cod_Jefe))
                  AND ISNULL(s.EstadoUsuario, 0) = 0)
        SET @EsSubordinado = 1;

    RETURN @EsSubordinado;
END
GO
PRINT 'Fn_RTA_EsSubordinado creada.';
GO

/* ---------------------------------------------- 3. Sp_RTA_PerfilEquipo --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilEquipo','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilEquipo;
GO

/* Lo que una jefatura puede ver de alguien de su equipo. SEIS result sets, en
   este orden: cabecera recortada, contactos de emergencia, estudios,
   certificaciones, experiencia, foto. El Dao los recorre POR POSICION, igual
   que en Sp_RTA_PerfilColaborador: si algun dia hay que anadir algo, va al
   final y no en medio.

   Lo que NO devuelve, a proposito, porque la matriz de permisos del diseno no
   se lo concede a la jefatura: cedula, fecha de nacimiento, edad, domicilio,
   correo y telefono personales, estado civil, cargas familiares y documentos
   de respaldo.

   Los documentos merecen su propia frase: la matriz concede certificaciones
   pero calla sobre sus respaldos, y el respaldo de una certificacion suele ser
   un escaneo con la cedula impresa -justo el campo que la matriz niega-. Un
   adjunto seria la puerta trasera de la restriccion.

   LA GUARDA. @EsSubordinado decide todo: si esa persona no le reporta a este
   jefe, los seis SELECT devuelven cero filas. No hay RETURN anticipado a
   proposito, porque el Dao recorre por posicion y espera que los seis
   conjuntos vengan siempre, aunque vacios; un RETURN rompe ese contrato. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilEquipo
    @Cod_Jefe    VARCHAR(50),
    @Cod_Usuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CodigoRepetido BIT = 0;
    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Jefe AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    /* El codigo del subordinado tambien puede estar repetido: en ese caso no se
       sabe de quien serian los datos y tampoco se entrega nada. */
    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario,0) = 0) > 1
        SET @CodigoRepetido = 1;

    /* La condicion vive en Fn_RTA_EsSubordinado y no aqui, para que la
       demostracion en SQL pueda llamar a la misma funcion en vez de a una copia
       de su logica. */
    DECLARE @EsSubordinado BIT = 0;

    IF @CodigoRepetido = 0
        SET @EsSubordinado = dbo.Fn_RTA_EsSubordinado(@Cod_Jefe, @Cod_Usuario);

    /* 1. cabecera recortada */
    SELECT  CodUsuario     = LTRIM(RTRIM(u.Cod_Usuario)),
            NombreCompleto = ISNULL(NULLIF(LTRIM(RTRIM(e.Nombre)), ''), u.Nom_Usuario),
            Cargo          = ISNULL(NULLIF(LTRIM(RTRIM(e.PuestoTrabajo)), ''), u.Cargo),
            Area           = ISNULL(NULLIF(LTRIM(RTRIM(e.AreaTrabajo)), ''), u.Departamento),
            Ciudad         = e.Ciudad,
            CorreoNotificacion = u.E_Mail,
            JefeInmediato  = j.Nom_Usuario,

            /* Subconsulta y no LEFT JOIN, por la misma razon que en
               Sp_RTA_PerfilColaborador: hay 5 usuarios con mas de una
               asignacion activa y un join los duplicaria, cuando este SELECT
               tiene que devolver exactamente una fila. */
            Horario = (SELECT TOP 1 h.Nombre
                         FROM dbo.R_UsuarioHorarioLaboral uh
                         JOIN dbo.R_HorarioLaboral h
                              ON h.IdHorarioLaboral = uh.IdHorarioLaboral
                        WHERE uh.Id_Responsable = u.Cod_Usuario
                          AND uh.Activo = 1
                        ORDER BY uh.FechaDesde DESC, uh.IdUsuarioHorario DESC)
      FROM  dbo.R_Usuarios u
      LEFT JOIN dbo.Empleados  e ON e.Cod_Usuario = u.Cod_Usuario
      LEFT JOIN dbo.R_Usuarios j ON LTRIM(RTRIM(j.Cod_Usuario)) = LTRIM(RTRIM(u.Cod_Jefe_Inm))
     WHERE  LTRIM(RTRIM(u.Cod_Usuario)) = LTRIM(RTRIM(@Cod_Usuario))
       AND  @EsSubordinado = 1;

    /* 2. contactos de emergencia */
    SELECT IdContacto, Nombre, Parentesco, Telefono
      FROM dbo.Perfil_ContactoEmergencia
     WHERE Cod_Usuario = @Cod_Usuario
       AND Estado = '1'
       AND @EsSubordinado = 1
     ORDER BY IdContacto;

    /* 3. estudios */
    SELECT IdEstudio, Nivel, Institucion, Titulo, AnioGraduacion
      FROM dbo.Perfil_Estudio
     WHERE Cod_Usuario = @Cod_Usuario
       AND Estado = '1'
       AND @EsSubordinado = 1
     ORDER BY ISNULL(AnioGraduacion, 0) DESC, IdEstudio;

    /* 4. certificaciones */
    SELECT IdCertificacion, Nombre, Entidad, FechaObtencion
      FROM dbo.Perfil_Certificacion
     WHERE Cod_Usuario = @Cod_Usuario
       AND Estado = '1'
       AND @EsSubordinado = 1
     ORDER BY FechaObtencion DESC, IdCertificacion;

    /* 5. experiencia */
    SELECT IdExperiencia, Empresa, Cargo, AnioDesde, AnioHasta, Funciones
      FROM dbo.Perfil_Experiencia
     WHERE Cod_Usuario = @Cod_Usuario
       AND Estado = '1'
       AND @EsSubordinado = 1
     ORDER BY ISNULL(AnioHasta, 9999) DESC, AnioDesde DESC;

    /* 6. foto */
    SELECT FotoBase64, FotoTipo
      FROM dbo.Perfil_Foto
     WHERE Cod_Usuario = @Cod_Usuario
       AND @EsSubordinado = 1;
END
GO
PRINT 'Sp_RTA_PerfilEquipo creado.';
GO
```

- [ ] **Step 4: Las aserciones**

Añade al final del script:

```sql
/* ------------------------------------------------------- 4. aserciones --- */

IF OBJECT_ID('dbo.Sp_RTA_PerfilEquipoLista','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilEquipoLista no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Sp_RTA_PerfilEquipo','P') IS NULL
    RAISERROR('FALLO: Sp_RTA_PerfilEquipo no quedo creado.', 16, 1);
IF OBJECT_ID('dbo.Fn_RTA_EsSubordinado','FN') IS NULL
    RAISERROR('FALLO: Fn_RTA_EsSubordinado no quedo creada.', 16, 1);

/* La guarda tiene que estar en el texto del procedimiento. Se apunta a que
   LLAME a la funcion Y a que su resultado filtre los SELECT: si alguien
   quitara la condicion de los SELECT dejando la llamada, un LIKE contra el
   nombre de la funcion solo seguiria pasando. */
IF NOT EXISTS (SELECT 1 FROM sys.sql_modules m
                JOIN sys.procedures p ON p.object_id = m.object_id
               WHERE p.name = 'Sp_RTA_PerfilEquipo'
                 AND m.definition LIKE '%Fn_RTA_EsSubordinado%'
                 AND m.definition LIKE '%AND @EsSubordinado = 1%')
    RAISERROR('FALLO: Sp_RTA_PerfilEquipo no tiene la guarda de jefatura.', 16, 1);

/* La guarda tiene que negar de verdad, no solo existir. Dos casos construidos
   con datos reales: alguien contra su propio jefe da 1, y contra un jefe que
   no es el suyo da 0. Si estas dos no se cumplen, nada de lo de arriba importa. */
DECLARE @J VARCHAR(50), @S VARCHAR(50);

SELECT TOP 1 @J = LTRIM(RTRIM(s.Cod_Jefe_Inm)), @S = LTRIM(RTRIM(s.Cod_Usuario))
  FROM dbo.R_Usuarios s
  JOIN dbo.R_Usuarios u ON LTRIM(RTRIM(u.Cod_Usuario)) = LTRIM(RTRIM(s.Cod_Jefe_Inm))
                       AND ISNULL(u.EstadoUsuario,0) = 0
 WHERE ISNULL(s.EstadoUsuario,0) = 0
 ORDER BY s.Cod_Usuario;

IF @J IS NOT NULL
BEGIN
    IF dbo.Fn_RTA_EsSubordinado(@J, @S) <> 1
        RAISERROR('FALLO: la guarda no reconoce a un subordinado real.', 16, 1);

    /* Un codigo que no existe, y no la relacion invertida: invertirla dependeria
       de que no haya ciclos de dos en el arbol de jefaturas, y eso no esta
       comprobado. Esto si es determinista. La demostracion completa
       -prueba-guarda-jefatura.sql- hace la version fuerte, con una persona real
       que no es del equipo. */
    IF dbo.Fn_RTA_EsSubordinado(@J, 'ZZ-NO-EXISTE-ZZ') <> 0
        RAISERROR('FALLO: la guarda acepta a alguien que no existe.', 16, 1);
END

/* Los seis conjuntos: si faltara uno, el Dao -que recorre por posicion- leeria
   la lista equivocada sin quejarse. */
IF NOT EXISTS (SELECT 1 FROM sys.sql_modules m
                JOIN sys.procedures p ON p.object_id = m.object_id
               WHERE p.name = 'Sp_RTA_PerfilEquipo'
                 AND m.definition LIKE '%FROM dbo.Perfil_ContactoEmergencia%'
                 AND m.definition LIKE '%FROM dbo.Perfil_Estudio%'
                 AND m.definition LIKE '%FROM dbo.Perfil_Certificacion%'
                 AND m.definition LIKE '%FROM dbo.Perfil_Experiencia%'
                 AND m.definition LIKE '%FROM dbo.Perfil_Foto%')
    RAISERROR('FALLO: a Sp_RTA_PerfilEquipo le falta alguno de los seis conjuntos.', 16, 1);

/* Lo que NO puede aparecer. La matriz de permisos niega estos campos a la
   jefatura, y esta asercion es lo que impide que vuelvan por descuido en una
   edicion futura del procedimiento. */
IF EXISTS (SELECT 1 FROM sys.sql_modules m
            JOIN sys.procedures p ON p.object_id = m.object_id
           WHERE p.name = 'Sp_RTA_PerfilEquipo'
             AND (m.definition LIKE '%Perfil_ContactoPersonal%'
                  OR m.definition LIKE '%Emp_CargaFamiliar%'
                  OR m.definition LIKE '%Perfil_Documento%'
                  OR m.definition LIKE '%Fecha_nacimiento%'))
    RAISERROR('FALLO: Sp_RTA_PerfilEquipo toca datos que la matriz de permisos le niega a la jefatura.', 16, 1);

PRINT 'Fase 3b: script terminado.';
GO
```

- [ ] **Step 5: Commit**

```bash
git add docs/sql/2026-09-15-perfil-colaborador-fase3b.sql
git commit -m "feat(perfil): script SQL de la fase 3b - la vista de jefatura y su guarda"
```

- [ ] **Step 6: Avisar**

En el reporte, escribe textualmente: **«El script `docs/sql/2026-09-15-perfil-colaborador-fase3b.sql` está listo y commiteado. No lo ejecuté. Hace falta correrlo contra producción antes de la tarea 3.»**

---

### Task 2: La entidad recortada, y la prueba que la mantiene recortada

**Files:**
- Create: `CapaEntidad/EntPerfilEquipoItem.cs`, `CapaEntidad/EntPerfilEquipoCabecera.cs`, `CapaEntidad/EntPerfilEquipo.cs`, `CapaPruebas/EntPerfilEquipoTests.cs`
- Modify: `CapaEntidad/CapaEntidad.csproj`, `CapaPruebas/CapaPruebas.csproj`

**Interfaces:**
- Consumes: `EntPerfilEmergencia`, `EntPerfilEstudio`, `EntPerfilCertificacion`, `EntPerfilExperiencia`, `EntPerfilFoto` — todas ya existen en `CapaEntidad` y se reutilizan tal cual.
- Produces: `EntPerfilEquipoItem` (`CodUsuario`, `NombreCompleto`, `Cargo`, `Area`, `Ciudad`, todos `string`); `EntPerfilEquipoCabecera` (`CodUsuario`, `NombreCompleto`, `Cargo`, `Area`, `Ciudad`, `CorreoNotificacion`, `JefeInmediato`, `Horario`, todos `string`); `EntPerfilEquipo` con `Cabecera` (`EntPerfilEquipoCabecera`), `Emergencia`, `Estudios`, `Certificaciones`, `Experiencia` (listas) , `Foto` (`EntPerfilFoto`) y `bool PerfilEncontrado`.

Esta tarea es **TDD al revés de lo habitual**: la prueba no describe lo que la clase hace, sino lo que la clase **no puede llegar a tener**. Escríbela primero igual.

- [ ] **Step 1: Escribir la prueba que falla**

Crea `CapaPruebas/EntPerfilEquipoTests.cs`:

```csharp
using CapaEntidad;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reflection;

namespace CapaPruebas
{
    /// <summary>
    /// La restriccion de la vista de jefatura, hecha prueba.
    ///
    /// La matriz de permisos del diseno le niega a la jefatura la cedula, la
    /// fecha de nacimiento, la edad, el domicilio, el correo y el telefono
    /// personales, el estado civil, las cargas familiares y los documentos de
    /// respaldo. El procedimiento almacenado no los devuelve, pero eso solo se
    /// puede comprobar contra una base de datos. Lo que si se puede comprobar
    /// aqui, sin base y en cada compilacion, es que la clase que viaja al
    /// navegador NO TIENE DONDE PONERLOS.
    ///
    /// Si alguien le agrega manana una propiedad Cedula a EntPerfilEquipoCabecera
    /// -por simetria con EntPerfilCabecera, que es lo que va a parecer razonable-,
    /// esta prueba se pone roja antes de que ese campo llegue a la pantalla de
    /// nadie. Es el unico guardian de esta decision que sobrevive a que todos
    /// olvidemos por que se tomo.
    /// </summary>
    [TestClass]
    public class EntPerfilEquipoTests
    {
        /// <summary>
        /// Nombres de propiedad prohibidos, en minusculas. Se compara por nombre
        /// y no por tipo a proposito: el riesgo es que alguien reproduzca el
        /// campo, se llame como se llame su tipo.
        /// </summary>
        private static readonly string[] Prohibidos =
        {
            "cedula", "fechanactexto", "fechanacimiento", "edad",
            "direccion", "correopersonal", "telefonopersonal", "estadocivil",
            "cargasfamiliares", "documentos"
        };

        private static void NoDebeTenerCamposProhibidos(System.Type tipo)
        {
            foreach (PropertyInfo p in tipo.GetProperties())
            {
                string nombre = p.Name.ToLowerInvariant();

                Assert.IsFalse(Prohibidos.Contains(nombre),
                    "La clase " + tipo.Name + " tiene la propiedad '" + p.Name +
                    "', que la matriz de permisos le niega a la jefatura. " +
                    "Si de verdad hace falta, hay que cambiar el diseno primero, " +
                    "no la clase.");
            }
        }

        [TestMethod]
        public void EntPerfilEquipoCabecera_NoTieneCamposQueLaJefaturaNoPuedeVer()
        {
            NoDebeTenerCamposProhibidos(typeof(EntPerfilEquipoCabecera));
        }

        [TestMethod]
        public void EntPerfilEquipo_NoTieneCamposQueLaJefaturaNoPuedeVer()
        {
            NoDebeTenerCamposProhibidos(typeof(EntPerfilEquipo));
        }

        [TestMethod]
        public void EntPerfilEquipoItem_NoTieneCamposQueLaJefaturaNoPuedeVer()
        {
            NoDebeTenerCamposProhibidos(typeof(EntPerfilEquipoItem));
        }

        /// <summary>
        /// La cabecera de la jefatura tiene que ser estrictamente mas pobre que
        /// la del dueno del perfil. Si algun dia tuvieran las mismas propiedades,
        /// es que alguien reuso la clase equivocada.
        /// </summary>
        [TestMethod]
        public void LaCabeceraDeJefaturaTieneMenosCamposQueLaDelDueno()
        {
            int jefatura = typeof(EntPerfilEquipoCabecera).GetProperties().Length;
            int dueno = typeof(EntPerfilCabecera).GetProperties().Length;

            Assert.IsTrue(jefatura < dueno,
                "EntPerfilEquipoCabecera tiene " + jefatura + " propiedades y " +
                "EntPerfilCabecera tiene " + dueno + ". La de jefatura debe ser " +
                "estrictamente mas pobre.");
        }

        /// <summary>
        /// Las listas se inicializan en el constructor: si llegaran nulas, el
        /// serializador las manda como null y el JavaScript revienta al recorrer
        /// -y solo para quien todavia no registro nada, que el primer dia es
        /// todo el mundo-.
        /// </summary>
        [TestMethod]
        public void EntPerfilEquipo_NaceConSusListasYSuCabecera()
        {
            EntPerfilEquipo equipo = new EntPerfilEquipo();

            Assert.IsNotNull(equipo.Cabecera);
            Assert.IsNotNull(equipo.Emergencia);
            Assert.IsNotNull(equipo.Estudios);
            Assert.IsNotNull(equipo.Certificaciones);
            Assert.IsNotNull(equipo.Experiencia);
            Assert.IsNotNull(equipo.Foto);
            Assert.IsFalse(equipo.PerfilEncontrado);
        }
    }
}
```

- [ ] **Step 2: Registrar la prueba en `CapaPruebas.csproj`**

Junto a `<Compile Include="NegPerfilCvTests.cs" />`, añade:

```xml
    <Compile Include="EntPerfilEquipoTests.cs" />
```

- [ ] **Step 3: Correr y verificar que NO compila**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas/CapaPruebas.csproj -p:Configuration=Debug -v:minimal
```

Esperado: **no compila**, con `El nombre del tipo o del espacio de nombres 'EntPerfilEquipoCabecera' no existe`.

- [ ] **Step 4: Crear `EntPerfilEquipoItem`**

Crea `CapaEntidad/EntPerfilEquipoItem.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>
    /// Una fila de la lista del equipo de una jefatura. Lo justo para
    /// reconocer a alguien y decidir si abrir su ficha.
    /// </summary>
    public class EntPerfilEquipoItem
    {
        public string CodUsuario { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string Cargo { get; set; } = "";
        public string Area { get; set; } = "";
        public string Ciudad { get; set; } = "";
    }
}
```

- [ ] **Step 5: Crear `EntPerfilEquipoCabecera`**

Crea `CapaEntidad/EntPerfilEquipoCabecera.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>
    /// La cabecera de un subordinado, tal como la ve su jefatura.
    ///
    /// Es deliberadamente mas pobre que EntPerfilCabecera y NO debe converger
    /// con ella. Le faltan, porque la matriz de permisos del diseno se los
    /// niega a la jefatura: Cedula, FechaNacTexto y Edad. No es que no se
    /// llenen: es que no existen, y por eso no se pueden enviar por descuido.
    ///
    /// EntPerfilEquipoTests comprueba esto en cada compilacion. Si vas a
    /// agregar una propiedad aqui, esa prueba te va a parar: es lo que tiene
    /// que pasar. Cambia el diseno primero.
    /// </summary>
    public class EntPerfilEquipoCabecera
    {
        public string CodUsuario { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string Cargo { get; set; } = "";
        public string Area { get; set; } = "";
        public string Ciudad { get; set; } = "";
        public string CorreoNotificacion { get; set; } = "";
        public string JefeInmediato { get; set; } = "";

        /// <summary>Nombre del horario vigente, o vacio si no tiene ninguno.</summary>
        public string Horario { get; set; } = "";
    }
}
```

- [ ] **Step 6: Crear `EntPerfilEquipo`**

Crea `CapaEntidad/EntPerfilEquipo.cs`:

```csharp
using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>
    /// Lo que devuelve una sola llamada a Sp_RTA_PerfilEquipo: seis result sets
    /// en este orden -cabecera, emergencia, estudios, certificaciones,
    /// experiencia, foto-.
    ///
    /// Las listas reusan las entidades del perfil propio porque son el mismo
    /// dato. La cabecera NO: esa es recortada y tiene su propia clase, que es
    /// donde vive la restriccion. Ver EntPerfilEquipoCabecera.
    /// </summary>
    public class EntPerfilEquipo
    {
        public EntPerfilEquipoCabecera Cabecera { get; set; }
        public List<EntPerfilEmergencia> Emergencia { get; set; }
        public List<EntPerfilEstudio> Estudios { get; set; }
        public List<EntPerfilCertificacion> Certificaciones { get; set; }
        public List<EntPerfilExperiencia> Experiencia { get; set; }
        public EntPerfilFoto Foto { get; set; }

        /// <summary>
        /// false cuando el procedimiento no devolvio cabecera. Son dos casos y
        /// se tratan igual a proposito: que esa persona no le reporte a quien
        /// pregunta, o que alguno de los dos codigos de usuario este repetido.
        /// Distinguirlos en la respuesta le confirmaria a quien esta probando
        /// codigos cual existe.
        /// </summary>
        public bool PerfilEncontrado { get; set; }

        public EntPerfilEquipo()
        {
            Cabecera = new EntPerfilEquipoCabecera();
            Emergencia = new List<EntPerfilEmergencia>();
            Estudios = new List<EntPerfilEstudio>();
            Certificaciones = new List<EntPerfilCertificacion>();
            Experiencia = new List<EntPerfilExperiencia>();
            Foto = new EntPerfilFoto();
        }
    }
}
```

- [ ] **Step 7: Registrar las tres clases en `CapaEntidad.csproj`**

Junto a los demás `EntPerfil*`, añade:

```xml
    <Compile Include="EntPerfilEquipo.cs" />
    <Compile Include="EntPerfilEquipoCabecera.cs" />
    <Compile Include="EntPerfilEquipoItem.cs" />
```

- [ ] **Step 8: Correr las pruebas y verificar que pasan**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas/CapaPruebas.csproj -p:Configuration=Debug -v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas/bin/Debug/CapaPruebas.dll
```

Esperado: **109 pruebas, todas pasando** (104 previas + 5 nuevas). Contrasta con lo que informe `vstest`; si difiere, dilo en vez de ajustar el número.

- [ ] **Step 9: Demostrar que la prueba discrimina**

Una prueba de reflexión que no se ha visto fallar no vale nada. Hazlo y pega las dos salidas:

1. Añade temporalmente `public string Cedula { get; set; } = "";` a `EntPerfilEquipoCabecera`.
2. Compila y corre **sólo** `EntPerfilEquipoCabecera_NoTieneCamposQueLaJefaturaNoPuedeVer`. Debe **FALLAR**, y el mensaje debe nombrar la propiedad.
3. Quita esa línea, compila y corre las 109. Deben pasar.

Para correr una sola: añade `/Tests:EntPerfilEquipoCabecera_NoTieneCamposQueLaJefaturaNoPuedeVer` al comando de `vstest.console.exe`.

Si en el paso 2 la prueba pasa, el guardián no guarda nada: avísame en vez de seguir.

- [ ] **Step 10: Commit**

```bash
git add CapaEntidad/EntPerfilEquipo.cs CapaEntidad/EntPerfilEquipoCabecera.cs CapaEntidad/EntPerfilEquipoItem.cs CapaEntidad/CapaEntidad.csproj CapaPruebas/EntPerfilEquipoTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(perfil): la entidad de la vista de jefatura, y la prueba que la mantiene recortada"
```

---

### Task 3: La capa de datos y la fachada

**Files:**
- Modify: `CapaDato/DaoPerfil.cs`, `CapaNegocio/NegPerfil.cs`

**Interfaces:**
- Consumes: `Sp_RTA_PerfilEquipoLista` y `Sp_RTA_PerfilEquipo` (tarea 1); `EntPerfilEquipoItem`, `EntPerfilEquipoCabecera`, `EntPerfilEquipo` (tarea 2); los helpers privados ya existentes de `DaoPerfil`: `Texto`, `EnteroDe`, `EnteroNuloDe`, `FechaMesDe`.
- Produces: `DaoPerfil.ListaEquipo(string codJefe, string filtro) → List<EntPerfilEquipoItem>`; `DaoPerfil.PerfilEquipo(string codJefe, string codUsuario) → EntPerfilEquipo`; y los dos iguales en `NegPerfil`.

**Requisito previo:** el script de la tarea 1 tiene que estar aplicado en producción. Si no lo está, di **BLOCKED** y no sigas.

- [ ] **Step 1: `ListaEquipo` en el Dao**

En `CapaDato/DaoPerfil.cs`, después de `ObtenerDocumento` y antes de los helpers privados, añade:

```csharp
        /// <summary>
        /// El equipo directo de una jefatura, filtrado por texto en el servidor.
        ///
        /// Sin paginacion a proposito: el equipo mas grande de la empresa es de
        /// 49 personas y una lista de ese tamanio no necesita paginarse. Si algun
        /// dia deja de ser cierto, se vera en esta consulta antes que en ningun
        /// otro sitio.
        /// </summary>
        public static List<EntPerfilEquipoItem> ListaEquipo(string codJefe, string filtro)
        {
            List<EntPerfilEquipoItem> lista = new List<EntPerfilEquipoItem>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilEquipoLista", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Jefe", SqlDbType.VarChar,  50).Value = codJefe;
                cmd.Parameters.Add("@Filtro",   SqlDbType.VarChar, 100).Value = filtro ?? string.Empty;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntPerfilEquipoItem
                        {
                            CodUsuario     = Texto(dr, "CodUsuario"),
                            NombreCompleto = Texto(dr, "NombreCompleto"),
                            Cargo          = Texto(dr, "Cargo"),
                            Area           = Texto(dr, "Area"),
                            Ciudad         = Texto(dr, "Ciudad")
                        });
                    }
                }
            }

            return lista;
        }
```

Añade `using System.Collections.Generic;` a los `using` del archivo si no está.

- [ ] **Step 2: `PerfilEquipo` en el Dao**

Justo después, añade:

```csharp
        /// <summary>
        /// El perfil de un subordinado, tal como lo ve su jefatura.
        ///
        /// Recorre SEIS result sets por posicion, igual que CargarPerfil. El
        /// procedimiento devuelve los seis siempre, aunque vacios: por eso aqui
        /// no hay ningun atajo que se salte los restantes cuando la cabecera
        /// viene sin filas.
        ///
        /// Cero filas en la cabecera NO es un error que haya que distinguir:
        /// significa que esa persona no le reporta a quien pregunta, o que
        /// alguno de los dos codigos esta repetido. Los dos casos salen igual
        /// -PerfilEncontrado en false- a proposito.
        /// </summary>
        public static EntPerfilEquipo PerfilEquipo(string codJefe, string codUsuario)
        {
            EntPerfilEquipo perfil = new EntPerfilEquipo();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilEquipo", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Jefe",    SqlDbType.VarChar, 50).Value = codJefe;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    /* 1. cabecera recortada */
                    if (dr.Read())
                    {
                        perfil.PerfilEncontrado            = true;
                        perfil.Cabecera.CodUsuario         = Texto(dr, "CodUsuario");
                        perfil.Cabecera.NombreCompleto     = Texto(dr, "NombreCompleto");
                        perfil.Cabecera.Cargo              = Texto(dr, "Cargo");
                        perfil.Cabecera.Area               = Texto(dr, "Area");
                        perfil.Cabecera.Ciudad             = Texto(dr, "Ciudad");
                        perfil.Cabecera.CorreoNotificacion = Texto(dr, "CorreoNotificacion");
                        perfil.Cabecera.JefeInmediato      = Texto(dr, "JefeInmediato");
                        perfil.Cabecera.Horario            = Texto(dr, "Horario");
                    }

                    /* 2. contactos de emergencia */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Emergencia.Add(new EntPerfilEmergencia
                            {
                                IdContacto = EnteroDe(dr, "IdContacto"),
                                Nombre     = Texto(dr, "Nombre"),
                                Parentesco = Texto(dr, "Parentesco"),
                                Telefono   = Texto(dr, "Telefono")
                            });
                        }
                    }

                    /* 3. estudios */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Estudios.Add(new EntPerfilEstudio
                            {
                                IdEstudio      = EnteroDe(dr, "IdEstudio"),
                                Nivel          = Texto(dr, "Nivel"),
                                Institucion    = Texto(dr, "Institucion"),
                                Titulo         = Texto(dr, "Titulo"),
                                AnioGraduacion = EnteroNuloDe(dr, "AnioGraduacion")
                            });
                        }
                    }

                    /* 4. certificaciones */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Certificaciones.Add(new EntPerfilCertificacion
                            {
                                IdCertificacion = EnteroDe(dr, "IdCertificacion"),
                                Nombre          = Texto(dr, "Nombre"),
                                Entidad         = Texto(dr, "Entidad"),
                                FechaObtencion  = FechaMesDe(dr, "FechaObtencion")
                            });
                        }
                    }

                    /* 5. experiencia */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Experiencia.Add(new EntPerfilExperiencia
                            {
                                IdExperiencia = EnteroDe(dr, "IdExperiencia"),
                                Empresa       = Texto(dr, "Empresa"),
                                Cargo         = Texto(dr, "Cargo"),
                                AnioDesde     = EnteroNuloDe(dr, "AnioDesde"),
                                AnioHasta     = EnteroNuloDe(dr, "AnioHasta"),
                                Funciones     = Texto(dr, "Funciones")
                            });
                        }
                    }

                    /* 6. foto */
                    if (dr.NextResult() && dr.Read())
                    {
                        string base64 = Texto(dr, "FotoBase64");

                        if (base64 != "")
                        {
                            string tipo = Texto(dr, "FotoTipo");
                            if (tipo == "") { tipo = "image/jpeg"; }

                            /* Solo DataUri, igual que en CargarPerfil: las tres
                               propiedades juntas duplicarian el JSON por nada. */
                            perfil.Foto.DataUri = "data:" + tipo + ";base64," + base64;
                        }
                    }
                }
            }

            return perfil;
        }
```

- [ ] **Step 3: La fachada en `NegPerfil`**

En `CapaNegocio/NegPerfil.cs`, después de `ObtenerDocumento`, añade:

```csharp
        /// <summary>El equipo directo de una jefatura.</summary>
        public static List<EntPerfilEquipoItem> ListaEquipo(string codJefe, string filtro)
        {
            return DaoPerfil.ListaEquipo(codJefe, filtro);
        }

        /// <summary>
        /// El perfil recortado de un subordinado. Devuelve PerfilEncontrado en
        /// false si esa persona no le reporta a quien pregunta: la comprobacion
        /// la hace el procedimiento, no esta capa.
        /// </summary>
        public static EntPerfilEquipo PerfilEquipo(string codJefe, string codUsuario)
        {
            return DaoPerfil.PerfilEquipo(codJefe, codUsuario);
        }
```

Añade `using System.Collections.Generic;` a los `using` del archivo si no está.

- [ ] **Step 4: Compilar y correr las pruebas**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln -p:Configuration=Debug -v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas/bin/Debug/CapaPruebas.dll
```

Esperado: `0 Error(s)` y **109 pruebas pasando**.

- [ ] **Step 5: Contar los `NextResult()`**

El contrato es posicional: seis conjuntos, cinco llamadas.

```bash
sed -n '/public static EntPerfilEquipo PerfilEquipo/,/^        }/p' CapaDato/DaoPerfil.cs | grep -c "NextResult()"
```

Esperado: **5**. Cualquier otro número es un desfase que no da ningún error y hace aterrizar los datos en la lista equivocada. Pega la salida.

- [ ] **Step 6: Commit**

```bash
git add CapaDato/DaoPerfil.cs CapaNegocio/NegPerfil.cs
git commit -m "feat(perfil): la capa de datos de la vista de jefatura"
```

---

### Task 4: Las dos acciones del handler

**Files:**
- Modify: `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs`

**Interfaces:**
- Consumes: `NegPerfil.ListaEquipo(string, string)` y `NegPerfil.PerfilEquipo(string, string)` (tarea 3); los helpers privados ya existentes del handler: `CodUsuarioSesion(context)`, `Texto(campos, clave, omision)`, `ToJson(obj)`, `responseMessage(...)`.
- Produces: las acciones JSON `ListaEquipo` (parámetro `filtro`, opcional) y `PerfilEquipo` (parámetro `codUsuario`, obligatorio).

- [ ] **Step 1: Registrar las dos acciones**

En `ProcessRequest`, después del bloque `if (Action == "EliminarDocumento")`, añade:

```csharp
                if (Action == "ListaEquipo")
                {
                    existAction = true;
                    responseAction.Append(ListaEquipo(context, parametros[0]["parameters"]));
                }

                if (Action == "PerfilEquipo")
                {
                    existAction = true;
                    responseAction.Append(PerfilEquipo(context, parametros[0]["parameters"]));
                }
```

- [ ] **Step 2: Los dos métodos**

Después del método `EliminarDocumento`, añade:

```csharp
        /// <summary>
        /// El equipo directo de quien esta conectado.
        ///
        /// El unico parametro que se acepta del cliente es el texto del
        /// buscador. De quien es el equipo lo dice la sesion: no hay forma de
        /// pedir el equipo de otra persona porque no hay donde decirlo.
        /// </summary>
        private string ListaEquipo(HttpContext context, dynamic campos)
        {
            try
            {
                string codJefe = CodUsuarioSesion(context);
                if (codJefe == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                string filtro = Texto(campos, "filtro", "");

                return ToJson(NegPerfil.ListaEquipo(codJefe, filtro));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar su equipo. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// El perfil recortado de alguien del equipo.
        ///
        /// Esta accion es la UNICA de todo el modulo que acepta del cliente el
        /// codigo de otra persona. Por eso no se comprueba aqui si esa persona
        /// es subordinada: se le pasa al procedimiento junto con el codigo del
        /// jefe, que sale de la sesion, y es el procedimiento el que decide. Una
        /// comprobacion en este metodo seria una segunda verdad que algun dia
        /// discreparia de la primera.
        ///
        /// Cuando no es subordinado, PerfilEncontrado vuelve en false y el
        /// mensaje es el mismo que cuando el codigo no existe. Distinguirlos le
        /// confirmaria a quien esta probando codigos cual de ellos es real.
        /// </summary>
        private string PerfilEquipo(HttpContext context, dynamic campos)
        {
            try
            {
                string codJefe = CodUsuarioSesion(context);
                if (codJefe == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                string codUsuario = Texto(campos, "codUsuario", "");
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se indicó a quién consultar.", "warning");
                }

                return ToJson(NegPerfil.PerfilEquipo(codJefe, codUsuario));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar el perfil. " + ex.Message, "danger");
            }
        }
```

- [ ] **Step 3: Verificar el invariante de identidad**

```bash
grep -c "CodUsuarioSesion(context)" ReporteTareas/Formulario/AdministrarPerfil.ashx.cs
grep -n 'Texto(campos, "codUsuario"' ReporteTareas/Formulario/AdministrarPerfil.ashx.cs
```

Esperado: el primero devuelve **18** (16 previas + las dos nuevas). El segundo devuelve **exactamente una línea**, la de `PerfilEquipo` — es la única lectura de un código de usuario desde el cliente en todo el handler, y tiene que seguir siendo única. Pega las dos salidas; si los números no cuadran, dilo en vez de ajustarlos.

- [ ] **Step 4: Compilar y correr las pruebas**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln -p:Configuration=Debug -v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas/bin/Debug/CapaPruebas.dll
```

Esperado: `0 Error(s)` y **109 pruebas pasando**.

- [ ] **Step 5: Commit**

```bash
git add ReporteTareas/Formulario/AdministrarPerfil.ashx.cs
git commit -m "feat(perfil): acciones de equipo y de perfil de subordinado"
```

---

### Task 5: La pestaña de equipo

**Files:**
- Modify: `ReporteTareas/Formulario/MiPerfil.aspx`, `ReporteTareas/js/miPerfil.js`

**Interfaces:**
- Consumes: las acciones `ListaEquipo` (`{filtro}`) y `PerfilEquipo` (`{codUsuario}`) de la tarea 4; `respuesta.Cabecera.EsJefe` de `CargarPerfil`; y las funciones ya existentes `PostPerfil`, `MostrarMensaje`, `Iniciales`.
- Produces: `MostrarPestanaEquipo(esJefe)`, `BuscarEquipo()`, `LimpiarBusquedaEquipo()`, `PintarListaEquipo(lista)`, `VerSubordinado(codUsuario)`, `PintarSubordinado(perfil)`.

- [ ] **Step 1: Subir la versión del JavaScript**

En `ReporteTareas/Formulario/MiPerfil.aspx`, línea 4, cambia `?v=6` por `?v=7`:

```aspx
    <script src="../js/miPerfil.js?v=7" type="text/javascript"></script>
```

Sin esto, los navegadores que ya cargaron la pantalla siguen con el JavaScript viejo y la pestaña no funciona para nadie hasta que alguien vacíe su caché. **Ninguna otra tarea toca este número.**

- [ ] **Step 2: La pestaña, oculta por omisión**

En la lista `<ul class="nav nav-tabs">`, después del `<li>` de *Cargas familiares*, añade:

```aspx
                    <!-- Aparece sola: se muestra desde el JavaScript si la
                         cabecera dice EsJefe. No la enciende ningun perfil ni
                         ninguna fila de menu, asi que no hay una lista de jefes
                         que mantener. Son 22 personas hoy. -->
                    <li id="liTabEquipo" style="display: none">
                        <a href="#tabEquipo" data-toggle="tab"><i class="fa fa-sitemap"></i> Mi equipo</a>
                    </li>
```

- [ ] **Step 3: El contenido de la pestaña**

Dentro de `<div class="tab-content">`, después del `<div class="tab-pane" id="tabCargas">`, añade:

```aspx
                    <div class="tab-pane" id="tabEquipo">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Mi equipo
                                <span class="label label-default pull-right">
                                    <i class="fa fa-lock"></i> Solo consulta
                                </span>
                            </div>
                            <div class="panel-body">
                                <p class="text-muted" style="font-size: 12px">
                                    Las personas que le reportan directamente. Puede consultar su
                                    información laboral y de emergencia; los datos personales y las
                                    cargas familiares no se muestran.
                                </p>

                                <div class="row">
                                    <div class="form-group col-lg-6">
                                        <label>Buscar (nombre, código o cargo):</label>
                                        <input type="text" class="form-control" id="txtBuscarEquipo"
                                               placeholder="Escriba para filtrar…"
                                               onkeypress="if(event.keyCode==13){BuscarEquipo();return false;}" />
                                    </div>
                                    <div class="form-group col-lg-6" style="padding-top: 25px">
                                        <button type="button" class="btn btn-primary" onclick="BuscarEquipo()">
                                            <i class="fa fa-search"></i> Buscar
                                        </button>
                                        <button type="button" class="btn btn-default" onclick="LimpiarBusquedaEquipo()">
                                            Mostrar todos
                                        </button>
                                    </div>
                                </div>

                                <div class="table-responsive">
                                    <table class="table table-bordered table-hover" style="font-size: 90%">
                                        <thead class="bg-primary">
                                            <tr><th>Nombre</th><th>Cargo</th><th>Área</th><th>Ciudad</th><th style="width:70px">Ver</th></tr>
                                        </thead>
                                        <tbody id="cuerpoEquipo"></tbody>
                                    </table>
                                </div>
                            </div>
                        </div>

                        <!-- Se muestra al elegir a alguien de la lista. -->
                        <div class="panel panel-default" id="panelSubordinado" style="display: none">
                            <div class="panel-heading">
                                <span id="subNombre">–</span>
                                <button type="button" class="close" onclick="$('#panelSubordinado').hide()">&times;</button>
                            </div>
                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-lg-3 text-center">
                                        <img id="subFoto" alt="Foto"
                                             style="display: none; width: 96px; height: 96px; margin: 0 auto 10px;
                                                    border-radius: 50%; object-fit: cover" />
                                        <div id="subAvatar"
                                             style="width: 96px; height: 96px; margin: 0 auto 10px; border-radius: 50%;
                                                    background: #750202; color: #fff; font-size: 34px; line-height: 96px;">–</div>
                                    </div>
                                    <div class="col-lg-9">
                                        <div class="row">
                                            <div class="form-group col-lg-6"><label>Cargo</label>
                                                <p class="form-control-static" id="subCargo">–</p></div>
                                            <div class="form-group col-lg-6"><label>Área</label>
                                                <p class="form-control-static" id="subArea">–</p></div>
                                            <div class="form-group col-lg-6"><label>Ciudad</label>
                                                <p class="form-control-static" id="subCiudad">–</p></div>
                                            <div class="form-group col-lg-6"><label>Correo de notificación</label>
                                                <p class="form-control-static" id="subCorreo">–</p></div>
                                            <div class="form-group col-lg-6"><label>Jefe inmediato</label>
                                                <p class="form-control-static" id="subJefe">–</p></div>
                                            <div class="form-group col-lg-6"><label>Horario</label>
                                                <p class="form-control-static" id="subHorario">–</p></div>
                                        </div>
                                    </div>
                                </div>

                                <hr />
                                <h5><i class="fa fa-ambulance"></i> Contactos de emergencia</h5>
                                <div class="table-responsive">
                                    <table class="table table-bordered" style="font-size: 90%">
                                        <thead class="bg-primary"><tr><th>Nombre</th><th>Parentesco</th><th>Teléfono</th></tr></thead>
                                        <tbody id="cuerpoSubEmergencia"></tbody>
                                    </table>
                                </div>

                                <h5><i class="fa fa-graduation-cap"></i> Formación</h5>
                                <div class="table-responsive">
                                    <table class="table table-bordered" style="font-size: 90%">
                                        <thead class="bg-primary"><tr><th>Título</th><th>Institución</th><th>Nivel</th><th style="width:70px">Año</th></tr></thead>
                                        <tbody id="cuerpoSubEstudios"></tbody>
                                    </table>
                                </div>

                                <h5><i class="fa fa-certificate"></i> Certificaciones</h5>
                                <div class="table-responsive">
                                    <table class="table table-bordered" style="font-size: 90%">
                                        <thead class="bg-primary"><tr><th>Nombre</th><th>Entidad</th><th style="width:110px">Obtenida</th></tr></thead>
                                        <tbody id="cuerpoSubCertificaciones"></tbody>
                                    </table>
                                </div>

                                <h5><i class="fa fa-briefcase"></i> Experiencia</h5>
                                <div class="table-responsive">
                                    <table class="table table-bordered" style="font-size: 90%">
                                        <thead class="bg-primary"><tr><th>Empresa</th><th>Cargo</th><th style="width:110px">Período</th><th>Funciones</th></tr></thead>
                                        <tbody id="cuerpoSubExperiencia"></tbody>
                                    </table>
                                </div>
                            </div>
                        </div>
                    </div>
```

- [ ] **Step 4: Mostrar la pestaña y cargar el equipo**

En `ReporteTareas/js/miPerfil.js`, dentro de `CargarPerfil`, después de `PintarFoto(respuesta.Foto);`, añade:

```js
        MostrarPestanaEquipo(respuesta.Cabecera.EsJefe);
```

Y al final del archivo, añade:

```js
/* ---------------------------------------------------------------- equipo -- */

/* La pestana existe o no segun el dato, no segun un perfil ni una fila de
   menu: si alguien tiene gente que le reporta, la ve. Son 22 personas hoy y
   el dia que cambie no hay nada que mantener. */
function MostrarPestanaEquipo(esJefe) {
    if (!esJefe) { return; }

    $("#liTabEquipo").show();
    BuscarEquipo();
}

function BuscarEquipo() {
    PostPerfil("ListaEquipo", { filtro: $("#txtBuscarEquipo").val() }, function (respuesta) {
        // Un objeto con "estado" es un EntRespuesta, es decir, un error.
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        PintarListaEquipo(respuesta || []);
    });
}

function LimpiarBusquedaEquipo() {
    $("#txtBuscarEquipo").val("");
    BuscarEquipo();
}

function PintarListaEquipo(lista) {
    var $cuerpo = $("#cuerpoEquipo").empty();

    /* Al cambiar la lista se cierra el detalle: dejarlo abierto mostraria a una
       persona que ya no esta en los resultados. */
    $("#panelSubordinado").hide();

    if (lista.length === 0) {
        $cuerpo.append('<tr><td colspan="5" class="text-center text-muted">' +
                       'No hay personas que coincidan con esa búsqueda.</td></tr>');
        return;
    }

    $.each(lista, function (i, p) {
        var $fila = $("<tr></tr>");

        /* .text() y no concatenacion de HTML: estos nombres y cargos son de
           OTRAS personas y los teclearon ellas o Talento Humano. */
        $fila.append($("<td></td>").text(p.NombreCompleto || "–"));
        $fila.append($("<td></td>").text(p.Cargo || "–"));
        $fila.append($("<td></td>").text(p.Area || "–"));
        $fila.append($("<td></td>").text(p.Ciudad || "–"));

        var $ver = $('<button type="button" class="btn btn-primary btn-xs"><i class="fa fa-eye"></i></button>')
            .on("click", function () { VerSubordinado(p.CodUsuario); });

        $fila.append($('<td class="text-center"></td>').append($ver));
        $cuerpo.append($fila);
    });
}
```

- [ ] **Step 5: Pintar el detalle de un subordinado**

Al final de `miPerfil.js`, añade:

```js
function VerSubordinado(codUsuario) {
    PostPerfil("PerfilEquipo", { codUsuario: codUsuario }, function (respuesta) {
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        /* El servidor no distingue "no es de su equipo" de "no existe", y aqui
           tampoco: un mensaje distinto le confirmaria a quien prueba codigos
           cual de ellos es real. */
        if (!respuesta.PerfilEncontrado) {
            MostrarMensaje("No pudimos mostrar ese perfil.", "warning");
            $("#panelSubordinado").hide();
            return;
        }

        PintarSubordinado(respuesta);
    });
}

function PintarSubordinado(perfil) {
    var c = perfil.Cabecera;

    $("#subNombre").text(c.NombreCompleto || "–");
    $("#subCargo").text(c.Cargo || "–");
    $("#subArea").text(c.Area || "–");
    $("#subCiudad").text(c.Ciudad || "–");
    $("#subCorreo").text(c.CorreoNotificacion || "–");
    $("#subJefe").text(c.JefeInmediato || "–");
    $("#subHorario").text(c.Horario || "–");

    if (perfil.Foto && perfil.Foto.DataUri) {
        $("#subFoto").attr("src", perfil.Foto.DataUri).show();
        $("#subAvatar").hide();
    } else {
        $("#subFoto").hide().removeAttr("src");
        $("#subAvatar").text(Iniciales(c.NombreCompleto)).show();
    }

    PintarFilas("#cuerpoSubEmergencia", perfil.Emergencia, 3, function (x) {
        return [x.Nombre, x.Parentesco, x.Telefono];
    });

    PintarFilas("#cuerpoSubEstudios", perfil.Estudios, 4, function (x) {
        return [x.Titulo, x.Institucion, x.Nivel,
                x.AnioGraduacion === null ? "–" : String(x.AnioGraduacion)];
    });

    PintarFilas("#cuerpoSubCertificaciones", perfil.Certificaciones, 3, function (x) {
        return [x.Nombre, x.Entidad, x.FechaObtencion === "" ? "–" : x.FechaObtencion];
    });

    PintarFilas("#cuerpoSubExperiencia", perfil.Experiencia, 4, function (x) {
        var desde = x.AnioDesde === null ? "" : String(x.AnioDesde);
        var hasta = x.AnioHasta === null ? "Actual" : String(x.AnioHasta);
        return [x.Empresa, x.Cargo, desde === "" ? "–" : desde + " - " + hasta, x.Funciones];
    });

    $("#panelSubordinado").show();
}

/* Las cuatro tablas del detalle son de solo lectura y tienen la misma forma:
   vaciar, y pintar una celda por columna con .text(). Se comparte una funcion
   en vez de repetir el bucle cuatro veces con distinto numero de columnas. */
function PintarFilas(selectorCuerpo, lista, columnas, celdasDe) {
    var $cuerpo = $(selectorCuerpo).empty();

    if (!lista || lista.length === 0) {
        $cuerpo.append($('<tr></tr>').append(
            $('<td class="text-center text-muted"></td>')
                .attr("colspan", columnas)
                .text("Sin registros.")));
        return;
    }

    $.each(lista, function (i, x) {
        var $fila = $("<tr></tr>");

        $.each(celdasDe(x), function (j, valor) {
            $fila.append($("<td></td>").text(valor || "–"));
        });

        $cuerpo.append($fila);
    });
}
```

- [ ] **Step 6: Verificar que todos los selectores existen**

```bash
for id in liTabEquipo tabEquipo txtBuscarEquipo cuerpoEquipo panelSubordinado subNombre subFoto subAvatar subCargo subArea subCiudad subCorreo subJefe subHorario cuerpoSubEmergencia cuerpoSubEstudios cuerpoSubCertificaciones cuerpoSubExperiencia; do
  echo -n "$id -> "; grep -c "id=\"$id\"" ReporteTareas/Formulario/MiPerfil.aspx
done
```

Esperado: `1` para los dieciocho. Un `0` es un selector que no resuelve y que no da ningún error en el navegador: la función simplemente no hace nada. Pega la salida.

- [ ] **Step 7: Verificar que nada se pinta con HTML**

```bash
grep -n "\.html(" ReporteTareas/js/miPerfil.js
```

Esperado: **una sola línea**, la de `MostrarMensaje`, que es preexistente y recibe texto nuestro. Cualquier otra es un dato de una persona metido en una cadena de HTML. Pega la salida.

- [ ] **Step 8: Compilar**

```bash
MSYS_NO_PATHCONV=1 "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas.sln -p:Configuration=Debug -v:minimal
```

Esperado: `0 Error(s)`.

- [ ] **Step 9: Commit**

```bash
git add ReporteTareas/Formulario/MiPerfil.aspx ReporteTareas/js/miPerfil.js
git commit -m "feat(perfil): la jefatura consulta a su equipo desde la pantalla"
```

---

### Task 6: La prueba de la guarda, y cerrar el módulo

**Files:**
- Create: `docs/sql/2026-09-15-prueba-guarda-jefatura.sql`, `docs/superpowers/plans/2026-09-15-perfil-colaborador-fase3b-verificacion.md`
- Modify: `docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md`

La verificación número 3 del diseño —«la guarda de jefatura: que un `codUsuario` que no es subordinado devuelva vacío»— no se puede cubrir con una prueba unitaria: vive en SQL y necesita datos. Se cubre con una demostración contra la base real, dentro de una transacción que se revierte, que es lo que ya se hizo en la fase 2 para el borrado de RRHH.

**No ejecutes el script.** Lo corre el controlador de la sesión y te devuelve la salida.

- [ ] **Step 1: El script de la demostración**

Crea `docs/sql/2026-09-15-prueba-guarda-jefatura.sql`:

```sql
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

BEGIN TRANSACTION;

DECLARE @Jefe VARCHAR(50), @Suyo VARCHAR(50), @Ajeno VARCHAR(50);

/* Un jefe con al menos un subordinado, y uno de sus subordinados. */
SELECT TOP 1 @Jefe = LTRIM(RTRIM(s.Cod_Jefe_Inm)), @Suyo = LTRIM(RTRIM(s.Cod_Usuario))
  FROM dbo.R_Usuarios s
  JOIN dbo.R_Usuarios u ON LTRIM(RTRIM(u.Cod_Usuario)) = LTRIM(RTRIM(s.Cod_Jefe_Inm))
                       AND ISNULL(u.EstadoUsuario,0) = 0
 WHERE ISNULL(s.EstadoUsuario,0) = 0
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

IF (SELECT COUNT(*) FROM #Lista) > 0
    PRINT 'OK 4: la lista del equipo devolvio al menos una persona.';
ELSE
    RAISERROR('FALLO 4: la lista del equipo de un jefe real vino vacia.', 16, 1);

DROP TABLE #Lista;

ROLLBACK TRANSACTION;
PRINT 'Transaccion revertida. La base quedo exactamente como estaba.';
GO
```

- [ ] **Step 2: La lista de comprobación manual**

Crea `docs/superpowers/plans/2026-09-15-perfil-colaborador-fase3b-verificacion.md`:

```markdown
# Fase 3b — comprobación manual

El entorno de desarrollo no permite iniciar sesión: el login va por Active
Directory. Esta lista la corre el usuario después de desplegar.

## Antes

- [ ] El script `docs/sql/2026-09-15-perfil-colaborador-fase3b.sql` está aplicado
      en producción, sin `RAISERROR` en la salida.
- [ ] La demostración `docs/sql/2026-09-15-prueba-guarda-jefatura.sql` corrió con
      sus cuatro `OK` y terminó revirtiendo la transacción.
- [ ] Los binarios de las fases 2, 3a y 3b están desplegados.

## La pestaña aparece cuando debe

- [ ] Entrar con alguien que **no** tiene gente a cargo: la pestaña «Mi equipo»
      **no** se ve.
- [ ] Entrar con uno de los 22 jefes: la pestaña se ve y trae su equipo cargado.
- [ ] El jefe con **49 reportes**: la lista los muestra sin trabarse y el
      buscador filtra por nombre, por código y por cargo.

## Lo que la jefatura ve y lo que no

- [ ] Abrir la ficha de alguien del equipo: se ven cargo, área, ciudad, correo
      de notificación, jefe inmediato, horario, foto, contactos de emergencia,
      formación, certificaciones y experiencia.
- [ ] En esa misma ficha **no** aparecen: cédula, fecha de nacimiento, edad,
      domicilio, correo ni teléfono personales, estado civil, cargas familiares
      ni documentos de respaldo.
- [ ] No hay ningún botón que permita editar nada de esa ficha.
- [ ] No hay forma de descargar el CV de un subordinado.

## La guarda, desde el navegador

- [ ] Con las herramientas de desarrollador, repetir la petición de
      `PerfilEquipo` cambiando el `codUsuario` por el de **alguien que no es de
      su equipo**: la respuesta trae `PerfilEncontrado` en `false` y ningún dato.
- [ ] Repetirla con un `codUsuario` **que no existe**: el resultado es el mismo,
      sin que nada permita distinguir un caso del otro.

## Los casos que dictaron los datos

- [ ] Un jefe **de los 22** cuyo subordinado no tiene ficha de empleado: la ficha
      sale con lo disponible, no vacía.
- [ ] Un subordinado que todavía no registró nada de su hoja de vida: las cuatro
      tablas dicen «Sin registros.» en vez de quedarse en blanco.
```

- [ ] **Step 3: Marcar la fase en el diseño**

En `docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md`, en la sección `## Fases`, reemplaza el párrafo de la fase 3b por:

```markdown
**Fase 3b — lo compartido.** [COMPLETA] La vista de jefatura. Se separó de la 3a
a propósito: es la única parte del módulo donde una persona ve datos de otra, y
merece su propio ciclo de revisión en vez de compartirlo con un generador de PDF.

La restricción se construyó en dos capas que se refuerzan: la **guarda** vive en
`Sp_RTA_PerfilEquipo` —un `codUsuario` que no es subordinado devuelve cero filas
en los seis conjuntos— y el **recorte de campos** vive en `EntPerfilEquipoCabecera`,
que sencillamente no tiene dónde poner una cédula. `EntPerfilEquipoTests` falla el
día que alguien le agregue una propiedad prohibida.

Los documentos de respaldo **no** se le muestran a la jefatura. La matriz concede
certificaciones y calla sobre sus respaldos, y el respaldo de una certificación
suele ser un escaneo con la cédula impresa: sería la puerta trasera de la
restricción.
```

Y en la sección `## Verificación`, el punto 3 de la lista numerada —que hoy dice que le corresponde a la fase 3b— pasa a:

```markdown
3. La guarda de jefatura — que un `codUsuario` que no es subordinado devuelva vacío.
   **Cubierta** por `docs/sql/2026-09-15-prueba-guarda-jefatura.sql`, que lo
   demuestra contra la base real dentro de una transacción revertida. No se puede
   cubrir con una prueba unitaria porque la guarda vive en SQL y necesita datos.
```

Y el punto correspondiente de su lista de comprobación manual:

```markdown
- Un intento de leer el perfil de alguien que no es subordinado: vuelve vacío.
  **Cubierto** por la demostración en SQL y por la lista de la fase 3b.
```

- [ ] **Step 4: Commit**

```bash
git add docs/sql/2026-09-15-prueba-guarda-jefatura.sql docs/superpowers/plans/2026-09-15-perfil-colaborador-fase3b-verificacion.md docs/superpowers/specs/2026-09-14-perfil-colaborador-design.md
git commit -m "docs(perfil): la demostracion de la guarda de jefatura y el cierre del modulo"
```

- [ ] **Step 5: Avisar**

En el reporte, escribe textualmente: **«La demostración `docs/sql/2026-09-15-prueba-guarda-jefatura.sql` está lista y commiteada. No la ejecuté.»**

---

## Lo que queda para el usuario

1. **Correr los dos scripts SQL** contra producción, antes de publicar los binarios.
2. **Publicar con `FolderProfile` (Release) y copiar SIN sincronizar.** Un `robocopy /MIR` borra `connections.config` y el sitio no arranca.
3. **Regenerar y commitear el paquete de `obj/Release/Package/PackageTmp`.** El repositorio lo versiona a propósito y se olvidó en las fases 1, 2 y 3a.
4. **Correr la lista de comprobación manual** de `2026-09-15-perfil-colaborador-fase3b-verificacion.md`.
5. **Decidir si Talento Humano revisa lo que expone esta vista.** El diseño lo advirtió: un jefe con 49 reportes ve 49 fichas, y aunque el recorte deja fuera lo personal, sigue siendo superficie real de datos de terceros. Este es el momento de mirarlo.
