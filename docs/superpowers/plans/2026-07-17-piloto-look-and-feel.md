# Piloto Look-and-Feel — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Aplicar una capa de tema CSS moderna (rojo DOS) sobre el sistema WebForms, rediseñar el chrome (navbar + sidebar con buscador) y limpiar 3 pantallas piloto, sin tocar la lógica construida.

**Architecture:** Un único `tema-dos.css` con variables `:root` se carga como último `<link>` y pisa a Bootstrap 3 / sb-admin-2 por cascada. El chrome se estiliza vía CSS + un JS nuevo de buscador de menú. El markup de presentación de 3 pantallas se limpia (se quitan `<font>` e inline styles) manteniendo intactos todos los controles `asp:`, sus IDs, code-behind y stored procedures. La revisión se hace con un preview estático (`preview/tema-preview.html`) que no requiere compilar el proyecto .NET.

**Tech Stack:** ASP.NET WebForms (.aspx), Bootstrap 3, sb-admin-2, metisMenu, jQuery 1.9.1, Font Awesome 4, CSS custom properties, JS vanilla.

## Global Constraints

- **NO tocar lógica:** `CapaNegocio`, `CapaDato`, `CapaEntidad`, stored procedures, ningún `Page_Load`/evento de servidor (única excepción: la plantilla HTML del menú en `Master.master.cs`), ni IDs de controles `asp:`.
- **Reversibilidad:** comentar el `<link>` de `tema-dos.css` debe devolver el sistema al estado actual. No borrar CSS existente; solo pisarlo por cascada.
- **Identidad:** rojo DOS modernizado como color de marca. Primario inicial `#A32020` (se afina en el preview). Neutros grises fríos. Todo color vía variable CSS — nada hardcodeado nuevo.
- **`tema-dos.css` es el último `<link>` del `<head>`** en todo archivo donde se cargue.
- **Los valores exactos de color/espaciado/sombra son punto de partida** y se ajustan durante la revisión del preview; no son placeholders, son defaults tuneables.
- **Idioma:** todo texto visible en español, respetando los textos actuales.
- Commits frecuentes, uno por tarea.

---

## File Structure

- **Create** `preview/tema-preview.html` — arnés de revisión estático (chrome + form + tabla de ejemplo). No forma parte del deploy.
- **Create** `ReporteTareas/dist/css/tema-dos.css` — toda la capa de tema (tokens + componentes + chrome).
- **Create** `ReporteTareas/js/menuBuscador.js` — buscador de menú lateral + estado activo.
- **Modify** `ReporteTareas/Formulario/Master.Master` — cargar tema/JS, input de buscador en sidebar.
- **Modify** `ReporteTareas/Formulario/Master.master.cs` — solo la plantilla HTML que emite el menú (clases/atributos), no la fuente de datos.
- **Modify** `ReporteTareas/Formulario/Login.aspx` — cargar tema, quitar `<font>`/inline.
- **Modify** `ReporteTareas/Formulario/Principal.aspx` — quitar `<font>`/inline, markup responsive.
- **Modify** `ReporteTareas/Formulario/ParametrizacionHorarioUsuario.aspx` — unificar `card`/`panel`, ordenar form/tabla/modales.
- **Modify** `ReporteTareas/Formulario/ReporteHorasExtras.aspx` — limpiar cabecera/filtros/GridView.

Nota: rutas relativas. Desde `Formulario/*.aspx` el tema es `../dist/css/tema-dos.css` y el JS `../js/menuBuscador.js`.

---

### Task 1: Tokens de diseño + preview base

**Files:**
- Create: `ReporteTareas/dist/css/tema-dos.css`
- Create: `preview/tema-preview.html`

**Interfaces:**
- Produces: variables CSS en `:root` consumidas por todas las tareas siguientes: `--dos-primary`, `--dos-primary-600`, `--dos-primary-700`, `--dos-primary-050`, `--dos-ink`, `--dos-muted`, `--dos-bg`, `--dos-surface`, `--dos-border`, `--dos-success`, `--dos-danger`, `--dos-warning`, `--dos-radius`, `--dos-shadow-sm`, `--dos-shadow-md`, `--dos-space`, `--dos-font`.

- [ ] **Step 1: Crear `tema-dos.css` con el bloque de tokens y base tipográfica**

