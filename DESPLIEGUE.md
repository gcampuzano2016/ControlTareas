# Despliegue

Guía para publicar ReporteTareas. Lee la sección 1 antes del primer
despliegue posterior a agosto de 2026: cambió dónde viven las contraseñas y un
despliegue a medias tumba el sitio.

---

## 1. Lo que cambió: los secretos ya no están en el código

Antes, las cadenas de conexión estaban escritas dentro de `CapaDato/*.cs`, con
la contraseña de `sa` en claro, y las claves de cifrado en el `Web.config`. Como
el repositorio es público, cualquiera podía leerlas.

Ahora hay **dos** archivos de secretos, con el mismo trato:

| archivo | contenido | ¿va en git? |
|---|---|---|
| `ReporteTareas/Web.config` | solo los `configSource` que apuntan a los otros dos | sí, y no tiene secretos |
| `ReporteTareas/connections.config` | las cadenas reales, con contraseñas | **no**, está en `.gitignore` |
| `ReporteTareas/connections.config.ejemplo` | plantilla con placeholders | sí |
| `ReporteTareas/appsettings.config` | las claves de cifrado reales | **no**, está en `.gitignore` |
| `ReporteTareas/appsettings.config.ejemplo` | plantilla con placeholders | sí |

La consecuencia práctica: **ninguno de los dos viaja en git ni en la
publicación**. Existen porque alguien los creó a mano en esa máquina. En un
servidor nuevo hay que crear los dos.

Si falta alguno, la aplicación **no arranca**: `Global.asax` lo verifica y corta
con el nombre de lo que falta. Eso es a propósito. Antes fallaba mucho más
tarde y mintiendo — un `NullReferenceException` en la capa de datos, o un
"el usuario no existe" en el login.

> **Ojo con `appsettings.config`.** De sus dos valores se deriva la llave AES con
> la que `FileEncryptionService` cifra archivos. Si cambian, **los archivos ya
> cifrados quedan ilegibles**. Al preparar un ambiente nuevo hay que copiar los
> valores exactos del ambiente existente, no inventar unos nuevos.

---

## 2. Preparar una máquina o un servidor por primera vez

```
copy ReporteTareas\connections.config.ejemplo ReporteTareas\connections.config
copy ReporteTareas\appsettings.config.ejemplo ReporteTareas\appsettings.config
```

Luego edita los dos y reemplaza los placeholders por los valores reales del
ambiente: en `connections.config`, `SERVIDOR`, `USUARIO` y `CONTRASENA`; en
`appsettings.config`, `CLAVE_DE_CIFRADO` y `SALT_DE_CIFRADO`. Se hace **una sola
vez**: las publicaciones posteriores no los tocan.

