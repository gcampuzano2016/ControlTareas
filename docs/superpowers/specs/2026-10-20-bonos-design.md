# Bonos de empleados — Diseño

**Fecha:** 2026-10-20
**Estado:** aprobado por el usuario el 2026-10-22
**Rama:** `ProyectoNuevosCambios`
**Pedido por:** Talento Humano (arma hoy la «Base de ingresos» de bonos a mano cada mes)
**Antecedentes:**
- Archivo de origen: `RevisionBonos/9. Septiembre.xlsx` (sin trackear). Hojas `Sep_2026` (43 bonos, $3.195) y
  `vista_mensual` (total de cada mes desde abril 2025).
- Conversación de diseño del 6–7 de octubre (preguntas 1–7 y parte 1 del modelo) y del 19–20 de octubre (partes 1–4,
  bocetos en `.superpowers/brainstorm/1009-1792467466/content/bonos-flujo-pantallas.html`).

## Objetivo

Que los bonos mensuales (standby, contrato por cliente y sueltos) dejen de armarse a mano en Excel: cada jefe propone
los de su gente, Talento Humano los revisa y cierra el mes, y el sistema entrega a Nómina el mismo Excel de hoy con el
concepto SAP de cada fila. Además, que no se pague dos veces la misma semana de standby y que quede el histórico mes a
mes.

**Éxito:** el mes de octubre 2026 se cierra en el sistema y Nómina recibe el Excel sin que Talento Humano lo retoque.

**Antiobjetivos** (aunque funcione, sería un fracaso):
- que Talento Humano tenga que corregir el Excel a mano antes de mandarlo;
- que un bono ya pagado se pueda volver a pagar o modificar después de cerrar el mes;
- que un jefe vea o proponga bonos de gente que no le reporta.

## Lo que se acordó

| Decisión | Valor |
|---|---|
| Quién arma el mes | los **jefes proponen**, **Talento Humano aprueba** y consolida |
| Jefe | el `Cod_Jefe_Inm` de la persona; propone **solo para sus reportes directos**, no para toda su cadena |
| Talento Humano | perfiles **14** (Talento Humano) y **18** (Super Admin), como en Horas Extras |
| Revisión | **fila por fila**: aprobar, ajustar el valor con motivo, o rechazar con motivo; lo rechazado vuelve al jefe |
| Registro directo | Talento Humano también registra bonos sin jefe (Cumplimiento, Nuevo cargo, etc.) |
| Valor | **catálogo de tipos** con valor sugerido **editable con motivo**; en standby la tarifa es **por semana** |
| Período | **mensual**, abierto/cerrado; **sin fecha límite**: Talento Humano cierra cuando quiere |
| Semanas | **lunes a domingo**, elegidas en un calendario; pueden ser de meses anteriores no pagados |
| Doble pago | prohibido: misma persona, mismo tipo y misma semana una sola vez, aunque sean meses distintos |
| Salida | **el Excel de hoy + columna de concepto SAP**; el concepto depende del tipo y la empresa y es configurable |
| Lugar | la **ciudad donde trabaja** la persona; no existe en `R_Usuarios`, se guarda en el bono |
| Histórico | **septiembre 2026 con detalle** (primer mes cerrado) y **abril 2025–agosto 2026 solo con su total** |
| Correos | al abrir el mes (jefes), al enviar (Talento Humano) y al rechazar (jefe) |
| Construcción | **módulo propio** con la estructura de Horas Extras, Tablero y Agenda; `RegistroIngresosEgresos.aspx` no se toca |

## Contexto verificado (producción, 2026-10-20)

### Personas

Las 38 personas del Excel de septiembre **existen en `R_Usuarios`, activas** (`Usuario_Estado = 'A'`,
`EstadoUsuario` NULL) **y con jefe**. No hace falta un registro de beneficiarios externos.

- **DOS (22):** se identifican por `R_Usuarios.Cod_Sap`, que es el «ID SAP» del Excel.
- **AGILITY (16):** se identifican por `R_Usuarios.Cedula`; casi ninguno tiene `Cod_Sap`. En el Excel su «ID SAP» es la
  cédula. **Una persona de AGILITY** no tiene cédula cargada.
- `R_Usuarios.Empresa` trae `AGILITY` y `Agility` (y 4 activos con NULL): se compara sin mayúsculas ni espacios.
- Una cédula del Excel venía sin el cero inicial (9 dígitos): las cédulas se comparan rellenadas a 10 dígitos.

Jefes de esas 38 personas: `222692` 22, `1051` 9,
`1202` 6, `989` 1. El usuario confirmó que `222692` decide de verdad los bonos de
sus 22 personas.

