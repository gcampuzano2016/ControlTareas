# Perfil del colaborador

Fecha: 2026-09-14
Rama: `ProyectoNuevosCambios`
Origen: maqueta `perfil-colaborador (2).html` (aprobada por el equipo)

## Problema

Cada colaborador debe tener su perfil dentro del sistema: ver los datos que RRHH
mantiene sobre él, actualizar por su cuenta los que le corresponden, registrar su
formación y su experiencia, y dejar constancia de a quién llamar en una emergencia.
Cada jefatura debe poder consultar —sin editar— lo indispensable de su equipo.

Hoy no existe nada de eso. La opción **Perfil Usuario** del desplegable de usuario
(`ReporteTareas/Formulario/Master.Master:104`) es un `href="#"` que no lleva a ningún
lado.

## Terreno verificado

Todo lo de esta sección se comprobó con consultas de solo lectura contra `ReporTarea`
en producción el 2026-09-14, con autorización expresa.

### Dos mundos de datos que no se hablan

La maqueta mezcla campos de dos orígenes sin relación declarada entre ellos:

| Origen | Aporta | Llave |
|---|---|---|
| `dbo.R_Usuarios` (el login) | Nom_Usuario, Cedula, Departamento, E_Mail, Cod_Jefe_Inm | `Cod_Usuario` |
| `dbo.Empleados` (ficha de RRHH, 133 filas) | PuestoTrabajo, AreaTrabajo, Ciudad, Direccion, Telefono, Correo, EstadoCivil, Fecha_nacimiento | `IdEmpleado` |

`dbo.Info_empleados` existe pero es una copia vieja y más pobre. Se ignora.

### El enlace por cédula cubre la mitad de la gente

De 231 usuarios activos (`EstadoUsuario = 0`; hay 237 en total):

| Situación | Usuarios |
|---|---:|
| Con cédula **y** ficha de empleado | **118** (51%) |
| Con cédula, sin ficha | 47 (20%) |
| **Sin cédula** — no hay forma de enlazarlos | **66** (29%) |
| Comparten cédula con otro login | 6 |

De los 66 sin cédula, 34 tienen correo `@dos.com.ec` y 35 tienen departamento: son
personas reales, no cuentas de prueba.

**Consecuencia de diseño:** `Empleados` no puede ser la columna vertebral del perfil, o
el 49% de la empresa entra y ve una pantalla vacía.

### `Cod_Sap` no sirve como llave

Parecía la mejor opción con 192 coincidencias, hasta mirar la causa: 63 de 133
empleados tienen `CodigoSap` en 0 o nulo y 60 usuarios lo tienen vacío. Son 71 valores
distintos para 133 filas. Esas coincidencias son basura cruzándose con basura.
Descartado.

### Tres campos que la maqueta da por poblados están vacíos

| Campo | Filas con dato |
|---|---:|
| `R_Usuarios.Cargo` | **2** de 231 |
| `R_Usuarios.TelefonosEmergencia` | **0** |
| `InfoAdicionalHisClinicas.NomContactoEmergencia` | **0** de 18 filas |

El cargo real vive en `Empleados.PuestoTrabajo` (132 de 133 poblados).

Los contactos de emergencia **no existen en ninguna parte del sistema**. No hay
migración que hacer: se construyen de cero.

### El módulo de cargas familiares existe y nunca se usó

`dbo.Emp_CargaFamiliar` tiene **0 filas**, pese a tener CRUD completo
(`DaoCargaFamiliar` con cuatro SPs y la UI en `RRHHEmpleados.aspx`). Sirve como código
de referencia, pero no hay datos que respetar: la tabla se puede reformar libremente.

El modal de subida de documentos de esa pantalla (`RRHHEmpleados.aspx:424`) es un
cascarón: `<formview action="/action_page.php">` con un submit plano. No postea a ningún
lado.

### El árbol de jefaturas sí sirve

`Cod_Jefe_Inm` está poblado en **231 de 231** activos. De esos, **215 resuelven** a un
`Cod_Usuario` real y 16 no. Hay **26 personas que son jefe de alguien** y nadie es jefe
de sí mismo.

