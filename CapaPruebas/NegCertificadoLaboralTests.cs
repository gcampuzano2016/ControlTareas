using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CapaPruebas
{
    /// <summary>
    /// El certificado laboral. Lo que se prueba es lo que nadie ve hasta que el
    /// papel ya se entrego: un certificado emitido sin fecha de ingreso, con el
    /// sueldo de otro periodo, o con el membrete de la otra empresa.
    /// </summary>
    [TestClass]
    public class NegCertificadoLaboralTests
    {
        private static readonly DateTime Hoy = new DateTime(2026, 10, 28, 10, 30, 0);

        private static EntCertificadoDatos DatosCompletos()
        {
            return new EntCertificadoDatos
            {
                Encontrado = true,
                EnMaestro = true,
                CodUsuario = "0001",
                Cedula = "0916559735",
                Nombre = "Aguirre Salas Valeria",
                Cargo = "Jefe de servicios",
                Empresa = "DOS",
                FechaIngreso = new DateTime(2018, 7, 2),
                Ciudad = "QUITO",
                Salarios = new List<EntHeSalario>
                {
                    new EntHeSalario { Monto = 1000m, FechaVigenciaDesde = new DateTime(2024, 1, 1), Origen = "Rol" },
                    new EntHeSalario { Monto = 1250m, FechaVigenciaDesde = new DateTime(2026, 1, 1), Origen = "Rol" },
                    new EntHeSalario { Monto = 9999m, FechaVigenciaDesde = new DateTime(2027, 1, 1), Origen = "Rol" }
                }
            };
        }

        private static EntCertificadoLaboral Armar(EntCertificadoDatos d, string tipo, out string error)
        {
            return NegCertificadoLaboral.Armar(d, tipo, "autor", Hoy, out error);
        }

        [TestMethod]
        public void Armar_SinSalario_NoLlevaSueldo()
        {
            string error;
            EntCertificadoLaboral c = Armar(DatosCompletos(), "SIN", out error);

            Assert.AreEqual("", error);
            Assert.IsNull(c.Salario);
            Assert.AreEqual("DOS", c.Empresa);
            Assert.AreEqual("AGUIRRE SALAS VALERIA", c.Nombre);
            Assert.AreEqual("JEFE DE SERVICIOS", c.Cargo);
        }

        /// <summary>
        /// El vigente a la fecha de hoy: ni el viejo ni el que rige en el futuro.
        /// </summary>
        [TestMethod]
        public void Armar_ConSalario_TomaElVigenteAHoy()
        {
            string error;
            EntCertificadoLaboral c = Armar(DatosCompletos(), "con", out error);

            Assert.AreEqual("", error);
            Assert.AreEqual("CON", c.Tipo);
            Assert.AreEqual(1250m, c.Salario);
        }

        [TestMethod]
        public void Armar_ConSalarioSinSueldoRegistrado_NoSeEmite()
        {
            EntCertificadoDatos d = DatosCompletos();
            d.Salarios.Clear();

            string error;
            Assert.IsNull(Armar(d, "CON", out error));
            StringAssert.Contains(error, "sin salario");
        }

        [TestMethod]
        public void Armar_FueraDelMaestro_NoSeEmite()
        {
            EntCertificadoDatos d = DatosCompletos();
            d.EnMaestro = false;

            string error;
            Assert.IsNull(Armar(d, "SIN", out error));
            StringAssert.Contains(error, "fecha de ingreso");
        }

        [TestMethod]
        public void Armar_SinFechaDeIngreso_NoSeEmite()
        {
            EntCertificadoDatos d = DatosCompletos();
            d.FechaIngreso = null;

            string error;
            Assert.IsNull(Armar(d, "SIN", out error));
            StringAssert.Contains(error, "fecha de ingreso");
        }

        [TestMethod]
        public void Armar_EmpresaDesconocida_NoSeEmite()
        {
            EntCertificadoDatos d = DatosCompletos();
            d.Empresa = "";

            string error;
            Assert.IsNull(Armar(d, "SIN", out error));
            StringAssert.Contains(error, "empresa");
        }

        [TestMethod]
        public void Armar_PerfilNoEncontrado_NoSeEmite()
        {
            EntCertificadoDatos d = DatosCompletos();
            d.Encontrado = false;

            string error;
            Assert.IsNull(Armar(d, "SIN", out error));
            Assert.AreNotEqual("", error);
        }

        [TestMethod]
        public void Armar_TipoInvalido_NoSeEmite()
        {
            string error;
            Assert.IsNull(Armar(DatosCompletos(), "OTRO", out error));
            Assert.AreNotEqual("", error);
        }

        [TestMethod]
        public void Empresa_ReconoceLasVariantesDelMaestro()
        {
            Assert.AreEqual("DOS", NegCertificadoLaboral.Empresa("DOS"));
            Assert.AreEqual("DOS", NegCertificadoLaboral.Empresa("dos s.a."));
            Assert.AreEqual("AGILITY", NegCertificadoLaboral.Empresa("AGILITY"));
            Assert.AreEqual("AGILITY", NegCertificadoLaboral.Empresa("AGILITY - SERVICIOS PROFESIONALES"));
            Assert.AreEqual("", NegCertificadoLaboral.Empresa("DOSIFICADORA"));
            Assert.AreEqual("", NegCertificadoLaboral.Empresa(null));
        }

        [TestMethod]
        public void Formatos_ComoLasPlantillasDeTalentoHumano()
        {
            Assert.AreEqual("2 de julio de 2018", NegCertificadoLaboral.FechaLarga(new DateTime(2018, 7, 2)));
            Assert.AreEqual("$1.250,00 USD", NegCertificadoLaboral.Dinero(1250m));
            Assert.AreEqual("$870,50 USD", NegCertificadoLaboral.Dinero(870.5m));
            Assert.AreEqual("Quito, D.M.", NegCertificadoLaboral.Lugar("QUITO"));
            Assert.AreEqual("Quito, D.M.", NegCertificadoLaboral.Lugar(""));
            Assert.AreEqual("Guayaquil", NegCertificadoLaboral.Lugar("GUAYAQUIL"));
            Assert.AreEqual("Santo Domingo", NegCertificadoLaboral.Lugar("SANTO DOMINGO"));
        }

        [TestMethod]
        public void Html_ConSalario_LlevaElMonto_SinSalarioNo()
        {
            string error;
            string con = NegCertificadoLaboral.Html(Armar(DatosCompletos(), "CON", out error), "", "");
            string sin = NegCertificadoLaboral.Html(Armar(DatosCompletos(), "SIN", out error), "", "");

            StringAssert.Contains(con, "$1.250,00 USD");
            Assert.IsFalse(sin.Contains("USD"));
            Assert.IsFalse(sin.Contains("Percibiendo"));
        }

        [TestMethod]
        public void Html_FirmaYRazonSocialSegunLaEmpresa()
        {
            string error;
            EntCertificadoDatos d = DatosCompletos();
            string dos = NegCertificadoLaboral.Html(Armar(d, "SIN", out error), "data:dos", "data:ag");
            d.Empresa = "AGILITY";
            string ag = NegCertificadoLaboral.Html(Armar(d, "SIN", out error), "data:dos", "data:ag");

            StringAssert.Contains(dos, "COMPUEQUIP DOS S.A.");
            StringAssert.Contains(dos, "data:dos");
            Assert.IsFalse(dos.Contains("data:ag"));

            StringAssert.Contains(ag, "AGILITY SERVICES IT");
            StringAssert.Contains(ag, "CARLOS PINOS");
            StringAssert.Contains(ag, "data:ag");
            Assert.IsFalse(ag.Contains("COMPUEQUIP"));
        }

        /// <summary>
        /// El nombre y el cargo vienen del Excel de GTH. Un caracter sin escapar no
        /// lanza nada: corta el certificado en silencio.
        /// </summary>
        [TestMethod]
        public void Html_EscapaLosDatos()
        {
            EntCertificadoDatos d = DatosCompletos();
            d.Cargo = "Analista <b>&</b> soporte";

            string error;
            string html = NegCertificadoLaboral.Html(Armar(d, "SIN", out error), "", "");

            StringAssert.Contains(html, "ANALISTA &lt;B&gt;&amp;&lt;/B&gt; SOPORTE");
        }

        [TestMethod]
        public void Html_LaFechaImpresaEsLaDeLaSolicitud()
        {
            string error;
            string html = NegCertificadoLaboral.Html(Armar(DatosCompletos(), "SIN", out error), "", "");

            StringAssert.Contains(html, "Quito, D.M., 28 de octubre de 2026");
            StringAssert.Contains(html, "desde el 2 de julio de 2018");
        }
    }
}
