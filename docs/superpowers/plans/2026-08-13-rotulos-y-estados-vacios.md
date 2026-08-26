# Rótulos completos y estado vacío — plan de implementación

> **Para agentes:** SUB-SKILL REQUERIDA: usar `superpowers:subagent-driven-development` (recomendado) o `superpowers:executing-plans` para ejecutar tarea por tarea. Los pasos usan casillas (`- [ ]`).

**Meta:** Que los rótulos se lean completos en las 6 pantallas que abrevian, y que una consulta sin resultados muestre un bloque con jerarquía en vez de texto pelado.

**Arquitectura:** Un archivo nuevo `js/dos-vacio.js`, cargado desde `Master.Master`, concentra el marcado y los textos. Los ~98 sitios de llamada pasan a invocarlo. El CSS va al final de `dos-tema.css` usando solo tokens existentes.

**Stack:** ASP.NET WebForms (.NET 4), jQuery 1.9.1, Bootstrap 3, Font Awesome 4.2.0, `dos-tema.css`.

**Spec:** `docs/superpowers/specs/2026-08-12-rotulos-y-estados-vacios-design.md`

## Restricciones globales

- **Los colores no se tocan.** Ni valores nuevos ni cambios a los existentes. Todo color nuevo debe salir de un token ya definido en `dos-tema.css` (`--crm-tinta-media`, `--crm-tinta-suave`, `--crm-linea`). La cabecera del diálogo de descarga conserva su `#f2dede`.
- **Ningún cambio de marcado en las pantallas** salvo los rótulos de la Tarea 5. El patrón se inyecta desde JS.
- **Cachés:** todo archivo que cambie y se enlace con `?v=` sube de versión. `dos-tema.css` va de `?v=9` a `?v=10` en `Master.Master` y en `Login.aspx`.
- **Despliegue:** todo archivo nuevo bajo `js/` o `css/` debe agregarse al `.csproj` como `<Content Include>`, o no viaja en la publicación.
- **Escapado:** el helper escapa el texto que recibe. Los mensajes pueden venir del servidor.
- No hay framework de pruebas. La verificación es una página de reproducción servida localmente, igual que en los dos trabajos anteriores de esta rama.

### Codificación y finales de línea — leer antes de escribir cualquier script

Medido sobre los 65 js y los 7 aspx involucrados:

| Qué | Estado |
|---|---|
| BOM en los js | **56 de 65 lo traen**, 9 no |
| BOM en los 7 aspx | los 7 lo traen |
| UTF-8 válido | los 72, sin excepción |
| Finales de línea en los js | 51 solo CRLF, 14 solo LF, **ninguno mixto** |

De ahí tres reglas para todo script que reescriba archivos:

1. **Leer y escribir con `encoding="utf-8"`, nunca `utf-8-sig`.** Con `utf-8` el BOM viaja como el carácter `\ufeff` al principio de la cadena y se vuelve a escribir igual. Con `utf-8-sig` al leer y `utf-8` al escribir, el BOM desaparece de 56 archivos.
2. **Pasar `newline=""` al abrir, tanto al leer como al escribir.** Sin eso Python traduce los finales de línea al escribir y los 14 archivos LF se convierten en CRLF: un cambio de una línea se vuelve un diff de archivo completo.
3. **No usar `errors="replace"` en un script que reescribe.** Los 72 archivos son UTF-8 válido, así que no hace falta, y si algún día uno no lo fuera reemplazaría el byte por U+FFFD y corrompería el archivo al guardarlo.