```css
/* tema-dos.css — capa de tema DOS. Se carga como ÚLTIMO <link>, pisa Bootstrap 3.
   Reversible: quitar el <link> devuelve el sistema al estado anterior. */
:root {
    --dos-primary: #A32020;
    --dos-primary-600: #8B1A1A;
    --dos-primary-700: #6F1414;
    --dos-primary-050: #FBEDED;
    --dos-ink: #1F2937;
    --dos-muted: #6B7280;
    --dos-bg: #F3F4F6;
    --dos-surface: #FFFFFF;
    --dos-border: #E5E7EB;
    --dos-success: #157347;
    --dos-danger: #B02A37;
    --dos-warning: #B88600;
    --dos-radius: 10px;
    --dos-shadow-sm: 0 1px 2px rgba(16,24,40,.06), 0 1px 3px rgba(16,24,40,.10);
    --dos-shadow-md: 0 4px 12px rgba(16,24,40,.10);
    --dos-space: 16px;
    --dos-font: "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
}

body {
    font-family: var(--dos-font);
    color: var(--dos-ink);
    background: var(--dos-bg);
    font-size: 14px;
    line-height: 1.5;
}

h1, h2, h3, h4, h5 { font-family: var(--dos-font); font-weight: 600; color: var(--dos-ink); }

/* Neutraliza <font> heredado sin tener que borrarlo del markup viejo */
font { font-size: inherit !important; color: inherit !important; font-family: inherit !important; }
```

- [ ] **Step 2: Crear el arnés de preview**

Crear `preview/tema-preview.html`. Carga Bootstrap 3 y el tema por ruta relativa al repo, con secciones vacías que las tareas siguientes irán llenando.

```html
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Preview Tema DOS</title>
    <link href="../ReporteTareas/bower_components/bootstrap/dist/css/bootstrap.min.css" rel="stylesheet">
    <link href="../ReporteTareas/bower_components/font-awesome/css/font-awesome.min.css" rel="stylesheet">
    <link href="../ReporteTareas/dist/css/sb-admin-2.css" rel="stylesheet">
    <!-- ÚLTIMO: el tema pisa a los anteriores -->
    <link href="../ReporteTareas/dist/css/tema-dos.css" rel="stylesheet">
</head>
<body>
    <div class="container-fluid" style="padding:24px">
        <h1>Preview Tema DOS</h1>
        <p class="text-muted">Arnés de revisión. No forma parte del deploy.</p>
        <section id="preview-tipografia">
            <h2>Tipografía</h2>
            <h1>Encabezado H1</h1><h2>Encabezado H2</h2><h3>Encabezado H3</h3>
            <p>Párrafo de ejemplo con <a href="#">un enlace</a> y texto <b>en negrita</b>.</p>
        </section>
        <hr>
        <!-- Task 2 añade componentes aquí -->
        <!-- Task 3 añade chrome aquí -->
    </div>
</body>
</html>
```

- [ ] **Step 3: Verificar en el navegador**

Abrir `preview/tema-preview.html` en el navegador.
Esperado: el fondo es gris claro (`--dos-bg`), la tipografía es Segoe UI/sistema, los encabezados se ven en gris oscuro. No hay errores en consola (F12).

- [ ] **Step 4: Commit**

```bash
git add ReporteTareas/dist/css/tema-dos.css preview/tema-preview.html
git commit -m "feat(tema): tokens de diseno DOS y arnes de preview estatico"
```

---

### Task 2: Componentes (botones, formularios, cards/panels, tablas, modales)

**Files:**
- Modify: `ReporteTareas/dist/css/tema-dos.css`
- Modify: `preview/tema-preview.html`

**Interfaces:**
- Consumes: tokens `:root` de Task 1.
- Produces: reglas de tema para las clases Bootstrap 3 existentes (`.btn`, `.btn-primary`, `.btn-success`, `.btn-danger`, `.btn-default`, `.form-control`, `.panel`, `.card`, `.table`, `.modal-content`). Sin clases nuevas: se pisan las existentes para no tocar markup.

- [ ] **Step 1: Añadir reglas de componentes a `tema-dos.css`**

Agregar al final del archivo. Estas reglas pisan a Bootstrap 3 por orden de carga.

