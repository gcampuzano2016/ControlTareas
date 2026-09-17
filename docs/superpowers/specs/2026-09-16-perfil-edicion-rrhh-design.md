# Perfil del colaborador: edición por Talento Humano — Diseño

**Fecha:** 2026-09-16
**Estado:** aprobado, pendiente de plan de implementación
**Rama:** `ProyectoNuevosCambios`
**Módulo:** Perfil del colaborador (fases 1 a 3b ya construidas)

> **Sin datos personales.** Este documento describe estructura y reglas. El repositorio es público.

---

## Qué se pide

El perfil de un empleado a veces necesita corrección, y esa corrección la debe
poder hacer **el propio empleado o Talento Humano**, nadie más.

Hoy solo la hace el empleado. `AdministrarPerfil.ashx` toma el `Cod_Usuario` de
la sesión y no acepta ninguno del cliente; la única acción de todo el módulo que
recibe el código de otra persona es `PerfilEquipo`, y es de **solo lectura**.

Lo que falta, entonces, es que Talento Humano pueda **escribir** sobre el perfil
de un tercero.

---

## Decisiones tomadas

| Pregunta | Decisión |
|---|---|
| ¿Qué se necesita editar? | Que Talento Humano edite el perfil de otro. **No** se desbloquea la pestaña «Datos personales», que sigue con candado para todos. |
| ¿Por dónde entra? | **Pantalla nueva**, aparte. `MiPerfil.aspx` sigue siendo «mi» perfil y nada más. |
| ¿Qué puede tocar? | **Todo lo que edita el dueño**: contacto, emergencia, formación, certificaciones, experiencia, cargas familiares, foto y documentos. |
| ¿Hasta dónde llega el rastro? | **Autor en cada fila**: `Usu_Modificacion` pasa a guardar quién hizo el cambio, no de quién es el perfil. Sin bitácora aparte ni antes/después. |
| ¿Y las dos tablas ajenas sin columna de autor? | **Se les agrega la columna.** |
| ¿Cómo se comparte el marcado? | **Control de usuario `.ascx`**, el primero del repositorio. |

**Quién es «Talento Humano»:** los perfiles **14 (Talento Humano)** y **18
(Super Admin)**, que es el mismo criterio que ya usa `AdministrarHorasExtras.ashx`
y el mismo que el menú aplica a las pantallas de nómina.

---

## Terreno verificado

Medido contra el repositorio el 2026-09-16, no supuesto.

### El módulo habla por tres canales, no por uno

Este es el hallazgo que corrige la forma del diseño. No hay un punto de entrada
sino tres, cada uno con su propio transporte:

| Superficie | Transporte | De dónde saca hoy la identidad |
|---|---|---|
| `AdministrarPerfil.ashx` — 13 acciones | JSON `[{action, parameters}]` | `CodUsuarioSesion(context)`, **18 llamadas** |
| `AdministrarPerfil.ashx` — subir documento | **multipart** (`Request.Files`) | la sesión |
| `DescargarPerfil.ashx` — documento y CV | **GET query string** | la sesión |

Escribir el criterio tres veces es tener tres sitios que pueden divergir. La
regla vive en un solo lugar y cada superficie solo **extrae** de su transporte.

### Tamaño real del módulo

| Archivo | Líneas |
|---|---:|
| `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs` | 859 |
| `ReporteTareas/Formulario/MiPerfil.aspx` | 498 |
| `ReporteTareas/js/miPerfil.js` | 790 |
| `CapaDato/DaoPerfil.cs` | 789 |
| `CapaNegocio/NegPerfilCampos.cs` | 609 |
| `CapaNegocio/NegPerfil.cs` | 138 (fachada de paso, sin lógica) |

**19 procedimientos** del módulo: **15 escriben**, 4 leen.

### La lectura ya servía perfiles ajenos, y nadie se había dado cuenta

`Sp_RTA_PerfilColaborador` recibe `@Cod_Usuario`. **Nunca asumió que fuera el de
la sesión** — eso lo imponía el handler. En cuanto se le entregue otro código
sirve el perfil ajeno sin tocar una línea de SQL.

