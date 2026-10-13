# Horas por persona y cliente — Plan de implementación

> **Para quien ejecute esto:** SUB-SKILL REQUERIDA: usar `superpowers:subagent-driven-development` (recomendado) o `superpowers:executing-plans` para ejecutar tarea por tarea. Los pasos usan casillas (`- [ ]`) para llevar la cuenta.

**Objetivo:** Mostrar, dentro del dashboard de `AprobacionTareasJefatura.aspx`, una tabla de horas **aprobadas** por persona y por cliente; y corregir el gráfico de empresas, que hoy mezcla estados.

**Arquitectura:** El procedimiento que ya alimenta el dashboard gana una columna y un sexto conjunto, los dos sobre la tabla temporal `#Base` que ya existe. El pivote y el recorte de columnas viven en `CapaNegocio`, como funciones puras con pruebas. La pantalla solo dibuja.

**Tecnologías:** SQL Server (T-SQL), C# .NET Framework 4.6.1, MSTest, jQuery, Chart.js 4.5.1.

**Spec:** `docs/superpowers/specs/2026-09-23-horas-por-persona-y-cliente-design.md`

## Restricciones globales

- **Compilar solo con** `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe`, desde PowerShell. El MSBuild del PATH falla con errores que despistan.
- **Correr las pruebas con:** `"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll`
- **Las tres capas se publican como `.exe`**, no `.dll`: `CapaEntidad.exe`, `CapaNegocio.exe`, `CapaDato.exe`.
- **`CapaDato` no puede llamar a `CapaNegocio`.** El proyecto web referencia `CapaEntidad` y `CapaNegocio`, nunca `CapaDato`.
- **Los `.aspx` necesitan BOM UTF-8**; los `.sql` versionados van **sin BOM**.
- **Correr un script SQL con** `bash docs/sql/run-sql.sh <archivo.sql>`, que apunta siempre a la base de la aplicación.
- **El repositorio es público:** ningún nombre, cédula ni dato de persona real en código, scripts ni documentos.

---

### Task 1: El procedimiento devuelve lo aprobado por empresa y el cruce persona × cliente

**Archivos:**
- Crear: `docs/sql/2026-09-23-dashboard-horas-por-cliente.sql`
- Referencia (no se modifica): `docs/sql/2026-09-21-dashboard-aprobacion.sql`

**Interfaces:**
- Produce: `Sp_RTA_DashboardAprobacionJefatura` con **seis** conjuntos. El quinto suma una columna `MinutosAprobados`; el sexto devuelve `Id_Responsable, Nombre, Empresa, MinutosAprobados`.
- Consume: la tabla temporal `#Base (Id_Responsable, Nombre, Fecha, Estado, Empresa, Minutos, FechaAprob)`, que **no se toca**.

- [ ] **Paso 1: Copiar el script vigente como punto de partida**

```bash
cp docs/sql/2026-09-21-dashboard-aprobacion.sql docs/sql/2026-09-23-dashboard-horas-por-cliente.sql
```

- [ ] **Paso 2: Reemplazar la cabecera del archivo nuevo**

Borrar el bloque de comentario inicial (hasta el primer `SET NOEXEC`, si lo hubiera) y dejar este encabezado. **Sin BOM.**

```sql
/* ============================================================================
   Dashboard de aprobacion: horas aprobadas por empresa y cruce persona x cliente
   ReporTarea  |  2026-09-23

   PENDIENTE DE EJECUTAR.

   Recrea Sp_RTA_DashboardAprobacionJefatura con dos cambios:

   - Conjunto 5 (por empresa): agrega la columna MinutosAprobados. La columna
     Minutos se conserva a proposito: quitarla romperia el DAO que hoy esta
     desplegado, durante la ventana entre este script y los binarios.

   - Conjunto 6 (nuevo): minutos aprobados por responsable Y empresa. Devuelve
     todas las combinaciones sin recortar; el top N y la porcion "Otras" los
     arma NegDashboardAprobacion, que esta probado.

   Los dos salen de #Base, que ya trae Estado y Empresa: la consulta que la
   llena no cambia. Por eso los numeros cuadran con la tabla de APROBADAS.

   Idempotente: DROP y CREATE. No toca ninguna tabla: solo lee.
   ============================================================================ */
```

- [ ] **Paso 3: Cambiar el conjunto 5**

Reemplazar el `SELECT` del bloque `/* --- 5. por empresa` por:

```sql
    SELECT
        Empresa          = Empresa,
        Minutos          = ISNULL(SUM(Minutos), 0),
        MinutosAprobados = ISNULL(SUM(CASE WHEN Estado = 2 THEN Minutos END), 0)
      FROM #Base
     GROUP BY Empresa
     ORDER BY 3 DESC;
```