### Septiembre 2026 en el Excel

- 43 filas, total **$3.195** (`D48` y `vista_mensual` coinciden).
- Una fila de **$0** («Bono cotrato GPF»).
- Standby con semanas escritas a mano y mal partidas («Del 3 al 10 de agosto»); casi todas son de **agosto**, algunas
  de septiembre.
- Standby sin semanas de valor fijo: «Bono standby GPF» $50 y «Bono de standby Difare» $50.
- Lugares: Quito, Guayaquil, Ambato, Cayambe, Tabacundo, Portoviejo; cada persona tiene un solo lugar.

### Infraestructura que se reutiliza

- **Permisos** (`ReporteTareas/clases/Permisos.cs`, `NegPermisos`): `Permisos.PuedeEntrar(href)` por menú.
- **Correo**: `EnvioCorreoHelper`, envío SMTP en el momento, como Agenda.
- **Excel**: la misma librería y patrón de `NegTableroCostosExportacion` / `NegHeExportacion`.
- **Nombres libres**: no existe ningún objeto `Bono*` ni `Sp_RTA_Bono*` en la base.

## Arquitectura

### 1. Datos

Seis tablas nuevas en `ReporTarea`. **Ninguna tabla existente se modifica.**

**`Bono_Tipo`**: el catálogo. `IdTipo`, `Nombre` («Standby Cisco UIO», «Contrato Nestlé», «Cumplimiento»…), `Familia`
(Standby / Contrato / Otro), `ValorSugerido DECIMAL(9,2)` (NULL = sin sugerido), `PorSemana BIT`, `Activo`, auditoría.
Si `PorSemana = 1`, `ValorSugerido` es la tarifa de una semana.

**`Bono_TipoConcepto`**: `IdTipo`, `Empresa` (`DOS` / `AGILITY`, normalizada), `CodigoConcepto` (`1M34`, `1M40`…).
Único por `(IdTipo, Empresa)`.

**`Bono_Periodo`**: `IdPeriodo`, `Anio`, `Mes` (único por `(Anio, Mes)`), `Estado` (Abierto / Cerrado / Histórico),
`FechaApertura`, `UsuarioApertura`, `FechaCierre`, `UsuarioCierre`, `TotalHistorico DECIMAL(11,2)` (solo Histórico).

**`Bono_Detalle`**: una fila por bono (= una fila del Excel).
- Persona: `CodUsuario` (obligatorio) y la **copia** de `Empresa`, `Nombre`, `Identificacion`, `Lugar` tomada al enviar
  (o al registrar, si lo hace Talento Humano). No cambia después: el Excel de un mes cerrado sale siempre igual.
- Valor: `IdTipo`, `Semanas` (cantidad), `ValorSugerido`, `Valor`, `MotivoAjuste`, `Observacion` (nota libre).
- Flujo: `IdPeriodo` (NULL = pendiente sin período, ver §2), `Estado` (Borrador / Enviado / Aprobado / Rechazado),
  `Origen` (Jefe / TalentoHumano / Migracion), `CodJefe`, `UsuarioRevisa`, `FechaRevisa`, `MotivoRechazo`, auditoría.

**`Bono_DetalleSemana`**: `IdBono`, `Lunes DATE` con `CHECK` de que sea lunes. Único por `(IdBono, Lunes)`.

**`Bono_Bitacora`**: `IdBono` (o `IdPeriodo`), `Accion` (Crear, Editar, Enviar, Aprobar, Ajustar, Rechazar, Abrir,
Cerrar, Correo), `ValorAnterior`, `ValorNuevo`, `Detalle`, `Usuario`, `Fecha`. Los correos registran destinatario y,
si falló, el error.

### 2. Reglas (clases `Neg*` puras, probadas en `CapaPruebas` sin base)

- **Mi gente:** las personas activas (`Usuario_Estado = 'A'`, `EstadoUsuario` NULL, internas) cuyo `Cod_Jefe_Inm` es el
  usuario. Solo reportes directos.
- **Identificación:** DOS → `Cod_Sap`; AGILITY → `Cedula` rellenada a 10 dígitos. Si falta, el bono **no se puede
  enviar ni aprobar** y el mensaje dice qué dato completar en Administración de Usuarios. Empresa vacía o distinta de
  DOS/AGILITY: mismo tratamiento.
- **Valor sugerido:** `PorSemana` → tarifa × semanas marcadas; si no → `ValorSugerido`. Si el tipo no tiene sugerido, el
  valor es libre y no pide motivo. Si el valor final difiere del sugerido, **motivo obligatorio** (jefe o Talento Humano).
  Valor > 0.
