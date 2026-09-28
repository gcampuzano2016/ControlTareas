# Horas Extras: tiempo fuera de horario — Plan de implementación

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Mostrar en la pantalla y en el Excel de Horas Extras, por colaborador, el tiempo registrado fuera de su horario que no se va a pagar, separado en «sin aprobar» y «rechazado», con el detalle tramo por tramo y un aviso al cerrar el período. No cambia ningún monto.

**Architecture:** Un procedimiento de solo lectura (`Sp_RTA_HeFueraDeHorario`) pasa cada tramo por la función que ya clasifica el registro (`FN_RTA_ClasificarTramosHorario`) y devuelve dos conjuntos: los segmentos fuera de horario de los tramos no pagados, y los intervalos ya pagados. Una clase pura de `CapaNegocio` (`NegHeFueraDeHorario`) cuenta cada minuto una sola vez, con la prioridad pagado > sin aprobar > rechazado. La pantalla lo pide aparte de la grilla y el Excel lo agrega sin tocar la hoja «Pagos».

**Tech Stack:** ASP.NET WebForms (.NET Framework 4.8 en el web, 4.6.1 en las capas), SQL Server, jQuery + Bootstrap 3, EPPlus, MSTest v1 (ensamblado de VS2019, sin NuGet), `vstest.console.exe`.

**Spec:** `docs/superpowers/specs/2026-09-28-horas-extras-fuera-de-horario-design.md`

## Global Constraints

- **Nada cambia un monto.** `HE_Detalle`, los totales de la pantalla y la hoja «Pagos» quedan exactamente igual.
- **Cruce de identidad:** `Id_Responsable → R_Usuarios.Cod_Usuario → R_Usuarios.Cedula → Empleados.Cedula → Empleados.IdEmpleado → HE_ColaboradorParametro (Estado = '1')`. Nunca en sentido contrario.
- **Pagado** = `Det_Horas_Extras_Estado = 2` **y** `Det_Horas_Extras_Tipo IN (1, 2)`. **Rechazado** = estado 3. **Sin aprobar** = estado 0, 1, o 2 con tipo 0.
- **Prioridad de un minuto disputado:** pagado > sin aprobar > rechazado. Dentro de una misma categoría, 100% gana a 50%.
- **Los totales se suman en minutos enteros.** Las horas se redondean a 2 decimales una sola vez, solo al mostrar.
- **`FN_RTA_ClasificarTramosHorario` no se modifica ni se vuelve a crear.** Se versiona como referencia no ejecutable.
- **Sin datos personales en git.** Las pruebas usan datos inventados; la validación con datos reales vive en `Actualizacion/`, que está ignorada.
- **Los `.sql` versionados van sin BOM y sin tildes.** `HorasExtras.aspx` **con** BOM.
- **Las capas se publican como `.exe`** (`CapaEntidad.exe`, `CapaDato.exe`, `CapaNegocio.exe`), siempre junto con `ReporteTareas.dll`.
- **Se compila con** `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe`, **desde PowerShell**.
- **Las pruebas se ejecutan con** `"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll`.
- **Perfiles:** la pantalla la usan los que ya están en `AdministrarHorasExtras.PerfilesAutorizados` (2, 14 y 18). No se agrega ninguna comprobación nueva.
- **Commits:** solo con los archivos de la tarea. `ReporteTareas/Web.config` y los `bin/` que ya aparecen modificados en el árbol de trabajo **no son de este trabajo** y no se agregan.

## Review Focus

1. **Cambiar de período mientras carga el cálculo.** La respuesta vieja no debe pintarse sobre el período nuevo. → Tarea 5, paso de prueba manual 3.
2. **Repintar la grilla después de Guardar.** Las dos columnas vuelven a llenarse; no quedan en «…» ni desalinean el encabezado. → Tarea 5, prueba manual 4.
3. **Una actividad con `<`, `&` o tildes.** Tiene que verse como texto en la ventana; nunca se interpreta como HTML. → Tarea 5, prueba manual 5 (todo se pinta con `.text()`).
4. **Una persona en el cálculo que no está en la grilla.** El Excel no falla y deja el nombre vacío. → Tarea 6, prueba `Construir_PersonaFueraDeLaGrilla_NoFalla`.
5. **El cálculo no está disponible al descargar.** Salen igual las tres hojas, la tercera con el mensaje. → Tarea 6, prueba `Construir_SinCalculo_HojaConMensaje`.

---

## Mapa de archivos

| Archivo | Acción | Responsabilidad |
|---|---|---|
| `docs/sql/referencia/FN_RTA_ClasificarTramosHorario.sql` | Crear | Copia fiel de la función, marcada como no ejecutable |
| `docs/sql/2026-09-28-horas-extras-fuera-de-horario.sql` | Crear | `Sp_RTA_HeFueraDeHorario` y su verificación |
| `CapaEntidad/EntHeSegmentoFuera.cs` | Crear | Un segmento fuera de horario, tal como lo entrega el SQL |
| `CapaEntidad/EntHeIntervaloPagado.cs` | Crear | Un tramo ya pagado, usado solo para descontar |
| `CapaEntidad/EntHeFueraDeHorario.cs` | Crear | El resultado: personas, detalle, totales y error |
| `CapaEntidad/CapaEntidad.csproj` | Modificar | Incluir los tres archivos |
| `CapaNegocio/NegHeFueraDeHorario.cs` | Crear | La aritmética de minutos (clase pura) |
| `CapaNegocio/NegHorasExtrasPantalla.cs` | Modificar | `CalcularFueraDeHorario` y `CargarFueraDeHorario` |
| `CapaNegocio/NegHeExportacion.cs` | Modificar | Columnas 24 y 25 en el detalle, y la hoja «Fuera de horario» |
| `CapaNegocio/CapaNegocio.csproj` | Modificar | Incluir `NegHeFueraDeHorario.cs` |
| `CapaDato/DaoHorasExtras.cs` | Modificar | `LeerFueraDeHorario` |
| `CapaPruebas/NegHeFueraDeHorarioTests.cs` | Crear | Pruebas de la clase pura |
| `CapaPruebas/NegHeExportacionTests.cs` | Modificar | Tres hojas y las columnas nuevas |
| `CapaPruebas/CapaPruebas.csproj` | Modificar | Incluir el archivo de pruebas nuevo |
| `ReporteTareas/Formulario/AdministrarHorasExtras.ashx.cs` | Modificar | Acción `CargarFueraDeHorario` |
| `ReporteTareas/Formulario/DescargarHorasExtras.ashx.cs` | Modificar | Pasar el cálculo al Excel |
| `ReporteTareas/Formulario/HorasExtras.aspx` | Modificar | Dos columnas, pie, ventana y `?v=8` |
| `ReporteTareas/js/horasExtras.js` | Modificar | Cargar, pintar, ventana de detalle y aviso al cerrar |
| `DESPLIEGUE.md` | Modificar | Sección 12 |
| `Actualizacion/validar-fuera-de-horario.ps1` | Crear (no se versiona) | Ejecuta la clase real sobre los datos reales del período 3 |
| `Actualizacion/conciliacion-he-16ago-15sep.ps1` | Crear (no se versiona) | Cuadre final contra el reporte manual |

---

### Task 1: SQL — procedimiento y referencia de la función

**Files:**
- Create: `docs/sql/referencia/FN_RTA_ClasificarTramosHorario.sql`
- Create: `docs/sql/2026-09-28-horas-extras-fuera-de-horario.sql`

**Interfaces:**
- Produces: `dbo.Sp_RTA_HeFueraDeHorario(@FechaInicio DATE, @FechaFin DATE)` con dos conjuntos:
  1. `IdEmpleado BIGINT, IdRegDetTareas BIGINT, TramoInicio DATETIME, TramoFin DATETIME, SegmentoInicio DATETIME, SegmentoFin DATETIME, TipoHora INT, EstadoTramo INT, TipoRegistrado INT, Actividad VARCHAR(200)`
  2. `IdEmpleado BIGINT, Inicio DATETIME, Fin DATETIME`

- [ ] **Step 1: Volcar la función actual a la referencia**

`PRINT` trunca a 8000 caracteres; la función mide unos 5.500, pero se vuelca con `-y 0` igual. Desde Git Bash, en la raíz del repositorio:

```bash
cfg="ReporteTareas/connections.config"; cs=$(grep -io 'connectionString="[^"]*"' "$cfg" | head -1)
servidor=$(echo "$cs" | sed -n 's/.*[Dd]ata [Ss]ource=\([^;]*\).*/\1/p'); base=$(echo "$cs" | sed -n 's/.*[Ii]nitial [Cc]atalog=\([^;]*\).*/\1/p')
usuario=$(echo "$cs" | sed -n 's/.*[Uu]ser [Ii][Dd]=\([^;]*\).*/\1/p'); clave=$(echo "$cs" | sed -n 's/.*[Pp]assword=\([^;"]*\).*/\1/p')
mkdir -p docs/sql/referencia
sqlcmd -S "$servidor" -d "$base" -U "$usuario" -P "$clave" -f 65001 -y 0 -h -1 \
  -Q "SET NOCOUNT ON; SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.FN_RTA_ClasificarTramosHorario'));" \
  > /tmp/fn.sql
wc -c /tmp/fn.sql
```

Si `-h -1` choca con `-y 0` («mutually exclusive»), se quita `-h -1` y se borran a mano las dos primeras líneas del archivo (el encabezado y la línea de guiones).

Expected: unos 5.400-5.600 bytes, empezando por `CREATE FUNCTION dbo.FN_RTA_ClasificarTramosHorario`.

- [ ] **Step 2: Escribir la referencia con su advertencia**

Crear `docs/sql/referencia/FN_RTA_ClasificarTramosHorario.sql` con este encabezado, seguido del texto volcado en el paso 1 **sin tocarlo**:

```sql
/* ============================================================================
   REFERENCIA - NO EJECUTAR

   Copia fiel de dbo.FN_RTA_ClasificarTramosHorario tal como estaba en
   produccion el 2026-09-28. Se versiona para poder leerla y revisarla: hasta
   hoy solo existia en la base.

   NO correr este archivo. La usa Sp_RTAInsertaDetalleTarea_V2 en cada registro
   de actividad y ahora tambien Sp_RTA_HeFueraDeHorario. Volver a crearla desde
   aqui es la forma mas facil de cambiarla sin querer, y si el texto de este
   archivo quedo viejo, se pisaria la version buena.

   Si la funcion cambia en la base, se vuelve a volcar este archivo; no al
   reves.
   ============================================================================ */

SET NOEXEC ON;
GO
```

Al final del archivo, después del texto de la función:

```sql
GO
SET NOEXEC OFF;
GO
```

`SET NOEXEC ON` hace que, si alguien lo corre por error, SQL Server lo compile sin ejecutar nada.

- [ ] **Step 3: Escribir el script del procedimiento**

Crear `docs/sql/2026-09-28-horas-extras-fuera-de-horario.sql` (UTF-8 **sin BOM**, sin tildes):