- [ ] **Paso 4: Agregar el conjunto 6, justo antes de `DROP TABLE #Base;`**

```sql
    /* --- 6. persona x cliente ---------------------------------------------
       Solo aprobadas: es lo unico que la tabla de la pantalla muestra. Se
       devuelven todas las combinaciones, sin recortar, porque el top N y la
       porcion "Otras" viven en NegDashboardAprobacion y ahi estan probadas.

       Las tareas sin empresa NO se descartan: si se descartaran, la suma de la
       tabla dejaria de coincidir con la tarjeta de horas aprobadas, y esa
       diferencia haria dudar de las dos cifras. El nombre visible del grupo lo
       pone CapaNegocio. */
    SELECT
        Id_Responsable   = b.Id_Responsable,
        Nombre           = MAX(b.Nombre),
        Empresa          = ISNULL(b.Empresa, ''),
        MinutosAprobados = ISNULL(SUM(b.Minutos), 0)
      FROM #Base b
     WHERE b.Estado = 2
     GROUP BY b.Id_Responsable, ISNULL(b.Empresa, '')
     ORDER BY 4 DESC;
```

- [ ] **Paso 5: Correr el script**

```bash
bash docs/sql/run-sql.sh docs/sql/2026-09-23-dashboard-horas-por-cliente.sql
```

Esperado: `Sp_RTA_DashboardAprobacionJefatura actualizado.` y ningún `Msg`.

- [ ] **Paso 6: Verificar que el sexto conjunto cuadra con el primero**

Correr, reemplazando el código de jefe y el rango por uno con datos:

```sql
EXEC dbo.Sp_RTA_DashboardAprobacionJefatura @IdUsuarioJefe = '<cod>',
     @FechaDesde = '2026-09-01', @FechaHasta = '2026-09-22';
```

Esperado: **seis** grillas. La suma de `MinutosAprobados` del sexto conjunto tiene que ser **igual** a `MinutosAprobados` del primero. Si difieren, el `WHERE b.Estado = 2` no está filtrando lo mismo que el `CASE` del conjunto 1: revisar antes de seguir.

- [ ] **Paso 7: Commit**

```bash
git add docs/sql/2026-09-23-dashboard-horas-por-cliente.sql
git commit -m "feat(dashboard): el procedimiento devuelve aprobadas por empresa y el cruce persona x cliente"
```

---

### Task 2: El gráfico de empresas muestra solo lo aprobado

**Archivos:**
- Modificar: `CapaEntidad/EntDashboardAprobacion.cs` (clase `EntDashboardEmpresa`)
- Modificar: `CapaNegocio/NegDashboardAprobacion.cs` (nueva `SoloAprobadas`, y `Cargar`)
- Modificar: `CapaPruebas/NegDashboardAprobacionTests.cs`

**Interfaces:**
- Consume: `EntDashboardEmpresa.Minutos` (ya existe).
- Produce: `EntDashboardEmpresa.MinutosAprobados` (int) y `NegDashboardAprobacion.SoloAprobadas(List<EntDashboardEmpresa>) → List<EntDashboardEmpresa>`. El gráfico sigue leyendo `Horas`, que `Convertir` ya calcula desde `Minutos`.

> **Por qué así y no tocando `TopConOtras`:** esa función ordena y agrupa por `Minutos`, y tiene cinco pruebas que dependen de eso. En vez de cambiarle el criterio, se le entrega una lista donde `Minutos` **ya es** lo aprobado. `TopConOtras` queda intacta y sus pruebas siguen verdes.

- [ ] **Paso 1: Escribir las pruebas que fallan**

Agregar en `CapaPruebas/NegDashboardAprobacionTests.cs`:

```csharp
[TestMethod]
public void SoloAprobadas_PasaLoAprobadoComoMinutos()
{
    var lista = new List<EntDashboardEmpresa>
    {
        new EntDashboardEmpresa { Empresa = "A", Minutos = 100, MinutosAprobados = 60 }
    };

    var r = NegDashboardAprobacion.SoloAprobadas(lista);

    Assert.AreEqual(1, r.Count);
    Assert.AreEqual("A", r[0].Empresa);
    Assert.AreEqual(60, r[0].Minutos);
}

[TestMethod]
public void SoloAprobadas_NoModificaLaListaOriginal()
{
    var original = new EntDashboardEmpresa { Empresa = "A", Minutos = 100, MinutosAprobados = 60 };
    var lista = new List<EntDashboardEmpresa> { original };

    NegDashboardAprobacion.SoloAprobadas(lista);

    Assert.AreEqual(100, original.Minutos, "la entidad original viaja al JSON: no se pisa");
}

[TestMethod]
public void SoloAprobadas_ConListaNula_DevuelveVacia()
{
    var r = NegDashboardAprobacion.SoloAprobadas(null);

    Assert.IsNotNull(r);
    Assert.AreEqual(0, r.Count);
}
```

