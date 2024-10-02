using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using CapaEntidad;
using CapaNegocio;
using CorreoHelper;

namespace ReporteTareas.Formulario
{
    public partial class RespuestaAprobacion : System.Web.UI.Page
    {

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
                        else if (parametrosSolicitud[2] == "SA")
                        {
                            EntRespuesta respuesta = new EntRespuesta();
                            EntSolicitud registro = new EntSolicitud();
                            registro.IdVacaciones = Convert.ToInt32(parametrosSolicitud[0]);
                            registro.EstadoSolicitud = "APROBADO";
                            registro.Cod_Usuario = parametrosSolicitud[3];
                            registro.Tipo =5;
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
                        else if (parametrosSolicitud[2] == "SR")
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
        public void EnviarCorreoRH(EntSolicitud Lista,string IdSolicitud,string correoUsuario,string IdUsuarioSession)
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