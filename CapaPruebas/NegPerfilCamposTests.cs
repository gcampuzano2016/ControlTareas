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

        /* ------------------------------------------- emergencia valida ------ */

        [TestMethod]
        public void ValidarEmergencia_CompletoYCorrecto_SinError()
        {
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "Lucia Ortiz Vega", Parentesco = "Madre", Telefono = "0987654321"
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_SinNombre_Rechaza()
        {
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "   ", Parentesco = "Madre", Telefono = "0987654321"
            };

            Assert.AreEqual("Escriba el nombre del contacto de emergencia.",
                            NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_SinTelefono_Rechaza()
        {
            // El caso que importa: un contacto sin telefono ocupa el lugar del
            // bueno y hace creer que el dato esta.
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "Lucia Ortiz Vega", Parentesco = "Madre", Telefono = ""
            };

            Assert.AreEqual("Escriba el teléfono del contacto de emergencia.",
                            NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_TelefonoConLetras_Rechaza()
        {
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "Lucia Ortiz Vega", Parentesco = "Madre", Telefono = "no tengo"
            };

            Assert.AreEqual("El teléfono solo puede tener números, espacios, guiones, paréntesis, signo más y puntos.",
                            NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_TelefonoConGuionesYEspacios_LoAcepta()
        {
            // La gente escribe "099 123-4567". Contar digitos, no caracteres.
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "Lucia Ortiz Vega", Parentesco = "Madre", Telefono = "099 123-4567"
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_SinParentesco_Rechaza()
        {
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "Lucia Ortiz Vega", Parentesco = "", Telefono = "0987654321"
            };

            Assert.AreEqual("Indique el parentesco del contacto de emergencia.",
                            NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_ContactoNulo_Rechaza()
        {
            Assert.AreEqual("No se recibió el contacto de emergencia.",
                            NegPerfilCampos.ValidarEmergencia(null));
        }

        [TestMethod]
        public void ValidarEmergencia_TextoLibreConDigitos_Rechaza()
        {
            // Caso real que motivo el cambio: el usuario escribio literalmente
            // "no tengo celular" en el campo, y como tiene 10 digitos dentro, se
            // guardaba como un contacto de emergencia valido.
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "Lucia Ortiz Vega", Parentesco = "Madre", Telefono = "mi cedula es 1710034678, no tengo celular"
            };

            Assert.AreEqual("El teléfono solo puede tener números, espacios, guiones, paréntesis, signo más y puntos.",
                            NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_DigitosConLetras_Rechaza()
        {
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "Lucia Ortiz Vega", Parentesco = "Madre", Telefono = "aaa1234567bbb"
            };

            Assert.AreEqual("El teléfono solo puede tener números, espacios, guiones, paréntesis, signo más y puntos.",
                            NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_ConSignoMasYEspacios_Acepta()
        {
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "Lucia Ortiz Vega", Parentesco = "Madre", Telefono = "+593 99 123 4567"
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_ConParentesisYGuiones_Acepta()
        {
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "Lucia Ortiz Vega", Parentesco = "Madre", Telefono = "(02) 246-8000"
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_MasDe15Digitos_Rechaza()
        {
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "Lucia Ortiz Vega", Parentesco = "Madre", Telefono = "9999999999999999"
            };

            Assert.AreEqual("El teléfono no puede tener más de 15 dígitos.",
                            NegPerfilCampos.ValidarEmergencia(contacto));
        }

        [TestMethod]
        public void ValidarEmergencia_PocosDigitosCaracteresValidos_Rechaza()
        {
            // Cobertura del mensaje "al menos 7 digitos": con caracteres validos
            // pero insuficientes. Asegura que ese mensaje siga siendo alcanzable
            // con las nuevas reglas.
            var contacto = new EntPerfilEmergencia
            {
                Nombre = "Lucia Ortiz Vega", Parentesco = "Madre", Telefono = "12345"
            };

            Assert.AreEqual("El teléfono debe tener al menos 7 dígitos.",
                            NegPerfilCampos.ValidarEmergencia(contacto));
        }

        /* ------------------------------------------------------ estudios ---- */

        [TestMethod]
        public void ValidarEstudio_CompletoYCorrecto_SinError()
        {
            var e = new EntPerfilEstudio
            {
                Nivel = "Tercer nivel", Institucion = "Universidad Central del Ecuador",
                Titulo = "Ingenieria en Sistemas", AnioGraduacion = 2019
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarEstudio(e));
        }

        [TestMethod]
        public void ValidarEstudio_SinTitulo_Rechaza()
        {
            var e = new EntPerfilEstudio
            {
                Nivel = "Tercer nivel", Institucion = "Universidad Central del Ecuador",
                Titulo = "   ", AnioGraduacion = 2019
            };

            Assert.AreEqual("Escriba el título obtenido.", NegPerfilCampos.ValidarEstudio(e));
        }

        [TestMethod]
        public void ValidarEstudio_SinInstitucion_Rechaza()
        {
            var e = new EntPerfilEstudio
            {
                Nivel = "Tercer nivel", Institucion = "", Titulo = "Ingenieria en Sistemas",
                AnioGraduacion = 2019
            };

            Assert.AreEqual("Escriba la institución donde estudió.", NegPerfilCampos.ValidarEstudio(e));
        }

        [TestMethod]
        public void ValidarEstudio_SinNivel_Rechaza()
        {
            var e = new EntPerfilEstudio
            {
                Nivel = "", Institucion = "Universidad Central del Ecuador",
                Titulo = "Ingenieria en Sistemas", AnioGraduacion = 2019
            };

            Assert.AreEqual("Seleccione el nivel de estudio.", NegPerfilCampos.ValidarEstudio(e));
        }

        [TestMethod]
        public void ValidarEstudio_AnioEnElFuturo_Rechaza()
        {
            // Se permite el anio que viene -alguien que se gradua en diciembre lo
            // registra en enero- pero no mas alla.
            var e = new EntPerfilEstudio
            {
                Nivel = "Tercer nivel", Institucion = "Universidad Central del Ecuador",
                Titulo = "Ingenieria en Sistemas",
                AnioGraduacion = System.DateTime.Today.Year + 2
            };

            Assert.AreEqual("El año de graduación no puede ser posterior al próximo año.",
                            NegPerfilCampos.ValidarEstudio(e));
        }

        [TestMethod]
        public void ValidarEstudio_AnioProximo_LoAcepta()
        {
            var e = new EntPerfilEstudio
            {
                Nivel = "Tercer nivel", Institucion = "Universidad Central del Ecuador",
                Titulo = "Ingenieria en Sistemas",
                AnioGraduacion = System.DateTime.Today.Year + 1
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarEstudio(e));
        }

        [TestMethod]
        public void ValidarEstudio_AnioDemasiadoAntiguo_Rechaza()
        {
            var e = new EntPerfilEstudio
            {
                Nivel = "Tercer nivel", Institucion = "Universidad Central del Ecuador",
                Titulo = "Ingenieria en Sistemas", AnioGraduacion = 1939
            };

            Assert.AreEqual("El año de graduación no parece correcto.",
                            NegPerfilCampos.ValidarEstudio(e));
        }

        [TestMethod]
        public void ValidarEstudio_SinAnio_LoAcepta()
        {
            // Nulo es "no lo recuerdo", que es legitimo y no debe bloquear el registro.
            var e = new EntPerfilEstudio
            {
                Nivel = "Tercer nivel", Institucion = "Universidad Central del Ecuador",
                Titulo = "Ingenieria en Sistemas", AnioGraduacion = null
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarEstudio(e));
        }

        [TestMethod]
        public void ValidarEstudio_Nulo_Rechaza()
        {
            Assert.AreEqual("No se recibió el estudio.", NegPerfilCampos.ValidarEstudio(null));
        }

        /* ------------------------------------------------ certificaciones --- */

        [TestMethod]
        public void ValidarCertificacion_CompletaYCorrecta_SinError()
        {
            var c = new EntPerfilCertificacion
            {
                Nombre = "ITIL Foundation v4", Entidad = "AXELOS", FechaObtencion = "2023-05"
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarCertificacion(c));
        }

        [TestMethod]
        public void ValidarCertificacion_SinNombre_Rechaza()
        {
            var c = new EntPerfilCertificacion
            {
                Nombre = "", Entidad = "AXELOS", FechaObtencion = "2023-05"
            };

            Assert.AreEqual("Escriba el nombre de la certificación.",
                            NegPerfilCampos.ValidarCertificacion(c));
        }

        [TestMethod]
        public void ValidarCertificacion_SinEntidad_Rechaza()
        {
            var c = new EntPerfilCertificacion
            {
                Nombre = "ITIL Foundation v4", Entidad = "  ", FechaObtencion = "2023-05"
            };

            Assert.AreEqual("Escriba la entidad que la emitió.",
                            NegPerfilCampos.ValidarCertificacion(c));
        }

        [TestMethod]
        public void ValidarCertificacion_FechaConFormatoRaro_Rechaza()
        {
            var c = new EntPerfilCertificacion
            {
                Nombre = "ITIL Foundation v4", Entidad = "AXELOS", FechaObtencion = "mayo 2023"
            };

            Assert.AreEqual("La fecha de obtención no es válida.",
                            NegPerfilCampos.ValidarCertificacion(c));
        }

        [TestMethod]
        public void ValidarCertificacion_FechaFutura_Rechaza()
        {
            string futura = System.DateTime.Today.AddYears(1).ToString("yyyy-MM");
            var c = new EntPerfilCertificacion
            {
                Nombre = "ITIL Foundation v4", Entidad = "AXELOS", FechaObtencion = futura
            };

            Assert.AreEqual("La fecha de obtención no puede estar en el futuro.",
                            NegPerfilCampos.ValidarCertificacion(c));
        }

        [TestMethod]
        public void ValidarCertificacion_SinFecha_LaAcepta()
        {
            var c = new EntPerfilCertificacion
            {
                Nombre = "ITIL Foundation v4", Entidad = "AXELOS", FechaObtencion = ""
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarCertificacion(c));
        }

        [TestMethod]
        public void ValidarCertificacion_Nula_Rechaza()
        {
            Assert.AreEqual("No se recibió la certificación.",
                            NegPerfilCampos.ValidarCertificacion(null));
        }

        /* ----------------------------------------------------- experiencia -- */

        [TestMethod]
        public void ValidarExperiencia_CompletaYCorrecta_SinError()
        {
            var x = new EntPerfilExperiencia
            {
                Empresa = "SoporteTec Cia. Ltda.", Cargo = "Tecnico de Soporte N1",
                AnioDesde = 2017, AnioHasta = 2019, Funciones = "Mesa de ayuda."
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarExperiencia(x));
        }

        [TestMethod]
        public void ValidarExperiencia_SinEmpresa_Rechaza()
        {
            var x = new EntPerfilExperiencia
            {
                Empresa = "", Cargo = "Tecnico de Soporte N1", AnioDesde = 2017, AnioHasta = 2019
            };

            Assert.AreEqual("Escriba el nombre de la empresa.",
                            NegPerfilCampos.ValidarExperiencia(x));
        }

        [TestMethod]
        public void ValidarExperiencia_SinCargo_Rechaza()
        {
            var x = new EntPerfilExperiencia
            {
                Empresa = "SoporteTec Cia. Ltda.", Cargo = "   ", AnioDesde = 2017, AnioHasta = 2019
            };

            Assert.AreEqual("Escriba el cargo que ocupó.", NegPerfilCampos.ValidarExperiencia(x));
        }

        [TestMethod]
        public void ValidarExperiencia_SinAnioDesde_Rechaza()
        {
            // A diferencia del anio de graduacion, este si es obligatorio: sin el,
            // el CV no puede ordenar la experiencia, que es para lo que existe.
            var x = new EntPerfilExperiencia
            {
                Empresa = "SoporteTec Cia. Ltda.", Cargo = "Tecnico de Soporte N1",
                AnioDesde = null, AnioHasta = 2019
            };

            Assert.AreEqual("Indique el año en que empezó.", NegPerfilCampos.ValidarExperiencia(x));
        }

        [TestMethod]
        public void ValidarExperiencia_HastaAnteriorADesde_Rechaza()
        {
            var x = new EntPerfilExperiencia
            {
                Empresa = "SoporteTec Cia. Ltda.", Cargo = "Tecnico de Soporte N1",
                AnioDesde = 2019, AnioHasta = 2017
            };

            Assert.AreEqual("El año en que terminó no puede ser anterior al año en que empezó.",
                            NegPerfilCampos.ValidarExperiencia(x));
        }

        [TestMethod]
        public void ValidarExperiencia_MismoAnioDesdeYHasta_LoAcepta()
        {
            // Un contrato de pocos meses empieza y termina el mismo anio.
            var x = new EntPerfilExperiencia
            {
                Empresa = "SoporteTec Cia. Ltda.", Cargo = "Tecnico de Soporte N1",
                AnioDesde = 2019, AnioHasta = 2019
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarExperiencia(x));
        }

        [TestMethod]
        public void ValidarExperiencia_HastaNulo_SignificaActualidad()
        {
            var x = new EntPerfilExperiencia
            {
                Empresa = "SoporteTec Cia. Ltda.", Cargo = "Tecnico de Soporte N1",
                AnioDesde = 2017, AnioHasta = null
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarExperiencia(x));
        }

        [TestMethod]
        public void ValidarExperiencia_DesdeEnElFuturo_Rechaza()
        {
            var x = new EntPerfilExperiencia
            {
                Empresa = "SoporteTec Cia. Ltda.", Cargo = "Tecnico de Soporte N1",
                AnioDesde = System.DateTime.Today.Year + 1
            };

            Assert.AreEqual("El año en que empezó no puede estar en el futuro.",
                            NegPerfilCampos.ValidarExperiencia(x));
        }

        [TestMethod]
        public void ValidarExperiencia_Nula_Rechaza()
        {
            Assert.AreEqual("No se recibió la experiencia laboral.",
                            NegPerfilCampos.ValidarExperiencia(null));
        }

        /* ------------------------------------------------ carga familiar ---- */

        [TestMethod]
        public void ValidarCargaFamiliar_CompletaYCorrecta_SinError()
        {
            var c = new EntPerfilCargaFamiliar
            {
                Nombre = "Mateo Comina Ortiz", Parentesco = "Hijo/a", FechaNacimiento = "2019-06-02"
            };

            Assert.AreEqual("", NegPerfilCampos.ValidarCargaFamiliar(c));
        }

        [TestMethod]
        public void ValidarCargaFamiliar_SinNombre_Rechaza()
        {
            var c = new EntPerfilCargaFamiliar
            {
                Nombre = "", Parentesco = "Hijo/a", FechaNacimiento = "2019-06-02"
            };

            Assert.AreEqual("Escriba el nombre completo de la carga familiar.",
                            NegPerfilCampos.ValidarCargaFamiliar(c));
        }

        [TestMethod]
        public void ValidarCargaFamiliar_SinParentesco_Rechaza()
        {
            var c = new EntPerfilCargaFamiliar
            {
                Nombre = "Mateo Comina Ortiz", Parentesco = "", FechaNacimiento = "2019-06-02"
            };

            Assert.AreEqual("Seleccione el parentesco.", NegPerfilCampos.ValidarCargaFamiliar(c));
        }

        [TestMethod]
        public void ValidarCargaFamiliar_FechaFutura_Rechaza()
        {
            string futura = System.DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            var c = new EntPerfilCargaFamiliar
            {
                Nombre = "Mateo Comina Ortiz", Parentesco = "Hijo/a", FechaNacimiento = futura
            };

            Assert.AreEqual("La fecha de nacimiento no puede estar en el futuro.",
                            NegPerfilCampos.ValidarCargaFamiliar(c));
        }

        [TestMethod]
        public void ValidarCargaFamiliar_FechaConFormatoRaro_Rechaza()
        {
            var c = new EntPerfilCargaFamiliar
            {
                Nombre = "Mateo Comina Ortiz", Parentesco = "Hijo/a", FechaNacimiento = "02/06/2019"
            };

            Assert.AreEqual("La fecha de nacimiento no es válida.",
                            NegPerfilCampos.ValidarCargaFamiliar(c));
        }

        [TestMethod]
        public void ValidarCargaFamiliar_SinFecha_Rechaza()
        {
            // Aqui la fecha SI es obligatoria: sin ella no se sabe si la carga
            // sigue siendo carga, que es para lo que Talento Humano la usa.
            var c = new EntPerfilCargaFamiliar
            {
                Nombre = "Mateo Comina Ortiz", Parentesco = "Hijo/a", FechaNacimiento = ""
            };

            Assert.AreEqual("Indique la fecha de nacimiento.",
                            NegPerfilCampos.ValidarCargaFamiliar(c));
        }

        [TestMethod]
        public void ValidarCargaFamiliar_Nula_Rechaza()
        {
            Assert.AreEqual("No se recibió la carga familiar.",
                            NegPerfilCampos.ValidarCargaFamiliar(null));
        }
    }
}