Los equipos más grandes son de **49, 35, 18 y 15** reportes directos. El `<select>` de
tres nombres de la maqueta no sirve a esa escala.

### La fecha de nacimiento parece podrida y no lo está

`Empleados.Fecha_nacimiento` es `nvarchar(50)`. Con `TRY_CONVERT(date, ...)` fallan 80
de 133 y parece dato corrupto. No lo es: las 129 no vacías son `dd/mm/yyyy` y convierten
**todas** con `TRY_CONVERT(date, Fecha_nacimiento, 103)`.

**Sin el estilo 103 explícito la edad se rompe para el 60% de la gente, en silencio.**

8 fechas quedan fuera de rango razonable (menores de 15 o nacidos antes de 1940). Son
erratas de carga, para RRHH.

### Ninguno de los dos correos es limpiamente personal ni corporativo

| Campo | Corporativos | Externos |
|---|---:|---:|
| `Empleados.Correo` | 108 | 23 |
| `R_Usuarios.E_Mail` | 171 | 39 |

El "Correo personal" editable de la maqueta no tiene dónde vivir, y reinterpretar
`Empleados.Correo` como personal declararía personales 108 direcciones `@dos.com.ec`.

### El generador de PDF bueno no es el que parece

`clases/PDF.cs` usa Pechkin 0.5.8 sobre wkhtmltopdf. Según sus propios comentarios
(`PDF.cs:419`): compilación **de 32 bits** que no carga en un grupo de aplicaciones de
64 bits, **falla una de cada dos veces en el servidor** (hay un bucle de dos intentos
puesto para tapar eso), no entiende flexbox ni grid, y no resuelve data URI —las
imágenes van por ruta de archivo.

`clases/PdfLista.cs` usa **PdfSharp + HtmlRenderer**: puramente gestionado, sin DLL
nativa, sin el problema de 32/64 bits, sin fallos intermitentes.

## Decisiones

**`R_Usuarios.Cod_Usuario` es el eje; `Empleados` es enriquecimiento opcional.** La
cabecera se arma con `LEFT JOIN`, de modo que los 119 sin ficha entran y ven su perfil
con lo que sí se sabe de ellos. Todas las tablas nuevas cuelgan de `Cod_Usuario`, así
que formación, certificaciones, experiencia y emergencia funcionan para los 231 desde el
primer día.

**Se agrega `Empleados.Cod_Usuario` como enlace explícito**, poblado una sola vez
cruzando por cédula. Enlaza 112 y deja 119 sueltos: el listado de esos 119 es un
entregable para RRHH, no un bloqueante para salir.

**El `Cod_Usuario` nunca viaja en el payload.** Sale de `context.Session`.

**Lo que la jefatura puede ver se decide en el SP, no en la vista.** Es un procedimiento
distinto que sencillamente no selecciona las columnas prohibidas.

**Tipo de sangre se queda en el módulo médico.** Es dato de salud con sus propias reglas
de confidencialidad; no entra a un perfil autogestionado.

**Antigüedad sale del sidebar.** No existe fecha de ingreso de personas en ninguna
tabla. La edad sí queda, calculada con estilo 103.

**El CV se genera con PdfSharp/HtmlRenderer**, no con Pechkin. Un CV es títulos y
listas; no justifica una ruta que falla la mitad de las veces.

**La pantalla se adapta al sistema.** Estructura de la maqueta, piezas de Bootstrap 3 e
iconos `fa`. Sin Google Fonts.

**No hay claves foráneas.** El esquema no tiene ninguna; poner una sola aquí daría
integridad de mentira.

## Diseño

### Componentes

| Archivo | Rol |
|---|---|
| `ReporteTareas/Formulario/MiPerfil.aspx` + `.cs` | Content bajo `Master.Master`, molde de `ParametrizacionHorarioUsuario.aspx` |
| `ReporteTareas/js/miPerfil.js` | Pestañas, render de listas, control de cambios, subidas |
| `ReporteTareas/Formulario/AdministrarPerfil.ashx` + `.cs` | Handler del módulo |
| `CapaEntidad/EntPerfil*.cs` | Entidades |
| `CapaDato/DaoPerfil.cs`, `CapaNegocio/NegPerfil.cs` | Las otras dos capas |
| `docs/sql/2026-09-14-perfil-colaborador.sql` | Tablas, SPs y poblado del enlace |