- [ ] **Paso 2: Compilar y ver que fallan**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo
```

Esperado: **error de compilación** `CS0117` o `CS0103` — ni `MinutosAprobados` ni `SoloAprobadas` existen todavía. Eso cuenta como la prueba en rojo.

- [ ] **Paso 3: Agregar los dos campos a la entidad**

En `CapaEntidad/EntDashboardAprobacion.cs`, dentro de `EntDashboardEmpresa`, debajo de `Minutos`:

```csharp
        /* Lo aprobado, aparte del total. El grafico muestra esto; Minutos se
           conserva porque el DAO viejo lo lee durante la ventana entre el
           script y los binarios.

           No hay HorasAprobadas a proposito: Convertir hace
           e.Horas = HorasDecimales(e.Minutos), y despues de SoloAprobadas ese
           Minutos ya es lo aprobado. Un campo mas viajaria siempre en cero. */
        public int MinutosAprobados { get; set; }
```

- [ ] **Paso 4: Escribir `SoloAprobadas`**

En `CapaNegocio/NegDashboardAprobacion.cs`, encima de `TopConOtras`:

```csharp
        /// <summary>
        /// La misma lista, con lo aprobado puesto en Minutos.
        ///
        /// Existe para no cambiarle el criterio a TopConOtras, que ordena y
        /// agrupa por Minutos y tiene pruebas que dependen de eso. Devuelve
        /// copias: las entidades originales viajan al JSON de la pantalla y
        /// pisarlas cambiaria lo que ve el navegador.
        /// </summary>
        public static List<EntDashboardEmpresa> SoloAprobadas(List<EntDashboardEmpresa> lista)
        {
            List<EntDashboardEmpresa> resultado = new List<EntDashboardEmpresa>();
            if (lista == null) { return resultado; }

            foreach (EntDashboardEmpresa e in lista)
            {
                resultado.Add(new EntDashboardEmpresa
                {
                    Empresa = e.Empresa,
                    Minutos = e.MinutosAprobados,
                    MinutosAprobados = e.MinutosAprobados
                });
            }

            return resultado;
        }
```

- [ ] **Paso 5: Enchufarlo en `Cargar`**

En `CapaNegocio/NegDashboardAprobacion.cs`, reemplazar la línea

```csharp
            datos.Empresas = TopConOtras(datos.Empresas, 10);
```

por

```csharp
            /* Solo aprobadas: hasta hoy este grafico sumaba todos los estados y
               mostraba un numero que no correspondia a ninguna de las dos
               tarjetas de arriba. */
            datos.Empresas = TopConOtras(SoloAprobadas(datos.Empresas), 10);
```

- [ ] **Paso 6: Compilar y correr las pruebas**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Esperado: 0 errores y **todas** las pruebas en verde, incluidas las cinco de `TopConOtras`, que no debieron cambiar.

- [ ] **Paso 7: Commit**

```bash
git add CapaEntidad/EntDashboardAprobacion.cs CapaNegocio/NegDashboardAprobacion.cs CapaPruebas/NegDashboardAprobacionTests.cs
git commit -m "fix(dashboard): el grafico de empresas muestra lo aprobado, no la mezcla de estados"
```

---

### Task 3: El pivote persona × cliente

**Archivos:**
- Modificar: `CapaEntidad/EntDashboardAprobacion.cs` (tres clases nuevas y dos propiedades en la raíz)
- Modificar: `CapaNegocio/NegDashboardAprobacion.cs` (`Pivote`, constante, y `Cargar`)
- Modificar: `CapaPruebas/NegDashboardAprobacionTests.cs`

**Interfaces:**
- Consume: `NegDashboardAprobacion.HorasDecimales(int) → decimal` (ya existe).
- Produce:
  - `EntDashboardPersonaEmpresa { string Id_Responsable, string Nombre, string Empresa, int Minutos }`
  - `EntDashboardFilaCliente { string Nombre, List<int> Minutos, List<decimal> Horas, int MinutosTotal, decimal HorasTotal }`
  - `EntDashboardTablaClientes { List<string> Columnas, List<EntDashboardFilaCliente> Filas, EntDashboardFilaCliente Totales }`
  - `EntDashboardAprobacion.PersonaEmpresa` (lista) y `EntDashboardAprobacion.TablaClientes`
  - `NegDashboardAprobacion.Pivote(List<EntDashboardPersonaEmpresa> filas, int columnas) → EntDashboardTablaClientes`
  - `NegDashboardAprobacion.EtiquetaSinEmpresa` = `"(sin empresa)"` — **la misma etiqueta que ya emite el SQL**, ver la nota abajo

- [ ] **Paso 1: Escribir las pruebas que fallan**

Agregar en `CapaPruebas/NegDashboardAprobacionTests.cs`:

```csharp
private static List<EntDashboardPersonaEmpresa> Cruce(params string[] datos)
{
    /* Cada terna es persona, empresa, minutos. */
    var lista = new List<EntDashboardPersonaEmpresa>();
    for (int i = 0; i < datos.Length; i += 3)
    {
        lista.Add(new EntDashboardPersonaEmpresa
        {
            Id_Responsable = datos[i],
            Nombre = datos[i],
            Empresa = datos[i + 1],
            Minutos = int.Parse(datos[i + 2], CultureInfo.InvariantCulture)
        });
    }
    return lista;
}

