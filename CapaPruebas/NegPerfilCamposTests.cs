using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Threading;

namespace CapaPruebas
{
    /// <summary>
    /// Las tres funciones que pueden fallar sin dar la cara. Un error aca no
    /// lanza excepcion ni pinta rojo en ningun lado: la edad sale mal, el campo
    /// bloqueado se guarda, el contacto de emergencia queda sin telefono.
    /// </summary>
    [TestClass]
    public class NegPerfilCamposTests
    {
        private CultureInfo _culturaOriginal;

        /// <summary>
        /// Cultura hostil a proposito: en-US interpreta "03/07/1985" como
        /// 7 de marzo, no 3 de julio. Si alguien le quita el formato explicito
        /// a EdadDesdeTexto y lo reemplaza por un TryParse implicito, las
        /// pruebas de esta clase tienen que fallar aqui mismo, no solo en un
        /// servidor con otra configuracion regional.
        /// </summary>
        [TestInitialize]
        public void FijarCulturaHostil()
        {
            _culturaOriginal = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }

        [TestCleanup]
        public void RestaurarCultura()
        {
            Thread.CurrentThread.CurrentCulture = _culturaOriginal;
        }

        /* ---------------------------------------------------------- edad ---- */

        /// <summary>
        /// El caso que motiva toda esta funcion. En la base hay 129 fechas en
        /// formato dd/MM/yyyy y 80 de ellas tienen el dia por encima de 12. Si
        /// alguien las interpreta como mm/dd/yyyy, esas 80 fallan y las otras 49
        /// salen bien: el error se ve como "a algunos no les carga la edad".
        /// </summary>
        [TestMethod]
        public void EdadDesdeTexto_DiaMayorQueDoce_NoLoConfundeConElMes()
        {
            int? edad = NegPerfilCampos.EdadDesdeTexto("25/12/1990");

            Assert.IsNotNull(edad, "25/12/1990 es una fecha valida en dd/MM/yyyy");
            Assert.AreEqual(AniosDesde(new DateTime(1990, 12, 25)), edad.Value);
        }

        [TestMethod]
        public void EdadDesdeTexto_DiaMenorQueDoce_LeeElDiaPrimero()
        {
            // 03/07 es 3 de julio, no 7 de marzo. Sin formato explicito esta
            // fecha "funciona" en los dos sentidos y da edades distintas.
            int? edad = NegPerfilCampos.EdadDesdeTexto("03/07/1985");

            Assert.AreEqual(AniosDesde(new DateTime(1985, 7, 3)), edad.Value);
        }

        [TestMethod]
        public void EdadDesdeTexto_CumpleAunNoCumplido_RestaUnAnio()
        {
            DateTime manana = DateTime.Today.AddDays(1);
            string texto = manana.AddYears(-30).ToString("dd/MM/yyyy");

            Assert.AreEqual(29, NegPerfilCampos.EdadDesdeTexto(texto).Value,
                            "si el cumpleanios es manana todavia tiene 29");
        }

        [TestMethod]
        public void EdadDesdeTexto_CumpleHoy_CuentaElAnio()
        {
            string texto = DateTime.Today.AddYears(-30).ToString("dd/MM/yyyy");

            Assert.AreEqual(30, NegPerfilCampos.EdadDesdeTexto(texto).Value);
        }

        [TestMethod]
        public void EdadDesdeTexto_VacioOBasura_DevuelveNull()
        {
            // 4 empleados tienen la fecha vacia. No es un error: es que no se
            // cargo. La pantalla muestra un guion, no un cero ni una excepcion.
            Assert.IsNull(NegPerfilCampos.EdadDesdeTexto(null));
            Assert.IsNull(NegPerfilCampos.EdadDesdeTexto(""));
            Assert.IsNull(NegPerfilCampos.EdadDesdeTexto("   "));
            Assert.IsNull(NegPerfilCampos.EdadDesdeTexto("no es fecha"));
            Assert.IsNull(NegPerfilCampos.EdadDesdeTexto("31/02/1990"));
        }