Lo mismo `Sp_RTA_PerfilDocumentoArchivo`: su guarda es «este documento es de esta
persona», que sigue siendo la correcta cuando la persona es la que Talento
Humano eligió.

**Consecuencia:** de los 19 procedimientos, **ninguno de los 4 de lectura
cambia**.

### Dos de las tablas que toca el módulo no tienen dónde anotar al autor

| Tabla | Columna de autor | ¿Sirve? |
|---|---|---|
| Las 7 `Perfil_*` | `Usu_Modificacion VARCHAR(50)` | **Sí** |
| `Empleados` (estado civil) | `Usu_Modificacion` es **`numeric(5)`** | **No cabe** un `Cod_Usuario` |
| `Emp_CargaFamiliar` | **no existe** | **No hay dónde** |

Las dos excepciones ya estaban documentadas en los scripts de las fases
anteriores, pero hasta hoy **no importaban**: dueño y autor eran la misma
persona, así que fecha e IP bastaban. Desde que Talento Humano edita a un
tercero, esas dos escrituras quedan sin rastro de quién las hizo.

### No existe ningún `.ascx` en el repositorio

Se buscó en todo el árbol. El control de usuario que este diseño introduce es
**el primero**. Es un patrón nuevo para este proyecto, y se asume a sabiendas:
la alternativa era duplicar 498 líneas de marcado.

### `CapaPruebas` no puede probar nada del proyecto web

`CapaPruebas.csproj` referencia **solo** `CapaEntidad` y `CapaNegocio`. Nada que
viva en `ReporteTareas/` es alcanzable desde una prueba unitaria. Esto decide
dónde va la pieza crítica (ver §4).

---

## 1. Dónde se decide de quién es el perfil

La regla se parte en dos capas, **y no por gusto**: la de arriba es la que se
puede probar.

```
CapaNegocio/NegPerfilAcceso.cs          <- la REGLA. Sin HttpContext. Probable.
    PerfilesRRHH = { 14, 18 }
    Objetivo(codSesion, idPerfilSesion, codPedido) -> código objetivo, o rechazo

ReporteTareas/clases/PerfilIdentidad.cs <- solo EXTRAE de cada transporte y delega
    Autor(context)                      -> QUIÉN actúa. Siempre la sesión. Punto.
    Objetivo(context, codPedido)        -> delega en NegPerfilAcceso
```

La regla, completa:

| Situación | Resultado |
|---|---|
| `codPedido` vacío | el de la sesión (es el caso de `MiPerfil.aspx`, que no manda nada) |
| `codPedido` igual al propio | permitido |
| Sesión **es** 14 o 18 | `codPedido` |
| Sesión **no es** 14 ni 18, y pide uno ajeno | **rechazo con mensaje explícito** |
| `Id_Perfil` ausente o no numérico | **rechazo** |

**Por qué rechazar y no caer en silencio sobre el propio perfil.** Un rechazo no
puede romper `MiPerfil.aspx`: esa pantalla no manda `codUsuario` nunca. Y
convierte un intento en algo visible en vez de en un guardado silencioso sobre
otra fila. Ante la duda, cierra.

**Qué es «rechazo» en cada superficie.** La regla devuelve lo mismo; cada canal
lo expresa en su propio idioma, el que ya usa hoy para cualquier otro error:

| Superficie | Forma del rechazo |
|---|---|
| Acciones JSON | `responseMessage("0", ..., "danger")`, igual que el resto del handler |
| Subida multipart | el mismo `responseMessage`, que es lo que ya devuelve `SubirDocumento` |
| `DescargarPerfil.ashx` | `NoDisponible(context, ...)`, con **el mismo texto** que cuando el documento no existe — un mensaje distinto le confirmaría a quien prueba códigos que acertó con uno |

**El autor no se negocia.** `Autor(context)` devuelve siempre el `Cod_Usuario` de
la sesión. No hay parámetro, no hay rama, no hay forma de que el cliente influya
en él. Es la mitad del diseño que hace que la trazabilidad signifique algo.