[TestMethod]
public void Pivote_ColocaCadaValorEnSuCelda()
{
    var t = NegDashboardAprobacion.Pivote(
        Cruce("ANA", "A", "60", "ANA", "B", "30", "LUIS", "A", "120"), 8);

    Assert.AreEqual(2, t.Columnas.Count);
    Assert.AreEqual("A", t.Columnas[0], "la columna con mas minutos va primero");
    Assert.AreEqual("B", t.Columnas[1]);

    Assert.AreEqual("LUIS", t.Filas[0].Nombre, "las filas van por total descendente");
    Assert.AreEqual(120, t.Filas[0].Minutos[0]);
    Assert.AreEqual(0, t.Filas[0].Minutos[1], "celda sin dato es cero, no un hueco");

    Assert.AreEqual("ANA", t.Filas[1].Nombre);
    Assert.AreEqual(60, t.Filas[1].Minutos[0]);
    Assert.AreEqual(30, t.Filas[1].Minutos[1]);
}

[TestMethod]
public void Pivote_TotalesPorFilaYPorColumnaCuadran()
{
    var t = NegDashboardAprobacion.Pivote(
        Cruce("ANA", "A", "60", "ANA", "B", "30", "LUIS", "A", "120"), 8);

    Assert.AreEqual(120, t.Filas[0].MinutosTotal);
    Assert.AreEqual(90, t.Filas[1].MinutosTotal);

    Assert.AreEqual(180, t.Totales.Minutos[0], "columna A");
    Assert.AreEqual(30, t.Totales.Minutos[1], "columna B");
    Assert.AreEqual(210, t.Totales.MinutosTotal);
}

[TestMethod]
public void Pivote_ConMasClientesQueColumnas_ElSobranteVaAOtras()
{
    var t = NegDashboardAprobacion.Pivote(
        Cruce("ANA", "A", "100", "ANA", "B", "50", "ANA", "C", "20", "ANA", "D", "5"), 2);

    Assert.AreEqual(3, t.Columnas.Count);
    Assert.AreEqual("Otras", t.Columnas[2]);
    Assert.AreEqual(25, t.Filas[0].Minutos[2], "20 + 5");
    Assert.AreEqual(175, t.Filas[0].MinutosTotal, "el total general no cambia por recortar");
}

[TestMethod]
public void Pivote_EmpresaVaciaSeAgrupaBajoLaEtiquetaDelSql()
{
    var t = NegDashboardAprobacion.Pivote(
        Cruce("ANA", "", "40", "ANA", "A", "10"), 8);

    Assert.IsTrue(t.Columnas.Contains("(sin empresa)"));
    Assert.AreEqual(50, t.Filas[0].MinutosTotal, "no se descarta: seguiria sin cuadrar con la tarjeta");
}

[TestMethod]
public void Pivote_ConvierteAHorasDespuesDeSumar()
{
    /* Tres tramos de 5 minutos son 15 minutos: 0,25 h, que HorasDecimales
       redondea al par y deja en 0,2. Convertir cada tramo antes de sumar daria
       0,1 + 0,1 + 0,1 = 0,3. Por eso la conversion va al final. */
    var t = NegDashboardAprobacion.Pivote(
        Cruce("ANA", "A", "5", "ANA", "A", "5", "ANA", "A", "5"), 8);

    Assert.AreEqual(15, t.Filas[0].Minutos[0]);
    Assert.AreEqual(0.2m, t.Filas[0].Horas[0]);
    Assert.AreEqual(0.2m, t.Filas[0].HorasTotal);
}

[TestMethod]
public void Pivote_ConListaNula_DevuelveTablaVacia()
{
    var t = NegDashboardAprobacion.Pivote(null, 8);

    Assert.IsNotNull(t);
    Assert.AreEqual(0, t.Columnas.Count);
    Assert.AreEqual(0, t.Filas.Count);
}
```

- [ ] **Paso 2: Compilar y ver que fallan**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo
```

