# Datos personales editables por Talento Humano — Diseño

**Fecha:** 2026-09-17
**Estado:** aprobado, pendiente de plan de implementación
**Rama:** `ProyectoNuevosCambios`
**Pantalla:** `PerfilesPersonal.aspx`, pestaña «Datos personales»

> **Sin datos personales.** Este documento describe estructura y reglas. El repositorio es público.

---

## Qué se pide

Que Talento Humano pueda **editar** la pestaña «Datos personales» desde
`PerfilesPersonal.aspx`. Hoy es de sólo lectura para todos —lleva un candado y un
rótulo «Gestionado por Talento Humano»— porque así lo decidió el diseño de la
edición por Talento Humano, que la dejó explícitamente fuera de alcance.

Este documento levanta esa decisión.

---

## Decisiones tomadas

| Pregunta | Decisión |
|---|---|
| ¿Quién edita, y a quién? | **Los perfiles 14 y 18, a cualquier colaborador.** Se reusa la regla de la entrega 1; no se agrega ninguna dimensión de permiso nueva |
| ¿Qué campos? | **Los ocho**: nombre, cédula, fecha de nacimiento, cargo, área, ciudad, jefe inmediato y correo |
| ¿Y el horario? | **De sólo lectura, con enlace a su módulo.** Único campo de la pestaña que queda fuera |
| ¿Y quien no tiene ficha en `Empleados`? | **Se adopta la huérfana si la hay, y si no se le crea al guardar.** Son 116 de 228 |
| ¿Dónde se escriben los campos con respaldo? | **En las dos tablas**, `Empleados` y `R_Usuarios` |

**El usuario pidió explícitamente los ocho, incluidos los tres estructurales.** Se
le señalaron las consecuencias —ver §7— y las confirmó. Queda dicho acá para que
quien lea esto no lo tome por un descuido.

---

## Terreno verificado

Medido **contra la base de producción** el 2026-09-17, con consultas de lectura.
No es supuesto.

### El bloqueo del diseño anterior no se sostiene

La decisión de dejar esta pestaña con candado decía: *«Esos campos viven en
`Empleados` y los alimenta SAP; abrirlos exige antes decidir qué los pisa y
cuándo.»*

**Nada automatizado escribe en `Empleados`.** Comprobado por dos vías:

| Comprobación | Resultado |
|---|---|
| Procedimientos que escriben en `Empleados` | 5, **todos de la aplicación**: `Sp_RTAInsUpdEmpleado`, `Sp_RTACambiarEstadoEmpleado`, `Sp_InsUpdHsCln`, `Sp_RTA_PerfilGuardarContacto`, `Sp_RTAListaSolicitud` |
| Trabajos del SQL Agent | 11 habilitados; **ninguno toca `Empleados`**. Sincronizan tareas de Aranda, pedidos, segmentación, marcas y costos |

`Sp_RTAInsUpdEmpleado` es el de `RRHHEmpleados.aspx`: **Talento Humano ya edita
estos campos hoy**, desde otra pantalla. Esto no abre una puerta nueva; mueve una
puerta que ya existía.

> **Una comprobación que resultó inservible, y por qué se dice.** El primer
> intento fue mirar `Empleados.Fec_Modificacion` buscando escrituras masivas. Sólo
> **4 de 133** filas la tienen: la columna no se mantiene, así que la ausencia de
> actualizaciones masivas no probaba nada. Quien repita este análisis no debe
> apoyarse en esa columna.

### Más de la mitad de la gente no tiene ficha

Contado sobre la población que el buscador de la pantalla realmente lista —el
filtro de `Sp_RTA_PerfilPersonalLista`: `ISNULL(EstadoUsuario,0) = 0` y sin
`Cod_Usuario` repetido—:

| | |
|---|---|
| Usuarios que la pantalla lista | 228 |
| Con fila en `Empleados` | 112 |
| **Sin fila** | **116** |
| De esos, sin cédula registrada | 65 |

(Con el filtro de estado solo, sin descartar los `Cod_Usuario` repetidos, son 232
y 120. Los repetidos quedan fuera por §10, no por esta decisión.)

Cinco de los ocho campos viven en `Empleados`. Para más de la mitad de la gente,
editarlos no tiene dónde aterrizar. De ahí la decisión de crear la ficha (§6).

### Hay 21 fichas sin dueño, y 4 son de gente que figura sin ficha