```sql
/* ============================================================================
   Horas Extras: tiempo registrado fuera de horario
   ReporTarea  |  2026-09-28

   Crea Sp_RTA_HeFueraDeHorario. SOLO LECTURA: no escribe en ninguna tabla.
   Nadie lo llama hasta que se publiquen los binarios, asi que puede correrse
   antes que ellos sin efecto en la pantalla.

   Que devuelve, para el rango del periodo (por Det_Fch_RegDetalleIni, la fecha
   del TRABAJO, igual que Sp_RTA_HeHorasAprobadas):

     1. Los segmentos fuera de horario de los tramos que NO estan pagados. La
        clasificacion la hace FN_RTA_ClasificarTramosHorario, la misma que usa
        el registro, con el horario que la persona tenia ese dia.
     2. Los intervalos de los tramos pagados (estado 2 y tipo 1 o 2), para que
        CapaNegocio descuente lo que ya cubren.

   Restar y unir intervalos NO se hace aqui a proposito: vive en
   NegHeFueraDeHorario, que tiene pruebas. En T-SQL no las tendria.

   El puente de identidad es el de Sp_RTA_HeHorasAprobadas, en el mismo
   sentido: tarea -> usuario -> cedula -> Empleados -> maestro. Al reves, una
   cedula con seis cuentas sumaria seis veces.

   Re-correrlo es inofensivo: el procedimiento es nuevo y no lo recrea ninguna
   otra fase. Si una fase posterior lo recrea con otra firma, este script tiene
   que llevar el guard SET NOEXEC de las fases 2, 4 y 5.
   ============================================================================ */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('dbo.Sp_RTA_HeFueraDeHorario','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_HeFueraDeHorario;
GO

CREATE PROCEDURE dbo.Sp_RTA_HeFueraDeHorario
    @FechaInicio DATE,
    @FechaFin    DATE
AS
BEGIN
    SET NOCOUNT ON;

    /* 1. Segmentos fuera de horario de los tramos no pagados.
          TipoHora 0 (horas normales) se descarta en el WHERE. Los tramos que
          cruzan la medianoche o caen en un dia sin configuracion no devuelven
          nada de la funcion: el registro ya los rechaza, y aqui no se inventa
          una clasificacion para ellos. */
    SELECT c.IdEmpleado,
           CAST(d.Id_RegDetTareas AS BIGINT)                   AS IdRegDetTareas,
           d.Det_Fch_RegDetalleIni                             AS TramoInicio,
           d.Det_Fch_RegDetalleFin                             AS TramoFin,
           s.TramoInicio                                       AS SegmentoInicio,
           s.TramoFin                                          AS SegmentoFin,
           CAST(s.TipoHora AS INT)                             AS TipoHora,
           CAST(d.Det_Horas_Extras_Estado AS INT)              AS EstadoTramo,
           CAST(ISNULL(d.Det_Horas_Extras_Tipo, 0) AS INT)     AS TipoRegistrado,
           LEFT(ISNULL(d.Det_Det_Tarea, ''), 200)              AS Actividad
      FROM dbo.R_DetTareasAranda d
      JOIN dbo.R_Usuarios u ON u.Cod_Usuario = d.Id_Responsable
      JOIN dbo.Empleados  e ON LTRIM(RTRIM(e.Cedula)) = LTRIM(RTRIM(u.Cedula))
      JOIN dbo.HE_ColaboradorParametro c ON c.IdEmpleado = e.IdEmpleado AND c.Estado = '1'
     CROSS APPLY dbo.FN_RTA_ClasificarTramosHorario(d.Id_Responsable,
                                                    d.Det_Fch_RegDetalleIni,
                                                    d.Det_Fch_RegDetalleFin) s
     WHERE d.Det_Fch_RegDetalleIni >= @FechaInicio
       AND d.Det_Fch_RegDetalleIni <  DATEADD(DAY, 1, @FechaFin)
       AND d.Det_Fch_RegDetalleFin IS NOT NULL
       AND s.TipoHora IN (1, 2)
       AND d.Det_Horas_Extras_Estado IN (0, 1, 2, 3)
       AND NOT (d.Det_Horas_Extras_Estado = 2 AND ISNULL(d.Det_Horas_Extras_Tipo, 0) IN (1, 2))
     ORDER BY c.IdEmpleado, s.TramoInicio;

    /* 2. Lo ya pagado: el intervalo COMPLETO del tramo, no solo su parte fuera
          de horario. Un minuto pagado no aparece en ninguna otra columna. */
    SELECT c.IdEmpleado,
           d.Det_Fch_RegDetalleIni AS Inicio,
           d.Det_Fch_RegDetalleFin AS Fin
      FROM dbo.R_DetTareasAranda d
      JOIN dbo.R_Usuarios u ON u.Cod_Usuario = d.Id_Responsable
      JOIN dbo.Empleados  e ON LTRIM(RTRIM(e.Cedula)) = LTRIM(RTRIM(u.Cedula))
      JOIN dbo.HE_ColaboradorParametro c ON c.IdEmpleado = e.IdEmpleado AND c.Estado = '1'
     WHERE d.Det_Fch_RegDetalleIni >= @FechaInicio
       AND d.Det_Fch_RegDetalleIni <  DATEADD(DAY, 1, @FechaFin)
       AND d.Det_Fch_RegDetalleFin IS NOT NULL
       AND d.Det_Horas_Extras_Estado = 2
       AND ISNULL(d.Det_Horas_Extras_Tipo, 0) IN (1, 2)
     ORDER BY c.IdEmpleado, d.Det_Fch_RegDetalleIni;
END
GO

/* ------------------------------------------------------ VERIFICACION ------ */
/* Sin GO entre el DECLARE y el veredicto: @Fallos no sobreviviria al lote. */
SET NOCOUNT ON;
DECLARE @Fallos INT = 0;

IF OBJECT_ID('dbo.Sp_RTA_HeFueraDeHorario','P') IS NULL
BEGIN
    PRINT 'FALLO: Sp_RTA_HeFueraDeHorario no existe.';
    SET @Fallos += 1;
END

IF (SELECT COUNT(*) FROM sys.parameters
     WHERE object_id = OBJECT_ID('dbo.Sp_RTA_HeFueraDeHorario')) <> 2
BEGIN
    PRINT 'FALLO: Sp_RTA_HeFueraDeHorario deberia tener exactamente 2 parametros.';
    SET @Fallos += 1;
END

/* Que corra sobre un periodo real sin error. Devuelve las dos grillas: son
   solo lectura y se pueden ignorar. */
DECLARE @Ini DATE, @Fin DATE;
SELECT TOP (1) @Ini = FechaInicio, @Fin = FechaFin FROM dbo.HE_Periodo ORDER BY FechaInicio DESC;

IF @Ini IS NOT NULL
BEGIN
    BEGIN TRY
        EXEC dbo.Sp_RTA_HeFueraDeHorario @FechaInicio = @Ini, @FechaFin = @Fin;
    END TRY
    BEGIN CATCH
        PRINT 'FALLO: Sp_RTA_HeFueraDeHorario fallo al ejecutarse: ' + ERROR_MESSAGE();
        SET @Fallos += 1;
    END CATCH
END

IF @Fallos = 0
    PRINT 'OK: Sp_RTA_HeFueraDeHorario listo.';
ELSE
    PRINT 'VEREDICTO: FALLO (' + CAST(@Fallos AS VARCHAR(10)) + ' comprobaciones). Revisar arriba.';
GO
```

- [ ] **Step 4: Comprobar que el SELECT funciona contra la base, sin crear nada**

Se ejecuta **solo el cuerpo** del primer SELECT como consulta suelta (solo lectura), con el rango del período 3. Así se prueba el SQL antes de pedir permiso para crear el procedimiento.

```bash
# mismas variables del paso 1
sqlcmd -S "$servidor" -d "$base" -U "$usuario" -P "$clave" -I -f 65001 -W -s "|" -Q "SET NOCOUNT ON;
DECLARE @FechaInicio DATE='2026-08-16', @FechaFin DATE='2026-09-15';
SELECT COUNT(*) AS Segmentos, SUM(DATEDIFF(MINUTE, s.TramoInicio, s.TramoFin)) AS Minutos
  FROM dbo.R_DetTareasAranda d
  JOIN dbo.R_Usuarios u ON u.Cod_Usuario = d.Id_Responsable
  JOIN dbo.Empleados  e ON LTRIM(RTRIM(e.Cedula)) = LTRIM(RTRIM(u.Cedula))
  JOIN dbo.HE_ColaboradorParametro c ON c.IdEmpleado = e.IdEmpleado AND c.Estado = '1'
 CROSS APPLY dbo.FN_RTA_ClasificarTramosHorario(d.Id_Responsable, d.Det_Fch_RegDetalleIni, d.Det_Fch_RegDetalleFin) s
 WHERE d.Det_Fch_RegDetalleIni >= @FechaInicio AND d.Det_Fch_RegDetalleIni < DATEADD(DAY,1,@FechaFin)
   AND d.Det_Fch_RegDetalleFin IS NOT NULL AND s.TipoHora IN (1,2)
   AND d.Det_Horas_Extras_Estado IN (0,1,2,3)
   AND NOT (d.Det_Horas_Extras_Estado = 2 AND ISNULL(d.Det_Horas_Extras_Tipo,0) IN (1,2));"
```

Expected: una fila con decenas de segmentos y un total de minutos mayor que cero, sin error. El 2026-09-28, **sin** el filtro de «no pagado», daba 378 segmentos y 39.205 minutos; con el filtro tiene que dar menos.

- [ ] **Step 5: Comprobar que ninguno de los dos archivos lleva BOM ni tildes**

```bash
head -c 3 docs/sql/2026-09-28-horas-extras-fuera-de-horario.sql | od -c | head -1
grep -nP '[^\x00-\x7F]' docs/sql/2026-09-28-horas-extras-fuera-de-horario.sql docs/sql/referencia/FN_RTA_ClasificarTramosHorario.sql
```

Expected: el primer byte es `/`, no `357 273 277`, y el `grep` no imprime nada. Si la función original trae alguna tilde en sus comentarios, se deja tal cual en la referencia: es una copia fiel y nunca se ejecuta.

- [ ] **Step 6: Commit**

```bash
git add docs/sql/2026-09-28-horas-extras-fuera-de-horario.sql docs/sql/referencia/FN_RTA_ClasificarTramosHorario.sql
git commit -m "feat(horas-extras): Sp_RTA_HeFueraDeHorario y referencia de la funcion de horarios

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

> **No se ejecuta en producción en esta tarea.** Crear el procedimiento es una escritura en la base, y va en la Tarea 9 con la aprobación del usuario.

---

### Task 2: Entidades y cálculo puro (TDD)

**Files:**
- Create: `CapaEntidad/EntHeSegmentoFuera.cs`, `CapaEntidad/EntHeIntervaloPagado.cs`, `CapaEntidad/EntHeFueraDeHorario.cs`
- Create: `CapaNegocio/NegHeFueraDeHorario.cs`
- Create: `CapaPruebas/NegHeFueraDeHorarioTests.cs`
- Modify: `CapaEntidad/CapaEntidad.csproj` (junto a `<Compile Include="EntHeSalario.cs" />`), `CapaNegocio/CapaNegocio.csproj` (junto a `<Compile Include="NegHeExportacion.cs" />`), `CapaPruebas/CapaPruebas.csproj` (junto a `<Compile Include="NegHeExportacionTests.cs" />`)

**Interfaces:**
- Produces:
  - `EntHeSegmentoFuera { long IdEmpleado; long IdRegDetTareas; DateTime TramoInicio, TramoFin, SegmentoInicio, SegmentoFin; int TipoHora; int EstadoTramo; int TipoRegistrado; string Actividad }`
  - `EntHeIntervaloPagado { long IdEmpleado; DateTime Inicio, Fin }`
  - `EntHeFueraDeHorarioPersona { long IdEmpleado; int MinutosSinAprobar50, MinutosSinAprobar100, MinutosRechazado50, MinutosRechazado100; decimal HorasSinAprobar {get;}; decimal HorasRechazado {get;} }`
  - `EntHeFueraDeHorarioDetalle { long IdEmpleado; long IdRegDetTareas; DateTime TramoInicio, TramoFin, Inicio, Fin; int Minutos; int TipoHora; string Categoria; string EstadoTexto; string Actividad; string Fecha {get;}; string TramoTexto {get;}; string FueraTexto {get;}; string TipoTexto {get;}; decimal Horas {get;} }`
  - `EntHeFueraDeHorario { bool Calculado; string Error; List<EntHeFueraDeHorarioPersona> Personas; List<EntHeFueraDeHorarioDetalle> Detalle; decimal TotalHorasSinAprobar {get;}; decimal TotalHorasRechazado {get;}; int PersonasConSinAprobar {get;}; int PersonasConRechazado {get;}; EntHeFueraDeHorarioPersona DePersona(long idEmpleado) }`
  - `static EntHeFueraDeHorario NegHeFueraDeHorario.Calcular(IList<EntHeSegmentoFuera> segmentos, IList<EntHeIntervaloPagado> pagados)`
  - `const string NegHeFueraDeHorario.CategoriaSinAprobar = "SinAprobar"`, `CategoriaRechazado = "Rechazado"`

- [ ] **Step 1: Crear las tres entidades**

`CapaEntidad/EntHeSegmentoFuera.cs`:

```csharp
using System;

namespace CapaEntidad
{
    /// <summary>
    /// Un pedazo de un tramo registrado que cae fuera del horario de la persona,
    /// tal como lo devuelve Sp_RTA_HeFueraDeHorario. Todavia no esta depurado:
    /// dos segmentos pueden cubrir el mismo minuto, o uno puede estar ya cubierto
    /// por un tramo pagado. Eso lo resuelve NegHeFueraDeHorario.
    /// </summary>
    public class EntHeSegmentoFuera
    {
        public long IdEmpleado { get; set; }
        public long IdRegDetTareas { get; set; }

        /// <summary>El tramo tal como lo registro la persona.</summary>
        public DateTime TramoInicio { get; set; }
        public DateTime TramoFin { get; set; }

        /// <summary>La parte de ese tramo que la funcion de horarios marca como 50% o 100%.</summary>
        public DateTime SegmentoInicio { get; set; }
        public DateTime SegmentoFin { get; set; }

        /// <summary>1 = 50%, 2 = 100%, segun el horario (no segun lo que se eligio al registrar).</summary>
        public int TipoHora { get; set; }

        /// <summary>Det_Horas_Extras_Estado: 0 sin solicitar, 1 solicitado, 2 aprobado, 3 rechazado.</summary>
        public int EstadoTramo { get; set; }

        /// <summary>Det_Horas_Extras_Tipo: 0 sin tipo, 1 = 50%, 2 = 100%. Decide si un aprobado esta pagado.</summary>
        public int TipoRegistrado { get; set; }

        public string Actividad { get; set; } = "";
    }
}
```

`CapaEntidad/EntHeIntervaloPagado.cs`:

```csharp
using System;

