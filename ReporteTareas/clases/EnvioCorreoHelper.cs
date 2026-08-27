using CapaEntidad;
using CapaNegocio;
using PDF;
using ReporteTareas.clases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace CorreoHelper
{
    public class EnvioCorreoHelper
    {
        public string ErrorProceso;

        #region EnvioCorreoEncuesta
        public bool EnvioCorreoEncuesta(string correosDestinatarios, string correoTitulo, string estructuraContenidoCorreo, Int32 Id_RegTareas)
        {
            bool respuestaEnvioCorreo = false;
            string contenidoCorreo = "";
            string urlAprobacion = "https://forms.office.com/Pages/ResponsePage.aspx?id=CUmHgCUzMUG2q3sFzRBrrhU9z6VXdKNPkrFPnJFtX29URFFYSTRUNEFXSUUwRlgxRjlQNFEwQ1pRNi4u";
            EntParametrosCorreo parametrosServidorCorreo = new EntParametrosCorreo();
            List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
            EntTareas objTarea = new EntTareas();
            EntDetalleTarea registro = new EntDetalleTarea();
            registro.Id_RegTareas = Convert.ToInt32(Id_RegTareas);
            objTarea = NegTareas.RTA_ConsultaTareaRTA(registro.Id_RegTareas);
            try
            {
                registro.Det_Num_OrdenServicio = objTarea.Num_OrdenServicio;
                registro.Det_Id_CompAranda = objTarea.Id_CompAranda;
                registro.Det_Nom_Empresa = objTarea.Nom_Empresa;
                registro.Id_RegTareas = objTarea.Id_RegTareas;
                // Llenado de listado de parámetros a ser reemplazados en el contenido del correo electrónico.
                listaCamposCorreo.Add(new EntItemValor() { Item = "tituloNotificacion", Valor = "Encuesta de satisfacción al cliente" });
                listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta1", Valor = "Orden de Servicio:" });
                listaCamposCorreo.Add(new EntItemValor() { Item = "texto1", Valor = registro.Det_Num_OrdenServicio });
                listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta11", Valor = "Cod. ARANDA:" });
                listaCamposCorreo.Add(new EntItemValor() { Item = "texto11", Valor = registro.Det_Id_CompAranda });
                listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Empresa:" });
                listaCamposCorreo.Add(new EntItemValor() { Item = "texto2", Valor = registro.Det_Nom_Empresa });
                listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta21", Valor = "Cliente:" });
                listaCamposCorreo.Add(new EntItemValor() { Item = "texto21", Valor = objTarea.NombreCliente });

                //listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta22", Valor = "Fecha:" });
                //listaCamposCorreo.Add(new EntItemValor() { Item = "texto22", Valor = campos["frmTxtFecha"] });
                //listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta23", Valor = "Hora Inicio:" });
                //listaCamposCorreo.Add(new EntItemValor() { Item = "texto23", Valor = campos["frmTxtHoraDesde"] });
                //listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta24", Valor = "Hora Fin:" });
                //listaCamposCorreo.Add(new EntItemValor() { Item = "texto24", Valor = campos["frmTxtHoraHasta"] });
                //listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta25", Valor = "Tiempo:" });
                //listaCamposCorreo.Add(new EntItemValor() { Item = "texto25", Valor = campos["frmTxtTiempo"] });

                listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta4", Valor = "Trabajo Realizado:" });
                listaCamposCorreo.Add(new EntItemValor() { Item = "texto4", Valor = objTarea.Nom_Responsable });
                listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta7", Valor = "Actividad Principal:" });
                listaCamposCorreo.Add(new EntItemValor() { Item = "texto7", Valor = objTarea.Det_Tarea });
                //listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaDescripcion", Valor = "Descripción de la Tarea:" });
                //listaCamposCorreo.Add(new EntItemValor() { Item = "textoDescripcion", Valor = registroOriginal.Det_Det_Tarea.ToString() });
                listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaBoton1", Valor = "Realizar encuesta" });
                listaCamposCorreo.Add(new EntItemValor() { Item = "urlBoton1", Valor = urlAprobacion });
                //listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaBoton2", Valor = "Rechazar" });
                //listaCamposCorreo.Add(new EntItemValor() { Item = "urlBoton2", Valor = urlRechazarAprobacion });

                parametrosServidorCorreo.smtpAddress = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("smtpAddress");
                parametrosServidorCorreo.emailFrom = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFrom");
                parametrosServidorCorreo.emailFromName = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFromName");
                parametrosServidorCorreo.password = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("password");
                parametrosServidorCorreo.portNumber = Convert.ToInt32(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("portNumber"));
                parametrosServidorCorreo.enableSSL = Convert.ToBoolean(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("enableSSL"));

                contenidoCorreo = EstructuraContenidoCorreoEncuesta();

                foreach (EntItemValor parametrosContenido in listaCamposCorreo)
                {
                    contenidoCorreo = contenidoCorreo.Replace("[" + parametrosContenido.Item + "]", parametrosContenido.Valor);
                }

                respuestaEnvioCorreo = EnviarCorreo(correosDestinatarios, correoTitulo, contenidoCorreo, parametrosServidorCorreo);

            }
            catch
            {
                respuestaEnvioCorreo = false;
            }

            return respuestaEnvioCorreo;
        }
        #endregion

        #region EnvioCorreoPermiso
        public bool EnvioCorreoPermiso(string correosDestinatarios, string correoTitulo, string estructuraContenidoCorreo, string body)
        {
            EntParametrosCorreo parametrosServidorCorreo = new EntParametrosCorreo();
            //leer ruta del archivo
            string ErrorProceso = "";
            string RutaDocumento = "";
            List<EntArchivoTarea> listaArchivosTarea = new List<EntArchivoTarea>();

            //leer ruta del archivo

            bool respuestaEnvioCorreo = false;
            string contenidoCorreo = "";
            try
            {
                parametrosServidorCorreo.smtpAddress = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("smtpAddress");
                parametrosServidorCorreo.emailFrom = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFrom");
                parametrosServidorCorreo.emailFromName = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFromName");
                parametrosServidorCorreo.password = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("password");
                parametrosServidorCorreo.portNumber = Convert.ToInt32(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("portNumber"));
                parametrosServidorCorreo.enableSSL = Convert.ToBoolean(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("enableSSL"));

                contenidoCorreo = AsuntoCorreoPermiso(estructuraContenidoCorreo);

                respuestaEnvioCorreo = EnviarCorreoPermiso(correosDestinatarios, correoTitulo, contenidoCorreo, parametrosServidorCorreo, RutaDocumento);

            }
            catch (Exception ex)
            {
                ErrorProceso = ex.Message.ToString().Trim();
                respuestaEnvioCorreo = false;
            }

            return respuestaEnvioCorreo;
        }
        #endregion

        #region EnvioCorreoPoliza
        public bool EnvioCorreoPoliza(string correosDestinatarios, string correoTitulo, string estructuraContenidoCorreo, string body, Int32 idpoliza)
        {
            EntParametrosCorreo parametrosServidorCorreo = new EntParametrosCorreo();
            //leer ruta del archivo
            string ErrorProceso = "";
            string RutaDocumento = "";
            List<EntArchivoTarea> listaArchivosTarea = new List<EntArchivoTarea>();
            Int32 IdTarea = Convert.ToInt32(idpoliza);
            listaArchivosTarea = NegTareas.ListaArchivosContrato(IdTarea);
            for (int i = 0; i < listaArchivosTarea.Count; i++)
            {
                RutaDocumento = RutaDocumento + listaArchivosTarea[i].Ruta_Archivo + "" + listaArchivosTarea[i].Nombre_ArchivoCodigo + ";";
            }
            //leer ruta del archivo

            bool respuestaEnvioCorreo = false;
            string contenidoCorreo = "";
            try
            {
                parametrosServidorCorreo.smtpAddress = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("smtpAddress");
                parametrosServidorCorreo.emailFrom = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFrom");
                parametrosServidorCorreo.emailFromName = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFromName");
                parametrosServidorCorreo.password = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("password");
                parametrosServidorCorreo.portNumber = Convert.ToInt32(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("portNumber"));
                parametrosServidorCorreo.enableSSL = Convert.ToBoolean(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("enableSSL"));

                contenidoCorreo = AsuntoCorreo(estructuraContenidoCorreo);

                respuestaEnvioCorreo = EnviarCorreoPoliza(correosDestinatarios, correoTitulo, contenidoCorreo, parametrosServidorCorreo, RutaDocumento);

            }
            catch (Exception ex)
            {
                ErrorProceso = ex.Message.ToString().Trim();
                respuestaEnvioCorreo = false;
            }

            return respuestaEnvioCorreo;
        }
        #endregion

        #region EnvioCorreoForeCast
        public bool EnvioCorreoForeCast(string correosDestinatarios, string correoTitulo, string estructuraContenidoCorreo, string body, Int32 IdForecast)
        {
            EntParametrosCorreo parametrosServidorCorreo = new EntParametrosCorreo();
            //leer ruta del archivo
            string ErrorProceso = "";
            string Notificacion = "";
            List<EntForeCast> DetalleForeCast = new List<EntForeCast>();
            DetalleForeCast = NegForeCast.ConsultaSp_RTAConsultarForeCast(IdForecast, 1);
            for (int i = 0; i < DetalleForeCast.Count; i++)
            {

                correoTitulo = correoTitulo = "Notificacion: " + "Cliente: " + DetalleForeCast[i].Cliente + " Marca: " + DetalleForeCast[i].Marca + " PVP: " + DetalleForeCast[i].PVPEstimado;
            }
            //leer ruta del archivo

            bool respuestaEnvioCorreo = false;
            string contenidoCorreo = "";
            try
            {
                parametrosServidorCorreo.smtpAddress = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("smtpAddress");
                parametrosServidorCorreo.emailFrom = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFrom");
                parametrosServidorCorreo.emailFromName = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFromName");
                parametrosServidorCorreo.password = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("password");
                parametrosServidorCorreo.portNumber = Convert.ToInt32(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("portNumber"));
                parametrosServidorCorreo.enableSSL = Convert.ToBoolean(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("enableSSL"));

                contenidoCorreo = AsuntoCorreoForeCast(estructuraContenidoCorreo);

                respuestaEnvioCorreo = EnviarCorreoForeCast(correosDestinatarios, correoTitulo, contenidoCorreo, parametrosServidorCorreo, Notificacion, "Notificación - Sistema de Gestión Interno");

            }
            catch (Exception ex)
            {
                ErrorProceso = ex.Message.ToString().Trim();
                respuestaEnvioCorreo = false;
            }

            return respuestaEnvioCorreo;
        }
        #endregion


        #region EnvioCorreo
        public bool EnvioCorreo(string correosDestinatarios, string correoTitulo, string estructuraContenidoCorreo, List<EntItemValor> listaCampos)
        {

            EntParametrosCorreo parametrosServidorCorreo = new EntParametrosCorreo();
            bool respuestaEnvioCorreo = false;
            string contenidoCorreo = "";

            try
            {
                parametrosServidorCorreo.smtpAddress = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("smtpAddress");
                parametrosServidorCorreo.emailFrom = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFrom");
                parametrosServidorCorreo.emailFromName = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFromName");
                parametrosServidorCorreo.password = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("password");
                parametrosServidorCorreo.portNumber = Convert.ToInt32(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("portNumber"));
                parametrosServidorCorreo.enableSSL = Convert.ToBoolean(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("enableSSL"));

                contenidoCorreo = estructuraContenidoCorreo;

                foreach (EntItemValor parametrosContenido in listaCampos)
                {
                    contenidoCorreo = contenidoCorreo.Replace("[" + parametrosContenido.Item + "]", parametrosContenido.Valor);
                }

                respuestaEnvioCorreo = EnviarCorreo(correosDestinatarios, correoTitulo, contenidoCorreo, parametrosServidorCorreo);

            }
            catch
            {
                respuestaEnvioCorreo = false;
            }

            return respuestaEnvioCorreo;

        }
        #endregion

        #region EnvioCorreoSolicitudJefe
        public bool EnvioCorreoSolicitudJefe(string correosDestinatarios, string correoTitulo, string estructuraContenidoCorreo, List<EntItemValor> listaCampos, string nombreArchivo)
        {
            //VerErrores("correosDestinatarios: " + correosDestinatarios, "Log", "Detalle");

            EntParametrosCorreo parametrosServidorCorreo = new EntParametrosCorreo();
            bool respuestaEnvioCorreo = false;
            string contenidoCorreo = "";

            try
            {
                parametrosServidorCorreo.smtpAddress = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("smtpAddress");
                parametrosServidorCorreo.emailFrom = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFrom");
                parametrosServidorCorreo.emailFromName = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFromName");
                parametrosServidorCorreo.password = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("password");
                parametrosServidorCorreo.portNumber = Convert.ToInt32(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("portNumber"));
                parametrosServidorCorreo.enableSSL = Convert.ToBoolean(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("enableSSL"));

                contenidoCorreo = EstructuraContenidoCorreoSolicitud(nombreArchivo);

                foreach (EntItemValor parametrosContenido in listaCampos)
                {
                    contenidoCorreo = contenidoCorreo.Replace("[" + parametrosContenido.Item + "]", parametrosContenido.Valor);
                }

                respuestaEnvioCorreo = EnviarCorreo(correosDestinatarios, correoTitulo, contenidoCorreo, parametrosServidorCorreo);
                //PDFs generarRide = new PDFs();
                //generarRide.EnvioCorreoEncuesta(contenidoCorreo);
            }
            catch
            {
                respuestaEnvioCorreo = false;
            }

            return respuestaEnvioCorreo;

        }
        #endregion

        #region EnvioCorreoCodigoValidacion
        public bool EnvioCorreoCodigoValidacion(string correosDestinatarios, string correoTitulo, string estructuraContenidoCorreo, List<EntItemValor> listaCampos, string nombreArchivo)
        {

            EntParametrosCorreo parametrosServidorCorreo = new EntParametrosCorreo();
            bool respuestaEnvioCorreo = false;
            string contenidoCorreo = "";

            try
            {
                parametrosServidorCorreo.smtpAddress = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("smtpAddress");
                parametrosServidorCorreo.emailFrom = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFrom");
                parametrosServidorCorreo.emailFromName = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFromName");
                parametrosServidorCorreo.password = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("password");
                parametrosServidorCorreo.portNumber = Convert.ToInt32(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("portNumber"));
                parametrosServidorCorreo.enableSSL = Convert.ToBoolean(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("enableSSL"));

                contenidoCorreo = EstructuraContenidoCorreoSolicitud(nombreArchivo);

                foreach (EntItemValor parametrosContenido in listaCampos)
                {
                    contenidoCorreo = contenidoCorreo.Replace("[" + parametrosContenido.Item + "]", parametrosContenido.Valor);
                }

                respuestaEnvioCorreo = EnviarCorreo(correosDestinatarios, correoTitulo, contenidoCorreo, parametrosServidorCorreo);
                //PDFs generarRide = new PDFs();
                //generarRide.EnvioCorreoEncuesta(contenidoCorreo);
            }
            catch
            {
                respuestaEnvioCorreo = false;
            }

            return respuestaEnvioCorreo;

        }
        #endregion

        #region EnvioCorreoMarcacion
        /// <summary>Correo de confirmación de una marcación (accion: 1=entrada, 2=salida).</summary>
        public bool EnvioCorreoMarcacion(string correoDestino, string nombreUsuario, int accion, DateTime fechaHora)
        {
            EntParametrosCorreo parametrosServidorCorreo = new EntParametrosCorreo();
            bool respuestaEnvioCorreo = false;

            try
            {
                parametrosServidorCorreo.smtpAddress = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("smtpAddress");
                parametrosServidorCorreo.emailFrom = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFrom");
                parametrosServidorCorreo.emailFromName = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFromName");
                parametrosServidorCorreo.password = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("password");
                parametrosServidorCorreo.portNumber = Convert.ToInt32(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("portNumber"));
                parametrosServidorCorreo.enableSSL = Convert.ToBoolean(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("enableSSL"));

                string tipo = (accion == 1) ? "entrada" : "salida";
                string fecha = fechaHora.ToString("dd/MM/yyyy");
                string hora = fechaHora.ToString("HH:mm");

                string titulo = "Registro de " + tipo + " - " + fecha + " " + hora;

                string contenido =
                    "<p>Estimado(a) " + nombreUsuario + ",</p>" +
                    "<p>Se registró su <b>" + tipo + "</b> con los siguientes datos:</p>" +
                    "<table cellpadding='6' style='border-collapse:collapse'>" +
                    "<tr><td style='border:1px solid #ddd'><b>Tipo</b></td><td style='border:1px solid #ddd'>" + tipo + "</td></tr>" +
                    "<tr><td style='border:1px solid #ddd'><b>Fecha</b></td><td style='border:1px solid #ddd'>" + fecha + "</td></tr>" +
                    "<tr><td style='border:1px solid #ddd'><b>Hora</b></td><td style='border:1px solid #ddd'>" + hora + "</td></tr>" +
                    "</table>" +
                    "<p>Si usted no reconoce este registro, comuníquese con Talento Humano.</p>" +
                    "<p style='color:#888;font-size:11px'>Mensaje automático del Sistema de Gestión Interno. No responda a este correo.</p>";

                respuestaEnvioCorreo = EnviarCorreo(correoDestino, titulo, contenido, parametrosServidorCorreo);
            }
            catch (Exception ex)
            {
                ErrorProceso = ex.Message.ToString().Trim();
                respuestaEnvioCorreo = false;
            }

            return respuestaEnvioCorreo;
        }
        #endregion

        #region EnvioCorreoSolicitudEmpleado
        public bool EnvioCorreoSolicitudEmpleado(string correosDestinatarios, string correoTitulo, string estructuraContenidoCorreo, List<EntItemValor> listaCampos, string nombreArchivo, int codigoSolicitud)
        {
            //VerErrores("correosDestinatarios: " + correosDestinatarios, "Log", "Detalle");
            EntParametrosCorreo parametrosServidorCorreo = new EntParametrosCorreo();
            bool respuestaEnvioCorreo = false;
            string contenidoCorreo = "";

            try
            {
                parametrosServidorCorreo.smtpAddress = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("smtpAddress");
                parametrosServidorCorreo.emailFrom = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFrom");
                parametrosServidorCorreo.emailFromName = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFromName");
                parametrosServidorCorreo.password = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("password");
                parametrosServidorCorreo.portNumber = Convert.ToInt32(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("portNumber"));
                parametrosServidorCorreo.enableSSL = Convert.ToBoolean(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("enableSSL"));

                string Cedula = "";
                string Colaborador = "";

                foreach (EntItemValor parametrosContenido in listaCampos)
                {
                    contenidoCorreo = contenidoCorreo.Replace("[" + parametrosContenido.Item + "]", parametrosContenido.Valor);
                    //Colaborador
                    if (parametrosContenido.Item == "texto2")
                    {
                        Colaborador = parametrosContenido.Valor;
                        //VerErrores("Colaborador: " + Colaborador, "Log", "Detalle");
                    }
                    //cedula
                    else if (parametrosContenido.Item == "texto11")
                    {
                        Cedula = parametrosContenido.Valor;
                        //VerErrores("Cedula: " + Cedula, "Log", "Detalle");
                    }
                }

                //VerErrores("correosDestinatarios: " + correosDestinatarios, "Log", "Detalle");
                if (correoTitulo != "Copia Solicitud de Vacaciones (Cancelado)" && correoTitulo != "Copia Solicitud de Planificación de Vacaciones (Cancelado)")
                {
                    PDFs generarRide = new PDFs();
                    PdfLista pdfLista = new PdfLista();

                    string rutaQR = generarRide.GenerarCodigoQR(Cedula + " " + Colaborador);

                    //VerErrores("rutaQR: " + rutaQR, "Log", "Detalle");

                    listaCampos.Add(new EntItemValor() { Item = "textoQR", Valor = "'" + rutaQR + "'" });

                    contenidoCorreo = EstructuraContenidoCorreoSolicitud(nombreArchivo);

                    foreach (EntItemValor parametrosContenido in listaCampos)
                    {
                        contenidoCorreo = contenidoCorreo.Replace("[" + parametrosContenido.Item + "]", parametrosContenido.Valor);
                    }

                    //VerErrores("contenidoCorreo: " + contenidoCorreo, "Log", "Detalle");
                    respuestaEnvioCorreo = EnviarCorreo(correosDestinatarios, correoTitulo, contenidoCorreo, parametrosServidorCorreo);
                    generarRide.EnvioCorreoEncuesta(contenidoCorreo, codigoSolicitud);
                    //pdfLista.CrearPDF(contenidoCorreo, codigoSolicitud);
                }
                else
                {

                    contenidoCorreo = EstructuraContenidoCorreoSolicitud(nombreArchivo);

                    foreach (EntItemValor parametrosContenido in listaCampos)
                    {
                        contenidoCorreo = contenidoCorreo.Replace("[" + parametrosContenido.Item + "]", parametrosContenido.Valor);
                    }

                    respuestaEnvioCorreo = EnviarCorreo(correosDestinatarios, correoTitulo, contenidoCorreo, parametrosServidorCorreo);
                }
            }
            catch (Exception ex)
            {
                respuestaEnvioCorreo = false;
                VerErrores("ex: " + ex.Message.ToString(), "Log", "Detalle");
            }

            return respuestaEnvioCorreo;

        }
        #endregion

        #region EstructuraContenidoCorreo
        public string EstructuraContenidoCorreo()
        {

            string mensaje = "";
            string nombreArchivo = "contenidoCorreoNotificacion.txt";
            string path = RutaPlantillaCorreo(nombreArchivo);

            if (File.Exists(path))
            {
                // Leer contenido del archivo
                mensaje = File.ReadAllText(path);
            }

            return mensaje;
        }
        #endregion

        #region EstructuraContenidoCorreoSolicitud
        public string EstructuraContenidoCorreoSolicitud(string nombreArchivo)
        {

            string mensaje = "";
            string path = RutaPlantillaCorreo(nombreArchivo);

            if (File.Exists(path))
            {
                // Leer contenido del archivo
                mensaje = File.ReadAllText(path);
            }

            return mensaje;
        }
        #endregion

        #region EstructuraContenidoCorreoUsuario
        public string EstructuraContenidoCorreoUsuario()
        {

            string mensaje = "";
            string nombreArchivo = "contenidoCorreoNotificacionUsuario.txt";
            string path = RutaPlantillaCorreo(nombreArchivo);

            if (File.Exists(path))
            {
                // Leer contenido del archivo
                mensaje = File.ReadAllText(path);
            }

            return mensaje;
        }
        #endregion

        #region EstructuraContenidoCorreoEncuesta
        public string EstructuraContenidoCorreoEncuesta()
        {

            string mensaje = "";
            string nombreArchivo = "contenidoCorreoNotificacionEncuesta.txt";
            string path = RutaPlantillaCorreo(nombreArchivo);

            if (File.Exists(path))
            {
                // Leer contenido del archivo
                mensaje = File.ReadAllText(path);
            }

            return mensaje;
        }
        #endregion

        #region TramosPorAutorizar
        /// <summary>
        /// Las filas de un guardado a las que hay que pedirles autorización.
        ///
        /// Normalmente la lista la arma el procedimiento, que puede haber insertado
        /// varias: parte el rango en tramos según el horario del responsable.
        ///
        /// El caso raro, y la razón de que esto exista, es un despliegue a medias.
        /// Esta DLL puede quedar arriba con la CapaDato anterior todavía en el
        /// servidor —se hace así a propósito, para publicar el PDF nuevo sin activar
        /// el partido en tramos—. Esa versión no llena IdsHorasExtras, y sin este
        /// respaldo la guarda nueva daría siempre falso: dejarían de salir los
        /// correos de autorización de horas extras sin ningún error que lo delate.
        ///
        /// Por eso se mira si la propiedad es null y no si está vacía. Son dos cosas
        /// distintas: null es "la capa de datos no me dijo nada", vacío es "me dijo
        /// que no hay ninguna". Tratarlas igual volvería a mandar correos por
        /// permisos que no son horas extras.
        ///
        /// Cuando CapaDato.exe suba con el resto, esta segunda rama deja de usarse y
        /// se puede borrar.
        /// </summary>
        /// <param name="respuesta">Lo que devolvió el alta.</param>
        /// <param name="tipoElegido">El tipo que mandó la pantalla. Solo se usa en el respaldo.</param>
        public static List<long> TramosPorAutorizar(EntRespuesta respuesta, long tipoElegido)
        {
            List<long> ids = new List<long>();

            if (respuesta == null || respuesta.estado != "1") { return ids; }

            if (respuesta.IdsHorasExtras != null)
            {
                foreach (string texto in respuesta.IdsHorasExtras.Split(','))
                {
                    long id;
                    if (long.TryParse(texto.Trim(), out id) && id > 0) { ids.Add(id); }
                }

                return ids;
            }

            /* Respaldo: el criterio anterior, sobre la única fila que insertaba esa
               versión. Lo decidía el combo de la pantalla. */
            if (tipoElegido == 1 || tipoElegido == 2)
            {
                long unica;
                if (long.TryParse(Convert.ToString(respuesta.resultado), out unica) && unica > 0)
                {
                    ids.Add(unica);
                }
            }

            return ids;
        }
        #endregion

        #region SolicitarAutorizacionHorasExtras
        /// <summary>
        /// Le pide al jefe inmediato que autorice UNA fila de horas extras.
        ///
        /// Vive acá y no en los handlers porque un solo guardado puede generar varias:
        /// Sp_RTAInsertaDetalleTarea_V2 parte el rango en tramos según el horario del
        /// responsable, así que 07:30 a 18:30 con jornada 08:30-17:30 deja dos tramos
        /// suplementarios y uno normal. Cada tramo se autoriza por separado, porque
        /// RespuestaAprobacion trabaja por Id_RegDetTareas y el jefe tiene que poder
        /// aprobar la mañana y rechazar la tarde.
        ///
        /// Las horas y el tiempo salen de la fila guardada, NO del formulario: el
        /// formulario tiene el rango completo, y mandarlo en los dos correos le pediría
        /// al jefe autorizar once horas dos veces.
        /// </summary>
        public EntRespuesta SolicitarAutorizacionHorasExtras(
            long idRegDetTarea,
            string codUsuarioSesion,
            string nombreSolicitante,
            string nombreCliente)
        {
            EntRespuesta respuesta = new EntRespuesta
            {
                estado = "0",
                mensaje = "No se pudo solicitar la autorización de las horas extras.",
                tipoMensaje = "warning",
                resultado = idRegDetTarea.ToString()
            };

            try
            {
                EntDetalleTarea fila =
                    NegTareas.RTA_ConsultaDetalleTareaRTA(Convert.ToInt32(idRegDetTarea));

                if (fila == null || fila.Id_RegDetTareas == 0)
                {
                    respuesta.mensaje =
                        "No se encontró la actividad " + idRegDetTarea
                        + " para solicitar la autorización.";

                    return respuesta;
                }

                string horaDesde = ParteDeLaFecha(fila.Det_Fch_RegDetalleIni, "HH:mm");
                string horaHasta = ParteDeLaFecha(fila.Det_Fch_RegDetalleFin, "HH:mm");

                string valorEncritar1 = fila.Id_RegDetTareas.ToString() + ";"
                    + fila.Id_RegTareas.ToString() + ";" + "D" + ";"
                    + fila.Det_Horas_Extras_Tipo.ToString();

                string valorEncritar2 = fila.Id_RegDetTareas.ToString() + ";"
                    + fila.Id_RegTareas.ToString() + ";" + "R" + ";"
                    + fila.Det_Horas_Extras_Tipo.ToString();

                string valorIncritado1 = Encrypt(valorEncritar1, "3m1l10100", "3m1l10100");
                string valorIncritado2 = Encrypt(valorEncritar2, "3m1l10100", "3m1l10100");

                string urlSiteAprobacion =
                    NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_SITE_APROBACIONES");

                string urlAprobacion = urlSiteAprobacion
                    + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado1;

                string urlRechazarAprobacion = urlSiteAprobacion
                    + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado2;

                List<EntItemValor> campos = new List<EntItemValor>();

                campos.Add(new EntItemValor() { Item = "tituloNotificacion", Valor = "SOLICITUD DE HORAS EXTRAS" });
                campos.Add(new EntItemValor() { Item = "etiqueta1", Valor = "Orden de Servicio:" });
                campos.Add(new EntItemValor() { Item = "texto1", Valor = fila.Det_Num_OrdenServicio });
                campos.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Empresa:" });
                campos.Add(new EntItemValor() { Item = "texto2", Valor = fila.Det_Nom_Empresa });
                campos.Add(new EntItemValor() { Item = "etiqueta21", Valor = "Cliente:" });
                campos.Add(new EntItemValor() { Item = "texto21", Valor = nombreCliente ?? string.Empty });
                campos.Add(new EntItemValor() { Item = "etiqueta22", Valor = "Fecha:" });
                campos.Add(new EntItemValor() { Item = "texto22", Valor = ParteDeLaFecha(fila.Det_Fch_RegDetalleIni, "dd/MM/yyyy") });
                campos.Add(new EntItemValor() { Item = "etiqueta23", Valor = "Hora Inicio:" });
                campos.Add(new EntItemValor() { Item = "texto23", Valor = horaDesde });
                campos.Add(new EntItemValor() { Item = "etiqueta24", Valor = "Hora Fin:" });
                campos.Add(new EntItemValor() { Item = "texto24", Valor = horaHasta });
                campos.Add(new EntItemValor() { Item = "etiqueta25", Valor = "Tiempo:" });
                campos.Add(new EntItemValor() { Item = "texto25", Valor = fila.Det_Tiempo });

                /* La descripción la puso el procedimiento al insertar la fila: 50% o
                   100% según el tramo. No se recalcula acá. */
                if (fila.Det_Horas_Extras_Tipo == 1 || fila.Det_Horas_Extras_Tipo == 2)
                {
                    campos.Add(new EntItemValor() { Item = "etiqueta3", Valor = "Tipo de Horas Extra:" });
                    campos.Add(new EntItemValor() { Item = "texto3", Valor = fila.Det_Horas_Extras_Descripcion });
                }

                campos.Add(new EntItemValor() { Item = "etiqueta4", Valor = "Solicitante:" });
                campos.Add(new EntItemValor() { Item = "texto4", Valor = nombreSolicitante ?? string.Empty });
                campos.Add(new EntItemValor() { Item = "etiquetaDescripcion", Valor = "Descripción de la Tarea:" });
                campos.Add(new EntItemValor() { Item = "textoDescripcion", Valor = fila.Det_Det_Tarea ?? string.Empty });
                campos.Add(new EntItemValor() { Item = "etiquetaBoton1", Valor = "Aprobar" });
                campos.Add(new EntItemValor() { Item = "urlBoton1", Valor = urlAprobacion });
                campos.Add(new EntItemValor() { Item = "etiquetaBoton2", Valor = "Rechazar" });
                campos.Add(new EntItemValor() { Item = "urlBoton2", Valor = urlRechazarAprobacion });

                string correoJefeInmediato =
                    NegUsuario.RTA_CorreoJefeInmediato(codUsuarioSesion);

                bool seEnvio = EnvioCorreo(
                    correoJefeInmediato,
                    "Autorización de Horas Extras",
                    EstructuraContenidoCorreo(),
                    campos);

                /* El estado pasa a 1 (solicitud enviada) aunque el correo haya fallado.
                   La fila ya está pedida: dejarla en 0 la deja fuera del listado de
                   pendientes y nadie la vuelve a mirar. Que el correo no salió se le
                   avisa al usuario, que puede insistir.

                   Antes esta llamada estaba detrás de un return, así que con el correo
                   caído el estado no se movía nunca. */
                EntRespuesta cambioEstado =
                    NegTareas.RTAActualizarEstadoHorasExtras(Convert.ToInt32(idRegDetTarea), 1);

                if (!seEnvio)
                {
                    respuesta.mensaje =
                        "No se pudo enviar el correo de autorización de las horas de "
                        + horaDesde + " a " + horaHasta + ".";

                    return respuesta;
                }

                if (cambioEstado == null || cambioEstado.estado == "0")
                {
                    respuesta.mensaje =
                        "Se envió el correo de las horas de " + horaDesde + " a " + horaHasta
                        + ", pero no se pudo cambiar el estado de la solicitud.";

                    return respuesta;
                }

                respuesta.estado = "1";
                respuesta.tipoMensaje = "success";
                respuesta.mensaje = "Autorización solicitada.";

                return respuesta;
            }
            catch (Exception ex)
            {
                VerErrores("SolicitarAutorizacionHorasExtras: " + ex.Message, "Log", "Detalle");

                respuesta.mensaje =
                    "Ocurrió un error al solicitar la autorización de las horas extras.";

                return respuesta;
            }
        }

        /// <summary>
        /// Formatea como texto la fecha que viene de la base. Si no se puede leer se
        /// devuelve tal cual: esto va a un correo, y una fecha cruda es mejor que nada.
        /// </summary>
        private string ParteDeLaFecha(string valor, string formato)
        {
            DateTime fecha;

            if (DateTime.TryParse(valor, out fecha))
            {
                return fecha.ToString(formato);
            }

            return valor ?? string.Empty;
        }
        #endregion

        #region RutaPlantillaCorreo
        /// <summary>
        /// Dónde está la plantilla de un correo.
        ///
        /// Los cuatro métodos de arriba la buscaban en una ruta absoluta de la
        /// máquina de desarrollo del autor original. Cuando esa carpeta no existe
        /// —o sea, en el servidor— File.Exists da falso, el método devuelve cadena
        /// vacía, y el correo sale sin cuerpo sin ningún error que lo delate.
        ///
        /// Se conserva la ruta vieja como primera opción a propósito: si en algún
        /// servidor alguien la creó a mano, lo que hoy funciona sigue funcionando
        /// igual. Recién cuando no está se busca dentro del sitio, que es donde el
        /// csproj despliega las plantillas.
        /// </summary>
        private string RutaPlantillaCorreo(string nombreArchivo)
        {
            string rutaHistorica =
                "C:\\Desarrollo\\Desarrollo\\PRY_Sistema ReporteTareas\\ReporteTareas\\Formulario\\"
                + nombreArchivo;

            if (File.Exists(rutaHistorica))
            {
                return rutaHistorica;
            }

            /* Sin petición web no hay sitio desde el cual resolver la ruta relativa.
               Ahí se devuelve la histórica y todo queda como estaba. */
            if (System.Web.HttpContext.Current == null)
            {
                return rutaHistorica;
            }

            try
            {
                return System.Web.HttpContext.Current.Server.MapPath(
                    "~/Formulario/" + nombreArchivo);
            }
            catch
            {
                return rutaHistorica;
            }
        }
        #endregion

        #region Encrypt
        public string Encrypt(string dataToEncrypt, string password, string salt)
        {
            AesManaged managed = null;
            CryptoStream stream = null;
            string str;
            MemoryStream stream2 = null;
            try
            {
                Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 0x2710);
                managed = new AesManaged
                {
                    Key = bytes.GetBytes(0x20),
                    IV = bytes.GetBytes(0x10)
                };
                stream2 = new MemoryStream();
                stream = new CryptoStream(stream2, managed.CreateEncryptor(), CryptoStreamMode.Write);
                byte[] buffer = Encoding.UTF8.GetBytes(dataToEncrypt);
                stream.Write(buffer, 0, buffer.Length);
                stream.FlushFinalBlock();
                str = Convert.ToBase64String(stream2.ToArray());
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (stream2 != null)
                {
                    stream2.Close();
                }
                if (managed != null)
                {
                    managed.Clear();
                }
            }
            return str;
        }
        #endregion

        #region EnviarCorreo
        /*
         * Función que permite el envio de correo electrónico 
         * Requiere de la entidad EntParametrosCorreo de donde toma la configuración del servidor de correo.
         * Se pueden enviar copias a correos si en el parametro strMailAdress se envía los correos separados por punto y coma.
        */
        public bool EnviarCorreo(string correosDestinatarios, string correoTitulo, string correoContenido, EntParametrosCorreo parametrosServidorCorreo)
        {
            bool Temp = false;

            string smtpAddress = parametrosServidorCorreo.smtpAddress;
            string emailFrom = parametrosServidorCorreo.emailFrom;
            string password = parametrosServidorCorreo.password;
            string subject = correoTitulo;
            string body = correoContenido;
            int portNumber = parametrosServidorCorreo.portNumber;
            bool enableSSL = parametrosServidorCorreo.enableSSL;
            string emailFromName = parametrosServidorCorreo.emailFromName;

            using (MailMessage mail = new MailMessage())
            {
                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                {
                    // El armado de destinatarios va DENTRO del try: una direccion mal
                    // formada hace que mail.To.Add lance FormatException, y si eso ocurre
                    // fuera del try la excepcion sube sin dejar rastro de por que no salio
                    // el correo. Aqui se convierte en un false con ErrorProceso y bitacora.
                    try
                    {
                        mail.From = new MailAddress(emailFrom, emailFromName);

                        foreach (string correoIndividual in (correosDestinatarios ?? string.Empty).Split(new Char[] { ';' }))
                        {
                            if (correoIndividual.Trim() != "")
                            {
                                mail.To.Add(correoIndividual.Trim());
                            }
                        }

                        if (mail.To.Count == 0)
                        {
                            throw new FormatException("No hay destinatarios validos en: '" + (correosDestinatarios ?? string.Empty) + "'.");
                        }

                        mail.Subject = subject;
                        mail.Body = body;
                        mail.IsBodyHtml = true;

                        smtp.Credentials = new NetworkCredential(emailFrom, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                        Temp = true;
                    }
                    catch (Exception ex)
                    {
                        Temp = false;
                        ErrorProceso = ex.Message.ToString().Trim();
                        VerErrores("ErrorProceso: " + ErrorProceso + " | Destinatarios: " + (correosDestinatarios ?? string.Empty), "Log", "Detalle");
                    }
                }
            }


            return Temp;
        }

        public bool EnviarCorreoPoliza(string correosDestinatarios, string correoTitulo, string correoContenido, EntParametrosCorreo parametrosServidorCorreo, string RutaDocumento)
        {
            bool Temp = false;

            string smtpAddress = parametrosServidorCorreo.smtpAddress;
            string emailFrom = parametrosServidorCorreo.emailFrom;
            string password = parametrosServidorCorreo.password;
            string subject = correoTitulo;
            string body = correoContenido;
            int portNumber = parametrosServidorCorreo.portNumber;
            bool enableSSL = parametrosServidorCorreo.enableSSL;
            string emailFromName = "Registro de Poliza - Sistema de Tareas";

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(emailFrom, emailFromName);

                foreach (string correoIndividual in correosDestinatarios.Split(new Char[] { ';' }))
                {
                    if (correoIndividual != "")
                    {
                        mail.To.Add(correoIndividual);
                    }
                }
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                foreach (string documentos in RutaDocumento.Split(new Char[] { ';' }))
                {
                    if (documentos != "")
                    {
                        if (System.IO.File.Exists(documentos))
                        {
                            mail.Attachments.Add(new Attachment(documentos));
                        }
                    }
                }

                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                {
                    try
                    {
                        smtp.Credentials = new NetworkCredential(emailFrom, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                        Temp = true;
                    }
                    catch (Exception ex)
                    {
                        Temp = false;
                        ErrorProceso = ex.Message.ToString().Trim();
                    }
                }
            }


            return Temp;
        }

        public bool EnviarCorreoForeCast(string correosDestinatarios, string correoTitulo, string correoContenido, EntParametrosCorreo parametrosServidorCorreo, string RutaAdjunto, string emailFromName)
        {
            bool Temp = false;

            string smtpAddress = parametrosServidorCorreo.smtpAddress;
            string emailFrom = parametrosServidorCorreo.emailFrom;
            string password = parametrosServidorCorreo.password;
            string subject = correoTitulo;
            string body = correoContenido;
            int portNumber = parametrosServidorCorreo.portNumber;
            bool enableSSL = parametrosServidorCorreo.enableSSL;
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(emailFrom, emailFromName);

                foreach (string correoIndividual in correosDestinatarios.Split(new Char[] { ';' }))
                {
                    if (correoIndividual != "")
                    {
                        mail.To.Add(correoIndividual);
                    }
                }
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                //foreach (string documentos in RutaDocumento.Split(new Char[] { ';' }))
                //{
                //    if (documentos != "")
                //    {
                //        if (System.IO.File.Exists(documentos))
                //        {
                //            mail.Attachments.Add(new Attachment(documentos));
                //        }
                //    }
                //}

                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                {
                    try
                    {
                        smtp.Credentials = new NetworkCredential(emailFrom, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                        Temp = true;
                    }
                    catch (Exception ex)
                    {
                        Temp = false;
                        ErrorProceso = ex.Message.ToString().Trim();
                    }
                }
            }


            return Temp;
        }

        public bool EnviarCorreoPermiso(string correosDestinatarios, string correoTitulo, string correoContenido, EntParametrosCorreo parametrosServidorCorreo, string RutaDocumento)
        {
            bool Temp = false;

            string smtpAddress = parametrosServidorCorreo.smtpAddress;
            string emailFrom = parametrosServidorCorreo.emailFrom;
            string password = parametrosServidorCorreo.password;
            string subject = correoTitulo;
            string body = correoContenido;
            int portNumber = parametrosServidorCorreo.portNumber;
            bool enableSSL = parametrosServidorCorreo.enableSSL;
            string emailFromName = "Registro de Permiso - Sistema de Tareas";

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(emailFrom, emailFromName);

                foreach (string correoIndividual in correosDestinatarios.Split(new Char[] { ';' }))
                {
                    if (correoIndividual != "")
                    {
                        mail.To.Add(correoIndividual);
                    }
                }
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                foreach (string documentos in RutaDocumento.Split(new Char[] { ';' }))
                {
                    if (documentos != "")
                    {
                        if (System.IO.File.Exists(documentos))
                        {
                            mail.Attachments.Add(new Attachment(documentos));
                        }
                    }
                }

                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                {
                    try
                    {
                        smtp.Credentials = new NetworkCredential(emailFrom, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                        Temp = true;
                    }
                    catch (Exception ex)
                    {
                        Temp = false;
                        ErrorProceso = ex.Message.ToString().Trim();
                    }
                }
            }


            return Temp;
        }

        #endregion

        #region AsuntoCorreo
        public string AsuntoCorreo(string detalle)
        {

            string[] datosDoc = detalle.Split('↨');
            string asunto = "<table style='font-family:'Open Sans',sans-serif, Calibri; font-size: 14;text-align:justify;background-color:#F0F8FF;padding:10px;'><tr><td>Estimado(a)<b>{detalle}<br/></td></tr><tr><td>OS: <b>{os}</b></td></tr><tr><td>CLIENTE: <b>{cliente}</b></td></tr><tr><td>PEDIDO: <b>{pedido}</b></td></tr></table>";
            asunto = asunto.Replace("{detalle}", datosDoc[3]);
            asunto = asunto.Replace("{os}", datosDoc[0]);
            asunto = asunto.Replace("{cliente}", datosDoc[1]);
            asunto = asunto.Replace("{pedido}", datosDoc[2]);
            return asunto;
        }
        #endregion

        #region AsuntoCorreoForeCast
        public string AsuntoCorreoForeCast(string detalle)
        {

            string[] datosDoc = detalle.Split('↨');
            string asunto = "<table style='font-family:'Open Sans',sans-serif, Calibri; font-size: 14;text-align:justify;background-color:#F0F8FF;padding:10px;'><tr><td>Estimado(a)<b>{usuario}<br/></td></tr><tr><td><b>{observacion}</b></td></tr></table>";
            asunto = asunto.Replace("{usuario}", datosDoc[0]);
            asunto = asunto.Replace("{observacion}", datosDoc[1]);
            return asunto;
        }
        #endregion

        #region AsuntoCorreoPermiso
        public string AsuntoCorreoPermiso(string detalle)
        {

            string[] datosDoc = detalle.Split('↨');


            string asunto = "<table style='font-family:'Open Sans',sans-serif, Calibri; font-size: 14;text-align:justify;background-color:#F0F8FF;padding:10px;'><tr><td>Estimado(a). {usuario} <b>{detalle}<br/></td></tr></table>";
            asunto = asunto.Replace("{detalle}", datosDoc[0]);
            asunto = asunto.Replace("{usuario}", datosDoc[1]);
            return asunto;
        }
        #endregion

        #region VerErrores
        public void VerErrores(string valor, string Carpeta, string rucEmpresa)
        {
            try
            {
                string fecha;
                fecha = DateTime.Now.ToString("dd-MM-yyyy");//DateTime.Now.ToShortDateString().Replace("/", "-");
                if (!Directory.Exists(@"C:\\" + rucEmpresa + "\\" + Carpeta + "\\" + fecha))
                {
                    Directory.CreateDirectory(@"C:\\" + rucEmpresa + "\\" + Carpeta + "\\" + fecha);
                }

                string path = @"C:\\" + rucEmpresa + "\\" + Carpeta + "\\" + fecha + "\\log.txt";
                TextWriter tw = new StreamWriter(path, true);
                tw.WriteLine("A fecha de : " + DateTime.Now.ToString() + ": " + valor);
                tw.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", "Exception: " + ex.Message);
            }
        }
        #endregion
    }
}