En el servidor van junto al `Web.config` desplegado, no en `bin\`.

Nunca los agregues a git. Si `git status` los muestra, algo se rompió en el
`.gitignore`.

---

## 3. Publicar

El proyecto trae el perfil `FolderProfile`, que publica a una carpeta local:

- **Configuración:** Release
- **Destino:** `C:\respaldodisco\Desarrollo\Publicar`
- **Borra el destino antes de publicar:** sí (`DeleteExistingFiles`)

Desde Visual Studio: clic derecho en el proyecto `ReporteTareas` → *Publicar* →
perfil `FolderProfile`.

> **Cuidado con el borrado.** La publicación vacía la carpeta destino. No
> guardes ahí los archivos de secretos ni nada que quieras conservar: se pierde
> en la siguiente publicación. Los dos van en el servidor, junto al `Web.config`
> desplegado, no en la carpeta de publicación.

### Cómo llegan los archivos al servidor

A mano, por carpeta compartida: se copia el contenido de
`C:\respaldodisco\Desarrollo\Publicar` sobre la carpeta del sitio en el
servidor, reemplazando los archivos existentes.

> **No uses una copia que borre sobrantes.** `robocopy /MIR`, un "sincronizar
> carpetas" o cualquier opción de *eliminar archivos que no están en el
> origen* **borrará `connections.config` y `appsettings.config` del servidor**,
> porque esos archivos no vienen en la publicación. Desde agosto de 2026 el
> sitio no arranca y dice cuál falta, en vez de caerse al primer query sin
> explicar por qué. Copia y reemplaza; nunca sincronices.

Qué debe quedar en el servidor después de copiar:

- todo lo publicado (`bin\`, `Controles\`, `Formulario\`, `js\`, `Web.config`, …)
  — **`Controles\` es una carpeta nueva en la raíz del sitio**: si no viaja,
  «Mi perfil» y «Perfiles del personal» mueren con un error de parser, porque
  las dos registran `~/Controles/PerfilFichas.ascx`
- `connections.config` y `appsettings.config`, **que ya estaban ahí y no se tocan**

No hace falta reiniciar IIS: al reemplazar `Web.config` o el contenido de
`bin\`, ASP.NET recicla la aplicación solo. La primera visita después de
copiar es más lenta de lo normal; eso es esperado.

---

## 4. El orden importa

El `.dll` nuevo y el `Web.config` viejo son incompatibles. Si actualizas uno
sin el otro, el sitio se cae.

| qué despliegas | resultado |
|---|---|
| solo `bin\` sobre un `Web.config` viejo | **falla** — el código busca cadenas que ese `Web.config` no declara |
| un paquete viejo completo | funciona, pero reintroduce las contraseñas en el código |
| publicación nueva + los dos archivos de secretos en el servidor | correcto |

Orden seguro:

1. Verifica que `connections.config` y `appsettings.config` ya existan en el
   servidor (sección 2).
2. Ejecuta los scripts SQL pendientes de `docs/sql/` (ver abajo).
3. Publica y copia los archivos.
4. Recién entonces deja entrar tráfico.

### Los scripts SQL van antes que los binarios

Cuando un cambio trae script en `docs/sql/`, **el script corre primero**. Nunca
después.

La razón es que el `.dll` nuevo espera columnas y procedimientos que el script
crea. Si los binarios llegan antes, el código pide algo que todavía no existe y
**no falla solo el campo nuevo: falla la consulta entera**, así que la pantalla
deja de listar.

| qué haces primero | resultado |
|---|---|
| script SQL, después los binarios | correcto |
| binarios, después el script SQL | **falla** — la pantalla no lista nada hasta que corras el script |

> **Hay una excepción documentada**, y es el script de menú de la entrega 2 del
> Perfil del colaborador (`2026-09-17-perfil-personal-menu.sql`): ése va
> **después** de copiar los archivos, porque es el único que produce algo
> visible y antes de los binarios deja a los perfiles 14 y 18 una opción de menú
> que da 404. El porqué y el orden completo están más abajo, en la sección de
> esa entrega. Si un script futuro solo toca el menú, se le aplica el mismo
> razonamiento.

El orden correcto es seguro incluso si tardas en copiar los binarios: los
procedimientos declaran los parámetros nuevos con valor por defecto, así que el
código viejo los sigue llamando sin enterarse. Por eso hay margen entre un paso
y el otro, pero solo en ese sentido.

**Ese margen es de funcionamiento, no de trazabilidad.** Un parámetro con valor
por defecto hace que el código viejo no falle; no hace que se comporte como el
nuevo. Cuando lo que agrega el script es un registro de auditoría, el código
viejo sigue escribiendo el dato y deja de escribir el rastro, sin error que lo
delate. Es exactamente el caso de `@Auditar` en `Sp_RTA_HeGuardarFila`
(`docs/sql/2026-09-16-horas-extras-fase3.sql`): entre el script y los binarios,
las horas que alguien digite se guardan sin quedar en `HE_DetalleAuditoria`.
Con un cambio así, copia los binarios en la misma parada y no dejes la pantalla
con tráfico en medio. Y si alguna vez revierte los binarios sin revertir el
script, cuenta con lo mismo: vuelve a escribir sin auditar.

**La fase 4 de Horas Extras cambia el contrato de tres procedimientos y trae un
guard cruzado con la fase 3.** El orden es el de siempre: primero
`docs/sql/2026-09-16-horas-extras-fase4.sql`, después los binarios.

El script convierte `HE_Periodo` de `(Anio, Mes)` a `(FechaInicio, FechaFin)`
-las columnas `Anio` y `Mes` se eliminan-, agrega `HorasOrigen` a `HE_Detalle`,
crea `Sp_RTA_HeHorasAprobadas` y modifica `HeCrearPeriodo` (que ahora rechaza
rangos solapados con el código `-5`), `HeCargarPeriodo`, `HeListarPeriodos` y
`HeGuardarFila`. Al abrir un período, las horas aprobadas en
`R_DetTareasAranda` se siembran solas.

`@HorasOrigen` en `Sp_RTA_HeGuardarFila` vale `'Manual'` por omisión, no
`'Tareas'`, y es a propósito: durante la ventana entre el script y los
binarios el DAO viejo sigue llamando al procedimiento sin ese parámetro
mientras alguien de Nómina digita horas. Marcarlas como manuales es el lado
seguro -una fila `'Manual'` no se vuelve a sembrar-, así que en el peor caso se
pierde una siembra, que se puede volver a pedir, y nunca se pisa una
corrección escrita a mano, que no se recupera.

El script de la fase 3 (`docs/sql/2026-09-16-horas-extras-fase3.sql`) ahora
trae su propio guard: si detecta que la fase 4 ya está aplicada -mira si
`HE_Periodo.FechaInicio` existe- se salta entero con `NOEXEC`. **No lo vuelvas
a correr a mano pensando que es inofensivo por ser idempotente en las fases
anteriores**: en esta sí haría daño, porque recrearía `Sp_RTA_HeGuardarFila`
con la firma vieja, de 23 parámetros, y cada guardado posterior fallaría hasta
volver a correr la fase 4.

Esta fase también sube `horasExtras.js` de `?v=4` a `?v=5`. Verifica con
Ctrl+F5 después de copiar los binarios -ver sección 5-, porque un `?v=` viejo
en el navegador sigue mostrando el período por mes en vez de por rango.

**La fase 5 de Horas Extras agrega una pantalla para editar los parámetros de
cálculo, y antes que nada corrige de dónde salen esos parámetros al calcular
un período.** El orden es el de siempre -primero
`docs/sql/2026-09-16-horas-extras-fase5.sql`, después los binarios-, pero acá
el margen entre uno y otro importa más que en fases anteriores.

Hasta esta fase, un período se calculaba con el sueldo vigente a la fecha de
fin del período pero con los parámetros (`Factor50`, `Factor100`,
`TopeDiario50`, etc.) vigentes **hoy** (`GETDATE()`): dos relojes distintos
para el mismo cálculo. Con una sola versión de cada parámetro en
`HE_Parametro` -que es lo único que ha existido hasta ahora- el error nunca se
notó, porque "hoy" y "el único valor que hay" siempre coincidían. La pantalla
nueva permite que una misma clave tenga varias versiones con vigencia
distinta, y ahí el error sí se nota: un cambio de parámetro recalcularía en
silencio períodos ya cerrados que no le corresponden. Por eso el arreglo del
cálculo va incluido en los mismos binarios que la pantalla nueva -no llega
después-: **el script y esos binarios no deben quedar separados mucho
tiempo**. Con el script puesto y los binarios viejos todavía corriendo, un
cambio de parámetro guardado a través de `Sp_RTA_HeParametroGuardar` se
aplicaría a períodos que no le tocan, exactamente el bug que esta fase cierra.

Qué trae el script:

- `Sp_RTA_HeParametrosListar` - el historial completo de las siete claves, la
  vigente y las cerradas.
- `Sp_RTA_HeParametroGuardar` - **no pisa la fila vigente**: la cierra con
  `FechaVigenciaHasta` e inserta una versión nueva. Devuelve `0` en éxito y
  `-1` a `-5` según el motivo del rechazo.
- El registro en el menú de `ParametrizacionHorasExtras.aspx`
  (`Id_Menu 20083`), colgada del mismo grupo Nómina que `HorasExtras.aspx`
  (`Id_MenuPadre 20081`), visible para los perfiles **14** y **18**.

> **Ojo con la ventana entre el script y los binarios.** La entrada de menú
> nace visible en cuanto corre el script, antes de que el `.aspx` esté
> publicado. Quien tenga perfil 14 o 18 y le dé clic a esa opción va a ver un
> 404 hasta que se copien los binarios. Se corrige solo al publicar -no hace
> falta ninguna acción aparte-, pero conviene que quien despliega lo sepa para
> no perder tiempo investigando un 404 esperado.

Esta fase también sube `horasExtras.js` de `?v=5` a `?v=6` y agrega
`parametrosHorasExtras.js?v=1`. Verifica con Ctrl+F5 después de copiar los
binarios -ver sección 5-.

**La entrega 1 del Perfil del colaborador separa de quién es el perfil de quién
lo toca, y trae dos scripts que corren en orden.** Primero
`docs/sql/2026-09-16-perfil-autor-columnas.sql`, después
`docs/sql/2026-09-16-perfil-autor-procedimientos.sql`, y recién después los
binarios. El orden entre los dos scripts no es negociable: el segundo recrea
tres procedimientos que escriben en las columnas que crea el primero, y si el
primero no corrió se detiene solo con un mensaje que lo dice.

El primero agrega `Emp_CargaFamiliar.Usu_Modificacion` y
`Empleados.Usu_ModificacionCod`, las dos `varchar(50)` y nulas. El segundo
recrea con `DROP`+`CREATE` los 14 procedimientos de escritura del módulo, cada
uno con un parámetro nuevo `@Usu_Accion` -el autor del cambio-, opcional a
propósito para que los binarios viejos sigan funcionando entre un paso y el
otro.

> **Antes de correr el segundo script, ejecuta la consulta de permisos que está
> en su bloque `VERIFICACION`.** El `DROP` de cada uno de los 14 se lleva
> puestos los permisos a nivel de objeto -un `GRANT EXECUTE` a un usuario o a un
> rol- y el `CREATE` no los repone; ningún script de `docs/sql/` tiene un
> `GRANT`, así que si se pierden no hay de dónde recuperarlos y el módulo de
> perfil empieza a fallar con *"EXECUTE permission denied"* para todos. Lo
> esperable es que devuelva **cero filas**. Si devuelve filas, anótalas antes de
> correr el script y vuelve a otorgar esos mismos permisos después: mirar
> después no sirve, el `DROP` ya los descartó.

**Estos binarios llevan dentro las fases 2, 3a y 3b del módulo de perfil**, que
estaban esperando desde el 2026-09-15 con su esquema ya aplicado y sin
desplegar. Salen todas juntas en este mismo despliegue, así que la pantalla
*Mi perfil* va a cambiar bastante más de lo que sugiere el nombre de la entrega:
hoja de vida, foto y documentos de respaldo aparecen en la misma parada.

> **El margen entre el script y los binarios funciona en un solo sentido.** SQL
> nuevo con binarios viejos es seguro: `@Usu_Accion` tiene valor por omisión y
> el módulo se comporta igual que antes. **SQL viejo con binarios nuevos deja
> todo el módulo sin guardar**, porque `DaoPerfil` manda `@Usu_Accion` por
> nombre a procedimientos que no lo declaran y el usuario ve en pantalla el
> texto crudo de SQL Server. Es el caso de quien se saltea el script, y también
> el de quien revierte el SQL con los binarios ya puestos: si hay que volver
> atrás, se vuelven atrás los binarios primero.

Ese margen tampoco es de trazabilidad, igual que en Horas Extras: mientras
corran los binarios viejos, `Empleados.Usu_ModificacionCod` y
`Emp_CargaFamiliar.Usu_Modificacion` se llenan con el código del **dueño** del
perfil, indistinguible de un autor real. Los primeros valores de esas dos
columnas no prueban que nadie de Talento Humano haya tocado nada.

Los tres scripts de origen del módulo -`2026-09-14-perfil-colaborador.sql`,
`2026-09-14-perfil-colaborador-fase2.sql` y
`2026-09-15-perfil-colaborador-fase3a.sql`- ahora traen su propio guard, igual
que el de la fase 3 de Horas Extras: si detectan que `@Usu_Accion` ya está
aplicado, se saltan enteros con `NOEXEC`. **No los vuelvas a correr a mano
pensando que son inofensivos por ser idempotentes**: recrearían los 14
procedimientos sin `@Usu_Accion` y, los dos primeros, además devolverían
`Sp_RTA_PerfilColaborador` a una versión más vieja que la vigente.

Los scripts son idempotentes: si dudas si ya corriste uno, córrelo de nuevo. Los
`PRINT` te dicen si creó algo o si ya existía. La excepción son los scripts que
una entrega posterior dejó atrás -la fase 3 de Horas Extras y los tres de origen
del Perfil del colaborador-: esos traen un guard que los detiene solos, y el
mensaje te dice cuál es el script vigente.

**La entrega 2 del Perfil del colaborador agrega la pantalla con la que Talento
Humano consulta y corrige el perfil de cualquier colaborador, y trae dos
scripts independientes entre sí** -no comparten nada-, pero **no van en el
mismo momento**: uno antes de copiar los archivos y el otro después.

1. `docs/sql/2026-09-17-perfil-personal-lista.sql` - **antes** de copiar, como
   siempre. Crea `Sp_RTA_PerfilPersonalLista`, el mismo SELECT de
   `Sp_RTA_PerfilEquipoLista` sin el filtro por jefatura: el personal activo
   entero, para que Talento Humano busque a cualquiera.
2. `docs/sql/2026-09-17-perfil-personal-menu.sql` - **después** de copiar los
   archivos. Registra `PerfilesPersonal.aspx` en el menú, colgada del mismo
   grupo que `RRHHEmpleados.aspx`, visible para los perfiles 14 y 18. Es el
   único script de esta entrega que produce algo visible: corrido antes de los
   binarios, les deja a los perfiles 14 y 18 una opción de menú que da 404
   hasta que termines de copiar.

> **Esta entrega exige que la entrega 1 ya esté desplegada, en la base y en los
> binarios.** La pantalla nueva pide perfiles ajenos -de cualquier colaborador,
> no del usuario que la abre- y quién es el dueño de un perfil frente a quién lo
> edita es exactamente lo que separó la entrega 1 (`@Usu_Accion`,
> `Usu_Modificacion`, `Usu_ModificacionCod`). Sin eso desplegado, el handler
> `AdministrarPerfil.ashx` rechaza cualquier perfil que no sea el propio: la
> pantalla se ve, el menú la lista, la búsqueda hasta puede listar personal,
> pero abrir a cualquiera de ellos vuelve rechazado. No es un 404 ni un error
> de servidor -es más engañoso-: la pantalla se ve pero no sirve.

> **El script de menú enciende también la fila del grupo, no solo la de la
> hoja nueva.** `2026-09-17-perfil-personal-menu.sql` hace
> `UPDATE PerfilMenu SET Estado='0'` para los perfiles 14 y 18 sobre dos filas
> cada uno: la de `PerfilesPersonal.aspx` y la de **su grupo**, el mismo del
> que ya cuelga `RRHHEmpleados.aspx`. Si la fila del grupo estuviera hoy en
> `'1'` para alguno de los dos perfiles, encenderla no solo destapa la pantalla
> nueva: también revela cualquier **hoja hermana** de ese mismo grupo que ya
> tuviera `Estado='0'` para ese perfil pero que el padre en `'1'` mantenía
> tapada. Es improbable -el perfil 14 ya tiene que ver ese grupo, o no le
> renderizaría `RRHHEmpleados.aspx`-, pero `MenuDos` y `PerfilMenu` los
> comparte todo el menú del sistema, no solo este módulo. Por eso hay que
> mirar el menú completo de un usuario de perfil 14 y de uno de perfil 18
> **antes y después** de correr el script, y confirmar que no apareció
> ninguna opción que antes no estaba.

Esta entrega también sube `miPerfil.js` de `?v=7` a `?v=9`: el mismo archivo
ahora sabe de quién es el perfil que muestra (vacío en "Mi perfil", fijado por
el buscador de `PerfilesPersonal.aspx`) y por eso lo comparten las dos
pantallas. Para "Mi perfil" el comportamiento no cambia. Además agrega
`perfilesPersonal.js?v=2`. Verifica con Ctrl+F5 después de copiar los
binarios -ver sección 5-.

---

## 5. Verificar después de desplegar

1. Abre el sitio e inicia sesión. El login va por Active Directory, pero la
   pantalla siguiente ya consulta la base: si las cadenas están mal, falla ahí.
2. Abre una pantalla que liste tareas. Es la que usa la conexión `ReporTarea`,
   la de casi todo el sistema.
3. Si aparece un error de referencia nula al conectar, casi siempre es que
   falta uno de los archivos de secretos o que un nombre no coincide. Desde
   agosto de 2026 `Global.asax` corta al arrancar diciendo cuál falta. Los
   nombres que el código espera son exactamente `ReporTarea`, `ArandaDb` y `Sap`.
   código espera son exactamente `ReporTarea`, `ArandaDb` y `Sap`.
4. Si el cambio traía script SQL, comprueba la pantalla que lo usa. Para el de
   agosto de 2026 (teléfonos de emergencia): abre *Administración de usuarios*,
   confirma que el combo **Departamento** trae opciones, y guarda un usuario sin
   tocarle nada. Debe responder *"No hubo cambios que guardar"*. Ese único paso
   prueba tres cosas de golpe: que el JS nuevo llegó al navegador, que el
   procedimiento nuevo está en la base, y que la comparación de cambios no le
   altera el dato a nadie por el solo hecho de abrirle la ficha.
5. Si una pantalla se comporta como la versión anterior, es caché del navegador:
   haz Ctrl+F5. Si ahí funciona, al `.js` le faltó subirle el `?v=` y hay que
   corregirlo antes de que lo sufra el resto de usuarios.

---

## 6. Rotar contraseñas

Las claves que estuvieron en el repositorio público siguen siendo recuperables
del historial de git. Hay que rotarlas; mientras sigan siendo válidas, la
exposición continúa.

Al rotarlas ya no se recompila nada: editas `connections.config` en cada
servidor y reinicias el sitio. Eso es todo.

---

## 7. Qué NO subir nunca

- `connections.config` (contraseñas de base) y `appsettings.config` (claves de cifrado)
- Cadenas de conexión escritas dentro de archivos `.cs`, ni siquiera comentadas
  — así fue como se filtraron las anteriores

El `.gitignore` cubre el primer caso. El segundo depende de quien revisa.

---

## 8. Datos personales editables por Talento Humano

Permite que los perfiles 14 y 18 editen los ocho campos de la pestaña «Datos
personales» de cualquier colaborador, desde `PerfilesPersonal.aspx`.

**Depende de la entrega 1** (`2026-09-16-perfil-autor-*.sql`). Si
`Empleados.Usu_ModificacionCod` no existe, el script de procedimientos se detiene
solo con un mensaje que lo dice.

### 8.1 Base de datos

**Ya aplicado en producción el 2026-09-18.** Queda escrito para reponer el
entorno desde cero o para replicarlo en otro. El orden importa:

1. `docs/sql/2026-09-17-datos-personales-columnas.sql`
2. `docs/sql/2026-09-17-datos-personales-procedimientos.sql`

El segundo vuelve a crear **`Sp_RTA_PerfilColaborador`**, que es el que lee el
perfil en **las dos** pantallas. Si algo sale mal ahí, «Mi perfil» también se ve
afectado. Los dos scripts son idempotentes y se pueden volver a correr.

Comprobación después de correrlos:

```sql
SELECT nombre = name, creado = CONVERT(VARCHAR(20), modify_date, 120)
  FROM sys.procedures
 WHERE name IN ('Sp_RTA_PerfilGuardarDatosPersonales',
                'Sp_RTA_PerfilJefesLista',
                'Sp_RTA_PerfilColaborador');
```

### 8.2 Archivos a publicar

Del paquete `ReporteTareas\obj\Release\Package\PackageTmp`:

| Archivo | Por qué |
|---|---|
| `Controles\PerfilFichas.ascx` | El formulario de edición |
| `Formulario\PerfilesPersonal.aspx` | Enciende el formulario (`PERFIL_DATOS_EDITABLES`) |
| `Formulario\MiPerfil.aspx` | **Sólo le cambia el `?v=`**, pero sin eso esa pantalla sigue sirviendo el JavaScript viejo desde la caché |
| `js\miPerfil.js` | Pintar, llenar el combo de jefes y guardar |
| `bin\ReporteTareas.dll` | El handler, con las dos acciones nuevas |
| `bin\CapaEntidad.exe` | `EntPerfilDatosPersonales`, `EntPerfilJefe`, `CodJefeInmediato` |
| `bin\CapaNegocio.exe` | `NegPerfilCedula` y las dos funciones de `NegPerfilCampos` |
| `bin\CapaDato.exe` | `GuardarDatosPersonales` y `ListarJefes` |

> **Son `.exe`, no `.dll`.** Las tres bibliotecas de capa de este proyecto se
> compilan con `OutputType=Exe` y se publican como `CapaEntidad.exe`,
> `CapaNegocio.exe` y `CapaDato.exe`. Buscar `CapaNegocio.dll` no encuentra nada y
> lleva a publicar de menos.

**Copiar archivo por archivo. Nunca `robocopy /MIR` ni ninguna copia que
sincronice:** borra `connections.config` y `appsettings.config`, y el sitio no
levanta.

### 8.3 Antes de copiar: regenerar el paquete

El repositorio versiona `obj/Release/Package/PackageTmp` a propósito, y **es fácil
publicar desde un paquete viejo sin que nada dé error**. Se comprueba en un
segundo:

```bash
grep -h "miPerfil.js?v=" ReporteTareas/Formulario/MiPerfil.aspx \
     ReporteTareas/obj/Release/Package/PackageTmp/Formulario/MiPerfil.aspx
```

Los dos números tienen que coincidir. Si no, regenerar:

```
MSBuild.exe ReporteTareas\ReporteTareas.csproj /p:DeployOnBuild=true ^
            /p:PublishProfile=FolderProfile /p:Configuration=Release
