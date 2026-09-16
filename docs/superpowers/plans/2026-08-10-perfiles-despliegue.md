# Despliegue — Administración de perfiles

Estado al 2026-08-11: **la base ya está lista, falta subir los archivos.**

Los tres procedimientos almacenados ya están creados en producción y probados. La opción
de menú también está registrada, pero **oculta a propósito**: se activa al final, cuando
la pantalla ya esté publicada.

## Qué se puede hacer hoy en la pantalla

| Operación | Estado |
|---|---|
| Listar perfiles con su cantidad de usuarios | funciona |
| Crear un perfil | funciona |
| Eliminar un perfil sin usuarios | funciona |
| Eliminar un perfil con usuarios | lo rechaza, como se diseñó |
| **Editar un perfil** | **no disponible** — ver abajo |

**Por qué no hay edición:** el procedimiento `Sp_RTActualizarPerfil`, que parecía servir,
en realidad reasigna el perfil de un usuario (`@IdCambioPerfil`, `@IdUsuario`,
`@CorreoCambio`). Usarlo habría respondido "Datos Guardados con Exito" sin cambiar nada.
Se quitó el botón hasta que exista un procedimiento correcto. Si se quiere agregar, hace
falta un `Sp_RTA_ActualizarPerfilCatalogo(@IdPerfiles, @NombrePerfil, @Estado)` y su
método en `DaoPerfiles`.

## Orden de subida

**1. Binarios** de `bin/` — llevan la capa de datos y el handler nuevos:

- `CapaDato.exe`
- `CapaEntidad.exe`
- `CapaNegocio.exe`
- `ReporteTareas.dll`

**2. Archivos nuevos** de la aplicación:

| Origen | Destino |
|---|---|
| `ReporteTareas/js/parametrizacionPerfiles.js` | `js/parametrizacionPerfiles.js` |
| `ReporteTareas/Formulario/AdministrarPerfiles.ashx` | `Formulario/AdministrarPerfiles.ashx` |
| `ReporteTareas/Formulario/ParametrizacionPerfiles.aspx` | `Formulario/ParametrizacionPerfiles.aspx` |

**3. Activar la opción del menú**, recién ahora:

```
docs/superpowers/plans/sql/2026-08-10-perfiles-menu-activar.sql
```

Ese script pone en `Estado='0'` las tres filas de `PerfilMenu` (perfiles 2, 18 y 19), que
es lo que las hace visibles — la semántica está invertida. Es seguro ejecutarlo dos veces.

**El orden importa.** Si se activa el menú antes de subir la pantalla, quien entre por el
menú recibe un error. Por eso quedó oculta desde el registro.

## Lo que ya está hecho en la base, no repetir

- `Sp_RTA_EliminarPerfil`, `Sp_RTA_ListarPerfilesAdmin` y `Sp_RTAInsertaNuevoPerfil`
  creados y probados contra producción.
- La opción de menú `Id_Menu = 20078`, en el grupo 20042 ("Manejo de Perfiles"), con sus
  tres filas de `PerfilMenu` en estado oculto.

## Verificación posterior al despliegue

1. Entrar con un usuario de perfil 2, 18 o 19 y abrir la pantalla desde el menú.
2. **Que la columna Usuarios traiga números y no ceros en todos los perfiles.** Si todo da
   cero, el conteo está comparando columnas equivocadas.
3. Intentar eliminar un perfil poblado: debe rechazarlo indicando cuántos usuarios tiene.
   Con el perfil "Tecnico" (id 1) el mensaje debería hablar de unos 142 usuarios.
4. Crear un perfil de prueba, verificar que aparece con 0 usuarios, eliminarlo, y
   comprobar que desaparece de la lista.
5. Entrar con un usuario **de otro perfil** y confirmar que no ve la opción en el menú.

## Pendientes conocidos, ninguno bloqueante

- **Sin edición de perfiles**, por lo explicado arriba.
- **6 usuarios apuntan a perfiles que no existen** (`Id_Perfil` con valores -1, 20, 21 y
  41; el catálogo llega hasta 19). Es un problema de datos anterior a este trabajo: esas
  personas no tienen un perfil válido. Vale revisarlo con Talento Humano.
- **El alta no está en una transacción**: inserta y después ajusta `Codigo`. Si la
  conexión se corta entre ambas sentencias queda un perfil con `Codigo = 0`. Ventana de
  milisegundos.
- **No hay restricción `UNIQUE` sobre `NombrePerfil`**: el alta comprueba duplicados, pero
  dos altas simultáneas podrían pasar ambas la comprobación.