### Modelo de datos

Siete tablas nuevas, todas con eje `Cod_Usuario VARCHAR(50)` y las cuatro columnas de
auditoría del resto del esquema. Estilo de `docs/sql/2026-08-25-*.sql`:
`INT IDENTITY(1,1)`, constraints con nombre, `DATETIME2(0)` con `SYSDATETIME()`.

Las cinco tablas de lista llevan `Estado CHAR(1)` con default `'1'` para borrado
lógico. `Perfil_ContactoPersonal` y `Perfil_Foto` no: son de una fila por persona, con
`Cod_Usuario` como llave primaria, y ahí borrado lógico no significa nada — se
actualizan o se borran de verdad.

| Tabla | Columnas propias |
|---|---|
| `Perfil_ContactoPersonal` | `Cod_Usuario` PK, CorreoPersonal, TelefonoPersonal, Direccion |
| `Perfil_ContactoEmergencia` | Nombre, Parentesco, Telefono |
| `Perfil_Estudio` | Nivel, Institucion, Titulo, AnioGraduacion |
| `Perfil_Certificacion` | Nombre, Entidad, FechaObtencion |
| `Perfil_Experiencia` | Empresa, Cargo, AnioDesde, AnioHasta (nulo = actualidad), Funciones |
| `Perfil_Documento` | Origen (`CERTIFICACION` \| `CARGAFAMILIAR`), IdOrigen, NombreArchivo, NombreArchivoCodigo, Ruta |
| `Perfil_Foto` | `Cod_Usuario` PK, FotoBase64 `VARCHAR(MAX)`, FotoTipo |

`Perfil_ContactoPersonal` nace porque ni `Empleados.Correo` ni `R_Usuarios.E_Mail`
sirven como correo personal (ver terreno). Los dos existentes se quedan como están,
cumpliendo su función de notificación.

`AnioDesde`/`AnioHasta` en vez del `"2017 – 2019"` de texto libre de la maqueta: el CV
necesita ordenar la experiencia y con texto no se puede.

`Perfil_Documento` es una sola tabla para todos los respaldos, con el molde de
`EntArchivoTarea` (nombre original + nombre codificado + ruta), para que el respaldo de
una certificación y el de una carga familiar se suban, listen y borren con el mismo
código.

`Perfil_Foto` es tabla aparte y no columna de `R_Usuarios` para no meter un
`VARCHAR(MAX)` en la tabla que se lee en cada request del menú. Sigue el patrón de
`DaoFirmaUsuario.Obtener()`: base64 + tipo, armado como data URI al leer.

Dos cambios sobre lo existente, ambos aditivos:

```sql
ALTER TABLE dbo.Empleados         ADD Cod_Usuario VARCHAR(50) NULL;  -- + indice
ALTER TABLE dbo.Emp_CargaFamiliar ADD Cod_Usuario VARCHAR(50) NULL;  -- tabla vacia
```

`Emp_CargaFamiliar` lo recibe para que los 119 sin ficha puedan registrar cargas. Se le
escriben SPs nuevos para el perfil; los cuatro de RRHH quedan intactos.

### La regla de identidad

**`Cod_Usuario` no viaja nunca en el payload. Sale de `context.Session["Cod_Usuario"]`.**

Es el patrón que ya usan `AdministrarUsuarios.ashx`, `AdministrarPerfiles.ashx` y
`AdministrarMenuUsuario.ashx`: los tres con `IRequiresSessionState`, rechazo al tope si
no hay `UserLogin`, y el comentario de `AdministrarUsuarios.ashx.cs:238` — *"Quién hace
el cambio sale de la sesión, nunca del cliente."*

