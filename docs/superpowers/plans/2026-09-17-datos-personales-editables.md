# Datos personales editables por Talento Humano — Plan de implementación

> **Para trabajadores agénticos:** SUB-SKILL OBLIGATORIA: usar
> superpowers:subagent-driven-development (recomendado) o
> superpowers:executing-plans para ejecutar este plan tarea por tarea. Los pasos
> usan casillas (`- [ ]`) para seguimiento.

**Goal:** Que los perfiles 14 y 18 puedan editar los ocho campos de la pestaña
«Datos personales» de cualquier colaborador desde `PerfilesPersonal.aspx`,
escribiendo en `Empleados` y `R_Usuarios`, creando o adoptando la ficha cuando
falta, y dejando registrado quién hizo el cambio.

**Architecture:** Se reusa entero el camino de las otras dieciséis escrituras del
módulo: el navegador llama a `AdministrarPerfil.ashx` con una acción nueva, el
handler pregunta a `PerfilIdentidad.Objetivo` de quién es el perfil y a
`PerfilIdentidad.Autor` quién lo está tocando, `NegPerfilCampos` filtra claves y
valida valores, `NegPerfil` delega en `DaoPerfil` y un procedimiento almacenado
hace la escritura en las dos tablas dentro de una transacción. Lo único
verdaderamente nuevo es el algoritmo del dígito verificador, que hoy no existe en
C#.

**Tech Stack:** ASP.NET WebForms (.NET Framework 4.8 en el web, 4.6.1 en las
capas), SQL Server, jQuery + Bootstrap 3, MSTest v1 (ensamblado de VS2019, sin
NuGet), `vstest.console.exe`.

**Spec:** `docs/superpowers/specs/2026-09-17-datos-personales-editables-design.md`

---

## Global Constraints

- **Esta entrega depende de la entrega 1.** Antes de la Task 1 tienen que estar
  corridos en producción `docs/sql/2026-09-16-perfil-autor-columnas.sql` y
  `docs/sql/2026-09-16-perfil-autor-procedimientos.sql`. Si
  `Empleados.Usu_ModificacionCod` no existe, el script de la Task 4 se detiene.
- **Ningún `14` ni `18` literal en el código de esta entrega.** La regla vive en
  `NegPerfilAcceso.PerfilesRRHH` y se consulta por `PerfilIdentidad.Objetivo`.
- **Comentarios de código y SQL sin tildes; textos de usuario con tildes.** Es la
  convención del repositorio y se aplica sin excepción.
- **Los `.aspx` y `.ascx` se guardan con BOM.** `Web.config` declara
  `windows-1252` y no trae `fileEncoding`; sin BOM salen con caracteres rotos en
  producción.
- **Todo archivo nuevo se agrega a su `.csproj`.** Son proyectos de formato
  antiguo: un archivo que no está listado no se compila ni se publica, y no avisa.
- **Compilar con el MSBuild de VS2019**, no con el del PATH:
  `C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe`
- **Correr las pruebas con:**
  `"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll`
- **En SQL, `SET NOEXEC ON` en vez de `RETURN` para detener un script.** `RETURN`
  fuera de un procedimiento sale del lote, no del archivo.
- **`RAISERROR` sólo admite literales y variables locales** como argumentos de
  sustitución: un `ISNULL()` ahí es un error de sintaxis que se lleva puesto el
  lote entero.
- **Ningún subagente ejecuta SQL contra la base, ni publica a IIS, ni abre la
  aplicación.** Los scripts se entregan escritos; los corre el usuario.
- **Límites de ancho, que mandan sobre cualquier otro criterio** (el más angosto
  de las dos tablas): nombre **100**, cédula **32**, cargo **128**, área **128**,
  ciudad **150**, jefe **100**, correo **100**, fecha de nacimiento **10**.
- **`R_Usuarios.Nom_Usuario` es NOT NULL:** el nombre es el único campo
  obligatorio de los ocho.

---

## Estructura de archivos

| Archivo | Responsabilidad |
|---|---|
| `docs/sql/2026-09-17-datos-personales-columnas.sql` | **Crear.** Las tres columnas de auditoría de `R_Usuarios` |
| `docs/sql/2026-09-17-datos-personales-procedimientos.sql` | **Crear.** `Sp_RTA_PerfilGuardarDatosPersonales`, `Sp_RTA_PerfilJefesLista` y el `Sp_RTA_PerfilColaborador` con una columna más |
| `CapaEntidad/EntPerfilDatosPersonales.cs` | **Crear.** Los ocho campos editables |
| `CapaEntidad/EntPerfilJefe.cs` | **Crear.** Un código y un nombre, para el combo |
| `CapaEntidad/EntPerfilCabecera.cs` | **Modificar.** Agregar `CodJefeInmediato` |
| `CapaNegocio/NegPerfilCedula.cs` | **Crear.** El dígito verificador, solo y sin dependencias |
| `CapaNegocio/NegPerfilCampos.cs` | **Modificar.** `LeerDatosPersonales` y `ValidarDatosPersonales` |
| `CapaNegocio/NegPerfil.cs` | **Modificar.** `GuardarDatosPersonales` y `ListarJefes` |
| `CapaDato/DaoPerfil.cs` | **Modificar.** Las dos llamadas nuevas y `CodJefeInmediato` en la lectura |
| `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs` | **Modificar.** Dos acciones nuevas |
| `ReporteTareas/Controles/PerfilFichas.ascx` | **Modificar.** El formulario de edición de la pestaña |
| `ReporteTareas/js/miPerfil.js` | **Modificar.** Pintar, habilitar y guardar |
| `ReporteTareas/Formulario/PerfilesPersonal.aspx` | **Modificar.** Declarar `PERFIL_DATOS_EDITABLES` |
| `CapaPruebas/NegPerfilCedulaTests.cs` | **Crear.** El dígito verificador |
| `CapaPruebas/NegPerfilCamposTests.cs` | **Modificar.** Lectura y validación de los ocho campos |
| `DESPLIEGUE.md` | **Modificar.** Qué se publica y en qué orden |

### Una decisión de alcance, dicha de frente

**«Mi perfil» no cambia.** El formulario de edición se dibuja siempre en el
`.ascx` —es un control compartido— pero queda oculto salvo que la página lo
encienda, y la única que lo enciende es `PerfilesPersonal.aspx`. Un usuario con
perfil 14 que abra su propio «Mi perfil» sigue viendo la pestaña de sólo lectura.

Es deliberado: el servidor autoriza por perfil y no le importa desde qué pantalla
le hablen, así que la restricción del cliente no es una medida de seguridad sino
una de alcance. Deja «Mi perfil» exactamente como está hoy para las 228 personas
que la usan, y concentra lo nuevo en la pantalla nueva.

---

## Task 1: Las columnas de auditoría de `R_Usuarios`

**Files:**
- Create: `docs/sql/2026-09-17-datos-personales-columnas.sql`

**Interfaces:**
- Consumes: nada.
- Produces: `R_Usuarios.Usu_ModificacionCod VARCHAR(50) NULL`,
  `R_Usuarios.Fec_Modificacion DATETIME NULL`,
  `R_Usuarios.Ip_Modificacion VARCHAR(64) NULL`. La Task 4 escribe en las tres.

`R_Usuarios` no tiene hoy **ninguna** columna de auditoría —verificado contra
producción el 2026-09-17: ni autor, ni fecha de modificación, ni IP; lo único
parecido es `Fec_Creacion`, que es `varchar(50)`—. Sin esto, cambiar el jefe o el
correo de alguien no deja rastro de quién lo hizo ni de cuándo.

- [ ] **Step 1: Escribir el script**

Archivo nuevo, **con BOM UTF-8**, comentarios sin tildes:

```sql
/* ============================================================================
   Datos personales editables: donde anotar quien cambio que, y cuando
   ReporTarea  |  2026-09-17

   PENDIENTE DE EJECUTAR. Es el paso 1; el orden completo esta en
   docs/superpowers/specs/2026-09-17-datos-personales-editables-design.md.

   ----------------------------------------------------------------------------
   R_Usuarios no tiene NINGUNA columna de auditoria. Verificado contra
   produccion el 2026-09-17: no hay autor, ni fecha de modificacion, ni IP. Lo
   unico parecido es Fec_Creacion, y es varchar(50).

   Hasta hoy daba igual: a R_Usuarios le escribia AdministrarUsuarios.aspx y
   nadie mas. En cuanto Talento Humano pueda cambiar desde el perfil el jefe
   inmediato o el correo de notificacion -los dos campos que deciden quien
   aprueba las vacaciones de alguien y a donde le llegan los avisos- hace falta
   poder reconstruir quien lo hizo.

   Se agregan las tres, no solo el autor: saber quien cambio el jefe de alguien
   sin saber cuando no sirve para reconstruir nada.

   Las tres son NULL y ningun INSERT existente las nombra, asi que
   AdministrarUsuarios.aspx sigue funcionando exactamente igual.

   Idempotente.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------------------------------ 0. guarda --- */

/* Si Usu_ModificacionCod ya existe pero no admite un Cod_Usuario, este script
   no la toca y se detiene. El nombre es nuevo en esta tabla, asi que lo normal
   es que no exista; pero si alguien la creo a mano con otro tipo, un
   IF NOT EXISTS por nombre la daria por buena y el guardado fallaria recien en
   produccion, al escribir el autor.

   RAISERROR admite como argumentos de sustitucion literales y variables
   locales, nada mas: un ISNULL() ahi da "Msg 102, Incorrect syntax near
   'ISNULL'", y como el DECLARE, el SELECT, el IF y el SET NOEXEC ON viven en el
   MISMO lote, ese error de analisis se llevaria puesta la guarda entera. Por
   eso se normaliza sobre variables aparte, DENTRO del BEGIN, cuando el IF ya
   decidio. */
DECLARE @Tipo VARCHAR(128), @Largo INT, @MsgTipo VARCHAR(128), @MsgLargo INT;

SELECT @Tipo = t.name, @Largo = c.max_length
  FROM sys.columns c
  JOIN sys.types   t ON t.user_type_id = c.user_type_id
 WHERE c.object_id = OBJECT_ID('dbo.R_Usuarios')
   AND c.name      = 'Usu_ModificacionCod';

/* max_length = -1 es varchar(MAX): mas ancha que 50, sirve igual. */
IF @Tipo IS NOT NULL AND NOT (@Tipo = 'varchar' AND (@Largo >= 50 OR @Largo = -1))
BEGIN
    SET @MsgTipo  = ISNULL(@Tipo, '(no existe)');
    SET @MsgLargo = ISNULL(@Largo, 0);

    RAISERROR('R_Usuarios.Usu_ModificacionCod ya existe con un tipo que no sirve: es %s de largo %d y se esperaba varchar de 50 o mas, porque va a recibir un Cod_Usuario. No se creo ni se modifico ninguna columna, y el RESTO DE ESTE SCRIPT NO SE EJECUTO. Script detenido.', 16, 1, @MsgTipo, @MsgLargo);
    SET NOEXEC ON;
END
GO

/* Despues de la guarda a proposito: una corrida detenida no llega a imprimir
   "inicio" y se distingue de una normal con solo mirar la salida. */
PRINT '== Datos personales: auditoria de R_Usuarios - inicio ==';
GO

IF OBJECT_ID('dbo.R_Usuarios','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('dbo.R_Usuarios') AND name = 'Usu_ModificacionCod')
    BEGIN
        ALTER TABLE dbo.R_Usuarios ADD Usu_ModificacionCod VARCHAR(50) NULL;
        PRINT 'R_Usuarios.Usu_ModificacionCod creada.';
    END
    ELSE PRINT 'R_Usuarios.Usu_ModificacionCod ya existia.';

    IF NOT EXISTS (SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('dbo.R_Usuarios') AND name = 'Fec_Modificacion')
    BEGIN
        ALTER TABLE dbo.R_Usuarios ADD Fec_Modificacion DATETIME NULL;
        PRINT 'R_Usuarios.Fec_Modificacion creada.';
    END
    ELSE PRINT 'R_Usuarios.Fec_Modificacion ya existia.';

    IF NOT EXISTS (SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('dbo.R_Usuarios') AND name = 'Ip_Modificacion')
    BEGIN
        ALTER TABLE dbo.R_Usuarios ADD Ip_Modificacion VARCHAR(64) NULL;
        PRINT 'R_Usuarios.Ip_Modificacion creada.';
    END
    ELSE PRINT 'R_Usuarios.Ip_Modificacion ya existia.';
END
ELSE PRINT 'dbo.R_Usuarios no existe. Omitido.';
GO

/* Incondicional: si la guarda encendio NOEXEC, apagarlo aca evita que el resto
   de la sesion de SSMS quede sin ejecutar nada y parezca que los scripts
   siguientes "no hacen nada". */
SET NOEXEC OFF;
GO

PRINT '== Datos personales: auditoria de R_Usuarios - fin ==';
GO

/* ============================================================================
   VERIFICACION (correr a mano despues del script)

   Esperado: 3 filas, todas admiteNulo = 1.

SELECT  columna = c.name, tipo = t.name, largo = c.max_length,
        admiteNulo = c.is_nullable
  FROM  sys.columns c
  JOIN  sys.types  t ON t.user_type_id = c.user_type_id
 WHERE  c.object_id = OBJECT_ID('dbo.R_Usuarios')
   AND  c.name IN ('Usu_ModificacionCod','Fec_Modificacion','Ip_Modificacion');

   ============================================================================ */
```