        /// <summary>
        /// La base tiene 8 fechas de nacimiento fuera de rango razonable: un
        /// anio mal tipeado no es hipotetico. Una fecha de nacimiento futura
        /// no es una edad negativa, es un dato invalido.
        /// </summary>
        [TestMethod]
        public void EdadDesdeTexto_FechaFutura_DevuelveNull()
        {
            string texto = DateTime.Today.AddYears(65).ToString("dd/MM/yyyy");

            Assert.IsNull(NegPerfilCampos.EdadDesdeTexto(texto),
                          "una fecha de nacimiento en el futuro no es una edad negativa");
        }

        [TestMethod]
        public void EdadDesdeTexto_ConEspacios_LosIgnora()
        {
            Assert.IsNotNull(NegPerfilCampos.EdadDesdeTexto("  14/03/1996  "));
        }

        private static int AniosDesde(DateTime nacimiento)
        {
            DateTime hoy = DateTime.Today;
            int anios = hoy.Year - nacimiento.Year;
            if (nacimiento.Date > hoy.AddYears(-anios)) { anios--; }
            return anios;
        }

        /* -------------------------------------------------- lista blanca ---- */

        /// <summary>
        /// El corazon del control de acceso del modulo. En la maqueta los campos
        /// bloqueados eran un disabled de CSS, que no detiene a nadie que sepa
        /// abrir la consola del navegador. Aca el payload puede traer lo que
        /// quiera: si no esta en la lista, no hay propiedad donde aterrice.
        /// </summary>
        [TestMethod]
        public void LeerContacto_PayloadConCamposBloqueados_LosIgnora()
        {
            var payload = new System.Collections.Generic.Dictionary<string, object>
            {
                { "correoPersonal",   "alguien@gmail.com" },
                { "telefonoPersonal", "0991234567" },
                { "direccion",        "Av. Amazonas y Naciones Unidas" },
                { "estadoCivil",      "Casado/a" },
                // Los que RRHH administra. Vienen en el payload a proposito.
                { "cargo",            "Gerente General" },
                { "areaTrabajo",      "Directorio" },
                { "cedula",           "9999999999" },
                { "fechaNacimiento",  "01/01/1900" },
                { "puestoTrabajo",    "Gerente General" }
            };

            EntPerfilContacto contacto = NegPerfilCampos.LeerContacto(payload);

            Assert.AreEqual("alguien@gmail.com", contacto.CorreoPersonal);
            Assert.AreEqual("0991234567", contacto.TelefonoPersonal);
            Assert.AreEqual("Av. Amazonas y Naciones Unidas", contacto.Direccion);
            Assert.AreEqual("Casado/a", contacto.EstadoCivil);

            // La comprobacion de verdad: la entidad no tiene forma de cargar un
            // cargo. Si alguien le agrega la propiedad, esta prueba falla al
            // correr y obliga a mirar por que.
            Assert.AreEqual(4, typeof(EntPerfilContacto).GetProperties().Length,
                            "EntPerfilContacto es la lista blanca: cuatro campos, ni uno mas");
        }

        [TestMethod]
        public void LeerContacto_ClavesAusentes_QuedanEnCadenaVacia()
        {
            var payload = new System.Collections.Generic.Dictionary<string, object>
            {
                { "correoPersonal", "alguien@gmail.com" }
            };

            EntPerfilContacto contacto = NegPerfilCampos.LeerContacto(payload);

            Assert.AreEqual("alguien@gmail.com", contacto.CorreoPersonal);
            Assert.AreEqual("", contacto.TelefonoPersonal);
            Assert.AreEqual("", contacto.Direccion);
            Assert.AreEqual("", contacto.EstadoCivil);
        }

        [TestMethod]
        public void LeerContacto_RecortaEspacios()
        {
            var payload = new System.Collections.Generic.Dictionary<string, object>
            {
                { "correoPersonal", "  alguien@gmail.com  " }
            };

            Assert.AreEqual("alguien@gmail.com",
                            NegPerfilCampos.LeerContacto(payload).CorreoPersonal);
        }

        [TestMethod]
        public void LeerContacto_PayloadNulo_DevuelveEntidadVacia()
        {
            EntPerfilContacto contacto = NegPerfilCampos.LeerContacto(null);

            Assert.IsNotNull(contacto);
            Assert.AreEqual("", contacto.CorreoPersonal);
        }
    }
}