Se copia la **estructura** de `AdministrarHorarioUsuario.ashx`, no su manejo de
identidad: ese handler toma `codUsuario` del cliente y jamás mira la sesión
(`AdministrarHorarioUsuario.ashx.cs:97`). En una pantalla de administración detrás del
menú es tolerable; en un perfil autogestionado sería que cualquiera lea y sobrescriba el
perfil de cualquiera desde la consola del navegador.

Tres guardas:

1. **Perfil propio** — identidad de la sesión. Si el payload trae `codUsuario`, se
   ignora; no se valida. Lo que no se lee no se puede falsificar.
2. **Perfil del equipo** — única acción que recibe un `codUsuario` ajeno. La validación
   va en SQL, no en C#:
   ```sql
   WHERE u.Cod_Usuario  = @CodUsuarioConsultado
     AND u.Cod_Jefe_Inm = @CodUsuarioSesion
   ```
   Si no es tu subordinado directo, son cero filas y no hay rama de código que saltarse.
3. **Lista blanca de campos** — del payload solo se leen los editables. `Cargo`,
   `AreaTrabajo`, `Cedula`, `Fecha_nacimiento` y `PuestoTrabajo` no se leen nunca, así
   que ninguna petición los puede tocar. Es lo que en la maqueta era un `disabled` de
   CSS.

### Acciones del handler

`CargarPerfil` · `GuardarContacto` · `GuardarFoto` · `GuardarEstudio` /
`EliminarEstudio` · `GuardarCertificacion` / `EliminarCertificacion` ·
`GuardarExperiencia` / `EliminarExperiencia` · `GuardarEmergencia` /
`EliminarEmergencia` · `GuardarCargaFamiliar` / `EliminarCargaFamiliar` · `ListaEquipo`
· `PerfilEquipo` · `DescargarCV`

Los documentos van por `multipart` reusando `CargaArchivos.ashx`, que es donde ya vive
la subida real.

Respuestas con `EntRespuesta` (`estado`/`mensaje`/`tipoMensaje`) y `MostrarMensaje`,
como el resto. Se usa el `ToJson` local con `JavaScriptSerializer` que ya trae
`AdministrarHorarioUsuario`, porque el helper compartido usa `Encoding.Default` y rompe
las tildes.

### Flujo de carga

`CargarPerfil` no recibe parámetros. Llama a `Sp_RTA_PerfilColaborador @Cod_Usuario`,
que devuelve **siete result sets** en una sola ida: cabecera, estudios, certificaciones,
experiencia, emergencia, cargas familiares y documentos. El Dao los recorre con
`NextResult()`. Seis pestañas, una consulta.

La cabecera es `R_Usuarios LEFT JOIN Empleados ON Cod_Usuario` —**LEFT**, que es lo que
hace entrar a los 119 sin ficha— y la edad sale de
`TRY_CONVERT(date, Fecha_nacimiento, 103)`.

### Matriz de permisos

Lo que no está en la tabla, no se muestra ni se envía.

| Campo | Dueño | Jefatura directa | Origen |
|---|:--:|:--:|---|
| Nombre, cargo, área, ciudad | ve | ve | `Empleados` / `R_Usuarios` |
| Horario | ve | ve | `R_UsuarioHorarioLaboral` |
| Correo de notificación | ve | ve | `R_Usuarios.E_Mail` |
| Jefe inmediato | ve | ve | `R_Usuarios.Cod_Jefe_Inm` |
| Cédula, fecha de nacimiento, edad | ve | — | `Empleados` |
| Domicilio, correo y teléfono personales | **edita** | — | `Perfil_ContactoPersonal` |
| Estado civil | **edita** | — | `Empleados.EstadoCivil` |
| Formación, certificaciones, experiencia | **edita** | ve | tablas nuevas |
| Contactos de emergencia | **edita** | ve | `Perfil_ContactoEmergencia` |
| Cargas familiares | **edita** | — | `Emp_CargaFamiliar` |
| Foto | **edita** | ve | `Perfil_Foto` |

### Vista de jefatura

La pestaña **aparece sola**: el SP cuenta si alguien tiene `Cod_Jefe_Inm` apuntándole y
la pestaña existe o no según eso. No la enciende un perfil ni una fila de menú. Como el
campo está poblado al 100%, funciona desde el día uno sin mantener una lista de quién es
jefe.