```css
/* ---- Botones ---- */
.btn { border-radius: var(--dos-radius); font-weight: 600; padding: 6px 16px; border: 1px solid transparent; transition: background-color .15s, box-shadow .15s; }
.btn-primary { background: var(--dos-primary); border-color: var(--dos-primary); }
.btn-primary:hover, .btn-primary:focus, .btn-primary:active { background: var(--dos-primary-600); border-color: var(--dos-primary-700); }
.btn-success { background: var(--dos-success); border-color: var(--dos-success); }
.btn-danger  { background: var(--dos-danger);  border-color: var(--dos-danger); }
.btn-default { background: var(--dos-surface); border-color: var(--dos-border); color: var(--dos-ink); }
.btn-default:hover { background: var(--dos-bg); }

/* ---- Formularios ---- */
.form-control { border-radius: var(--dos-radius); border: 1px solid var(--dos-border); box-shadow: none; height: 38px; }
.form-control:focus { border-color: var(--dos-primary); box-shadow: 0 0 0 3px var(--dos-primary-050); }
label, .control-label { font-weight: 600; color: var(--dos-ink); margin-bottom: 4px; }
.form-group { margin-bottom: var(--dos-space); }

/* ---- Cards / Panels (unifica ambos patrones) ---- */
.panel, .card { border: 1px solid var(--dos-border); border-radius: var(--dos-radius); box-shadow: var(--dos-shadow-sm); background: var(--dos-surface); }
.panel-heading, .card-header { background: var(--dos-surface); border-bottom: 1px solid var(--dos-border); border-top-left-radius: var(--dos-radius); border-top-right-radius: var(--dos-radius); font-weight: 600; color: var(--dos-ink); }
.panel-default > .panel-heading { color: var(--dos-ink); }
.card-primary > .card-header { background: var(--dos-primary); color: #fff; }
.panel-body, .card-body { padding: var(--dos-space); }

/* ---- Tablas ---- */
.table { background: var(--dos-surface); }
.table > thead > tr > th { background: var(--dos-bg); color: var(--dos-ink); border-bottom: 2px solid var(--dos-border); font-weight: 600; }
.table-striped > tbody > tr:nth-of-type(odd) { background: #FAFAFB; }
.table-hover > tbody > tr:hover { background: var(--dos-primary-050); }
.table > tbody > tr > td { vertical-align: middle; }

/* ---- Modales ---- */
.modal-content { border-radius: var(--dos-radius); box-shadow: var(--dos-shadow-md); border: none; }
.modal-header { border-bottom: 1px solid var(--dos-border); background: var(--dos-surface) !important; }
.modal-title { font-weight: 600; }
```

- [ ] **Step 2: Añadir muestras de componentes al preview**

En `preview/tema-preview.html`, reemplazar el comentario `<!-- Task 2 añade componentes aquí -->` por bloques que ejerciten cada componente: una fila de botones (`btn-primary`, `btn-success`, `btn-danger`, `btn-default`), un formulario con `form-group`/`form-control`/`label`, un `panel panel-default`, un `card card-primary`, una `table table-striped table-hover table-bordered` con 3-4 filas, y un modal estático (`.modal` con `style="display:block;position:static"`).

- [ ] **Step 3: Verificar en el navegador**

Recargar `preview/tema-preview.html`.
Esperado: botones con esquinas redondeadas y el primario en rojo DOS; inputs con foco rojo suave; panel y card con sombra sutil; tabla con encabezado gris, zebra y hover rojo claro; modal con esquinas redondeadas. Comparar visualmente que se ve moderno y consistente.

- [ ] **Step 4: Commit**

```bash
git add ReporteTareas/dist/css/tema-dos.css preview/tema-preview.html
git commit -m "feat(tema): estilos de botones, formularios, cards, tablas y modales"
```

---

### Task 3: Chrome — navbar + sidebar

**Files:**
- Modify: `ReporteTareas/dist/css/tema-dos.css`
- Modify: `preview/tema-preview.html`

**Interfaces:**
- Consumes: tokens `:root` de Task 1.
- Produces: reglas para `.navbar`, `.navbar-brand`, `.navbar-top-links`, `.sidebar`, `#side-menu`, `.nav-second-level`, y las clases nuevas del buscador `.dos-menu-search` / `.dos-menu-search input` (consumidas por Task 4/5), más `.active` para el ítem de menú vigente (consumida por Task 4).

- [ ] **Step 1: Añadir estilos de chrome a `tema-dos.css`**

