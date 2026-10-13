# Tablero de Costos (horas y valor de las tareas Aranda) — Diseño

**Fecha:** 2026-10-12
**Estado:** aprobado, pendiente de plan de implementación
**Rama:** `ProyectoNuevosCambios`
**Pantalla:** `Formulario/TableroCostos.aspx` (nueva)

> **Sin datos personales.** Este documento describe estructura y reglas. El repositorio es público.
> Las pruebas versionadas usan datos inventados.

---

## Qué se pide

Llevar a la aplicación el libro `Dashboard_Tareas_Aranda.xlsx`, que hoy se genera a mano con
`C:\ReporteExcel\generar_dashboard.py`. El módulo muestra las horas registradas en
`R_DetTareasAranda` clasificadas en Normal / Extra / Traslado, con su valor en dólares,
con datos en vivo y sin correr el script.

**El Excel es la referencia:** si el tablero y el Excel no cuadran con los mismos datos,
el que está mal es el tablero.

---

## Decisiones tomadas

| Pregunta | Decisión |
|---|---|
| ¿Quién lo usa? | **Solo Gerencia (perfil 18) y Talento Humano (perfil 14)**, con valores en $ |
| ¿De dónde sale el valor hora? | **Solo lectura** desde `HE_Salario` / `HE_ColaboradorParametro` / `HE_Parametro`. Quien no tenga salario se valora en $0 y aparece en un aviso; se completa en el módulo Horas Extras |
| ¿Qué salario se aplica? | **El vigente más reciente, a todo el período** (mismo criterio que el Excel; no hay historial anterior a 2026) |
| ¿Alcance? | **Toda la hoja Dashboard** + detalle paginado de actividades + exportación a Excel del detalle filtrado |
| ¿Período? | **Filtro Desde / Hasta** (por omisión: 1 de enero del año en curso → hoy), máximo 24 meses |
| ¿Dónde se calcula? | **Enfoque A:** un procedimiento trae el detalle y las tarifas; una clase pura en `CapaNegocio` clasifica, valora, filtra y agrega |

Enfoques descartados:

- **Agregar en SQL, un procedimiento por sección.** Serían de 6 a 8 procedimientos que repiten
  los mismos 7 filtros, con la regla de clasificación en una función aparte. Cuadrarlos
  entre sí y probarlos cuesta mucho más.
- **Generar el Excel desde el servidor.** No es una pantalla, que es lo que se pidió.

---

## 1. Datos — `Sp_RTA_TableroCostos(@Desde DATE, @Hasta DATE)`

Solo lectura. Devuelve **dos resultados**.

**Resultado 1 — actividades** con `Det_Fch_RegDetalleIni >= @Desde AND < DATEADD(DAY, 1, @Hasta)`:

| Columna | Origen |
|---|---|
| `Id_RegDetTareas`, `Det_Fch_RegDetalleIni`, `Det_Fch_RegDetalleFin`, `Det_Tiempo` | `R_DetTareasAranda` |
| `Id_Responsable`, `Det_Nom_Empresa`, `Det_Id_CompAranda`, `Det_Num_OrdenServicio`, `Det_Det_Tarea`, `Det_Estado`, `IdDet_EstadoIni`, `Det_Horas_Extras_Tipo`, `Det_Horas_Extras_Estado` | `R_DetTareasAranda` |
| `NombreCliente` (= contrato) | `LEFT JOIN R_TareasAranda` por `Id_RegTareas` |
| `Nom_Usuario` | `OUTER APPLY (SELECT TOP 1 … FROM R_Usuarios WHERE Cod_Usuario = Id_Responsable ORDER BY Id_Usuario DESC)` |

`Id_Responsable` corresponde a `R_Usuarios.Cod_Usuario`. Se toma la cuenta más reciente
porque una misma persona puede tener varias cuentas.

**Resultado 2 — tarifas por responsable:**

```
Empleados e (Cod_Usuario IS NOT NULL)
  LEFT JOIN HE_ColaboradorParametro cp ON cp.IdEmpleado = e.IdEmpleado AND cp.Estado = '1'
  OUTER APPLY TOP 1 HE_Salario WHERE IdEmpleado = e.IdEmpleado AND Estado = '1'
      ORDER BY FechaVigenciaDesde DESC,
               CASE Origen WHEN 'Ajuste' THEN 0 ELSE 1 END,
               IdSalario DESC
→ Cod_Usuario, Cargo, JornadaHorasDia, AplicaHE, Monto
```