La lista del equipo lleva **buscador**, como `ParametrizacionHorarioUsuario`: con 49
reportes directos un `<select>` no sirve.

Sin jerarquía recursiva: la maqueta dice "su equipo" y el dato disponible es el jefe
inmediato. Un jefe de jefe no ve a los nietos.

### El CV

`DescargarCV` arma el HTML en el servidor con los mismos datos que ya cargó el perfil,
genera el PDF con **PdfSharp/HtmlRenderer** en `~/descargas/` con nombre
`CV_<Cod_Usuario>_<timestamp>.pdf` y devuelve la ruta. El marcado va con tablas y
estilos en línea, como `HtmlSolicitud`, por las limitaciones de CSS del renderizador.

**Solo del perfil propio.** Una jefatura consulta el perfil de su equipo; no se descarga
sus hojas de vida.

### La pantalla

Estructura de la maqueta con piezas de Bootstrap 3: sidebar `col-lg-3` (foto, nombre,
cargo, chips de área y ciudad, botones) y `col-lg-9` con `nav nav-tabs` + `tab-content`
para las seis pestañas. El candado y el lápiz pasan a
`<span class="label label-default"><i class="fa fa-lock">` y `label-info` con
`fa-pencil`. La barra flotante de guardar se ancla al pie del sidebar, con el mismo
comportamiento: aparece cuando hay cambios.

Se entra por el desplegable de usuario (`Master.Master:104`), que ven todos los que
iniciaron sesión — el público correcto para un perfil propio. **No hace falta registrar
nada en `MenuDos`/`PerfilMenu`.**

## Fases

El corte no es "lo fácil primero", es a cuánta gente le sirve.

**Fase 1 — el perfil propio.** Script SQL completo (siete tablas, dos `ALTER`, poblado
del enlace), `MiPerfil.aspx`, handler, tres capas, y el enlace en `Master.Master:104`.
Pestañas *Datos personales* (bloqueada), *Contacto y domicilio* y *Emergencia*.

El script crea las siete tablas de una vez, aunque la fase 1 solo use tres, para que
las fases 2 y 3 no vuelvan a tocar el esquema en producción. Una sola ventana de
cambio de base en vez de tres.

La emergencia va primero, aunque en la maqueta sea la última pestaña, porque es lo único
del módulo con valor operativo inmediato: hoy no existe en ninguna parte a quién llamar,
y funciona para los 231.

Entregable adicional, no de código: el listado de los **119 sin enlace de ficha, 66 sin
cédula, 6 con cédula repetida, 16 con jefe que no resuelve y 8 fechas de nacimiento
fuera de rango**, para RRHH.

**Fase 2 — la hoja de vida.** Formación, Certificaciones, Experiencia y cargas
familiares con `Cod_Usuario`. Todo para los 231.

**Fase 3 — lo visual y lo compartido.** Foto, documentos de respaldo reales sobre
`CargaArchivos.ashx`, CV en PDF y vista de jefatura — al final porque sin la fase 2 no
tendría casi nada que mostrar.

## Verificación

La solución no tiene proyecto de pruebas (cuatro proyectos, ninguno lo es). Se agrega
uno **chico, limitado a las tres cosas que fallan en silencio**:

1. La conversión de fecha con estilo 103 y el cálculo de edad.
2. La lista blanca de campos editables — que un payload con `Cargo` no lo guarde.
3. La guarda de jefatura — que un `codUsuario` que no es subordinado devuelva vacío.

Un error en cualquiera de las tres no da la cara: la edad sale mal, el campo se guarda,
el dato se muestra. Nadie va a volver a mirarlas.

El script SQL lleva consultas de aserción junto a los `PRINT`, como el resto de
`docs/sql/`.

Lista de comprobación manual por fase, con los casos que dictaron los datos:

- Un usuario **de los 118** con ficha: ve todo.
- Uno **de los 66 sin cédula**: ve su perfil con lo disponible, no una pantalla vacía.
- Uno **de los 6 con cédula repetida**: no ve datos de la otra persona.
- El jefe **con 49 reportes**: el buscador aguanta y lista.
- Un intento de leer el perfil de alguien que no es subordinado: vuelve vacío.
- Alguien con fecha de nacimiento `dd/mm/yyyy` con día > 12: la edad sale correcta.