Después de cada reemplazo masivo, confirmar que el diff toca solo las líneas esperadas:

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas"
git diff --stat -- ReporteTareas/js/
```

Esperado: la suma de líneas cambiadas coincide con la cantidad de sitios reemplazados. Si algún archivo aparece con el total de sus líneas modificadas, se le cambiaron los finales de línea: revertirlo con `git checkout --` y revisar el script.

---

### Task 1: El patrón — helper, CSS y registro

**Archivos:**
- Crear: `ReporteTareas/js/dos-vacio.js`
- Crear: `ReporteTareas/_tmp-vacio.html` (temporal, se borra en el paso 8)
- Modificar: `ReporteTareas/css/dos-tema.css` (agregar bloque al final)
- Modificar: `ReporteTareas/Formulario/Master.Master` (etiqueta script + `?v=10`)
- Modificar: `ReporteTareas/Formulario/Login.aspx` (`?v=10`)
- Modificar: `ReporteTareas/ReporteTareas.csproj` (`Content Include`)

**Interfaces:**
- Consume: nada.
- Produce: `DosVacio(mensaje, pista)` → string con el marcado del bloque. `DosTextoDescargaVacia()` → string con el texto del diálogo. `DosEscapar(t)` → string escapado. Las tareas 2, 3 y 4 llaman a las dos primeras.

- [ ] **Paso 1: Crear el helper**

Crear `ReporteTareas/js/dos-vacio.js`:

```js
/* ============================================================================
   Estado vacio
   ============================================================================
   Lo que se pinta cuando una consulta no trae filas. Antes era una cadena
   suelta inyectada en el contenedor; ese es justo el momento en que alguien
   piensa que la aplicacion se rompio.

   Vive aca y no en cada pantalla porque CargarPagina esta duplicada en 50
   archivos y DetalleTareasDescargaXLS en 30. Copiar el marcado en cada sitio
   repetiria el error que produjo esas duplicaciones: cambiar la redaccion
   serian cien ediciones en vez de una.

   Master.Master lo carga para las 75 pantallas. Las seis que no usan la master
   (Login, PaginaError, Plantilla, PrubaWebServices, ResetPassword,
   RespuestaAprobacion) son auxiliares y no tienen tablas.
   ========================================================================== */

/* Escapa el texto antes de meterlo en el marcado. Los mensajes pueden venir
   del servidor, asi que no se concatenan crudos. */
function DosEscapar(t) {
    return String(t)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;");
}

/* Devuelve el marcado del bloque.

   Los dos argumentos son opcionales. CargarPagina no sabe que esta cargando
   —recibe un selector y una URL, no un concepto— asi que el texto por defecto
   es generico a proposito. Una pantalla concreta puede afinarlo pasando los
   suyos, sin tocar el patron.

   Pasar "" como pista la omite del marcado. */
function DosVacio(mensaje, pista) {
    var m = mensaje || "No hay resultados para lo que buscaste.";
    var p = arguments.length > 1
        ? pista
        : "Prueba con otro rango de fechas o cambia los filtros.";

    var html = '<div class="dos-vacio">' +
               '<i class="fa fa-inbox dos-vacio__icono" aria-hidden="true"></i>' +
               '<p class="dos-vacio__mensaje">' + DosEscapar(m) + '</p>';

    if (p) {
        html += '<p class="dos-vacio__pista">' + DosEscapar(p) + '</p>';
    }

    return html + '</div>';
}

/* El texto del dialogo de "descargar Excel" cuando no hay nada que exportar.
   Es un dialogo y no un estado vacio: responde a una accion explicita del
   usuario. Por eso conserva su forma; lo que cambia es que hable de la
   descarga y no de una consulta. */
function DosTextoDescargaVacia() {
    return "No hay datos que descargar con los filtros elegidos.";
}
```

- [ ] **Paso 2: Agregar el CSS**

Agregar al final de `ReporteTareas/css/dos-tema.css`:

```css

/* ----------------------------------------------------------------------------
   Estado vacio
   ----------------------------------------------------------------------------
   El marcado lo genera js/dos-vacio.js. Los selectores de aca y las clases de
   alla tienen que moverse juntos.

   Sin borde ni fondo a proposito: esto vive dentro de una tarjeta blanca que ya
   los tiene. Agregarlos dibujaria una caja dentro de otra.

   Todos los colores salen de tokens existentes. No se introduce ninguno nuevo.
   -------------------------------------------------------------------------- */

.dos-vacio {
    padding: 48px 24px;
    text-align: center;
    font-family: var(--crm-texto);
}

.dos-vacio__icono {
    display: block;
    margin-bottom: 16px;
    font-size: 38px;
    line-height: 1;
    color: var(--crm-linea);
}