`Empleados` tiene 133 filas y sólo 112 llevan `Cod_Usuario`. Las otras **21 están
huérfanas**: son fichas de personas reales que nadie enlazó nunca al usuario.

De esas 21, **4 tienen la misma cédula que alguien a quien la pantalla considera
«sin ficha»** —y ninguna cédula coincide con más de una, así que no hay
ambigüedad—. Si el guardado insertara sin mirar, esas 4 personas quedarían con
**dos fichas** en `RRHHEmpleados.aspx`. De ahí la regla de adopción de §6.

### «Pertenecer a GTH» no era una regla utilizable

El pedido original hablaba de limitar la edición a GTH. La base da **dos
respuestas distintas**:

| Campo | Valor | Personas |
|---|---|---|
| `R_Usuarios.Departamento` | `GTH` | 10 activos |
| `Empleados.AreaTrabajo` | `TALENTO HUMANO` | 2 |

Y de los 10 de `Departamento = 'GTH'`, **sólo 7 tienen perfil 14 o 18**: los otros
tres —perfiles 3, 7 y 21— hoy ni siquiera pueden abrir la pantalla. A la inversa,
de los 7 usuarios con perfil 14, **uno no figura en GTH**.

Además el área de la ficha dice «ADMINISTRACIÓN», «RRHH UIO DOS» y «MARKETING»
para gente del departamento GTH: **ese campo no sirve para identificarlos**.

Por eso la regla es el perfil y no el departamento. Los dos conjuntos no coinciden,
y el perfil es el único que ya está probado y en producción.

### La cabecera lee con respaldo, y la precedencia cambia por campo

`Sp_RTA_PerfilColaborador` arma los campos así:

```sql
NombreCompleto = ISNULL(NULLIF(LTRIM(RTRIM(e.Nombre)), ''), u.Nom_Usuario)   -- Empleados manda
Cedula         = ISNULL(NULLIF(LTRIM(RTRIM(u.Cedula)), ''), e.Cedula)        -- R_Usuarios manda
Cargo          = ISNULL(NULLIF(LTRIM(RTRIM(e.PuestoTrabajo)), ''), u.Cargo)  -- Empleados manda
Area           = ISNULL(NULLIF(LTRIM(RTRIM(e.AreaTrabajo)), ''), u.Departamento)
```

**La precedencia no es la misma para todos.** Escribir en un solo lado deja el otro
viejo, y según el campo la pantalla mostraría el valor anterior después de guardar:
el usuario guarda, ve lo de antes, y vuelve a guardar. De ahí la decisión de
escribir en las dos tablas (§2).

### El horario no es un campo

Sale de `R_UsuarioHorarioLaboral`, por subconsulta y no por `LEFT JOIN`, porque
**5 usuarios tienen más de una asignación activa** y un join los duplicaría. Sólo
88 de 231 tienen horario asignado.

---

## 1. La regla de acceso

**No se escribe ninguna regla nueva.** Se reusa la de la entrega 1:

- `NegPerfilAcceso.PerfilesRRHH = { 14, 18 }`, con sus 11 pruebas.
- `PerfilIdentidad.Objetivo(context, codPedido)` decide de quién es el perfil.
- `PerfilIdentidad.Autor(context)` dice quién lo está tocando, siempre de la sesión.

La acción nueva es una escritura más del módulo y pasa por el mismo camino que las
otras quince. Si aparece un `14` o un `18` literal en el código de esta entrega,
está mal.

---

## 2. Los ocho campos, y de dónde salen

| Campo | Lectura hoy | Se escribe en |
|---|---|---|
| Nombre | `e.Nombre` → `u.Nom_Usuario` | **las dos** |
| Cédula | `u.Cedula` → `e.Cedula` | **las dos** |
| Cargo | `e.PuestoTrabajo` → `u.Cargo` | **las dos** |
| Área | `e.AreaTrabajo` → `u.Departamento` | **las dos** |
| Fecha de nacimiento | sólo `Empleados` | `e.Fecha_nacimiento` |
| Ciudad | sólo `Empleados` | `e.Ciudad` |
| Jefe inmediato | sólo `R_Usuarios` | `u.Cod_Jefe_Inm` |
| Correo | sólo `R_Usuarios` | `u.E_Mail` |