namespace CapaEntidad
{
    /// <summary>
    /// Un tramo ya pagado (aprobado con tipo 50% o 100%). Se usa entero, no solo
    /// su parte fuera de horario: un minuto que ya se paga no puede aparecer
    /// ademas como pendiente.
    /// </summary>
    public class EntHeIntervaloPagado
    {
        public long IdEmpleado { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
    }
}
```

`CapaEntidad/EntHeFueraDeHorario.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CapaEntidad
{
    /// <summary>
    /// El tiempo registrado fuera de horario que NO se va a pagar, por persona.
    /// Es informacion para que Nomina decida antes de cerrar; ningun monto sale
    /// de aqui.
    ///
    /// Calculado = false significa "no se pudo calcular" (Error dice por que), y
    /// no "no hay nada": confundir las dos cosas llevaria a cerrar un periodo
    /// creyendo que esta limpio.
    /// </summary>
    public class EntHeFueraDeHorario
    {
        public bool Calculado { get; set; }
        public string Error { get; set; } = "";
        public List<EntHeFueraDeHorarioPersona> Personas { get; set; } = new List<EntHeFueraDeHorarioPersona>();
        public List<EntHeFueraDeHorarioDetalle> Detalle { get; set; } = new List<EntHeFueraDeHorarioDetalle>();

        public decimal TotalHorasSinAprobar
        {
            get
            {
                int minutos = 0;
                foreach (EntHeFueraDeHorarioPersona p in Personas) { minutos += p.MinutosSinAprobar50 + p.MinutosSinAprobar100; }
                return EntHeFueraDeHorarioPersona.Horas(minutos);
            }
        }

        public decimal TotalHorasRechazado
        {
            get
            {
                int minutos = 0;
                foreach (EntHeFueraDeHorarioPersona p in Personas) { minutos += p.MinutosRechazado50 + p.MinutosRechazado100; }
                return EntHeFueraDeHorarioPersona.Horas(minutos);
            }
        }

        public int PersonasConSinAprobar
        {
            get
            {
                int n = 0;
                foreach (EntHeFueraDeHorarioPersona p in Personas) { if (p.MinutosSinAprobar50 + p.MinutosSinAprobar100 > 0) { n++; } }
                return n;
            }
        }

        public int PersonasConRechazado
        {
            get
            {
                int n = 0;
                foreach (EntHeFueraDeHorarioPersona p in Personas) { if (p.MinutosRechazado50 + p.MinutosRechazado100 > 0) { n++; } }
                return n;
            }
        }

        /// <summary>La persona, o null si no tiene nada fuera de horario.</summary>
        public EntHeFueraDeHorarioPersona DePersona(long idEmpleado)
        {
            foreach (EntHeFueraDeHorarioPersona p in Personas) { if (p.IdEmpleado == idEmpleado) { return p; } }
            return null;
        }
    }

    public class EntHeFueraDeHorarioPersona
    {
        public long IdEmpleado { get; set; }
        public int MinutosSinAprobar50 { get; set; }
        public int MinutosSinAprobar100 { get; set; }
        public int MinutosRechazado50 { get; set; }
        public int MinutosRechazado100 { get; set; }

        /// <summary>Se redondea UNA vez, sobre los minutos sumados: redondear cada tramo y sumar descuadra centavos.</summary>
        public decimal HorasSinAprobar { get { return Horas(MinutosSinAprobar50 + MinutosSinAprobar100); } }
        public decimal HorasRechazado { get { return Horas(MinutosRechazado50 + MinutosRechazado100); } }

        public static decimal Horas(int minutos)
        {
            return Math.Round(minutos / 60m, 2, MidpointRounding.AwayFromZero);
        }
    }

    public class EntHeFueraDeHorarioDetalle
    {
        public long IdEmpleado { get; set; }
        public long IdRegDetTareas { get; set; }
        public DateTime TramoInicio { get; set; }
        public DateTime TramoFin { get; set; }

        /// <summary>El pedazo que efectivamente suma, ya descontado lo pagado y lo repetido.</summary>
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public int Minutos { get; set; }
        public int TipoHora { get; set; }

        /// <summary>"SinAprobar" o "Rechazado" (constantes de NegHeFueraDeHorario).</summary>
        public string Categoria { get; set; } = "";

        /// <summary>Sin solicitar / Solicitado / Aprobado sin tipo / Rechazado.</summary>
        public string EstadoTexto { get; set; } = "";
        public string Actividad { get; set; } = "";

        // Textos ya armados: la pantalla y el Excel los muestran igual, y
        // JavaScriptSerializer entrega las fechas como \/Date(n)\/.
        public string Fecha { get { return Inicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture); } }
        public string TramoTexto { get { return Rango(TramoInicio, TramoFin); } }
        public string FueraTexto { get { return Rango(Inicio, Fin); } }
        public string TipoTexto { get { return TipoHora == 2 ? "100%" : "50%"; } }
        public decimal Horas { get { return EntHeFueraDeHorarioPersona.Horas(Minutos); } }

        private static string Rango(DateTime desde, DateTime hasta)
        {
            return desde.ToString("HH:mm", CultureInfo.InvariantCulture) + " - " + hasta.ToString("HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
```

Agregar a `CapaEntidad/CapaEntidad.csproj`, junto a `<Compile Include="EntHeSalario.cs" />`:

```xml
    <Compile Include="EntHeFueraDeHorario.cs" />
    <Compile Include="EntHeIntervaloPagado.cs" />
    <Compile Include="EntHeSegmentoFuera.cs" />
```

- [ ] **Step 2: Escribir las pruebas que fallan**

Crear `CapaPruebas/NegHeFueraDeHorarioTests.cs`:

```csharp
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CapaPruebas
{
    /// <summary>
    /// La aritmetica de minutos del tiempo fuera de horario. Los datos son
    /// inventados: el repositorio es publico.
    ///
    /// Cada caso describe una situacion real que salio en la validacion del
    /// periodo 16-08 / 15-09-2026, con otras personas y otras horas.
    /// </summary>
    [TestClass]
    public class NegHeFueraDeHorarioTests
    {
        private const long Persona = 101;
        private const long OtraPersona = 202;

        private static DateTime H(string texto)
        {
            return DateTime.ParseExact(texto, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        }

        private static EntHeSegmentoFuera Seg(long idTramo, long empleado, string inicio, string fin,
                                             int tipoHora, int estado, int tipoRegistrado = 0)
        {
            return new EntHeSegmentoFuera
            {
                IdEmpleado = empleado,
                IdRegDetTareas = idTramo,
                TramoInicio = H(inicio),
                TramoFin = H(fin),
                SegmentoInicio = H(inicio),
                SegmentoFin = H(fin),
                TipoHora = tipoHora,
                EstadoTramo = estado,
                TipoRegistrado = tipoRegistrado,
                Actividad = "ACTIVIDAD " + idTramo
            };
        }

        private static EntHeIntervaloPagado Pagado(long empleado, string inicio, string fin)
        {
            return new EntHeIntervaloPagado { IdEmpleado = empleado, Inicio = H(inicio), Fin = H(fin) };
        }

        private static EntHeFueraDeHorario Calcular(List<EntHeSegmentoFuera> s, List<EntHeIntervaloPagado> p = null)
        {
            return NegHeFueraDeHorario.Calcular(s, p ?? new List<EntHeIntervaloPagado>());
        }

        [TestMethod]
        public void SegmentoSolo_SumaCompleto()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-08-18 08:00", "2026-08-18 08:30", 1, 0)
            });

            Assert.IsTrue(r.Calculado);
            Assert.AreEqual(30, r.DePersona(Persona).MinutosSinAprobar50);
            Assert.AreEqual(1, r.Detalle.Count);
            Assert.AreEqual("08:00 - 08:30", r.Detalle[0].FueraTexto);
            Assert.AreEqual("Sin solicitar", r.Detalle[0].EstadoTexto);
        }

        [TestMethod]
        public void PagadoQueCubreTodo_NoSuma()
        {
            // El caso de un tramo pagado de 08:00 a 08:30 dentro de uno normal de
            // 08:00 a 17:30: el segmento fuera de horario del normal ya esta pagado.
            EntHeFueraDeHorario r = Calcular(
                new List<EntHeSegmentoFuera> { Seg(1, Persona, "2026-09-14 08:00", "2026-09-14 08:30", 1, 0) },
                new List<EntHeIntervaloPagado> { Pagado(Persona, "2026-09-14 08:00", "2026-09-14 08:30") });

            Assert.IsNull(r.DePersona(Persona), "Una persona sin nada pendiente no deberia aparecer.");
            Assert.AreEqual(0, r.Detalle.Count);
        }

        [TestMethod]
        public void PagadoParcial_DescuentaSoloLaSuperposicion()
        {
            EntHeFueraDeHorario r = Calcular(
                new List<EntHeSegmentoFuera> { Seg(1, Persona, "2026-08-18 17:30", "2026-08-18 18:30", 1, 0) },
                new List<EntHeIntervaloPagado> { Pagado(Persona, "2026-08-18 18:00", "2026-08-18 19:00") });

            Assert.AreEqual(30, r.DePersona(Persona).MinutosSinAprobar50);
            Assert.AreEqual(1, r.Detalle.Count);
            Assert.AreEqual("17:30 - 18:00", r.Detalle[0].FueraTexto);
            Assert.AreEqual("17:30 - 18:30", r.Detalle[0].TramoTexto, "El tramo registrado se muestra entero.");
        }

        [TestMethod]
        public void PagadoEnMedio_ParteElSegmentoEnDos()
        {
            EntHeFueraDeHorario r = Calcular(
                new List<EntHeSegmentoFuera> { Seg(1, Persona, "2026-08-18 17:30", "2026-08-18 19:30", 1, 0) },
                new List<EntHeIntervaloPagado> { Pagado(Persona, "2026-08-18 18:00", "2026-08-18 19:00") });

            Assert.AreEqual(60, r.DePersona(Persona).MinutosSinAprobar50);
            Assert.AreEqual(2, r.Detalle.Count);
            Assert.AreEqual("17:30 - 18:00", r.Detalle[0].FueraTexto);
            Assert.AreEqual("19:00 - 19:30", r.Detalle[1].FueraTexto);
        }

        [TestMethod]
        public void DosTramosSobreElMismoMinuto_CuentanUnaVez()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-08-18 08:00", "2026-08-18 08:30", 1, 0),
                Seg(2, Persona, "2026-08-18 08:00", "2026-08-18 08:30", 1, 1)
            });

            Assert.AreEqual(30, r.DePersona(Persona).MinutosSinAprobar50);
            Assert.AreEqual(1, r.Detalle.Count);
        }