**Precedente.** `AdministrarHorasExtras.ashx` ya declara
`PerfilesAutorizados = { 14, 18 }` y comprueba `Session["Id_Perfil"]`, con el
comentario de que *«el menú solo controla que se VEA la pantalla, no que se pueda
LLAMAR al handler»*. Aquí aplica igual: la pantalla nueva estará en el menú solo
para 14 y 18, y eso **no** es la barrera. La barrera es esta función.

### Las 18 llamadas

Las 18 apariciones de `CodUsuarioSesion(context)` en `AdministrarPerfil.ashx.cs`
pasan a `PerfilIdentidad.Objetivo(context, ...)` para el **dueño**, y las
escrituras suman `PerfilIdentidad.Autor(context)` para el **autor**. Dos
conceptos que hasta hoy eran uno.

---

## 2. El SQL

### 2.1 Columnas de autor en las dos tablas ajenas

- `Emp_CargaFamiliar`: se agrega `Usu_Modificacion VARCHAR(50) NULL`.
- `Empleados`: la columna existente es `numeric(5)` y no sirve; se agrega una
  **nueva** columna de texto para el código de usuario.

Agregar columnas anulables no rompe a `RRHHEmpleados.aspx`: sus `INSERT` no las
nombran. Es la razón por la que esta opción es barata.

### 2.2 Los 15 procedimientos de escritura

Cada uno gana `@Usu_Accion VARCHAR(50)`, y `Usu_Modificacion` pasa a guardar
**ese** valor en lugar de `@Cod_Usuario`:

```
Sp_RTA_PerfilGuardarContacto          Sp_RTA_PerfilEliminarEmergencia
Sp_RTA_PerfilGuardarEmergencia        Sp_RTA_PerfilEliminarEstudio
Sp_RTA_PerfilGuardarEstudio           Sp_RTA_PerfilEliminarCertificacion
Sp_RTA_PerfilGuardarCertificacion     Sp_RTA_PerfilEliminarExperiencia
Sp_RTA_PerfilGuardarExperiencia       Sp_RTA_PerfilEliminarCargaFamiliar
Sp_RTA_PerfilGuardarCargaFamiliar     Sp_RTA_PerfilEliminarFoto
Sp_RTA_PerfilGuardarFoto              Sp_RTA_PerfilEliminarDocumento
Sp_RTA_PerfilGuardarDocumento
```

Es mecánico y el compilador no ayuda: va en **un script único**, revisable de
una sentada, con los 15 juntos.

Arrastra los mismos cambios de firma hacia arriba: 15 métodos en `DaoPerfil` y 15
en `NegPerfil`. Ahí el compilador **sí** ayuda — ninguna llamada vieja compila.

> **Lo que no se toca.** Las guardas de `Cod_Usuario` repetido (`Respuestas = -2`)
> se quedan exactamente como están, y la comparación recortada de cada una
> también. Ver §6.

### 2.3 El listado de personal

`Sp_RTA_PerfilPersonalLista`, calcado de `Sp_RTA_PerfilEquipoLista` quitándole el
filtro `Cod_Jefe_Inm`. Mismas columnas, misma comparación recortada, mismo `LIKE`
sobre nombre, código y cargo.

Conserva a propósito la regla de **no listar a quien tiene el `Cod_Usuario`
repetido**: si el perfil no se puede abrir, mejor que no aparezca con un botón
que nunca va a funcionar. Es la misma corrección que ya se le hizo a la lista de
equipo.

Dos decisiones menores:

- **Solo personal activo** (`EstadoUsuario = 0`), igual que el resto del módulo.
- **El buscador exige al menos 2 caracteres** antes de traer nada, para no volcar
  la plantilla entera en una tabla sin paginar. DataTables no carga en este
  sistema.

---

## 3. La pantalla

### 3.1 El marcado, una sola vez

Las 498 líneas de `MiPerfil.aspx` se mudan a `Controles/PerfilFichas.ascx`.
`MiPerfil.aspx` queda en unas diez líneas que lo incluyen; la pantalla nueva
agrega su buscador y lo incluye igual.

