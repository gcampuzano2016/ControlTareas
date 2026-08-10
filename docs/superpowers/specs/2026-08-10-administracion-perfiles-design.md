# Administración de perfiles

Fecha: 2026-08-10
Rama: `ProyectoNuevosCambios`
Antecedente directo: `2026-08-05-administracion-usuarios-design.md`

## Problema

No existe ninguna pantalla que administre el catálogo de perfiles. Se pueden asignar
usuarios a un perfil (`Perfiles.aspx`, que pese al nombre es un listado de usuarios) y
se pueden repartir menús por perfil (`ParametrizacionMenuPerfil.aspx`), pero el catálogo
en sí —crear, renombrar, dar de baja un perfil— sólo se toca por base de datos.

El pedido concreto es poder **eliminar un perfil entero**.

## Terreno verificado

### Ya existe media infraestructura sin usar

`CapaDato/DaoPerfiles.cs` y `CapaNegocio/NegPerfiles.cs` implementan
`Sp_RTAInsertaNuevoPerfil` y `Sp_RTActualizarPerfil`. Ningún handler ni pantalla los
llama. Crear y editar están escritos y muertos; falta la pantalla y falta eliminar.

### De un perfil cuelgan tres cosas, no una

| Referencia | Qué guarda |
|---|---|
| `R_Usuarios.Id_Perfil` | qué usuarios tienen ese perfil |
| `PerfilMenu.IdPerfil` | qué pantallas ve ese perfil |
| `PerfilInicio` | a qué página entra ese perfil al iniciar sesión |

Borrar el perfil sin mirar estas tres deja usuarios apuntando a un perfil inexistente:
en la práctica **entran al sistema y se quedan sin menú**.

### El catálogo bueno es `dbo.Perfiles`

`dbo.Perfiles(IdPerfiles, NombrePerfil, Estado)` es la tabla que corresponde a
`R_Usuarios.Id_Perfil`. `dbo.R_Perfil` existe pero **no** corresponde: sólo tiene los
IDs 1..5 con otros nombres. Ver la memoria del proyecto
`dos-catalogos-y-dos-estados-en-r-usuarios`. Todo lo de acá usa `dbo.Perfiles`.

Consecuencia conocida de esa confusión: `PerfilMenu` tiene filas para los perfiles 18 y
19, que no existen en `R_Perfil`. Son huérfanas. Es exactamente el estado que este
trabajo debe evitar producir.

## Decisiones

**Eliminar se bloquea si el perfil tiene usuarios.** No se reasigna, no se borra en
cascada sobre usuarios: la pantalla rechaza la operación e informa cuántos hay, para que
se reasignen primero. Es la única opción que no puede dejar a nadie sin menú por un
descuido, y la reasignación masiva ya tiene su propia pantalla.

**La validación vive en el servidor.** El JS puede deshabilitar el botón para guiar, pero
la cuenta de usuarios y el rechazo van dentro del procedimiento almacenado. El handler es
alcanzable directamente por HTTP; una validación que sólo esté en el navegador no es una
validación.

**Los menús y la página de inicio se borran con el perfil.** Cuando el perfil ya no tiene
usuarios y se procede a borrarlo, sus filas de `PerfilMenu` y `PerfilInicio` se borran en
la misma transacción. Son configuración suya: sin el perfil no significan nada y
quedarían huérfanas.

**ABM completo, no sólo eliminar.** La pantalla lista, crea, edita y elimina. Crear y
editar salen casi gratis porque los SPs y su capa de datos ya están escritos, y dejan un
módulo coherente en vez de una pantalla que sólo sabe borrar.

## Diseño

### Componentes

Se sigue el patrón de la Administración de Usuarios (`2026-08-05`):

- **`Formulario/ParametrizacionPerfiles.aspx`** — pantalla: buscador, tabla y formulario.
- **`Formulario/AdministrarPerfiles.ashx`** — handler con sesión obligatoria, una acción
  por operación: `Listar`, `Guardar`, `Eliminar`.
- **`js/parametrizacionPerfiles.js`** — la pantalla del lado del navegador.
- **`CapaDato/DaoPerfiles.cs`** — se le agrega `EliminarPerfil`; ya tiene insertar y
  actualizar.
- **`CapaNegocio/NegPerfiles.cs`** — se le agrega el paso correspondiente.
- **SP nuevo `Sp_RTA_EliminarPerfil`** — descrito abajo.

### El listado

Una fila por perfil con: nombre, estado y **cantidad de usuarios asignados**. Esa última
columna es la que hace utilizable el botón: se ve de antemano qué perfiles se pueden
eliminar, sin tener que intentarlo y recibir un rechazo.