```

### 8.4 Avisarle a Talento Humano antes

Cuando se guarda el perfil de alguien **sin ficha**, el procedimiento se la crea
(o adopta una huérfana que tenga su misma cédula). Esa persona **empieza a
aparecer en `RRHHEmpleados.aspx`**, que hoy no la lista. Son hasta 116 personas.

Si nadie avisa, Talento Humano va a ver crecer esa lista sin saber por qué.

---

## 9. Dashboard de aprobación para jefatura

Agrega una tercera vista a `AprobacionTareasJefatura.aspx` (junto a PENDIENTE y
APROBADAS): seis tarjetas y cuatro gráficos con Chart.js, para el mismo rango
de fechas que ya usa la pantalla.

### 9.1 Base de datos

**Ya aplicado en producción el 2026-09-21**, en su versión corregida (la que
devuelve `PersonasDiaTotal` y `DiasPromedio` con decimal). Queda escrito para
reponer el entorno desde cero o para replicarlo en otro.

> **Los binarios de esta sección necesitan esta versión del procedimiento.** Si
> alguna vez se repone la base con la versión anterior, el dashboard falla al
> leer esas dos columnas. El script va antes que los binarios, como en la
> sección 4.

`docs/sql/2026-09-21-dashboard-aprobacion.sql` — crea
`Sp_RTA_DashboardAprobacionJefatura`. Es idempotente (`DROP`+`CREATE`) y **no
toca ninguna tabla: sólo lee**, agregando en un `#Base` temporal el mismo
parseo de `Det_Tiempo` que ya usa `Sp_RTAListaHorasRecursosPorJefatura` para la
tabla de APROBADAS. Por eso el dashboard y la tabla tienen que coincidir en
horas para el mismo rango: comparten la misma cuenta.

### 9.2 Archivos a publicar

- `Formulario\AprobacionTareasJefatura.aspx`
- `js\aprobacionTareasJefatura.js`
- `js\dashboardAprobacion.js` **(nuevo)**
- `js\chart.umd.js` **(nuevo, 208 KB)**
- `bin\ReporteTareas.dll`
- `bin\CapaEntidad.exe`
- `bin\CapaNegocio.exe`
- `bin\CapaDato.exe`

> **Los tres de capa son `.exe`, no `.dll`.** Igual que en la entrega de la
> sección 8: `CapaEntidad.exe`, `CapaNegocio.exe` y `CapaDato.exe` se compilan
> con `OutputType=Exe`. Buscar los `.dll` de esos tres no encuentra nada y lleva
> a publicar de menos.

**Copiar archivo por archivo. Nunca `robocopy /MIR` ni ninguna copia que
sincronice:** borra `connections.config` y `appsettings.config`, y el sitio no
levanta.

Antes de copiar, regenerar el paquete y comprobar que los dos `.js` nuevos
viajaron (es fácil publicar desde un paquete viejo sin que nada dé error):

```
MSBuild.exe ReporteTareas\ReporteTareas.csproj /p:DeployOnBuild=true ^
            /p:PublishProfile=FolderProfile /p:Configuration=Release

dir ReporteTareas\obj\Release\Package\PackageTmp\js\chart.umd.js
dir ReporteTareas\obj\Release\Package\PackageTmp\js\dashboardAprobacion.js
```

Si alguno de los dos no aparece en `PackageTmp\js\`, no siguió el `.csproj` y
el dashboard va a salir en blanco sin ningún error de compilación que lo avise.

### 9.3 Si el dashboard sale en blanco

Lo primero a mirar es si `js/chart.umd.js` llegó al servidor. La pantalla lo
avisa con un mensaje, pero si no aparece ni el mensaje, es que tampoco llegó
`dashboardAprobacion.js`. En ambos casos la causa más probable es que el
paquete que se copió al servidor era uno viejo, regenerado antes de que estos
dos archivos existieran.

## 10. Horas por persona y cliente en el dashboard

### 10.1 Base de datos

`docs/sql/2026-09-23-dashboard-horas-por-cliente.sql` — recrea
`Sp_RTA_DashboardAprobacionJefatura`. Idempotente, y no toca ninguna tabla: solo lee.
Va **antes** de los binarios: el DAO nuevo lee un conjunto que el procedimiento viejo no
devuelve.

### 10.2 Archivos a publicar

- `bin\ReporteTareas.dll`, `bin\CapaEntidad.exe`, `bin\CapaNegocio.exe`, `bin\CapaDato.exe`
- `Formulario\AprobacionTareasJefatura.aspx` (sube a `dashboardAprobacion.js?v=5`)
- `js\dashboardAprobacion.js`

Los tres de capa son `.exe`, no `.dll`.

**Los binarios van primero.** Con la pantalla y el `.js` nuevos pero los binarios
viejos, el JSON que arma el handler no trae `TablaClientes` — el DAO viejo no lo
llena — y llega `undefined` al navegador. La función que dibuja la tabla cae en su
rama de vacío y muestra «Sin horas aprobadas en el rango» sobre un rango que sí las
tiene: un mensaje tranquilizador y falso. El orden inverso es inocuo: binarios
nuevos con pantalla vieja simplemente no dibujan la tabla nueva, sin mentir sobre
los datos.

### 10.3 Avisar antes de publicar

**El gráfico "Horas por empresa" va a mostrar números más bajos.** Hasta esta entrega
sumaba todos los estados; ahora muestra solo lo aprobado, como las tarjetas de arriba. No
es una pérdida de datos, es la corrección — pero la jefatura ya está mirando ese gráfico.

## 11. Calificación de oportunidades del Forecast

### 11.1 Base de datos

`docs/sql/2026-09-28-calificacion-forecast.sql`: crea la tabla `ForeCastCalificacion`, los
procedimientos `Sp_RTAListaCalificacionForeCast` y `Sp_RTAInsertaCalificacionForeCast`, y la
opción de menú bajo *Forecacst*, para los mismos perfiles que ven *Registro ForeCast*.
Idempotente. **No toca `ForeCast`.** Va **antes** de los binarios.

Verificar con `docs/sql/2026-09-28-calificacion-forecast-verificacion.sql`, que no deja datos.

> **Si se cambia `Sp_RTAListaForeCast`:** `Sp_RTAListaCalificacionForeCast` replica sus
> 40 columnas en una tabla temporal (`INSERT … EXEC`). Si se le agrega una columna, hay que
> agregarla también en `#Visibles`, o la pantalla de calificación deja de cargar.

### 11.2 Archivos a publicar

- `bin\ReporteTareas.dll`, `bin\CapaEntidad.exe`, `bin\CapaNegocio.exe`, `bin\CapaDato.exe`
- `Formulario\CalificacionForeCast.aspx`
- `Formulario\AdministrarCalificacionForeCast.ashx`
- `Formulario\DescargarCalificacionForeCast.ashx`
- `js\CalificacionForeCast.js`

Los tres de capa son `.exe`, no `.dll`.

### 11.3 Verificar después de publicar

1. Con un usuario que tenga filtro de sucursal, el número de «Abiertas» coincide con lo que ve en
   *Registro ForeCast* filtrado a las etapas LEAD, PIPELINE, UPSIDE y FORECAST (esa pantalla lista
   todas las etapas por defecto), o con el número esperado que imprime la sección 3 de
   `docs/sql/2026-09-28-calificacion-forecast-verificacion.sql`.
2. Calificar una oportunidad y comprobar que la fila cambia de puntaje.
3. Descargar el Excel.

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

### 12.4 Volver atrás

Basta con reponer los binarios y los dos archivos de la pantalla que había antes. El
procedimiento `Sp_RTA_HeFueraDeHorario` puede quedar en la base: es de solo lectura y los
binarios anteriores no lo llaman.

## 13. Horas Extras: personas fuera del módulo, completar datos y ajustes de sueldo

Tres cambios en la pantalla de Horas Extras:

- **Recuadro amarillo:** muestra a los empleados activos que tienen horas en el período pero **no
  están en el módulo**. Van sin montos, porque no tienen sueldo cargado. También salen en el
  aviso de cierre y en la hoja nueva del Excel, «No están en el módulo».
- **«Completar datos»:** los perfiles 14 y 18 pueden agregar a esa persona al módulo, con
  empresa, cargo, jornada y sueldo.
- **Ajuste de sueldo:** el ícono junto a cada nombre registra un ajuste. Agrega una fila al
  historial y **nunca** borra ni modifica un sueldo.

### 13.1 Base de datos

`docs/sql/2026-09-28-horas-extras-fuera-del-modulo.sql` crea tres procedimientos:

- `Sp_RTA_HeFueraDelModulo`: solo lectura.
- `Sp_RTA_HeAgregarColaborador`: inserta, o reactiva, la persona en el maestro y le carga un
  sueldo 'Rol'.
- `Sp_RTA_HeAgregarAjusteSalario`: inserta un sueldo 'Ajuste'.

El script va **antes** de los binarios. Su verificación no escribe nada: solo prueba los códigos
de rechazo. Imprime `OK` o `VEREDICTO: FALLO`.

### 13.2 Archivos a publicar

Del paquete `ReporteTareas\obj\Release\Package\PackageTmp`:

- `bin\ReporteTareas.dll`, `bin\CapaEntidad.exe`, `bin\CapaNegocio.exe` y `bin\CapaDato.exe`,
  **los cuatro juntos**;
- `Formulario\HorasExtras.aspx`, que pasa a `horasExtras.js?v=9`;
- `js\horasExtras.js`.

### 13.3 Verificar después de publicar

1. En el período 16-08 / 15-09-2026 aparece el recuadro con 2 personas: una con 11,00 h
   aprobadas al 100% y otra con 2,98 h sin aprobar.
2. Con un perfil 14 o 18, el recuadro muestra «Completar datos» y la grilla muestra el ícono de
   ajuste. Con el perfil 2, no aparece ninguno de los dos.
3. El Excel trae cuatro hojas.

### 13.4 Cargar los datos que faltan

Los datos los confirma **RRHH**. Se cargan desde la pantalla, no con un script, así cada fila
queda con el usuario que la cargó. Si el período está abierto, al guardar se recalcula solo.

### 13.5 No volver a correr la carga de la plantilla

`docs/sql/carga-generada/carga-horas-extras.sql` hace MERGE sobre `HE_ColaboradorParametro` con
los datos de la plantilla. Si se vuelve a correr, puede **pisar la jornada y la empresa**
cargadas desde la pantalla y **desactivar de nuevo** a quien se reactivó, porque gana el
`Estado` de la plantilla. Después de usar «Completar datos», esa carga ya no se corre. Si hace
falta volver a cargar la plantilla, antes hay que incluir en ella a las personas agregadas desde
la pantalla.

### 13.6 Volver atrás

Basta con reponer los binarios y los dos archivos de la pantalla anteriores. Los procedimientos
pueden quedar en la base. Lo que ya se haya guardado desde la pantalla son filas normales del
maestro y del historial de sueldos, y siguen valiendo con los binarios anteriores.

## 14. Horas Extras: decisión por tramo y pago por minutos

Nómina deja de depender de una regla automática para decidir qué tiempo fuera de horario se
paga: ahora **decide tramo por tramo**, desde la pantalla, y esa decisión queda registrada con
usuario y fecha. El pago pasa de calcularse con horas a **minutos exactos**, redondeando el
dinero una sola vez por persona.

### 14.1 Base de datos

`docs/sql/2026-09-29-horas-extras-decision-por-tramo.sql` crea:

- `HE_TramoDecision`: tabla nueva, **solo se inserta** en ella. Cambiar una decisión es insertar
  otra fila; nunca se actualiza ni se borra, así que la tabla es su propio historial.
- `Sp_RTA_HeTramosCandidatos`: solo lectura. Devuelve los tramos que Nómina puede incluir en un
  período, las decisiones vigentes sobre ellos y los períodos.
- `Sp_RTA_HeGuardarDecisiones`: inserta un lote de decisiones, todo o nada, con protección contra
  el pago doble entre períodos.

**Ningún procedimiento existente se recrea.** En particular, `Sp_RTA_HeHorasAprobadas` (la
siembra) no se toca.

El script va **antes** que los binarios, como siempre. Su verificación no escribe nada: prueba
los códigos de rechazo con lotes que se rechazan antes de insertar, y cuenta las filas antes y
después. Imprime `OK` o `VEREDICTO: FALLO`.

### 14.2 Archivos a publicar

Del paquete `ReporteTareas\obj\Release\Package\PackageTmp`:

- `bin\ReporteTareas.dll`, `bin\CapaEntidad.exe`, `bin\CapaNegocio.exe` y `bin\CapaDato.exe`,
  **los cuatro juntos**;
- `Formulario\HorasExtras.aspx`, que pasa a `horasExtras.js?v=11`;
- `js\horasExtras.js`.

**Copiar archivo por archivo. Nunca `robocopy /MIR` ni ninguna copia que sincronice:** borra
`connections.config` y `appsettings.config`, y el sitio no levanta.

### 14.3 ANTES de publicar

