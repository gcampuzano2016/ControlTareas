using System.Collections.Generic;
using System.Linq;
using CapaEntidad;
using CapaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CapaPruebas
{
    /// <summary>
    /// Reenviar al jefe el correo de aprobacion de una solicitud, para cuando el
    /// envio original fallo y la solicitud quedo POR APROBAR sin que el jefe se
    /// enterara.
    ///
    /// El servidor no confia en que el boton este oculto: vuelve a comprobar que
    /// la solicitud sea de quien la reenvia y que siga pendiente.
    /// </summary>
    [TestClass]
    public class NegReenvioSolicitudTests
    {
        private static EntSolicitud Pendiente(string codUsuario)
        {
            return new EntSolicitud { IdVacaciones = 120, IdTipoSolicitud = 2, EstadoSolicitud = "POR APROBAR", Cod_Usuario = codUsuario };
        }

        [TestMethod]
        public void MotivoParaNoReenviar_SolicitudPropiaPendiente_PermiteReenviar()
        {
            Assert.IsNull(NegReenvioSolicitud.MotivoParaNoReenviar(Pendiente("1234"), "1234"));
        }

        [TestMethod]
        public void MotivoParaNoReenviar_SinSolicitud_NoPermite()
        {
            Assert.IsNotNull(NegReenvioSolicitud.MotivoParaNoReenviar(null, "1234"));
        }

        [TestMethod]
        public void MotivoParaNoReenviar_SolicitudQueNoExiste_NoPermite()
        {
            // El DAO devuelve una entidad vacia, con IdVacaciones en cero, cuando no encuentra la fila.
            EntSolicitud vacia = new EntSolicitud { Cod_Usuario = "1234", EstadoSolicitud = "POR APROBAR" };

            Assert.IsNotNull(NegReenvioSolicitud.MotivoParaNoReenviar(vacia, "1234"));
        }

        [TestMethod]
        public void MotivoParaNoReenviar_SolicitudDeOtroColaborador_NoPermite()
        {
            Assert.IsNotNull(NegReenvioSolicitud.MotivoParaNoReenviar(Pendiente("9999"), "1234"));
        }

        [TestMethod]
        public void MotivoParaNoReenviar_SinUsuarioDeSesion_NoPermite()
        {
            Assert.IsNotNull(NegReenvioSolicitud.MotivoParaNoReenviar(Pendiente(""), ""));
        }

        [TestMethod]
        public void MotivoParaNoReenviar_CodigoConEspacios_LoReconoceComoPropio()
        {
            // Cod_Usuario viene de una columna char en algunas tablas y puede traer relleno.
            Assert.IsNull(NegReenvioSolicitud.MotivoParaNoReenviar(Pendiente("1234  "), " 1234"));
        }

        [TestMethod]
        public void MotivoParaNoReenviar_SolicitudYaAprobada_NoPermite()
        {
            EntSolicitud aprobada = Pendiente("1234");
            aprobada.EstadoSolicitud = "APROBADO";

            Assert.IsNotNull(NegReenvioSolicitud.MotivoParaNoReenviar(aprobada, "1234"));
        }

        [TestMethod]
        public void MotivoParaNoReenviar_EstadoConEspaciosOMinusculas_SigueSiendoPendiente()
        {
            EntSolicitud pendiente = Pendiente("1234");
            pendiente.EstadoSolicitud = " por aprobar ";

            Assert.IsNull(NegReenvioSolicitud.MotivoParaNoReenviar(pendiente, "1234"));
        }

        [TestMethod]
        public void Asunto_Permiso()
        {
            Assert.AreEqual("Solicitud de Permiso (Reenvío)", NegReenvioSolicitud.Asunto(1));
        }

        [TestMethod]
        public void Asunto_Vacaciones()
        {
            Assert.AreEqual("Solicitud de Vacaciones (Reenvío)", NegReenvioSolicitud.Asunto(2));
        }

        [TestMethod]
        public void Asunto_Planificacion()
        {
            Assert.AreEqual("Solicitud de Planificación de Vacaciones (Reenvío)", NegReenvioSolicitud.Asunto(3));
        }

        [TestMethod]
        public void MarcaDelEnlace_PermisoUsaNO_ElRestoUsaEM()
        {
            // El quinto valor que viaja cifrado en el enlace de aprobar/rechazar.
            // RespuestaAprobacion.aspx distingue con el el permiso de las vacaciones.
            Assert.AreEqual("NO", NegReenvioSolicitud.MarcaDelEnlace(1));
            Assert.AreEqual("EM", NegReenvioSolicitud.MarcaDelEnlace(2));
            Assert.AreEqual("EM", NegReenvioSolicitud.MarcaDelEnlace(3));
        }
    }

    /// <summary>
    /// Los campos que se reemplazan en la plantilla del correo al jefe.
    ///
    /// Estaban escritos a mano dentro del guardado del permiso y del de
    /// vacaciones. Se sacaron para que el reenvio mande exactamente lo mismo que
    /// el envio original; estas pruebas fijan la lista tal como se armaba, asi
    /// que el correo del guardado no cambia.
    /// </summary>
    [TestClass]
    public class NegCamposCorreoSolicitudTests
    {
        private static EntSolicitud Solicitud()
        {
            return new EntSolicitud
            {
                IdVacaciones = 120,
                FechaRegistro = "25/09/2026",
                Cedula = "0912345678",
                Colaborador = "ANA MUÑOZ",
                Departamento = "SISTEMAS",
                JefeInmediato = "PEDRO PÉREZ",
                Actividad = "Cita médica",
                Remplazo = "LUIS ROJAS",
                FechaDesde = "01/10/2026",
                FechaHasta = "05/10/2026",
                Horas = "02:00",
                TotalDias = 5,
                SaldoDias = 10,
                StrCargoVacaciones = "SI",
                Observacion = "Sin novedad"
            };
        }

        private static string[] Pares(List<EntItemValor> campos)
        {
            return campos.Select(c => c.Item + "=" + c.Valor).ToArray();
        }

        [TestMethod]
        public void Permiso_ArmaLosMismosCamposQueElGuardado()
        {
            List<EntItemValor> campos = NegCamposCorreoSolicitud.Permiso(Solicitud(), "http://ok", "http://no");

            CollectionAssert.AreEqual(new[]
            {
                "tituloNotificacion=SOLICITUD DE PERMISO",
                "etiqueta1=Fecha:", "texto1=25/09/2026",
                "etiqueta11=Cédula:", "texto11=0912345678",
                "etiqueta2=Colaborador:", "texto2=ANA MUÑOZ",
                "etiqueta21=Departamento:", "texto21=SISTEMAS",
                "etiqueta3=Jefe Inmedianto:", "texto3=PEDRO PÉREZ",
                "etiqueta4=Actividad a Realizar:", "texto4=Cita médica",
                "texto22=Horas de Permiso",
                "etiqueta23=Desde:", "texto23=01/10/2026",
                "etiqueta25=Hasta:", "texto25=05/10/2026",
                "etiqueta26=Total de Horas:", "texto26=02:00",
                "etiqueta27=Cargo a vacaciones:", "texto27=SI",
                "etiqueta28=Observación:", "texto28=Sin novedad",
                "etiquetaBoton1=Aprobar", "urlBoton1=http://ok",
                "etiquetaBoton2=Rechazar", "urlBoton2=http://no"
            }, Pares(campos));
        }

        [TestMethod]
        public void Vacaciones_ArmaLosMismosCamposQueElGuardado()
        {
            List<EntItemValor> campos = NegCamposCorreoSolicitud.Vacaciones(Solicitud(), 2, "http://ok", "http://no");

            CollectionAssert.AreEqual(new[]
            {
                "tituloNotificacion=SOLICITUD DE VACACIONES",
                "etiqueta1=Fecha:", "texto1=25/09/2026",
                "etiqueta11=Cédula:", "texto11=0912345678",
                "etiqueta2=Colaborador:", "texto2=ANA MUÑOZ",
                "etiqueta21=Departamento:", "texto21=SISTEMAS",
                "etiqueta3=Jefe Inmedianto:", "texto3=PEDRO PÉREZ",
                "etiqueta4=Reemplazo:", "texto4=LUIS ROJAS",
                "texto22=Dias de Vacaciones",
                "etiqueta23=Desde:", "texto23=01/10/2026",
                "etiqueta25=Hasta:", "texto25=05/10/2026",
                "etiqueta26=Días Solicitados:", "texto26=5",
                "etiqueta27=Saldo de días:", "texto27=10",
                "etiquetaBoton1=Aprobar", "urlBoton1=http://ok",
                "etiquetaBoton2=Rechazar", "urlBoton2=http://no"
            }, Pares(campos));
        }

        [TestMethod]
        public void Vacaciones_DePlanificacion_CambiaSoloElTitulo()
        {
            List<EntItemValor> campos = NegCamposCorreoSolicitud.Vacaciones(Solicitud(), 3, "http://ok", "http://no");

            Assert.AreEqual("tituloNotificacion=SOLICITUD DE PLANIFICACIÓN DE VACACIONES", Pares(campos)[0]);
            Assert.AreEqual(26, campos.Count);
        }

        [TestMethod]
        public void Vacaciones_DeOtroTipo_NoLlevaTitulo()
        {
            // Asi se comportaba el guardado: el titulo solo se agregaba para 2 y 3.
            List<EntItemValor> campos = NegCamposCorreoSolicitud.Vacaciones(Solicitud(), 1, "http://ok", "http://no");

            Assert.IsFalse(campos.Any(c => c.Item == "tituloNotificacion"));
        }

        [TestMethod]
        public void Vacaciones_ConDecimales_UsaElFormatoDeLaCulturaActual()
        {
            // El guardado usaba double.ToString() sin cultura; se conserva igual.
            EntSolicitud solicitud = Solicitud();
            solicitud.TotalDias = 2.5;

            List<EntItemValor> campos = NegCamposCorreoSolicitud.Vacaciones(solicitud, 2, "", "");

            Assert.AreEqual((2.5).ToString(), campos.Single(c => c.Item == "texto26").Valor);
        }
    }
}