```css
/* ---- Navbar ---- */
.navbar-default { background: var(--dos-surface); border: none; border-bottom: 1px solid var(--dos-border); box-shadow: var(--dos-shadow-sm); }
.navbar-brand { padding: 8px 16px; display: flex; align-items: center; }
.navbar-brand img { max-height: 34px; width: auto; }
.navbar-top-links > li > a { color: var(--dos-ink); }
.navbar .navbar-text { color: var(--dos-primary-600) !important; font-weight: 600; }

/* ---- Sidebar ---- */
.navbar-default.sidebar { background: var(--dos-primary-700); }
.sidebar .nav > li > a { color: rgba(255,255,255,.85); border-bottom: none; }
.sidebar .nav > li > a:hover, .sidebar .nav > li > a:focus { background: rgba(0,0,0,.18); color: #fff; }
.sidebar .nav .nav-second-level > li > a { color: rgba(255,255,255,.75); padding-left: 32px; }
.sidebar .nav > li.active > a,
.sidebar .nav .nav-second-level > li.active > a { background: var(--dos-primary); color: #fff; border-left: 3px solid #fff; font-weight: 600; }
.sidebar .arrow { color: rgba(255,255,255,.55); }

/* ---- Buscador de menú ---- */
.dos-menu-search { padding: 10px 12px; }
.dos-menu-search input { width: 100%; height: 34px; border-radius: var(--dos-radius); border: 1px solid rgba(255,255,255,.25); background: rgba(255,255,255,.12); color: #fff; padding: 4px 10px; }
.dos-menu-search input::placeholder { color: rgba(255,255,255,.65); }
.dos-menu-search input:focus { outline: none; border-color: #fff; background: rgba(255,255,255,.2); }
.dos-menu-hidden { display: none !important; }
```

- [ ] **Step 2: Añadir el chrome al preview**

En `preview/tema-preview.html`, reemplazar `<!-- Task 3 añade chrome aquí -->` por un `navbar navbar-default` con brand + `navbar-top-links` (botones Entrada/Salida, nombre de usuario), y un `div.navbar-default.sidebar` que contenga el `<div class="dos-menu-search"><input id="dos-menu-filtro" placeholder="Buscar en el menú..."></div>` seguido de un `<ul class="nav" id="side-menu">` con 2 padres y sub-ítems `nav-second-level`, marcando uno con `class="active"`. Estructura idéntica a la que genera `Master.master.cs` (Task 5) para que el preview sea fiel.

- [ ] **Step 3: Verificar en el navegador**

Recargar el preview y reducir el ancho de ventana a ~375px.
Esperado: sidebar rojo oscuro con texto blanco legible, el ítem `.active` resaltado con barra blanca a la izquierda, el input de búsqueda visible dentro del sidebar, navbar blanca con sombra y nombre de usuario en rojo. En 375px el layout no se rompe (Bootstrap colapsa el navbar).

- [ ] **Step 4: Commit**

```bash
git add ReporteTareas/dist/css/tema-dos.css preview/tema-preview.html
git commit -m "feat(tema): estilos de navbar y sidebar con estado activo y buscador"
```

---

### Task 4: JS del buscador de menú + estado activo

**Files:**
- Create: `ReporteTareas/js/menuBuscador.js`
- Modify: `preview/tema-preview.html`

**Interfaces:**
- Consumes: markup `#side-menu`, `#dos-menu-filtro`, clases `.nav-second-level`, `.dos-menu-hidden`, `.active` (Task 3).
- Produces: comportamiento cliente; no expone API a otras tareas. Se auto-inicializa en `DOMContentLoaded`.

- [ ] **Step 1: Escribir `menuBuscador.js`**