El movimiento es **solo marcado**: se corta el contenido de `<asp:Content>` y se
pega. No se mueve lógica, porque `MiPerfil.aspx.cs` no tiene ninguna más allá de
`RedireccionarALogin`.

> **La pestaña «Equipo» no se dibuja en la pantalla nueva.** No es cosmética.
> `ListaEquipo` y `PerfilEquipo` siguen tomando al jefe **de la sesión**, así que
> al abrir el perfil de otra persona esa pestaña mostraría el equipo de *quien
> mira*, no el del perfil abierto — con el nombre de otro en la cabecera. El
> control la trae porque `MiPerfil.aspx` la necesita; la pantalla nueva la oculta
> siempre, sin pasar por `MostrarPestanaEquipo(esJefe)`.

### 3.2 El JavaScript, también una sola vez

`miPerfil.js` gana **una variable**, `_codObjetivo` — vacía en «Mi perfil»,
fijada por el buscador en la pantalla nueva — y `PostPerfil` la adjunta a cada
llamada. Con eso las 790 líneas sirven a las dos pantallas.

Salvo los **dos sitios que no pasan por `PostPerfil`**:

- el `FormData` de la subida de documentos, que suma un campo `codUsuario`;
- las URL de `DescargarPerfil.ashx`, que suman un parámetro.

Son, otra vez, exactamente las tres superficies de la §1. Si aparece una cuarta
superficie en el futuro, este es el lugar donde se nota.

La pantalla nueva suma un `.js` pequeño y propio: pintar la lista de personal y
fijar `_codObjetivo` al elegir a alguien.

### 3.3 El menú

`MenuDos` + `PerfilMenu` para los perfiles **14 y 18**, clonando filas modelo que
ya funcionan en producción en vez de inventar las 18 columnas.

Dos trampas conocidas de `PerfilMenu`, las dos documentadas en el script de Horas
Extras:

- La semántica de `Estado` está **invertida**: `'0'` **muestra** la opción y `'1'`
  la oculta.
- Una hoja solo se ve si su **grupo padre** también tiene `Estado = '0'` para ese
  perfil. Hay que dar permiso en las dos filas.

---

## 4. Pruebas

`CapaPruebas` no alcanza al proyecto web (ver «Terreno verificado»). Por eso la
regla vive en `CapaNegocio/NegPerfilAcceso.cs` y no dentro del handler: si
viviera en `ReporteTareas/clases/`, la función de la que depende toda la
seguridad de este diseño quedaría **sin una sola prueba**.

Es el mismo razonamiento que el módulo ya dejó escrito para `Fn_RTA_EsSubordinado`:

> *«La guarda, en una función y no escrita a mano dentro del procedimiento. El
> motivo es que se pueda PROBAR. (...) Una prueba que verifica un duplicado del
> código no verifica el código.»*

### Pruebas unitarias — `NegPerfilAccesoTests` (MSTest, `vstest.console.exe` de VS2019)

| Caso | Esperado |
|---|---|
| Sin código pedido | el de la sesión |
| Perfil 14 pide uno ajeno | el ajeno |
| Perfil 18 pide uno ajeno | el ajeno |
| Perfil cualquiera pide uno ajeno | **rechazo** |
| Perfil cualquiera pide el suyo | permitido |
| Código pedido con relleno a los lados | recortado — todo el módulo compara con `TRIM` |
| `Id_Perfil` ausente o no numérico | **rechazo** |

### Verificación contra la base

Esto no lo cubre ninguna prueba unitaria y hay que hacerlo a mano:

1. Talento Humano edita a otro → `Usu_Modificacion` guarda **el código de Talento
   Humano**, no el del empleado. Es la razón de ser de todo el §2.
2. El empleado sigue editando lo suyo desde `MiPerfil.aspx`, sin regresión.
3. Un usuario común llamando al `.ashx` a mano con un `codUsuario` ajeno →
   rechazado. Se prueba **sin pasar por la pantalla**, que es justamente el caso
   que la pantalla no protege.