- [ ] **Step 2: Verificar la forma del archivo sin ejecutarlo**

No se conecta a la base. Se comprueba que el archivo esté bien formado:

```bash
python -c "
import io
raw = io.open('docs/sql/2026-09-17-datos-personales-columnas.sql','rb').read()
cuerpo = raw[3:] if raw[:3]==b'\xef\xbb\xbf' else raw
print('BOM:', raw[:3]==b'\xef\xbb\xbf')
print('no-ascii tras BOM:', sorted(set(b for b in bytearray(cuerpo) if b>127)) or 'ninguno')
s = raw.decode('utf-8')
print('NOEXEC ON:', s.count('SET NOEXEC ON'), 'NOEXEC OFF:', s.count('SET NOEXEC OFF'))
print('ALTER TABLE:', s.count('ALTER TABLE'))
print('comentarios balanceados:', s.count('/*') == s.count('*/'))
"
```

Esperado: `BOM: True`, `no-ascii tras BOM: ninguno`, `NOEXEC ON: 2`,
`NOEXEC OFF: 1`, `ALTER TABLE: 3`, `comentarios balanceados: True`.

> `NOEXEC ON: 2` es correcto y no un descuido: una de las dos es la mención
> dentro del comentario de la guarda. Ejecutable hay una sola. Comprobarlo con
> `grep -n "SET NOEXEC" docs/sql/2026-09-17-datos-personales-columnas.sql`: la
> que está dentro de `/* ... */` no cuenta.

- [ ] **Step 3: Commit**

```bash
git add docs/sql/2026-09-17-datos-personales-columnas.sql
git commit -m "feat(perfil): R_Usuarios no tenia donde anotar quien la modifico"
```

---

## Task 2: El dígito verificador de la cédula

**Files:**
- Create: `CapaNegocio/NegPerfilCedula.cs`
- Create: `CapaPruebas/NegPerfilCedulaTests.cs`
- Modify: `CapaNegocio/CapaNegocio.csproj`, `CapaPruebas/CapaPruebas.csproj`

**Interfaces:**
- Consumes: nada. La clase no depende de nada del proyecto.
- Produces: `public static bool CapaNegocio.NegPerfilCedula.EsValida(string cedula)`.
  La Task 3 la llama desde `ValidarDatosPersonales`.

**Esta función no existe en el repositorio.** Lo único parecido es
`cedula_valida()` en `docs/sql/generar-carga-horas-extras.py`, un generador de un
solo uso en Python que la aplicación no puede llamar. Se escribe de cero, en su
propio archivo porque es un algoritmo cerrado y sin dependencias, y porque
`NegPerfilCampos.cs` ya tiene 637 líneas.

> **Los números de las pruebas ya están calculados.** Las cinco cédulas de abajo
> son inventadas y se construyeron con el algoritmo de referencia
> (`cedula_valida()` en `docs/sql/generar-carga-horas-extras.py`) para que den
> exactamente el dígito que la prueba afirma. Si alguna vez hiciera falta
> comprobarlo:
>
> ```bash
> python -c "
> def d(c):
>     s=0
>     for i,ch in enumerate(c[:9]):
>         n=int(ch)*(2 if i%2==0 else 1)
>         s += n-9 if n>9 else n
>     return (10-(s%10))%10
> for c in ['171003406','301003406','176003406','001003406','251003406']:
>     print(c + str(d(c)))
> "
> ```
>
> Imprime `1710034065`, `3010034068`, `1760034064`, `0010034064` y `2510034065`.

- [ ] **Step 1: Escribir las pruebas que fallan**

Crear `CapaPruebas/NegPerfilCedulaTests.cs`:

```csharp
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CapaPruebas
{
    /// <summary>
    /// El digito verificador. Una cedula mal escrita que pase por buena rompe
    /// el enlace entre R_Usuarios y Empleados, que es por donde el modulo de
    /// horas extras identifica a la gente.
    ///
    /// Las cedulas de estas pruebas son inventadas y se construyeron para que
    /// den el digito que dicen. El repositorio es publico: no se usa ninguna
    /// cedula real.
    /// </summary>
    [TestClass]
    public class NegPerfilCedulaTests
    {
        /* Prefijo 171003406 + su verificador. Provincia 17 (Pichincha), tercer
           digito 1: persona natural. */
        private const string CedulaValida = "1710034065";

        [TestMethod]
        public void EsValida_ConDigitoCorrecto_DevuelveTrue()
        {
            Assert.IsTrue(NegPerfilCedula.EsValida(CedulaValida));
        }

        [TestMethod]
        public void EsValida_ConElUltimoDigitoCambiado_DevuelveFalse()
        {
            // La misma de arriba con el verificador corrido en uno.
            string rota = CedulaValida.Substring(0, 9)
                        + ((CedulaValida[9] - '0' + 1) % 10).ToString();

            Assert.IsFalse(NegPerfilCedula.EsValida(rota));
        }

        [TestMethod]
        public void EsValida_ConNueveDigitos_DevuelveFalse()
        {
            Assert.IsFalse(NegPerfilCedula.EsValida("171003406"));
        }

        [TestMethod]
        public void EsValida_ConOnceDigitos_DevuelveFalse()
        {
            // Hay una asi en produccion hoy. Ver la seccion 5 del diseno.
            Assert.IsFalse(NegPerfilCedula.EsValida(CedulaValida + "1"));
        }

        [TestMethod]
        public void EsValida_ConLetras_DevuelveFalse()
        {
            Assert.IsFalse(NegPerfilCedula.EsValida("17100A406" + CedulaValida[9]));
        }

        [TestMethod]
        public void EsValida_ConProvinciaCero_DevuelveFalse()
        {
            // Prefijo 001003406: provincia 00, que no existe.
            Assert.IsFalse(NegPerfilCedula.EsValida("0010034064"));
        }

        [TestMethod]
        public void EsValida_ConProvinciaMayorQue24YDistintaDe30_DevuelveFalse()
        {
            // Prefijo 251003406: provincia 25, que no existe.
            Assert.IsFalse(NegPerfilCedula.EsValida("2510034065"));
        }

        [TestMethod]
        public void EsValida_ConProvincia30_DevuelveTrue()
        {
            // 30 es el codigo de los ecuatorianos nacidos en el exterior.
            Assert.IsTrue(NegPerfilCedula.EsValida("3010034068"));
        }

        [TestMethod]
        public void EsValida_ConTercerDigitoSeisOMas_DevuelveFalse()
        {
            // Prefijo 176003406: tercer digito 6, que no es persona natural.
            // Hay dos asi en produccion hoy, con el tercer digito en 9.
            Assert.IsFalse(NegPerfilCedula.EsValida("1760034064"));
        }

        [TestMethod]
        public void EsValida_ConEspaciosAlrededor_LosIgnora()
        {
            Assert.IsTrue(NegPerfilCedula.EsValida("  " + CedulaValida + "  "));
        }

        [TestMethod]
        public void EsValida_ConNulo_DevuelveFalse()
        {
            Assert.IsFalse(NegPerfilCedula.EsValida(null));
        }

        [TestMethod]
        public void EsValida_ConVacio_DevuelveFalse()
        {
            // Vacio no es valido: quien decide si una cedula vacia se acepta es
            // NegPerfilCampos.ValidarDatosPersonales, no esta funcion.
            Assert.IsFalse(NegPerfilCedula.EsValida("   "));
        }
    }
}
```

- [ ] **Step 2: Compilar para verlas fallar**

```bash
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
```

Esperado: **falla la compilación** con `CS0103: El nombre 'NegPerfilCedula' no
existe en el contexto actual`. Ese es el rojo de esta etapa.

- [ ] **Step 3: Escribir la implementación**

Crear `CapaNegocio/NegPerfilCedula.cs`:

```csharp
namespace CapaNegocio
{
    /// <summary>
    /// El digito verificador de la cedula ecuatoriana.
    ///
    /// Vive en su propio archivo y no en NegPerfilCampos porque no depende de
    /// nada -ni de entidades, ni de la base, ni de HttpContext- y porque es el
    /// unico pedazo de este modulo que tiene una respuesta correcta conocida de
    /// antemano para cualquier entrada.
    ///
    /// El algoritmo es el mismo de cedula_valida() en
    /// docs/sql/generar-carga-horas-extras.py, que es un generador de un solo
    /// uso en Python y que la aplicacion no puede llamar. Esto NO es una
    /// reutilizacion: es la primera vez que la comprobacion existe en C#.
    /// </summary>
    public static class NegPerfilCedula
    {
        /// <summary>Codigo de provincia mas alto que existe.</summary>
        private const int ProvinciaMaxima = 24;

        /// <summary>Los ecuatorianos nacidos en el exterior llevan 30.</summary>
        private const int ProvinciaExterior = 30;

        /// <summary>
        /// true si el texto es una cedula ecuatoriana bien formada y con digito
        /// verificador correcto. Vacio, nulo y cualquier otra cosa dan false;
        /// quien decide si una cedula vacia se acepta es
        /// NegPerfilCampos.ValidarDatosPersonales, no esta funcion.
        /// </summary>
        public static bool EsValida(string cedula)
        {
            if (cedula == null) { return false; }

            string c = cedula.Trim();

            if (c.Length != 10) { return false; }

            foreach (char ch in c)
            {
                if (ch < '0' || ch > '9') { return false; }
            }

            int provincia = int.Parse(c.Substring(0, 2));
            if (!((provincia >= 1 && provincia <= ProvinciaMaxima) || provincia == ProvinciaExterior))
            {
                return false;
            }

            /* El tercer digito dice de que tipo es el documento: menor que 6 es
               una persona natural. 6 y 9 son entidades publicas y juridicas, que
               no son cedulas aunque tengan diez digitos. */
            if (c[2] - '0' >= 6) { return false; }

            /* Coeficientes 2 y 1 alternados sobre los nueve primeros digitos.
               Un producto de dos cifras se reduce restandole 9, que es lo mismo
               que sumar sus dos digitos. */
            int suma = 0;
            for (int i = 0; i < 9; i++)
            {
                int n = (c[i] - '0') * (i % 2 == 0 ? 2 : 1);
                suma += n > 9 ? n - 9 : n;
            }

            int verificador = (10 - (suma % 10)) % 10;

            return verificador == c[9] - '0';
        }
    }
}
```

- [ ] **Step 4: Registrar los dos archivos en sus `.csproj`**

En `CapaNegocio/CapaNegocio.csproj`, junto a las otras líneas `NegPerfil*`
(alrededor de la línea 208), en orden alfabético:

```xml
    <Compile Include="NegPerfilCampos.cs" />
    <Compile Include="NegPerfilCedula.cs" />
```

En `CapaPruebas/CapaPruebas.csproj`, junto a `NegPerfilCamposTests.cs`
(alrededor de la línea 49):

```xml
    <Compile Include="NegPerfilCamposTests.cs" />
    <Compile Include="NegPerfilCedulaTests.cs" />
```

- [ ] **Step 5: Compilar y correr las pruebas**