- **Semanas:** lunes a domingo; solo semanas **ya terminadas** (domingo anterior a hoy) y **desde el lunes 2026-08-03**
  (la primera semana del Excel de septiembre): antes de esa fecha los meses solo tienen total y el sistema no puede saber
  qué semanas se pagaron. Tipo por semana: al menos una. Tipo no por semana: ninguna.
- **Repetido no semanal:** si la persona ya tiene otro bono no rechazado del **mismo tipo no semanal en el mismo
  período**, el sistema avisa y pide confirmar (no bloquea: puede ser legítimo, por ejemplo un pendiente que llega del
  mes anterior).
- **Doble pago:** no puede existir otro bono **no rechazado** con la misma persona, el mismo `IdTipo` y el mismo
  `Lunes`, en cualquier período. Se valida al guardar, al enviar y al aprobar (un índice único no alcanza porque el estado
  vive en `Bono_Detalle`). En el calendario, las semanas ya tomadas salen tachadas.
- **Quién edita qué:**
  - Jefe: crea, edita y borra bonos de su gente en **Borrador** o **Rechazado**, y los envía. No toca Enviado ni
    Aprobado.
  - Talento Humano: aprueba, ajusta (con motivo) o rechaza (con motivo) lo **Enviado**; puede volver a ajustar o
    rechazar lo **Aprobado** mientras el mes esté abierto; registra bonos directos (quedan Aprobados).
  - Nada se edita en un mes **Cerrado** o **Histórico**.
- **Período:** a lo sumo **uno abierto** a la vez; se abre el mes siguiente al último existente. El jefe solo crea bonos
  si hay período abierto.
- **Cierre:** entra solo lo **Aprobado**. Antes de confirmar, Talento Humano ve lo que queda fuera (borradores, enviados,
  rechazados) y los tipos×empresa sin concepto SAP. **Si falta algún concepto, no se puede cerrar.** Lo que queda fuera
  pasa a pendiente sin período (`IdPeriodo` NULL) conservando su estado, y se engancha solo al próximo mes que se abra.
- **Total del mes:** suma de lo aprobado (Cerrado/Abierto) o `TotalHistorico` (Histórico); variación contra el mes
  anterior.

### 3. Capas

Patrón de Horas Extras / Tablero / Agenda:
- `CapaEntidad`: `EntBono*`.
- `CapaDato`: `DaoBono` → procedimientos `Sp_RTA_Bono*`.
- `CapaNegocio`: reglas puras (`NegBonoReglas`, `NegBonoFechas`…), orquestación (`NegBono`), exportación
  (`NegBonoExportacion`), correos (`NegBonoCorreo`).
- `ReporteTareas`: `AdministrarBonos.ashx` (acciones JSON) y `DescargarBonos.ashx` (Excel); en `Permisos.cs` las
  constantes de las 4 pantallas.

### 4. Pantallas

Dentro de `Master.Master`, colores del portal. Bocetos aprobados en la conversación (ver Antecedentes).

1. **`BonosProponer.aspx` — Proponer bonos (jefe).** Período abierto y su gente. Lista de sus bonos del período y
   pendientes, con estado y motivo de rechazo. Agregar/editar: persona (solo su gente), tipo, calendario de semanas
   (tachadas las ya tomadas), valor con el sugerido al lado, motivo si cambia, lugar (sugerido del último bono de la
   persona), observación. Botón **Enviar a Talento Humano** (envía todos sus borradores y rechazados corregidos).
   Si el usuario no tiene gente a cargo: aviso y nada más.
2. **`BonosRevisar.aspx` — Revisar y aprobar (Talento Humano).** Resumen del período (enviados, aprobados, rechazados,
   total aprobado). Tabla con jefe, persona, tipo, semanas, sugerido, valor, motivo; acciones Aprobar / Ajustar /
   Rechazar por fila y aprobar varios a la vez. **Registrar bono directo** para cualquier persona activa.
   **Abrir mes** y **Cerrar el mes…** (con la pantalla previa de lo que queda fuera).
3. **`BonosCatalogo.aspx` — Catálogo (Talento Humano).** Alta/edición de tipos: nombre, familia, sugerido, por semana,
   activo, y concepto SAP para DOS y AGILITY. Un tipo con bonos no se borra, se desactiva.
4. **`BonosReporte.aspx` — Reporte mensual (Talento Humano).** Mes, total, variación, estado; **Excel para Nómina** en
   los meses cerrados (se puede descargar las veces que se quiera; también un Excel provisional del mes abierto, marcado
   «PROVISIONAL»).

