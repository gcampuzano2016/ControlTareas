using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CapaPruebas
{
    /// <summary>
    /// El HTML del CV. Lo que se prueba aqui es lo que falla en silencio: un
    /// caracter sin escapar no lanza ninguna excepcion -HtmlRenderer no valida
    /// nada- y el PDF sale con la seccion cortada o con el texto en el lugar
    /// equivocado, sin que nadie se entere hasta que alguien mira su CV.
    /// </summary>
    [TestClass]
    public class NegPerfilCvTests
    {
        private static EntPerfilCompleto PerfilMinimo()
        {
            EntPerfilCompleto perfil = new EntPerfilCompleto();
            perfil.PerfilEncontrado = true;
            perfil.Cabecera.NombreCompleto = "Ana Pérez";
            perfil.Cabecera.Cargo = "Analista";
            perfil.Cabecera.Area = "Sistemas";
            return perfil;
        }

        [TestMethod]
        public void Construir_PerfilNulo_DevuelveVacio()
        {
            Assert.AreEqual("", NegPerfilCv.Construir(null));
        }

        /// <summary>
        /// Sin fila de cabecera no se sabe de quien es el perfil. Un CV en blanco
        /// con membrete seria peor que ninguno.
        /// </summary>
        [TestMethod]
        public void Construir_PerfilNoEncontrado_DevuelveVacio()
        {
            EntPerfilCompleto perfil = new EntPerfilCompleto();
            perfil.PerfilEncontrado = false;
            Assert.AreEqual("", NegPerfilCv.Construir(perfil));
        }

        [TestMethod]
        public void Construir_PerfilMinimo_TraeElNombreYEsUnDocumento()
        {
            string html = NegPerfilCv.Construir(PerfilMinimo());

            StringAssert.Contains(html, "Ana P");
            StringAssert.Contains(html, "<html");
            StringAssert.Contains(html, "</html>");
        }

        /// <summary>
        /// La prueba que justifica que esta clase exista. Los nombres los teclea
        /// la propia persona y van sin filtrar a un documento HTML.
        /// </summary>
        [TestMethod]
        public void Construir_NombreConMarcado_LoEscapa()
        {
            EntPerfilCompleto perfil = PerfilMinimo();
            perfil.Cabecera.NombreCompleto = "<b>Ana</b> & Cía";

            string html = NegPerfilCv.Construir(perfil);

            Assert.IsFalse(html.Contains("<b>Ana</b>"), "El marcado del nombre no se escapo.");
            StringAssert.Contains(html, "&lt;b&gt;Ana&lt;/b&gt;");
            StringAssert.Contains(html, "&amp;");
        }

        [TestMethod]
        public void Construir_TextoDeUnaFilaConMarcado_LoEscapa()
        {
            EntPerfilCompleto perfil = PerfilMinimo();
            perfil.Estudios.Add(new EntPerfilEstudio
            {
                Nivel       = "Tercer nivel",
                Institucion = "<script>alert(1)</script>",
                Titulo      = "Ingeniería",
                AnioGraduacion = 2015
            });

            string html = NegPerfilCv.Construir(perfil);

            Assert.IsFalse(html.Contains("<script>"), "El marcado de una fila no se escapo.");
        }

        /// <summary>
        /// Un CV con encabezados de secciones vacias se lee como un formulario a
        /// medio llenar. Las secciones sin filas no se imprimen.
        /// </summary>
        [TestMethod]
        public void Construir_SinEstudios_NoImprimeLaSeccion()
        {
            string html = NegPerfilCv.Construir(PerfilMinimo());
            Assert.IsFalse(html.Contains("Formación académica"));
        }

        [TestMethod]
        public void Construir_ConUnEstudio_ImprimeLaSeccionYElTitulo()
        {
            EntPerfilCompleto perfil = PerfilMinimo();
            perfil.Estudios.Add(new EntPerfilEstudio
            {
                Nivel          = "Tercer nivel",
                Institucion    = "Universidad Central",
                Titulo         = "Ingeniería en Sistemas",
                AnioGraduacion = 2015
            });

            string html = NegPerfilCv.Construir(perfil);

            StringAssert.Contains(html, "Formación académica");
            StringAssert.Contains(html, "Universidad Central");
            StringAssert.Contains(html, "2015");
        }

        [TestMethod]
        public void Construir_ConUnaCertificacion_ImprimeLaSeccion()
        {
            EntPerfilCompleto perfil = PerfilMinimo();
            perfil.Certificaciones.Add(new EntPerfilCertificacion
            {
                Nombre         = "Scrum Master",
                Entidad        = "Scrum Alliance",
                FechaObtencion = "2023-04"
            });

            string html = NegPerfilCv.Construir(perfil);

            StringAssert.Contains(html, "Certificaciones");
            StringAssert.Contains(html, "Scrum Alliance");
        }

        [TestMethod]
        public void Construir_ConExperiencia_ImprimeLaSeccion()
        {
            EntPerfilCompleto perfil = PerfilMinimo();
            perfil.Experiencia.Add(new EntPerfilExperiencia
            {
                Empresa   = "Acme",
                Cargo     = "Desarrollador",
                AnioDesde = 2018,
                AnioHasta = null,
                Funciones = "Mantenimiento"
            });

            string html = NegPerfilCv.Construir(perfil);

            StringAssert.Contains(html, "Experiencia laboral");
            StringAssert.Contains(html, "Acme");
            /* Sin anio de fin, el periodo se lee "2018 - Actual" y no "2018 - ". */
            StringAssert.Contains(html, "Actual");
        }

        /// <summary>
        /// Los contactos de emergencia y las cargas familiares NO van en el CV: un
        /// CV es lo que uno entrega a un tercero, y el telefono de la madre de
        /// alguien no tiene por que salir en el.
        /// </summary>
        [TestMethod]
        public void Construir_ConContactoDeEmergencia_NoLoImprime()
        {
            EntPerfilCompleto perfil = PerfilMinimo();
            perfil.Emergencia.Add(new EntPerfilEmergencia
            {
                IdContacto = 1,
                Nombre     = "Rosa Calderón",
                Parentesco = "Madre",
                Telefono   = "0991234567"
            });

            string html = NegPerfilCv.Construir(perfil);

            Assert.IsFalse(html.Contains("Rosa Calder"), "El contacto de emergencia salio en el CV.");
        }

        [TestMethod]
        public void Construir_ConCargaFamiliar_NoLaImprime()
        {
            EntPerfilCompleto perfil = PerfilMinimo();
            perfil.CargasFamiliares.Add(new EntPerfilCargaFamiliar
            {
                IdCargaFam      = 1,
                Nombre          = "Mateo Suárez",
                Parentesco      = "Hijo/a",
                FechaNacimiento = "2019-05-02"
            });

            string html = NegPerfilCv.Construir(perfil);

            Assert.IsFalse(html.Contains("Mateo Su"), "La carga familiar salio en el CV.");
        }
    }
}