**Escribir en las dos tablas tiene una consecuencia que no es local a esta
pantalla:** `R_Usuarios` es el catálogo que usa todo el sistema. Cambiar el nombre
acá lo cambia en los listados, en las aprobaciones y en los correos. No hay forma
de evitarlo sin dejar la pantalla mintiendo después de guardar.

### Las dos columnas no miden lo mismo

Cuatro campos se escriben en las dos tablas, y en los cuatro las columnas tienen
anchos distintos. **El límite efectivo es el más angosto de los dos**, porque un
valor que entra en una y no en la otra no deja la pantalla a medias: hace fallar el
`UPDATE` entero con «String or binary data would be truncated».

| Campo | `Empleados` | `R_Usuarios` | Límite a aplicar |
|---|---|---|---|
| Nombre | `Nombre` nvarchar(350) | `Nom_Usuario` varchar(100) **NOT NULL** | **100** |
| Cédula | `Cedula` nvarchar(200) | `Cedula` varchar(32) | **32** |
| Cargo | `PuestoTrabajo` varchar(250) | `Cargo` varchar(128) | **128** |
| Área | `AreaTrabajo` nvarchar(150) | `Departamento` varchar(128) | **128** |

Los otros cuatro campos viven en una sola tabla: `Fecha_nacimiento` nvarchar(50),
`Ciudad` nvarchar(150), `Cod_Jefe_Inm` varchar(100) y `E_Mail` varchar(100).

`R_Usuarios.Nom_Usuario` es **NOT NULL**: el nombre no se puede dejar en blanco.
Es el único campo obligatorio de los ocho, y la validación tiene que decirlo con
ese nombre y no dejar que el error salga de SQL Server.

### `Fecha_nacimiento` es texto, no fecha

`Empleados.Fecha_nacimiento` es `nvarchar(50)` con formato `dd/MM/yyyy`. Hay 129
fechas en ese formato y 80 con el día por encima de 12; interpretarlas como
`mm/dd/yyyy` rompe 80 de 133 **sin dar error**. La validación va en `CapaNegocio`,
donde `NegPerfilCampos.EdadDesdeTexto` ya resuelve exactamente este problema y
tiene pruebas. Se escribe en el mismo formato que ya está guardado.

---

## 3. El jefe inmediato no es un texto libre

`R_Usuarios.Cod_Jefe_Inm` guarda un `Cod_Usuario`, y de él cuelgan la cadena de
aprobaciones de vacaciones y permisos y la pestaña «Equipo».

Por lo tanto **no se edita escribiendo**: se elige de una lista de usuarios
activos, y se guarda el código, no el nombre. Un texto libre acá deja la solicitud
sin aprobador y nadie se entera hasta que alguien pide vacaciones.

Dos guardas en el servidor:

1. El código elegido tiene que existir y estar activo en `R_Usuarios`.
2. **Nadie puede ser su propio jefe.** `Cod_Jefe_Inm = Cod_Usuario` deja a esa
   persona sin quien le apruebe nada, y la consulta de equipo la devolvería como
   subordinada de sí misma.

No se comprueba que el grafo no tenga ciclos más largos: A jefe de B y B jefe de A
es posible y este diseño no lo impide. Es una decisión consciente —el sistema hoy
tampoco lo impide desde `AdministrarUsuarios.aspx`— y meterse ahí es otro trabajo.

---

## 4. El correo

`R_Usuarios.E_Mail` es a donde llegan las notificaciones, y además es por donde se
le atribuye la firma a Talento Humano: `RTA_CodigoUsuarioPorCorreo` busca por
correo y, **ante dos usuarios con el mismo correo, toma el de `Id_Usuario` más
alto**.

Así que duplicar un correo no da error: cambia en silencio a quién se le atribuye
una firma. Se valida formato y **se avisa —sin bloquear— si ese correo ya lo tiene
otro usuario activo**. Avisar y no bloquear porque el sistema ya convive con
correos repetidos y bloquear impediría corregir justamente esos casos.

---

## 5. La cédula

Es el puente de identidad entre `R_Usuarios` y `Empleados`, y el módulo de horas
extras lo usa para enlazar. Se valida que tenga 10 dígitos y **dígito verificador
ecuatoriano válido**, y se avisa si ya la tiene otro usuario activo.