El desempate `Ajuste` antes que `Rol` es obligatorio: cinco personas tienen dos salarios con
la misma fecha de vigencia, y sin él cobrarían sobre el salario anterior al ajuste, sin
que nada falle.

**Parámetros:** se leen de `HE_Parametro` los vigentes (`FechaVigenciaHasta IS NULL OR >= GETDATE()`):
el factor del 50%, el del 100% y los días por mes. Si falta alguno se usan los del Excel
(1,5 / 2 / 30) y la pantalla lo avisa.

En la fase de plan se verifica que exista un índice útil sobre `Det_Fch_RegDetalleIni`.
Volumen medido: unas 20.700 actividades en 2025–2026.

---

## 2. Lógica — `CapaNegocio/NegTableroCostos.cs`

Clase **pura**: sin base de datos, sin sesión y sin `HttpContext`. Recibe las listas de
entidades y los filtros, y devuelve un `EntTableroCostos`. Las entidades van en
`CapaEntidad`, y la capa de datos solo mapea columnas.

### 2.1 Clasificación (una actividad cae en un solo tipo, en este orden)

1. **Traslado**: `Det_Estado`, recortado y en minúsculas, empieza con `"movilizaci"`, **o**
   `IdDet_EstadoIni ∈ {14, 28, 62}`. Tiene prioridad sobre Extra.
2. **Extra**: `Det_Horas_Extras_Tipo ∈ {1, 2}`.
3. **Normal**: el resto, en cualquier estado (también las pausas).

| Campo | Regla |
|---|---|
| **Recargo** | Extra o Traslado con tipo 1 → `50%`; con tipo 2 → `100%`. Traslado con otro tipo → `Sin recargo`. Normal → vacío |
| **Estado HE** | Si tiene recargo: 1 `Solicitado`, 2 `Aprobado`, 3 `Rechazado`, otro valor `Sin solicitud`. Traslado sin recargo → `No aplica`. Normal → vacío |
| **Horas** | `Det_Tiempo` (`hh:mm:ss`) → `h + m/60 + s/3600`, redondeado a 4 decimales. Si el formato es inválido, 0 |
| **Responsable** | `Nom_Usuario`; si no existe, `"Cod <Id_Responsable>"` |
| **Empresa** | `Det_Nom_Empresa`; si está vacía, `"(Sin empresa)"` |
| **Contrato** | `NombreCliente`; si está vacío, `"(Sin contrato)"` |
| **Período** | `Ene-2025`, `Feb-2025`… según el mes de `Det_Fch_RegDetalleIni` |

Los textos se recortan (`Trim`) antes de compararlos o agruparlos.

### 2.2 Valoración

- **Valor hora** = `Salario / (JornadaHorasDia × DíasMes)`. Ejemplos: con 8 h son ÷240; con 4 h, ÷120.
  Si no hay jornada, se toman 8.
- **Factor** = `Factor50` si el recargo es 50%, `Factor100` si es 100%; **1** en cualquier otro caso,
  o si el colaborador tiene `AplicaHE = No`.
- **Valor $** = `Horas × ValorHora × Factor`.
- Un responsable sin salario (o que no aparece en `Empleados`) se valora en **$0** y entra en
  la lista `SinSalario`, que la pantalla muestra como aviso.

### 2.3 Filtros (iguales al Excel)

| Filtro | A qué se aplica |
|---|---|
| Mes, Responsable, Empresa, Contrato | A todas las secciones, salvo las excepciones de 2.4 |
| Recargo, Estado HE | **Solo a Extra y Traslado.** No afectan a Normal |

«Todos» equivale a no filtrar. Las listas de cada filtro son **dependientes**: solo ofrecen
valores que tienen actividades con los demás filtros elegidos, igual que el Excel. Si
una combinación queda vacía, se devuelve el mismo aviso que muestra el Excel en `S3`.

### 2.4 Secciones que devuelve