```bash
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Esperado: las 12 pruebas nuevas en verde y **ninguna de las anteriores en rojo**.

- [ ] **Step 6: Commit**

```bash
git add CapaNegocio/NegPerfilCedula.cs CapaNegocio/CapaNegocio.csproj CapaPruebas/NegPerfilCedulaTests.cs CapaPruebas/CapaPruebas.csproj
git commit -m "feat(perfil): el digito verificador de la cedula, por primera vez en C#"
```

---

## Task 3: Leer y validar los ocho campos

**Files:**
- Create: `CapaEntidad/EntPerfilDatosPersonales.cs`
- Modify: `CapaEntidad/EntPerfilCabecera.cs`
- Modify: `CapaEntidad/CapaEntidad.csproj`
- Modify: `CapaNegocio/NegPerfilCampos.cs`
- Modify: `CapaPruebas/NegPerfilCamposTests.cs`

**Interfaces:**
- Consumes: `NegPerfilCedula.EsValida(string)` de la Task 2.
- Produces:
  - `CapaEntidad.EntPerfilDatosPersonales` con ocho propiedades `string`, todas
    inicializadas en `""`: `Nombre`, `Cedula`, `FechaNacimiento`, `Cargo`,
    `Area`, `Ciudad`, `CodJefeInmediato`, `CorreoNotificacion`.
  - `EntPerfilCabecera.CodJefeInmediato` (`string`, inicializada en `""`).
  - `public static EntPerfilDatosPersonales NegPerfilCampos.LeerDatosPersonales(IDictionary<string, object> campos)`
  - `public static string NegPerfilCampos.ValidarDatosPersonales(EntPerfilDatosPersonales enviado, EntPerfilCabecera actual, string codUsuario)`
    — cadena vacía si sirve; si no, el mensaje para el usuario.
  La Task 6 llena `CodJefeInmediato` desde la base y la Task 7 llama a las dos
  funciones.

**La regla que da forma a esta tarea:** un campo **sólo se valida si cambió**
respecto de lo que está guardado. Tres personas reales tienen hoy una cédula que
no pasa el dígito verificador; si se validara siempre, Talento Humano no podría
guardarles **ningún** campo. Por eso `ValidarDatosPersonales` recibe la cabecera
actual: para comparar.

- [ ] **Step 1: Crear la entidad y agregar `CodJefeInmediato`**

`CapaEntidad/EntPerfilDatosPersonales.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>
    /// Los ocho campos de la pestana "Datos personales" que Talento Humano
    /// puede editar. El horario no esta: sale de R_UsuarioHorarioLaboral, tiene
    /// su propio modulo y queda de solo lectura.
    ///
    /// Todas arrancan en "" y no en null, igual que EntPerfilCabecera: asi un
    /// payload al que le falte una clave no deja propiedades nulas que revienten
    /// en el primer .Trim().
    ///
    /// CodJefeInmediato guarda un Cod_Usuario, no un nombre. La pantalla muestra
    /// el nombre y envia el codigo; ver la seccion 3 del diseno.
    /// </summary>
    public class EntPerfilDatosPersonales
    {
        public string Nombre { get; set; } = "";
        public string Cedula { get; set; } = "";

        /// <summary>Texto dd/MM/yyyy, igual que Empleados.Fecha_nacimiento.</summary>
        public string FechaNacimiento { get; set; } = "";

        public string Cargo { get; set; } = "";
        public string Area { get; set; } = "";
        public string Ciudad { get; set; } = "";
        public string CodJefeInmediato { get; set; } = "";
        public string CorreoNotificacion { get; set; } = "";
    }
}
```

En `CapaEntidad/EntPerfilCabecera.cs`, junto a `JefeInmediato`:

```csharp
        /// <summary>
        /// El Cod_Usuario del jefe, no su nombre. JefeInmediato guarda el nombre
        /// y sirve para mostrar; este sirve para preseleccionar el combo de la
        /// edicion y para comparar al validar.
        /// </summary>
        public string CodJefeInmediato { get; set; } = "";
```

Registrar la entidad nueva en `CapaEntidad/CapaEntidad.csproj`, junto a las otras
`EntPerfil*`, en orden alfabético:

```xml
    <Compile Include="EntPerfilDatosPersonales.cs" />
