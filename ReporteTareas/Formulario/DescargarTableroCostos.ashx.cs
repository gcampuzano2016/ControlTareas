using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Specialized;
using System.Web;

namespace JsonJQueryNetTableroCostos
{
    /// <summary>
    /// El Excel del Tablero de Costos. Es un GET que escribe bytes, como
    /// DescargarHorasExtras.ashx.
    ///
    /// La descarga se REGISTRA ANTES de armar el archivo: si TC_Auditoria no
    /// acepta la fila, no se entrega nada. El archivo lleva valores calculados
    /// con salarios.
    ///
    /// Response.End() queda FUERA de los try: End() lanza ThreadAbortException,
    /// y un catch alrededor intentaria escribir con la respuesta ya enviada.
    /// </summary>
    public class DescargarTableroCostos : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            if (!AdministrarTableroCostos.Autenticado(context))
            {
                NoDisponible(context, "Su sesión expiró. Vuelva a iniciar sesión.", 401);
                return;
            }
            if (!AdministrarTableroCostos.Autorizado(context))
            {
                NoDisponible(context, "No tiene permisos para descargar este archivo.", 403);
                return;
            }

            /* Request.QueryString decodifica con requestEncoding (windows-1252
               en Web.config) y jQuery $.param manda UTF-8: una tilde o una Ñ
               llegaria rota. Se parsea una sola vez, en UTF-8 explicito. */
            NameValueCollection consulta = HttpUtility.ParseQueryString(context.Request.Url.Query, System.Text.Encoding.UTF8);

            DateTime desde, hasta;
            string error;
            if (!AdministrarTableroCostos.LeerRango(Param(consulta, "desde"), Param(consulta, "hasta"), out desde, out hasta, out error))
            {
                NoDisponible(context, error, 400);
                return;
            }

            EntTcFiltro filtro = AdministrarTableroCostos.LeerFiltro(n => Param(consulta, n));
            string descripcion = NegTableroCostos.DescribirFiltro(desde, hasta, filtro);
            string codUsuario = Convert.ToString(context.Session["Cod_Usuario"]).Trim();

            try
            {
                NegTableroCostos.AuditarDescarga(codUsuario, descripcion);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                NoDisponible(context, "No se pudo registrar la descarga y, por seguridad, no se entrega el archivo.", 500);
                return;
            }

            byte[] archivo;
            try
            {
                EntTcClasificado datos = AdministrarTableroCostos.Datos(context, desde, hasta, false);
                EntTableroCostos tablero = NegTableroCostos.Armar(datos, filtro);
                archivo = NegTableroCostosExportacion.Construir(tablero, descripcion);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                NoDisponible(context, "No se pudo generar el archivo.", 500);
                return;
            }

            context.Response.Clear();
            context.Response.Buffer = true;
            context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            context.Response.AddHeader("Content-Disposition",
                "attachment; filename=" + NegTableroCostosExportacion.NombreDeArchivo(desde, hasta));
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            context.Response.BinaryWrite(archivo);
            context.Response.End();
        }

        private static string Param(NameValueCollection consulta, string nombre)
        {
            return (consulta[nombre] ?? "").Trim();
        }

        private static void NoDisponible(HttpContext context, string mensaje, int statusCode)
        {
            context.Response.Clear();
            context.Response.TrySkipIisCustomErrors = true;
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
