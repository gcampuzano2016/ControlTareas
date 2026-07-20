# Olas Look-and-Feel — Roll-out a las páginas restantes

> Continuación del piloto (`2026-07-17-piloto-look-and-feel.md`). El piloto dejó el tema
> `tema-dos.css` cargado globalmente vía `Master.Master`. Este plan cubre las **71 pantallas
> restantes** en olas por módulo funcional.

## Premisa (leer primero)

**El tema YA está aplicado en todas las páginas que usan el Master.** Al cargar `tema-dos.css`
como último `<link>` del Master, todas las páginas de contenido heredan automáticamente el estilo
de botones, formularios, panels, tablas, modales y chrome. Además, **ninguna de las 71 páginas
restantes tiene `<font>`** (eso era exclusivo de Login/Principal).

Por tanto esto **no es una reescritura**: es un **barrido de verificación**. Regla de oro:
**verificar cada pantalla y corregir SOLO lo que se vea roto.** No re-indentar, no quitar estilos
inline benignos, no reorganizar markup que ya se ve bien.

## Objetivo

Que las 71 pantallas restantes se vean correctas y consistentes con el tema DOS, corrigiendo
únicamente los defectos visibles, sin tocar lógica.

## Global Constraints (heredadas del piloto)

- **NO tocar lógica:** `CapaNegocio`/`CapaDato`/`CapaEntidad`, stored procedures, `Page_Load`/eventos
  de servidor, ni IDs de controles `asp:`. Solo presentación.
- **Reversibilidad:** el tema sigue quitándose comentando el `<link>` en el Master.
- Todo color nuevo vía variable CSS del tema; nada hardcodeado.
- Idioma español, respetando textos actuales.
- Un commit por pantalla (o por grupo de pantallas triviales de la misma ola).
- Verificación por pantalla: **build MSBuild 0 errores** + revisión visual (réplica estática con el
  `tema-dos.css` real, o la app en vivo) + grep de no-regresión de los controles/IDs de esa pantalla.

## Qué cuenta como "roto" (checklist de revisión por página)

Corregir solo si se observa alguno de estos:

1. **GridView sin la clase base `table`** → no hereda el tema de tabla. (Ver lista concreta abajo.)
2. **Modal con fondo hardcodeado** (`#fcf8e3` amarillo u otros) en `modal-header`/`modal-content`.
3. **Encabezado/título** con color heredado feo o ilegible, o panel sin unificar.
4. **Colores hardcodeados** que choquen con el rojo DOS (fondos, textos) o problemas de **contraste**
   (texto sobre fondo del mismo tono).
5. **Botón** que no tomó el estilo del tema (clases raras o `BackColor`/`ForeColor` inline).
6. **Inputs/formularios** con ancho o fondo inline que rompan el layout.
7. **Desborde horizontal** en móvil (~375px) — envolver tablas anchas en `table-responsive`.

Si la página se ve bien, **no se toca**: se marca verificada y se pasa a la siguiente.

## Correcciones transversales ya identificadas

**A) GridViews sin la clase base `table` (5, corrección segura y concreta):**
`CssClass="table-responsive table-striped table-bordered table-hover"` → anteponer `table`.
Presentes en: `AprobarHorasExtras.aspx`, `Tareas.aspx`, `TiempoTarea.aspx`.
(Mismo patrón que se corrigió en `ReporteHorasExtras.aspx` en el piloto.)

**B) Páginas standalone que NO usan el Master** (no heredan el tema; agregar el `<link>`
`../dist/css/tema-dos.css` como último del `<head>` si se quieren temadas):
`ResetPassword.aspx`, `PaginaError.aspx`, `RespuestaAprobacion.aspx`.
Páginas de prueba (baja prioridad o descartar): `Plantilla.aspx`, `PruebaMenu.aspx`, `PrubaWebServices.aspx`.

---

## Olas (por módulo funcional, ordenadas por uso diario)

Leyenda de flags: `lin`=líneas · `inl`=estilos inline · `mod`=nº bloques modal · `⚠`=forma pesada (revisar, no limpiar a fondo).

### Ola 0 — Standalone + quick wins (arreglo puntual)
- `ResetPassword.aspx` (88, 3 inl, 5 mod) — agregar `<link>` del tema.
- `PaginaError.aspx` (16) — agregar `<link>` del tema.
- `RespuestaAprobacion.aspx` (47) — agregar `<link>` del tema.
- Corrección transversal (A): base `table` en los 5 GridView de `AprobarHorasExtras`, `Tareas`, `TiempoTarea`.

### Ola 1 — Tareas (uso diario, alto impacto)
`Tareas.aspx` (352, grid), `ActualizarTareas.aspx` (417, 25 mod), `CreaTarea.aspx` (176),
`CreaTareas.aspx` (128), `CambiarEstadoTarea.aspx` (293, 25 mod), `ValidarTareas.aspx` (137, 14 mod),
`AprobacionTareasJefatura.aspx` (471, 32 mod), `AprobacionTareasRevisor.aspx` (471, 32 mod),
`TiempoTarea.aspx` (204, grid), `RegistroActividad.aspx` (163),
`ReporteTareas.aspx` (340, 18 mod), `ReporteTareasDetalle.aspx` (438, 18 mod),
`ReporteCumplimientoDiario.aspx` (358, 25 mod).

