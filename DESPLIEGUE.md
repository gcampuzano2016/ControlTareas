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