Acceso con `Permisos.PuedeEntrar`. Menú en un grupo nuevo **«Bonos»**: Proponer para todos los perfiles con usuarios
internos activos; Revisar, Catálogo y Reporte para 14 y 18. El handler valida cada acción en el servidor (jefe solo
sobre su gente; Talento Humano según pantalla).

### 5. Correos

- **Al abrir el mes:** a los jefes con al menos un bono en los últimos 3 meses con detalle (al inicio: los 4 jefes de
  septiembre), y a cualquier jefe que tenga pendientes sin período. Asunto «Bonos *mes año*: período abierto».
- **Al enviar un jefe:** a los usuarios activos con perfil 14, con el resumen (jefe, cantidad, total) y enlace a Revisar.
- **Al rechazar:** al jefe, con persona, tipo, valor y motivo, y enlace a Proponer. Si se rechazan varios del mismo jefe
  en una sesión de revisión, un correo por bono es aceptable.
- Si falla o no hay correo: la acción se guarda igual, queda en `Bono_Bitacora` (`Accion = Correo`, con error) y la
  pantalla avisa.

### 6. Excel para Nómina

Columnas: `EMPRESA`, `NOMBRES APELLIDOS`, `ID SAP`, `VALOR`, `OBSERVACION`, `FECHAS`, `Lugar`, **`CONCEPTO SAP`**.
- `ID SAP` = la identificación copiada (código SAP en DOS, cédula en AGILITY, como texto para no perder ceros).
- `OBSERVACION` = «*n* semana(s) *tipo*» en standby por semana, o el nombre del tipo; más « · *nota*» si hay.
- `FECHAS` = semanas agrupadas en rangos consecutivos: «Del 18 al 31 de agosto», «Del 28 de julio al 3 de agosto»,
  «Del 4 al 10 y del 25 al 31 de agosto». Vacío si no es por semana.
- Orden: empresa (DOS, AGILITY), familia, nombre. Fila final con el total. Sin subtotales.

### 7. Migración

Script `docs/sql/…-bonos-migracion.sql`, re-ejecutable:
- **Catálogo inicial** deducido de las observaciones de septiembre (Standby Cisco UIO/GYE, F5 UIO/GYE, Data Center
  UIO/GYE, Microsoft, GPF, Standby GPF fijo, Standby Difare fijo, Contrato Nestlé/Linde/ESPOL/GPF/Belcorp,
  Cumplimiento, Nuevo cargo) con sus valores, y conceptos: DOS `1M34` (Cumplimiento `1M30`), AGILITY `1M40`
  (Cumplimiento `1M37`). Talento Humano los revisa después en el Catálogo.
- **Septiembre 2026** como período Cerrado con sus filas, `Origen = Migracion`, persona por `Cod_Sap`/cédula. Las
  semanas escritas a mano se ajustan a su lunes. La fila de **$0** no se migra. El script imprime la lista de cada
  ajuste y omisión; el total migrado debe dar **$3.195**.
- **Abril 2025 – agosto 2026** como períodos Histórico con el total de `vista_mensual`.

### 8. Scripts SQL

En `docs/sql/`, re-ejecutables, cada uno termina en `VEREDICTO: OK`: esquema (tablas + `Sp_RTA_Bono*`), migración,
menú (con reversa comentada; se corre al desplegar) y verificación (sumas por mes, doble pago inexistente, conceptos
completos, menú esperado).

## Manejo de errores

- Sin sesión o sin permiso: el handler responde estado `0` con mensaje y no ejecuta nada; las pantallas redirigen a
  Principal.
- Todas las reglas del §2 se validan en el servidor aunque la pantalla ya las valide.
- Cambios sobre un bono que cambió de estado mientras tanto (otro usuario lo aprobó o rechazó): se rechazan con mensaje
  claro.
- Un fallo de correo nunca deshace el guardado.
- JSON del `.ashx` con `ContentEncoding` UTF-8 explícito y `.aspx`/`.js` con BOM (trampas conocidas del proyecto).

## Pruebas

**Unitarias** (`CapaPruebas`): mi gente (directos sí, nietos no, inactivos no); identificación DOS/AGILITY, cédula sin
cero, faltantes; valor sugerido y motivo obligatorio; semanas (no lunes, no terminada, cantidad según tipo); doble pago
(mismo período, otro período, ignorando rechazados); permisos de edición por estado y rol; cierre (bloqueo sin concepto,
pendientes a sin período); texto de `FECHAS` (consecutivas, cruzando mes, salteadas); total y variación.

**En navegador, en local:** un jefe (`222692` o `1051`) y Talento Humano, ciclo completo: abrir, proponer,
enviar, rechazar, corregir, aprobar, cerrar, descargar Excel; comparar el Excel de septiembre migrado con el original.