.dos-vacio__mensaje {
    margin: 0;
    font-size: 15px;
    color: var(--crm-tinta-media);
}

.dos-vacio__pista {
    margin: 6px 0 0;
    font-size: 13px;
    color: var(--crm-tinta-suave);
}
```

- [ ] **Paso 3: Registrar el script en Master.Master**

En `ReporteTareas/Formulario/Master.Master`, justo DESPUÉS de la etiqueta `<script src="../js/jquery.blockUI.js"></script>` y su bloque inline, agregar:

```html
    <script src="../js/dos-vacio.js?v=1"></script>
```

Va al pie junto al resto porque solo se invoca desde callbacks de AJAX, nunca durante el parseo.

- [ ] **Paso 4: Subir la versión del CSS**

En `ReporteTareas/Formulario/Master.Master` y en `ReporteTareas/Formulario/Login.aspx`, cambiar `dos-tema.css?v=9` por `dos-tema.css?v=10`.

- [ ] **Paso 5: Agregar el js al csproj**

En `ReporteTareas/ReporteTareas.csproj`, junto a las demás entradas de `js\`:

```xml
    <Content Include="js\dos-vacio.js" />
```

- [ ] **Paso 6: Escribir la página de reproducción**

Crear `ReporteTareas/_tmp-vacio.html`:

```html
<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<title>Prueba del estado vacio</title>
<link href="bower_components/bootstrap/dist/css/bootstrap.min.css" rel="stylesheet" />
<link href="dist/css/sb-admin-2.css" rel="stylesheet" />
<link href="bower_components/font-awesome/css/font-awesome.min.css" rel="stylesheet" />
<link href="css/dos-tema.css?v=10" rel="stylesheet" />
</head>
<body class="app-dos">
<div id="page-wrapper" style="padding:0">
    <div class="col-lg-12" style="padding:0">
        <div class="panel panel-default">
            <div class="panel-heading">Por defecto</div>
            <div class="panel-body" id="caso1"></div>
        </div>
        <div class="panel panel-default">
            <div class="panel-heading">Mensaje propio, sin pista</div>
            <div class="panel-body" id="caso2"></div>
        </div>
        <div class="panel panel-default">
            <div class="panel-heading">Comprobaciones</div>
            <div class="panel-body"><pre id="salida"></pre></div>
        </div>
    </div>
</div>
<script src="bower_components/jquery/dist/jquery.min.js"></script>
<script src="js/dos-vacio.js?v=1"></script>
<script>
    $("#caso1").html(DosVacio());
    $("#caso2").html(DosVacio("Este empleado no tiene tareas registradas.", ""));

    var r = [];
    function comprobar(nombre, condicion) {
        r.push((condicion ? "OK   " : "FALLA ") + nombre);
    }

    comprobar("por defecto trae el mensaje generico",
        DosVacio().indexOf("No hay resultados para lo que buscaste.") > -1);
    comprobar("por defecto trae la pista",
        DosVacio().indexOf("Prueba con otro rango de fechas") > -1);
    comprobar("respeta el mensaje propio",
        DosVacio("hola").indexOf("hola") > -1);
    comprobar("con un solo argumento conserva la pista",
        DosVacio("hola").indexOf("dos-vacio__pista") > -1);
    comprobar("pista vacia se omite",
        DosVacio("hola", "").indexOf("dos-vacio__pista") === -1);
    comprobar("escapa el marcado",
        DosVacio("<img src=x onerror=alert(1)>").indexOf("<img") === -1);
    comprobar("texto de descarga habla de descarga",
        DosTextoDescargaVacia().indexOf("descargar") > -1);

    $("#salida").text(r.join("\n"));
</script>
</body>
</html>
```

- [ ] **Paso 7: Servir y verificar**

Levantar un servidor estático sobre `ReporteTareas/`:

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas/ReporteTareas"
python -m http.server 8791
```

Abrir `http://127.0.0.1:8791/_tmp-vacio.html`.