```javascript
// menuBuscador.js — filtra el menú lateral y marca el ítem de la página actual.
// No depende de jQuery. No toca datos ni seguridad; solo presentación cliente.
(function () {
    "use strict";

    function normaliza(txt) {
        // NFD + quitar marcas diacríticas (acentos) para búsqueda sin acento-sensibilidad
        return (txt || "").toLowerCase()
            .normalize("NFD").replace(/[̀-ͯ]/g, "");
    }

    function filtrar(termino) {
        var menu = document.getElementById("side-menu");
        if (!menu) return;
        var q = normaliza(termino);
        var padres = menu.querySelectorAll(":scope > li");
        padres.forEach(function (padre) {
            var enlaces = padre.querySelectorAll(".nav-second-level > li");
            var algunoVisible = false;
            enlaces.forEach(function (li) {
                var texto = normaliza(li.textContent);
                var coincide = q === "" || texto.indexOf(q) !== -1;
                li.classList.toggle("dos-menu-hidden", !coincide);
                if (coincide) algunoVisible = true;
            });
            // Si no hay hijos, evalúa el propio padre
            var textoPadre = normaliza(padre.textContent);
            var mostrarPadre = q === "" || algunoVisible || textoPadre.indexOf(q) !== -1;
            padre.classList.toggle("dos-menu-hidden", !mostrarPadre);
            if (q !== "" && algunoVisible) {
                var subUl = padre.querySelector(".nav-second-level");
                if (subUl) subUl.classList.add("in");
            }
        });
    }

    function marcarActivo() {
        var menu = document.getElementById("side-menu");
        if (!menu) return;
        var actual = window.location.pathname.split("/").pop().toLowerCase();
        if (!actual) return;
        var enlaces = menu.querySelectorAll("a[href]");
        enlaces.forEach(function (a) {
            var href = (a.getAttribute("href") || "").split("/").pop().toLowerCase();
            if (href && href === actual) {
                var li = a.closest("li");
                if (li) li.classList.add("active");
                var padreUl = a.closest(".nav-second-level");
                if (padreUl) {
                    padreUl.classList.add("in");
                    var liPadre = padreUl.closest("li");
                    if (liPadre) liPadre.classList.add("active");
                }
            }
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        var input = document.getElementById("dos-menu-filtro");
        if (input) {
            input.addEventListener("input", function () { filtrar(input.value); });
        }
        marcarActivo();
    });
})();
```

- [ ] **Step 2: Cargar el JS en el preview**

En `preview/tema-preview.html`, antes de `</body>`, añadir `<script src="../ReporteTareas/js/menuBuscador.js"></script>`.

- [ ] **Step 3: Verificar en el navegador**

Recargar el preview y escribir en el input de búsqueda del sidebar.
Esperado: al teclear, los ítems del menú que no coinciden desaparecen y los que coinciden quedan (sin acento-sensibilidad: "parametriz" encuentra "Parametrización"). Al borrar el texto, reaparecen todos. Sin errores en consola.

- [ ] **Step 4: Commit**

```bash
git add ReporteTareas/js/menuBuscador.js preview/tema-preview.html
git commit -m "feat(menu): buscador de menu lateral y marcado de item activo"
```

---

### Task 5: Cablear tema y menú en el Master

**Files:**
- Modify: `ReporteTareas/Formulario/Master.Master`
- Modify: `ReporteTareas/Formulario/Master.master.cs:38-58`

**Interfaces:**
- Consumes: `tema-dos.css` (Task 1-3), `menuBuscador.js` (Task 4).
- Produces: chrome real temado en las 69 páginas que usan este Master.

- [ ] **Step 1: Cargar el tema como último `<link>` del head**

En `Master.Master`, tras la línea 32 (`sweetalert.init.js`) y antes del `<style>` de la línea 33, añadir:

```html
    <link href="../dist/css/tema-dos.css" rel="stylesheet" />
```

- [ ] **Step 2: Añadir el input de buscador al sidebar**

En `Master.Master`, dentro de `<div class="sidebar-nav navbar-collapse">` (línea 94), justo antes de `<asp:Label ID="lblCargarMenu" ...>`, insertar:

```html
                        <div class="dos-menu-search">
                            <input type="text" id="dos-menu-filtro" placeholder="Buscar en el menú..." autocomplete="off" />
                        </div>
```

- [ ] **Step 3: Cargar `menuBuscador.js` al final del body**

En `Master.Master`, tras la línea 132 (datatables), antes de `</body>` (línea 135), añadir:

```html
    <script src="../js/menuBuscador.js"></script>
```

- [ ] **Step 4: Ajustar SOLO la plantilla HTML del menú en el code-behind**

En `Master.master.cs`, la fuente de datos y el filtrado NO cambian. Reemplazar únicamente el bloque de construcción de string (líneas 38-58, desde `plan1 = plan1 + "<li>";` hasta el cierre del `foreach` externo) por la versión que emite `data-titulo` (para el buscador) y mantiene la clase `nav-second-level` que el JS espera. El markup resultante es equivalente al actual; solo se ajustan atributos:

```csharp
                foreach (DataRow data in dtPadres.Rows)
                {
                    plan1 = plan1 + "<li>";
                    plan1 = plan1 + "<a href='#'><i class='" + data["Class_Icon"].ToString() + "'></i>   " + data["Titulo"].ToString() + "<span class='fa arrow'></span></a>";
                    DataTable dtChild = new DataTable();
                    DataView view2 = dtPrincipal.DefaultView;
                    view2.RowFilter = "Id_MenuPadre=" + data["id_Menu"] + "";
                    dtChild = view.ToTable("UniqueLastNames", true, "id_Menu", "Titulo", "Class_Icon", "Href");
                    plan1 = plan1 + "<ul class='nav nav-second-level'>";
                    foreach (DataRow detalle in dtChild.Rows)
                    {
                        plan1 = plan1 + "<li>";
                        plan1 = plan1 + "<a href='" + detalle["Href"].ToString() + "'><i class='" + detalle["Class_Icon"].ToString() + "'></i>   " + detalle["Titulo"].ToString() + "</a>";
                        plan1 = plan1 + "</li>";
                    }
                    plan1 = plan1 + "</ul>";
                    plan1 = plan1 + "</li>";
                }
```

Nota: el bug preexistente `dtChild = view.ToTable(...)` (usa `view`, no `view2`) NO se corrige aquí — es lógica, fuera de alcance. Se mantiene idéntico.

- [ ] **Step 5: Verificar (grep de no-regresión de lógica)**

Run:
```bash
grep -n "Sp_RTA_ConsultarMenuPerfilUsuario\|seguridad.Encripta\|Session\[" ReporteTareas/Formulario/Master.master.cs
```
Expected: las llamadas a `Sp_RTA_ConsultarMenuPerfilUsuario(Idperfil)`, `seguridad.Encripta(...)` y los accesos a `Session[...]` siguen presentes e idénticos (la lógica intacta).

- [ ] **Step 6: Verificar visualmente (requiere levantar el proyecto en VS)**

Compilar y navegar a cualquier página que use el Master (ej. `Principal.aspx`). Confirmar: sidebar temado, buscador funcional, ítem activo resaltado, navbar temada. Si no se compila en este momento, dejar anotado para la validación final del piloto.

- [ ] **Step 7: Commit**

```bash
git add ReporteTareas/Formulario/Master.Master ReporteTareas/Formulario/Master.master.cs
git commit -m "feat(chrome): cargar tema-dos y buscador de menu en el Master"
```

---

### Task 6: Login.aspx

**Files:**
- Modify: `ReporteTareas/Formulario/Login.aspx`

**Interfaces:**
- Consumes: `tema-dos.css`.
- Produces: pantalla de login temada. Controles `asp:` (`txt_login`, `txt_pass`, `btn_ingresar`, etc.) y sus IDs intactos.

- [ ] **Step 1: Cargar el tema como último `<link>`**

En `Login.aspx`, tras la línea 19 (font-awesome) añadir:

```html
    <link href="../dist/css/tema-dos.css" rel="stylesheet">
```

- [ ] **Step 2: Reemplazar `<font>` e inline por markup limpio en el panel de login**

Reemplazar el bloque `panel-heading` (líneas 40-44) por:

```html
                            <div class="panel-heading" style="text-align:center;background:var(--dos-surface)">
                                <img src="../Img/iconoDos.png" height="110" alt="DOS" /><br />
                                <h1 class="text-center" style="font-size:22px;margin:12px 0 4px">Sistema de Gestión Interno</h1>
                                <small class="text-muted">Versión 2.5</small>
                            </div>
```

Reemplazar los dos labels con `<font>` (líneas 48 y 54) por texto plano dentro de su `<h4>`/`<label>`:

- Línea 48: `<h4 class="panel-title text-left"><label>Usuario</label></h4>`
- Línea 54: dejar el `<asp:Label ID="lab2" ...>` pero con su texto sin `<FONT>`: `<asp:Label ID="lab2" runat="server" Enabled="False"><b>Contraseña</b></asp:Label>`

Cambiar el `panel-body` inline gris (línea 45) `style="background-color: #d3d3d3"` por `style="background:var(--dos-surface)"`.

- [ ] **Step 3: Verificar (grep de no-regresión)**

Run:
```bash
grep -n "txt_login\|txt_pass\|btn_ingresar\|OnClick" ReporteTareas/Formulario/Login.aspx
```
Expected: `txt_login`, `txt_pass`, `btn_ingresar` con `OnClick="btn_ingresar_Click"` presentes e intactos.

- [ ] **Step 4: Verificar visualmente**

Abrir `Login.aspx` en la app (o comparar el bloque en el preview replicando el markup). Esperado: login centrado, logo, título limpio sin `<font>`, botón Ingresar en rojo DOS, sin fondo gris `#d3d3d3`.

- [ ] **Step 5: Commit**

```bash
git add ReporteTareas/Formulario/Login.aspx
git commit -m "feat(login): tema DOS y limpieza de <font>/estilos inline"
```

