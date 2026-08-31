using CapaEntidad;
using CapaNegocio;
using CorreoHelper;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ReporteTareas.Formulario
{
    public partial class RespuestaAprobacion : System.Web.UI.Page
    {

        #region Firma del jefe
        /// <summary>
        /// Muestra el pad y espera. La decision se aplica recien al confirmar.
        /// </summary>
        private void PedirFirmaDelJefe(bool esAprobacion)
        {
            litTituloFirma.Text = esAprobacion
                ? "Firme para aprobar la solicitud"
                : "Firme para registrar el rechazo";

            litAyudaFirma.Text = esAprobacion
                ? "Su firma queda en el documento que descarga el colaborador. Dibujela con el mouse o el dedo, o suba una imagen."
                : "Su firma queda registrada junto al rechazo. Dibujela con el mouse o el dedo, o suba una imagen.";

            btnConfirmar.Text = esAprobacion ? "Firmar y aprobar" : "Firmar y rechazar";

            lblmensaje.Text = "";
            pnlFirma.Visible = true;
        }

        /// <summary>
        /// El jefe firmo: se guarda la firma y recien ahi se aplica la decision.
        ///
        /// El orden importa. Si fallara el cambio de estado queda una firma de un
        /// paso que no se completo, y reintentar lo termina; al reves quedaria una
        /// solicitud aprobada sin firma, que es justo lo que esto viene a evitar.
        /// </summary>
        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            try
            {
                string[] parametrosSolicitud = hfParametros.Value.Split(new char[] { ';' });
                bool esAprobacion = parametrosSolicitud[2] == "SA";

                long idVacaciones = Convert.ToInt64(parametrosSolicitud[0]);

                /* Si ya hay firma del jefe no se vuelve a aplicar nada. Pasa cuando se
                   abre dos veces el mismo enlace del correo, que es comun. */
                if (YaFirmoElJefe(idVacaciones))
                {
                    pnlFirma.Visible = false;
                    lblmensaje.Text = "Esta solicitud <b>ya fue firmada</b>. No se registro dos veces.";
                    return;
                }

                if (!RegistrarFirmaDelJefe(idVacaciones, parametrosSolicitud[3], esAprobacion))
                {
                    lblmensaje.Text = "<b>No se pudo registrar su firma.</b> Vuelva a intentarlo, o ingrese a la aplicacion para aprobar desde ahi.";
                    return;
                }

                pnlFirma.Visible = false;

                if (esAprobacion) { AplicarAprobacion(parametrosSolicitud); }
                else { AplicarRechazo(parametrosSolicitud); }
            }
            catch (Exception ex)
            {
                lblmensaje.Text = "Error al registrar la Aprobación o Rechazo. Error:" + ex.Message.ToString();
            }
        }

        private bool YaFirmoElJefe(long idVacaciones)
        {
            List<EntFirmaSolicitud> firmas = NegFirmaSolicitud.Listar(idVacaciones);
            if (firmas == null) { return false; }

            foreach (EntFirmaSolicitud f in firmas)
            {
                if (f.Rol == "JEFE" && f.Secuencia == 1) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Guarda la firma como JEFE, secuencia 1: la misma que registra la pantalla
        /// cuando el jefe aprueba desde la aplicacion.
        ///
        /// A quien se le atribuye: el enlace del correo lleva el codigo del
        /// SOLICITANTE, no el del jefe, asi que el jefe se resuelve por su
        /// Cod_Jefe_Inm. Es una inferencia, no una autenticacion: quien tenga el
        /// enlace reenviado puede firmar por el. El camino con identidad real es que
        /// el jefe entre a la aplicacion y firme ahi.
        /// </summary>
        private bool RegistrarFirmaDelJefe(long idVacaciones, string codSolicitante, bool esAprobacion)
        {
            try
            {
                byte[] trazo = TrazoDesdeDataUri(hfTrazo.Value);
                if (trazo == null || trazo.Length == 0) { return false; }

                string codJefe = CodigoDelJefe(codSolicitante);
                if (string.IsNullOrEmpty(codJefe)) { return false; }

                string ip = Request.Headers["X-Forwarded-For"];
                if (string.IsNullOrEmpty(ip)) { ip = Request.UserHostAddress; }
                if (!string.IsNullOrEmpty(ip) && ip.Length > 45) { ip = ip.Substring(0, 45); }

                string dispositivo = Request.UserAgent ?? "";
                if (dispositivo.Length > 300) { dispositivo = dispositivo.Substring(0, 300); }

                EntFirmaSolicitud firma = new EntFirmaSolicitud()
                {
                    IdVacaciones = idVacaciones,
                    Rol = "JEFE",
                    Secuencia = 1,
                    Decision = esAprobacion ? "APROBADO" : "RECHAZADO",
                    Comentario = "Firmado desde el correo de aprobacion.",
                    TrazoTipo = "image/png",
                    Cod_Usuario = codJefe
                };

                EntRespuesta guardada = NegFirmaSolicitud.Guardar(firma, trazo, ip, dispositivo);
                return guardada != null && guardada.estado == "1";
            }
            catch (Exception ex)
            {
                VerErrores("RegistrarFirmaDelJefe: " + ex.Message, "Log", "Detalle");
                return false;
            }
        }

        /// <summary>El jefe inmediato de un colaborador, o cadena vacia.</summary>
        private string CodigoDelJefe(string codSolicitante)
        {
            List<EntUsuario> datos = NegUsuario.ConsultarDatosReemplazo(codSolicitante, 1);
            if (datos == null || datos.Count == 0) { return ""; }

            return datos[0].Cod_Jefe_Inm ?? "";
        }

        /// <summary>Los bytes de un data URI "data:image/png;base64,...".</summary>
        private byte[] TrazoDesdeDataUri(string dataUri)
        {
            if (string.IsNullOrEmpty(dataUri)) { return null; }

            int coma = dataUri.IndexOf(',');
            string base64 = coma >= 0 ? dataUri.Substring(coma + 1) : dataUri;

            try { return Convert.FromBase64String(base64); }
            catch { return null; }
        }

        private void AplicarAprobacion(string[] parametrosSolicitud)
        {
                EntRespuesta respuesta = new EntRespuesta();
                EntSolicitud registro = new EntSolicitud();
                registro.IdVacaciones = Convert.ToInt32(parametrosSolicitud[0]);
                registro.EstadoSolicitud = "APROBADO";
                registro.Cod_Usuario = parametrosSolicitud[3];
                registro.Tipo = 5;
                registro.UsuarioAprobo = "";
                registro.UsuarioRechazo = "";
                respuesta = NegSolicitud.RTA_ActualizarSolicitud(registro);
                if (respuesta.estado == "1")
                {
                    lblmensaje.Text = "Se ha registrado la <b>APROBACION</b> de la solicitud.";

                    #region Enviar mail Recurso Humano
                    if (parametrosSolicitud[4] == "EM")
                    {
                        EntSolicitud Lista = new EntSolicitud();
                        List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
                        string CorreoRH = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CORREORH");
                        int tipo = 0;
                        EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
                        Lista = NegSolicitud.ConsultaSp_RTANotificarSolicitud(tipo, Convert.ToInt32(parametrosSolicitud[0]));
                        EnviarCorreoRH(Lista, parametrosSolicitud[0], CorreoRH, parametrosSolicitud[3]);
                        Aprobados.Visible = true;
                    }
                    #endregion
                    else if (parametrosSolicitud[4] == "NO")
                    {
                        PermisoAprobado.Visible = true;
                    }
                }
                else
                {
                    lblmensaje.Text = "<b>No se pudo registrar</b> la APROBACION de la solicitud.";
                    Aprobados.Visible = false;
                }
        }

        private void AplicarRechazo(string[] parametrosSolicitud)
        {
                EntRespuesta respuesta = new EntRespuesta();
                EntSolicitud registro = new EntSolicitud();
                registro.IdVacaciones = Convert.ToInt32(parametrosSolicitud[0]);
                registro.EstadoSolicitud = "RECHAZADO";
                registro.Cod_Usuario = parametrosSolicitud[3];
                registro.Tipo = 6;
                registro.UsuarioAprobo = "";
                registro.UsuarioRechazo = "";
                respuesta = NegSolicitud.RTA_ActualizarSolicitud(registro);
                if (respuesta.estado == "1")
                {
                    lblmensaje.Text = "Se ha registrado el <b>RECHAZO</b> de la solicitud.";
                    if (parametrosSolicitud[4] == "NO")
                    {
                        PermisoRechazado.Visible = true;
                    }
                    else if (parametrosSolicitud[4] == "EM")
                    {
                        Rechazado.Visible = true;
                    }
                }
                else
                {
                    lblmensaje.Text = "<b>No se pudo registrar</b> el RECHAZO de la solicitud.";
                    if (parametrosSolicitud[4] == "NO")
                    {
                        PermisoRechazado.Visible = true;
                    }
                    else if (parametrosSolicitud[4] == "EM")
                    {
                        Rechazado.Visible = true;
                    }
                }
        }
        #endregion

        #region Page_Load
        protected void Page_Load(object sender, EventArgs e)
        {
            EntRespuesta respuestaActualizaHorasExtras = new EntRespuesta();
            int Id_RegDetTarea = 0;

            if (!IsPostBack)
            {
                try
                {
                    string userHostAddress = this.Request.UserHostAddress;
                    string encryptedString = this.Request.QueryString["idValor"];
                    string parametrosRecibidos = Decrypt(encryptedString, "3m1l10100", "3m1l10100");
                    //VerErrores("parametrosRecibidos: " + parametrosRecibidos, "Log", "Detalle");
                    string[] parametrosSolicitud = parametrosRecibidos.Split(new char[] { ';' });

                    if (parametrosRecibidos != null)
                    {
                        if (parametrosSolicitud[2] == "D")
                        {
                            Id_RegDetTarea = Convert.ToInt32(parametrosSolicitud[0]);

                            // Se envia el codigo de la solicitud y el valor 2-Solicitud Aprobada Horas Extras
                            respuestaActualizaHorasExtras = NegTareas.RTAActualizarEstadoHorasExtras(Id_RegDetTarea, 2);
                            if (respuestaActualizaHorasExtras.estado == "0")
                            {
                                lblmensaje.Text = "<b>No se pudo registrar</b> la APROBACION de la solicitud de Horas Extras.";
                            }
                            else
                            {
                                lblmensaje.Text = "Se ha registrado la <b>APROBACION</b> de la solicitud de Horas Extras.";
                            }
                        }
                        else if (parametrosSolicitud[2] == "R")
                        {
                            Id_RegDetTarea = Convert.ToInt32(parametrosSolicitud[0]);

                            // Se envia el codigo de la solicitud y el valor 3-Solicitud Rechazada Horas Extras
                            respuestaActualizaHorasExtras = NegTareas.RTAActualizarEstadoHorasExtras(Id_RegDetTarea, 3);
                            if (respuestaActualizaHorasExtras.estado == "0")
                            {
                                lblmensaje.Text = "<b>No se pudo registrar</b> el RECHAZO de la solicitud de Horas Extras.";
                            }
                            else
                            {
                                lblmensaje.Text = "Se ha registrado el <b>RECHAZO</b> de la solicitud de Horas Extras.";
                            }

                        }

                        else if (parametrosSolicitud[2] == "NOT")
                        {
                            EntSolicitud Lista = new EntSolicitud();
                            List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
                            int tipo = 0;
                            string correoUsuario = "";
                            correoUsuario = NegUsuario.RTA_CorreoUsuario(parametrosSolicitud[3]);
                            //VerErrores("correoUsuario: " + correoUsuario, "Log", "Detalle");
                            EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
                            Lista = NegSolicitud.ConsultaSp_RTANotificarSolicitud(tipo, Convert.ToInt32(parametrosSolicitud[0]));
                            EnviarCorreoColaborador(Lista, parametrosSolicitud[0], correoUsuario, parametrosSolicitud[3]);
                            lblmensaje.Text = "Se ha registrado la <b>APROBACION</b> de la solicitud.";
                            Aprobados.Visible = true;
                        }
                        else if (parametrosSolicitud[2] == "SA" || parametrosSolicitud[2] == "SR")
                        {
                            /* Ya no se aplica de una. La solicitud no cambia de estado
                               hasta que el jefe firme: sin eso el PDF salia con el
                               recuadro del jefe en "Pendiente" aunque hubiera aprobado.

                               Los parametros viajan en el formulario para no volver a
                               descifrar la URL en el postback. */
                            hfParametros.Value = parametrosRecibidos;
                            PedirFirmaDelJefe(parametrosSolicitud[2] == "SA");
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblmensaje.Text = "Error al registrar la Aprobación o Rechazo de Horas Extras. Error:" + ex.Message.ToString();
                }
            }
        }
        #endregion

        #region Decrypt
        public string Decrypt(string dataToDecrypt, string password, string salt)
        {
            string str = "";
            try
            {
                dataToDecrypt = Microsoft.VisualBasic.Strings.Replace(dataToDecrypt, " ", "+", 1, -1, CompareMethod.Binary);
                AesManaged managed = null;
                MemoryStream stream = null;
                try
                {
                    Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 0x2710);
                    managed = new AesManaged
                    {
                        Key = bytes.GetBytes(0x20),
                        IV = bytes.GetBytes(0x10)
                    };
                    stream = new MemoryStream();
                    CryptoStream stream2 = new CryptoStream(stream, managed.CreateDecryptor(), CryptoStreamMode.Write);
                    byte[] buffer = Convert.FromBase64String(dataToDecrypt);
                    stream2.Write(buffer, 0, buffer.Length);
                    stream2.FlushFinalBlock();
                    byte[] buffer2 = stream.ToArray();
                    if (stream2 != null)
                    {
                        stream2.Dispose();
                    }
                    return Encoding.UTF8.GetString(buffer2, 0, buffer2.Length);
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                    }
                    if (managed != null)
                    {
                        managed.Clear();
                    }
                }
            }
            catch (Exception exception1)
            {
                return str;
            }
            return str;
        }
        #endregion

        #region EnviarCorreoRH
        public void EnviarCorreoRH(EntSolicitud Lista, string IdSolicitud, string correoUsuario, string IdUsuarioSession)
        {
            EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
            List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
            // Se encripta los parametros de aprobación y rechazo para enviar en la url de aprobación.
            string valorEncritar1 = Lista.IdVacaciones.ToString() + ";" + Lista.IdVacaciones.ToString() + ";" + "NOT" + ";" + IdUsuarioSession.ToString();
            string valorEncritar2 = Lista.IdVacaciones.ToString() + ";" + Lista.IdVacaciones.ToString() + ";" + "SR" + ";" + IdUsuarioSession.ToString();
            string valorIncritado1 = envioCorreo.Encrypt(valorEncritar1, "3m1l10100", "3m1l10100");
            string valorIncritado2 = envioCorreo.Encrypt(valorEncritar2, "3m1l10100", "3m1l10100");

            // Se trae el parametro de la URL del sitio de aprobaciones.
            string urlSiteAprobacion = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_SITE_APROBACIONES");
            // Se estrcutura las URL de aprobación y rechazo de las horas extras
            string urlAprobacion = urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado1;
            string urlRechazarAprobacion = urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado2;
            // Llenado de listado de parámetros a ser reemplazados en el contenido del correo electrónico.
            listaCamposCorreo.Add(new EntItemValor() { Item = "tituloNotificacion", Valor = "REGISTRAR SOLICITUD DE VACACIONES" });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta1", Valor = "Fecha:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto1", Valor = Lista.FechaRegistro });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta11", Valor = "Cédula:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto11", Valor = Lista.Cedula });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Colaborador:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto2", Valor = Lista.Colaborador });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta21", Valor = "Departamento:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto21", Valor = Lista.Departamento });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta3", Valor = "Jefe Inmedianto:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto3", Valor = Lista.JefeInmediato });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta4", Valor = "Reemplazo:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto4", Valor = Lista.Remplazo });

            listaCamposCorreo.Add(new EntItemValor() { Item = "texto22", Valor = "Dias de Vacaciones" });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta23", Valor = "Desde:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto23", Valor = Lista.FechaDesde });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta25", Valor = "Hasta:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto25", Valor = Lista.FechaHasta });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta26", Valor = "Días Solicitados:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto26", Valor = Lista.TotalDias.ToString() });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta27", Valor = "Saldo de días:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto27", Valor = Lista.SaldoDias.ToString() });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaBoton1", Valor = "Enviar solicitud de aprobación" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "urlBoton1", Valor = urlAprobacion });

            bool respuestaEnvioCorreo = false;
            bool respuestaEnvioCorreoUsuario = false;
            //respuestaEnvioCorreoUsuario = envioCorreo.EnvioCorreoSolicitudEmpleado(correoUsuario, "Copia Solicitud de Autorización de Vacaciones", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionSolicitudUsuario.txt"), listaCamposCorreo, "contenidoCorreoNotificacionSolicitudUsuario.txt", Convert.ToInt32(IdSolicitud));
            respuestaEnvioCorreo = envioCorreo.EnvioCorreoSolicitudJefe(correoUsuario, "Registrar Autorización de Vacaciones", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionSolicitudRH.txt"), listaCamposCorreo, "contenidoCorreoNotificacionSolicitudRH.txt");
        }
        #endregion

        #region EnviarCorreoColaborador
        public void EnviarCorreoColaborador(EntSolicitud Lista, string IdSolicitud, string correoUsuario, string IdUsuarioSession)
        {
            EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
            List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
            // Se encripta los parametros de aprobación y rechazo para enviar en la url de aprobación.
            string valorEncritar1 = Lista.IdVacaciones.ToString() + ";" + Lista.IdVacaciones.ToString() + ";" + "NOT" + ";" + IdUsuarioSession.ToString();
            string valorEncritar2 = Lista.IdVacaciones.ToString() + ";" + Lista.IdVacaciones.ToString() + ";" + "SR" + ";" + IdUsuarioSession.ToString();
            string valorIncritado1 = envioCorreo.Encrypt(valorEncritar1, "3m1l10100", "3m1l10100");
            string valorIncritado2 = envioCorreo.Encrypt(valorEncritar2, "3m1l10100", "3m1l10100");

            // Se trae el parametro de la URL del sitio de aprobaciones.
            string urlSiteAprobacion = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_SITE_APROBACIONES");
            // Se estrcutura las URL de aprobación y rechazo de las horas extras
            string urlAprobacion = urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado1;
            string urlRechazarAprobacion = urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado2;
            // Llenado de listado de parámetros a ser reemplazados en el contenido del correo electrónico.
            listaCamposCorreo.Add(new EntItemValor() { Item = "tituloNotificacion", Valor = "SOLICITUD DE VACACIONES APROBADAS Y REGISTRADAS" });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta1", Valor = "Fecha:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto1", Valor = Lista.FechaRegistro });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta11", Valor = "Cédula:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto11", Valor = Lista.Cedula });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Colaborador:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto2", Valor = Lista.Colaborador });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta21", Valor = "Departamento:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto21", Valor = Lista.Departamento });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta3", Valor = "Jefe Inmedianto:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto3", Valor = Lista.JefeInmediato });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta4", Valor = "Reemplazo:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto4", Valor = Lista.Remplazo });

            listaCamposCorreo.Add(new EntItemValor() { Item = "texto22", Valor = "Dias de Vacaciones" });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta23", Valor = "Desde:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto23", Valor = Lista.FechaDesde });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta25", Valor = "Hasta:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto25", Valor = Lista.FechaHasta });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta26", Valor = "Días Solicitados:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto26", Valor = Lista.TotalDias.ToString() });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta27", Valor = "Saldo de días:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto27", Valor = Lista.SaldoDias.ToString() });

            bool respuestaEnvioCorreo = false;
            bool respuestaEnvioCorreoUsuario = false;
            //respuestaEnvioCorreoUsuario = envioCorreo.EnvioCorreoSolicitudEmpleado(correoUsuario, "Copia Solicitud de Autorización de Vacaciones", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionSolicitudUsuario.txt"), listaCamposCorreo, "contenidoCorreoNotificacionSolicitudUsuario.txt", Convert.ToInt32(IdSolicitud));
            respuestaEnvioCorreo = envioCorreo.EnvioCorreoSolicitudEmpleado(correoUsuario, "Vacaciones Aprobadas y Registradas", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionSolicitudUsuario.txt"), listaCamposCorreo, "contenidoCorreoNotificacionSolicitudUsuario.txt", Convert.ToInt32(IdSolicitud));
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