## Publicación y reversa

Orden: esquema, migración, menú, verificación; luego, **juntos**, `ReporteTareas.dll` y los tres `.exe` de capa, las 4
`.aspx`, sus `.js` y los 2 `.ashx`; verificación otra vez. Sección nueva en `DESPLIEGUE.md`. Reversa: binarios
anteriores juntos + reversa del menú; las tablas pueden quedar.

**Antes de migrar:** completar en Administración de Usuarios la cédula de la persona de AGILITY que no la tiene (su código está en `DESPLIEGUE.md` §22.2, paso 2; ver «Resuelto en la revisión de constructor», punto 3).

## Decisiones propuestas por mí y confirmadas por el usuario (2026-10-22)

1. **`CodUsuario` obligatorio** en cada bono: el modelo conversado lo permitía vacío para externos, pero las 38 personas
   existen en `R_Usuarios`.
2. Solo se marcan **semanas ya terminadas**.
3. Standby **por semana** y standby de **valor fijo** son tipos distintos y no se cruzan en la regla de doble pago.
4. Lo que queda fuera al cerrar **no se pierde**: pasa a pendiente sin período y entra al mes siguiente.
5. Talento Humano puede volver a ajustar o rechazar lo ya aprobado **mientras el mes esté abierto**.
6. `OBSERVACION` y `FECHAS` las **arma el sistema**; el Excel no lleva subtotales.
7. La fila de $0 de septiembre **no se migra**.
8. Correo de apertura solo a jefes con bonos en los últimos 3 meses o con pendientes.
9. «Proponer bonos» en el menú de todos los perfiles internos; quien no tiene gente a cargo ve un aviso.
10. Conceptos SAP iniciales: DOS `1M34`/`1M30`, AGILITY `1M40`/`1M37`. El usuario decidió arrancar con ellos sin esperar a Nómina; Talento Humano los corrige en el Catálogo cuando Nómina los confirme (`1M32`, `1M39` y los «Serv.» siguen sin asignar).
11. No se pueden marcar semanas **anteriores al 2026-08-03**: antes no hay detalle para impedir el doble pago.
12. Un bono no semanal repetido (misma persona, tipo y período) **avisa y pide confirmar**, no bloquea.

Lo que dijo el usuario al confirmarlas, porque explica el porqué: todos los beneficiarios tienen usuario (1); el standby
se paga por guardias ya hechas, nunca por adelantado (2); el standby GPF/Difare de $50 es un concepto distinto de las
semanas de standby (3); la fila de $0 no significa nada (7); con el límite del 3 de agosto basta (11); dos bonos del
mismo tipo no semanal en un mes pueden ser legítimos (12).

## Resuelto en la revisión de constructor (2026-10-22, decisiones mías)

1. **Un mes cerrado no cambia nunca.** Al cerrar, `Bono_Detalle` guarda también `CodigoConcepto` y `NombreTipo`, como ya
   guarda empresa, nombre, identificación y lugar. El Excel de un mes cerrado sale de esas copias, no del catálogo; un
   cambio de concepto en el Catálogo rige solo para el mes abierto y los siguientes. La migración de septiembre las llena.
2. **Editar un bono Rechazado lo devuelve a Borrador.** El último motivo de rechazo se sigue mostrando al jefe en la fila
   (además de quedar en `Bono_Bitacora`), así no se pierde de vista que fue rechazado. «Enviar» manda solo los Borradores.
   Un Rechazado que el jefe no corrige ni borra sigue pasando de mes en mes como pendiente (decisión 4): no se paga, y se
   ve en la lista previa al cierre.
3. **La cédula que le falta a una persona de AGILITY se carga antes de migrar** (el código está en `DESPLIEGUE.md`
   §22.2, paso 2), no solo antes de usar el módulo: sin ella una fila de septiembre no se encuentra y el mes no suma su
   total. El script de migración no lleva casos especiales: si una
   fila no encuentra a su persona, no migra nada y lista las faltantes.
4. **La columna H del Excel de septiembre son subtotales de trabajo** (`=SUM` de tres bloques de filas). El usuario
   confirmó que el Excel no lleva subtotales (decisión 6), así que `CONCEPTO SAP` ocupa esa posición.

## Fuera de alcance

- `RegistroIngresosEgresos.aspx` y los demás conceptos de nómina (comisiones, movilización…).
- Archivo con formato de carga directa a SAP.
- Un nivel de aprobación de Gerencia.
- Recordatorios automáticos o fechas límite.
- Que el colaborador vea sus propios bonos.