---

### Task 7: Principal.aspx

**Files:**
- Modify: `ReporteTareas/Formulario/Principal.aspx`

**Interfaces:**
- Consumes: `tema-dos.css` (heredado del Master).
- Produces: portada limpia y responsive. El carrusel y el `<video>` siguen funcionando (mismas rutas/IDs Bootstrap).

- [ ] **Step 1: Reemplazar el bloque de bienvenida con `<font>` (líneas 11-16)**

```html
                        <h1 style="color:var(--dos-primary-600);font-weight:700">Bienvenidos al Sistema de Gestión Interno</h1>
                        <p class="text-muted">Este es el Sistema de Gestión Interno de DOS.</p>
```

- [ ] **Step 2: Reemplazar el bloque de texto corporativo con `<font>` (líneas 70-83)**

Mantener el carrusel y el `<video>` intactos; solo cambiar las envolturas `<font>`:

```html
                    <div class="text-center">
                        <h2 style="font-weight:700;margin:24px 0">30 años como la empresa de tecnología líder del Ecuador</h2>
                        <p class="text-left">Hemos crecido como aliados de nuestros clientes apoyando su gestión empresarial con talento humano altamente calificado, brindando servicios de asesoría, data center, cloud computing, almacenamiento, mantenimiento preventivo y correctivo, redes empresariales y mucho más. Más de tres décadas nos respaldan como la empresa de tecnología más importante del Ecuador. Somos representantes autorizados de las mejores marcas del mundo: Microsoft, HP y HPE, Cisco, Oracle, Xerox, Red Hat, F5, Veeam, Simplivity, VMware, Micro Focus y Cylance.</p>
                        <video controls muted width="80%" style="max-width:100%;height:auto">
                            <source src="../carrusel/imagenes/VideoDOS.mp4" type="video/mp4">
                        </video>
                    </div>
```

- [ ] **Step 3: Envolver el contenido en un card temado**

Cambiar el `<div class="panel-body">` de la línea 9 por `<div class="panel panel-default"><div class="panel-body">` y cerrar el `</div>` extra correspondiente al final del bloque (antes de cerrar `col-lg-12`). Verificar balanceo de tags tras el cambio.

- [ ] **Step 4: Verificar (grep de no-regresión)**

Run:
```bash
grep -n "myCarousel\|carousel-inner\|VideoDOS" ReporteTareas/Formulario/Principal.aspx
```
Expected: `myCarousel`, `carousel-inner` y `VideoDOS.mp4` presentes; el carrusel no se removió.

- [ ] **Step 5: Verificar visualmente**

Abrir `Principal.aspx` en la app. Esperado: título rojo DOS sin `<font size=70>`, texto legible, carrusel y video funcionando, contenido dentro de un card con sombra. Reducir a móvil: el video y las imágenes no desbordan.

- [ ] **Step 6: Commit**

```bash
git add ReporteTareas/Formulario/Principal.aspx
git commit -m "feat(principal): limpieza de <font>/inline y layout responsive en card"
```

---

### Task 8: ParametrizacionHorarioUsuario.aspx

**Files:**
- Modify: `ReporteTareas/Formulario/ParametrizacionHorarioUsuario.aspx`

**Interfaces:**
- Consumes: `tema-dos.css`.
- Produces: pantalla form+tabla+modales unificada. IDs (`txtBuscar`, `btnBuscar`, `modalAsignar`, `cmbPerfilHorario`, etc.) y llamadas JS (`BuscarUsuarios()`, `GuardarAsignacion()`) intactos.

- [ ] **Step 1: Unificar el encabezado a un solo patrón de card**

Reemplazar el bloque `card card-primary` (líneas 11-16) por un `panel` temado consistente con el resto:

```html
                <div class="panel panel-default">
                    <div class="panel-heading" style="text-align:center">
                        <h3 style="margin:6px 0">Parametrización de horario por usuario</h3>
                    </div>
                </div>
```

- [ ] **Step 2: Quitar los `style="background:#fcf8e3"` de los headers de modal (líneas 60 y 97)**

Reemplazar `style="background: #fcf8e3"` por (sin atributo; el tema ya estiliza `.modal-header`). En línea 97 conservar el `id="modalMensajeInformativoTipo"` (lo usa el JS):

```html
                    <div class="modal-header">
```
y
```html
                    <div class="modal-header" id="modalMensajeInformativoTipo">
```

- [ ] **Step 3: Quitar los paddings inline redundantes**