```

- [ ] **Step 2: Escribir las pruebas que fallan**

Agregar dentro de la clase `NegPerfilCamposTests`, **antes de su llave de
cierre**. Comprobar que el archivo trae `using System;`,
`using System.Collections.Generic;`, `using System.Globalization;` y
`using CapaEntidad;`; agregar los que falten.

```csharp
        /* ----------------------------------------- datos personales ------- */

        /// <summary>Los valores "ya guardados" contra los que se compara.</summary>
        private static EntPerfilCabecera CabeceraGuardada()
        {
            return new EntPerfilCabecera
            {
                CodUsuario         = "USR001",
                NombreCompleto     = "Nombre Guardado",
                Cedula             = "1710034065",
                FechaNacTexto      = "03/07/1985",
                Cargo              = "Analista",
                Area               = "Sistemas",
                Ciudad             = "Quito",
                CodJefeInmediato   = "USR002",
                CorreoNotificacion = "guardado@dos.com.ec"
            };
        }

        /// <summary>Lo mismo que hay guardado: nada cambio.</summary>
        private static EntPerfilDatosPersonales DatosSinCambios()
        {
            return new EntPerfilDatosPersonales
            {
                Nombre             = "Nombre Guardado",
                Cedula             = "1710034065",
                FechaNacimiento    = "03/07/1985",
                Cargo              = "Analista",
                Area               = "Sistemas",
                Ciudad             = "Quito",
                CodJefeInmediato   = "USR002",
                CorreoNotificacion = "guardado@dos.com.ec"
            };
        }

        [TestMethod]
        public void LeerDatosPersonales_LeeLasOchoClaves()
        {
            var campos = new Dictionary<string, object>
            {
                { "nombre", " Ana Perez " },
                { "cedula", " 1710034065 " },
                { "fechaNacimiento", "03/07/1985" },
                { "cargo", "Analista" },
                { "area", "Sistemas" },
                { "ciudad", "Quito" },
                { "codJefeInmediato", "USR002" },
                { "correoNotificacion", "ana@dos.com.ec" }
            };

            EntPerfilDatosPersonales d = NegPerfilCampos.LeerDatosPersonales(campos);

            Assert.AreEqual("Ana Perez", d.Nombre);
            Assert.AreEqual("1710034065", d.Cedula);
            Assert.AreEqual("03/07/1985", d.FechaNacimiento);
            Assert.AreEqual("Analista", d.Cargo);
            Assert.AreEqual("Sistemas", d.Area);
            Assert.AreEqual("Quito", d.Ciudad);
            Assert.AreEqual("USR002", d.CodJefeInmediato);
            Assert.AreEqual("ana@dos.com.ec", d.CorreoNotificacion);
        }

        /// <summary>
        /// La lista blanca filtra CLAVES: una clave que no esta escrita en
        /// LeerDatosPersonales no tiene propiedad donde aterrizar.
        /// </summary>
        [TestMethod]
        public void LeerDatosPersonales_IgnoraClavesAjenas()
        {
            var campos = new Dictionary<string, object>
            {
                { "nombre", "Ana Perez" },
                { "estado", "Inactivo" },
                { "rolUsuario", "1" }
            };

            EntPerfilDatosPersonales d = NegPerfilCampos.LeerDatosPersonales(campos);

            Assert.AreEqual("Ana Perez", d.Nombre);
            Assert.AreEqual("", d.Cedula);
            Assert.AreEqual("", d.CodJefeInmediato);
        }

        [TestMethod]
        public void LeerDatosPersonales_ConNulo_DevuelveTodoVacio()
        {
            EntPerfilDatosPersonales d = NegPerfilCampos.LeerDatosPersonales(null);

            Assert.AreEqual("", d.Nombre);
            Assert.AreEqual("", d.CorreoNotificacion);
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConTodoIgualALoGuardado_NoSeQueja()
        {
            Assert.AreEqual("", NegPerfilCampos.ValidarDatosPersonales(
                DatosSinCambios(), CabeceraGuardada(), "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_SinNombre_SeQueja()
        {
            var d = DatosSinCambios();
            d.Nombre = "   ";

            Assert.AreNotEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConNombreDeMasDe100_SeQueja()
        {
            var d = DatosSinCambios();
            d.Nombre = new string('A', 101);

            Assert.AreNotEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConNombreDe100Exactos_NoSeQueja()
        {
            var d = DatosSinCambios();
            d.Nombre = new string('A', 100);

            Assert.AreEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "USR001"));
        }

        /// <summary>
        /// El caso que motiva la regla de "solo si cambio": hay tres personas en
        /// produccion cuya cedula guardada no pasa el digito verificador. Si se
        /// validara siempre, no se les podria guardar ningun campo.
        /// </summary>
        [TestMethod]
        public void ValidarDatosPersonales_ConCedulaGuardadaInvalidaQueNoCambia_NoSeQueja()
        {
            var actual = CabeceraGuardada();
            actual.Cedula = "17100340651";        // once digitos, como en produccion

            var d = DatosSinCambios();
            d.Cedula = "17100340651";             // la misma: no cambio

            Assert.AreEqual("", NegPerfilCampos.ValidarDatosPersonales(d, actual, "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConCedulaNuevaInvalida_SeQueja()
        {
            var d = DatosSinCambios();
            d.Cedula = "1710034066";              // verificador equivocado

            Assert.AreNotEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConCedulaVaciaQueNoCambia_NoSeQueja()
        {
            var actual = CabeceraGuardada();
            actual.Cedula = "";

            var d = DatosSinCambios();
            d.Cedula = "";

            Assert.AreEqual("", NegPerfilCampos.ValidarDatosPersonales(d, actual, "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConCedulaDeMasDe32_SeQueja()
        {
            var d = DatosSinCambios();
            d.Cedula = new string('1', 33);

            Assert.AreNotEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConFechaDeNacimientoQueNoEsFecha_SeQueja()
        {
            var d = DatosSinCambios();
            d.FechaNacimiento = "31/02/1985";

            Assert.AreNotEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "USR001"));
        }

        /// <summary>
        /// La cultura hostil de esta clase es en-US, donde "25/12/1985" no es una
        /// fecha. Tiene que aceptarse igual: el formato va explicito.
        /// </summary>
        [TestMethod]
        public void ValidarDatosPersonales_ConDiaMayorQue12_NoSeQueja()
        {
            var d = DatosSinCambios();
            d.FechaNacimiento = "25/12/1985";

            Assert.AreEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConFechaDeNacimientoVacia_NoSeQueja()
        {
            var d = DatosSinCambios();
            d.FechaNacimiento = "";

            Assert.AreEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConFechaDeNacimientoFutura_SeQueja()
        {
            var d = DatosSinCambios();
            d.FechaNacimiento = DateTime.Today.AddDays(1)
                                    .ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            Assert.AreNotEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConCorreoNuevoSinArroba_SeQueja()
        {
            var d = DatosSinCambios();
            d.CorreoNotificacion = "ana.dos.com.ec";

            Assert.AreNotEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConCorreoGuardadoInvalidoQueNoCambia_NoSeQueja()
        {
            var actual = CabeceraGuardada();
            actual.CorreoNotificacion = "sin-arroba";

            var d = DatosSinCambios();
            d.CorreoNotificacion = "sin-arroba";

            Assert.AreEqual("", NegPerfilCampos.ValidarDatosPersonales(d, actual, "USR001"));
        }

        /// <summary>
        /// Nadie puede ser su propio jefe: quedaria sin quien le apruebe nada y
        /// la consulta de equipo lo devolveria como subordinado de si mismo.
        /// </summary>
        [TestMethod]
        public void ValidarDatosPersonales_ConJefeIgualAUnoMismo_SeQueja()
        {
            var d = DatosSinCambios();
            d.CodJefeInmediato = "USR001";

            Assert.AreNotEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConJefeIgualAUnoMismoConEspacios_SeQueja()
        {
            var d = DatosSinCambios();
            d.CodJefeInmediato = "  USR001  ";

            Assert.AreNotEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "  USR001 "));
        }

        [TestMethod]
        public void ValidarDatosPersonales_SinJefe_NoSeQueja()
        {
            var d = DatosSinCambios();
            d.CodJefeInmediato = "";

            Assert.AreEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConCargoDeMasDe128_SeQueja()
        {
            var d = DatosSinCambios();
            d.Cargo = new string('A', 129);

            Assert.AreNotEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConAreaDeMasDe128_SeQueja()
        {
            var d = DatosSinCambios();
            d.Area = new string('A', 129);

            Assert.AreNotEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConCiudadDeMasDe150_SeQueja()
        {
            var d = DatosSinCambios();
            d.Ciudad = new string('A', 151);

            Assert.AreNotEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConCorreoDeMasDe100_SeQueja()
        {
            var d = DatosSinCambios();
            d.CorreoNotificacion = new string('a', 95) + "@dos.ec";

            Assert.AreNotEqual("", NegPerfilCampos.ValidarDatosPersonales(
                d, CabeceraGuardada(), "USR001"));
        }

        [TestMethod]
        public void ValidarDatosPersonales_ConDatosNulos_SeQueja()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarDatosPersonales(
                null, CabeceraGuardada(), "USR001"));
        }

        /// <summary>
        /// Sin cabecera no hay con que comparar, y validar contra nada seria
        /// validarlo todo: justo lo que traba a las tres personas con cedula
        /// vieja invalida. Se rechaza en vez de adivinar.
        /// </summary>
        [TestMethod]
        public void ValidarDatosPersonales_SinCabeceraActual_SeQueja()
        {
            Assert.AreNotEqual("", NegPerfilCampos.ValidarDatosPersonales(
                DatosSinCambios(), null, "USR001"));
        }
```

- [ ] **Step 3: Compilar para verlas fallar**

```bash
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
```

Esperado: **falla la compilación** con `CS0117: 'NegPerfilCampos' no contiene una
definición para 'LeerDatosPersonales'`.

- [ ] **Step 4: Escribir la implementación**

En `CapaNegocio/NegPerfilCampos.cs`, después de `ValidarContacto`:

```csharp
        /* ------------------------------------------- datos personales ------ */

        /* Los anchos son los de la columna MAS ANGOSTA de las dos tablas donde
           cae cada campo, no los de la mas ancha: un valor que entra en
           Empleados.Nombre nvarchar(350) pero no en R_Usuarios.Nom_Usuario
           varchar(100) no deja el guardado a medias, lo hace fallar entero con
           "String or binary data would be truncated". Medidos contra produccion
           el 2026-09-17; estan en la seccion 2 del diseno. */
        private const int LargoMaximoNombre = 100;
        private const int LargoMaximoCedula = 32;
        private const int LargoMaximoCargo  = 128;
        private const int LargoMaximoArea   = 128;
        private const int LargoMaximoCiudad = 150;
        private const int LargoMaximoCorreo = 100;

        /// <summary>
        /// Arma los datos personales editables a partir del payload, leyendo
        /// solo las ocho claves que este codigo esta escrito para leer.
        ///
        /// La lista blanca es la forma de este codigo, no un arreglo que se
        /// recorra: una clave como "estado" o "rolUsuario" no se cuela porque no
        /// hay linea que la pida ni propiedad que la reciba. El horario no esta
        /// a proposito: es de solo lectura y tiene su propio modulo.
        /// </summary>
        public static EntPerfilDatosPersonales LeerDatosPersonales(IDictionary<string, object> campos)
        {
            return new EntPerfilDatosPersonales
            {
                Nombre             = Texto(campos, "nombre"),
                Cedula             = Texto(campos, "cedula"),
                FechaNacimiento    = Texto(campos, "fechaNacimiento"),
                Cargo              = Texto(campos, "cargo"),
                Area               = Texto(campos, "area"),
                Ciudad             = Texto(campos, "ciudad"),
                CodJefeInmediato   = Texto(campos, "codJefeInmediato"),
                CorreoNotificacion = Texto(campos, "correoNotificacion")
            };
        }

        /// <summary>
        /// Cadena vacia si los datos personales sirven; si no, el mensaje para el
        /// usuario.
        ///
        /// Recibe la cabecera GUARDADA porque un campo solo se valida cuando
        /// cambio. No es una comodidad: hay tres personas en produccion cuya
        /// cedula no pasa el digito verificador -una de once digitos y dos con el
        /// tercer digito en 9-, y validar siempre dejaria a esas tres sin poder
        /// guardar NINGUN campo, trabadas por un valor que nadie estaba tocando.
        /// Lo mismo vale para el correo.
        ///
        /// codUsuario es de quien es el perfil, y hace falta para la unica regla
        /// que no se puede leer de los campos: que nadie sea su propio jefe.
        /// </summary>
        public static string ValidarDatosPersonales(EntPerfilDatosPersonales enviado,
                                                    EntPerfilCabecera actual,
                                                    string codUsuario)
        {
            if (enviado == null) { return "No se recibieron los datos personales."; }

            /* Sin cabecera no hay contra que comparar, y validarlo todo seria
               justo lo que traba a las tres personas de arriba. Se rechaza en vez
               de adivinar. */
            if (actual == null) { return "No se pudieron leer los datos actuales de la persona."; }

            string nombre = (enviado.Nombre ?? "").Trim();
            if (nombre == "") { return "El nombre es obligatorio."; }
            if (nombre.Length > LargoMaximoNombre)
            {
                return "El nombre no puede pasar de " + LargoMaximoNombre + " caracteres.";
            }

            string cedula = (enviado.Cedula ?? "").Trim();
            if (cedula.Length > LargoMaximoCedula)
            {
                return "La cédula no puede pasar de " + LargoMaximoCedula + " caracteres.";
            }
            if (Cambio(cedula, actual.Cedula) && cedula != "" && !NegPerfilCedula.EsValida(cedula))
            {
                return "La cédula no es válida: revise el número.";
            }

            string fecha = (enviado.FechaNacimiento ?? "").Trim();
            if (fecha != "")
            {
                DateTime nacimiento;
                bool valida = DateTime.TryParseExact(fecha, FormatoFecha,
                                                     CultureInfo.InvariantCulture,
                                                     DateTimeStyles.None, out nacimiento);
                if (!valida)
                {
                    return "La fecha de nacimiento tiene que estar en formato dd/mm/aaaa.";
                }
                if (nacimiento.Date > DateTime.Today)
                {
                    return "La fecha de nacimiento no puede ser futura.";
                }
            }

            if ((enviado.Cargo ?? "").Trim().Length > LargoMaximoCargo)
            {
                return "El cargo no puede pasar de " + LargoMaximoCargo + " caracteres.";
            }

            if ((enviado.Area ?? "").Trim().Length > LargoMaximoArea)
            {
                return "El área no puede pasar de " + LargoMaximoArea + " caracteres.";
            }

            if ((enviado.Ciudad ?? "").Trim().Length > LargoMaximoCiudad)
            {
                return "La ciudad no puede pasar de " + LargoMaximoCiudad + " caracteres.";
            }

            /* Que el jefe exista y este activo lo comprueba el procedimiento, que
               es el unico que puede mirar la base. Aca va lo que no necesita
               mirarla: que no sea uno mismo. */
            string jefe = (enviado.CodJefeInmediato ?? "").Trim();
            if (jefe != "" && string.Equals(jefe, (codUsuario ?? "").Trim(),
                                            StringComparison.OrdinalIgnoreCase))
            {
                return "Una persona no puede ser su propio jefe inmediato.";
            }

            string correo = (enviado.CorreoNotificacion ?? "").Trim();
            if (correo.Length > LargoMaximoCorreo)
            {
                return "El correo no puede pasar de " + LargoMaximoCorreo + " caracteres.";
            }
            if (Cambio(correo, actual.CorreoNotificacion) && correo != ""
                && !CorreoBienFormado(correo))
            {
                return "El correo de notificación no es válido.";
            }

            return "";
        }

        /// <summary>
        /// true si el valor enviado difiere del guardado. Compara recortado y
        /// tratando null como cadena vacia; distingue mayusculas, porque
        /// cambiarle la capitalizacion a un correo SI es un cambio.
        /// </summary>
        private static bool Cambio(string enviado, string guardado)
        {
            return (enviado ?? "").Trim() != (guardado ?? "").Trim();
        }

        /// <summary>
        /// Misma comprobacion que ValidarContacto le hace al correo personal:
        /// algo antes de la arroba, algo despues, y un punto despues de la
        /// arroba.
        /// </summary>
        private static bool CorreoBienFormado(string correo)
        {
            int arroba = correo.IndexOf('@');

            bool tieneAlgoAntes    = arroba > 0;
            bool tieneAlgoDespues  = arroba >= 0 && arroba < correo.Length - 1;
            bool tienePuntoDespues = arroba >= 0 && correo.IndexOf('.', arroba + 1) > arroba;

            return tieneAlgoAntes && tieneAlgoDespues && tienePuntoDespues;
        }
```

- [ ] **Step 5: Compilar y correr las pruebas**

```bash
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Esperado: todo en verde, incluidas las pruebas anteriores del módulo.

- [ ] **Step 6: Commit**

```bash
git add CapaEntidad/EntPerfilDatosPersonales.cs CapaEntidad/EntPerfilCabecera.cs CapaEntidad/CapaEntidad.csproj CapaNegocio/NegPerfilCampos.cs CapaPruebas/NegPerfilCamposTests.cs
git commit -m "feat(perfil): validar los ocho campos, y solo el que cambio"
```

---

## Task 4: El procedimiento que guarda

**Files:**
- Create: `docs/sql/2026-09-17-datos-personales-procedimientos.sql`

**Interfaces:**
- Consumes: `R_Usuarios.Usu_ModificacionCod/Fec_Modificacion/Ip_Modificacion` de
  la Task 1; `Empleados.Usu_ModificacionCod` de la entrega 1.
- Produces: `dbo.Sp_RTA_PerfilGuardarDatosPersonales` y
  `dbo.Sp_RTA_PerfilJefesLista`, con los parámetros que usa la Task 6.
  `Respuestas`: `0` guardado sin avisos; `1`, `2` y `3` guardado **con** aviso
  (correo repetido, cédula repetida, las dos); `-2` `Cod_Usuario` repetido; `-3`
  el jefe elegido no existe o no está activo. Los positivos guardaron, los
  negativos no.

**La regla que da forma a este procedimiento:** antes de crear una ficha hay que
**buscar una huérfana con la misma cédula y adoptarla**. `Empleados` tiene 133
filas y sólo 112 llevan `Cod_Usuario`; de las 21 huérfanas, **4 son de gente que
la pantalla da por «sin ficha»**. Insertar sin mirar deja a esas 4 personas con
dos fichas en `RRHHEmpleados.aspx` y nada lo advierte.

- [ ] **Step 1: Escribir el script**

Archivo nuevo, **con BOM UTF-8**, comentarios sin tildes:

```sql
/* ============================================================================
   Datos personales editables: el guardado
   ReporTarea  |  2026-09-17

   PENDIENTE DE EJECUTAR. Corre DESPUES de
   docs/sql/2026-09-17-datos-personales-columnas.sql.

   ----------------------------------------------------------------------------
   Escribe los ocho campos en las DOS tablas. No es duplicacion: la cabecera de
   Sp_RTA_PerfilColaborador lee con respaldo y la precedencia no es la misma para
   todos los campos -Nombre y Cargo los manda Empleados, Cedula la manda
   R_Usuarios-, asi que escribir en un solo lado deja el otro viejo y la pantalla
   muestra el valor anterior despues de guardar.

   Idempotente: DROP y CREATE.
   ============================================================================ */

USE [ReporTarea];
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* Las columnas de autor tienen que existir antes: estos procedimientos escriben
   en ellas y el CREATE fallaria al compilarse.

   NOEXEC y no RETURN: RETURN fuera de un procedimiento sale del LOTE, no del
   script, y despues del GO los CREATE se intentarian igual. Se apaga al final
   del archivo. */
IF  NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.R_Usuarios') AND name='Usu_ModificacionCod')
 OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.R_Usuarios') AND name='Fec_Modificacion')
 OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.R_Usuarios') AND name='Ip_Modificacion')
BEGIN
    RAISERROR('Faltan columnas de auditoria en R_Usuarios: correr primero 2026-09-17-datos-personales-columnas.sql. Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.Empleados') AND name='Usu_ModificacionCod')
BEGIN
    RAISERROR('Falta Empleados.Usu_ModificacionCod: correr primero la entrega 1 (2026-09-16-perfil-autor-columnas.sql). Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO

PRINT '== Datos personales: procedimientos - inicio ==';
GO

/* ====================================================== 1. el guardado ===== */

IF OBJECT_ID('dbo.Sp_RTA_PerfilGuardarDatosPersonales','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilGuardarDatosPersonales;
GO

/* Respuestas:
      0  guardado, sin nada que avisar
      1  guardado, pero el correo ya lo tiene otro usuario activo
      2  guardado, pero la cedula ya la tiene otro usuario activo
      3  guardado, y las dos cosas
     -2  Cod_Usuario repetido entre usuarios activos. Mismo caso que bloquean
         Sp_RTA_PerfilColaborador y las otras escrituras del modulo: con dos
         personas compartiendo codigo, no hay forma de saber a cual escribirle.
     -3  el jefe elegido no existe o no esta activo. Un Cod_Jefe_Inm que no
         apunta a nadie deja las solicitudes de vacaciones de esa persona sin
         aprobador, y nadie se entera hasta que alguien pide vacaciones.

   Los positivos son todos "se guardo": el 1, el 2 y el 3 son avisos, no
   errores. Los negativos son "no se guardo nada".

   Los anchos de los parametros son los de la columna MAS ANGOSTA de las dos
   tablas, los mismos que valida NegPerfilCampos. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilGuardarDatosPersonales
    @Cod_Usuario  VARCHAR(50),
    @Nombre       VARCHAR(100),
    @Cedula       VARCHAR(32),
    @FechaNac     VARCHAR(10),
    @Cargo        VARCHAR(128),
    @Area         VARCHAR(128),
    @Ciudad       VARCHAR(150),
    @CodJefeInm   VARCHAR(100),
    @Correo       VARCHAR(100),
    @Ip           VARCHAR(64),
    @Usu_Accion   VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /* El autor del cambio, no el dueno del perfil. Si no viene -una llamada
       vieja, o un binario sin desplegar- se cae al dueno, que es exactamente lo
       que hacia el modulo antes de la entrega 1. */
    DECLARE @Autor VARCHAR(50);
    SET @Autor = ISNULL(NULLIF(LTRIM(RTRIM(@Usu_Accion)), ''), @Cod_Usuario);

    DECLARE @Jefe VARCHAR(100);
    SET @Jefe = LTRIM(RTRIM(ISNULL(@CodJefeInm, '')));

    IF (SELECT COUNT(*) FROM dbo.R_Usuarios
         WHERE Cod_Usuario = @Cod_Usuario AND ISNULL(EstadoUsuario, 0) = 0) <> 1
    BEGIN
        SELECT Respuestas = -2;
        RETURN;
    END

    IF @Jefe <> ''
    BEGIN
        /* Que no sea uno mismo tambien lo comprueba NegPerfilCampos, con
           pruebas. Se repite aca porque este procedimiento es llamable desde
           SSMS y desde cualquier codigo futuro, y porque el dano -una persona
           que se aprueba sus propias vacaciones- no se nota hasta que ya paso. */
        IF @Jefe = LTRIM(RTRIM(@Cod_Usuario))
        BEGIN
            SELECT Respuestas = -3;
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM dbo.R_Usuarios
                        WHERE LTRIM(RTRIM(Cod_Usuario)) = @Jefe
                          AND ISNULL(EstadoUsuario, 0) = 0)
        BEGIN
            SELECT Respuestas = -3;
            RETURN;
        END
    END

    BEGIN TRANSACTION;

    /* --- R_Usuarios ------------------------------------------------------
       Nom_Usuario es NOT NULL: por eso el nombre es el unico campo obligatorio
       y NegPerfilCampos lo rechaza vacio antes de llegar aca.

       Cod_Jefe_Inm va a NULL cuando viene vacio y no a cadena vacia: el LEFT
       JOIN de la cabecera y la consulta de equipo comparan contra Cod_Usuario, y
       una cadena vacia no empareja con nadie pero tampoco se lee como "sin
       jefe". */
    UPDATE dbo.R_Usuarios
       SET Nom_Usuario         = @Nombre,
           Cedula              = @Cedula,
           Cargo               = @Cargo,
           Departamento        = @Area,
           Cod_Jefe_Inm        = NULLIF(@Jefe, ''),
           E_Mail              = @Correo,
           Usu_ModificacionCod = @Autor,
           Fec_Modificacion    = GETDATE(),
           Ip_Modificacion     = LEFT(@Ip, 64)
     WHERE Cod_Usuario = @Cod_Usuario;

    /* --- Empleados: primero adoptar, despues crear ------------------------ */
    DECLARE @IdEmpleado BIGINT;

    /* TOP 1 con ORDER BY, igual que la adopcion de mas abajo. Hoy ningun
       Cod_Usuario tiene dos fichas -verificado el 2026-09-17- pero no hay indice
       unico que lo impida, y sin ORDER BY una segunda ficha se elegiria al azar
       y el guardado iria a parar a una u otra sin que nadie se entere. */
    SELECT TOP 1 @IdEmpleado = IdEmpleado
      FROM dbo.Empleados
     WHERE Cod_Usuario = @Cod_Usuario
     ORDER BY IdEmpleado;

    /* La adopcion. Empleados tiene 21 filas sin Cod_Usuario -fichas de gente
       real que nadie enlazo nunca- y 4 de ellas son de personas que esta
       pantalla da por "sin ficha". Sin este SELECT, esas 4 terminan con dos
       fichas en RRHHEmpleados.aspx y ningun mensaje lo advierte.

       Por cedula exacta y sin LIKE: emparejar de mas es peor que no emparejar,
       porque mezcla las fichas de dos personas distintas. TOP 1 con ORDER BY
       para que sea determinista; hoy ninguna cedula huerfana esta repetida. */
    IF @IdEmpleado IS NULL AND LTRIM(RTRIM(ISNULL(@Cedula, ''))) <> ''
    BEGIN
        SELECT TOP 1 @IdEmpleado = IdEmpleado
          FROM dbo.Empleados
         WHERE LTRIM(RTRIM(ISNULL(Cod_Usuario, ''))) = ''
           AND LTRIM(RTRIM(ISNULL(Cedula, '')))      = LTRIM(RTRIM(@Cedula))
         ORDER BY IdEmpleado;
    END

    IF @IdEmpleado IS NULL
    BEGIN
        /* IdEmpleado es IDENTITY y la unica columna NOT NULL de la tabla, asi
           que el INSERT no necesita nada mas. Estado = 'Activo' es el valor que
           usan las 128 fichas vivas; sin el, la ficha nueva no aparece en
           RRHHEmpleados.aspx y el efecto seria justo el contrario del buscado.

           Ip_Modificacion de Empleados es varchar(32), mas angosta que las 64 de
           R_Usuarios: con una IPv6 esto no truncaria en silencio, fallaria con
           "String or binary data would be truncated" y se caeria el guardado
           entero. Por eso LEFT(@Ip, 32). */
        INSERT INTO dbo.Empleados
              (Cod_Usuario, Nombre, Cedula, Fecha_nacimiento, PuestoTrabajo,
               AreaTrabajo, Ciudad, Estado, Fec_Modificacion,
               Usu_ModificacionCod, Ip_Modificacion)
        VALUES (@Cod_Usuario, @Nombre, @Cedula, @FechaNac, @Cargo,
               @Area, @Ciudad, 'Activo', GETDATE(),
               @Autor, LEFT(@Ip, 32));
    END
    ELSE
    BEGIN
        /* Cod_Usuario se escribe tambien en el UPDATE: es lo que completa la
           adopcion de una ficha huerfana. Para una ficha que ya era de esta
           persona, es escribir el mismo valor. */
        UPDATE dbo.Empleados
           SET Cod_Usuario         = @Cod_Usuario,
               Nombre              = @Nombre,
               Cedula              = @Cedula,
               Fecha_nacimiento    = @FechaNac,
               PuestoTrabajo       = @Cargo,
               AreaTrabajo         = @Area,
               Ciudad              = @Ciudad,
               Fec_Modificacion    = GETDATE(),
               Usu_ModificacionCod = @Autor,
               Ip_Modificacion     = LEFT(@Ip, 32)
         WHERE IdEmpleado = @IdEmpleado;
    END

    COMMIT TRANSACTION;

    /* --- Los avisos, despues de guardar ----------------------------------
       Un correo repetido NO impide guardar: el sistema ya convive con correos
       repetidos, y bloquear impediria corregir justamente esos casos. Pero
       tampoco puede pasar en silencio: RTA_CodigoUsuarioPorCorreo busca por
       correo y, ante dos usuarios con el mismo, se queda con el de Id_Usuario
       mas alto. Duplicar un correo no da error, cambia a quien se le atribuye
       una firma.

       Lo mismo con la cedula, que es el puente de identidad entre R_Usuarios y
       Empleados y por donde horas extras enlaza a la gente.

       Se suman como banderas para no necesitar dos viajes: 1 correo, 2 cedula,
       3 los dos. Todos son "guardado", a diferencia de -2 y -3. */
    DECLARE @Aviso INT;
    SET @Aviso = 0;

    IF LTRIM(RTRIM(ISNULL(@Correo, ''))) <> ''
       AND EXISTS (SELECT 1 FROM dbo.R_Usuarios
                    WHERE LTRIM(RTRIM(ISNULL(E_Mail, ''))) = LTRIM(RTRIM(@Correo))
                      AND Cod_Usuario <> @Cod_Usuario
                      AND ISNULL(EstadoUsuario, 0) = 0)
        SET @Aviso = @Aviso + 1;

    IF LTRIM(RTRIM(ISNULL(@Cedula, ''))) <> ''
       AND EXISTS (SELECT 1 FROM dbo.R_Usuarios
                    WHERE LTRIM(RTRIM(ISNULL(Cedula, ''))) = LTRIM(RTRIM(@Cedula))
                      AND Cod_Usuario <> @Cod_Usuario
                      AND ISNULL(EstadoUsuario, 0) = 0)
        SET @Aviso = @Aviso + 2;

    SELECT Respuestas = @Aviso;
END
GO

/* ================================================ 2. la lista de jefes ===== */

IF OBJECT_ID('dbo.Sp_RTA_PerfilJefesLista','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilJefesLista;
GO

/* Los candidatos a jefe inmediato para el combo de la pantalla.

   Mismo filtro que Sp_RTA_PerfilPersonalLista: ISNULL(EstadoUsuario,0) = 0 y sin
   Cod_Usuario repetido. Los repetidos no se ofrecen porque elegir uno dejaria la
   cadena de aprobaciones apuntando a dos personas.

   Se excluye a la persona misma: que nadie sea su propio jefe tambien lo
   comprueban NegPerfilCampos y el guardado, pero no ofrecerlo evita que aparezca
   como una opcion legitima. */
CREATE PROCEDURE dbo.Sp_RTA_PerfilJefesLista
    @Cod_Usuario VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  CodUsuario = LTRIM(RTRIM(u.Cod_Usuario)),
            Nombre     = ISNULL(NULLIF(LTRIM(RTRIM(e.Nombre)), ''), u.Nom_Usuario)
      FROM  dbo.R_Usuarios u
      LEFT JOIN dbo.Empleados e ON e.Cod_Usuario = u.Cod_Usuario
     WHERE  ISNULL(u.EstadoUsuario, 0) = 0
       AND  (SELECT COUNT(*) FROM dbo.R_Usuarios r
              WHERE r.Cod_Usuario = u.Cod_Usuario
                AND ISNULL(r.EstadoUsuario, 0) = 0) = 1
       AND  LTRIM(RTRIM(u.Cod_Usuario)) <> LTRIM(RTRIM(ISNULL(@Cod_Usuario, '')))
     ORDER BY Nombre;
END
GO

SET NOEXEC OFF;
GO

PRINT '== Datos personales: procedimientos - fin ==';
GO

/* ============================================================================
   VERIFICACION (correr a mano despues del script)

   Esperado: 2 filas.

SELECT nombre = name, creado = CONVERT(VARCHAR(20), modify_date, 120)
  FROM sys.procedures
 WHERE name IN ('Sp_RTA_PerfilGuardarDatosPersonales','Sp_RTA_PerfilJefesLista');

   ============================================================================ */
```

- [ ] **Step 2: Verificar la forma del archivo sin ejecutarlo**

```bash
python -c "
import io
raw = io.open('docs/sql/2026-09-17-datos-personales-procedimientos.sql','rb').read()
cuerpo = raw[3:] if raw[:3]==b'\xef\xbb\xbf' else raw
s = raw.decode('utf-8')
print('BOM:', raw[:3]==b'\xef\xbb\xbf')
print('no-ascii tras BOM:', sorted(set(b for b in bytearray(cuerpo) if b>127)) or 'ninguno')
print('CREATE PROCEDURE:', s.count('CREATE PROCEDURE'), '| DROP PROCEDURE:', s.count('DROP PROCEDURE'))
print('BEGIN TRANSACTION:', s.count('BEGIN TRANSACTION'), '| COMMIT:', s.count('COMMIT TRANSACTION'))
print('NOEXEC ON:', s.count('SET NOEXEC ON'), '| NOEXEC OFF:', s.count('SET NOEXEC OFF'))
print('comentarios balanceados:', s.count('/*') == s.count('*/'))
"
```

Esperado: `BOM: True`, `no-ascii tras BOM: ninguno`, `CREATE PROCEDURE: 2`,
`DROP PROCEDURE: 2`, `BEGIN TRANSACTION: 1`, `COMMIT: 1`, `NOEXEC ON: 2`,
`NOEXEC OFF: 1`, `comentarios balanceados: True`.

> Los avisos de correo y cédula repetidos se calculan **después** del
> `COMMIT TRANSACTION`, no dentro: son dos consultas de lectura que no tienen por
> qué mantener abierta una transacción que ya terminó su trabajo.

> `NOEXEC ON: 2` es correcto: son las dos guardas. `NOEXEC OFF: 1` también: se
> apaga una sola vez, al final, sin condición.

- [ ] **Step 3: Commit**

```bash
git add docs/sql/2026-09-17-datos-personales-procedimientos.sql
git commit -m "feat(perfil): guardar los datos personales en las dos tablas, adoptando la ficha huerfana"
```

---

## Task 5: `CodJefeInmediato` en la lectura

**Files:**
- Modify: `docs/sql/2026-09-17-datos-personales-procedimientos.sql`

**Interfaces:**
- Consumes: nada nuevo.
- Produces: `Sp_RTA_PerfilColaborador` devuelve una columna más en su primer
  conjunto de resultados, `CodJefeInmediato`. La Task 6 la lee.

La cabecera devuelve hoy `JefeInmediato`, que es el **nombre**. Para
preseleccionar el combo hace falta el **código**. Es una columna más en un
`SELECT` que ya existe: aditivo, y el `Dao` lee por nombre de columna, así que
ninguna lectura actual se entera.

- [ ] **Step 1: Traer el cuerpo vigente del procedimiento**

`Sp_RTA_PerfilColaborador` **no se parchea**: se vuelve a crear entero, y el
cuerpo tiene que ser el que está corriendo, no el que alguien recuerde.

**Tres scripts del repositorio lo crean**, y sólo uno sirve:

```bash
grep -ln "CREATE PROCEDURE dbo.Sp_RTA_PerfilColaborador" docs/sql/*.sql
```

| Script | ¿Coincide con producción? |
|---|---|
| `2026-09-14-perfil-colaborador.sql` | **no** — 39 líneas de diferencia |
| `2026-09-14-perfil-colaborador-fase2.sql` | **no** — 18 líneas de diferencia |
| `2026-09-15-perfil-colaborador-fase3a.sql` | **sí, exactamente** |

Copiar de **`2026-09-15-perfil-colaborador-fase3a.sql`**, desde el
`CREATE PROCEDURE` hasta el `GO` que lo cierra. Copiar de cualquiera de los otros
dos revierte las fases posteriores en silencio.

(Comprobado el 2026-09-17 contra la definición viva de la base. Si volviera a
haber dudas, la fuente definitiva es `OBJECT_DEFINITION` sobre el procedimiento,
no el repositorio.)

- [ ] **Step 2: Agregar la sección 3 al script de la Task 4**

En `docs/sql/2026-09-17-datos-personales-procedimientos.sql`, **antes de
`SET NOEXEC OFF`**, agregar:

```sql
/* ========================================= 3. la cabecera, con el codigo ==== */

IF OBJECT_ID('dbo.Sp_RTA_PerfilColaborador','P') IS NOT NULL
    DROP PROCEDURE dbo.Sp_RTA_PerfilColaborador;
GO

/* Identico al que esta en produccion salvo UNA linea: CodJefeInmediato en el
   primer SELECT. Es aditivo -el Dao lee por nombre de columna- asi que ninguna
   de las dos pantallas que lo usan se entera del cambio.

   Se vuelve a crear entero y no se parchea porque un ALTER parcial de un
   procedimiento de nueve conjuntos de resultados es justo el tipo de cambio que
   se rompe sin avisar. */
```

y a continuación el `CREATE PROCEDURE` completo del paso 1, con **una sola
línea agregada**, justo después de `JefeInmediato   = j.Nom_Usuario,`:

```sql
            JefeInmediato   = j.Nom_Usuario,

            /* El codigo, ademas del nombre. JefeInmediato sirve para mostrar;
               este sirve para preseleccionar el combo de la edicion y para
               comparar al validar. Sale de u.Cod_Jefe_Inm y no de j, para que un
               Cod_Jefe_Inm que apunta a alguien inactivo -y que por lo tanto no
               empareja en el LEFT JOIN- se siga viendo en vez de aparecer como
               "sin jefe". */
            CodJefeInmediato = LTRIM(RTRIM(ISNULL(u.Cod_Jefe_Inm, ''))),
```

- [ ] **Step 3: Verificar que no se cambió nada más**

```bash
python -c "
import io, re
nuevo = io.open('docs/sql/2026-09-17-datos-personales-procedimientos.sql', encoding='utf-8').read()
viejo = io.open('docs/sql/2026-09-14-perfil-colaborador.sql', encoding='utf-8').read()
def cuerpo(s):
    i = s.find('CREATE PROCEDURE dbo.Sp_RTA_PerfilColaborador')
    if i < 0: return None
    j = s.find('\nGO', i)
    return [l.strip() for l in s[i:j].split('\n') if l.strip()]
a, b = cuerpo(viejo), cuerpo(nuevo)
print('lineas viejo:', len(a), '| nuevo:', len(b), '| diferencia:', len(b)-len(a))
import difflib
for l in difflib.unified_diff(a, b, lineterm='', n=0):
    if l.startswith(('+','-')) and not l.startswith(('+++','---')): print(l)
"
```

Esperado: **sólo líneas `+`**, las del comentario y la de `CodJefeInmediato`.
Ninguna línea `-`. Si aparece alguna `-`, se perdió algo del procedimiento
original: **parar y avisar**.

- [ ] **Step 4: Commit**

```bash
git add docs/sql/2026-09-17-datos-personales-procedimientos.sql
git commit -m "feat(perfil): la cabecera devuelve el codigo del jefe, no solo su nombre"
```

---

## Task 6: Las capas de datos y de negocio

**Files:**
- Create: `CapaEntidad/EntPerfilJefe.cs`
- Modify: `CapaEntidad/CapaEntidad.csproj`
- Modify: `CapaDato/DaoPerfil.cs`
- Modify: `CapaNegocio/NegPerfil.cs`

**Interfaces:**
- Consumes: `EntPerfilDatosPersonales` y `EntPerfilCabecera.CodJefeInmediato`
  (Task 3), los procedimientos de las Tasks 4 y 5.
- Produces:
  - `CapaEntidad.EntPerfilJefe` con `CodUsuario` y `Nombre`, ambos `string = ""`.
  - `public static EntRespuesta DaoPerfil.GuardarDatosPersonales(string codUsuario, string codAutor, EntPerfilDatosPersonales datos, string ip)`
  - `public static List<EntPerfilJefe> DaoPerfil.ListarJefes(string codUsuario)`
  - `public static EntRespuesta NegPerfil.GuardarDatosPersonales(string codUsuario, string codAutor, EntPerfilDatosPersonales datos, string ip)`
  - `public static List<EntPerfilJefe> NegPerfil.ListarJefes(string codUsuario)`
  La Task 7 llama a las dos de `NegPerfil`.

> **Sin pruebas automáticas.** `CapaPruebas` sólo referencia `CapaEntidad` y
> `CapaNegocio`; `DaoPerfil` toca la base y no es alcanzable desde ahí. La
> verificación de esta tarea es que compile; la funcional es la de la Task 10.

- [ ] **Step 1: Crear `EntPerfilJefe`**

`CapaEntidad/EntPerfilJefe.cs`:

```csharp
namespace CapaEntidad
{
    /// <summary>
    /// Un candidato a jefe inmediato: el codigo que se guarda y el nombre que se
    /// muestra. Cod_Jefe_Inm guarda un Cod_Usuario, asi que el combo tiene que
    /// enviar el codigo aunque la persona elija por nombre.
    /// </summary>
    public class EntPerfilJefe
    {
        public string CodUsuario { get; set; } = "";
        public string Nombre { get; set; } = "";
    }
}
```

Registrarla en `CapaEntidad/CapaEntidad.csproj` junto a las demás `EntPerfil*`:

```xml
    <Compile Include="EntPerfilJefe.cs" />
```

- [ ] **Step 2: Leer `CodJefeInmediato` en `DaoPerfil.CargarPerfil`**

En `CapaDato/DaoPerfil.cs`, en el bloque que arma la cabecera, junto a la línea
que lee `JefeInmediato`:

```csharp
                        JefeInmediato      = Texto(dr, "JefeInmediato"),
                        CodJefeInmediato   = Texto(dr, "CodJefeInmediato"),
```

- [ ] **Step 3: Agregar las dos funciones al `Dao`**

En `CapaDato/DaoPerfil.cs`, después de `GuardarContacto`:

```csharp
        /// <summary>
        /// Guarda los ocho campos de datos personales en las DOS tablas y, si
        /// hace falta, adopta o crea la ficha de Empleados.
        ///
        /// codUsuario es DE QUIEN es el perfil; codAutor es QUIEN lo esta
        /// tocando. Esta es la primera escritura del modulo donde casi nunca son
        /// la misma persona: la pantalla que la usa es la de Talento Humano.
        ///
        /// El -3 no pasa por RespuestaDe porque necesita un mensaje propio: un
        /// jefe que no existe no es "no se pudo guardar", es un dato concreto que
        /// la persona tiene que corregir.
        /// </summary>
        public static EntRespuesta GuardarDatosPersonales(string codUsuario, string codAutor,
                                                          EntPerfilDatosPersonales datos, string ip)
        {
            int r = EjecutarEscritura("Sp_RTA_PerfilGuardarDatosPersonales", cmd =>
            {
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@Nombre",      SqlDbType.VarChar, 100).Value = datos.Nombre;
                cmd.Parameters.Add("@Cedula",      SqlDbType.VarChar,  32).Value = datos.Cedula;
                cmd.Parameters.Add("@FechaNac",    SqlDbType.VarChar,  10).Value = datos.FechaNacimiento;
                cmd.Parameters.Add("@Cargo",       SqlDbType.VarChar, 128).Value = datos.Cargo;
                cmd.Parameters.Add("@Area",        SqlDbType.VarChar, 128).Value = datos.Area;
                cmd.Parameters.Add("@Ciudad",      SqlDbType.VarChar, 150).Value = datos.Ciudad;
                cmd.Parameters.Add("@CodJefeInm",  SqlDbType.VarChar, 100).Value = datos.CodJefeInmediato;
                cmd.Parameters.Add("@Correo",      SqlDbType.VarChar, 100).Value = datos.CorreoNotificacion;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar,  64).Value = ip ?? "";
                cmd.Parameters.Add("@Usu_Accion",  SqlDbType.VarChar,  50).Value = codAutor ?? "";
            });

            if (r == -3)
            {
                return new EntRespuesta
                {
                    estado      = "0",
                    mensaje     = "El jefe inmediato seleccionado ya no existe o está inactivo. Vuelva a elegirlo.",
                    tipoMensaje = "warning"
                };
            }

            /* 1, 2 y 3 son GUARDADO CON AVISO, no errores, y por eso van con
               estado "1": la pantalla tiene que recargar el perfil igual que en
               un guardado limpio. Se interceptan antes de RespuestaDe porque
               para ella cualquier valor que no sea 0 ni -2 es un fracaso.

               No se bloquea a proposito: el sistema ya convive con correos y
               cedulas repetidos, y bloquear impediria corregir justamente esos
               casos. Ver las secciones 4 y 5 del diseno. */
            if (r >= 1 && r <= 3)
            {
                string aviso = "";

                if ((r & 1) == 1)
                {
                    aviso += " Atención: ese correo ya lo tiene otro usuario activo, y las notificaciones y las firmas pueden atribuirse a la persona equivocada.";
                }

                if ((r & 2) == 2)
                {
                    aviso += " Atención: esa cédula ya la tiene otro usuario activo.";
                }

                return new EntRespuesta
                {
                    estado      = "1",
                    mensaje     = "Los datos personales se guardaron correctamente." + aviso,
                    tipoMensaje = "warning"
                };
            }

            return RespuestaDe(r,
                               "Los datos personales se guardaron correctamente.",
                               "No se pudieron guardar los datos personales.");
        }

        /// <summary>
        /// Los candidatos a jefe inmediato, sin la persona misma. Lista vacia si
        /// el procedimiento no devuelve filas: la pantalla lo muestra como un
        /// combo con una sola opcion, "Sin jefe inmediato".
        /// </summary>
        public static List<EntPerfilJefe> ListarJefes(string codUsuario)
        {
            List<EntPerfilJefe> jefes = new List<EntPerfilJefe>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilJefesLista", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario ?? "";

                cnx.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        jefes.Add(new EntPerfilJefe
                        {
                            CodUsuario = Texto(dr, "CodUsuario"),
                            Nombre     = Texto(dr, "Nombre")
                        });
                    }
                }
            }

            return jefes;
        }
```

> Comprobar que `DaoPerfil.cs` trae `using System.Collections.Generic;`. Si no,
> agregarlo.

- [ ] **Step 4: Agregar la fachada en `NegPerfil`**

En `CapaNegocio/NegPerfil.cs`, después de `GuardarContacto`:

```csharp
        /// <summary>
        /// Guarda los ocho campos de datos personales. codAutor: ver
        /// GuardarContacto.
        ///
        /// Quien valida es NegPerfilCampos.ValidarDatosPersonales, y lo llama el
        /// handler porque necesita la cabecera guardada para comparar. Aca no se
        /// vuelve a validar: seria la misma comprobacion dos veces y sin la
        /// cabecera a mano.
        /// </summary>
        public static EntRespuesta GuardarDatosPersonales(string codUsuario, string codAutor,
                                                          EntPerfilDatosPersonales datos, string ip)
        {
            return DaoPerfil.GuardarDatosPersonales(codUsuario, codAutor, datos, ip);
        }

        /// <summary>Los candidatos a jefe inmediato, sin la persona misma.</summary>
        public static List<EntPerfilJefe> ListarJefes(string codUsuario)
        {
            return DaoPerfil.ListarJefes(codUsuario);
        }
```

- [ ] **Step 5: Compilar y correr las pruebas**

```bash
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Esperado: compila sin errores y las pruebas siguen en verde.

- [ ] **Step 6: Commit**

```bash
git add CapaEntidad/EntPerfilJefe.cs CapaEntidad/CapaEntidad.csproj CapaDato/DaoPerfil.cs CapaNegocio/NegPerfil.cs
git commit -m "feat(perfil): las capas de datos y negocio para los datos personales"
```

---

## Task 7: Las dos acciones del handler

**Files:**
- Modify: `ReporteTareas/Formulario/AdministrarPerfil.ashx.cs`

**Interfaces:**
- Consumes: `NegPerfilCampos.LeerDatosPersonales`,
  `NegPerfilCampos.ValidarDatosPersonales`, `NegPerfil.CargarPerfil`,
  `NegPerfil.GuardarDatosPersonales`, `NegPerfil.ListarJefes`.
- Produces: las acciones `"GuardarDatosPersonales"` y `"ListarJefes"` en
  `AdministrarPerfil.ashx`. La Task 9 las llama por `PostPerfil`.

- [ ] **Step 1: Anotar el conteo de acciones de partida**

```bash
grep -c 'if (Action == "' ReporteTareas/Formulario/AdministrarPerfil.ashx.cs
```

Anotar el número. Al final de la tarea tiene que ser **exactamente ese más 2**.

- [ ] **Step 2: Agregar el despacho**

En `ProcessRequest`, después del bloque de `GuardarContacto`:

```csharp
                if (Action == "GuardarDatosPersonales")
                {
                    existAction = true;
                    responseAction.Append(GuardarDatosPersonales(context, parametrosAccion));
                }

                if (Action == "ListarJefes")
                {
                    existAction = true;
                    responseAction.Append(ListarJefes(context, parametrosAccion));
                }
```

- [ ] **Step 3: Escribir los dos métodos**

Después del método `GuardarContacto`:

```csharp
        /// <summary>
        /// Guarda los ocho campos de datos personales del perfil que indica
        /// PerfilIdentidad.Objetivo, anotando como autor a PerfilIdentidad.Autor.
        ///
        /// Es la primera accion del modulo donde el objetivo y el autor casi
        /// nunca son la misma persona: la usa Talento Humano sobre el perfil de
        /// otro. La regla de quien puede hacerlo no esta aca sino en
        /// NegPerfilAcceso, igual que en las otras dieciseis escrituras.
        ///
        /// Antes de validar se LEE el perfil guardado. No es un rodeo: un campo
        /// solo se valida cuando cambio, y para saber si cambio hace falta saber
        /// que hay. Sin esto, las tres personas cuya cedula guardada no pasa el
        /// digito verificador no podrian guardar ningun campo.
        /// </summary>
        private string GuardarDatosPersonales(HttpContext context, dynamic campos)
        {
            try
            {
                EntPerfilObjetivo objetivo =
                    PerfilIdentidad.Objetivo(context, PerfilIdentidad.CodigoPedidoJson(campos));

                if (!objetivo.Permitido)
                {
                    return responseMessage("0", objetivo.Mensaje, "danger");
                }

                string codUsuario = objetivo.CodUsuario;
                string codAutor   = PerfilIdentidad.Autor(context);

                var diccionario = campos as System.Collections.Generic.IDictionary<string, object>;
                EntPerfilDatosPersonales datos = NegPerfilCampos.LeerDatosPersonales(diccionario);

                EntPerfilCompleto actual = NegPerfil.CargarPerfil(codUsuario);

                /* PerfilEncontrado en falso es el Cod_Usuario repetido: la
                   cabecera viene vacia y comparar contra ella daria por
                   "cambiado" todo lo que llegue. El procedimiento tambien lo
                   rechaza con -2, pero avisar aca evita una escritura inutil y da
                   el mensaje correcto. */
                if (!actual.PerfilEncontrado)
                {
                    return responseMessage("0",
                        "No pudimos identificar ese perfil de forma única. Hay que corregir el código de usuario antes de editarlo.",
                        "warning");
                }

                string error = NegPerfilCampos.ValidarDatosPersonales(datos, actual.Cabecera, codUsuario);
                if (error != "")
                {
                    return responseMessage("0", error, "warning");
                }

                return ToJson(NegPerfil.GuardarDatosPersonales(codUsuario, codAutor, datos,
                                                               context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar los datos personales. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Los candidatos a jefe inmediato para el combo.
        ///
        /// Pasa por la misma comprobacion de acceso que el resto: la lista es el
        /// catalogo completo de usuarios activos, y quien no puede editar un
        /// perfil tampoco tiene por que poder enumerarlos desde aca.
        /// </summary>
        private string ListarJefes(HttpContext context, dynamic campos)
        {
            try
            {
                EntPerfilObjetivo objetivo =
                    PerfilIdentidad.Objetivo(context, PerfilIdentidad.CodigoPedidoJson(campos));

                if (!objetivo.Permitido)
                {
                    return responseMessage("0", objetivo.Mensaje, "danger");
                }

                return ToJson(NegPerfil.ListarJefes(objetivo.CodUsuario));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar la lista de jefes. " + ex.Message, "danger");
            }
        }
```

- [ ] **Step 4: Comprobar el conteo**

```bash
grep -c 'if (Action == "' ReporteTareas/Formulario/AdministrarPerfil.ashx.cs
```

**Si no es el número del Step 1 más 2 exactamente, parar y avisar** en vez de
ajustar nada.

- [ ] **Step 5: Compilar el proyecto web**

```bash
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas\ReporteTareas.csproj /p:Configuration=Debug /v:minimal
```

Esperado: compila sin errores.

- [ ] **Step 6: Commit**

```bash
git add ReporteTareas/Formulario/AdministrarPerfil.ashx.cs
git commit -m "feat(perfil): las acciones de guardar datos personales y listar jefes"
```

---

## Task 8: El formulario en la pestaña

**Files:**
- Modify: `ReporteTareas/Controles/PerfilFichas.ascx`
- Modify: `ReporteTareas/Formulario/PerfilesPersonal.aspx`

**Interfaces:**
- Consumes: nada de C#.
- Produces, con estos `id` exactos:
  - Sólo lectura, ya existen y no se tocan: `dpNombre`, `dpCedula`, `dpFnac`,
    `dpCargo`, `dpArea`, `dpJefe`, `dpCiudad`, `dpCorreo`, `dpHorario`.
  - Nuevos, de edición: `edNombre`, `edCedula`, `edFnac`, `edCargo`, `edArea`,
    `edCiudad`, `edJefe` (un `<select>`), `edCorreo`, `dpHorarioEdicion`.
  - Contenedores: `panelDatosLectura`, `panelDatosEdicion`, `etiquetaDatos`,
    `btnGuardarDatosPersonales`.
  - La variable global `PERFIL_DATOS_EDITABLES`.

**El `.ascx` es compartido.** Lo usan `MiPerfil.aspx` y `PerfilesPersonal.aspx`,
así que el formulario se dibuja siempre pero nace oculto; lo enciende la Task 9
sólo si la página declaró la bandera.

- [ ] **Step 1: Reemplazar el cuerpo de la pestaña**

En `ReporteTareas/Controles/PerfilFichas.ascx`, reemplazar **todo** el
`<div class="tab-pane active" id="tabPersonal">` (arranca alrededor de la línea
128) por:

```html
                    <div class="tab-pane active" id="tabPersonal">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                Información personal
                                <span class="label label-default pull-right" id="etiquetaDatos">
                                    <i class="fa fa-lock"></i> Gestionado por Talento Humano
                                </span>
                            </div>
                            <div class="panel-body">

                                <!-- Lo que ve todo el mundo. En "Mi perfil" es lo
                                     unico que existe. -->
                                <div class="row" id="panelDatosLectura">
                                    <div class="form-group col-lg-6"><label>Nombres completos</label>
                                        <p class="form-control-static" id="dpNombre">–</p></div>
                                    <div class="form-group col-lg-6"><label>Número de cédula</label>
                                        <p class="form-control-static" id="dpCedula">–</p></div>
                                    <div class="form-group col-lg-6"><label>Fecha de nacimiento</label>
                                        <p class="form-control-static" id="dpFnac">–</p></div>
                                    <div class="form-group col-lg-6"><label>Cargo</label>
                                        <p class="form-control-static" id="dpCargo">–</p></div>
                                    <div class="form-group col-lg-6"><label>Área</label>
                                        <p class="form-control-static" id="dpArea">–</p></div>
                                    <div class="form-group col-lg-6"><label>Jefe inmediato</label>
                                        <p class="form-control-static" id="dpJefe">–</p></div>
                                    <div class="form-group col-lg-6"><label>Ciudad</label>
                                        <p class="form-control-static" id="dpCiudad">–</p></div>
                                    <div class="form-group col-lg-6"><label>Correo de notificación</label>
                                        <p class="form-control-static" id="dpCorreo">–</p></div>
                                    <div class="form-group col-lg-6"><label>Horario</label>
                                        <p class="form-control-static" id="dpHorario">–</p></div>
                                </div>

                                <!-- Solo lo enciende "Perfiles del personal", y
                                     solo despues de cargar el perfil. Nace oculto
                                     para que "Mi perfil" no cambie en nada. El
                                     horario NO es editable: tiene su propio
                                     modulo y 5 personas con mas de una asignacion
                                     activa, algo que un solo campo no puede
                                     representar. -->
                                <div id="panelDatosEdicion" style="display: none">
                                    <div class="row">
                                        <div class="form-group col-lg-6"><label>Nombres completos</label>
                                            <input type="text" class="form-control" id="edNombre" maxlength="100" /></div>
                                        <div class="form-group col-lg-6"><label>Número de cédula</label>
                                            <input type="text" class="form-control" id="edCedula" maxlength="32" /></div>
                                        <div class="form-group col-lg-6"><label>Fecha de nacimiento</label>
                                            <input type="text" class="form-control" id="edFnac" maxlength="10" placeholder="dd/mm/aaaa" /></div>
                                        <div class="form-group col-lg-6"><label>Cargo</label>
                                            <input type="text" class="form-control" id="edCargo" maxlength="128" /></div>
                                        <div class="form-group col-lg-6"><label>Área</label>
                                            <input type="text" class="form-control" id="edArea" maxlength="128" /></div>
                                        <div class="form-group col-lg-6"><label>Jefe inmediato</label>
                                            <select class="form-control" id="edJefe">
                                                <option value="">Sin jefe inmediato</option>
                                            </select></div>
                                        <div class="form-group col-lg-6"><label>Ciudad</label>
                                            <input type="text" class="form-control" id="edCiudad" maxlength="150" /></div>
                                        <div class="form-group col-lg-6"><label>Correo de notificación</label>
                                            <input type="email" class="form-control" id="edCorreo" maxlength="100" /></div>
                                        <div class="form-group col-lg-6"><label>Horario</label>
                                            <p class="form-control-static" id="dpHorarioEdicion">–</p>
                                            <p class="text-muted">
                                                El horario se asigna desde su propio módulo, no desde aquí.
                                            </p></div>
                                    </div>
                                    <div class="row">
                                        <div class="col-lg-12">
                                            <p class="text-muted">
                                                El nombre, el cargo, el área y el correo se usan en todo el
                                                sistema: en los listados, en las aprobaciones y en los correos
                                                que se envían. El jefe inmediato decide quién aprueba las
                                                vacaciones y los permisos de esta persona.
                                            </p>
                                            <button type="button" class="btn btn-primary"
                                                    id="btnGuardarDatosPersonales"
                                                    onclick="GuardarDatosPersonales()">
                                                <i class="fa fa-save"></i> Guardar datos personales
                                            </button>
                                        </div>
                                    </div>
                                </div>

                            </div>
                        </div>
                    </div>
```

- [ ] **Step 2: Declarar la bandera en `PerfilesPersonal.aspx`**

En el mismo `<script>` donde ya está `PERFIL_SIN_CARGA_INICIAL`, **antes** de las
etiquetas que cargan `miPerfil.js`:

```html
        /* Enciende el formulario de edicion de "Datos personales". La bandera es
           de ALCANCE, no de seguridad: quien puede editar lo decide
           NegPerfilAcceso en el servidor, y el handler lo vuelve a comprobar en
           cada llamada. Esto solo evita que "Mi perfil" cambie para las 228
           personas que la usan. */
        var PERFIL_DATOS_EDITABLES = true;
```

- [ ] **Step 3: Subir el `?v=` de `miPerfil.js` en las DOS páginas**

La Task 9 modifica `miPerfil.js`, y las dos páginas lo referencian con un
número de versión que sirve de rompe-cachés. Sin subirlo, el navegador sigue
sirviendo el archivo viejo y la pestaña no se vuelve editable —ya pasó en este
sistema con un `?v=` que quedó atrás—.

```bash
grep -rn "miPerfil.js?v=" ReporteTareas/ --include=*.aspx
```

Subir el número **en las dos**, `PerfilesPersonal.aspx` y `MiPerfil.aspx`. La
segunda no cambia de comportamiento —la bandera no existe ahí— pero se sirve el
mismo archivo, y dejarla con el número viejo deja media aplicación con una
versión y media con otra.

**`MiPerfil.aspx` pasa a ser un archivo a publicar**, aunque su único cambio sea
ese número.

- [ ] **Step 4: Verificar el BOM y los `id`**

```bash
python -c "
import io
for p in ['ReporteTareas/Controles/PerfilFichas.ascx','ReporteTareas/Formulario/PerfilesPersonal.aspx']:
    raw = io.open(p,'rb').read()
    print(p.split('/')[-1], 'BOM:', raw[:3]==b'\xef\xbb\xbf')
"
echo -n 'campos ed*: '; grep -c 'id="ed' ReporteTareas/Controles/PerfilFichas.ascx
echo -n 'page-wrapper: '; grep -c 'id="page-wrapper"' ReporteTareas/Controles/PerfilFichas.ascx
```

Esperado: `BOM: True` en los dos, `campos ed*: 8`, y **`page-wrapper: 0`** —ese
`div` se sacó del control en el commit `d09b5f6`, y volver a meterlo repite el
error del buscador dibujado debajo del menú lateral.

- [ ] **Step 5: Commit**

```bash
git add ReporteTareas/Controles/PerfilFichas.ascx ReporteTareas/Formulario/PerfilesPersonal.aspx ReporteTareas/Formulario/MiPerfil.aspx
git commit -m "feat(perfil): el formulario de datos personales, oculto salvo en la pantalla de Talento Humano"
```

---

## Task 9: Pintar, habilitar y guardar

**Files:**
- Modify: `ReporteTareas/js/miPerfil.js`

**Interfaces:**
- Consumes: los `id` de la Task 8, las acciones de la Task 7, y lo que ya existe
  en el archivo: `PostPerfil`, `MostrarMensaje`, `CargarPerfil`, `_perfil`.
- Produces: `GuardarDatosPersonales()`, llamada desde el `onclick` del botón.

- [ ] **Step 1: Encender el formulario al final de `PintarCabecera`**

En `PintarCabecera`, **después** de `$("#dpHorario").text(c.Horario || "–");` y
**antes** del `if (!c.TieneFicha)`:

```javascript
    /* El horario tambien va en el panel de edicion, donde es de solo lectura:
       tiene su propio modulo y 5 personas con mas de una asignacion activa. */
    $("#dpHorarioEdicion").text(c.Horario || "–");

    PintarDatosPersonalesEditables(c);
```

- [ ] **Step 2: Escribir las tres funciones nuevas**

Justo después de `PintarCabecera`:

```javascript
/* La bandera la declara "Perfiles del personal" en un <script> propio antes de
   cargar este archivo. Es de ALCANCE, no de seguridad: quien puede editar lo
   decide NegPerfilAcceso en el servidor y el handler lo comprueba en cada
   llamada. Esto solo mantiene "Mi perfil" exactamente como estaba. */
function DatosPersonalesEditables() {
    return typeof PERFIL_DATOS_EDITABLES !== "undefined" && PERFIL_DATOS_EDITABLES;
}

function PintarDatosPersonalesEditables(c) {
    if (!DatosPersonalesEditables()) { return; }

    $("#edNombre").val(c.NombreCompleto || "");
    $("#edCedula").val(c.Cedula || "");
    $("#edFnac").val(c.FechaNacTexto || "");
    $("#edCargo").val(c.Cargo || "");
    $("#edArea").val(c.Area || "");
    $("#edCiudad").val(c.Ciudad || "");
    $("#edCorreo").val(c.CorreoNotificacion || "");

    $("#panelDatosLectura").hide();
    $("#panelDatosEdicion").show();
    $("#etiquetaDatos")
        .removeClass("label-default").addClass("label-info")
        .html('<i class="fa fa-pencil"></i> Editable');

    CargarJefes(c.CodJefeInmediato || "");
}

/* El combo se llena en cada carga de perfil y no una sola vez: la lista excluye a
   la persona misma, asi que cambia con cada perfil que se abre. */
function CargarJefes(codJefeActual) {
    PostPerfil("ListarJefes", {}, function (respuesta) {
        // Un objeto con "estado" es un EntRespuesta, es decir, un error.
        if (respuesta != null && typeof respuesta.estado != "undefined") {
            MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);
            return;
        }

        var $combo = $("#edJefe").empty();
        $combo.append($("<option></option>").attr("value", "").text("Sin jefe inmediato"));

        /* .text(nombre) y no concatenacion de cadenas: el nombre viene de la base
           y puede traer cualquier cosa. Armar el <option> pegando HTML seria
           inyectar contenido ajeno en la pagina. */
        $.each(respuesta || [], function (i, jefe) {
            $combo.append($("<option></option>")
                .attr("value", jefe.CodUsuario)
                .text(jefe.Nombre));
        });

        /* Si el jefe guardado no esta en la lista -esta inactivo, o su codigo
           esta repetido- se agrega igual y se deja elegido. Perderlo en silencio
           dejaria a esa persona sin aprobador la primera vez que alguien guarde
           cualquier otro campo. */
        var yaEsta = false;
        $combo.find("option").each(function () {
            if ($(this).val() === codJefeActual) { yaEsta = true; return false; }
        });

        if (codJefeActual !== "" && !yaEsta) {
            $combo.append($("<option></option>")
                .attr("value", codJefeActual)
                .text(codJefeActual + " (inactivo o no listado)"));
        }

        $combo.val(codJefeActual);
    });
}

function GuardarDatosPersonales() {
    /* La comprobacion real esta en el servidor -el handler es alcanzable por
       HTTP directo-, pero esta evita una peticion inutil cuando ya se sabe que el
       codigo de usuario esta repetido. */
    if (_perfil && !_perfil.PerfilEncontrado) {
        MostrarMensaje("No pudimos identificar ese perfil de forma única. Hay que corregir el código de usuario antes de editarlo.", "warning");
        return;
    }

    var datos = {
        nombre:             $("#edNombre").val(),
        cedula:             $("#edCedula").val(),
        fechaNacimiento:    $("#edFnac").val(),
        cargo:              $("#edCargo").val(),
        area:               $("#edArea").val(),
        ciudad:             $("#edCiudad").val(),
        codJefeInmediato:   $("#edJefe").val(),
        correoNotificacion: $("#edCorreo").val()
    };

    var $boton = $("#btnGuardarDatosPersonales").prop("disabled", true);

    PostPerfil("GuardarDatosPersonales", datos, function (respuesta) {
        $boton.prop("disabled", false);
        MostrarMensaje(respuesta.mensaje, respuesta.tipoMensaje);

        /* Se recarga solo si guardo. Lo que se acaba de escribir cambia lo que
           devuelve la cabecera -la ficha puede haberse creado, y la precedencia
           entre Empleados y R_Usuarios no es la misma para todos los campos-, asi
           que dejar la pantalla con lo que se tecleo mostraria algo que no es lo
           que quedo guardado. */
        if (respuesta.estado === "1") {
            CargarPerfil();
        }
    });
}
```

- [ ] **Step 3: Verificar la sintaxis**

```bash
node --check ReporteTareas/js/miPerfil.js
```

Esperado: sin salida. Si `node` no está disponible, saltar este paso y mirar la
consola del navegador en la Task 10.

- [ ] **Step 4: Commit**

```bash
git add ReporteTareas/js/miPerfil.js
git commit -m "feat(perfil): pintar y guardar los datos personales desde la pantalla de Talento Humano"
```

---

## Task 10: Despliegue y verificación a mano

**Files:**
- Modify: `DESPLIEGUE.md`

**Interfaces:**
- Consumes: todo lo anterior.
- Produces: la lista de archivos a publicar y el orden.

- [ ] **Step 1: Compilar en Release y correr todas las pruebas**

```bash
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" ReporteTareas\ReporteTareas.csproj /p:Configuration=Release /v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" CapaPruebas\CapaPruebas.csproj /p:Configuration=Debug /v:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" CapaPruebas\bin\Debug\CapaPruebas.dll
```

Esperado: compila y **todas** las pruebas en verde, no sólo las nuevas.

- [ ] **Step 2: Agregar la sección a `DESPLIEGUE.md`**

```markdown
## Datos personales editables por Talento Humano

**Depende de la entrega 1.** Si `Empleados.Usu_ModificacionCod` no existe, el
script de procedimientos se detiene solo con un mensaje que lo dice.

### 1. Base de datos, en este orden

1. `docs/sql/2026-09-17-datos-personales-columnas.sql`
2. `docs/sql/2026-09-17-datos-personales-procedimientos.sql`

El segundo vuelve a crear `Sp_RTA_PerfilColaborador`, que es el que lee el perfil
en **las dos** pantallas. Si algo sale mal ahí, «Mi perfil» también se ve
afectado.

### 2. Archivos

- `Controles\PerfilFichas.ascx`
- `Formulario\PerfilesPersonal.aspx`
- `Formulario\MiPerfil.aspx` (sólo le cambia el `?v=` de `miPerfil.js`, pero sin
  eso esa pantalla sigue sirviendo el JavaScript viejo desde la caché)
- `js\miPerfil.js`
- `bin\ReporteTareas.dll`
- `bin\CapaEntidad.dll`
- `bin\CapaNegocio.dll`
- `bin\CapaDato.dll`

**Copiar archivo por archivo. Nunca `robocopy /MIR` ni ninguna copia que
sincronice:** borra `connections.config` y `appsettings.config`, y el sitio no
levanta.

### 3. Avisarle a Talento Humano antes

Las fichas que se creen aparecen en `RRHHEmpleados.aspx`, que hoy no lista a esas
personas. Son hasta 116. Si nadie avisa, van a ver crecer esa lista sin saber por
qué.
```

- [ ] **Step 3: Commit**

```bash
git add DESPLIEGUE.md
git commit -m "docs(perfil): como publicar la edicion de datos personales"
```

- [ ] **Step 4: Entregar la verificación a mano**

**La corre el usuario, no un subagente**, después de publicar:

| # | Qué hacer | Qué tiene que pasar |
|---|---|---|
| 1 | Abrir «Mi perfil» con un usuario cualquiera | La pestaña «Datos personales» se ve **igual que siempre**: sólo lectura, con el candado |
| 2 | Abrir «Perfiles del personal» con perfil 14 o 18 y buscar a alguien | La pestaña muestra los campos editables y la etiqueta dice «Editable» |
| 3 | Desplegar el combo de jefe inmediato | Lista a los usuarios activos por nombre, **sin** la persona que se está editando, y viene preseleccionado el jefe actual |
| 4 | Cambiar la ciudad y guardar | Mensaje verde, y al recargar la pantalla la ciudad nueva sigue ahí |
| 5 | Escribir una cédula con el último dígito cambiado y guardar | Mensaje amarillo «La cédula no es válida», y **nada se guarda** |
| 6 | Borrar el nombre y guardar | Mensaje amarillo «El nombre es obligatorio» |
| 7 | Abrir a alguien **sin ficha** (sale la nota de «sin ficha»), cambiar la ciudad y guardar | Guarda; al recargar, la ciudad está y la nota de «sin ficha» ya no aparece |
| 8 | Buscar a esa persona en `RRHHEmpleados.aspx` | Aparece **una sola vez**, no dos |
| 9 | Cambiar el jefe inmediato de alguien y guardar | Guarda; en la pestaña «Equipo» del jefe nuevo aparece esa persona |
| 9b | Ponerle a alguien un correo que ya tiene otro usuario activo y guardar | **Guarda igual**, con un mensaje amarillo que avisa del correo repetido. No bloquea |
| 10 | Consultar en la base quién quedó registrado | `SELECT Cod_Usuario, Usu_ModificacionCod, Fec_Modificacion FROM R_Usuarios WHERE Cod_Usuario = '<el editado>'` devuelve el código de **quien editó**, no el del editado |

---

## Notas para quien ejecute este plan

**Lo que más fácil se rompe, en orden:**

1. **Olvidar el `.csproj`.** Cuatro archivos nuevos: `NegPerfilCedula.cs`,
   `NegPerfilCedulaTests.cs`, `EntPerfilDatosPersonales.cs` y `EntPerfilJefe.cs`.
   Un archivo que no está listado no se compila y no avisa: la clase
   sencillamente «no existe».
2. **Guardar el `.ascx` o el `.aspx` sin BOM.** Sale a producción con los acentos
   rotos y no se nota hasta que alguien mira la pantalla.
3. **Validar siempre en vez de sólo al cambiar.** Es la regla menos intuitiva del
   diseño y la que deja trabadas a tres personas reales si se implementa mal.
4. **Insertar la ficha sin buscar la huérfana.** Duplica a 4 personas en otra
   pantalla, y el error aparece lejos de donde se cometió.
5. **Reintroducir `<div id="page-wrapper">` en el `.ascx`.** Ya pasó una vez
   (commit `d09b5f6`): el buscador queda dibujado debajo del menú lateral.
6. **Perder parte de `Sp_RTA_PerfilColaborador` en la Task 5.** Es un
   procedimiento de nueve conjuntos de resultados que leen las dos pantallas; el
   `diff` del Step 3 de esa tarea existe justamente para eso.