        [TestMethod]
        public void SinAprobarGanaARechazado()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-08-18 08:15", "2026-08-18 08:45", 1, 3),
                Seg(2, Persona, "2026-08-18 08:00", "2026-08-18 08:30", 1, 0)
            });

            EntHeFueraDeHorarioPersona p = r.DePersona(Persona);
            Assert.AreEqual(30, p.MinutosSinAprobar50);
            Assert.AreEqual(15, p.MinutosRechazado50, "Del rechazado solo queda 08:30-08:45.");
        }

        [TestMethod]
        public void CienGanaACincuentaEnLaMismaCategoria()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-08-18 17:30", "2026-08-18 18:30", 1, 0),
                Seg(2, Persona, "2026-08-18 18:00", "2026-08-18 18:30", 2, 0)
            });

            EntHeFueraDeHorarioPersona p = r.DePersona(Persona);
            Assert.AreEqual(30, p.MinutosSinAprobar100);
            Assert.AreEqual(30, p.MinutosSinAprobar50);
        }

        [TestMethod]
        public void SegmentoDeTramoPagado_SeIgnora()
        {
            // Defensa: el SQL no deberia mandarlo, pero si llega no puede contar como pendiente.
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-08-18 18:00", "2026-08-18 19:00", 1, 2, tipoRegistrado: 1)
            });

            Assert.IsNull(r.DePersona(Persona));
        }

        [TestMethod]
        public void AprobadoSinTipo_CuentaComoSinAprobar()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-08-18 18:00", "2026-08-18 19:00", 1, 2, tipoRegistrado: 0)
            });

            Assert.AreEqual(60, r.DePersona(Persona).MinutosSinAprobar50);
            Assert.AreEqual("Aprobado sin tipo", r.Detalle[0].EstadoTexto);
            Assert.AreEqual(NegHeFueraDeHorario.CategoriaSinAprobar, r.Detalle[0].Categoria);
        }

        [TestMethod]
        public void Rechazado_SeSeparaPorTipo()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-09-05 11:30", "2026-09-05 12:25", 2, 3)
            });

            EntHeFueraDeHorarioPersona p = r.DePersona(Persona);
            Assert.AreEqual(55, p.MinutosRechazado100);
            Assert.AreEqual(0, p.MinutosSinAprobar50 + p.MinutosSinAprobar100);
            Assert.AreEqual("Rechazado", r.Detalle[0].EstadoTexto);
            Assert.AreEqual(1, r.PersonasConRechazado);
            Assert.AreEqual(0, r.PersonasConSinAprobar);
        }

        [TestMethod]
        public void Horas_SeRedondeanUnaSolaVez()
        {
            // Tres pedazos de 20 min: 60 min = 1,00 h. Redondeando cada uno (0,33 x 3) saldria 0,99.
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-08-18 07:00", "2026-08-18 07:20", 1, 0),
                Seg(2, Persona, "2026-08-19 07:00", "2026-08-19 07:20", 1, 0),
                Seg(3, Persona, "2026-08-20 07:00", "2026-08-20 07:20", 1, 0)
            });

            Assert.AreEqual(1.00m, r.DePersona(Persona).HorasSinAprobar);
            Assert.AreEqual(1.00m, r.TotalHorasSinAprobar);
        }

        [TestMethod]
        public void SinSegmentos_Calculado_YVacio()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>());

            Assert.IsTrue(r.Calculado, "Vacio es un resultado valido, no un error.");
            Assert.AreEqual(0, r.Personas.Count);
            Assert.AreEqual(0m, r.TotalHorasSinAprobar);
        }

        [TestMethod]
        public void NulosSeTratanComoListasVacias()
        {
            EntHeFueraDeHorario r = NegHeFueraDeHorario.Calcular(null, null);

            Assert.IsTrue(r.Calculado);
            Assert.AreEqual(0, r.Personas.Count);
        }

        [TestMethod]
        public void PagadoDeOtraPersona_NoDescuenta()
        {
            EntHeFueraDeHorario r = Calcular(
                new List<EntHeSegmentoFuera> { Seg(1, Persona, "2026-08-18 08:00", "2026-08-18 08:30", 1, 0) },
                new List<EntHeIntervaloPagado> { Pagado(OtraPersona, "2026-08-18 08:00", "2026-08-18 08:30") });

            Assert.AreEqual(30, r.DePersona(Persona).MinutosSinAprobar50);
        }

        [TestMethod]
        public void SegmentoVacioOAlReves_SeIgnora()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(1, Persona, "2026-08-18 08:30", "2026-08-18 08:30", 1, 0),
                Seg(2, Persona, "2026-08-18 09:00", "2026-08-18 08:00", 1, 0)
            });

            Assert.IsNull(r.DePersona(Persona));
        }

        [TestMethod]
        public void Detalle_OrdenadoPorEmpleadoYHora()
        {
            EntHeFueraDeHorario r = Calcular(new List<EntHeSegmentoFuera>
            {
                Seg(3, OtraPersona, "2026-08-18 07:00", "2026-08-18 07:30", 1, 0),
                Seg(2, Persona, "2026-08-19 07:00", "2026-08-19 07:30", 1, 0),
                Seg(1, Persona, "2026-08-18 18:00", "2026-08-18 18:30", 1, 3)
            });

            Assert.AreEqual(3, r.Detalle.Count);
            Assert.AreEqual(1L, r.Detalle[0].IdRegDetTareas);
            Assert.AreEqual(2L, r.Detalle[1].IdRegDetTareas);
            Assert.AreEqual(3L, r.Detalle[2].IdRegDetTareas);
            Assert.AreEqual(2, r.Personas.Count);
            Assert.AreEqual(Persona, r.Personas[0].IdEmpleado);
        }
    }
}
```

Agregar a `CapaPruebas/CapaPruebas.csproj`, junto a `<Compile Include="NegHeExportacionTests.cs" />`:

```xml
    <Compile Include="NegHeFueraDeHorarioTests.cs" />
```

- [ ] **Step 3: Compilar para ver que falla**

Desde PowerShell, en la raíz del repositorio:

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo
```

Expected: `error CS0103` (o `CS0246`): `NegHeFueraDeHorario` no existe. Se cuentan los errores, no los cientos de warnings que ya existen.

- [ ] **Step 4: Implementar el cálculo**

Crear `CapaNegocio/NegHeFueraDeHorario.cs`:

```csharp
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Cuenta el tiempo fuera de horario que no se va a pagar, sin base de datos
    /// ni pantalla. Recibe lo que devuelve Sp_RTA_HeFueraDeHorario y decide, minuto
    /// a minuto, en que categoria cae cada uno.
    ///
    /// Por que minuto a minuto y no restando intervalos: los tramos se registran
    /// con precision de minuto (DATEDIFF(MINUTE) en todo el modulo), un periodo son
    /// unos 45.000 minutos por persona como mucho, y un conjunto de minutos
    /// ocupados hace imposible contar dos veces el mismo, que es justo el error que
    /// esta clase existe para evitar. Una resta de intervalos escrita a mano tiene
    /// mas casos borde que pruebas.
    ///
    /// Prioridad de un minuto disputado: pagado > sin aprobar > rechazado, y dentro
    /// de una misma categoria 100% antes que 50%.
    /// </summary>
    public static class NegHeFueraDeHorario
    {
        public const string CategoriaSinAprobar = "SinAprobar";
        public const string CategoriaRechazado = "Rechazado";

        /// <summary>Cualquier fecha anterior a los datos sirve de origen para numerar minutos.</summary>
        private static readonly DateTime Origen = new DateTime(2000, 1, 1);

        public static EntHeFueraDeHorario Calcular(IList<EntHeSegmentoFuera> segmentos, IList<EntHeIntervaloPagado> pagados)
        {
            EntHeFueraDeHorario resultado = new EntHeFueraDeHorario();
            resultado.Calculado = true;

            IList<EntHeSegmentoFuera> todos = segmentos ?? new List<EntHeSegmentoFuera>();
            IList<EntHeIntervaloPagado> yaPagados = pagados ?? new List<EntHeIntervaloPagado>();

            var porPersona = todos
                .Where(s => !EstaPagado(s)
                            && (s.TipoHora == 1 || s.TipoHora == 2)
                            && s.SegmentoFin > s.SegmentoInicio)
                .GroupBy(s => s.IdEmpleado)
                .OrderBy(g => g.Key);

            foreach (var grupo in porPersona)
            {
                HashSet<long> ocupados = new HashSet<long>();

                foreach (EntHeIntervaloPagado p in yaPagados.Where(x => x.IdEmpleado == grupo.Key))
                {
                    for (long m = Minuto(p.Inicio); m < Minuto(p.Fin); m++) { ocupados.Add(m); }
                }

                EntHeFueraDeHorarioPersona persona = new EntHeFueraDeHorarioPersona();
                persona.IdEmpleado = grupo.Key;

                var enOrden = grupo
                    .OrderBy(s => Categoria(s) == CategoriaSinAprobar ? 0 : 1)
                    .ThenByDescending(s => s.TipoHora)
                    .ThenBy(s => s.SegmentoInicio)
                    .ThenBy(s => s.IdRegDetTareas);

                foreach (EntHeSegmentoFuera s in enOrden)
                {
                    long fin = Minuto(s.SegmentoFin);
                    long desde = -1;

                    // Recorre hasta fin INCLUSIVE para cerrar el ultimo pedazo abierto.
                    for (long m = Minuto(s.SegmentoInicio); m <= fin; m++)
                    {
                        bool libre = m < fin && ocupados.Add(m);

                        if (libre && desde < 0) { desde = m; }
                        if (!libre && desde >= 0)
                        {
                            Sumar(resultado, persona, s, desde, m);
                            desde = -1;
                        }
                    }
                }

                if (persona.MinutosSinAprobar50 + persona.MinutosSinAprobar100
                    + persona.MinutosRechazado50 + persona.MinutosRechazado100 > 0)
                {
                    resultado.Personas.Add(persona);
                }
            }

            resultado.Detalle = resultado.Detalle
                .OrderBy(d => d.IdEmpleado)
                .ThenBy(d => d.Inicio)
                .ThenBy(d => d.IdRegDetTareas)
                .ToList();

            return resultado;
        }

        /// <summary>Aprobado con tipo 50% o 100%: ya esta en lo que se paga.</summary>
        private static bool EstaPagado(EntHeSegmentoFuera s)
        {
            return s.EstadoTramo == 2 && (s.TipoRegistrado == 1 || s.TipoRegistrado == 2);
        }

        private static string Categoria(EntHeSegmentoFuera s)
        {
            return s.EstadoTramo == 3 ? CategoriaRechazado : CategoriaSinAprobar;
        }

        private static string EstadoTexto(EntHeSegmentoFuera s)
        {
            switch (s.EstadoTramo)
            {
                case 1: return "Solicitado";
                case 2: return "Aprobado sin tipo";
                case 3: return "Rechazado";
                default: return "Sin solicitar";
            }
        }

        /// <summary>Numero de minuto, truncado: coincide con DATEDIFF(MINUTE) del SQL.</summary>
        private static long Minuto(DateTime t)
        {
            return (long)Math.Floor((t - Origen).TotalMinutes);
        }

        private static void Sumar(EntHeFueraDeHorario resultado, EntHeFueraDeHorarioPersona persona,
                                  EntHeSegmentoFuera s, long desde, long hasta)
        {
            int minutos = (int)(hasta - desde);
            string categoria = Categoria(s);
            bool cien = s.TipoHora == 2;

            if (categoria == CategoriaRechazado)
            {
                if (cien) { persona.MinutosRechazado100 += minutos; } else { persona.MinutosRechazado50 += minutos; }
            }
            else
            {
                if (cien) { persona.MinutosSinAprobar100 += minutos; } else { persona.MinutosSinAprobar50 += minutos; }
            }

            EntHeFueraDeHorarioDetalle d = new EntHeFueraDeHorarioDetalle();
            d.IdEmpleado = s.IdEmpleado;
            d.IdRegDetTareas = s.IdRegDetTareas;
            d.TramoInicio = s.TramoInicio;
            d.TramoFin = s.TramoFin;
            d.Inicio = Origen.AddMinutes(desde);
            d.Fin = Origen.AddMinutes(hasta);
            d.Minutos = minutos;
            d.TipoHora = s.TipoHora;
            d.Categoria = categoria;
            d.EstadoTexto = EstadoTexto(s);
            d.Actividad = s.Actividad ?? "";
            resultado.Detalle.Add(d);
        }
    }
}
```

Agregar a `CapaNegocio/CapaNegocio.csproj`, junto a `<Compile Include="NegHeExportacion.cs" />`:

```xml
    <Compile Include="NegHeFueraDeHorario.cs" />
```

- [ ] **Step 5: Compilar y ejecutar las pruebas**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Expected: 0 errores de compilación. Las 16 pruebas nuevas pasan y **todas las que ya existían siguen pasando**.

- [ ] **Step 6: Commit**

```bash
git add CapaEntidad/EntHeSegmentoFuera.cs CapaEntidad/EntHeIntervaloPagado.cs CapaEntidad/EntHeFueraDeHorario.cs CapaEntidad/CapaEntidad.csproj \
        CapaNegocio/NegHeFueraDeHorario.cs CapaNegocio/CapaNegocio.csproj \
        CapaPruebas/NegHeFueraDeHorarioTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(horas-extras): calculo del tiempo fuera de horario sin aprobar y rechazado

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 3: Lectura desde la base y método de negocio

**Files:**
- Modify: `CapaDato/DaoHorasExtras.cs` (después de `LeerHorasAprobadas`, cerca de la línea 293)
- Modify: `CapaNegocio/NegHorasExtrasPantalla.cs` (después de `CargarPantalla(int idPeriodo, EntHeParametros parametros)`)

**Interfaces:**
- Consumes: `Sp_RTA_HeFueraDeHorario` (Tarea 1), las entidades y `NegHeFueraDeHorario.Calcular` (Tarea 2), `DaoHorasExtras.CargarPeriodo(int)` (ya existe; devuelve `EntHePantalla` con `Periodo` nulo si el período no existe).
- Produces:
  - `static void DaoHorasExtras.LeerFueraDeHorario(DateTime inicio, DateTime fin, out List<EntHeSegmentoFuera> segmentos, out List<EntHeIntervaloPagado> pagados)`
  - `static EntHeFueraDeHorario NegHorasExtrasPantalla.CalcularFueraDeHorario(EntHePeriodo periodo)`: **nunca lanza**; ante un fallo devuelve `Calculado = false` con `Error`.
  - `static EntRespuesta NegHorasExtrasPantalla.CargarFueraDeHorario(int idPeriodo)`: `estado "1"` con `resultado = EntHeFueraDeHorario`, o `estado "0"` si el período no existe.

- [ ] **Step 1: Agregar la lectura en `DaoHorasExtras`**

Después de `LeerHorasAprobadas`:

```csharp
        /// <summary>
        /// Los dos conjuntos de Sp_RTA_HeFueraDeHorario: los segmentos fuera de
        /// horario de los tramos no pagados, y los intervalos pagados. No los
        /// cruza: restar y unir es de NegHeFueraDeHorario, y CapaDato no puede
        /// llamar a CapaNegocio.
        /// </summary>
        public static void LeerFueraDeHorario(DateTime inicio, DateTime fin,
                                              out List<EntHeSegmentoFuera> segmentos,
                                              out List<EntHeIntervaloPagado> pagados)
        {
            segmentos = new List<EntHeSegmentoFuera>();
            pagados = new List<EntHeIntervaloPagado>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeFueraDeHorario", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FechaInicio", SqlDbType.Date).Value = inicio;
                cmd.Parameters.Add("@FechaFin", SqlDbType.Date).Value = fin;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        EntHeSegmentoFuera s = new EntHeSegmentoFuera();
                        s.IdEmpleado = Convert.ToInt64(dr["IdEmpleado"]);
                        s.IdRegDetTareas = Convert.ToInt64(dr["IdRegDetTareas"]);
                        s.TramoInicio = Convert.ToDateTime(dr["TramoInicio"]);
                        s.TramoFin = Convert.ToDateTime(dr["TramoFin"]);
                        s.SegmentoInicio = Convert.ToDateTime(dr["SegmentoInicio"]);
                        s.SegmentoFin = Convert.ToDateTime(dr["SegmentoFin"]);
                        s.TipoHora = Convert.ToInt32(dr["TipoHora"]);
                        s.EstadoTramo = Convert.ToInt32(dr["EstadoTramo"]);
                        s.TipoRegistrado = Convert.ToInt32(dr["TipoRegistrado"]);
                        s.Actividad = Convert.ToString(dr["Actividad"]);
                        segmentos.Add(s);
                    }

                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            EntHeIntervaloPagado p = new EntHeIntervaloPagado();
                            p.IdEmpleado = Convert.ToInt64(dr["IdEmpleado"]);
                            p.Inicio = Convert.ToDateTime(dr["Inicio"]);
                            p.Fin = Convert.ToDateTime(dr["Fin"]);
                            pagados.Add(p);
                        }
                    }
                }
            }
        }