**Esa comprobación hay que escribirla; no existe en el código.** Una versión
anterior de este documento decía que «ya existe en el repositorio»: es falso. Lo
único que existe es `cedula_valida()` en `docs/sql/generar-carga-horas-extras.py`,
un generador de un solo uso en Python que no forma parte de la aplicación y que
nada en C# puede llamar. De ahí salen los «64 de 64»: es lo que validó esa carga,
no una prueba de que el código exista. La función va escrita de nuevo en
`CapaNegocio`, con pruebas, siguiendo el mismo algoritmo:

```
10 dígitos, todos numéricos
los dos primeros (provincia) entre 01 y 24, o 30
el tercero menor que 6
a los nueve primeros: los de posición par se multiplican por 2 —y si el
producto pasa de 9, se le resta 9—, los de posición impar por 1
el décimo dígito tiene que ser (10 - (suma % 10)) % 10
```

### Sólo se valida si el campo cambió

**Tres personas que hoy están en la base no pasan esa validación**: una tiene 11
dígitos y dos tienen el tercer dígito en 9 —probablemente pasaportes o errores de
carga antiguos—. Si la validación se aplicara siempre, Talento Humano no podría
guardarle a esas tres personas **ningún** campo: quedarían trabadas por un valor
que no estaban tocando.

Por eso la regla es: **la cédula se valida cuando el valor enviado difiere del que
está guardado.** Si no cambió, pasa tal como está. Lo mismo vale para el formato
del correo, por la misma razón.

---

## 6. La ficha: primero se adopta, después se crea

Cuando la persona no tiene fila en `Empleados`, el procedimiento hace **dos cosas,
en este orden**:

1. **Busca una ficha huérfana con la misma cédula** —una fila de `Empleados` con
   `Cod_Usuario` vacío y esa cédula— y, si la encuentra, le pone el `Cod_Usuario`.
   Adopta la que ya estaba en vez de crear otra. Son 4 casos hoy.
2. **Sólo si no encontró ninguna, inserta** una ficha nueva con el `Cod_Usuario`,
   los campos que se estén guardando y `Estado = 'Activo'`. `IdEmpleado` es
   `IDENTITY` y es la única columna obligatoria de la tabla, así que el `INSERT`
   no necesita nada más.

El paso 1 no es una optimización: sin él, esas 4 personas aparecen dos veces en
`RRHHEmpleados.aspx` y ningún mensaje lo advierte. La búsqueda es por cédula
exacta, sin `LIKE` y sin normalizar más allá de `LTRIM`/`RTRIM`; si la cédula
viene vacía —65 de los 116— no hay con qué emparejar y se va derecho al paso 2.

**El efecto lateral, dicho de frente:** esas personas empiezan a aparecer en
`RRHHEmpleados.aspx`, que hoy no las lista porque no tienen ficha. Es un cambio
visible en otra pantalla, para otra gente. Se eligió igual porque la alternativa
—que Talento Humano tenga que ir a crear la ficha a mano en la otra pantalla y
volver— es el mismo resultado con más pasos.

**Hay que avisarle a Talento Humano antes de desplegar**, o van a ver crecer esa
lista sin saber por qué.

---

## 7. Lo que cambia fuera de esta pantalla

Se escribe acá porque es la parte del diseño que no se ve leyendo el código:

| Campo | Qué mueve fuera del perfil |
|---|---|
| **Jefe inmediato** | Quién aprueba las vacaciones y los permisos de esa persona, y en el equipo de quién aparece |
| **Correo** | A dónde llegan sus notificaciones, y a quién se le atribuye la firma de Talento Humano |
| **Nombre** | El nombre que se ve en todo el sistema: listados, aprobaciones, correos |
| **Cédula** | El enlace entre `R_Usuarios` y `Empleados`, y la identidad en horas extras |
| **Crear la ficha** | Esa persona aparece en `RRHHEmpleados.aspx` |

---

## 8. Trazabilidad

La entrega 1 dejó el mecanismo: la columna de autor guarda **quién hizo el
cambio**, no de quién es el perfil. Esta entrega lo usa igual.

- `Empleados` → `Usu_ModificacionCod`, junto a `Fec_Modificacion` e
  `Ip_Modificacion`, que ya existen. Esa columna **la crea la entrega 1**
  (`docs/sql/2026-09-16-perfil-autor-columnas.sql`), porque la `Usu_Modificacion`
  original es `numeric(5)` y no admite un `Cod_Usuario`.
