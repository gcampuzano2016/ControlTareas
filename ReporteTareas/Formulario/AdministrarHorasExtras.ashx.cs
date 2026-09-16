using CapaEntidad;
using CapaNegocio;
using System;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetHorasExtras
{
    /// <summary>
    /// Handler de la pantalla de horas extras.
    ///
    /// IRequiresSessionState no es decorativo: sin el, context.Session es null en
    /// un IHttpHandler y no habria identidad con la que sellar quien guardo que.
    ///
    /// Ninguna accion acepta un total del cliente. Las horas si vienen de ahi
    /// -son lo que el usuario digita-, pero el dinero lo recalcula el servidor y
    /// devuelve lo suyo.
    ///
    /// ListarPeriodos entra por NegHorasExtrasPantalla y no por DaoHorasExtras
    /// directo: toda la casa entra por CapaNegocio, y ademas CapaDato no esta
    /// referenciado por este proyecto -saltarse la capa aqui ni compilaria-.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarHorasExtras : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            StringBuilder salida = new StringBuilder();

            if (context.Session == null || context.Session["UserLogin"] == null)
            {
                salida.Append(Mensaje("0", "Su sesión expiró. Vuelva a iniciar sesión.", "danger"));
            }
            else if (context.Request.ContentType != null && context.Request.ContentType.Contains("json"))
            {
                var lector = new System.IO.StreamReader(context.Request.InputStream);
                var json = lector.ReadToEnd();

                JavaScriptSerializer s = new JavaScriptSerializer();
                dynamic parametros = s.Deserialize(json.ToString(), typeof(object));

                var accion = parametros[0]["action"];
                bool existe = false;

                if (accion == "ListarPeriodos") { existe = true; salida.Append(ListarPeriodos()); }
                if (accion == "AbrirPeriodo")   { existe = true; salida.Append(AbrirPeriodo(context, parametros[0]["parameters"])); }
                if (accion == "CargarPeriodo")  { existe = true; salida.Append(CargarPeriodo(parametros[0]["parameters"])); }
                if (accion == "GuardarFila")    { existe = true; salida.Append(GuardarFila(context, parametros[0]["parameters"])); }

                if (!existe) { salida.Append(Mensaje("0", "La acción solicitada no existe.", "danger")); }
            }
            else
            {
                salida.Append(Mensaje("0", "Petición no válida.", "danger"));
            }

            context.Response.ContentType = "application/json";
            context.Response.Write(salida.ToString());
        }

        private string ListarPeriodos()
        {
            EntRespuesta r = new EntRespuesta();
            r.estado = "1";
            r.resultado = NegHorasExtrasPantalla.ListarPeriodos();
            r.tipoMensaje = "success";
            return new JavaScriptSerializer().Serialize(r);
        }

        private string AbrirPeriodo(HttpContext context, dynamic p)
        {
            int anio = Entero(p["anio"]);
            int mes = Entero(p["mes"]);
            EntRespuesta r = NegHorasExtrasPantalla.AbrirPeriodo(anio, mes, Usuario(context), Ip(context));
            return new JavaScriptSerializer().Serialize(r);
        }

        private string CargarPeriodo(dynamic p)
        {
            EntRespuesta r = NegHorasExtrasPantalla.CargarPantalla(Entero(p["idPeriodo"]));
            return new JavaScriptSerializer().Serialize(r);
        }

        private string GuardarFila(HttpContext context, dynamic p)
        {
            EntRespuesta r = NegHorasExtrasPantalla.GuardarHoras(
                Entero(p["idPeriodo"]),
                Convert.ToInt64(Entero(p["idEmpleado"])),
                Decimal(p["horas50"]),
                Decimal(p["horas100"]),
                Convert.ToString(p["observacion"]),
                Usuario(context), Ip(context));
            return new JavaScriptSerializer().Serialize(r);
        }

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

        private static int Entero(object v)
        {
            int n;
            return int.TryParse(Convert.ToString(v), out n) ? n : 0;
        }

        /// <summary>
        /// Lo que manda el cliente puede venir con coma, vacio o con basura. Un
        /// dato ilegible vale cero: el servidor no adivina cuantas horas quiso
        /// escribir alguien.
        /// </summary>
        private static decimal Decimal(object v)
        {
            decimal d;
            string texto = Convert.ToString(v);
            if (texto == null) { return 0m; }
            texto = texto.Trim().Replace(",", ".");
            return decimal.TryParse(texto, System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out d) ? d : 0m;
        }

        private static string Mensaje(string estado, string mensaje, string tipo)
        {
            EntRespuesta r = new EntRespuesta();
            r.estado = estado;
            r.mensaje = mensaje;
            r.tipoMensaje = tipo;
            return new JavaScriptSerializer().Serialize(r);
        }

        public bool IsReusable { get { return false; } }
    }
}
