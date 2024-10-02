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

namespace WinCorreo
{
    public class ClsEnvioCorreo
    {
        public string ErrorProceso;

        public void EnvioCorreo()
        {

            List<EntTareas> listaTareas = new List<EntTareas>();
            listaTareas = NegTareas.RTAConsultaTareaHorasExtras();

            EntParametrosConfiguracion parametroConfiguracion = new EntParametrosConfiguracion();
            parametroConfiguracion = NegParametrosConfiguracion.RTA_ConsultaParametroConfiguracion("URL_SITE_APROBACIONES");

            foreach (EntTareas entTareas in listaTareas)
            {
                string ValorEncritar1 = entTareas.Id_RegTareasHorasExtras.ToString() + ";" + entTareas.Id_RegTareas + ";" + "D" + ";" + entTareas.TipoAprobacion;
                string ValorEncritar2 = entTareas.Id_RegTareasHorasExtras.ToString() + ";" + entTareas.Id_RegTareas + ";" + "R" + ";" + entTareas.TipoAprobacion;
                string ValorIncritado1 = Encrypt(ValorEncritar1, "3m1l10100", "3m1l10100");
                string ValorIncritado2 = Encrypt(ValorEncritar2, "3m1l10100", "3m1l10100");
                bool ValorRes = false;

                parametroConfiguracion = NegParametrosConfiguracion.RTA_ConsultaParametroConfiguracion("URL_SITE_APROBACIONES");

                ValorRes = EnviarMailOtros(entTareas.MailResponsableAprobacion, entTareas.MailAprobacionTitulo, EnvioActividad(entTareas.Num_OrdenServicio, entTareas.Nom_Empresa, entTareas.Nom_Responsable, entTareas.Det_Tarea, ValorIncritado1, ValorIncritado2, parametroConfiguracion.Valor.ToString()));
                if (ValorRes == true)
                {
                    var Respuesta = 0;
                    EntTareas entTareas2 = new EntTareas();
                    entTareas2.Id_RegTareasHorasExtras = entTareas.Id_RegTareasHorasExtras;
                    entTareas2.Id_RegTareas = entTareas.Id_RegTareas;
                    //entTareas2.Tipo = 3;
                    entTareas2.Tipo = entTareas.TipoAprobacion;
                    Respuesta = NegTareas.RTAActuaAprobacionHorasExtras(entTareas2);
                }
            }
        }

        #region EnviarMailOtros
        public bool EnviarMailOtros(string strMailAdress, string strTitulo, string strContenido)
        {
            bool Temp = false;

            string smtpAddress = "compuequip-com.mail.protection.outlook.com";
            string emailFrom = "comprobantesemitidos@compuequip.com";
            string password = "dos.2014";
            string subject = strTitulo;
            string body = strContenido;
            int portNumber = 25;
            bool enableSSL = true;

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(emailFrom, "Solicitud Aprobación - Sistema de Tarea");

                foreach (string r in strMailAdress.Split(new Char[] { ';' }))
                {
                    if (r != "")
                    {
                        mail.To.Add(r);
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
                    }
                }
            }


            return Temp;
        }
        #endregion

        #region EnvioActividad
        public string EnvioActividad(string numeroOrden, string empresa, string responsable, string tarea, string valorIncritado1, string valorIncritado2, string urlSiteAprobacion)
        {
            string ValorIncritado = valorIncritado1;
            string NumeroOrden = numeroOrden;
            string Empresa = empresa;
            string Responsable = responsable;
            string Tarea = tarea;
            string mensaje = "";
            mensaje = mensaje + "<div dir ='ltr'><div><br/><div>";
            mensaje = mensaje + "<table align='center' style='word-wrap:break-word; word-break:break-word; background-color:#F0F0F0'>";
            mensaje = mensaje + "<tbody><tr>";
            mensaje = mensaje + "<td style='font-size:14px; line-height:21px; padding:10; text-align:left; background-color:#00ACD7; vertical-align:top; font-family:'>";
            mensaje = mensaje + "<div align='center'><span style='font:Arial,Helvetica,sans-serif; font-size:16px; padding:10px; font-weight:bold; color:#030303'> Aprobar Horas Extras</span></div>";
            mensaje = mensaje + "</td></tr>";
            mensaje = mensaje + "<tr>";
            mensaje = mensaje + "<td width='600' style='font-size:14px; line-height:21px; padding:10; text-align:left; vertical-align:top; color:#757575; font-family:'>";
            mensaje = mensaje + "<div style='padding:0'>";

            mensaje = mensaje + "<p align='center' style='font-size:14px; line-height:20px'><span><strong>Numero Orden</strong></span></p>";
            mensaje = mensaje + "<p align='center' style='font-size:14px; line-height:20px'><span>" + NumeroOrden + "</span></p>";

            mensaje = mensaje + "<p align='center' style='font-size:14px; line-height:20px'><span><strong>Empresa</strong></span></p>";
            mensaje = mensaje + "<p align='center' style='font-size:14px; line-height:20px'><span>" + Empresa + "</span></ p >";

            mensaje = mensaje + "<p align='center' style='font-size:14px; line-height:20px'><span><strong>Nombre del Responsable</strong></span></p>";
            mensaje = mensaje + "<p align='center' style='font-size:14px; line-height:20px'><span>" + Responsable + "</span></p>";

            mensaje = mensaje + "<p align='center' style='font-size:14px; line-height:20px'><span><strong>Detalle de la Tarea </strong></span></p>";
            mensaje = mensaje + "<p align='center' style='font-size:14px; line-height:20px'><span>" + Tarea + "</span></p>";

            mensaje = mensaje + "</div></td></tr>";
            mensaje = mensaje + "<tr>";
            mensaje = mensaje + "<td>";
            mensaje = mensaje + "<span style='text-font-family: Calibri; font-size: 12; font-weight: bold;'><P ALIGN=left>";
            mensaje = mensaje + "<b><center><a href='" + urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado1 + "'><input type='button' value='APROBAR'></a><b>";
            //mensaje = mensaje + "</span></P>";
            //mensaje = mensaje + "<span style='text-font-family: Calibri; font-size: 12; font-weight: bold;'><P ALIGN=left>";
            mensaje = mensaje + "&nbsp;&nbsp;&nbsp;&nbsp;<b><a href='" + urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado2 + "'><input type='button' value='RECHAZAR'></a></center><b>";
            mensaje = mensaje + "</span></P>";
            mensaje = mensaje + "</td>";
            mensaje = mensaje + "</tr>";
            mensaje = mensaje + "</tbody>";
            mensaje = mensaje + "</table>";

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
    }
}
