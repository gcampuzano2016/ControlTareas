using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetHorasExtras
{
    /// <summary>
    /// Handler de la pantalla de parametros de horas extras.
    ///
    /// Misma forma que AdministrarHorasExtras.ashx.cs -IRequiresSessionState,
    /// payload [{"action","parameters"}], EntRespuesta, un try/catch por
    /// accion-, y por las mismas razones: sin IRequiresSessionState no hay
    /// Session en un IHttpHandler y no habria identidad con la que sellar
    /// quien cambio que parametro.
    ///
    /// Lo que esta pantalla escribe no es un dato de una persona: es el factor
    /// con el que se le paga a las 64. Un Factor50 mal puesto no se nota en la
    /// pantalla de parametros, se nota en la nomina del mes.
    ///
    /// El orden de las comprobaciones importa: sesion primero, perfil despues,
    /// parametros al final. Comprobar el perfil antes que la sesion leeria
    /// Session["Id_Perfil"] sobre una sesion nula; validar parametros antes que
    /// el perfil le contaria a quien no tiene permiso que forma tiene el
    /// payload que si lo tendria.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarParametrosHE : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        /// <summary>
        /// Talento Humano (14) y Super Admin (18): los mismos perfiles a los
        /// que el menu le muestra la opcion.
        ///
        /// LA BARRERA REAL ESTA AQUI. Ni CapaNegocio ni el procedimiento SQL
        /// conocen perfiles, y el menu solo decide que la opcion se VEA, no
        /// que el .ashx no se pueda LLAMAR: sin esta lista, cualquier usuario
        /// con sesion iniciada podria cambiar el factor de recargo con una
        /// peticion fabricada que no pase nunca por el menu.
        ///
        /// internal, no private, igual que en el modulo hermano:
        /// ParametrizacionHorasExtras.aspx.cs la reusa para su redireccion de
        /// cortesia en vez de declarar su propia copia. Un perfil anadido aqui
        /// y olvidado alli deja a esa gente con permiso de handler y sin
        /// pantalla, y nadie relaciona una cosa con la otra.
        /// </summary>
        internal static readonly int[] PerfilesAutorizados = { 14, 18 };

        /// <summary>
        /// Las claves cuyo valor no lo decide la empresa sino el Codigo del
        /// Trabajo. La pantalla las marca para que quien administre entienda
        /// que puede cambiarlas -el campo se edita igual- pero que hacerlo la
        /// pone fuera de la ley, no solo fuera de politica.
        ///
        /// Vive aqui y no en el .aspx ni en el .js por la misma razon por la
        /// que las etiquetas viven en NegHeParametroPantalla: la pantalla no
        /// tiene que saber como se llaman las claves. Lo correcto seria que
        /// NegHeParametroPantalla expusiera tambien este rasgo junto a
        /// Etiqueta y Activo -queda anotado como deuda-; mientras tanto el
        /// handler es el sitio menos malo: ya conoce las claves que hay,
        /// porque las recibe de CapaNegocio, y es un solo lugar.
        /// </summary>
        private static readonly string[] ClavesFijadasPorLey = { "Factor50", "Factor100" };

        /// <summary>
        /// Lo que viaja a la pantalla por cada fila del historial. Es
        /// EntHeParametroFila mas los tres rasgos que la pantalla necesita
        /// para pintarse y que NO puede deducir de la fila: como se llama la
        /// clave en castellano, si algun calculo la usa, y si la fija la ley.
        ///
        /// Se arma aqui y no en el .js a proposito: la etiqueta y la lista de
        /// claves conocidas viven en NegHeParametroPantalla, y escribirlas
        /// tambien en el JavaScript es como una pantalla acaba llamando "Dias
        /// del mes" a algo que en CapaNegocio ya se renombro. El JS pinta lo
        /// que le llega; no sabe una sola clave de memoria.
        /// </summary>
        private class FilaParametro
        {
            public int IdParametro { get; set; }
            public string Clave { get; set; }
            public string Etiqueta { get; set; }
            public bool Conocida { get; set; }
            public bool Activo { get; set; }
            public bool FijadoPorLey { get; set; }
            public decimal Valor { get; set; }
            public DateTime FechaVigenciaDesde { get; set; }
            public DateTime? FechaVigenciaHasta { get; set; }
            public string Usu_Modificacion { get; set; }
            public DateTime? Fec_Modificacion { get; set; }

            /// <summary>
            /// Si esta es la version que rige hoy. FechaVigenciaHasta NULL es
            /// "sigue abierta", que es justo el criterio con el que
            /// Sp_RTA_HeParametroGuardar cierra la anterior. Se manda resuelto
            /// y no se deja que el JS compare contra null: un /Date(...)/ y un
            /// null se distinguen mal en JavaScript, y de eso depende que fila
            /// sale en la tabla de arriba y cual en el historial.
            /// </summary>
            public bool EsVigente { get; set; }
        }

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

                if (accion == "Listar")  { existe = true; salida.Append(Listar()); }
                if (accion == "Guardar") { existe = true; salida.Append(Guardar(context, parametros[0]["parameters"])); }

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
        /// El historial completo. El try/catch no es decorativo: sin el, una
        /// SqlException cualquiera -procedimiento faltante, timeout, deadlock-
        /// sale como pagina de error de ASP.NET en vez de JSON, jQuery no la
        /// puede parsear, y lo unico que ve el usuario es "No se pudo
        /// contactar al servidor" sin ninguna pista de la causa real.
        /// </summary>
        private string Listar()
        {
            try
            {
                return new JavaScriptSerializer().Serialize(Enriquecer(NegHeParametroPantalla.Listar()));
            }
            catch (Exception ex)
            {
                return Mensaje("0", "Error al listar los parámetros. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Guarda una version nueva de una clave.
        ///
        /// Las tres comprobaciones de aqui -clave conocida, valor mayor que
        /// cero, fecha legible- las repite Sp_RTA_HeParametroGuardar, que es
        /// quien manda. Se hacen igual porque el procedimiento contesta con un
        /// codigo generico y esta capa puede decir exactamente que falta: un
        /// "Indique desde que fecha rige" vale mas que un "No se pudo guardar
        /// el parametro" cuando lo que pasa es que el campo de la fecha quedo
        /// vacio.
        ///
        /// Una clave desconocida se para en seco y no llega al procedimiento:
        /// EsClaveConocida es la misma lista de siete con la que la pantalla
        /// se pinta, asi que una clave fuera de ella solo puede venir de una
        /// peticion fabricada a mano.
        ///
        /// El valor NO se recorta ni se redondea aqui. DecimalesMonto con mas
        /// de dos decimales lo rechaza el CHECK de la tabla y CapaNegocio lo
        /// traduce (-5); redondearlo por cuenta propia guardaria en silencio
        /// algo distinto de lo que la persona escribio.
        /// </summary>
        private string Guardar(HttpContext context, dynamic p)
        {
            try
            {
                string clave = Convert.ToString(p["clave"]);
                clave = clave == null ? "" : clave.Trim();

                if (!NegHeParametroPantalla.EsClaveConocida(clave))
                {
                    return Mensaje("0", "Esa clave no existe entre los parámetros conocidos.", "warning");
                }

                decimal valor = Decimal(p["valor"]);

                if (valor <= 0m)
                {
                    return Mensaje("0", "El valor tiene que ser mayor que cero.", "warning");
                }

                DateTime desde = Fecha(p["desde"]);

                if (desde == DateTime.MinValue)
                {
                    return Mensaje("0", "Indique desde qué fecha rige el valor nuevo.", "warning");
                }

                EntRespuesta r = NegHeParametroPantalla.Guardar(clave, valor, desde, Usuario(context), Ip(context));
                return new JavaScriptSerializer().Serialize(Enriquecer(r));
            }
            catch (Exception ex)
            {
                return Mensaje("0", "Error al guardar el parámetro. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Anade a cada fila del historial la etiqueta, si esta activa y si la
        /// fija la ley. Guardar devuelve el historial recargado cuando sale
        /// bien -y nada cuando sale mal-, asi que las dos acciones pasan por
        /// aqui y la respuesta tiene siempre la misma forma.
        ///
        /// Un resultado que no sea la lista esperada se deja tal cual en vez
        /// de reventar: si CapaNegocio devolviera un estado "0" con
        /// resultado nulo, el mensaje que trae es lo unico util que hay.
        /// </summary>
        private static EntRespuesta Enriquecer(EntRespuesta respuesta)
        {
            if (respuesta == null) { return null; }

            List<EntHeParametroFila> origen = respuesta.resultado as List<EntHeParametroFila>;
            if (origen == null) { return respuesta; }

            List<FilaParametro> filas = new List<FilaParametro>();

            foreach (EntHeParametroFila f in origen)
            {
                filas.Add(new FilaParametro
                {
                    IdParametro = f.IdParametro,
                    Clave = f.Clave,
                    Etiqueta = NegHeParametroPantalla.EtiquetaDe(f.Clave),
                    Conocida = NegHeParametroPantalla.EsClaveConocida(f.Clave),
                    Activo = NegHeParametroPantalla.EsParametroActivo(f.Clave),
                    FijadoPorLey = EsFijadoPorLey(f.Clave),
                    Valor = f.Valor,
                    FechaVigenciaDesde = f.FechaVigenciaDesde,
                    FechaVigenciaHasta = f.FechaVigenciaHasta,
                    Usu_Modificacion = f.Usu_Modificacion,
                    Fec_Modificacion = f.Fec_Modificacion,
                    EsVigente = !f.FechaVigenciaHasta.HasValue
                });
            }

            respuesta.resultado = filas;
            return respuesta;
        }

        private static bool EsFijadoPorLey(string clave)
        {
            string buscada = (clave ?? "").Trim();

            foreach (string c in ClavesFijadasPorLey)
            {
                if (string.Equals(c, buscada, StringComparison.OrdinalIgnoreCase)) { return true; }
            }

            return false;
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

        /// <summary>
        /// Lo que manda el cliente puede venir con coma, vacio o con basura. Un
        /// dato ilegible vale cero, igual que en el handler hermano, y un cero
        /// lo rechaza Guardar con un mensaje claro en vez de mandarlo al
        /// procedimiento.
        ///
        /// Sin AllowLeadingSign: ningun parametro de este modulo puede ser
        /// negativo -ni un factor, ni unos dias, ni unos decimales-, asi que
        /// "-1.5" no es un numero valido aqui, es basura igual que "abc".
        /// InvariantCulture, porque el separador decimal del navegador no
        /// tiene por que ser el de la cultura del servidor.
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
        /// ParseExact con "yyyy-MM-dd" e InvariantCulture, no Parse: un
        /// input type="date" manda SIEMPRE ese formato independientemente del
        /// idioma del navegador, y un Parse con la cultura del servidor
        /// confundiria dia y mes el dia que algo mande "09/16/2026". Una fecha
        /// ilegible vale DateTime.MinValue y quien llama decide.
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