1. **Contar las filas `HorasOrigen = 'Manual'` en los períodos abiertos:**

   ```sql
   SELECT p.IdPeriodo, COUNT(*) AS Filas
     FROM dbo.HE_Detalle d
     JOIN dbo.HE_Periodo p ON p.IdPeriodo = d.IdPeriodo
    WHERE p.EstadoPeriodo = 'Abierto'
      AND d.HorasOrigen = 'Manual'
    GROUP BY p.IdPeriodo;
   ```

   Con los binarios de hoy, **cada guardado desde la pantalla marca la fila como `'Manual'`**
   (es el lado seguro: una fila `'Manual'` no se vuelve a sembrar sola). Si la consulta devuelve
   filas, avisar: esas filas **no toman las nuevas decisiones por tramo hasta que alguien guarde
   de nuevo** esa observación después de publicar, porque hasta entonces siguen con el pago que
   ya tenían.

   **Qué hacer si el conteo es mayor que 0:** después de publicar, abrir cada una de esas filas
   en la pantalla y **guardar su observación** (puede ser la misma que ya tiene). Al guardar, la
   fila deja de ser `'Manual'` y se recalcula desde las decisiones. Hasta entonces se ven como
   **«Desactualizada»**, y «Traer aprobaciones nuevas» no las arregla (no reescribe filas
   `'Manual'`); mientras sigan desactualizadas, el período **no se deja cerrar** (ver 14.4).
   Al 29-09-2026 el conteo era 0 en los dos períodos abiertos.

2. **Corregir en la pantalla los sueldos cruzados de las dos personas que se cargaron el 28-09
   desde «Completar datos»**, con la opción **«Registrar ajuste de sueldo»** (el ícono junto al
   nombre), usando la misma fecha de vigencia con la que se cargaron. Sin este paso, el período
   16-08/15-09 sigue pagando a esas dos personas con el sueldo de la otra.

3. **Avisar a Nómina**, antes de que alguien abra el período:
   - el pago pasa a calcularse con **minutos exactos** y **un solo redondeo por persona** (en
     vez de horas a dos decimales redondeadas por tramo);
   - eso **cambia centavos en muchas filas la primera vez que se recalcula** un período tras
     publicar — no es un error, es el cálculo más preciso;
   - la **columna 100%** de la grilla y del Excel sale **por diferencia** (`Total − 50%`), así
     que puede diferir en **1 centavo** de su valor exacto en algunas filas; el **total por
     persona** siempre es el exacto;
   - además de los centavos, el pago por minutos **une los tramos aprobados que se pisan**: un
     minuto que está en dos tramos aprobados de la misma persona se paga una sola vez (antes se
     pagaba dos veces). Al 29-09-2026 esto afecta a **1 par de tramos, 6 minutos**, en los
     períodos abiertos;
   - **antes de cerrar un período, pulse «Traer aprobaciones nuevas»; el sistema no deja cerrar
     con filas desactualizadas** (ni cuando no pudo verificarlo: en ese caso, intentar de nuevo).

### 14.4 Verificar después de publicar

1. Abrir el período 16-08 / 15-09-2026, abrir el detalle de una persona con tramos fuera de
   horario: se ven los tramos con su valor en USD.
2. Cambiar una casilla (incluir/excluir, tipo o minutos) y guardar: pide confirmación en dos
   clics («¿Es la persona correcta?» → «Sí, guardar»). Al guardar, la grilla se recalcula sola.
3. El Excel trae **6 hojas**, incluidas «Conciliación» y «Decisiones».
4. Cuando corresponde, aparece el aviso de filas desactualizadas y nombra el botón real,
   **«Traer aprobaciones nuevas»** (nunca «Recalcular»). Con el período **cerrado**, la pantalla
   y la hoja «Conciliación» del Excel dicen en cambio **«reabra el período para actualizarlo»**
   (el botón no aparece en un período cerrado).
5. **Antes de cerrar un período, pulse «Traer aprobaciones nuevas»; el sistema no deja cerrar
   con filas desactualizadas.** Si alguna quedó desactualizada, «Cerrar período» responde «Hay N
   fila(s) desactualizada(s) respecto de las decisiones: pulse «Traer aprobaciones nuevas» antes
   de cerrar.» y el período sigue abierto. Si la verificación misma falla, responde «No se pudo
   verificar que el período esté al día; no se cerró. Intente de nuevo.»

### 14.5 Cargar las decisiones del análisis manual

No hay script de carga: las decisiones de Nómina para el período 16-08/15-09 se registran **desde
la pantalla**, tramo por tramo, igual que cualquier otra decisión. La validación en memoria
(fuera del repositorio, en `Actualizacion/`) confirmó que aplicando esas decisiones sobre los
datos reales, sin escribir en la base, el total cuadra **39/39 personas** y da **6.241,14** en
total, igual que el análisis manual.

### 14.6 Volver atrás

Basta con reponer los binarios y los dos archivos anteriores de la pantalla (`HorasExtras.aspx`
en su `?v=` previo y `horasExtras.js`). La tabla `HE_TramoDecision` y los dos procedimientos
nuevos pueden quedar en la base, sin uso. `Sp_RTA_HeHorasAprobadas` no se tocó, así que la siembra
vieja vuelve a funcionar exactamente como antes. Las decisiones que ya se hayan guardado desde la
pantalla nueva quedan en `HE_TramoDecision`, pero sin ningún efecto mientras corran los binarios
anteriores.

**Efecto en el dinero — avisar a Nómina si se vuelve atrás:**

- con los binarios anteriores, **recalcular un período ABIERTO** («Traer aprobaciones nuevas» o
  editar fechas) **vuelve a pagar solo las horas aprobadas**: las exclusiones y las inclusiones
  del análisis manual (lo decidido tramo por tramo) **dejan de aplicarse** en ese período. Los
  períodos cerrados no cambian;
- **cada guardado de una fila con los binarios anteriores la marca `'Manual'`**. Si después se
  vuelve a publicar la versión nueva, esas filas **ignoran las decisiones** hasta que alguien
  **guarde de nuevo su observación** (ver 14.3, paso 1): antes de re-publicar, contarlas con la
  misma consulta.

### 14.7 Limitaciones conocidas

- **Un tramo que cambia de aprobación después de cerrado su período.** El pago doble se decide
  por tramo: a lo sumo un período lo retiene. Un período dueño que cerró pagando un tramo
  aprobado **sin decisión explícita** (por omisión) lo retiene solo mientras el tramo siga
  aprobado. Si después la jefatura cambia la aprobación (por ejemplo, lo pasa a rechazado), el
  sistema ya no puede saber que ese período lo pagó —no guarda una foto del pago por tramo— y
  otro período podría incluirlo. **Remedio:** reabrir el período cerrado y recalcularlo, o no
  incluir ese tramo en otro período. Un período cerrado que lo incluyó **por decisión** sí lo
  sigue reteniendo aunque el tramo cambie.

## 15. Curva de Aprendizaje (módulo nuevo de Talento Humano)

Seguimiento de la curva de aprendizaje del primer año de cada colaborador nuevo: cinco etapas
(30/60/90/180/365), autoevaluación y evaluación del jefe, compromisos, semáforo, plan de acción,
dashboard, tres informes PDF y una evaluación remota por enlace. Reemplaza la aplicación aparte
(Node + SQLite). Especificación completa: `ESPECIFICACION-MODULO-WEBFORMS.md`.

### 15.1 Base de datos (en este orden)

1. `docs/sql/2026-10-05-curva-aprendizaje-esquema.sql`: las 9 tablas `CA_*` y los 28
   procedimientos `Sp_RTA_Curva*`. **Ya aplicado en `ReporTarea` el 2026-10-05.** Re-correrlo es
   inofensivo (las tablas solo se crean si no existen).
2. El catálogo, **uno de los dos**:
   - **Con migración (lo previsto):** `python docs/sql/generar-carga-curva-aprendizaje.py <copia del .sqlite>`
     y correr el `docs/sql/carga-generada/carga-curva-aprendizaje.sql` que genera. Trae el
     catálogo **con sus IDs originales** y reemplaza la siembra. El `.sqlite` está en
     `%LOCALAPPDATA%\DOS-CurvaAprendizaje\data\curva_aprendizaje.sqlite` de la PC donde corría
     la aplicación vieja: **usar una copia** y no versionar ni el `.sqlite` ni la salida (son
     datos de RRHH; la salida ya está en `.gitignore`).
     **Hecha el 2026-10-05** desde el export `curva_aprendizaje_sqlserver.sql` (5 colaboradores,
     21 evaluaciones, 153 respuestas): el generador también lee ese formato. **Ese export NO se
     ejecuta tal cual en ReporTarea**: crea `dbo.empleados` y la base no distingue mayúsculas, así
     que es la misma tabla que `dbo.Empleados`, la ficha de personal. Los 15 índices de la curva
     coinciden con los del sistema original (prueba 19).
   - **Sin migración:** `docs/sql/2026-10-05-curva-aprendizaje-siembra.sql` (5 etapas, 90
     preguntas). **Ya aplicado el 2026-10-05**; la carga la reemplaza sola mientras no haya
     evaluaciones.
3. `docs/sql/2026-10-05-curva-aprendizaje-menu.sql`: tres opciones bajo el grupo de
   `RRHHEmpleados.aspx`, para los perfiles 14 y 18. **Aplicado el 2026-10-05** (Id_Menu 20086,
   20087 y 20088, grupo 20060), antes que los binarios: hasta publicarlos, esas opciones dan
   «página no encontrada».

Cada script termina con `OK` o `VEREDICTO: FALLO`.

### 15.2 Archivos a publicar

Del paquete `ReporteTareas\obj\Release\Package\PackageTmp`:

- `bin\ReporteTareas.dll`, `bin\CapaEntidad.exe`, `bin\CapaNegocio.exe`, `bin\CapaDato.exe`
  (las capas son `.exe` y van **juntas** con el `.dll`). `bin\PdfSharp.dll` y
  `bin\Newtonsoft.Json.dll` ya están en el servidor.
- `Global.asax` (fuerza TLS 1.2 para la IA; no cambia nada más).
- `Formulario\CurvaColaboradores.aspx`, `CurvaColaboradorNuevo.aspx`, `CurvaColaboradorFicha.aspx`,
  `CurvaEvaluacion.aspx`, `CurvaDashboard.aspx`, `CurvaEvaluacionRemota.aspx`
- `Formulario\AdministrarCurvaAprendizaje.ashx`, `CurvaEvaluacionRemota.ashx`, `DescargarCurvaAprendizaje.ashx`
- `js\curvaComun.js`, `curvaFormulario.js`, `curvaColaboradores.js`, `curvaNuevo.js`,
  `curvaFicha.js`, `curvaEvaluacion.js`, `curvaDashboard.js`, `curvaRemota.js`
- `css\curvaAprendizaje.css`

> **Estos binarios llevan todo lo que haya commiteado en la rama**, no solo este módulo.

### 15.3 Configuración (opcional)

En `appsettings.config` del servidor (ver `appsettings.config.ejemplo`):

- `CA.UrlBasePublica`: base del enlace remoto, por ejemplo `https://servidor/ReporteTareas`.
  Sin ella, el enlace usa el host con el que GTH abrió la ficha; si ese host no es el que los
  evaluadores pueden abrir, **configurarla**.
- `CA.IA.Modelo`, `CA.IA.Effort`, `CA.IA.TimeoutSegundos`: los valores por omisión sirven.

**La IA queda apagada a propósito.** Sin la variable de entorno `ANTHROPIC_API_KEY` en el pool de
IIS, los resúmenes ejecutivos guardan un aviso y todo lo demás funciona. Activarla exige una
cuenta corporativa de la API aprobada por TI/Legal: los resúmenes envían respuestas de
evaluaciones de RRHH a un servicio externo.

### 15.4 Verificar después de publicar

1. Con perfil 14 o 18, el menú muestra «Curva de aprendizaje», «Curva: nuevo colaborador» y
   «Curva: dashboard». Con otro perfil no aparecen, y la URL directa redirige a Principal.
2. Registrar un colaborador de prueba: la ficha muestra dos tarjetas de 30 días.
3. «Generar enlace remoto» en una tarjeta y abrir el enlace **en una ventana privada** (sin
   sesión): se ve el formulario sin menú. Enviarlo y reabrir el enlace: «¡Gracias!».
4. «Ver informe (PDF)» de la evaluación completada abre el PDF con tildes bien.
5. Eliminar el colaborador de prueba (dos confirmaciones).

### 15.5 Volver atrás

Correr el script de menú al revés (poner `Estado = '1'` en `PerfilMenu` para las tres hojas) y
reponer los binarios anteriores. Las tablas y procedimientos pueden quedar: los binarios
anteriores no los llaman.

### 15.6 Decisiones de negocio pendientes

- **La pregunta NPS (365, colaborador, 0 a 10) baja el índice cuando la respuesta es buena**: su
  primera opción es la peor. Se dejó igual que el sistema original para que la curva de los
  colaboradores migrados coincida. Hay que decidir si se excluye del índice o se reordena (y en
  ese caso migrar sus respuestas).
- Cambiar una decisión de `aprobado` a `rechazado` detiene el proceso pero **no borra** las
  evaluaciones de etapas siguientes ya generadas (igual que el sistema original).

