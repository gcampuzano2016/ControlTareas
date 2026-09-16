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
    ///
    /// El control de acceso por perfil no es cosmetico: el menu solo controla
    /// que se VEA la pantalla, no que se pueda LLAMAR al handler. Sin esta
    /// comprobacion, cualquier usuario con sesion iniciada -no solo Nomina o
    /// Talento Humano- podia pedir el sueldo de las 64 personas con una
    /// peticion directa a este .ashx, sin pasar nunca por el menu. Mismo
    /// patron que AdministrarPerfiles.ashx.cs: Id_Perfil sale de la sesion,
    /// nunca del cliente.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarHorasExtras : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        /// <summary>Talento Humano (14) y Super Admin (18): los mismos perfiles a los que el menu les muestra la pantalla.</summary>
        private static readonly int[] PerfilesAutorizados = { 14, 18 };

        public void ProcessRequest(HttpContext context)
        {
            StringBuilder salida = new StringBuilder();

            if (context.Session == null || context.Session["UserLogin"] == null)
            {
                salida.Append(Mensaje("0", "Su sesión expiró. Vuelva a iniciar sesión.", "danger"));
            }
            else if (!TienePermiso(context))
            {
                salida.Append(Mensaje("0", "No tiene permisos para esta pantalla.", "danger"));
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

        private static bool TienePermiso(HttpContext context)
        {
            int idPerfil;
            if (!int.TryParse(Convert.ToString(context.Session["Id_Perfil"]), out idPerfil)) { return false; }
            return Array.IndexOf(PerfilesAutorizados, idPerfil) >= 0;
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
        ///
        /// NumberStyles.AllowDecimalPoint, sin AllowLeadingSign: unas horas
        /// nunca llevan signo, asi que "-5" no es un numero valido aqui, es
        /// basura igual que "abc". El navegador ya no deja escribirlo -la
        /// grilla lo rechaza en la celda-, pero este handler es alcanzable con
        /// una peticion fabricada que se salte el navegador por completo, y
        /// sin este limite un "-5" pasaba el parseo, NegHorasExtras lo dejaba
        /// pasar tal cual a HE_Detalle -solo el dinero se pone en cero, las
        /// horas negativas SI se guardan-, y el indicador de horas del
        /// tablero podia quedar en negativo.
        /// </summary>
        private static decimal Decimal(object v)
        {
            decimal d;
            string texto = Convert.ToString(v);
            if (texto == null) { return 0m; }
            texto = texto.Trim().Replace(",", ".");
            return decimal.TryParse(texto, System.Globalization.NumberStyles.AllowDecimalPoint,
                                    System.Globalization.CultureInfo.InvariantCulture, out d) && d >= 0m
                   ? d : 0m;
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