### Ola 2 — Horas extra / Asistencia / Horario
`AprobarHorasExtras.aspx` (201, grid), `AprobarHorasExtraPendientes.aspx` (147, 14 mod),
`RegistroAsistencia.aspx` (152), `ReporteHorarioLaboral.aspx` (459, 34 mod).

### Ola 3 — Vacaciones / Permisos
`ListaVacacionesPermiso.aspx` (474, 34 mod), `SolicitarVacacionesPermiso.aspx` (457, 27 mod),
`SolicitarVacacionesPermisoDesa.aspx` (397, 27 mod).

### Ola 4 — RRHH / Empleados / Perfiles
`ListaEmpleados.aspx` (360), `RRHHEmpleados.aspx` (443, 13 mod), `Perfiles.aspx` (87).

### Ola 5 — Médico / Historia clínica (formas pesadas — solo verificar)
`ReporteSalud.aspx` (74), `InfoDepMedico.aspx` (166), `HistoriaClinicaArchivosRRHH.aspx` (144),
`HistoriaCertificado.aspx` (378), `HistoriaClinica.aspx` (482),
`HistoriaReintegro.aspx` ⚠ (886, 119 inl), `HistoriaRetiro.aspx` ⚠ (893, 122 inl),
`HistorialClinico.aspx` ⚠ (928, 331 inl), `HistoriaInmunizaciones.aspx` ⚠ (1294, 227 inl),
`HistoriaPeriodica.aspx` ⚠ (1892, 260 inl), `HistoriaEvaluacion.aspx` ⚠ (2559, 421 inl),
`DepartamentoMedico.aspx` ⚠ (2817, 379 inl).
> En las ⚠: **solo verificar y corregir defectos puntuales**. La limpieza total de inline queda
> fuera de este barrido (sería un proyecto aparte). Registrar hallazgos, no reescribir.

### Ola 6 — Comercial / ForeCast / Rebates / Cuotas / Coste
`ReporteComercial.aspx` (70), `DashboardRebates.aspx` (75), `RegistroCuotas.aspx` (131),
`RegistroCoste.aspx` (314), `ForeCastDetalle.aspx` (308, 14 mod), `RegistroForeCastGD.aspx` (540, 14 mod),
`RegistroRebates.aspx` (590, 28 mod), `RegistroForeCast.aspx` (716, 35 mod).

### Ola 7 — Inventario / Ingresos-Egresos
`RegistroIngresosEgresos.aspx` (195), `RegistroInventario.aspx` (351),
`EgresoInventario.aspx` (673, 122 inl, 20 mod).

### Ola 8 — Contratos / Pedidos / Proyecto
`RegistroProyecto.aspx` (202), `DetallePedido.aspx` (518, 14 mod), `DetalleContrato.aspx` (625, 48 inl),
`RegistrarPedido.aspx` (980, 37 mod), `RegistrarContrato.aspx` (1005, 42 mod),
`InformacionContratos.aspx` ⚠ (1692, 556 inl, 27 mod).

### Ola 9 — Reportes varios (mayormente simples)
`Reporte.aspx` (74), `ReporteFacturacion.aspx` (70), `ReporteGerencia.aspx` (70),
`ReporteServicio.aspx` (70), `ReporteGastoContable.aspx` (118), `FrmRptReporteGerencial.aspx` (121),
`ReporteGPF.aspx` (142, 14 mod), `ReportesSap.aspx` (258, 58 inl),
`FrmRptReporteGerencial.aspx` (121), `ReporteGerencia.aspx`.

### Ola 10 — Células (landing simples) + misceláneos
`CelulaAppdome.aspx`, `CelulaCisco.aspx`, `CelulaCiscoGYE.aspx`, `CelulaF5.aspx`,
`CelulaSoftware.aspx` (67–71 líneas, 0 inline — casi seguro solo verificar).
Test/opcionales: `Plantilla.aspx`, `PruebaMenu.aspx`, `PrubaWebServices.aspx`.

---

## Flujo de trabajo por ola

1. Abrir cada pantalla (réplica estática con `tema-dos.css` real, o la app en vivo si está levantada).
2. Recorrer el checklist "qué cuenta como roto".
3. Corregir solo lo roto; mantener IDs/controles/lógica intactos.
4. `grep` de no-regresión de los controles clave de esa pantalla.
5. Build MSBuild (0 errores).
6. Commit por pantalla (o grupo trivial). Mensaje: `fix(<modulo>): tema en <Pantalla>` / `chore(...)`.
7. Anotar en `progress.md` (o un `progress-olas.md`) qué se verificó y qué se corrigió.

## Estimación (profundidad: verificar + arreglar solo lo roto)

- Olas 0–4, 6, 7, 9, 10: pantallas simples/medianas → revisión rápida; la mayoría "solo verificar".
- Olas 5 y 8 (formas pesadas ⚠): más tiempo de revisión visual, pero **sin limpieza profunda**.
- Ritmo realista: ~1 ola por sesión (o 2 de las cortas). Total ~7–10 sesiones.

## Definition of Done (por ola)

- [ ] Cada pantalla de la ola revisada contra el checklist.
- [ ] Defectos rotos corregidos; controles/IDs/lógica intactos (grep).
- [ ] Build 0 errores.
- [ ] Commits por pantalla y progreso anotado.

## Fuera de alcance

- Limpieza total de estilos inline en las formas pesadas (médico/contratos) — proyecto aparte si se decide.
- Cambios de lógica, correcciones del bug `view`/`view2`, o refactors de markup que no arreglen un defecto visible.