## 16. Tablero de Costos (Nómina, perfiles 14 y 18)

Lleva a la aplicación el `Dashboard_Tareas_Aranda.xlsx` que se generaba a mano con
`C:\ReporteExcel\generar_dashboard.py`. Spec: `docs/superpowers/specs/2026-10-12-tablero-costos-design.md`.

### 16.1 Base de datos

`docs/sql/2026-10-12-tablero-costos.sql` crea:
- el procedimiento de lectura `Sp_RTA_TableroCostos`;
- la tabla `TC_Auditoria` y el procedimiento `Sp_RTA_TableroCostosAuditar`;
- la hoja de menú *Nómina → Tablero de Costos*, visible **solo** para los perfiles 14 y 18.

Es repetible y **no escribe en `HE_*` ni en `R_DetTareasAranda`**. Va **antes** de los binarios. La última línea debe decir `VEREDICTO: OK`.

Para verificar, correr `docs/sql/2026-10-12-tablero-costos-verificacion.sql`. Es de solo lectura; la prueba de auditoría se revierte.

> **Si se agrega un perfil al tablero:** ya no se toca el código. Basta con marcar la opción en `PerfilMenu`
> (pantalla *Menú por perfil*): el acceso lo decide el menú (ver §17).
>
> **Para dárselo a una sola persona:** ver 16.5.

### 16.2 Archivos a publicar

Todos juntos. Las capas son `.exe` y van con el `.dll` de la web; publicarlos por separado ya rompió los correos el 18-09.

- `bin\ReporteTareas.dll`, `bin\CapaEntidad.exe`, `bin\CapaNegocio.exe`, `bin\CapaDato.exe`
- `Formulario\TableroCostos.aspx`
- `Formulario\AdministrarTableroCostos.ashx`
- `Formulario\DescargarTableroCostos.ashx`
- `js\tableroCostos.js`
- `js\chart.umd.js`, si el servidor todavía no lo tiene. Lo usa también el dashboard de jefatura.

### 16.3 Verificar después de publicar

1. Con un usuario de perfil 14 o 18, abrir el tablero con Desde `2025-01-01` y Hasta `2026-12-31`: el total de actividades coincide con el paso 2 de la verificación SQL.
2. Descargar el Excel y comprobar que hay una fila nueva en `TC_Auditoria`.
3. Con un usuario de otro perfil, la opción no aparece y la URL directa redirige a Principal.
4. Si alguien no ve cambios del `.js` después de un redespliegue, subir el `?v=` en `TableroCostos.aspx`.
5. La opción aparece en Nómina para los perfiles 14 y 18.

### 16.4 Reversa

1. Ocultar la hoja del menú (identificarla por su `Href`, no por el Id):
   `UPDATE PerfilMenu SET Estado = '1' WHERE id_Menu IN (SELECT Id_Menu FROM MenuDos WHERE Href = 'TableroCostos.aspx')`
   (afecta a los perfiles 14 y 18).
2. Reponer los binarios anteriores **todos juntos** (`ReporteTareas.dll` y los tres `.exe` de capa).
3. La tabla `TC_Auditoria` y los procedimientos pueden quedar: ningún binario anterior los llama.

### 16.5 Acceso para una persona puntual (módulo extra)

Además de los perfiles 14 y 18, entra quien tenga **«Tablero de Costos»** marcado como módulo extra en
*Menú por usuario* (`ParametrizacionMenuUsuario.aspx`, tabla `R_UsuarioMenu`). La misma marca le dibuja la
opción en el menú y le da el permiso en el código: no hay que escribir su código de usuario en ningún lado.

> **Reemplazado por la §17.** `Sp_RTA_TableroCostosAccesoUsuario` (y su script `2026-10-14-tablero-costos-acceso-por-usuario.sql`)
> ya no se usa: el acceso pasa por la regla general de permisos. Ya no hace falta correr ese script.

El acceso sigue dándose igual: en *Menú por usuario*, buscar a la persona, marcar *Nómina → Tablero de Costos* y guardar.
**La pantalla guarda la lista completa de extras de esa persona**: no desmarcar lo que ya tenía.
Para quitarlo, desmarcar la opción y guardar. Tarda hasta 2 minutos en aplicarse (ver §17).
Dar un **perfil** en `PerfilMenu` también abre el tablero a ese perfil, sin cambios de código.

## 17. Permisos desde el portal

Spec: `docs/superpowers/specs/2026-10-14-permisos-desde-el-portal-design.md`.

### 17.1 Qué es

El menú decide el acceso a **9** pantallas protegidas: Tablero de Costos, Horas Extras, Parámetros de Horas Extras,
Curva de Aprendizaje (todo el módulo), Perfiles del personal, Perfiles, Usuarios, Menú por perfil y Menú por usuario.
La descarga del Excel de Horas Extras usa el permiso de Horas Extras. Además hay **acciones** por perfil y por usuario (`HorasExtras.Reabrir`,
`HorasExtras.EditarMaestro`, `Usuarios.CambiarPerfil`). El perfil 18 puede todo: en *Menú por perfil* sus pantallas
se pueden editar, pero sus acciones salen bloqueadas.

### 17.2 Orden

1. `docs/sql/2026-10-14-permisos-esquema.sql`.
2. `docs/sql/2026-10-14-permisos-ajustes-menu.sql`.
3. `docs/sql/2026-10-14-permisos-verificacion.sql` **antes** de publicar (debe dar `VEREDICTO: OK`: revisa la matriz de
   menú, los extras por usuario y que no haya acciones por usuario).
4. Binarios **juntos** (`ReporteTareas.dll` y los tres `.exe` de capa: `CapaEntidad`, `CapaNegocio`, `CapaDato`), más
   `ParametrizacionMenuPerfil.aspx`, `ParametrizacionMenuUsuario.aspx` y sus dos `.js`
   (`js\parametrizacionMenuPerfil.js`, `js\parametrizacionMenuUsuario.js`).
5. `docs/sql/2026-10-14-permisos-verificacion.sql` otra vez.

Cada script termina con `VEREDICTO: OK`.

**Advertencias:**

- Binarios publicados **antes** que el SQL: todo usuario que no sea perfil 18 queda negado en las pantallas protegidas.
- Reponer los binarios anteriores reabre el `AdministrarMenuPerfil.ashx` viejo, que no exige sesión. Si se hace, publicar de
  nuevo los binarios nuevos cuanto antes o retirar ese `.ashx`.

### 17.3 Cómo dar una pantalla o una acción

Marcarla en *Menú por perfil* o en *Menú por usuario*. Tarda hasta 2 minutos en aplicarse para quien ya tiene la sesión abierta.

- **Curva de aprendizaje:** solo la opción «Curva de aprendizaje» da el módulo (pantalla y descargas). Marcar juntas las tres
  opciones de Curva («Curva de aprendizaje», «Curva: nuevo colaborador» y «Curva: dashboard»); las dos últimas no dan acceso sin la primera. La pantalla de administración no avisa de esto.
- **Completar datos (`HorasExtras.EditarMaestro`):** además de agregar personas y registrar ajustes de sueldo, controla qué
  tramos se pagan.

### 17.4 Cambios futuros

- **Agregar una acción nueva:** una fila en `RTA_Accion`, su constante en `Acciones` (`ReporteTareas/clases/Permisos.cs`) y una línea `Permisos.PuedeHacer` en el código.
- **Proteger una pantalla más:** una línea `Permisos.PuedeEntrar` en su `.aspx.cs` y en su handler.

### 17.5 Reversa

Reponer los binarios anteriores, **todos juntos**. Las tablas pueden quedar. El script de ajustes trae su reversa comentada.

### 17.6 Lista para probar en producción

- **Con alguien del perfil 14:** entra a Tablero, Horas Extras, Parámetros de Horas Extras, Curva, Perfiles del personal y Usuarios;
  **no** entra a Perfiles, Menú por perfil ni Menú por usuario; ve «Completar datos» y **no** ve «Reabrir».
- **Con la persona de perfil 2 que tiene el Tablero como adicional:** entra a Tablero y a Horas Extras, sin «Completar datos».
- **Con cualquiera de los dos:** escribir la dirección de una pantalla que no tiene lo manda a Principal.

## 18. Módulo de empleados protegido

### 18.1 Qué es

`ObtenerListaTareas.ashx` no usaba la sesión: las acciones de las pantallas «Lista de Empleados» (`ListaEmpleados.aspx`) y
«Datos Empleado» (`RRHHEmpleados.aspx`) confiaban en el perfil y el usuario que mandaba el navegador. Cualquiera que mandara
`tipo=0` recibía la nómina completa (cédula, dirección, teléfono, fecha de nacimiento), y la descarga del Excel no pedía ni sesión.

Ahora el handler lee la sesión (`IReadOnlySessionState`) y decide con Permisos:

| Acción | Exige |
|---|---|
| `BuscarListaEmpleados` con «Lista» (todos) | «Lista de Empleados» |
| `BuscarListaEmpleados` sin «Lista» (la ficha propia, por el `Cod_Usuario` de la sesión) | cualquiera de las dos pantallas |
| `BuscarListaEmpleadosDescargar`, `EliminarEmpleado`, `BuscarListaCargaFam`, `EliminarCargaFam` | «Lista de Empleados» |

Las dos `.aspx` mandan a Principal a quien no tenga la pantalla. `GuardarNuevoEmpleado` y `GuardarNuevaCargaFamiliar`, que
llama el `.js`, no existen en el handler (responden «No existe la acción solicitada»); no cambian.

En el menú de producción (medido el 2026-10-15) las dos pantallas están visibles solo para el perfil 14, sin adicionales por
usuario: nadie pierde lo que hoy usa. Dar «Datos Empleado» sin «Lista de Empleados» deja ver la ficha propia pero no sus cargas
familiares.

### 18.2 Orden

Sin SQL. Binarios **juntos** (`ReporteTareas.dll` y los tres `.exe` de capa: `CapaEntidad`, `CapaNegocio`, `CapaDato`). Las
`.aspx` no cambian: el cambio de las páginas está en su código, dentro del `.dll`.

### 18.3 Reversa

Reponer los binarios anteriores, todos juntos. Reabre el hueco.

### 18.4 Lista para probar en producción

- **Con alguien del perfil 14:** «Lista de Empleados» muestra a todos y descarga el Excel; «Datos Empleado» muestra solo su ficha
  y sus cargas familiares. Igual que antes.
- **Con alguien de otro perfil (no 14 ni 18):** escribir `.../Formulario/ListaEmpleados.aspx` o `.../Formulario/RRHHEmpleados.aspx`
  lo manda a Principal.

## 19. Equipo y agenda (Entrega 1 de «Equipo, Calendario y Procesos»)

Spec: `docs/superpowers/specs/2026-10-15-agenda-equipo-entrega1-design.md`.

### 19.1 Qué es

Dos pantallas nuevas en el grupo de menú «Equipo y agenda»:

- **«Equipo y agenda»** (`Agenda.aspx`): tarjetas por área con la carga de la semana, y la agenda de lunes a viernes de
  cada persona, con actividades, entregables y atrasadas. La ve cualquier usuario interno: cada uno a sí mismo, cada jefe a
  toda su cadena hacia abajo (`Cod_Jefe_Inm`), y a todos quien tiene la acción `Agenda.VerTodos` (perfil 10 «Gerencia»; el
  18 siempre). Asignarle algo a otra persona le envía un correo.
- **«Áreas de la agenda»** (`ParametrizacionAreasAgenda.aspx`): catálogo de áreas con color y área de cada persona. Perfiles
  14 y 18.

Las actividades **no** son las tareas de servicio de Aranda.

### 19.2 Orden

1. `docs/sql/2026-10-15-agenda-esquema.sql` (ya aplicado en desarrollo, que es la misma base: repetirlo no hace nada).
2. `docs/sql/2026-10-15-agenda-precarga.sql` (ídem).
3. `docs/sql/2026-10-15-agenda-menu.sql` — **recién ahora**: antes, la opción de menú llevaría a una página que no existe.
3b. `docs/sql/2026-10-15-agenda-verificacion.sql` — correrlo aquí, justo después del menú y **antes** de los binarios; se vuelve a
   correr después de ellos (paso 5).
4. Binarios **juntos** (`ReporteTareas.dll` y los tres `.exe` de capa: `CapaEntidad`, `CapaNegocio`, `CapaDato`), más
   `Formulario\Agenda.aspx`, `Formulario\ParametrizacionAreasAgenda.aspx`, `Formulario\AdministrarAgenda.ashx`,
   `js\agenda.js` y `js\parametrizacionAreasAgenda.js`.

   Estos binarios **también** llevan el §18 «Módulo de empleados protegido» (commit d166526), que no se había publicado:
   publicar el §19 aplica también el §18, así que los controles del §18.4 se corren junto con los del §19.4.

   **No publicar `Web.config` en esta entrega.** El `Web.config` del paquete ya trae el bloque corregido
   `<mimeMap fileExtension=".js" …>` (c6da315), pero producción no ha recibido un `Web.config` desde el 2026-09-23; publicarlo
   es una decisión aparte, y si algún día se publica, comprobar después que un `.js` responde 200 y, si da 500.19, quitar el bloque.