- `R_Usuarios` → **no tiene ninguna columna de auditoría**. Verificado contra
  producción el 2026-09-17: no hay autor, ni fecha de modificación, ni IP; lo
  único parecido es `Fec_Creacion`, y es `varchar(50)`. Hay que agregar las tres,
  con el mismo criterio que el resto del módulo y todas anulables, de modo que
  ningún `INSERT` existente las nombre:

  ```sql
  Usu_ModificacionCod VARCHAR(50)  NULL
  Fec_Modificacion    DATETIME     NULL
  Ip_Modificacion     VARCHAR(64)  NULL
  ```

  Sólo el autor no alcanza: saber quién cambió el jefe de alguien sin saber cuándo
  no sirve para reconstruir nada.

Sin esto, los cambios sobre los cuatro campos que viven en `R_Usuarios` —incluidos
jefe y correo, los dos más delicados— quedarían sin rastro de quién los hizo.

> **Esta entrega depende de la 1.** Al 2026-09-17 los dos scripts de la entrega 1
> todavía **no están corridos** en producción: `Empleados.Usu_ModificacionCod` no
> existe y ninguno de los 14 procedimientos declara `@Usu_Accion`. El plan de esta
> entrega no puede empezar sin eso.

---

## 9. Pruebas

`CapaPruebas` sólo referencia `CapaEntidad` y `CapaNegocio`. Lo que se pueda probar
va ahí, y lo que no, se dice.

**Probable, y por lo tanto va en `CapaNegocio`:**

- La validación de cada campo: cédula (10 dígitos y dígito verificador, **escrita
  de cero** —ver §5—, incluidas las tres cédulas reales que hoy no la pasan y que
  tienen que poder guardarse mientras no se toquen), correo (formato), fecha de
  nacimiento (`dd/MM/yyyy`, con el día por encima de 12 sin confundirse con el
  mes), el nombre obligatorio y no más largo de 100, y que nadie sea su propio
  jefe.
- Que un campo que **no cambió** no se valide: es lo que permite guardar a las
  personas cuyos datos viejos no pasarían la validación de hoy.
- Que la validación rechace el conjunto entero si un campo falla, y no guarde a
  medias.

**No probable, y se verifica a mano:** que el procedimiento escriba en las dos
tablas, que cree la ficha cuando falta, y que el autor quede registrado.

---

## 10. Lo que queda fuera, a propósito

- **El horario.** Sale de `R_UsuarioHorarioLaboral` y tiene su propio módulo, con
  tramos y con 5 usuarios que tienen más de una asignación activa —algo que esta
  pestaña, que muestra un solo valor, no puede representar. Queda de sólo lectura
  con un enlace. Si algún día se edita desde acá, es llamando a ese módulo y no
  reimplementándolo.
- **Los ciclos en la cadena de jefes** más largos que uno. Ver §3.
- **Unificar `Emp_CargaFamiliar` entre las dos pantallas.** Sigue pendiente de la
  entrega 2 y es un trabajo propio.
- **Los `Cod_Usuario` repetidos.** Los procedimientos del módulo se siguen negando
  a escribir en ese caso y el buscador no los lista. Esta entrega no lo cambia.

---

## 11. Riesgos conocidos

| Riesgo | Mitigación |
|---|---|
| Un jefe mal asignado deja solicitudes sin aprobador | Se elige de lista, no se escribe; se valida que exista, esté activo y no sea uno mismo |
| Escribir en `R_Usuarios` cambia cosas fuera del perfil | Está dicho en §7 y hay que avisarle a Talento Humano. No hay forma de evitarlo sin que la pantalla mienta al guardar |
| La ficha creada aparece en `RRHHEmpleados.aspx` | Avisar antes de desplegar. Son hasta 116 personas |
| Crear una ficha para alguien que ya tenía una huérfana lo duplica | Se adopta por cédula antes de insertar (§6). Hoy son 4 casos y ninguno ambiguo |
| Validar la cédula siempre trabaría a quien ya la tiene mal cargada | Sólo se valida el campo que cambió (§5). Son 3 personas |
| Un valor entra en una tabla y no en la otra y el guardado falla entero | El límite es el de la columna más angosta, y está en §2 campo por campo |
| `Fecha_nacimiento` es texto y se puede romper en silencio | La validación va en `CapaNegocio`, con pruebas, sobre el mismo formato que ya está guardado |
| Un correo duplicado cambia a quién se le atribuye una firma | Se avisa al guardar; no se bloquea, porque bloquear impediría corregir los duplicados que ya existen |