El conteo sale de un `LEFT JOIN` contra `R_Usuarios` agrupando por perfil. Se decide
explícitamente contar **todos** los usuarios que tienen el perfil, activos o no: un
usuario inactivo con ese perfil sigue siendo una referencia, y reactivarlo después de
borrar el perfil lo dejaría sin menú.

### `Sp_RTA_EliminarPerfil`

Recibe `@IdPerfiles`. Devuelve un código y un mensaje, como el resto de los SPs de la
aplicación.

1. Si el perfil no existe → rechaza: "El perfil ya no existe."
2. Cuenta `R_Usuarios` con ese `Id_Perfil`. Si hay alguno → rechaza:
   "Este perfil tiene N usuarios asignados. Reasígnelos antes de eliminarlo."
3. Si no hay ninguno, dentro de una transacción:
   - `DELETE FROM PerfilMenu WHERE IdPerfil = @IdPerfiles`
   - `DELETE` de la fila de `PerfilInicio` de ese perfil
   - `DELETE FROM dbo.Perfiles WHERE IdPerfiles = @IdPerfiles`
4. Confirma o revierte entera.

Los tres borrados van en una transacción porque un borrado a medias es peor que no
borrar: dejaría el perfil vivo pero sin menús, o los menús sin perfil.

**Ojo con el casing de `PerfilMenu`**: la columna es `IdPerfil`, sin guion bajo, a
diferencia de `R_Usuarios.Id_Perfil`. Está documentado en la memoria
`modelo-menu-y-feriado` y es una fuente conocida de errores.

### Seguridad

El handler valida sesión antes de cualquier acción, como `AdministrarUsuarios.ashx`. Sin
sesión responde el mismo error que el resto y no ejecuta nada. El `IdPerfiles` llega como
parámetro tipado al SP, nunca concatenado.

### Registro en el menú

La pantalla se registra en `MenuDos` y se habilita en `PerfilMenu` para los perfiles 2,
18 y 19 — el mismo criterio del registro de `ParametrizacionUsuarios` (`c658f91`).
Recordar la semántica invertida: en `PerfilMenu`, `Estado='0'` **muestra** la opción y
`Estado='1'` la oculta.

Se entrega como script SQL en `docs/superpowers/plans/sql/`, siguiendo la costumbre del
proyecto.

## Verificación

1. Compilar con MSBuild y confirmar EXIT 0.
2. Sobre la pantalla, comprobar los tres caminos del borrado:
   - perfil **con** usuarios → rechazado, con el número correcto en el mensaje
   - perfil **sin** usuarios pero **con** menús → borrado, y `PerfilMenu` queda sin filas
     de ese perfil
   - perfil inexistente (llamando al handler con un id inventado) → rechazado sin error
     de servidor
3. Confirmar que un usuario de un perfil vecino sigue viendo su menú intacto.

**No verificable desde el entorno de desarrollo actual:** la base `192.168.11.14` no
responde desde esta máquina, así que los conteos reales de usuarios por perfil y la
existencia exacta de la tabla de `PerfilInicio` deben confirmarse contra la base antes de
implementar. El nombre y las columnas de `PerfilInicio` están tomados del código
(`DaoPerfilInicio.cs`), no de la base.

## Fuera de alcance

- **Reasignación masiva de usuarios entre perfiles.** Es lo que habría que hacer antes de
  poder borrar un perfil poblado, y ya tiene pantalla propia.
- **Limpiar las filas huérfanas de `PerfilMenu`** para los perfiles 18 y 19. Es anterior a
  este trabajo y merece decidirse aparte.
- **Unificar `dbo.Perfiles` y `dbo.R_Perfil`.** Es la raíz de varias confusiones, pero
  tocarlo excede este pedido.
- **Credenciales de SQL en el código fuente.** `DaoArandaDb.cs` y
  `DaoReporTareaAranda.cs` traen servidor, usuario `sa` y contraseña escritos y
  commiteados. No es parte de este trabajo, pero está anotado.

## Riesgos

El riesgo real es borrar un perfil que parecía libre y no lo estaba, por una referencia
que este diseño no contempló. Se acota de dos maneras: la transacción, que impide un
borrado a medias, y el hecho de que sólo se borran las tres tablas identificadas. Si
aparece una cuarta referencia, el `DELETE` sobre `dbo.Perfiles` fallará por integridad
referencial y la transacción revertirá sola, en vez de dejar la base inconsistente.

Conviene confirmar contra la base si existen claves foráneas declaradas hacia
`dbo.Perfiles`. Si no existen —cosa probable en este esquema— esa red de seguridad no
está, y entonces la lista de tablas a revisar tiene que verificarse a mano antes de
implementar.