5. `docs/sql/2026-10-15-agenda-verificacion.sql` otra vez — debe dar `VEREDICTO: OK`.

### 19.3 Quién cuenta como activo

`Usuario_Estado = 'A'` y `EstadoUsuario` NULL, solo internos: el criterio de los selectores de jefe. Está en un solo lugar,
`Sp_RTA_AgendaPersonasActivas`. Quien no cumple el criterio no entra a la agenda (ve un aviso para que Talento Humano revise
su estado), y la gente cuyo jefe no cumple el criterio solo la ve `Agenda.VerTodos`. La verificación lista a esas personas
(chequeo 6).

### 19.4 Lista para probar en producción

Es **obligatoria** antes de anunciar el módulo a los usuarios. Los roles de jefe y de colaborador nunca se probaron de punta a
punta: solo se probaron las reglas y, en local, un usuario con `Agenda.VerTodos`.

- **Gerencia (perfil 10):** ve todas las áreas, asigna una actividad a alguien y a esa persona le llega el correo.
- **Un jefe:** ve solo su cadena, aun buscando por nombre a alguien de otra área.
- **Un colaborador:** entra directo a su agenda, marca sus entregables, no puede mover la fecha de lo que le asignaron y no
  abre «Áreas de la agenda» (escribir la dirección lo manda a Principal).
- **Correo real:** la prueba en local se hizo solo con una persona sin correo (aviso amarillo y bitácora con Enviado = 0). Que llegue el correo con tildes y el botón «Ver en mi agenda» se comprueba aquí, en la prueba de Gerencia. Si no llega, mirar `AG_CorreoBitacora.Error` de esa actividad.

### 19.5 Reversa

Reponer los binarios anteriores, todos juntos, y correr la reversa comentada al final de `2026-10-15-agenda-menu.sql`.
Revertir los binarios también deshace el §18 (reabre el hueco de la lista de empleados): tras una reversa hay que volver a
publicar el §18 lo antes posible. Las
tablas `AG_*` pueden quedar: no las usa nada más.

## 20. Editar una actividad clasifica la hora extra

### 20.1 Qué es

Hasta el 2026-10-16, editar una actividad guardaba el tipo de hora que traía el combo (en blanco = 0). Quien creaba la actividad
con la fecha por defecto y después le cambiaba fecha y hora a un horario fuera de jornada perdía la hora extra sin aviso.
Ahora `Sp_RTAActualizaDetalleTarea` clasifica según el horario del responsable, igual que al crear.

### 20.2 Orden

1. `docs/sql/2026-09-15-clasificacion-al-editar.sql` — **ya ejecutado en producción el 2026-10-16 17:14.**
2. Binarios **juntos** (`ReporteTareas.dll` y los tres `.exe` de capa). Sin ellos la edición ya clasifica, pero **no envía la
   solicitud a la jefatura**. Son los mismos binarios del §19: publicar el §19 aplica también este.

### 20.3 Lista para probar en producción

- Crear una actividad dentro de la jornada y editarla a un sábado o a un horario nocturno: debe quedar al 100% o al 50% y llegar
  el correo de solicitud al jefe inmediato.
- Editarla a un rango que cruza el fin de la jornada: debe rechazarse con la lista de tramos para registrar por separado.

### 20.4 Reversa

`Actualizacion/rollback-Sp_RTAActualizaDetalleTarea-2023.sql` (cambiar `CREATE` por `ALTER`). Vuelve a perder el tipo al editar.

## 21. Dashboard de Contratos (sexta pestaña de Registro de Contratos)

### 21.1 Qué es

Pestaña *Dashboard* en `RegistrarContrato.aspx`: indicadores de horas y costos, alertas de los contratos en ejecución (horas o
costo excedidos, cierre vencido o en 30 días o menos, consumo sin horas contratadas) y cuatro gráficos. Reutiliza la acción
`ReporteListaContratos` de la pestaña *Listas de Contratos* y filtra en el navegador. **Sin SQL, sin menú y sin binarios.**
La ve quien ya tiene acceso a Registro de Contratos.

### 21.2 Archivos a publicar

- `Formulario\RegistrarContrato.aspx`
- `js\dashboardContratos.js` (nuevo)
- `js\chart.umd.js` ya está en el servidor desde el §16; copiarlo solo si faltara.

El `ReporteTareas.dll` del paquete se regeneró pero no trae cambios de código para esto: no hace falta publicarlo.

### 21.3 Lista para probar en producción

- Abrir *Registro de Contratos → Dashboard*: con el estado *EN EJECUCION* debe mostrar la misma cantidad de contratos que
  `SELECT COUNT(*) FROM OrdenServicio WHERE Estado = 1 AND ESTATUS = 'EN EJECUCION'` (513 el 2026-10-19).
