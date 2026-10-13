using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Specialized;
using System.Web;

namespace JsonJQueryNetCalificacionForeCast
{
    /// <summary>
    /// La tabla de la calificacion en Excel. Handler aparte porque es un GET que
    /// escribe bytes, como DescargarHorasExtras.ashx.
    ///
    /// IRequiresSessionState no es decorativo: el usuario sale de la sesion
    /// (Cod_Usuario), no de la URL, y sin el context.Session es null.
    ///
    /// El Response.End() queda FUERA del try: End() lanza ThreadAbortException
    /// y un catch alrededor intentaria escribir con la respuesta ya enviada.
    /// </summary>
    public class DescargarCalificacionForeCast : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.Session == null || context.Session["UserLogin"] == null || context.Session["Cod_Usuario"] == null)
            {
                NoDisponible(context, "Su sesión expiró. Vuelva a iniciar sesión.", 401);
                return;
            }

            string idUsuario = Convert.ToString(context.Session["Cod_Usuario"]).Trim();
            if (string.IsNullOrWhiteSpace(idUsuario))
            {
                NoDisponible(context, "Su sesión expiró. Vuelva a iniciar sesión.", 401);
                return;
            }

            // Web.config fija requestEncoding en windows-1252, pero el JS arma esta
            // URL con $.param, que codifica en UTF-8: sin este parseo explicito los
            // filtros con tildes (gerente, sucursal, etc.) llegarian mal decodificados.
            NameValueCollection query = HttpUtility.ParseQueryString(context.Request.Url.Query, System.Text.Encoding.UTF8);
            EntFiltroCalificacion filtro = new EntFiltroCalificacion
            {
                GerentedeCuenta = Param(query, "gerente"),
                Sucursal = Param(query, "sucursal"),
                Prioridad = Param(query, "etapa"),
                Estado = Param(query, "estado"),
                Marca = Param(query, "marca")
            };

            byte[] archivo;
            try
            {
                EntPantallaCalificacion pantalla = NegCalificacionForeCast.Cargar(idUsuario, filtro);
                archivo = NegCalificacionExportacion.Construir(pantalla);
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
                "attachment; filename=" + NegCalificacionExportacion.NombreDeArchivo(DateTime.Today));
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            context.Response.BinaryWrite(archivo);
            context.Response.End();
        }

        private static string Param(NameValueCollection query, string nombre)
        {
            return (query[nombre] ?? "").Trim();
        }

        private static void NoDisponible(HttpContext context, string mensaje, int statusCode)
        {
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.Write(mensaje);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