Esperado: error de compilación — no existen `EntDashboardPersonaEmpresa` ni `Pivote`.

> **El redondeo ya está decidido y probado:** `HorasDecimales` usa `Math.Round(minutos / 60m, 1)`, que redondea **al par**. Por eso 15 minutos dan 0,2 y no 0,3, y 75 minutos dan 1,2 y no 1,3 — hay una prueba existente que lo fija. Si una aserción nueva no cuadra, se corrige la prueba nueva, nunca esa función.

- [ ] **Paso 3: Agregar las tres entidades**

En `CapaEntidad/EntDashboardAprobacion.cs`, antes de `EntDashboardAprobacion`:

```csharp
    /// <summary>
    /// Una celda del cruce: cuanto tiempo aprobado puso una persona en un
    /// cliente. Viene del sexto conjunto del procedimiento, sin recortar.
    /// </summary>
    public class EntDashboardPersonaEmpresa
    {
        public string Id_Responsable { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Empresa { get; set; } = "";
        public int Minutos { get; set; }
    }

    /// <summary>
    /// Una fila de la tabla. Minutos y Horas tienen una posicion por columna,
    /// en el mismo orden que EntDashboardTablaClientes.Columnas.
    /// </summary>
    public class EntDashboardFilaCliente
    {
        public string Nombre { get; set; } = "";
        public System.Collections.Generic.List<int> Minutos { get; set; }
            = new System.Collections.Generic.List<int>();
        public System.Collections.Generic.List<decimal> Horas { get; set; }
            = new System.Collections.Generic.List<decimal>();
        public int MinutosTotal { get; set; }
        public decimal HorasTotal { get; set; }
    }

    public class EntDashboardTablaClientes
    {
        public System.Collections.Generic.List<string> Columnas { get; set; }
            = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<EntDashboardFilaCliente> Filas { get; set; }
            = new System.Collections.Generic.List<EntDashboardFilaCliente>();
        public EntDashboardFilaCliente Totales { get; set; } = new EntDashboardFilaCliente();
    }
```

Y dentro de `EntDashboardAprobacion`, junto a `Empresas`:

```csharp
        public System.Collections.Generic.List<EntDashboardPersonaEmpresa> PersonaEmpresa { get; set; }
            = new System.Collections.Generic.List<EntDashboardPersonaEmpresa>();

        public EntDashboardTablaClientes TablaClientes { get; set; } = new EntDashboardTablaClientes();
```

- [ ] **Paso 4: Escribir `Pivote`**

En `CapaNegocio/NegDashboardAprobacion.cs`, debajo de `TopConOtras`. La constante va junto a `EtiquetaOtras`:

```csharp
        public const string EtiquetaSinEmpresa = "(sin empresa)";
```

