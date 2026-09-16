using CapaEntidad;
using CapaNegocio;
using System;
using System.Web;

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
                NoDisponible(context, "Su sesión expiró. Vuelva a iniciar sesión.");
                return;
            }

            if (!TienePermiso(context))
            {
                NoDisponible(context, "Su perfil no puede descargar este archivo.");
                return;
            }

            int idPeriodo;
            if (!int.TryParse(context.Request.QueryString["periodo"], out idPeriodo) || idPeriodo <= 0)
            {
                NoDisponible(context, "No se indicó qué período descargar.");
                return;
            }

            EntRespuesta respuesta = NegHorasExtrasPantalla.CargarPantalla(idPeriodo);

            if (respuesta.estado != "1" || respuesta.resultado == null)
            {
                NoDisponible(context, "No se encontró ese período.");
                return;
            }

            EntHePantalla pantalla = (EntHePantalla)respuesta.resultado;
            byte[] archivo = NegHeExportacion.Construir(pantalla);

            context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            context.Response.AddHeader("Content-Disposition",
                "attachment;filename=\"" + NegHeExportacion.NombreDeArchivo(pantalla.Periodo) + "\"");
            context.Response.BinaryWrite(archivo);
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

        /// <summary>
        /// Un texto plano, no una pagina de error: quien esto recibe es una
        /// descarga, y el navegador va a mostrar lo que llegue.
        /// </summary>
        private static void NoDisponible(HttpContext context, string mensaje)
        {
            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.Write(mensaje);
        }

        public bool IsReusable { get { return false; } }
    }
}
