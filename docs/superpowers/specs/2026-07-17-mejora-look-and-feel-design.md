# Mejora de Look-and-Feel — Sistema ReporteTareas (DOS)

**Fecha:** 2026-07-17
**Autor:** Guillermo (con Claude Code)
**Estado:** Aprobado — piloto

## 1. Objetivo

Modernizar la interfaz del sistema ASP.NET WebForms "ReporteTareas" para que sea
más amigable, intuitiva y responsive, **sin afectar la lógica construida**.

Dolores a resolver (los cuatro son alcance):

1. Se ve viejo / desactualizado (tema SB Admin 2 + Bootstrap 3 de 2019).
2. Es difícil de navegar (menú lateral sin buscador ni estado activo claro).
3. Formularios y tablas confusos (campos apretados, tablas densas, feedback pobre).
4. No funciona bien en móvil / tablet.

## 2. Contexto técnico

- **Stack:** ASP.NET WebForms (.aspx + code-behind C#), Bootstrap 3, sb-admin-2,
  jQuery 1.9.1 (CDN externo), Font Awesome 4, metisMenu, Morris, DataTables 1.10.
- **75 páginas** en `ReporteTareas/Formulario/`. **69 comparten** un único
  `Formulario/Master.Master` (navbar + sidebar + chrome). `Login.aspx` tiene su
  propio HTML completo (no usa el Master).
- El menú lateral se genera **concatenando strings HTML** en
  `Formulario/Master.master.cs` (`Page_Load`), a partir de
  `NegMenuDos.Sp_RTA_ConsultarMenuPerfilUsuario(Idperfil)`.
- Problemas de presentación observados: etiquetas `<font color size>` (ej.
  `Principal.aspx`), estilos inline sueltos, colores hardcodeados (`#750202`,
  `#930001`), mezcla de clases `card` (BS4) y `panel` (BS3) en páginas recientes.

## 3. Identidad visual

**Decisión:** mantener el rojo corporativo DOS, modernizado.

- Rojo DOS como color de marca, usado con criterio: acciones primarias, estado
  activo, acentos. Neutros limpios (grises fríos) alrededor.
- Primario aproximado `#8B1A1A` / `#A32020` con escala de tonos; se afina en el
  preview.
- El logo y las alertas conservan el rojo; no se "tiñe" toda la UI de rojo.

## 4. Enfoque elegido

**Enfoque B — Tema CSS + rediseño del chrome + limpieza por olas**, arrancando
con un **piloto** (chrome + 3 pantallas) revisado mediante **preview estático**.

Descartados:
- **A (solo CSS):** no resuelve navegación ni responsive (viven en el HTML).
- **C (migrar a Bootstrap 5):** rompe 75 páginas a la vez (`panel`→`card`, grid,
  `form-group`), y plugins (metisMenu, Morris, DataTables viejo) no soportan BS5.
  Riesgo desproporcionado frente al requisito de no arriesgar la lógica.

## 5. Arquitectura del tema

- Nuevo archivo **`ReporteTareas/dist/css/tema-dos.css`**, cargado como el
  **último** `<link>` del `<head>` en `Master.Master` (y en `Login.aspx`). Por
  cascada, gana sobre Bootstrap 3 / sb-admin-2 sin borrar nada existente.
- Cabecera del archivo: bloque `:root` con **variables CSS** (tokens):
  - Paleta: primario rojo DOS + escala, neutros, estados (éxito/peligro/aviso).
  - Radio, sombras, espaciado, tipografía.
- Todo el CSS del tema consume esas variables → reajustar la marca = editar el
  bloque `:root`.
- **Reversibilidad total:** comentar el `<link>` de `tema-dos.css` devuelve el
  sistema exactamente al estado actual.

## 6. Chrome (navbar + sidebar + menú)

- **Navbar:** se moderniza vía CSS (altura consistente, rojo DOS como fondo/acento,
  botones Entrada/Salida y nombre de usuario alineados y legibles). Se retira el
  color inline `#750202` hacia variable.
- **Sidebar:** se estiliza el `metisMenu` existente — estado activo claro (pantalla
  actual), hover legible, íconos alineados — y se añade un **buscador de menú**
  (input que filtra ítems del lado cliente con JS nuevo).
- **Generador de menú (`Master.master.cs`):** se modifica **únicamente** la parte
  que arma el string HTML (clases/estructura emitidas) para habilitar buscador y
  estado activo.
  - **NO se toca:** la fuente de datos `Sp_RTA_ConsultarMenuPerfilUsuario(Idperfil)`,
    el filtrado por perfil, ni la seguridad.

## 7. Piloto — 3 pantallas

1. **`Principal.aspx`** — eliminar `<font>` y estilos inline; carrusel y bienvenida
   en markup limpio y responsive.
2. **`ParametrizacionHorarioUsuario.aspx`** — unificar `card`/`panel` a un solo
   patrón; ordenar formulario de búsqueda, tabla y los dos modales. Patrón
   representativo "formulario + tabla + modal".
3. **Un reporte con tabla / DataTable** (a elegir al llegar) — validar tablas densas:
   encabezados, zebra, paginación, responsive con scroll horizontal.

En las tres: **cero cambios** a controles `asp:`, a sus IDs, ni a los `.js` /
code-behind que los manejan. Solo markup de presentación y clases CSS.

## 8. Verificación

- Generar **`preview/tema-preview.html`**: página estática autocontenida que carga
  Bootstrap 3 + `tema-dos.css` y reproduce navbar, sidebar con buscador, un
  formulario y una tabla con markup de ejemplo.
- Se abre directo en el navegador (o se publica como Artifact) para iterar el look
  sin compilar el proyecto .NET.
- Al aprobar el preview, se aplica el tema al `Master` y a las 3 pantallas reales.

## 9. Alcance — lo que NO se toca (restricción dura)

- `CapaNegocio`, `CapaDato`, `CapaEntidad`.
- Cualquier `Page_Load` / evento de servidor (salvo la plantilla HTML del menú en
  `Master.master.cs`, sección 6).
- IDs de controles `asp:`.
- Stored procedures.
- Lógica de los `.js` existentes (solo se **añade** el JS del buscador de menú).

## 10. Criterios de éxito

- El sistema luce moderno y profesional, reconociblemente DOS (rojo de marca).
- El menú lateral tiene buscador y estado activo; navegar es más rápido.
- Formularios y tablas del piloto son legibles y ordenados.
- Las pantallas del piloto se ven y funcionan bien en móvil/tablet.
- Ningún cambio de comportamiento en la lógica; el piloto es reversible quitando
  el `<link>` del tema y revirtiendo el markup de las 3 páginas.

## 11. Después del piloto

Con el piloto aprobado, la limpieza del resto de las ~66 páginas se planifica por
**olas** (agrupadas por tipo: formularios de registro, reportes/tablas, historias
clínicas, etc.), cada ola desplegable y reversible por separado. Fuera del alcance
de este spec.