- Clic en una orden de la tabla de alertas: abre ese contrato en la pestaña *Registro de Contratos*.
- Si no aparece la pestaña o los gráficos salen vacíos, revisar que `dashboardContratos.js` y `chart.umd.js` estén en `js\`
  (un 404 en la consola del navegador).

### 21.4 Reversa

Reponer el `RegistrarContrato.aspx` anterior. `dashboardContratos.js` puede quedar: nadie más lo carga.

## 22. Bonos de empleados

Spec: `docs/superpowers/specs/2026-10-20-bonos-design.md`.

### 22.1 Qué es

Los jefes proponen los bonos mensuales de sus reportes directos, Talento Humano los revisa fila por fila y cierra el mes, y
Nómina recibe el Excel de hoy con el concepto SAP. Una misma persona, tipo y semana no se paga dos veces. Cuatro pantallas en
el grupo de menú «Bonos»:

- **«Proponer bonos»** (`BonosProponer.aspx`): todo perfil con usuarios internos activos, y el 18.
- **«Revisar y aprobar»** (`BonosRevisar.aspx`), **«Catálogo de bonos»** (`BonosCatalogo.aspx`) y **«Reporte mensual»**
  (`BonosReporte.aspx`): perfiles 14 y 18.

Seis tablas `Bono_*` y doce procedimientos `Sp_RTA_Bono*`. Ninguna tabla existente se modifica.

### 22.2 Estado y orden

**El módulo está en uso en producción desde el 2026-10-23** (comprobado en la base el 2026-10-27): el menú existe, Talento
Humano armó el catálogo a mano (16 tipos, con otros nombres que los del Excel), octubre 2026 quedó **cerrado** con un bono y
noviembre 2026 está **abierto**. El orden original de esta sección suponía un módulo sin estrenar y ya no aplica.

| Paso | Estado |
|---|---|
| Esquema (`2026-10-22-bonos-esquema.sql`) | Aplicado el 2026-10-22; repetirlo no hace nada |
| Binarios y páginas | Publicados; la última entrega (Lugar como combo y «valor por semana × semanas») se copió el 2026-10-27 |
| Menú (`2026-10-22-bonos-menu.sql`) | **NO CORRER.** El menú ya existe y su visibilidad se restringió a mano: hoy solo lo ven los perfiles 4, 18, 21 y 41. El script la devolvería a «todos los perfiles» |
| Migración de septiembre e históricos | **Pendiente**: espera la cédula de una persona de AGILITY (paso 1 abajo) |

**Publicar una entrega nueva** del módulo: binarios **juntos** (`bin\ReporteTareas.dll` + `bin\CapaEntidad.exe` +
`bin\CapaNegocio.exe` + `bin\CapaDato.exe`) más las páginas y `.js` que cambien (`Formulario\Bonos*.aspx`,
`Formulario\AdministrarBonos.ashx`, `Formulario\DescargarBonos.ashx`, `js\bonos*.js`), sin ningún script de base.

**Trampa:** publicar el `.dll` sin los tres `.exe` de capa rompe el envío de correos (pasó el 18-09: 0 correos desde entonces y
la bitácora no lo dijo). Los cuatro binarios se copian siempre juntos. **No publicar `Web.config`** (ver §19.2: producción no lo
recibe desde el 2026-09-23 y publicarlo es una decisión aparte).

**Migración, cuando se pueda:**

1. **Antes:** Talento Humano carga la cédula del usuario `3627051` en *Administración de Usuarios*. La migración identifica a
   cada persona por cédula y por `Cod_Sap`; sin ella esa fila no aparece y el script no migra nada (lo dice y se detiene).
2. **Generar**, en la máquina que tiene `RevisionBonos/9. Septiembre.xlsx` (el Excel y la salida no están en git: el
   repositorio es público y no versiona nombres, cédulas ni montos):
   ```
   python -I docs/sql/generar-migracion-bonos.py
   ```
   Imprime la correspondencia de cada tipo del Excel con el tipo de producción (`DESTINO` en el generador, confirmada el
   2026-10-27: por ejemplo «Standby F5 UIO» y «Standby F5 GYE» → «Bono Standby F5»; Nestlé y Linde por monto). La migración
   **no duplica el catálogo**: solo crea «Bono Nuevo cargo» y marca «Bono Standby Networking» por semana. Si Talento Humano
   renombra un tipo de destino antes de migrar, el script se detiene; hay que ajustar `DESTINO` y volver a generar.
3. **Probar con ROLLBACK:** una copia del script generado con su `COMMIT TRANSACTION;` cambiado por
   `ROLLBACK TRANSACTION;`. Debe llegar hasta el final sin `FALLO:` (la verificación de esa corrida da `FALLA` porque no quedó
   nada: es lo esperado).
4. **Correr de verdad dos veces:**
   ```
   sqlcmd -S <servidor> -d ReporTarea -U <usuario> -P <clave> -b -I -i docs/sql/carga-generada/bonos-migracion.sql
   ```
   Las dos terminan en `VEREDICTO: OK` y la segunda no inserta nada. Septiembre debe dar **3195,00** con **42** bonos, y debe
   haber **17** períodos históricos (abril 2025 a agosto 2026, solo con su total). Se revierte entero si falta un tipo de
   destino, si un tipo no coincide en «por semana» o si una semana de septiembre ya la paga otro bono de la misma persona y tipo.
5. `docs/sql/2026-10-22-bonos-verificacion.sql`. Con el módulo en uso **no llega a `VEREDICTO: OK`** y no hace falta que llegue:
   el chequeo 8 (menú) falla a propósito por la visibilidad restringida a mano, y el 6 (tipos sin concepto DOS y AGILITY) falla
   hasta que Talento Humano complete los conceptos en el Catálogo. Los que sí deben dar `OK` después de migrar son el 1 a 5 y
   el 7.

### 22.3 Lista para probar en producción

Es **obligatoria** antes de anunciar el módulo. Los roles de jefe y de Talento Humano solo se probaron en local.

- **Un jefe** (p. ej. `222692` o `1051`): ve «Proponer bonos» y no ve las otras tres; propone un bono para un reporte directo,
  con el calendario de semanas (las ya tomadas, tachadas); no puede proponer para un nieto ni para un inactivo; lo envía.
- **Talento Humano** (perfil 14): le llega el correo «al enviar»; en «Revisar y aprobar» aprueba, ajusta con motivo y rechaza con
  motivo; al jefe le llega el correo de rechazo, corrige y reenvía.
- **Doble pago:** repetir la misma persona, tipo y semana es rechazado; el aviso de bono repetido pide confirmación.
- **Dos usuarios sobre el mismo bono** (dos sesiones a la vez, con la pantalla cargada en ambas antes de actuar): con un bono
  **Enviado**, una sesión de Talento Humano lo aprueba y la otra lo rechaza; o el jefe abre un **Borrador** en «Editar» y,
  antes de que guarde, Talento Humano lo rechaza desde el filtro «Borrador (sin enviar)». El segundo en guardar recibe «El
  bono cambió mientras lo revisaba; recargue la pantalla» y nada se pisa. Ojo: **dos ajustes de un bono ya Aprobado no
  chocan** (Aprobado → Aprobado: el estado esperado coincide en las dos): gana el último que guarda, y la bitácora del módulo
  registra los dos.
- **Borrador huérfano:** con un bono en Borrador, cambiar el jefe de la persona en *Administración de Usuarios*. El jefe viejo
  sigue viendo el bono en «Proponer bonos», ya no lo puede editar ni enviar, pero sí borrar. Talento Humano lo encuentra en
  «Revisar y aprobar» con el filtro «Borrador (sin enviar)», marcado «La persona ya no está a cargo de este jefe», y solo lo puede
  rechazar (con motivo): rechazado, deja de contar para el doble pago y las semanas quedan libres para el jefe nuevo.
- **Enviar con bonos que no salen:** si alguno se salta (falta la identificación, por ejemplo), el mensaje sale en amarillo y no
  se borra solo.
- **Mes de septiembre:** «Reporte mensual» muestra septiembre cerrado con 3195,00; el Excel de Nómina descargado coincide con el
  Excel original (con la columna de concepto SAP) y se puede descargar varias veces.
- **Abrir mes y cerrar mes:** abrir el mes siguiente envía el correo de apertura a los jefes con bonos en los últimos 3 períodos
  cerrados; cerrar muestra antes lo que queda fuera y bloquea si falta algún concepto.
- **Catálogo:** un tipo con bonos no se borra, se desactiva; cambiar un concepto no altera el Excel de un mes ya cerrado.
- **Tildes y símbolos** (`&`, `<`) en nota, motivo, lugar y nombre de tipo: llegan bien al Excel y al correo.
- **Correo real:** que llegue con tildes. Si no llega, mirar la bitácora del módulo y comprobar que los cuatro binarios son de la
  misma entrega.
- **Menú:** un colaborador sin gente a cargo ve el aviso y nada más; escribir la dirección de las otras tres pantallas lo manda a
  Principal.
- **Entrega del 2026-10-27** (en Proponer y en Revisar; recargar con Ctrl+F5 la primera vez):
  - **Lugar** es un combo con GUAYAQUIL, QUITO, CUENCA y AMBATO; el servidor rechaza cualquier otro valor.
  - **Tipo por semana:** al marcar semanas, «N.º de semanas» se llena solo (no se edita) y Total = valor por semana × semanas.
  - **Tipo no semanal:** «N.º de semanas» se escribe (1 a 52) y multiplica el Total y el sugerido; 0 o 53 da error.
  - **Editar en Proponer:** abrir un bono y guardarlo sin tocar nada no cambia su Total (un total que no divide exacto se
    muestra por semana con 4 decimales).

### 22.4 Reversa

**Una entrega del código:** reponer los binarios anteriores, todos juntos, con sus páginas y `.js`. No hace falta ningún script.

**Sacar el módulo entero:** además, la reversa comentada al final de `2026-10-22-bonos-menu.sql`. Con el módulo en uso eso
borra el menú y su visibilidad restringida a mano, así que es una decisión aparte. Las tablas `Bono_*` pueden quedar: no las usa
nada más.

**La migración:** no tiene reversa automática. Si hiciera falta, se borran los bonos `Origen = 'Migracion'` (con sus semanas y su
bitácora) y los períodos de septiembre 2026 e históricos; «Bono Nuevo cargo» y el cambio a «por semana» de «Bono Standby
Networking» se deshacen desde el Catálogo.

## 23. Procesos entre áreas (Entrega 2 de «Equipo, Calendario y Procesos»)

Spec: `docs/superpowers/specs/2026-10-23-procesos-entrega2-design.md`.

### 23.1 Qué es

Un trabajo que cruza áreas se lleva como un **proceso** de pasos en orden que avanza solo: al completarse la actividad de un
paso, el siguiente aparece en la agenda de su responsable (con correo «Te toca un paso»), y al terminar el último se avisa a
quien inició el proceso. Dos pantallas nuevas en el grupo de menú «Equipo y agenda» (el de §19):

- **«Procesos»** (`Procesos.aspx`): todo perfil con usuarios internos activos, y el 18. Cada uno ve y gestiona los procesos que
  inició; quien tiene la acción `Agenda.VerTodos` (perfil 10; el 18 siempre) gestiona cualquiera.
- **«Plantillas de procesos»** (`ParametrizacionPlantillasProcesos.aspx`): perfiles 10 y 18.

Plazo de un paso: de 1 a 60 días hábiles (lunes a viernes que no están en `Feriado` con `Activo = 1`), contados desde el día
siguiente a la activación. Las actividades de la agenda que no son de un proceso (§19) se comportan igual que antes. Tablas
nuevas `AG_Plantilla`, `AG_PlantillaPaso`, `AG_Proceso`, `AG_ProcesoPaso`, más `AG_Actividad.IdProcesoPaso` y
`AG_CorreoBitacora.IdProceso`; ninguna tabla fuera de `AG_*` se modifica.

### 23.2 Orden

1. `docs/sql/2026-10-23-procesos-esquema.sql` — **ya aplicado en producción el 2026-10-23** (la base local es la de producción;
   se corrió dos veces, la segunda sin cambios, y `2026-10-23-procesos-prueba.sql` dio 91/91). Repetirlo es inofensivo; si una
   entrega posterior sube la marca `AG_VersionEsquema` por encima de 2, el script se salta solo para no devolver los
   procedimientos a esta versión.
2. Binarios **juntos**: `bin\ReporteTareas.dll` + `bin\CapaEntidad.exe` + `bin\CapaNegocio.exe` + `bin\CapaDato.exe`, más:
   - `Formulario\Procesos.aspx` y `Formulario\ParametrizacionPlantillasProcesos.aspx`
   - `js\procesos.js` y `js\parametrizacionPlantillasProcesos.js`
   - `Formulario\Agenda.aspx` y `js\agenda.js` (actualizados: etiqueta del proceso y confirmación al completar; `agenda.js`
     se carga con `?v=2`, para que los navegadores no sigan con la copia vieja)

   **Trampa:** publicar el `.dll` sin los tres `.exe` de capa rompe el envío de correos (pasó el 18-09: 0 correos desde entonces
   y la bitácora no lo dijo). Los cuatro binarios se copian siempre juntos.

   **No publicar `Web.config` en esta entrega** (ver §19.2: producción no lo recibe desde el 2026-09-23 y publicarlo es una
   decisión aparte).
3. `docs/sql/2026-10-23-procesos-menu.sql` — **recién ahora, justo después de los binarios**: antes, la opción de menú llevaría
   a páginas que todavía no existen en el servidor. Agrega las dos hojas al grupo «Equipo y agenda» que ya existe; no crea
   ninguna acción nueva.
4. `docs/sql/2026-10-23-procesos-verificacion.sql` — debe dar `VEREDICTO: OK`. Imprime una línea por chequeo (`OK`, `FALLA` o
   `PENDIENTE`); antes del menú da `FALLA` a propósito (el chequeo 6 queda `PENDIENTE`), porque todo lo que no sea `OK` cuenta
   como falla.

**Nota para quien toque `AG_Actividad` a mano:** la tabla tiene un índice filtrado (`UX_AG_Actividad_ProcesoPaso`), así que
cualquier `INSERT`, `UPDATE` o `DELETE` suelto debe correr con `SET QUOTED_IDENTIFIER ON` (con `sqlcmd`, la opción `-I`); sin
eso falla con el error 1934. Los procedimientos `Sp_RTA_Agenda*` ya se crean así.

### 23.3 Plantillas de ejemplo (sugerencia)

No se precarga ninguna plantilla: Gerencia las crea desde «Plantillas de procesos» si le sirven. Las tres de la maqueta, solo
nombre y pasos (el área de cada paso es ilustrativa: **hay que ponerla según el catálogo real de «Áreas de la agenda»**, que
no coincide con los nombres de la maqueta; y cada paso necesita un plazo en días hábiles):

- **Nuevo contrato con cliente:** 1) Firma de contrato y condiciones (Comercial), 2) Registro y facturación inicial
  (Finanzas), 3) Orden de compra de equipos (Compras), 4) Verificar si requiere contratación de personal (Talento Humano).
- **Contratación de personal:** 1) Publicar vacante y filtrar candidatos (Talento Humano), 2) Entrevistas y selección final
  (Talento Humano), 3) Aprobación de banda salarial (Finanzas), 4) Onboarding e inducción (Talento Humano).
- **Compra de equipos:** 1) Cotización con proveedores (Compras), 2) Aprobación de presupuesto (Finanzas), 3) Orden de compra
  y seguimiento (Compras), 4) Recepción y configuración (Hybrid Cloud).

### 23.4 Lista para probar en producción

Es **obligatoria** antes de anunciar el módulo. Las pruebas en el navegador no se pudieron hacer en local (el ingreso es por
Active Directory): estas son las primeras de punta a punta. Usar personas reales del equipo de pruebas y un proceso con nombre
claro de prueba, y cancelarlo al terminar.

- **Plantilla:** Gerencia (perfil 10) crea una plantilla de 3 pasos en áreas distintas; un colaborador no ve «Plantillas de
  procesos» y escribir la dirección lo manda a Principal.
- **Iniciar:** desde «Procesos», iniciar el proceso con esa plantilla. El primer paso aparece en la agenda de su responsable y
  los otros dos **no**; a ese responsable le llega el correo «Te toca un paso» (con tildes), salvo que sea quien inició.
- **Un área sin personas activas** se rechaza nombrando el área, sin crear nada.
- **Completar:** el responsable completa su actividad (o marca todos sus entregables). El segundo paso aparece en la agenda de
  su responsable y le llega el correo. Marcar dos veces seguidas el último entregable activa el siguiente **una sola vez**.
- **Reasignar** el segundo paso a otra persona: la actividad pasa a su agenda y conserva los entregables.
- **Terminar:** completar el último paso; a quien inició le llega el correo «Proceso terminado» y el proceso queda Terminado.
- **Cancelar** otro proceso a mitad: los pasos pendientes y la actividad en curso dejan de aparecer.
- **Permisos:** un jefe sin `Agenda.VerTodos` no gestiona procesos ajenos; Gerencia sí.
- **Plazos:** un paso cuyo plazo cae en feriado o fin de semana vence el siguiente día hábil.
- **Feriados de 2027:** cargar los feriados de 2027 en la tabla `Feriado` antes de usar procesos con plazos largos: hoy el
  último feriado activo es el 25-12-2026.
- **Regresión de la Entrega 1:** una actividad normal de la agenda (sin proceso) se crea, se edita, marca sus entregables y se
  completa exactamente como antes: sin etiqueta de proceso, sin confirmación extra, sin correos nuevos.
- **Correo real:** si no llega, mirar `AG_CorreoBitacora.Error` de esa actividad (o del proceso, para «Proceso terminado») y
  comprobar que los cuatro binarios son de la misma entrega.

### 23.5 Reversa

1. Reponer los binarios anteriores, **todos juntos** (`ReporteTareas.dll` y los tres `.exe`), y las `.aspx`/`.js` anteriores
   (`Agenda.aspx` y `agenda.js` incluidos; las dos pantallas nuevas pueden quedar sin menú).
2. Correr la reversa comentada al final de `2026-10-23-procesos-menu.sql` (quita solo las dos hojas).
3. Los procedimientos de la Entrega 1 que este script cambió (`Actividades`, `Actividad`, `GuardarActividad`,
   `MarcarEntregable`, `MarcarCompleta`, `RegistrarCorreo`) tienen su cuerpo anterior comentado al final de
   `2026-10-23-procesos-esquema.sql`: copiar cada `ALTER PROCEDURE` en su propio lote. Con los binarios anteriores no hace
   falta: las versiones nuevas dan el mismo resultado para una actividad sin proceso, y el código viejo no lee la fila extra.
   Si hay procesos en curso, sus actividades quedan en la agenda pero ya no avanzan solas.
4. (Hacerlo antes del paso 1.) Antes de reponer los binarios anteriores, cancelar o terminar los procesos en curso: los binarios viejos permiten anular la
   actividad de un paso y ese paso ya no podría completarse.

Las tablas `AG_*` nuevas pueden quedar: no las usa nada más. Revertir los binarios también deshace lo de §18 a §22 si se
publicaron en la misma entrega.

## 24. Saldos de vacaciones (GTH)

Plan: `docs/superpowers/plans/2026-10-28-vacaciones-motor-de-saldos.md` (F5, primera pantalla).

### 24.1 Qué es

Pantalla «Saldos de vacaciones» (`VacacionesSaldos.aspx`) en el grupo «Vacaciones & Permisos», visible para los perfiles 14
(Talento Humano) y 18 (super admin); a otra persona se le da como extra desde Permisos. Solo lee: muestra el saldo de cada
colaborador calculado por `Sp_RTA_VacCalcular` a la fecha que se elija, tarjetas de totales (solo activos, como el DASHBOARD
del Excel de GTH), filtros, descarga CSV y, al hacer clic en una persona, sus periodos y sus solicitudes con lo que descuenta
cada una. Muestra cédulas de todo el personal: el handler `AdministrarVacSaldos.ashx` comprueba el permiso en cada llamada.

### 24.2 Orden

1. SQL **ya aplicado el 2026-10-28** (la base local es la de producción): `2026-10-28-vacaciones-esquema.sql`,
   `2026-10-28-vacaciones-motor.sql`, la carga generada desde el Excel (15) y `2026-10-28-vacaciones-saldos-pantalla.sql`
   (`Sp_RTA_VacColaboradores` y `Sp_RTA_VacMovimientosOrigen`). Repetirlos es inofensivo, salvo la carga, que borra y recarga.
2. Binarios **juntos**: `bin\ReporteTareas.dll` + `bin\CapaEntidad.exe` + `bin\CapaNegocio.exe` + `bin\CapaDato.exe`, más
   `Formulario\VacacionesSaldos.aspx`, `Formulario\AdministrarVacSaldos.ashx` y `js\vacacionesSaldos.js`.
3. `docs/sql/2026-10-28-vacaciones-saldos-menu.sql` — **recién después de los binarios** (corrido el 2026-10-28 tras publicar:
   Id_Menu 20101). Crea la hoja y la hace visible a 14
   y 18; avisa si el grupo 5 no fuera visible para alguno de los dos. Reversa al final del script.

Para comprobar: con «Activos» y la fecha 28/10/2026, la tarjeta «Saldo total» debe decir 2.326,41 días (95 activos).

## 25. Certificado laboral y respaldo en el alta de cargas familiares

### 25.1 Qué es

Pestaña «Certificado laboral» en Mi perfil: con o sin salario, vista previa, «Generar y descargar PDF» e historial propio.
En Perfiles del personal (14 y 18), el panel «Historial de certificados laborales» con indicadores, filtros, PDF por fila y
CSV; desde la ficha de otra persona Talento Humano también puede generárselo. Datos de `Vac_Colaborador` (sección 24) y
sueldo de `HE_Salario`. Además, el alta de una carga familiar acepta el respaldo en el mismo formulario.

### 25.2 Orden

1. SQL **ya aplicado el 2026-10-28**: `docs/sql/2026-10-28-certificado-laboral.sql` (tabla `Perfil_CertificadoLaboral` y
   4 `Sp_RTA_PerfilCertificado*`). Repetible.
2. Del paquete, **juntos**: `bin\ReporteTareas.dll` + `bin\CapaEntidad.exe` + `bin\CapaNegocio.exe` + `bin\CapaDato.exe`,
   `Controles\PerfilFichas.ascx`, `Formulario\MiPerfil.aspx`, `Formulario\PerfilesPersonal.aspx`, `js\miPerfil.js`
   (`?v=12`), `js\perfilesPersonal.js` (`?v=3`) y la carpeta nueva `Img\certificado\`. Sin ningún paso de menú.

El mismo paquete trae la sección 24 (saldos de vacaciones): los binarios son compartidos, así que se publican las dos a la
vez; el script de menú de la 24 sigue yendo después.

Para comprobar: Mi perfil → Certificado laboral de alguien del maestro; generar uno con salario y uno sin, que bajen en una
página con el membrete de su empresa y que aparezcan en «Mis certificados» y en el historial de Perfiles del personal.
Quien no está en `Vac_Colaborador` debe ver el aviso de escribir a Talento Humano.

## 26. Estado individual de vacaciones en PDF (para firmar)

Plan: `docs/superpowers/plans/2026-10-28-vacaciones-motor-de-saldos.md` (F5).

### 26.1 Qué es

Botón «PDF para firmar» en la ficha de una persona, dentro de «Saldos de vacaciones» (§24); mismo permiso que esa pantalla.
Genera la hoja ESTADO INDIVIDUAL del Excel de GTH: datos, casillas del saldo, mensaje, firmas del colaborador y de Talento
Humano y, como anexo, el detalle por periodo y las solicitudes que descuentan con el periodo al que se carga cada una (misma
regla que la columna O de REGISTRO; cuadra con el Excel en las 1.386 solicitudes). Cada PDF lleva un **folio** y queda
registrado en `Vac_EstadoEmitido` con los números impresos, quién lo generó y desde qué IP.

### 26.2 Orden

1. `docs/sql/2026-10-28-vacaciones-motor.sql` — **ya reinstalado el 2026-10-29**: `DiasTomados`, `DiasBase`, `DiasEquivalentes`
   y `DiasDescuenta` salen con 8 decimales (antes 4); los saldos no cambian. La prueba de reglas y la de oro siguen en OK.
2. `docs/sql/2026-10-29-vacaciones-estado-individual.sql` — crea `Vac_EstadoEmitido` y `Sp_RTA_VacEstadoRegistrar`. Sin esto el
   botón responde «No se pudo generar el estado de vacaciones». Repetible.
3. Binarios **juntos** (`bin\ReporteTareas.dll` + los tres `.exe` de capa), más `Formulario\DescargarVacEstado.ashx`,
   `Formulario\VacacionesSaldos.aspx` y `js\vacacionesSaldos.js` (se carga con `?v=2`).

Para comprobar: abrir la ficha de una persona, «PDF para firmar». La primera hoja trae folio, casillas y firmas; en
`Vac_EstadoEmitido` aparece una fila con ese folio y el saldo impreso.

**Trampa de HtmlRenderer:** el sitio usa `HtmlRenderer.PdfSharp` 1.5.0.6 con `HtmlRenderer` 1.5.0.5. Con esa pareja, dentro de una
celda de tabla solo se dibuja un texto simple: un `<b>`, un segundo `<div>` o un `<br />` en la misma celda desaparecen sin
error. El HTML del estado está armado con una línea por celda; lo mismo puede estar pasando en las tablas del CV.

## 27. Mis vacaciones (cada colaborador ve su saldo)

Autorizado por GTH el 2026-10-29.

### 27.1 Qué es

Página «Mis vacaciones» (`MisVacaciones.aspx`) en el grupo «Vacaciones & Permisos», visible para todo perfil que hoy ve ese
grupo. Muestra el saldo propio con el mismo cálculo que GTH (§24): casillas, mensaje, periodos y solicitudes que descuentan con
el periodo al que se carga cada una. **Cada uno ve solo lo suyo:** `AdministrarMisVacaciones.ashx` no acepta ningún parámetro
de a quién consultar; usa el `Cod_Usuario` de la sesión y lo cruza por cédula (`R_Usuarios.Cedula` y `Empleados.Cedula`). Si
no casa con nadie, o con más de una persona, muestra «Escribe a Talento Humano» y nada más.

Al 2026-10-29 tienen saldo 93 de los 95 activos. Los 2 que no casan no tienen usuario en `R_Usuarios` ni en `Empleados` con su
cédula; GTH o Sistemas tienen que completar ese dato para que lo vean.

### 27.2 Orden

1. `docs/sql/2026-10-29-vacaciones-mis-vacaciones.sql` — **ya aplicado el 2026-10-29** (`Sp_RTA_VacColaboradorDeUsuario`).
2. Binarios **juntos** (`bin\ReporteTareas.dll` + los tres `.exe` de capa), más `Formulario\MisVacaciones.aspx`,
   `Formulario\AdministrarMisVacaciones.ashx` y `js\misVacaciones.js`.
3. `docs/sql/2026-10-29-vacaciones-mis-vacaciones-menu.sql` — **después de los binarios**. Crea la hoja y la hace visible a los
   mismos perfiles que ven el grupo 5. Reversa al final del script.

Para comprobar: con un usuario común, «Vacaciones & Permisos → Mis vacaciones» muestra su nombre y su saldo; con un usuario
sin registro, el aviso de escribir a Talento Humano.

## 28. Nómina y provisión de vacaciones (GTH)

### 28.1 Qué es

Pantalla «Nómina de vacaciones» (`VacacionesNomina.aspx`) en el grupo «Vacaciones & Permisos», **solo perfiles 14 y 18**:
muestra sueldos. Es la hoja NOMINA del Excel de GTH: por persona, sueldo, valor día, días a liquidar (el saldo del motor) y
valor = días × sueldo ÷ 30; tarjetas y resumen por empresa de los activos; filtros y CSV. Para inactivos, a la fecha de salida.

**De dónde sale el sueldo:** el vigente en `HE_Salario` (misma regla que horas extras y el certificado); si la persona no está
ahí, el de `Vac_Sueldo`, una tabla nueva **solo para vacaciones**, guardada por cédula y cargada desde el Excel. No se cargó
nada en `HE_Salario` a propósito: lo leen también el Tablero de Costos y horas extras. Al 2026-10-29: 61 activos con
`HE_Salario` (todos iguales al Excel) y 34 con el del Excel.

**Cuadre (2026-10-28):** 95 activos, 2.326,41 días y **$113.257,39**, igual que NOMINA!N4:O4; días y valor iguales al Excel
en las 106 personas.

### 28.2 Orden

1. SQL **ya aplicado el 2026-10-29**: `docs/sql/2026-10-29-vacaciones-nomina.sql` (`Vac_Sueldo`, `Sp_RTA_VacSueldos`) y la
   carga de sueldos `docs/sql/carga-generada/vacaciones-sueldos.sql` (96 sueldos; generada por `generar-carga-vacaciones.py`,
   fuera de git porque son sueldos).
2. Binarios **juntos** (`bin\ReporteTareas.dll` + los tres `.exe` de capa), más `Formulario\VacacionesNomina.aspx`,
   `Formulario\AdministrarVacNomina.ashx` y `js\vacacionesNomina.js`.
3. `docs/sql/2026-10-29-vacaciones-nomina-menu.sql` — **después de los binarios**. Hoja visible para 14 y 18.

**Recarga desde un Excel nuevo:** `vacaciones-sueldos.sql` reemplaza solo los sueldos de origen EXCEL. La carga completa
(`vacaciones-carga.sql`) ahora **se niega si ya hay folios emitidos** (§26): recrea las personas con Id nuevos y los folios
quedarían apuntando a otra persona.

## 29. Liquidación de vacaciones para salidas (GTH)

### 29.1 Qué es

Pantalla «Liquidación de vacaciones» (`VacacionesLiquidacion.aspx`) en el grupo «Vacaciones & Permisos», **solo perfiles 14 y
18** (muestra sueldos). Es la hoja LIQUIDACION del Excel de GTH: se elige a la persona y la fecha de salida (por omisión, la
registrada; si no tiene, hoy) y muestra el proporcional del periodo en curso con su fórmula, los días a liquidar (cumplidos +
proporcional − usado), el valor referencial (sueldo ÷ 30, el mismo sueldo de la nómina §28), el valor del Art. 71 si se ingresa
lo ganado en los últimos 12 meses, el detalle por periodo y las solicitudes consideradas. **Solo simula: no registra nada.**

**Solo cuenta lo tomado hasta la fecha de salida:** para eso `Sp_RTA_VacCalcular` tiene el parámetro `@Liquidacion` (1 = a esa
fecha exacta, sin solicitudes posteriores). Con 0, que es el valor por omisión, el cálculo es el de siempre.

**Cuadre:** el ejemplo del Excel (salida 28/10/2026) da 14,14 días, el mismo valor referencial y los mismos 3 periodos.

### 29.2 Orden

1. `docs/sql/2026-10-28-vacaciones-motor.sql` — **ya reinstalado el 2026-10-29** con `@Liquidacion`; la prueba de reglas y la de
   oro siguen en OK.
2. Binarios **juntos** (`bin\ReporteTareas.dll` + los tres `.exe` de capa), más `Formulario\VacacionesLiquidacion.aspx`,
   `Formulario\AdministrarVacLiquidacion.ashx` y `js\vacacionesLiquidacion.js`.
3. `docs/sql/2026-10-29-vacaciones-liquidacion-menu.sql` — **después de los binarios**. Hoja visible para 14 y 18 (título con
   tilde vía `NCHAR`).

## 30. Ajustes de vacaciones: saldo de corte y compra (GTH)

### 30.1 Qué es

Pantalla «Ajustes de vacaciones» (`VacacionesAjustes.aspx`) en el grupo «Vacaciones & Permisos», **solo perfiles 14 y 18**.
**Es la primera pantalla del módulo que escribe saldos.** Por persona:

- **Saldo de corte:** ponerlo, cambiarlo o quitarlo. «Ver cómo queda» calcula el saldo con el corte nuevo usando el mismo
  motor (`Sp_RTA_VacCorteSimular` cambia, calcula y deshace), sin guardar nada.
- **Compra de vacaciones:** se registra como un movimiento `COMPRA DE VACACIONES`, `Origen SISTEMA`, que el motor ya descuenta
  (sin cargo de fin de semana). No se borra: se anula (`DADA BAJA`).
- **Bitácora** (`Vac_Bitacora`): cada cambio con antes, después, motivo obligatorio, usuario de la sesión, fecha e IP. El cambio y
  su fila de bitácora van en la misma transacción.

Reglas: el corte no puede ser anterior al ingreso, posterior a hoy ni posterior a la salida; el saldo va de 0 a 120 con 2
decimales. La compra no puede ser anterior al corte (el motor no la descontaría) ni pasar del saldo de hoy. Si dos personas de
GTH editan el corte a la vez, la segunda recibe «alguien más cambió el saldo de corte» y no pisa a la primera. Si la persona
tiene estados individuales emitidos (§26), la pantalla avisa que el papel firmado queda desactualizado.

**Consecuencias:**
- Con un ajuste hecho, la carga del Excel (`generar-carga-vacaciones.py`) **se niega a correr**: pisaría los cortes de GTH.
  Una compra registrada ya la bloqueaba por ser `Origen SISTEMA`.
- La prueba de oro (motor contra el Excel) deja de cuadrar para quien tenga un ajuste: es lo esperado, el sistema pasa a mandar.

### 30.2 Orden

1. `docs/sql/2026-10-29-vacaciones-ajustes.sql` — crea `Vac_Bitacora` y 5 procedimientos. Repetible; no toca nada existente.
   Puede ir antes que los binarios.
2. Binarios **juntos** (`bin\ReporteTareas.dll` + los tres `.exe` de capa), más `Formulario\VacacionesAjustes.aspx`,
   `Formulario\AdministrarVacAjustes.ashx` y `js\vacacionesAjustes.js`.
3. `docs/sql/2026-10-29-vacaciones-ajustes-menu.sql` — **después de los binarios**. Hoja visible para 14 y 18.