```csharp
        /// <summary>
        /// La tabla persona x cliente: una fila por persona, una columna por
        /// cliente, mas "Otras" cuando hay mas clientes que columnas.
        ///
        /// Las columnas se eligen por minutos totales, no por cuantas personas
        /// las tocaron: la tabla responde "en que se fue el tiempo".
        ///
        /// Se convierte a horas DESPUES de sumar, igual que en Convertir:
        /// sumar horas ya redondeadas no da lo mismo que redondear la suma.
        /// </summary>
        public static EntDashboardTablaClientes Pivote(List<EntDashboardPersonaEmpresa> filas, int columnas)
        {
            EntDashboardTablaClientes tabla = new EntDashboardTablaClientes();
            if (filas == null || filas.Count == 0) { return tabla; }

            /* 1. Totales por empresa, para elegir las columnas. */
            Dictionary<string, int> porEmpresa = new Dictionary<string, int>();
            foreach (EntDashboardPersonaEmpresa f in filas)
            {
                string empresa = NombreEmpresa(f.Empresa);
                if (!porEmpresa.ContainsKey(empresa)) { porEmpresa[empresa] = 0; }
                porEmpresa[empresa] += f.Minutos;
            }

            List<KeyValuePair<string, int>> ordenadas = new List<KeyValuePair<string, int>>(porEmpresa);
            ordenadas.Sort((a, b) => b.Value.CompareTo(a.Value));

            Dictionary<string, int> indice = new Dictionary<string, int>();
            bool hayOtras = columnas > 0 && ordenadas.Count > columnas;

            for (int i = 0; i < ordenadas.Count; i++)
            {
                if (!hayOtras || i < columnas)
                {
                    indice[ordenadas[i].Key] = tabla.Columnas.Count;
                    tabla.Columnas.Add(ordenadas[i].Key);
                }
            }

            int columnaOtras = -1;
            if (hayOtras)
            {
                columnaOtras = tabla.Columnas.Count;
                tabla.Columnas.Add(EtiquetaOtras);
            }

            /* 2. Una fila por persona. */
            Dictionary<string, EntDashboardFilaCliente> porPersona =
                new Dictionary<string, EntDashboardFilaCliente>();

            foreach (EntDashboardPersonaEmpresa f in filas)
            {
                string clave = f.Id_Responsable ?? "";
                if (!porPersona.ContainsKey(clave))
                {
                    porPersona[clave] = FilaVacia(f.Nombre, tabla.Columnas.Count);
                }

                string empresa = NombreEmpresa(f.Empresa);
                int destino = indice.ContainsKey(empresa) ? indice[empresa] : columnaOtras;
                if (destino < 0) { continue; }

                porPersona[clave].Minutos[destino] += f.Minutos;
                porPersona[clave].MinutosTotal += f.Minutos;
            }

            tabla.Filas = new List<EntDashboardFilaCliente>(porPersona.Values);
            tabla.Filas.Sort((a, b) => b.MinutosTotal.CompareTo(a.MinutosTotal));

            /* 3. Totales y conversion, al final. */
            tabla.Totales = FilaVacia("Total", tabla.Columnas.Count);

            foreach (EntDashboardFilaCliente fila in tabla.Filas)
            {
                for (int c = 0; c < tabla.Columnas.Count; c++)
                {
                    tabla.Totales.Minutos[c] += fila.Minutos[c];
                    fila.Horas[c] = HorasDecimales(fila.Minutos[c]);
                }
                tabla.Totales.MinutosTotal += fila.MinutosTotal;
                fila.HorasTotal = HorasDecimales(fila.MinutosTotal);
            }

            for (int c = 0; c < tabla.Columnas.Count; c++)
            {
                tabla.Totales.Horas[c] = HorasDecimales(tabla.Totales.Minutos[c]);
            }
            tabla.Totales.HorasTotal = HorasDecimales(tabla.Totales.MinutosTotal);

            return tabla;
        }

        private static EntDashboardFilaCliente FilaVacia(string nombre, int columnas)
        {
            EntDashboardFilaCliente fila = new EntDashboardFilaCliente { Nombre = nombre ?? "" };
            for (int i = 0; i < columnas; i++)
            {
                fila.Minutos.Add(0);
                fila.Horas.Add(0m);
            }
            return fila;
        }

        /// <summary>
        /// El nombre visible del cliente. La empresa vacia se agrupa y no se
        /// descarta: descartarla dejaria la tabla sin cuadrar con la tarjeta de
        /// horas aprobadas.
        /// </summary>
        private static string NombreEmpresa(string empresa)
        {
            string limpio = (empresa ?? "").Trim();
            return limpio.Length == 0 ? EtiquetaSinEmpresa : limpio;
        }
```

Si falta el `using System.Collections.Generic;` en el archivo, agregarlo.

- [ ] **Paso 5: Enchufarlo en `Cargar`**

En `CapaNegocio/NegDashboardAprobacion.cs`, debajo de la línea de `TopConOtras`:

```csharp
            /* Ocho columnas y no diez: aqui cada columna compite con el ancho de
               la pantalla, no con la leyenda de un grafico. */
            datos.TablaClientes = Pivote(datos.PersonaEmpresa, 8);
```

- [ ] **Paso 6: Compilar y correr las pruebas**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Esperado: 0 errores, todas verdes.

- [ ] **Paso 7: Commit**

```bash
git add CapaEntidad/EntDashboardAprobacion.cs CapaNegocio/NegDashboardAprobacion.cs CapaPruebas/NegDashboardAprobacionTests.cs
git commit -m "feat(dashboard): el pivote de horas aprobadas por persona y cliente, con pruebas"
```

---

### Task 4: El DAO lee la columna nueva y el sexto conjunto

**Archivos:**
- Modificar: `CapaDato/DaoDashboardAprobacion.cs`

**Interfaces:**
- Consume: `EntDashboardPersonaEmpresa`, `EntDashboardEmpresa.MinutosAprobados` (Task 2 y 3).
- Produce: `EntDashboardAprobacion.PersonaEmpresa` lleno.

- [ ] **Paso 1: Leer la columna nueva del quinto conjunto**

En el bloque `/* 5. por empresa */`, agregar dentro del `new EntDashboardEmpresa`:

```csharp
                                MinutosAprobados = Entero(dr, "MinutosAprobados")
```

(queda después de `Minutos = Entero(dr, "Minutos"),` — cuidado con la coma).

- [ ] **Paso 2: Agregar la lectura del sexto conjunto**

Justo después de cerrar el bloque del quinto, antes del cierre del `using`:

```csharp
                    /* 6. persona x cliente.

                       Si el procedimiento todavia es el viejo, NextResult()
                       devuelve false y la lista queda vacia: la pantalla dice
                       "sin horas aprobadas" en vez de fallar. Ese es el sentido
                       seguro de la ventana entre el script y los binarios. */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            d.PersonaEmpresa.Add(new EntDashboardPersonaEmpresa
                            {
                                Id_Responsable = Texto(dr, "Id_Responsable"),
                                Nombre = Texto(dr, "Nombre"),
                                Empresa = Texto(dr, "Empresa"),
                                Minutos = Entero(dr, "MinutosAprobados")
                            });
                        }
                    }
```

- [ ] **Paso 3: Compilar**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo
```

Esperado: 0 errores.

- [ ] **Paso 4: Commit**

```bash
git add CapaDato/DaoDashboardAprobacion.cs
git commit -m "feat(dashboard): el DAO lee el cruce persona x cliente"
```

---

### Task 5: La tabla en la pantalla

**Archivos:**
- Modificar: `ReporteTareas/Formulario/AprobacionTareasJefatura.aspx` (panel nuevo, título del gráfico, `?v=`)
- Modificar: `ReporteTareas/js/dashboardAprobacion.js`

**Interfaces:**
- Consume: `d.TablaClientes` con `Columnas`, `Filas[].Nombre`, `Filas[].Horas`, `Filas[].HorasTotal`, `Totales`.

- [ ] **Paso 1: Corregir el título del gráfico y agregar el panel de la tabla**

En `AprobacionTareasJefatura.aspx`, cambiar el encabezado del panel de empresas:

```html
<div class="panel-heading">Horas aprobadas por empresa</div>
```

Y después del `</div>` que cierra la fila de los gráficos (la que contiene `graficoPorEmpresa`), agregar:

```html
                            <div class="row">
                                <div class="col-lg-12">
                                    <div class="panel panel-default">
                                        <div class="panel-heading">Horas aprobadas por persona y cliente</div>
                                        <div class="panel-body" style="overflow-x: auto">
                                            <div id="dashTablaClientesVacia" class="text-muted" style="display: none">
                                                Sin horas aprobadas en el rango.
                                            </div>
                                            <table id="dashTablaClientes" class="table table-condensed table-striped" style="font-size: 12px"></table>
                                        </div>
                                    </div>
                                </div>
                            </div>
```

**El archivo necesita BOM UTF-8.** Si el editor lo quita, se ve con caracteres raros en producción.

- [ ] **Paso 2: Subir el `?v=` de `dashboardAprobacion.js`**

En el mismo archivo, cambiar `dashboardAprobacion.js?v=4` por `dashboardAprobacion.js?v=5`. Sin esto, el navegador sigue con el JavaScript viejo y la tabla no aparece.

- [ ] **Paso 3: Pintar la tabla**

En `ReporteTareas/js/dashboardAprobacion.js`, agregar la llamada dentro de `PintarDashboard`, después de `PintarPorEmpresa(d.Empresas || []);`:

```javascript
    PintarTablaClientes(d.TablaClientes || {});
```

Y la función, al final del archivo:

```javascript
/* La tabla persona x cliente. Se arma con texto y no con una libreria: son
   nueve columnas y treinta filas, y DataTables en esta pantalla no se carga. */
function PintarTablaClientes(tabla) {
    var $tabla = $("#dashTablaClientes");
    var $vacia = $("#dashTablaClientesVacia");
    var columnas = tabla.Columnas || [];
    var filas = tabla.Filas || [];

    if (columnas.length === 0 || filas.length === 0) {
        $tabla.empty().hide();
        $vacia.show();
        return;
    }

    $vacia.hide();
    $tabla.show();

    var html = "<thead><tr><th>Persona</th>";
    $.each(columnas, function (i, c) {
        html += "<th style='text-align:right'>" + DashEscapar(c) + "</th>";
    });
    html += "<th style='text-align:right'>Total</th></tr></thead><tbody>";

    $.each(filas, function (i, f) {
        html += "<tr><td>" + DashEscapar(f.Nombre) + "</td>";
        $.each(f.Horas, function (j, h) {
            html += "<td style='text-align:right'>" + DashNumero(h) + "</td>";
        });
        html += "<td style='text-align:right'><b>" + DashNumero(f.HorasTotal) + "</b></td></tr>";
    });

    html += "</tbody>";

    var t = tabla.Totales || {};
    html += "<tfoot><tr><th>Total</th>";
    $.each(t.Horas || [], function (j, h) {
        html += "<th style='text-align:right'>" + DashNumero(h) + "</th>";
    });
    html += "<th style='text-align:right'>" + DashNumero(t.HorasTotal || 0) + "</th></tr></tfoot>";

    $tabla.html(html);
}