## Despliegue

Según `DESPLIEGUE.md`:

1. **El script SQL corre primero**, siempre. Idempotente y con `PRINT`.
2. Publicar con `FolderProfile` y **copiar sin sincronizar** — un `robocopy /MIR` borra
   `connections.config` y `appsettings.config` del servidor y el sitio no arranca.
3. Compilar con el **MSBuild de VS2019**; el del PATH falla con errores que despistan.
4. `miPerfil.js` sale con `?v=1` y cada despliegue posterior sube el número.

**Rollback:** todo el cambio es aditivo —tablas, columnas, SPs y pantalla nuevos—. Se
republican los binarios anteriores y se devuelve el `href="#"` en `Master.Master:104`.
Las tablas quedan vacías sin molestar. No hay migración destructiva en ninguna fase.

## Fuera de alcance

- **Tipo de sangre en el perfil.** Se queda en la historia clínica.
- **Antigüedad.** No hay fecha de ingreso en ninguna tabla.
- **Jerarquía recursiva** en la vista de jefatura. Solo jefe inmediato.
- **Descargar el CV de un subordinado.**
- **Corregir los datos de RRHH.** El módulo entrega el listado de excepciones; la
  corrección es de RRHH.
- **Arreglar el modal muerto de `RRHHEmpleados.aspx:424`.** Esa pantalla queda como
  está; el perfil trae su propia subida funcionando.
- **Rotar el secreto de `sp.txt`.** Se detectó de paso (usuario y contraseña
  `onmicrosoft.com` en texto plano, versionados). Tarea aparte, en la línea de
  `DESPLIEGUE.md` §6.
- **La barra flotante de guardar con control de cambios.** Este diseño la prometía;
  la revisión final de la fase 1 la encontró sin construir y se decide dejarla fuera
  a propósito, no como algo pendiente de última hora. El botón fijo "Guardar cambios"
  que sí existe cumple la función; la barra con detección de cambios sin guardar queda
  para cuando el módulo tenga más de una sección editable a la vez.
- **La edición de contactos de emergencia.** También prometida en este diseño. El
  procedimiento `Sp_RTA_PerfilGuardarEmergencia` la soporta (`IdContacto` distinto de
  cero es edición) y `DaoPerfil.GuardarEmergencia` traduce su resultado `-1`, pero la
  pantalla de la fase 1 solo ofrece agregar y quitar: no hay botón ni flujo que llegue
  a esa rama. Se deja el código porque sirve tal cual para la fase 2; hoy es
  inalcanzable desde la interfaz y se difiere a propósito, no un olvido.

## Riesgos

**El módulo rinde a medias hasta que RRHH complete los datos.** 119 de 231 no tienen
ficha enlazada. Ven su perfil y usan todo lo nuevo, pero las pestañas de datos
personales les salen incompletas. Es visible y esperado, no una falla; conviene decirlo
al presentar el módulo para que no se lea como un error del sistema.

**El estilo 103 es fácil de perder en un cambio posterior.** Si alguien reescribe la
consulta sin él, la edad se rompe para el 60% sin ningún error. Por eso está en las
pruebas y con comentario en el SP.

**Un jefe con 49 reportes ve 49 perfiles.** Es superficie real de datos personales,
aunque la matriz esté recortada a lo operativo y de emergencia. Si RRHH quiere revisar
qué expone la vista de jefatura, este es el momento.

**Los 6 usuarios con cédula repetida.** Dos logins con la misma cédula enlazarían a la
misma ficha. El poblado del enlace los deja fuera a propósito y los reporta, en vez de
elegir uno; es el mismo criterio que ya usa `DaoFirmaUsuario` cuando el código de
usuario está repetido.

**PdfSharp/HtmlRenderer tiene CSS limitado.** El CV se ve más sobrio que la vista previa
de la maqueta. Es el precio de no usar la ruta que falla la mitad de las veces.