```

- [ ] **Step 2: Agregar los dos métodos en `NegHorasExtrasPantalla`**

Después del segundo `CargarPantalla`:

```csharp
        /// <summary>
        /// El tiempo fuera de horario del rango del periodo, ya depurado.
        ///
        /// NUNCA lanza. La pantalla y el Excel lo tratan como dato informativo: si
        /// falla, lo que se paga tiene que seguir mostrandose y descargandose, y el
        /// fallo se dice con Calculado = false en vez de parecer "no hay nada".
        /// </summary>
        public static EntHeFueraDeHorario CalcularFueraDeHorario(EntHePeriodo periodo)
        {
            try
            {
                if (periodo == null)
                {
                    return new EntHeFueraDeHorario { Calculado = false, Error = "El periodo no existe." };
                }

                List<EntHeSegmentoFuera> segmentos;
                List<EntHeIntervaloPagado> pagados;
                DaoHorasExtras.LeerFueraDeHorario(periodo.FechaInicio, periodo.FechaFin, out segmentos, out pagados);

                return NegHeFueraDeHorario.Calcular(segmentos, pagados);
            }
            catch (Exception ex)
            {
                return new EntHeFueraDeHorario { Calculado = false, Error = ex.Message };
            }
        }

        /// <summary>
        /// Lo que pide la pantalla despues de dibujar la grilla. Se calcula cada vez,
        /// tambien en un periodo cerrado: muestra el estado ACTUAL de los tramos, que
        /// es lo que dice si alguien regularizo algo despues del cierre.
        /// </summary>
        public static EntRespuesta CargarFueraDeHorario(int idPeriodo)
        {
            EntRespuesta respuesta = new EntRespuesta();
            EntHePantalla existente = DaoHorasExtras.CargarPeriodo(idPeriodo);

            if (existente.Periodo == null)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "El periodo no existe.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            respuesta.estado = "1";
            respuesta.resultado = CalcularFueraDeHorario(existente.Periodo);
            return respuesta;
        }
```

Si `NegHorasExtrasPantalla.cs` no tiene todavía `using System.Collections.Generic;`, agregarlo. Ya usa `Dictionary`, así que debería tenerlo.

- [ ] **Step 3: Compilar y ejecutar las pruebas**

Los mismos comandos del paso 5 de la Tarea 2.

Expected: 0 errores y todas las pruebas en verde. No hay pruebas nuevas en esta tarea: los dos métodos solo leen de la base y delegan, y la lógica ya está probada en la Tarea 2. Se validan contra la base real en la Tarea 7.

- [ ] **Step 4: Commit**

```bash
git add CapaDato/DaoHorasExtras.cs CapaNegocio/NegHorasExtrasPantalla.cs
git commit -m "feat(horas-extras): leer y calcular el tiempo fuera de horario de un periodo

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 4: Acción del handler

**Files:**
- Modify: `ReporteTareas/Formulario/AdministrarHorasExtras.ashx.cs` (despacho, cerca de la línea 110; método nuevo después de `CargarPeriodo`, cerca de la 240)

**Interfaces:**
- Consumes: `NegHorasExtrasPantalla.CargarFueraDeHorario(int)` (Tarea 3).
- Produces: acción JSON `CargarFueraDeHorario` con parámetro `idPeriodo`, que devuelve `EntRespuesta` serializada: `resultado` tiene `Calculado`, `Error`, `Personas[]`, `Detalle[]`, `TotalHorasSinAprobar`, `TotalHorasRechazado`, `PersonasConSinAprobar` y `PersonasConRechazado`.

- [ ] **Step 1: Registrar la acción**

Después de la línea de `ReabrirPeriodo`:

```csharp
                if (accion == "CargarFueraDeHorario") { existe = true; salida.Append(CargarFueraDeHorario(parametros[0]["parameters"])); }
```

- [ ] **Step 2: Agregar el método**

Después de `CargarPeriodo`:

```csharp
        /// <summary>
        /// El tiempo fuera de horario del periodo. Se pide aparte de CargarPeriodo,
        /// despues de dibujar la grilla: es informativo, y si tarda o falla la
        /// pantalla tiene que seguir funcionando igual que antes.
        ///
        /// La actividad trae tildes desde la base. ProcessRequest ya fija
        /// ContentEncoding UTF-8 para todo el handler, asi que no hace falta
        /// repetirlo aqui.
        /// </summary>
        private string CargarFueraDeHorario(dynamic p)
        {
            try
            {
                EntRespuesta r = NegHorasExtrasPantalla.CargarFueraDeHorario(Entero(p["idPeriodo"]));
                return new JavaScriptSerializer().Serialize(r);
            }
            catch (Exception ex)
            {
                return Mensaje("0", "Error al calcular el tiempo fuera de horario. " + ex.Message, "danger");
            }
        }
```

- [ ] **Step 3: Compilar**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo
```

Expected: 0 errores.

- [ ] **Step 4: Commit**

```bash
git add ReporteTareas/Formulario/AdministrarHorasExtras.ashx.cs
git commit -m "feat(horas-extras): accion CargarFueraDeHorario en el handler

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 5: Pantalla — columnas, ventana de detalle y aviso al cerrar

**Files:**
- Modify: `ReporteTareas/Formulario/HorasExtras.aspx` (línea 9, `?v=7` → `?v=8`; encabezado 289-295; `colspan` 297; pie 305-321; ventana nueva después de la línea 372)
- Modify: `ReporteTareas/js/horasExtras.js` (`ConfirmarCerrarPeriodo` línea 245; final de `PintarPantalla` línea 501; `colspan` de `PintarGrilla` línea 561; `ConstruirFila` línea 657; bloque nuevo después de `ActualizarTotales`, línea 1062)

**Interfaces:**
- Consumes: acción `CargarFueraDeHorario` (Tarea 4), `PostHE`, `MostrarConfirmacion`, `FormatoDosDecimales`, `_idPeriodoActual`.
- Produces: `CargarFueraDeHorario()`, `PintarFueraDeHorario()`, `MostrarDetalleFuera(idEmpleado)` y `TextoAvisoFueraDeHorario()`.

- [ ] **Step 1: El encabezado, el `colspan` y el pie en `HorasExtras.aspx`**

Reemplazar

```html
                                        <th>Total HE (USD)</th>
                                        <th>Observación</th>
```

por

```html
                                        <th>Total HE (USD)</th>
                                        <!-- Informativas: tiempo registrado fuera del horario de la
                                             persona que NO entra en el pago. Las llena
                                             CargarFueraDeHorario después de dibujar la grilla. -->
                                        <th title="Tiempo fuera de horario sin solicitar, pendiente de aprobación o aprobado sin tipo. No se paga.">Fuera de horario sin aprobar (h)</th>
                                        <th title="Tiempo fuera de horario que la jefatura rechazó. No se paga.">Rechazado (h)</th>
                                        <th>Observación</th>
```

En el `tbody` inicial, `<td colspan="16"` pasa a `<td colspan="18"`.

En el `tfoot`, reemplazar

```html
                                        <td id="pieTotalHE">USD 0.00</td>
                                        <td></td>
                                        <td></td>
```

por

```html
                                        <td id="pieTotalHE">USD 0.00</td>
                                        <td id="pieFueraSinAprobar"></td>
                                        <td id="pieFueraRechazado"></td>
                                        <td></td>
                                        <td></td>
```

Línea 9: `horasExtras.js?v=7` → `horasExtras.js?v=8`.

- [ ] **Step 2: La ventana de detalle en `HorasExtras.aspx`**

Después del cierre `<!-- /.modal -->` de `modalConfirmarPegado` (cerca de la línea 372):

```html
        <!-- Detalle del tiempo fuera de horario de un colaborador. Solo lectura:
             sirve para saber QUÉ tramos pedirle a la persona que solicite, o cuáles
             rechazó la jefatura. Lo llena MostrarDetalleFuera en horasExtras.js,
             siempre con .text(): la actividad es texto libre de la persona. -->
        <div class="modal fade" id="modalFueraDeHorario" tabindex="-1" role="dialog" aria-labelledby="tituloFueraDeHorario" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="tituloFueraDeHorario">Tiempo fuera de horario</h4>
                    </div>
                    <div class="modal-body">
                        <p class="text-muted">Información para revisión: estas horas NO están incluidas en los totales a pagar.</p>
                        <div class="table-responsive">
                            <table class="table table-condensed table-bordered">
                                <thead>
                                    <tr>
                                        <th>Fecha</th>
                                        <th>Tramo registrado</th>
                                        <th>Fuera de horario</th>
                                        <th>Min.</th>
                                        <th>Tipo</th>
                                        <th>Estado</th>
                                        <th>Actividad</th>
                                    </tr>
                                </thead>
                                <tbody id="cuerpoFueraDeHorario"></tbody>
                            </table>
                        </div>
                        <p id="pieFueraDeHorario"></p>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->
```

- [ ] **Step 3: Comprobar el BOM de la página**

```bash
head -c 3 ReporteTareas/Formulario/HorasExtras.aspx | od -c | head -1
```

Expected: `357 273 277`. Si la edición lo quitó, se restaura antes de seguir:

```bash
printf '\xef\xbb\xbf' | cat - ReporteTareas/Formulario/HorasExtras.aspx > /tmp/he.aspx && mv /tmp/he.aspx ReporteTareas/Formulario/HorasExtras.aspx
```

- [ ] **Step 4: Las celdas de cada fila y el `colspan` en `horasExtras.js`**

En `PintarGrilla`, `colspan="16"` pasa a `colspan="18"`.

En `ConstruirFila`, después de

```js
    $fila.append($('<td class="text-right he-total-he"></td>').text(FormatoDosDecimales(fila.TotalHE)));
```

agregar

```js
    /* Informativas: las llena PintarFueraDeHorario cuando llega la respuesta. */
    $fila.append($('<td class="text-right he-fuera-sin"></td>').text("…"));
    $fila.append($('<td class="text-right he-fuera-rech"></td>').text("…"));
```

- [ ] **Step 5: Pedir el cálculo al pintar la pantalla**

Al final de `PintarPantalla`, después de `PintarGrilla(pantalla);`:

```js
    CargarFueraDeHorario();
```

`PintarPantalla` también se ejecuta al guardar, cerrar y reabrir, así que las columnas se recalculan con el estado actual en cada uno de esos casos.

- [ ] **Step 6: El bloque de fuera de horario**

Después de `ActualizarTotales`, antes del comentario `/* --------------------------------------------------------- guardar todo -- */`:

```js
/* ------------------------------------------------------ fuera de horario -- */

/* Tiempo registrado fuera del horario de cada persona que NO entra en el pago:
   sin solicitar, pendiente o aprobado sin tipo («sin aprobar»), y lo que la
   jefatura rechazó. Es información para que Nómina decida antes de cerrar;
   nada de esto cambia un monto.

   Se pide aparte de la grilla, y si falla la grilla sigue igual: las dos
   columnas dicen «no disponible» y nada más. */
var _fueraDeHorario = null;
var _idPeriodoFuera = null;
var _estadoFuera = "sin-datos";   // "cargando" | "ok" | "error"

function CargarFueraDeHorario() {
    var idPeriodo = _idPeriodoActual;
    if (!idPeriodo) { return; }

    _idPeriodoFuera = idPeriodo;
    _fueraDeHorario = null;
    _estadoFuera = "cargando";
    PintarFueraDeHorario();

    /* suprimirMensaje: un fallo aquí no merece un modal encima de la grilla,
       que sí cargó bien. Se dice en las propias columnas. */
    PostHE("CargarFueraDeHorario", { idPeriodo: idPeriodo }, function (r) {
        /* Si entretanto se eligió otro período, esta respuesta ya no es de él. */
        if (idPeriodo !== _idPeriodoActual) { return; }
        _fueraDeHorario = r.resultado;
        _estadoFuera = (r.resultado && r.resultado.Calculado) ? "ok" : "error";
        PintarFueraDeHorario();
    }, function () {
        if (idPeriodo !== _idPeriodoActual) { return; }
        _estadoFuera = "error";
        PintarFueraDeHorario();
    }, true);
}

function PintarFueraDeHorario() {
    var porEmpleado = {};
    if (_estadoFuera === "ok") {
        $.each(_fueraDeHorario.Personas || [], function (i, p) { porEmpleado[String(p.IdEmpleado)] = p; });
    }

    $("#cuerpoHE tr[data-empleado]").each(function () {
        var $fila = $(this);
        var idEmpleado = $fila.attr("data-empleado");
        var $sin = $fila.find(".he-fuera-sin").empty();
        var $rech = $fila.find(".he-fuera-rech").empty();

        if (_estadoFuera === "cargando") { $sin.text("…"); $rech.text("…"); return; }
        if (_estadoFuera !== "ok") { $sin.text("no disponible"); $rech.text("no disponible"); return; }

        var p = porEmpleado[idEmpleado];
        $sin.append(EnlaceFuera(p ? p.HorasSinAprobar : 0, idEmpleado));
        $rech.append(EnlaceFuera(p ? p.HorasRechazado : 0, idEmpleado));
    });

    if (_estadoFuera === "ok") {
        $("#pieFueraSinAprobar").text(FormatoDosDecimales(_fueraDeHorario.TotalHorasSinAprobar));
        $("#pieFueraRechazado").text(FormatoDosDecimales(_fueraDeHorario.TotalHorasRechazado));
    } else {
        $("#pieFueraSinAprobar, #pieFueraRechazado").text("");
    }
}

/* Con cero, un guion; con horas, un enlace que abre el detalle de la persona. */
function EnlaceFuera(horas, idEmpleado) {
    if (!Number(horas)) { return $('<span class="text-muted"></span>').text("—"); }

    return $('<a href="javascript:void(0)"></a>')
        .text(FormatoDosDecimales(horas))
        .on("click", function () { MostrarDetalleFuera(idEmpleado); });
}

function MostrarDetalleFuera(idEmpleado) {
    if (_estadoFuera !== "ok") { return; }

    var nombre = $("#cuerpoHE tr[data-empleado='" + idEmpleado + "'] td:first").text();
    $("#tituloFueraDeHorario").text("Tiempo fuera de horario — " + nombre);

    var $cuerpo = $("#cuerpoFueraDeHorario").empty();
    $.each(_fueraDeHorario.Detalle || [], function (i, d) {
        if (String(d.IdEmpleado) !== String(idEmpleado)) { return; }

        /* Todo con .text(): la actividad es texto libre y nunca se interpreta como HTML. */
        $cuerpo.append($("<tr></tr>")
            .append($("<td></td>").text(d.Fecha))
            .append($("<td></td>").text(d.TramoTexto))
            .append($("<td></td>").text(d.FueraTexto))
            .append($('<td class="text-right"></td>').text(d.Minutos))
            .append($("<td></td>").text(d.TipoTexto))
            .append($("<td></td>").text(d.EstadoTexto))
            .append($("<td></td>").text(d.Actividad)));
    });

    var p = null;
    $.each(_fueraDeHorario.Personas || [], function (i, x) { if (String(x.IdEmpleado) === String(idEmpleado)) { p = x; } });

    $("#pieFueraDeHorario").text(p
        ? "Sin aprobar: 50% " + FormatoDosDecimales(p.MinutosSinAprobar50 / 60) + " h · 100% " +
          FormatoDosDecimales(p.MinutosSinAprobar100 / 60) + " h. Rechazado: 50% " +
          FormatoDosDecimales(p.MinutosRechazado50 / 60) + " h · 100% " +
          FormatoDosDecimales(p.MinutosRechazado100 / 60) + " h."
        : "");

    $("#modalFueraDeHorario").modal("show");
}

/* La línea que se agrega a la confirmación de cierre. Nunca bloquea: informa. */
function TextoAvisoFueraDeHorario() {
    if (_estadoFuera !== "ok" || _idPeriodoFuera !== _idPeriodoActual) {
        return "No se pudo verificar el tiempo fuera de horario.";
    }

    var f = _fueraDeHorario;
    if (f.PersonasConSinAprobar === 0 && f.PersonasConRechazado === 0) {
        return "No hay tiempo fuera de horario sin aprobar ni rechazado en este período.";
    }

    return "Hay " + f.PersonasConSinAprobar + " colaborador(es) con " +
           FormatoDosDecimales(f.TotalHorasSinAprobar) + " h fuera de horario sin aprobar y " +
           f.PersonasConRechazado + " con " + FormatoDosDecimales(f.TotalHorasRechazado) +
           " h rechazadas. Esas horas no están incluidas en el pago.";
}
```

- [ ] **Step 7: El aviso en la confirmación de cierre**

En `ConfirmarCerrarPeriodo`, reemplazar

```js
    MostrarConfirmacion(
        "El período dejará de admitir cambios: nadie podrá guardar horas ni observaciones hasta que se reabra.",
        EjecutarCerrarPeriodo,
```

por

```js
    var $contenido = $("<div></div>")
        .append($("<p></p>").text("El período dejará de admitir cambios: nadie podrá guardar horas ni observaciones hasta que se reabra."))
        .append($("<p></p>").text(TextoAvisoFueraDeHorario()));

    MostrarConfirmacion(
        $contenido,
        EjecutarCerrarPeriodo,
```

`MostrarConfirmacion` ya acepta un elemento en lugar de un texto (`else { $cuerpo.append(contenido); }`).

- [ ] **Step 8: Compilar**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo
```

Expected: 0 errores.

- [ ] **Step 9: Prueba manual en la aplicación local**

Estas pruebas se hacen en la Tarea 7, cuando el procedimiento ya existe en una base de pruebas o, con la aprobación del usuario, en producción. Se dejan escritas aquí porque son de esta tarea:

1. Al abrir el período 16-08 / 15-09 aparecen las dos columnas con «…» y enseguida con valores. La del caso tipo muestra **1,00**.
2. El enlace abre la ventana con dos filas de 30 minutos del 18-08, «Sin solicitar».
3. **Cambiar de período mientras carga:** con la red lenta del navegador activada (DevTools → Network → Slow 3G), elegir un período y enseguida otro. Las columnas terminan con los datos del segundo.
4. **Guardar una fila:** después de guardar, las columnas vuelven a mostrar valores y el encabezado sigue alineado, también con las columnas de detalle visibles.
5. **Actividad con caracteres especiales:** en la ventana, una actividad con tildes se ve bien y una con `<b>` se ve literal.
6. Con el procedimiento renombrado o sin permisos (en una base de pruebas), las columnas dicen «no disponible» y la grilla funciona igual.
7. «Cerrar período» muestra la línea de aviso con las cifras. Después de confirmar, el período se cierra: el aviso no bloquea.

- [ ] **Step 10: Commit**

```bash
git add ReporteTareas/Formulario/HorasExtras.aspx ReporteTareas/js/horasExtras.js
git commit -m "feat(horas-extras): columnas y detalle del tiempo fuera de horario en la pantalla

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 6: Excel — columnas en el detalle y hoja «Fuera de horario» (TDD)

**Files:**
- Modify: `CapaNegocio/NegHeExportacion.cs`
- Modify: `ReporteTareas/Formulario/DescargarHorasExtras.ashx.cs` (línea 84)
- Test: `CapaPruebas/NegHeExportacionTests.cs`

**Interfaces:**
- Consumes: `EntHeFueraDeHorario` (Tarea 2), `NegHorasExtrasPantalla.CalcularFueraDeHorario(EntHePeriodo)` (Tarea 3).
- Produces:
  - `static byte[] NegHeExportacion.Construir(EntHePantalla pantalla, EntHeFueraDeHorario fuera)`. `Construir(pantalla)` delega con `fuera = null`, que significa «no calculado».
  - `const string NombreHojaFueraDeHorario = "Fuera de horario"`, `const int ColumnaFueraSinAprobar = 24`, `const int ColumnaFueraRechazado = 25`, `const int FueraPrimeraFilaDeDatos = 5`.

- [ ] **Step 1: Actualizar la prueba que cuenta hojas y escribir las nuevas**

En `NegHeExportacionTests.cs`, cambiar

```csharp
                Assert.AreEqual(2, paquete.Workbook.Worksheets.Count, "El archivo deberia tener dos hojas: detalle y pagos.");
```

por

```csharp
                Assert.AreEqual(3, paquete.Workbook.Worksheets.Count, "El archivo deberia tener tres hojas: detalle, pagos y fuera de horario.");
```

Agregar al final de la clase, antes de la última llave (`PantallaDeEjemplo` y `AbrirPaquete` ya existen en la clase; `PantallaDeEjemplo` trae una fila con `IdEmpleado = 4242` y `NombreSnapshot = "PERSONA DE PRUEBA"`):

```csharp
        private static EntHeFueraDeHorario FueraDeEjemplo(long idEmpleado)
        {
            EntHeFueraDeHorario f = new EntHeFueraDeHorario();
            f.Calculado = true;
            f.Personas.Add(new EntHeFueraDeHorarioPersona
            {
                IdEmpleado = idEmpleado,
                MinutosSinAprobar50 = 60,
                MinutosRechazado100 = 30
            });
            f.Detalle.Add(new EntHeFueraDeHorarioDetalle
            {
                IdEmpleado = idEmpleado,
                IdRegDetTareas = 555,
                TramoInicio = new DateTime(2026, 8, 18, 8, 0, 0),
                TramoFin = new DateTime(2026, 8, 18, 9, 0, 0),
                Inicio = new DateTime(2026, 8, 18, 8, 0, 0),
                Fin = new DateTime(2026, 8, 18, 9, 0, 0),
                Minutos = 60,
                TipoHora = 1,
                Categoria = NegHeFueraDeHorario.CategoriaSinAprobar,
                EstadoTexto = "Sin solicitar",
                Actividad = "ACTIVIDAD DE PRUEBA"
            });
            return f;
        }

        [TestMethod]
        public void Construir_ConFueraDeHorario_ColumnasSoloEnElDetalle()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla, FueraDeEjemplo(4242))))
            {
                ExcelWorksheet detalle = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaDetalle];
                ExcelWorksheet pagos = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaPagos];

                Assert.AreEqual("Fuera de horario sin aprobar (h)",
                    Convert.ToString(detalle.Cells[6, NegHeExportacion.ColumnaFueraSinAprobar].Value));
                Assert.AreEqual(1.00m,
                    Convert.ToDecimal(detalle.Cells[NegHeExportacion.PrimeraFilaDeDatos, NegHeExportacion.ColumnaFueraSinAprobar].Value));
                Assert.AreEqual(0.50m,
                    Convert.ToDecimal(detalle.Cells[NegHeExportacion.PrimeraFilaDeDatos, NegHeExportacion.ColumnaFueraRechazado].Value));

                Assert.IsNull(pagos.Cells[6, NegHeExportacion.ColumnaFueraSinAprobar].Value,
                    "La hoja de pagos no lleva las columnas informativas: podrian sumarse al rol por error.");
            }
        }

        [TestMethod]
        public void Construir_ConFueraDeHorario_HojaConElDetalle()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla, FueraDeEjemplo(4242))))
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaFueraDeHorario];
                Assert.IsNotNull(hoja);

                int fila = NegHeExportacion.FueraPrimeraFilaDeDatos;
                Assert.AreEqual("PERSONA DE PRUEBA", Convert.ToString(hoja.Cells[fila, 2].Value));
                Assert.AreEqual("08:00 - 09:00", Convert.ToString(hoja.Cells[fila, 5].Value));
                Assert.AreEqual("Sin solicitar", Convert.ToString(hoja.Cells[fila, 9].Value));
                Assert.AreEqual(555L, Convert.ToInt64(hoja.Cells[fila, 11].Value));
            }
        }

        [TestMethod]
        public void Construir_SinCalculo_HojaConMensaje()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla)))
            {
                Assert.AreEqual(3, paquete.Workbook.Worksheets.Count);

                ExcelWorksheet hoja = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaFueraDeHorario];
                StringAssert.StartsWith(
                    Convert.ToString(hoja.Cells[NegHeExportacion.FueraPrimeraFilaDeDatos, 1].Value),
                    "No se pudo calcular el tiempo fuera de horario");

                ExcelWorksheet detalle = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaDetalle];
                Assert.IsNull(detalle.Cells[NegHeExportacion.PrimeraFilaDeDatos, NegHeExportacion.ColumnaFueraSinAprobar].Value,
                    "Sin calculo las columnas quedan vacias, no en cero: cero diria que no hay nada.");
            }
        }

        [TestMethod]
        public void Construir_CalculoConError_DiceElMotivo()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");
            EntHeFueraDeHorario fallido = new EntHeFueraDeHorario { Calculado = false, Error = "timeout" };

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla, fallido)))
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaFueraDeHorario];
                StringAssert.Contains(Convert.ToString(hoja.Cells[NegHeExportacion.FueraPrimeraFilaDeDatos, 1].Value), "timeout");
            }
        }

        [TestMethod]
        public void Construir_PersonaFueraDeLaGrilla_NoFalla()
        {
            EntHePantalla pantalla = PantallaDeEjemplo("Abierto");

            using (ExcelPackage paquete = AbrirPaquete(NegHeExportacion.Construir(pantalla, FueraDeEjemplo(9999))))
            {
                ExcelWorksheet hoja = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaFueraDeHorario];
                Assert.AreEqual(9999L, Convert.ToInt64(hoja.Cells[NegHeExportacion.FueraPrimeraFilaDeDatos, 1].Value));
                Assert.AreEqual("", Convert.ToString(hoja.Cells[NegHeExportacion.FueraPrimeraFilaDeDatos, 2].Value ?? ""));

                ExcelWorksheet detalle = paquete.Workbook.Worksheets[NegHeExportacion.NombreHojaDetalle];
                Assert.AreEqual(0m,
                    Convert.ToDecimal(detalle.Cells[NegHeExportacion.PrimeraFilaDeDatos, NegHeExportacion.ColumnaFueraSinAprobar].Value),
                    "La persona de la grilla no tiene nada fuera de horario.");
            }
        }
```