Esperado: las 7 comprobaciones en `OK`. Los dos casos muestran el bloque centrado, con el ícono tenue arriba. El segundo caso no muestra la línea de pista.

Comprobar además que no se introdujo ningún color:

```bash
git diff -- ReporteTareas/css/dos-tema.css | grep -E "^\+" | grep -iE "#[0-9a-f]{3,6}|rgb\(|rgba\("
```

Esperado: sin salida.

- [ ] **Paso 8: Limpiar y commitear**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas"
rm -f ReporteTareas/_tmp-vacio.html
git add ReporteTareas/js/dos-vacio.js ReporteTareas/css/dos-tema.css \
        ReporteTareas/Formulario/Master.Master ReporteTareas/Formulario/Login.aspx \
        ReporteTareas/ReporteTareas.csproj
git commit -m "feat(ui): patron de estado vacio"
```

---

### Task 2: Grupo A — los 54 sitios que ya escriben en el contenedor

**Archivos:**
- Modificar: los archivos de `ReporteTareas/js/` que contienen `$(div).html("No existen datos para esta consulta.");`
- Crear: script de reemplazo en el scratchpad (no se commitea)

**Interfaces:**
- Consume: `DosVacio()` de la Tarea 1.
- Produce: nada que otra tarea use.

- [ ] **Paso 1: Contar antes**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas/ReporteTareas"
grep -rc '\$(div)\.html("No existen datos para esta consulta\.");' js/*.js | grep -v ':0' | awk -F: '{s+=$2} END {print "sitios:", s}'
```

Esperado: `sitios: 54`.

- [ ] **Paso 2: Reemplazar**

Escribir en el scratchpad `reemplazo_grupo_a.py`:

```python
import os

JS = r"C:\respaldodisco\Desarrollo\PRY_Sistema ReporteTareas\ReporteTareas\js"
VIEJO = '$(div).html("No existen datos para esta consulta.");'
NUEVO = '$(div).html(DosVacio());'

# newline="" y encoding="utf-8" a proposito: preservan los finales de linea
# originales y el BOM que traen 56 de los 65 archivos. Ver las reglas de
# codificacion en las restricciones globales.
total = 0
for nombre in sorted(os.listdir(JS)):
    if not nombre.endswith(".js") or ".min." in nombre:
        continue
    ruta = os.path.join(JS, nombre)
    with open(ruta, encoding="utf-8", newline="") as fh:
        txt = fh.read()
    if VIEJO not in txt:
        continue

    # splitlines(keepends=True) conserva el \r\n o \n de cada linea tal cual
    salida = []
    cambios = 0
    for l in txt.splitlines(keepends=True):
        if VIEJO in l and not l.lstrip().startswith("//"):   # no tocar comentadas
            l = l.replace(VIEJO, NUEVO)
            cambios += 1
        salida.append(l)

    if cambios:
        with open(ruta, "w", encoding="utf-8", newline="") as fh:
            fh.write("".join(salida))
        print("%-38s %d" % (nombre, cambios))
        total += cambios
print("TOTAL:", total)
```

Ejecutarlo. Esperado: `TOTAL: 54`.

- [ ] **Paso 2b: Confirmar que el diff toca solo esas líneas**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas"
git diff --stat -- ReporteTareas/js/ | tail -1
```

Esperado: 54 inserciones y 54 supresiones. Si algún archivo aparece con todas sus líneas cambiadas, se le tocaron los finales de línea: `git checkout -- ReporteTareas/js/` y revisar el script.

- [ ] **Paso 3: Verificar que no quedó ninguno vivo**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas/ReporteTareas"
grep -rn '\$(div)\.html("No existen datos' js/*.js | grep -v '//'
```

Esperado: sin salida. Las dos líneas comentadas siguen ahí y está bien: no se ejecutan.

- [ ] **Paso 4: Verificar sintaxis de todos los archivos tocados**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas/ReporteTareas"
for f in $(git diff --name-only -- js/); do node --check "../$f" || echo "ROTO: $f"; done
```

Esperado: sin líneas `ROTO:`.

- [ ] **Paso 5: Commitear**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas"
git add ReporteTareas/js/
git commit -m "feat(ui): estado vacio en los 54 sitios de CargarPagina"
```

