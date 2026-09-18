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
        /// <summary>
        /// Supervisor Especialistas (2), Talento Humano (14) y Super Admin (18):
        /// los mismos perfiles a los que el menu les muestra la pantalla.
        ///
        /// Esta lista NO es de solo lectura: quien esta aqui puede teclear horas,
        /// guardar, cerrar un periodo y corregir sus fechas. Lo unico reservado
        /// al 18 es reabrir un periodo cerrado (PerfilesQueReabren). Agregar un
        /// perfil aqui es darle el modulo entero, no el permiso de mirarlo.
        ///
        /// internal, no private: DescargarHorasExtras.ashx.cs -mismo namespace,
        /// mismo proyecto de presentacion- reusa esta misma lista para su propia
        /// comprobacion de perfil en vez de declarar la suya, y HorasExtras.aspx.cs
        /// la lee en su Page_Load para redirigir a quien no este. Los tres
        /// protegen el mismo dato (el sueldo del periodo) con el mismo criterio;
        /// si este criterio cambia algun dia, cambia en un solo lugar.
        ///
        /// Quien agregue un perfil aqui tiene que acordarse de PerfilMenu: la
        /// pantalla se abre por URL directa aunque el menu no la muestre, y al
        /// reves, una fila de menu sin el perfil en esta lista dibuja una opcion
        /// que al pulsarla redirige a Principal.aspx.
        /// </summary>
        internal static readonly int[] PerfilesAutorizados = { 2, 14, 18 };

        /// <summary>
        /// Solo Super Admin (18) puede reabrir un periodo cerrado. Separacion de
        /// funciones: quien opera el mes puede cerrarlo, pero deshacer un cierre
        /// formal exige otro perfil.
        ///
        /// Se comprueba AQUI y no solo ocultando el boton: ocultarlo es cortesia
        /// para quien no puede, no una barrera para quien no debe. El
        /// procedimiento SQL no conoce perfiles y NegHorasExtrasPantalla.ReabrirPeriodo
        /// tampoco los comprueba a proposito -este handler es el unico lugar
        /// donde existe esta barrera.
        /// </summary>
        private static readonly int[] PerfilesQueReabren = { 18 };

        public void ProcessRequest(HttpContext context)
        {
            StringBuilder salida = new StringBuilder();

            if (context.Session == null || context.Session["UserLogin"] == null)
            {
                salida.Append(Mensaje("0", "Su sesión expiró. Vuelva a iniciar sesión.", "danger"));
            }
            else if (!EstaEn(context, PerfilesAutorizados))
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
                if (accion == "EditarFechasPeriodo") { existe = true; salida.Append(EditarFechasPeriodo(context, parametros[0]["parameters"])); }
                if (accion == "CargarPeriodo")  { existe = true; salida.Append(CargarPeriodo(parametros[0]["parameters"])); }
                if (accion == "GuardarFila")    { existe = true; salida.Append(GuardarFila(context, parametros[0]["parameters"])); }
                if (accion == "CerrarPeriodo")  { existe = true; salida.Append(CerrarPeriodo(context, parametros[0]["parameters"])); }
                if (accion == "ReabrirPeriodo") { existe = true; salida.Append(ReabrirPeriodo(context, parametros[0]["parameters"])); }

                if (!existe) { salida.Append(Mensaje("0", "La acción solicitada no existe.", "danger")); }
            }
            else
            {
                salida.Append(Mensaje("0", "Petición no válida.", "danger"));
            }

            context.Response.ContentType = "application/json";
            context.Response.Write(salida.ToString());
        }

        /// <summary>
        /// Sin este try/catch -el unico handler de la aplicacion que no lo
        /// tenia; el resto de la casa lleva entre 5 y 21, AdministrarPerfiles
        /// es el molde- una SqlException cualquiera (procedimiento faltante,
        /// timeout, deadlock, un indice unico violado por dos peticiones
        /// concurrentes) sale como pagina de error de ASP.NET en vez de JSON.
        /// jQuery no la puede parsear, y con customErrors en Off y debug en
        /// true en el Web.config, lo que llega al navegador es una pantalla
        /// amarilla con traza -y el usuario solo ve "No se pudo contactar al
        /// servidor", sin ninguna pista de la causa real-.
        /// </summary>
        private string ListarPeriodos()
        {
            try
            {
                EntRespuesta r = new EntRespuesta();
                r.estado = "1";
                r.resultado = NegHorasExtrasPantalla.ListarPeriodos();
                r.tipoMensaje = "success";
                return new JavaScriptSerializer().Serialize(r);
            }
            catch (Exception ex)
            {
                return Mensaje("0", "Error al listar los periodos. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// El periodo ya no es un mes calendario sino un rango libre de
        /// fechas, asi que lo que llega son dos fechas y no dos enteros.
        ///
        /// Las dos se comprueban ANTES de llamar a CapaNegocio: una fecha que
        /// no parsea vale DateTime.MinValue -mismo criterio que Entero() y
        /// Decimal(), donde un dato ilegible vale cero-, y abrir un periodo
        /// que empieza el 01/01/0001 no es un rango que nadie quisiera pedir:
        /// es un formulario a medio llenar. Sin esta comprobacion llegaria al
        /// procedimiento y volveria como "No se pudo abrir el periodo", que no
        /// le dice a nadie que lo que falta es teclear las fechas.
        ///
        /// El solapamiento con otro periodo NO se comprueba aqui: eso lo
        /// resuelve NegHorasExtrasPantalla.AbrirPeriodo, que es quien conoce el
        /// codigo -5 del procedimiento y devuelve su propio mensaje.
        /// </summary>
        private string AbrirPeriodo(HttpContext context, dynamic p)
        {
            try
            {
                DateTime inicio = Fecha(p["fechaInicio"]);
                DateTime fin = Fecha(p["fechaFin"]);

                if (inicio == DateTime.MinValue || fin == DateTime.MinValue)
                {
                    return Mensaje("0", "Indique la fecha de inicio y la de fin del periodo.", "warning");
                }

                EntRespuesta r = NegHorasExtrasPantalla.AbrirPeriodo(inicio, fin, Usuario(context), Ip(context));
                return new JavaScriptSerializer().Serialize(r);
            }
            catch (Exception ex)
            {
                return Mensaje("0", "Error al abrir el periodo. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Corrige las fechas de un periodo ya creado y lo deja recalculado.
        ///
        /// No lleva comprobacion de perfil propia: ProcessRequest ya cierra el
        /// handler entero a PerfilesAutorizados, y editar las fechas de un
        /// periodo ABIERTO es del mismo orden que abrirlo. Reabrir es lo unico
        /// que pide mas -solo el 18-, y este metodo no puede tocar un periodo
        /// cerrado: el procedimiento lo rechaza con -6.
        ///
        /// Las fechas vacias se atajan aqui, igual que en AbrirPeriodo: el
        /// procedimiento devolveria -1, pero ese codigo habla de un rango al
        /// reves y no le dice a nadie que lo que falta es teclear las fechas.
        /// </summary>
        private string EditarFechasPeriodo(HttpContext context, dynamic p)
        {
            try
            {
                int idPeriodo = Entero(p["idPeriodo"]);
                DateTime inicio = Fecha(p["fechaInicio"]);
                DateTime fin = Fecha(p["fechaFin"]);

                if (idPeriodo <= 0)
                {
                    return Mensaje("0", "Elija primero el período que quiere corregir.", "warning");
                }

                if (inicio == DateTime.MinValue || fin == DateTime.MinValue)
                {
                    return Mensaje("0", "Indique la fecha de inicio y la de fin del periodo.", "warning");
                }

                EntRespuesta r = NegHorasExtrasPantalla.EditarFechasPeriodo(
                    idPeriodo, inicio, fin, Usuario(context), Ip(context));
                return new JavaScriptSerializer().Serialize(r);
            }
            catch (Exception ex)
            {
                return Mensaje("0", "Error al cambiar las fechas del periodo. " + ex.Message, "danger");
            }
        }

        private string CargarPeriodo(dynamic p)
        {
            try
            {
                EntRespuesta r = NegHorasExtrasPantalla.CargarPantalla(Entero(p["idPeriodo"]));
                return new JavaScriptSerializer().Serialize(r);
            }
            catch (Exception ex)
            {
                return Mensaje("0", "Error al cargar el periodo. " + ex.Message, "danger");
            }
        }

        private string GuardarFila(HttpContext context, dynamic p)
        {
            try
            {
                EntRespuesta r = NegHorasExtrasPantalla.GuardarHoras(
                    Entero(p["idPeriodo"]),
                    EnteroLargo(p["idEmpleado"]),
                    Decimal(p["horas50"]),
                    Decimal(p["horas100"]),
                    Convert.ToString(p["observacion"]),
                    Usuario(context), Ip(context));
                return new JavaScriptSerializer().Serialize(r);
            }
            catch (Exception ex)
            {
                return Mensaje("0", "Error al guardar la fila. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Cerrar (14 o 18) y reabrir (solo 18) comparten esta misma forma de
        /// comprobar perfil; solo cambia la lista contra la que se compara.
        /// </summary>
        private string CerrarPeriodo(HttpContext context, dynamic p)
        {
            try
            {
                EntRespuesta r = NegHorasExtrasPantalla.CerrarPeriodo(
                    Entero(p["idPeriodo"]), Usuario(context), Ip(context));
                return new JavaScriptSerializer().Serialize(r);
            }
            catch (Exception ex)
            {
                return Mensaje("0", "Error al cerrar el periodo. " + ex.Message, "danger");
            }
        }

        private string ReabrirPeriodo(HttpContext context, dynamic p)
        {
            if (!EstaEn(context, PerfilesQueReabren))
            {
                return Mensaje("0", "Su perfil no puede reabrir un período cerrado.", "warning");
            }

            try
            {
                EntRespuesta r = NegHorasExtrasPantalla.ReabrirPeriodo(
                    Entero(p["idPeriodo"]), Usuario(context), Ip(context));
                return new JavaScriptSerializer().Serialize(r);
            }
            catch (Exception ex)
            {
                return Mensaje("0", "Error al reabrir el periodo. " + ex.Message, "danger");
            }
        }

        private static bool EstaEn(HttpContext context, int[] perfiles)
        {
            int idPerfil;
            if (!int.TryParse(Convert.ToString(context.Session["Id_Perfil"]), out idPerfil)) { return false; }
            return Array.IndexOf(perfiles, idPerfil) >= 0;
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
        /// IdEmpleado es BIGINT en la base y long en EntHeFila. Antes se leia
        /// con Entero() y se ensanchaba con Convert.ToInt64: un identificador
        /// que no entrara en un int32 se truncaba a un numero cualquiera -o a
        /// 0- antes de siquiera llegar a compararse, y la respuesta salia
        /// "Esa persona no esta en este periodo": un fallo real, pero por la
        /// razon equivocada.
        /// </summary>
        private static long EnteroLargo(object v)
        {
            long n;
            return long.TryParse(Convert.ToString(v), out n) ? n : 0L;
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

        /// <summary>
        /// Una fecha ilegible vale DateTime.MinValue, igual que un numero
        /// ilegible vale cero en Entero() y en Decimal(): este handler no
        /// adivina que fecha quiso escribir alguien, solo distingue "hay una
        /// fecha" de "no la hay", y quien llama decide que hacer con eso.
        ///
        /// ParseExact con "yyyy-MM-dd" e InvariantCulture, no Parse: un
        /// input type="date" manda SIEMPRE ese formato, independientemente
        /// del idioma del navegador, y un Parse con la cultura del servidor
        /// leeria "2026-09-16" bien por casualidad pero confundiria dia y mes
        /// el dia que algo mande "09/16/2026" o "16/09/2026". Aqui solo se
        /// acepta lo que de verdad manda la pantalla.
        /// </summary>
        private static DateTime Fecha(object v)
        {
            DateTime f;
            string texto = Convert.ToString(v);
            if (texto == null) { return DateTime.MinValue; }

            return DateTime.TryParseExact(texto.Trim(), "yyyy-MM-dd",
                                          System.Globalization.CultureInfo.InvariantCulture,
                                          System.Globalization.DateTimeStyles.None, out f)
                   ? f : DateTime.MinValue;
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