Quitar `style="padding: 0px"` de las líneas 8, 19, 41, 47 y `style="padding: 20px"` de la 10 (el tema ya define espaciado). No tocar el `style="height: 430px; overflow-y: auto; overflow-x: auto;"` de la línea 46 (es funcional: scroll de la tabla).

- [ ] **Step 4: Verificar (grep de no-regresión)**

Run:
```bash
grep -n "txtBuscar\|btnBuscar\|modalAsignar\|GuardarAsignacion\|cmbPerfilHorario\|modalMensajeInformativoTipo" ReporteTareas/Formulario/ParametrizacionHorarioUsuario.aspx
```
Expected: todos los IDs y llamadas presentes e intactos.

- [ ] **Step 5: Verificar visualmente**

Abrir la pantalla en la app. Esperado: encabezado consistente con el resto, form de búsqueda ordenado, tabla con scroll, modales con header temado (sin amarillo `#fcf8e3`). El botón Buscar y el guardado del modal siguen funcionando.

- [ ] **Step 6: Commit**

```bash
git add "ReporteTareas/Formulario/ParametrizacionHorarioUsuario.aspx"
git commit -m "feat(param-horario): unifica card/panel y limpia estilos inline"
```

---

### Task 9: ReporteHorasExtras.aspx

**Files:**
- Modify: `ReporteTareas/Formulario/ReporteHorasExtras.aspx`

**Interfaces:**
- Consumes: `tema-dos.css`.
- Produces: reporte con tabla temada. El `GridView` `dgv_Tareas`, filtros y code-behind intactos.

- [ ] **Step 1: Limpiar el encabezado (línea 14)**

Reemplazar `<h1>Reporte Horas Extras</h1>` (dentro de su bloque) para que quede en un panel-heading consistente. Verificar el contexto exacto con Read antes de editar; envolver así:

```html
                        <div class="panel-heading">
                            <h3 style="margin:6px 0">Reporte Horas Extras</h3>
                        </div>
```

- [ ] **Step 2: Quitar los estilos inline del GridView (línea 104)**

Reemplazar los atributos visuales inline del `GridView` por clases del tema, conservando `ID`, `runat`, `AutoGenerateColumns` y `Width`:

```aspx
<asp:GridView ID="dgv_Tareas" runat="server" CssClass="table table-striped table-bordered table-hover table-responsive" AutoGenerateColumns="False" Width="100%">
```
(Se eliminan `CellPadding="4"`, `ForeColor="#333333"`, `GridLines="None"` — el tema los cubre.)

- [ ] **Step 3: Normalizar los paddings inline redundantes de las filas de filtros**

Quitar los `style="padding-top: 10px; padding-top: 10px;"` duplicados (líneas 82, 93) dejando un solo `style="padding-top:10px"`. No tocar los `col-lg-*`/`col-xs-*` (son responsive y funcionales).

- [ ] **Step 4: Verificar (grep de no-regresión)**

Run:
```bash
grep -n "dgv_Tareas\|AutoGenerateColumns\|runat=\"server\"" ReporteTareas/Formulario/ReporteHorasExtras.aspx | head
```
Expected: `dgv_Tareas` con `runat="server"` y `AutoGenerateColumns="False"` presente e intacto.

- [ ] **Step 5: Verificar visualmente**

Abrir el reporte en la app y generar datos. Esperado: encabezado consistente, GridView con encabezado gris/zebra/hover del tema, filtros ordenados, sin desbordes en móvil (scroll horizontal en `table-responsive`).

- [ ] **Step 6: Commit**

```bash
git add ReporteTareas/Formulario/ReporteHorasExtras.aspx
git commit -m "feat(rpt-horas-extras): tema en GridView y limpieza de inline"
```

---

## Validación final del piloto

Tras la Task 9, con el proyecto compilado en Visual Studio:

- [ ] Login, Principal, ParametrizacionHorarioUsuario y ReporteHorasExtras se ven temadas y funcionan (buscar, guardar, generar reporte, marcación de entrada/salida).
- [ ] El buscador del menú filtra y el ítem activo se resalta en cada pantalla.
- [ ] Prueba de reversibilidad: comentar el `<link>` de `tema-dos.css` en `Master.Master` devuelve el chrome al estado anterior sin errores.
- [ ] Revisión responsive a ~375px de las 4 pantallas.
- [ ] Decidir con el usuario las olas para las ~66 páginas restantes (fuera de este plan).