| Sección | Contenido | Filtros que ignora |
|---|---|---|
| **Indicadores** | Horas, valor y actividades de Normal / Extra / Traslado / Total | — |
| **Resumen por tipo y recargo** | Las 11 filas del Excel: Normal, Extra 50%, Extra 100%, Subtotal Extras, Traslado sin recargo / 50% / 100%, Subtotal Traslado, Total, Total al 50%, Total al 100%. Columnas: horas, % horas, valor, % valor, actividades, $ por hora | — |
| **Extras por estado** | Filas Aprobado / Solicitado / Rechazado / Sin solicitud / Total × horas 50%, horas 100%, $ 50%, $ 100%, total $ | — |
| **Traslados por estado** | Filas Aprobado / Solicitado / Rechazado / Sin solicitud / No aplica / Total × horas sin recargo, 50%, 100%, total horas, total $ | — |
| **Por año** | Por año: horas y $ de los 6 tipos/recargos + totales | — |
| **Por mes** | Igual, por mes | **Mes** |
| **Top 15** | Normal (horas, $), Extra (horas 50%, 100%, $), Traslado (horas sin recargo, 50%, 100%, $), ordenados por $ descendente | **Responsable** |
| **Detalle** | Las actividades filtradas, con las columnas de la hoja `Datos` | — |
| **Listas de filtros** | Valores disponibles para cada filtro | — |
| **SinSalario** | Responsables del resultado que no tienen salario | — |

Los porcentajes se calculan contra el **Total** de la misma sección. Una división por cero da 0.

### 2.5 Pruebas

- Pruebas unitarias con datos inventados para:
  - la clasificación: la prioridad de Traslado sobre Extra, los 3 `IdDet_EstadoIni`, el prefijo con tilde y sin ella, y las mayúsculas;
  - el recargo y el estado HE de cada combinación;
  - la conversión de `Det_Tiempo`, incluido un formato inválido;
  - el factor con `AplicaHE = No`;
  - quien no tiene salario;
  - que cada filtro afecte solo a lo que dice 2.3 y 2.4;
  - los subtotales y totales;
  - las listas dependientes.
- **Prueba de cuadre (manual, no versionada):** con 2025-01-01 → 2026-12-31 y sin filtros,
  el tablero debe dar las cifras del Excel generado el 04-10-2026:
  **20.657 actividades, 43.552,07 h, $305.201,28**, con el mismo desglose por tipo. Para
  comparar dinero, se agrega por persona antes de sumar. Una diferencia mayor que un
  centavo por persona es un defecto.

---

## 3. Pantalla y handler

### `Formulario/TableroCostos.aspx`

Usa `Master.Master` y se guarda **con BOM**: sin BOM las tildes salen rotas en producción,
porque `Web.config` declara windows-1252.

1. **Filtros:** Desde / Hasta, Mes, Responsable, Empresa, Contrato, Recargo, Estado HE (todas con
   «Todos»), y los botones **Consultar** y **Exportar a Excel**. Debajo, el aviso de combinación vacía.
2. **Indicadores:** 4 tarjetas (Normales, Extras, Traslado, Total) con horas, $ y actividades.
3. **Tablas:** resumen por tipo y recargo, extras por estado, traslados por estado, por año, por mes, y los 3 top 15.
4. **Gráficos** (`js/chart.umd.js` local, no CDN), con los colores del script
   (Normal `#2F6DB5`, Extra `#E07B24`, Traslado `#1F9E89`):
   - dona de distribución de horas por tipo y recargo;
   - dona de distribución del valor por tipo y recargo;
   - barras apiladas de horas por mes, tipo y recargo;
   - barras apiladas de valor por mes, tipo y recargo;
   - barras horizontales del top 15 de Normal, de Extra y de Traslado (por $).
5. **Detalle de actividades**, paginado en el navegador, sin volver a consultar al servidor.
6. **Aviso «sin salario»**, con la lista de responsables y el texto «sus horas se valoran en $0; complételos en Horas Extras».
7. **Criterios**: un recuadro plegable con las reglas de 2.1 y 2.2.

El JavaScript va en un archivo propio, `js/tableroCostos.js`, referenciado con `?v=1`. Hay
que subir la versión en cada cambio: la caché del navegador ya causó un incidente con `?v=3`.

### `Formulario/AdministrarTableroCostos.ashx` — acción `consultar`