---

### Task 3: Grupo A bis — los 13 que escriben en la franja de estado

**Archivos:**
- Modificar: los archivos de `ReporteTareas/js/` con `$("#divMensajes").html("No existen datos para esta consulta.");`

**Interfaces:**
- Consume: `DosVacio()` de la Tarea 1.
- Produce: nada.

**Contexto:** `#divMensajes` es una franja de estado (`col-md-6 col-md-offset-3`) donde se pinta "Cargando Información..." y después se limpia. El código original manda ahí el mensaje vacío mientras los datos van a `$(div)` — una inconsistencia del original, no del cambio. Estos 13 deben ir al mismo contenedor que los otros 54.

- [ ] **Paso 1: Listar los sitios con su contexto**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas/ReporteTareas"
grep -rn '\$("#divMensajes")\.html("No existen datos' js/*.js
```

Esperado: 13 líneas.

- [ ] **Paso 2: Confirmar que `div` está en el alcance de cada uno**

Para CADA archivo de la lista, abrir la línea y subir hasta la declaración de la función que la contiene. Confirmar que `div` es uno de sus parámetros.

Si `div` NO es parámetro de la función contenedora, **dejar ese sitio sin tocar** y anotarlo para el informe final. No inventar un contenedor.

- [ ] **Paso 3: Reemplazar solo los confirmados**

En cada sitio confirmado, cambiar:

```js
$("#divMensajes").html("No existen datos para esta consulta.");
```

por:

```js
$(div).html(DosVacio());
```

- [ ] **Paso 4: Verificar**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas/ReporteTareas"
echo "quedan sin tocar:"; grep -rc '\$("#divMensajes")\.html("No existen datos' js/*.js | grep -v ':0'
for f in $(git diff --name-only -- js/); do node --check "../$f" || echo "ROTO: $f"; done
```

Esperado: solo aparecen los que se dejaron a propósito por el paso 2, y ninguna línea `ROTO:`.

- [ ] **Paso 5: Commitear**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas"
git add ReporteTareas/js/
git commit -m "feat(ui): el mensaje de vacio deja la franja de estado y va al contenedor"
```

---

### Task 4: Grupo B — el diálogo de descarga de Excel

**Archivos:**
- Modificar: los 30 archivos de `ReporteTareas/js/` con `var mesnajeError = "No existen datos para esta consulta.";`

**Interfaces:**
- Consume: `DosTextoDescargaVacia()` de la Tarea 1.
- Produce: nada.

**Contexto:** los 31 sitios están todos dentro de `DetalleTareasDescargaXLS`. No son estados vacíos: el usuario hizo clic en "descargar Excel" y no hay nada que exportar. El diálogo se queda y **la cabecera conserva su `#f2dede`**. Solo cambia el texto.

- [ ] **Paso 1: Contar antes**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas/ReporteTareas"
grep -rc 'var mesnajeError = "No existen datos para esta consulta\.";' js/*.js | grep -v ':0' | awk -F: '{s+=$2} END {print "sitios:", s}'
```

Esperado: `sitios: 31`.

- [ ] **Paso 2: Reemplazar**

Escribir en el scratchpad `reemplazo_grupo_b.py`:

```python
import os

JS = r"C:\respaldodisco\Desarrollo\PRY_Sistema ReporteTareas\ReporteTareas\js"
VIEJO = 'var mesnajeError = "No existen datos para esta consulta.";'
NUEVO = 'var mesnajeError = DosTextoDescargaVacia();'

# Mismas reglas de codificacion que el grupo A: newline="" y utf-8, para no
# tocar finales de linea ni perder el BOM.
total = 0
for nombre in sorted(os.listdir(JS)):
    if not nombre.endswith(".js") or ".min." in nombre:
        continue
    ruta = os.path.join(JS, nombre)
    with open(ruta, encoding="utf-8", newline="") as fh:
        txt = fh.read()
    n = txt.count(VIEJO)
    if n:
        with open(ruta, "w", encoding="utf-8", newline="") as fh:
            fh.write(txt.replace(VIEJO, NUEVO))
        print("%-38s %d" % (nombre, n))
        total += n
print("TOTAL:", total)
```

Ejecutarlo. Esperado: `TOTAL: 31`.

- [ ] **Paso 2b: Confirmar que el diff toca solo esas líneas**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas"
git diff --stat -- ReporteTareas/js/ | tail -1
```

Esperado: 31 inserciones y 31 supresiones.

- [ ] **Paso 3: Verificar que la cabecera roja NO cambió**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas"
git diff -- ReporteTareas/js/ | grep -E "^[+-]" | grep -i "f2dede"
```

Esperado: **sin salida**. Si aparece algo, se tocó un color y hay que revertirlo.

- [ ] **Paso 4: Verificar sintaxis**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas/ReporteTareas"
for f in $(git diff --name-only -- js/); do node --check "../$f" || echo "ROTO: $f"; done
```

Esperado: sin líneas `ROTO:`.

- [ ] **Paso 5: Commitear**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas"
git add ReporteTareas/js/
git commit -m "fix(ui): el dialogo de descarga habla de la descarga, no de una consulta"
```

---

### Task 5: Los rótulos

**Archivos:**
- Modificar: `ReporteTareas/Formulario/Tareas.aspx` (17)
- Modificar: `ReporteTareas/Formulario/AprobarHorasExtras.aspx` (15)
- Modificar: `ReporteTareas/Formulario/ReporteHorasExtras.aspx` (4)
- Modificar: `ReporteTareas/Formulario/TiempoTarea.aspx` (4)
- Modificar: `ReporteTareas/Formulario/CreaTarea.aspx` (1)
- Modificar: `ReporteTareas/Formulario/CreaTareas.aspx` (1)
- Modificar: `ReporteTareas/Formulario/RegistroInventario.aspx` (1)

**Interfaces:**
- Consume: nada.
- Produce: nada.

- [ ] **Paso 1: Contar antes**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas/ReporteTareas/Formulario"
grep -ohE '(Text|placeholder)="[^"]*(Nom\.|Fch\.|Id\.|N°|Cod\.|Num\.)[^"]*"' *.aspx | wc -l
```

Esperado: `43`.

- [ ] **Paso 2: Aplicar la tabla de reemplazos**

Son 43 reemplazos en 7 archivos: **hacerlos con ediciones puntuales, no con un script.** Los siete aspx traen BOM y el volumen no justifica el riesgo de reescribir el archivo entero. Una edición por rótulo, verificando el contexto de cada una.

En los siete archivos, reemplazar el VALOR de los atributos `Text=` y `placeholder=` según esta tabla. Cambiar solo el texto: no tocar `ID=`, `runat`, ni ningún otro atributo.

| Hoy | Propuesto |
|---|---|
| `Nom. Cliente` | `Cliente` |
| `Nom. Responsable` | `Responsable` |
| `Id. Responsable` | `Código del responsable` |
| `Fch. Registro` | `Fecha de registro` |
| `Fch. Tarea` | `Fecha de la tarea` |
| `Fch. Inicio` | `Inicio` |
| `Fch. Fin` | `Fin` |
| `Fch. Est. Solucion` | `Solución estimada` |
| `Fch. Est. Atencion` | `Atención estimada` |
| `N° OS` | `N° de orden de servicio` |
| `Num. OS` | `N° de orden de servicio` |
| `N° Orden de Servicio` | `N° de orden de servicio` |
| `Cod. Ticket` | `N° de ticket` |
| `N° Ticket` | `N° de ticket` |
| `N° PARTE` | `N° de parte` |

Cuidado con el orden: reemplazar `Fch. Est. Solucion` y `Fch. Est. Atencion` ANTES que cualquier regla sobre `Fch.`, o quedarán a medias.

- [ ] **Paso 3: Verificar que no sobrevive ninguna abreviatura**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas/ReporteTareas/Formulario"
grep -nE '(Text|placeholder)="[^"]*(Nom\.|Fch\.|Id\.|Cod\.|Num\.)[^"]*"' *.aspx
```

Esperado: sin salida.

- [ ] **Paso 4: Verificar que no se rompió ningún atributo**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas"
git diff -- ReporteTareas/Formulario/ | grep -E "^[+-]" | grep -vE 'Text="|placeholder="|^[+-]{3}'
```

Esperado: **sin salida**. Si sale otra cosa, se tocó algo que no era un rótulo.

- [ ] **Paso 5: Compilar**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas"
"/c/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/MSBuild.exe" \
  "ReporteTareas/ReporteTareas.csproj" //p:Configuration=Debug //p:VisualStudioVersion=16.0 //v:minimal //nologo | tail -3
```

Esperado: termina con `EXIT=0` y sin líneas `error`.

- [ ] **Paso 6: Commitear**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas"
git add ReporteTareas/Formulario/
git commit -m "feat(ui): rotulos completos en las seis pantallas que abreviaban"
```

---

### Task 6: Publicación

**Archivos:**
- Modificar: `ReporteTareas/obj/Release/Package/PackageTmp/**` (copias sincronizadas)
- Crear: carpeta de subida con lo que cambió

**Interfaces:**
- Consume: el resultado de las tareas 1 a 5.
- Produce: la carpeta de subida.

- [ ] **Paso 1: Publicar en Release a una carpeta nueva**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas"
"/c/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/MSBuild.exe" \
  "ReporteTareas/ReporteTareas.csproj" //p:Configuration=Release //p:VisualStudioVersion=16.0 \
  //p:DeployOnBuild=true //p:PublishProfile=FolderProfile \
  //p:publishUrl="C:\respaldodisco\Desarrollo\Publicar_rotulos" //v:minimal //nologo | tail -3
```

No publicar sobre `C:\respaldodisco\Desarrollo\Publicar`: el perfil trae `DeleteExistingFiles=True` y esa carpeta puede llevar algo pendiente.

- [ ] **Paso 2: Confirmar que el js nuevo viajó**

```bash
ls -l "C:/respaldodisco/Desarrollo/Publicar_rotulos/js/dos-vacio.js"
```

Esperado: el archivo existe. Si no, falta la entrada en el `.csproj` de la Tarea 1 paso 5.

- [ ] **Paso 3: Armar la carpeta de subida**

Copiar desde `Publicar_rotulos` a `C:\respaldodisco\Desarrollo\Subir_rotulos\sitio\`, respetando la estructura:

- `css/dos-tema.css`
- `js/dos-vacio.js`
- `js/` — todos los archivos que aparezcan en `git diff --name-only <commit-inicial>..HEAD -- ReporteTareas/js/`
- `Formulario/Master.Master`
- `Formulario/Login.aspx`
- `Formulario/` — las siete pantallas de la Tarea 5

- [ ] **Paso 4: Sincronizar las copias del paquete**

Para cada archivo de la lista anterior, copiar desde `ReporteTareas/<ruta>` a `ReporteTareas/obj/Release/Package/PackageTmp/<ruta>` y verificar con `diff -q` que quedan idénticos.

- [ ] **Paso 5: Commitear**

```bash
cd "C:/respaldodisco/Desarrollo/PRY_Sistema ReporteTareas"
git add ReporteTareas/obj/Release/Package/PackageTmp/
git commit -m "build: paquete con el estado vacio y los rotulos"
```

---

## Verificación final

- [ ] `grep -rn "No existen datos para esta consulta" ReporteTareas/js/*.js` devuelve solo las dos líneas comentadas y los sitios que la Tarea 3 paso 2 dejó a propósito.
- [ ] `git diff <commit-inicial>..HEAD -- ReporteTareas/css/ ReporteTareas/js/` no introduce ningún literal de color.
- [ ] Ninguna abreviatura sobrevive en las siete pantallas.
- [ ] La compilación termina en `EXIT=0`.
- [ ] `dos-tema.css` está enlazado como `?v=10` en `Master.Master` y en `Login.aspx`.
- [ ] `js\dos-vacio.js` figura en el `.csproj` y aparece en la carpeta publicada.