4. Descarga: Talento Humano baja el documento y el CV de otro; el usuario común,
   no.
5. Uno de los `Cod_Usuario` repetidos → mensaje claro, no un error genérico.

---

## 5. Despliegue

El SQL siempre antes que los binarios, como manda `DESPLIEGUE.md`.

1. Script de columnas de autor (`Emp_CargaFamiliar` y `Empleados`)
2. Script de los 15 procedimientos con `@Usu_Accion` + `Sp_RTA_PerfilPersonalLista`
3. Script de menú (`MenuDos` + `PerfilMenu`, perfiles 14 y 18, grupo padre incluido)
4. Binarios

> **Esto viaja junto con las fases 2, 3a y 3b**, que llevan desde el 2026-09-15
> con el esquema aplicado y los binarios sin desplegar. **Un solo despliegue, no
> dos.**

### Las trampas que este módulo ya cobró

- El `.aspx` nuevo y el `.ascx`, **con BOM**. `MiPerfil.aspx` ya salió una vez a
  producción con caracteres raros por no llevarlo: `Web.config` declara
  `windows-1252` y no trae `fileEncoding`.
- **Regenerar el paquete de despliegue** (`FolderProfile`, Release) y comparar el
  `?v=` de `miPerfil.js` dentro del paquete contra el del fuente. Se olvidó cuatro
  veces seguidas; el síntoma es la pantalla vieja sobre una base nueva, **sin
  ningún error**.
- Compilar con el **MSBuild de VS2019**, no con el del PATH.
- Copiar **sin sincronizar**: un `robocopy /MIR` borra `connections.config` y el
  sitio no arranca.
- Confirmar que `ReporteTareas/descargas/perfil/web.config` viajó. Su ausencia es
  lo único que no da ningún error, y sin él IIS sirve como archivos estáticos las
  cédulas y partidas de nacimiento que la gente adjunta.

---

## 6. Lo que queda fuera, a propósito

- **La pestaña «Datos personales» sigue con candado** para todos, incluido
  Talento Humano. Esos campos viven en `Empleados` y los alimenta SAP; abrirlos
  exige antes decidir qué los pisa y cuándo. No es parte de este trabajo.
- **Los `Cod_Usuario` repetidos no se arreglan desde aquí.** Hay 4 usuarios
  activos con código repetido, y en un caso son dos personas distintas. Los
  procedimientos se niegan a entregar o escribir en ese caso, y este diseño no
  cambia esa decisión: ni siquiera los lista. Necesitan que alguien les corrija
  el código en `R_Usuarios`. **Talento Humano tiene que saberlo**, o va a buscar
  a esas personas y no las va a encontrar.
- **Sin bitácora aparte y sin antes/después.** Se descartó explícitamente: el
  autor por fila es el mínimo honesto, y lo demás se puede agregar después sin
  rehacer nada de esto.
- **Nada cambia para las jefaturas.** La pestaña «Equipo» sigue siendo de solo
  lectura sobre el equipo directo. `Sp_RTA_PerfilEquipo` y
  `Sp_RTA_PerfilEquipoLista` no se tocan.

---

## 7. Riesgos conocidos

| Riesgo | Mitigación |
|---|---|
| Toda la seguridad descansa en una función | Vive en `CapaNegocio`, cubierta por 7 pruebas unitarias, y cierra por defecto ante cualquier entrada rara. |
| Se apila sobre tres fases sin desplegar | El diseño minimiza el diff sobre esas fases: el handler cambia 18 líneas, la capa de negocio solo firmas, y el marcado se **mueve** sin reescribirse. |
| Primer `.ascx` del repositorio | El movimiento es solo marcado; si sale mal, se nota al primer render y no en producción. |
| 15 procedimientos cambiados a mano | Un solo script, los 15 juntos, revisable de una sentada. |
| Tocar dos tablas de Talento Humano | Solo se **agregan** columnas anulables; ningún `INSERT` existente las nombra. |
