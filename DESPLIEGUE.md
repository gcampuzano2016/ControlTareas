# Despliegue

Guía para publicar ReporteTareas. Lee la sección 1 antes del primer
despliegue posterior a agosto de 2026: cambió dónde viven las contraseñas y un
despliegue a medias tumba el sitio.

---

## 1. Lo que cambió: las credenciales ya no están en el código

Antes, las cadenas de conexión estaban escritas dentro de `CapaDato/*.cs`, con
la contraseña de `sa` en claro. Como el repositorio es público, cualquiera
podía leerlas.

Ahora:

| archivo | contenido | ¿va en git? |
|---|---|---|
| `ReporteTareas/Web.config` | `<connectionStrings configSource="connections.config" />` | sí, y no tiene secretos |
| `ReporteTareas/connections.config` | las cadenas reales, con contraseñas | **no**, está en `.gitignore` |
| `ReporteTareas/connections.config.ejemplo` | plantilla con placeholders | sí |

La consecuencia práctica: **`connections.config` no viaja ni en git ni en la
publicación**. Existe porque alguien lo creó a mano en esa máquina. En un
servidor nuevo hay que crearlo, o el sitio levanta y falla al primer query.

---

## 2. Preparar una máquina o un servidor por primera vez

```
copy ReporteTareas\connections.config.ejemplo ReporteTareas\connections.config
```

Luego edita `connections.config` y reemplaza `SERVIDOR`, `USUARIO` y
`CONTRASENA` por los valores reales del ambiente. Se hace **una sola vez**: las
publicaciones posteriores no lo tocan.

Nunca lo agregues a git. Si `git status` lo muestra, algo se rompió en el
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
> guardes ahí `connections.config` ni nada que quieras conservar: se pierde en
> la siguiente publicación. El `connections.config` va en el servidor, junto al
> `Web.config` desplegado, no en la carpeta de publicación.

### Cómo llegan los archivos al servidor

<!-- PENDIENTE: completar. No está documentado en el repositorio y no consta
     en el historial. Quien haga el próximo despliegue debería escribir aquí
     el paso real: copia manual, recurso compartido, FTP, Web Deploy, etc.,
     junto con la ruta en el servidor y el nombre del sitio en IIS. -->

---

## 4. El orden importa

El `.dll` nuevo y el `Web.config` viejo son incompatibles. Si actualizas uno
sin el otro, el sitio se cae.

| qué despliegas | resultado |
|---|---|
| solo `bin\` sobre un `Web.config` viejo | **falla** — el código busca cadenas que ese `Web.config` no declara |
| un paquete viejo completo | funciona, pero reintroduce las contraseñas en el código |
| publicación nueva + `connections.config` en el servidor | correcto |

Orden seguro:

1. Verifica que `connections.config` ya exista en el servidor (sección 2).
2. Publica y copia los archivos.
3. Recién entonces deja entrar tráfico.

---

## 5. Verificar después de desplegar

1. Abre el sitio e inicia sesión. El login va por Active Directory, pero la
   pantalla siguiente ya consulta la base: si las cadenas están mal, falla ahí.
2. Abre una pantalla que liste tareas. Es la que usa la conexión `ReporTarea`,
   la de casi todo el sistema.
3. Si aparece un error de referencia nula al conectar, casi siempre es que
   falta `connections.config` o que un nombre no coincide. Los nombres que el
   código espera son exactamente `ReporTarea`, `ArandaDb` y `Sap`.

---

## 6. Rotar contraseñas

Las claves que estuvieron en el repositorio público siguen siendo recuperables
del historial de git. Hay que rotarlas; mientras sigan siendo válidas, la
exposición continúa.

Al rotarlas ya no se recompila nada: editas `connections.config` en cada
servidor y reinicias el sitio. Eso es todo.

---

## 7. Qué NO subir nunca

- `connections.config` (contraseñas reales)
- Cadenas de conexión escritas dentro de archivos `.cs`, ni siquiera comentadas
  — así fue como se filtraron las anteriores

El `.gitignore` cubre el primer caso. El segundo depende de quien revisa.