- Valida el perfil (sección 4); si no está autorizado, responde 403.
- Valida que Desde ≤ Hasta y que el rango no supere 24 meses.
- Llama al procedimiento **solo si cambió el rango**. El resultado del rango se guarda en
  `Session` y los demás filtros se aplican sobre él. Si la sesión expiró, se vuelve a consultar.
- Devuelve el `EntTableroCostos` en JSON, con `ContentEncoding = UTF8`: los nombres y las
  empresas llevan tildes, y sin esa línea el JSON sale en windows-1252.

---

## 4. Permisos, exportación y auditoría

### Permisos

- `AdministrarTableroCostos.PerfilesAutorizados = { 14, 18 }` es la **única** lista. La usan el
  handler, la descarga y el `Page_Load` de la página (que redirige al inicio). El menú es
  cortesía: la barrera real son estas comprobaciones.
- **Menú:** una hoja «Tablero de Costos» (`Href = 'TableroCostos.aspx'`) colgada del grupo
  **Nómina (Id_Menu 20081)**. Se crea clonando todas las columnas no identidad de la hoja
  hermana 20082, cambiando `Titulo` y `Href`. En `PerfilMenu` se agregan filas **solo** para
  los perfiles 14 y 18, con `Estado = '0'` (**visible**; la semántica está invertida).
  `Sp_RTA_ConsultarMenuPerfilUsuario` muestra únicamente las filas con `Estado = 0`, así que
  los demás perfiles no necesitan ninguna fila para quedar sin la opción.

  El script verifica que el grupo siga siendo el 20081 antes de insertar.

### Exportación — `Formulario/DescargarTableroCostos.ashx`

- Comprueba el perfil y aplica los mismos filtros que la pantalla, con el mismo cálculo de `NegTableroCostos`.
- Arma el archivo **en memoria** con EPPlus (`LicenseContext.NonCommercial`), como `NegHeExportacion`, y nunca lo escribe a disco. Dos hojas:
  - **Detalle**: las columnas de la hoja `Datos` del Excel actual, incluidos factor, valor hora y valor $.
  - **Resumen**: el resumen por tipo y recargo, con los filtros aplicados en la cabecera.
- Nombre del archivo: `TableroCostos_<desde>_<hasta>.xlsx`.

### Auditoría de descargas

El archivo lleva valores calculados a partir de los salarios, así que **cada descarga se
registra antes de generar el archivo**. Si el registro falla, no se entrega nada.

- Tabla nueva **`TC_Auditoria`**: `IdAuditoria INT IDENTITY PK`, `CodUsuario VARCHAR(20)`,
  `Accion VARCHAR(20)` (`'Descargar'`), `Filtros NVARCHAR(1000)` (los filtros en texto),
  `Fecha DATETIME DEFAULT GETDATE()`.
- Procedimiento `Sp_RTA_TableroCostosAuditar(@CodUsuario, @Accion, @Filtros)`.
- No se usa `HE_PeriodoAuditoria`, porque está atada a un período de nómina.
- Las consultas en pantalla **no** se auditan.

---

## 5. Despliegue

- **Un script**, `docs/sql/2026-10-12-tablero-costos.sql`, sin BOM y sin tildes. Crea o altera:
  - el procedimiento `Sp_RTA_TableroCostos`;
  - la tabla `TC_Auditoria` y su procedimiento;
  - el menú.
  
  Es repetible: correrlo dos veces no duplica el menú ni la tabla. Lleva aserciones con contador
  `@Fallos` y veredicto final, sin `GO` entre la declaración y el último uso, y empieza con
  `SET QUOTED_IDENTIFIER ON`.
- **Binarios:** `CapaEntidad`, `CapaDato`, `CapaNegocio` y la web se publican **juntos**. El
  18-09 se publicó solo el `.dll` y los correos dejaron de salir. Se compila con el MSBuild de VS2019.
- `DESPLIEGUE.md`: sección nueva. El paquete se regenera **al final**, después del último
  commit de código, y se comprueba buscando dentro algo escrito en ese commit.

---

## Fuera de alcance

- Editar salarios o tarifas desde el tablero: eso se hace en Horas Extras.
- Salario histórico por fecha de actividad.
- Generar el libro completo con fórmulas como el Excel actual.
- Acceso de jefaturas o de otros perfiles.
- Auditar las consultas en pantalla.