/* Los nombres de cliente y de persona vienen de la base y se insertan como
   HTML. Sin esto, un nombre con < o & rompe la tabla. */
function DashEscapar(texto) {
    return $("<div>").text(texto == null ? "" : texto).html();
}
```

- [ ] **Paso 4: Comprobar en el navegador**

Abrir `AprobacionTareasJefatura.aspx`, vista DASHBOARD, con **Ctrl+F5**. Consultar un rango con datos.

Esperado:
- La tabla aparece, con las personas ordenadas de más a menos horas.
- El **total de una persona coincide con sus horas aprobadas en la vista APROBADAS** para el mismo rango. Si no coinciden, es defecto del pivote: comparten `#Base`.
- El gráfico de empresas muestra números **menores** que antes: ahora son solo aprobadas.
- F12 → Consola: sin errores.

- [ ] **Paso 5: Commit**

```bash
git add ReporteTareas/Formulario/AprobacionTareasJefatura.aspx ReporteTareas/js/dashboardAprobacion.js
git commit -m "feat(dashboard): la tabla de horas aprobadas por persona y cliente"
```

---

### Task 6: Compilar, empaquetar y dejar escrito el despliegue

**Archivos:**
- Modificar: `DESPLIEGUE.md` (sección nueva)
- Modificar: los binarios y `ReporteTareas/obj/Release/Package/PackageTmp/`

- [ ] **Paso 1: Compilar y regenerar el paquete**

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas.sln" -t:Build -p:Configuration=Debug -v:minimal -nologo
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "ReporteTareas\ReporteTareas.csproj" -t:Package -p:Configuration=Release -v:minimal -nologo
```

- [ ] **Paso 2: Comprobar que el paquete no quedó viejo**

```bash
grep -h "dashboardAprobacion.js?v=" ReporteTareas/Formulario/AprobacionTareasJefatura.aspx \
     ReporteTareas/obj/Release/Package/PackageTmp/Formulario/AprobacionTareasJefatura.aspx
```

Esperado: los dos dicen `?v=5`. Si no, el paquete se generó antes del último commit: repetir el paso 1.

- [ ] **Paso 3: Descartar el ruido cosmético de los `web.config` del paquete**

```bash
git checkout -- ReporteTareas/obj/Release/Package/PackageTmp/Web.config ReporteTareas/obj/Release/Package/PackageTmp/descargas/perfil/web.config
```

- [ ] **Paso 4: Agregar la sección a `DESPLIEGUE.md`**

```markdown
## 10. Horas por persona y cliente en el dashboard

### 10.1 Base de datos

`docs/sql/2026-09-23-dashboard-horas-por-cliente.sql` — recrea
`Sp_RTA_DashboardAprobacionJefatura`. Idempotente, y no toca ninguna tabla: solo lee.
Va **antes** de los binarios: el DAO nuevo lee un conjunto que el procedimiento viejo no
devuelve.

### 10.2 Archivos a publicar

- `Formulario\AprobacionTareasJefatura.aspx` (sube a `dashboardAprobacion.js?v=5`)
- `js\dashboardAprobacion.js`
- `bin\ReporteTareas.dll`, `bin\CapaEntidad.exe`, `bin\CapaNegocio.exe`, `bin\CapaDato.exe`

Los tres de capa son `.exe`, no `.dll`.

### 10.3 Avisar antes de publicar

**El gráfico "Horas por empresa" va a mostrar números más bajos.** Hasta esta entrega
sumaba todos los estados; ahora muestra solo lo aprobado, como las tarjetas de arriba. No
es una pérdida de datos, es la corrección — pero la jefatura ya está mirando ese gráfico.
```

- [ ] **Paso 5: Commit**

```bash
git add DESPLIEGUE.md ReporteTareas/bin CapaDato/bin CapaEntidad/bin CapaNegocio/bin ReporteTareas/obj/Release/Package/PackageTmp/bin
git commit -m "build(dashboard): compilar, regenerar el paquete y documentar el despliegue"
```

---

## Verificación final

- [ ] `vstest.console.exe CapaPruebas\bin\Debug\CapaPruebas.dll` → todas verdes, ninguna prueba vieja rota.
- [ ] El total de una persona en la tabla nueva **es igual** a sus horas aprobadas en la vista APROBADAS, mismo rango.
- [ ] La suma de la fila Total **es igual** a la tarjeta "Horas aprobadas".
- [ ] Con un rango sin aprobaciones, la tabla dice "Sin horas aprobadas en el rango" y no queda en blanco.
- [ ] La consola del navegador, sin errores.
