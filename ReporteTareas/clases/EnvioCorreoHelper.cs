using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SendGridMail;
using SendGridMail.Transport;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.IO;
using CapaEntidad;
using CapaNegocio;
using PDF;
using ReporteTareas.clases;

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
                else {
                    
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
            string path = "C:\\Desarrollo\\Desarrollo\\PRY_Sistema ReporteTareas\\ReporteTareas\\Formulario\\" + nombreArchivo;

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
            string path = "C:\\Desarrollo\\Desarrollo\\PRY_Sistema ReporteTareas\\ReporteTareas\\Formulario\\" + nombreArchivo;

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
            string path = "C:\\Desarrollo\\Desarrollo\\PRY_Sistema ReporteTareas\\ReporteTareas\\Formulario\\" + nombreArchivo;

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
            string path = "C:\\Desarrollo\\Desarrollo\\PRY_Sistema ReporteTareas\\ReporteTareas\\Formulario\\" + nombreArchivo;

            if (File.Exists(path))
            {
                // Leer contenido del archivo
                mensaje = File.ReadAllText(path);
            }

            return mensaje;
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
                        VerErrores("ErrorProceso: " + ErrorProceso, "Log", "Detalle");
                    }
                }
            }


            return Temp;
        }

        public bool EnviarCorreoPoliza(string correosDestinatarios, string correoTitulo, string correoContenido, EntParametrosCorreo parametrosServidorCorreo,string RutaDocumento)
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
