using CapaEntidad;
using CapaNegocio;
using System;
using System.Web;
using System.Web.Caching;

namespace JsonJQueryNetHorasExtras
{
    /// <summary>
    /// La descarga del periodo en Excel. Handler aparte de
    /// AdministrarHorasExtras.ashx porque aquel responde JSON a peticiones POST
    /// y esto es un GET que escribe bytes: juntarlos obligaria a que uno de los
    /// dos rompiera su propio contrato.
    ///
    /// IRequiresSessionState no es decorativo: sin el, context.Session es null
    /// en un IHttpHandler y no habria con que comprobar el perfil.
    ///
    /// El archivo lleva el sueldo de todas las personas del periodo, asi que la
    /// comprobacion de perfil esta AQUI y no en el boton que lo pide.
    /// </summary>
    public class DescargarHorasExtras : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.Session == null || context.Session["UserLogin"] == null)
            {
                NoDisponible(context, "Su sesión expiró. Vuelva a iniciar sesión.", 401);
                return;
            }

            if (!TienePermiso(context))
            {
                NoDisponible(context, "Su perfil no puede descargar este archivo.", 403);
                return;
            }

            int idPeriodo;
            if (!int.TryParse(context.Request.QueryString["periodo"], out idPeriodo) || idPeriodo <= 0)
            {
                NoDisponible(context, "No se indicó qué período descargar.", 400);
                return;
            }

            EntHePantalla pantalla;
            byte[] archivo;

            /* Todo lo que toca la base -cargar el periodo, dejar registrada la
               descarga, armar el xlsx- va en este unico try. Sin el, con
               customErrors en Off (Web.config:157), una SqlException cualquiera
               (procedimiento faltante, timeout, deadlock) sale como la pantalla
               amarilla de ASP.NET con traza en vez de un mensaje: mismo motivo
               por el que lo tiene AdministrarHorasExtras.ashx.cs, el otro
               handler del modulo.

               El Response.End() de la respuesta buena, mas abajo, queda FUERA
               de este try a proposito: End() aborta el hilo con una
               ThreadAbortException que .NET vuelve a lanzar despues de
               cualquier catch, y si ese catch estuviera aqui adentro alcanzaria
               a ejecutarse con la respuesta ya enviada -Response.Clear()
               reventaria con "no se pueden agregar encabezados despues de
               enviarlos". Todo lo que puede fallar de verdad ya paso para
               cuando se llega ahi abajo. */
            try
            {
                EntRespuesta respuesta = NegHorasExtrasPantalla.CargarPantalla(idPeriodo);

                if (respuesta.estado != "1" || respuesta.resultado == null)
                {
                    NoDisponible(context, "No se encontró ese período.", 404);
                    return;
                }

                pantalla = (EntHePantalla)respuesta.resultado;

                /* Se registra ANTES de armar el archivo, no despues: esta es la
                   unica accion de todo el modulo que saca el sueldo de las 62
                   personas del periodo de un solo clic, y hasta ahora la unica
                   sin rastro -la edicion celda por celda ya lo tiene, dentro de
                   Sp_RTA_HeGuardarFila-. Si el registro falla, el catch de
                   abajo lo atrapa y no se llega a entregar nada: sin registro,
                   no hay descarga. */
                NegHorasExtrasPantalla.RegistrarDescarga(idPeriodo, Usuario(context), Ip(context));

                archivo = NegHeExportacion.Construir(pantalla);
            }
            catch (Exception ex)
            {
                NoDisponible(context, "No se pudo generar el archivo. " + ex.Message, 500);
                return;
            }

            context.Response.Clear();
            context.Response.Buffer = true;
            context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            context.Response.AddHeader("Content-Disposition",
                "attachment;filename=\"" + NegHeExportacion.NombreDeArchivo(pantalla.Periodo) + "\"");

            /* Un xlsx con 62 sueldos no deberia quedarse en la cache de disco
               del navegador ademas de en Descargas. */
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();

            context.Response.BinaryWrite(archivo);
            context.Response.End();
        }

        /// <summary>
        /// Mismo criterio que AdministrarHorasExtras.ashx.cs, misma lista: se
        /// reusa PerfilesAutorizados de ahi (internal, mismo namespace) en vez
        /// de declarar una copia que algun dia alguien cambie en un solo lugar
        /// y no en el otro.
        /// </summary>
        private static bool TienePermiso(HttpContext context)
        {
            int idPerfil;
            if (!int.TryParse(Convert.ToString(context.Session["Id_Perfil"]), out idPerfil)) { return false; }
            return Array.IndexOf(AdministrarHorasExtras.PerfilesAutorizados, idPerfil) >= 0;
        }

        /// <summary>De quien es esta descarga, para la auditoria. Sale de la sesion, nunca del cliente.</summary>
        private static string Usuario(HttpContext context)
        {
            object v = context.Session["Cod_Usuario"];
            return v == null ? "" : Convert.ToString(v).Trim();
        }

        private static string Ip(HttpContext context)
        {
            string ip = context.Request.UserHostAddress;
            return string.IsNullOrEmpty(ip) ? "" : ip;
        }

        /// <summary>
        /// Un texto plano, no una pagina de error: quien esto recibe es una
        /// descarga, y el navegador va a mostrar lo que llegue.
        ///
        /// ContentEncoding, no solo el charset del ContentType: este sitio fija
        /// responseEncoding="windows-1252" en Web.config, asi que sin esta
        /// linea el cuerpo se escribe en cp1252 y el navegador lo interpreta
        /// como UTF-8 -las tildes y la enie de estos mismos mensajes salen
        /// rotas-. DescargarPerfil.ashx.cs, el molde, pone las dos lineas
        /// juntas; la primera version de este handler solo copio la mitad.
        ///
        /// statusCode explicito porque sin el, Write() responde 200: la
        /// peticion queda registrada como exitosa aunque el cuerpo diga lo
        /// contrario.
        /// </summary>
        private static void NoDisponible(HttpContext context, string mensaje, int statusCode)
        {
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.Write(mensaje);
        }

        public bool IsReusable { get { return false; } }
    }
}
