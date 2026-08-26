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

- todo lo publicado (`bin\`, `Formulario\`, `js\`, `Web.config`, …)
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

El orden correcto es seguro incluso si tardas en copiar los binarios: los
procedimientos declaran los parámetros nuevos con valor por defecto, así que el
código viejo los sigue llamando sin enterarse. Por eso hay margen entre un paso
y el otro, pero solo en ese sentido.

Los scripts son idempotentes: si dudas si ya corriste uno, córrelo de nuevo. Los
`PRINT` te dicen si creó algo o si ya existía.

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