Si `AbrirPaquete` devuelve un paquete que no se puede usar en un `using`, se sigue el mismo patrón que ya usa la prueba de la línea 361, que sí lo hace.

- [ ] **Step 2: Compilar para ver que falla**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo
```

Expected: errores CS0117 (`NombreHojaFueraDeHorario`, `ColumnaFueraSinAprobar`, … no existen) y CS1501 (`Construir` no acepta 2 argumentos).

- [ ] **Step 3: Implementar en `NegHeExportacion`**

3a. Junto a las constantes públicas, después de `ColumnaValorHoraOrdinaria`:

```csharp
        public const string NombreHojaFueraDeHorario = "Fuera de horario";

        /// <summary>Informativas, solo en la hoja de detalle: van DESPUES de la ultima columna comun para no mover ninguna.</summary>
        public const int ColumnaFueraSinAprobar = 24;
        public const int ColumnaFueraRechazado = 25;

        /// <summary>Primera fila de datos de la hoja "Fuera de horario" (encabezados en la 4).</summary>
        public const int FueraPrimeraFilaDeDatos = 5;
        private const int FueraFilaDeEncabezados = 4;
        private const int FueraUltimaColumna = 11;
```

3b. Reemplazar la firma de `Construir` y su primera línea:

```csharp
        public static byte[] Construir(EntHePantalla pantalla)
        {
            return Construir(pantalla, null);
        }

        /// <summary>
        /// fuera = null o Calculado = false: el archivo sale igual, con las dos
        /// columnas informativas vacias y la hoja "Fuera de horario" con el motivo.
        /// Lo que se paga no puede quedar bloqueado por un dato informativo.
        /// </summary>
        public static byte[] Construir(EntHePantalla pantalla, EntHeFueraDeHorario fuera)
        {
```

El cuerpo sigue igual hasta `AjustarColumnas(hojaDetalle);`. Justo después de esa línea se agrega:

```csharp
                EscribirColumnasFueraDeHorario(hojaDetalle, pantalla.Filas, fuera, proximaFilaDetalle);
```

Y justo antes de `return paquete.GetAsByteArray();`:

```csharp
                EscribirHojaFueraDeHorario(paquete, pantalla, fuera);
```

3c. Los métodos nuevos, antes de `ConfigurarVista`:

```csharp
        private static bool HayCalculo(EntHeFueraDeHorario fuera)
        {
            return fuera != null && fuera.Calculado;
        }

        /// <summary>
        /// Las dos columnas informativas de la hoja de detalle, en el mismo orden
        /// de filas que EscribirFilasEnHoja. Sin calculo quedan vacias: un cero
        /// diria "no hay nada", que no es lo que se sabe.
        /// </summary>
        private static void EscribirColumnasFueraDeHorario(ExcelWorksheet hoja, IList<EntHeFila> filas,
                                                           EntHeFueraDeHorario fuera, int filaDeTotales)
        {
            EncabezadoFuera(hoja.Cells[FilaDeEncabezados, ColumnaFueraSinAprobar], "Fuera de horario sin aprobar (h)");
            EncabezadoFuera(hoja.Cells[FilaDeEncabezados, ColumnaFueraRechazado], "Rechazado (h)");

            if (HayCalculo(fuera))
            {
                int fila = PrimeraFilaDeDatos;
                foreach (EntHeFila f in filas)
                {
                    EntHeFueraDeHorarioPersona p = fuera.DePersona(f.IdEmpleado);
                    HorasEnCelda(hoja.Cells[fila, ColumnaFueraSinAprobar], p != null ? p.HorasSinAprobar : 0m);
                    HorasEnCelda(hoja.Cells[fila, ColumnaFueraRechazado], p != null ? p.HorasRechazado : 0m);
                    fila++;
                }

                HorasEnCelda(hoja.Cells[filaDeTotales + 1, ColumnaFueraSinAprobar], fuera.TotalHorasSinAprobar);
                HorasEnCelda(hoja.Cells[filaDeTotales + 1, ColumnaFueraRechazado], fuera.TotalHorasRechazado);
                hoja.Cells[filaDeTotales + 1, ColumnaFueraSinAprobar].Style.Font.Bold = true;
                hoja.Cells[filaDeTotales + 1, ColumnaFueraRechazado].Style.Font.Bold = true;
            }

            hoja.Column(ColumnaFueraSinAprobar).Width = 18;
            hoja.Column(ColumnaFueraRechazado).Width = 14;

            // El autofiltro comun cubre 1..23; en el detalle se extiende a las dos nuevas.
            int ultima = filas.Count > 0 ? PrimeraFilaDeDatos + filas.Count - 1 : FilaDeEncabezados;
            hoja.Cells[FilaDeEncabezados, 1, ultima, ColumnaFueraRechazado].AutoFilter = true;
        }

        private static void EncabezadoFuera(ExcelRange celda, string texto)
        {
            celda.Value = texto;
            celda.Style.Font.Bold = true;
        }

        private static void HorasEnCelda(ExcelRange celda, decimal horas)
        {
            celda.Value = horas;
            celda.Style.Numberformat.Format = FormatoHoras;
        }

        /// <summary>
        /// Una fila por cada pedazo que suma, de todas las personas, ordenada por
        /// nombre y fecha. El nombre sale de la grilla del periodo; si la persona
        /// no esta en ella, queda vacio en vez de fallar.
        /// </summary>
        private static void EscribirHojaFueraDeHorario(ExcelPackage paquete, EntHePantalla pantalla, EntHeFueraDeHorario fuera)
        {
            ExcelWorksheet hoja = paquete.Workbook.Worksheets.Add(NombreHojaFueraDeHorario);

            hoja.Cells[1, 1].Value = "Tiempo registrado fuera de horario - Período " + TextoPeriodo(pantalla.Periodo);
            hoja.Cells[1, 1].Style.Font.Bold = true;
            hoja.Cells[2, 1].Value = "Información para revisión: estas horas NO están incluidas en los totales a pagar.";
            hoja.Cells[2, 1].Style.Font.Italic = true;

            string[] encabezados =
            {
                "ID Empleado", "Nombre", "Fecha", "Tramo registrado", "Fuera de horario",
                "Minutos", "Horas", "Tipo", "Estado", "Actividad", "Id tramo"
            };
            for (int i = 0; i < encabezados.Length; i++)
            {
                hoja.Cells[FueraFilaDeEncabezados, i + 1].Value = encabezados[i];
                hoja.Cells[FueraFilaDeEncabezados, i + 1].Style.Font.Bold = true;
            }

            if (!HayCalculo(fuera))
            {
                string motivo = fuera == null || string.IsNullOrEmpty(fuera.Error) ? "no se consultó." : fuera.Error;
                hoja.Cells[FueraPrimeraFilaDeDatos, 1].Value = "No se pudo calcular el tiempo fuera de horario: " + motivo;
                return;
            }

            Dictionary<long, string> nombres = new Dictionary<long, string>();
            foreach (EntHeFila f in pantalla.Filas) { nombres[f.IdEmpleado] = f.NombreSnapshot ?? ""; }

            List<EntHeFueraDeHorarioDetalle> detalle = new List<EntHeFueraDeHorarioDetalle>(fuera.Detalle);
            detalle.Sort((a, b) =>
            {
                string na = nombres.ContainsKey(a.IdEmpleado) ? nombres[a.IdEmpleado] : "";
                string nb = nombres.ContainsKey(b.IdEmpleado) ? nombres[b.IdEmpleado] : "";
                int c = string.Compare(na, nb, StringComparison.OrdinalIgnoreCase);
                if (c != 0) { return c; }
                c = a.IdEmpleado.CompareTo(b.IdEmpleado);
                return c != 0 ? c : a.Inicio.CompareTo(b.Inicio);
            });

            int fila = FueraPrimeraFilaDeDatos;
            foreach (EntHeFueraDeHorarioDetalle d in detalle)
            {
                hoja.Cells[fila, 1].Value = d.IdEmpleado;
                hoja.Cells[fila, 1].Style.Numberformat.Format = FormatoEntero;
                hoja.Cells[fila, 2].Value = nombres.ContainsKey(d.IdEmpleado) ? nombres[d.IdEmpleado] : "";
                hoja.Cells[fila, 3].Value = d.Fecha;
                hoja.Cells[fila, 4].Value = d.TramoTexto;
                hoja.Cells[fila, 5].Value = d.FueraTexto;
                hoja.Cells[fila, 6].Value = d.Minutos;
                hoja.Cells[fila, 6].Style.Numberformat.Format = FormatoEntero;
                HorasEnCelda(hoja.Cells[fila, 7], d.Horas);
                hoja.Cells[fila, 8].Value = d.TipoTexto;
                hoja.Cells[fila, 9].Value = d.EstadoTexto;
                hoja.Cells[fila, 10].Value = d.Actividad;
                hoja.Cells[fila, 11].Value = d.IdRegDetTareas;
                hoja.Cells[fila, 11].Style.Numberformat.Format = FormatoEntero;
                fila++;
            }

            // Totales por categoria y tipo, sumados en minutos y redondeados una vez.
            int sin50 = 0, sin100 = 0, rech50 = 0, rech100 = 0;
            foreach (EntHeFueraDeHorarioPersona p in fuera.Personas)
            {
                sin50 += p.MinutosSinAprobar50; sin100 += p.MinutosSinAprobar100;
                rech50 += p.MinutosRechazado50; rech100 += p.MinutosRechazado100;
            }

            fila++;
            fila = LineaDeTotal(hoja, fila, "Total sin aprobar 50%", sin50);
            fila = LineaDeTotal(hoja, fila, "Total sin aprobar 100%", sin100);
            fila = LineaDeTotal(hoja, fila, "Total rechazado 50%", rech50);
            LineaDeTotal(hoja, fila, "Total rechazado 100%", rech100);

            hoja.View.FreezePanes(FueraPrimeraFilaDeDatos, 1);
            if (detalle.Count > 0)
            {
                hoja.Cells[FueraFilaDeEncabezados, 1, FueraPrimeraFilaDeDatos + detalle.Count - 1, FueraUltimaColumna].AutoFilter = true;
            }
            for (int c = 1; c <= FueraUltimaColumna; c++) { hoja.Column(c).Width = 14; }
            hoja.Column(2).Width = 30;
            hoja.Column(10).Width = 45;
        }

        private static int LineaDeTotal(ExcelWorksheet hoja, int fila, string etiqueta, int minutos)
        {
            hoja.Cells[fila, 5].Value = etiqueta;
            hoja.Cells[fila, 5].Style.Font.Bold = true;
            hoja.Cells[fila, 6].Value = minutos;
            hoja.Cells[fila, 6].Style.Numberformat.Format = FormatoEntero;
            HorasEnCelda(hoja.Cells[fila, 7], EntHeFueraDeHorarioPersona.Horas(minutos));
            hoja.Cells[fila, 7].Style.Font.Bold = true;
            return fila + 1;
        }
```

- [ ] **Step 4: Pasar el cálculo desde la descarga**

En `DescargarHorasExtras.ashx.cs`, reemplazar

```csharp
                archivo = NegHeExportacion.Construir(pantalla);
```

por

```csharp
                /* Informativo: CalcularFueraDeHorario nunca lanza. Si falla, el
                   archivo sale igual y la tercera hoja dice por que. */
                EntHeFueraDeHorario fuera = NegHorasExtrasPantalla.CalcularFueraDeHorario(pantalla.Periodo);
                archivo = NegHeExportacion.Construir(pantalla, fuera);
```

Comprobar que el archivo ya tiene `using CapaEntidad;`. Si no, agregarlo.

- [ ] **Step 5: Compilar y ejecutar las pruebas**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Expected: 0 errores; todas en verde, incluidas las 5 nuevas y la de tres hojas.

- [ ] **Step 6: Commit**

```bash
git add CapaNegocio/NegHeExportacion.cs CapaPruebas/NegHeExportacionTests.cs ReporteTareas/Formulario/DescargarHorasExtras.ashx.cs
git commit -m "feat(horas-extras): tiempo fuera de horario en el Excel, sin tocar la hoja de pagos

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

---

### Task 7: Validación con los datos reales (sin escribir en producción)

**Files:**
- Create: `Actualizacion/validar-fuera-de-horario.ps1` (ignorado por git: imprime nombres)

**Interfaces:**
- Consumes: el cuerpo SQL de la Tarea 1 (como consulta suelta) y `CapaNegocio.exe` / `CapaEntidad.exe` compilados en la Tarea 6.

Esta tarea ejecuta **la clase real** sobre **los datos reales**, sin crear el procedimiento y sin levantar el sitio. Así los números se comprueban antes de pedir permiso para tocar producción.

- [ ] **Step 1: Escribir el script**

`Actualizacion/validar-fuera-de-horario.ps1`:

```powershell
# Solo lectura. Ejecuta NegHeFueraDeHorario.Calcular sobre el periodo 16-08 / 15-09-2026
# con los datos de produccion, sin crear Sp_RTA_HeFueraDeHorario.
$ErrorActionPreference = "Stop"
$raiz = "C:\respaldodisco\Desarrollo\PRY_Sistema ReporteTareas"

$cfg = Get-Content "$raiz\ReporteTareas\connections.config" -Raw
$cs = [regex]::Match($cfg, 'connectionString="([^"]*)"').Groups[1].Value

[Reflection.Assembly]::LoadFrom("$raiz\CapaNegocio\bin\Debug\CapaEntidad.exe") | Out-Null
[Reflection.Assembly]::LoadFrom("$raiz\CapaNegocio\bin\Debug\CapaNegocio.exe") | Out-Null

# El cuerpo de Sp_RTA_HeFueraDeHorario, copiado de docs/sql/2026-09-28-horas-extras-fuera-de-horario.sql.
$sqlArchivo = Get-Content "$raiz\docs\sql\2026-09-28-horas-extras-fuera-de-horario.sql" -Raw
$cuerpo = [regex]::Match($sqlArchivo, '(?s)SET NOCOUNT ON;\s*(/\* 1\..*?)\r?\nEND\r?\nGO').Groups[1].Value
if (-not $cuerpo) { throw "No se encontro el cuerpo del procedimiento en el script." }
$consulta = "DECLARE @FechaInicio DATE = '2026-08-16', @FechaFin DATE = '2026-09-15';`n" + $cuerpo

$cn = New-Object System.Data.SqlClient.SqlConnection $cs
$cn.Open()
$cmd = $cn.CreateCommand(); $cmd.CommandText = $consulta
$dr = $cmd.ExecuteReader()

$segmentos = New-Object 'System.Collections.Generic.List[CapaEntidad.EntHeSegmentoFuera]'
while ($dr.Read()) {
    $s = New-Object CapaEntidad.EntHeSegmentoFuera
    $s.IdEmpleado = [long]$dr["IdEmpleado"]; $s.IdRegDetTareas = [long]$dr["IdRegDetTareas"]
    $s.TramoInicio = $dr["TramoInicio"]; $s.TramoFin = $dr["TramoFin"]
    $s.SegmentoInicio = $dr["SegmentoInicio"]; $s.SegmentoFin = $dr["SegmentoFin"]
    $s.TipoHora = [int]$dr["TipoHora"]; $s.EstadoTramo = [int]$dr["EstadoTramo"]
    $s.TipoRegistrado = [int]$dr["TipoRegistrado"]; $s.Actividad = [string]$dr["Actividad"]
    $segmentos.Add($s)
}
$pagados = New-Object 'System.Collections.Generic.List[CapaEntidad.EntHeIntervaloPagado]'
if ($dr.NextResult()) {
    while ($dr.Read()) {
        $p = New-Object CapaEntidad.EntHeIntervaloPagado
        $p.IdEmpleado = [long]$dr["IdEmpleado"]; $p.Inicio = $dr["Inicio"]; $p.Fin = $dr["Fin"]
        $pagados.Add($p)
    }
}
$dr.Close()

$cmd2 = $cn.CreateCommand(); $cmd2.CommandText = "SELECT IdEmpleado, Nombre FROM dbo.Empleados"
$nombres = @{}; $r2 = $cmd2.ExecuteReader(); while ($r2.Read()) { $nombres[[long]$r2["IdEmpleado"]] = [string]$r2["Nombre"] }; $r2.Close()
$cn.Close()

$t0 = Get-Date
$res = [CapaNegocio.NegHeFueraDeHorario]::Calcular($segmentos, $pagados)
$ms = ((Get-Date) - $t0).TotalMilliseconds

"Segmentos: $($segmentos.Count)  Pagados: $($pagados.Count)  Calculo: $([int]$ms) ms"
"Total sin aprobar: $($res.TotalHorasSinAprobar) h en $($res.PersonasConSinAprobar) personas"
"Total rechazado:   $($res.TotalHorasRechazado) h en $($res.PersonasConRechazado) personas"
""
$res.Personas | ForEach-Object {
    [pscustomobject]@{ IdEmpleado = $_.IdEmpleado; Nombre = $nombres[$_.IdEmpleado]
                       SinAprobar = $_.HorasSinAprobar; Rechazado = $_.HorasRechazado }
} | Sort-Object Nombre | Format-Table -AutoSize
```

- [ ] **Step 2: Ejecutarlo**

```powershell
powershell -ExecutionPolicy Bypass -File "Actualizacion\validar-fuera-de-horario.ps1"
```

Expected, con los casos del diagnóstico (`Actualizacion/diagnostico-resultado.txt`):

| Persona (IdEmpleado) | Sin aprobar | Rechazado |
|---|---|---|
| Soria (38) | **1,00** | 0 |
| Solórzano (184) | **1,50** o más | — |
| Coronel (68) | **2,75** o más | — |
| Rodríguez (63) | **2,00** o más | — |
| Haro (19) | **1,02-1,03** o más | — |
| Villarreal (48) | — | **6,20** o más |
| Tello (40) | ≥ 1,00 (14 y 15-09) | **10,75** |
| Cobeña (7) | sin los 30 min del 14-09 08:00 (ya pagados) | — |

«O más» significa que puede haber otros tramos fuera de horario que el diagnóstico no miró. **Menos** que la cifra esperada es un defecto. Cada «más» se revisa en el detalle (`$res.Detalle | Where-Object IdEmpleado -eq N`): tiene que ser tiempo que de verdad cae fuera del horario de esa persona.

- [ ] **Step 3: Si algo no cuadra, parar**

Se identifica el tramo con el detalle y se decide si falta una prueba en la Tarea 2, que es donde se corrige. No se sigue a la Tarea 8 con una cifra sin explicar.

---

### Task 8: Documentación de despliegue y paquete

**Files:**
- Modify: `DESPLIEGUE.md` (sección 12 nueva, al final)
- Regenerate: `ReporteTareas/obj/Release/Package/PackageTmp`

- [ ] **Step 1: Agregar la sección 12 a `DESPLIEGUE.md`**

```markdown
## 12. Horas Extras: tiempo fuera de horario

Columnas informativas en la pantalla y en el Excel: tiempo registrado fuera del horario de
cada persona que **no** se paga, separado en «sin aprobar» y «rechazado». **No cambia ningún
monto** ni toca ninguna tabla.

### 12.1 Base de datos

`docs/sql/2026-09-28-horas-extras-fuera-de-horario.sql`: crea `Sp_RTA_HeFueraDeHorario`, que es
de solo lectura. Va **antes** de los binarios; sin ellos nadie lo llama. El bloque final
imprime `OK` o `VEREDICTO: FALLO`.

`docs/sql/referencia/FN_RTA_ClasificarTramosHorario.sql` es **solo referencia: no se ejecuta.**

### 12.2 Archivos a publicar

Del paquete `ReporteTareas\obj\Release\Package\PackageTmp`:

- `bin\ReporteTareas.dll`, `bin\CapaEntidad.exe`, `bin\CapaNegocio.exe`, `bin\CapaDato.exe`
- `Formulario\HorasExtras.aspx` (pasa a `horasExtras.js?v=8`)
- `js\horasExtras.js`

Los tres de capa son `.exe`, no `.dll`, y van **juntos** con el `.dll`: el 18-09 se publicó el
`.dll` solo y dejó de salir todo correo.

> **Estos binarios llevan todo lo que haya commiteado en la rama**, no solo este cambio. Antes
> de copiar, confirmar que lo demás que viaja con ellos (por ejemplo, la sección 11) ya se
> puede publicar.

### 12.3 Verificar después de publicar

1. En Horas Extras, el período 16-08 / 15-09-2026 muestra las dos columnas con valores (no «…»
   ni «no disponible»).
2. Un enlace abre el detalle con fecha, tramo, parte fuera de horario, tipo y estado.
3. «Cerrar período» muestra la línea con las cifras y deja cerrar.
4. El Excel trae tres hojas. La de «Pagos» **no** tiene las columnas nuevas.
```

- [ ] **Step 2: Regenerar el paquete y comprobarlo**

Desde PowerShell:

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas\ReporteTareas.csproj" -p:DeployOnBuild=true -p:PublishProfile=FolderProfile -p:Configuration=Release -v:minimal -nologo
```

Luego, desde Git Bash:

```bash
grep -h "horasExtras.js?v=" ReporteTareas/Formulario/HorasExtras.aspx ReporteTareas/obj/Release/Package/PackageTmp/Formulario/HorasExtras.aspx
grep -c "CargarFueraDeHorario" ReporteTareas/obj/Release/Package/PackageTmp/js/horasExtras.js
```

Expected: los dos `?v=8` iguales, y un conteo mayor que 0. Se comprueba **dentro del paquete** algo escrito en el último commit, no solo que los archivos existan.

- [ ] **Step 3: Commit**

```bash
git add DESPLIEGUE.md ReporteTareas/obj/Release/Package/PackageTmp
git commit -m "docs(horas-extras): despliegue del tiempo fuera de horario y paquete regenerado

Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>"
```

Antes del `git add`, revisar con `git status` qué archivos del paquete cambiaron: **no** se agrega el `Web.config` del paquete si su cambio no viene de este trabajo.

---

### Task 9: Publicar (con aprobación explícita del usuario)

Cada paso **escribe en producción** y se hace solo cuando el usuario lo aprueba en ese momento.

- [ ] **Step 1: Ejecutar el script SQL.** `bash docs/sql/run-sql.sh docs/sql/2026-09-28-horas-extras-fuera-de-horario.sql`. Expected: `OK: Sp_RTA_HeFueraDeHorario listo.`
- [ ] **Step 2: Copiar los binarios** de la sección 12.2 de `DESPLIEGUE.md`, archivo por archivo (nunca `robocopy /MIR`).
- [ ] **Step 3: Verificar** con la sección 12.3, y con las pruebas manuales 1 a 7 de la Tarea 5, paso 9.

---

### Task 10: Conciliación final con el reporte manual

**Files:**
- Create: `Actualizacion/conciliacion-he-16ago-15sep.ps1` (ignorado por git)

El objetivo es que **cada dólar de diferencia** con el reporte manual quede explicado, persona por persona: sea por las columnas nuevas o por una causa de datos con nombre y dueño.

- [ ] **Step 1: Escribir el script de conciliación**

Reutiliza el cálculo de la Tarea 7. Para cada una de las 39 personas del reporte manual (sus montos están en la hoja «Validación Avelina» del Excel `Actualizacion/F-CS-001 Reporte de Horas Extras período ddel 16 dde agosto al 15 de sep.xlsx`), imprime:

```
Persona | Manual USD | Pagado USD | Sin aprobar USD* | Rechazado USD* | Residuo USD | Causa del residuo
```

- `*` Las columnas marcadas se valoran con el `ValorHora50` / `ValorHora100` de la persona en `HE_Detalle` del período 3. **Solo en este script**, nunca en el producto.
- `Residuo = Manual − (Pagado + la parte de sin aprobar y rechazado que el reporte manual sí pagó)`.

Lee el Excel con `Import-Excel` si está instalado; si no, con EPPlus cargado desde `packages\`.

- [ ] **Step 2: Clasificar cada residuo distinto de cero**

Las causas ya identificadas en el diagnóstico, que las columnas nuevas **no** resuelven porque no son tiempo fuera de horario:

| Causa | Personas | Qué hace falta | Quién decide |
|---|---|---|---|
| No está en el maestro de Horas Extras | 2 | Cargarlas en `HE_ColaboradorParametro` y `HE_Salario` y volver a sembrar | RRHH (sueldos) |
| Sueldo distinto (1.600 contra 1.650) | 1 | Confirmar el sueldo vigente y, si cambia, cargar un ajuste | RRHH |
| Tipo pagado distinto del que da el horario (sábado al 50%, jueves por la mañana al 100%) | 2 | Corregir el tipo de 4 tramos | Nómina + jefatura |
| El reporte manual excluye tramos aprobados | 1 | Decidir si esos tramos se pagan | Nómina |
| Tramo del 16-09, fuera del período | 1 | Decidir el corte del período | Nómina |
| Redondeo | 15 | Ninguno (±0,05 USD por persona) | — |

- [ ] **Step 3: Entregar el resultado al usuario**

La tabla por persona y la lista de decisiones pendientes. **Ninguna corrección de datos se ejecuta en esta tarea:** cada una es una escritura en producción que depende de un dato de RRHH o de una decisión de Nómina, y se prepara como script aparte cuando llegue esa decisión.
