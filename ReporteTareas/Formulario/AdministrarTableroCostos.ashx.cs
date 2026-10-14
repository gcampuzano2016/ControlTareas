using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace JsonJQueryNetTableroCostos
{
    /// <summary>
    /// Handler de TableroCostos.aspx. Accion: ConsultarTablero.
    ///
    /// El control de acceso NO es cosmetico: el menu solo esconde la opcion, y
    /// la pantalla muestra montos calculados con salarios. Autorizado es el
    /// unico punto; TableroCostos.aspx.cs y DescargarTableroCostos lo reusan.
    /// La regla (perfiles 14/18 o modulo extra por usuario) esta en
    /// NegTableroCostos.PuedeVer.
    ///
    /// Las actividades clasificadas del rango quedan en Session: cambiar un
    /// filtro no vuelve a la base. "refrescar" = "1" (boton Consultar) fuerza
    /// la relectura.
    /// </summary>
    public class AdministrarTableroCostos : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        private const string ClaveRango = "TableroCostos.Rango";
        private const string ClaveDatos = "TableroCostos.Datos";

        public void ProcessRequest(HttpContext context)
        {
            /* Sin esto los bytes salen en windows-1252 (Web.config) aunque la
               cabecera diga utf-8, y cada tilde llega rota. */
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();

            string salida;
            if (!Autenticado(context))
            {
                context.Response.TrySkipIisCustomErrors = true;
                context.Response.StatusCode = 401;
                salida = Mensaje("0", "Su sesión expiró. Vuelva a iniciar sesión.", "danger");
            }
            else if (!Autorizado(context))
            {
                context.Response.TrySkipIisCustomErrors = true;
                context.Response.StatusCode = 403;
                salida = Mensaje("0", "No tiene permisos para esta pantalla.", "danger");
            }
            else if (context.Request.ContentType != null && context.Request.ContentType.Contains("json"))
            {
                salida = Despachar(context);
            }
            else
            {
                salida = Mensaje("0", "Petición no válida.", "danger");
            }
            context.Response.Write(salida);
        }

        private string Despachar(HttpContext context)
        {
            try
            {
                string json = new System.IO.StreamReader(context.Request.InputStream, Encoding.UTF8).ReadToEnd();
                object[] lista = new JavaScriptSerializer().Deserialize<object[]>(json);
                IDictionary<string, object> peticion = lista != null && lista.Length > 0
                    ? lista[0] as IDictionary<string, object>
                    : null;
                if (peticion == null) { return Mensaje("0", "Petición no válida.", "danger"); }

                string accion = Campo(peticion, "action");
                object parametros;
                peticion.TryGetValue("parameters", out parametros);

                if (accion == "ConsultarTablero") { return Consultar(context, parametros); }
                return Mensaje("0", "La acción solicitada no existe.", "danger");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Mensaje("0", "Petición no válida.", "danger");
            }
        }

        private string Consultar(HttpContext context, object p)
        {
            try
            {
                DateTime desde, hasta;
                string error;
                if (!LeerRango(Campo(p, "desde"), Campo(p, "hasta"), out desde, out hasta, out error))
                {
                    return Mensaje("0", error, "warning");
                }

                EntTcClasificado datos = Datos(context, desde, hasta, Campo(p, "refrescar") == "1");
                EntTableroCostos tablero = NegTableroCostos.Armar(datos, LeerFiltro(n => Campo(p, n)));

                return Serializar(new EntRespuesta
                {
                    estado = "1",
                    mensaje = "",
                    tipoMensaje = "success",
                    resultado = ParaPantalla(tablero)
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Mensaje("0", "No se pudo consultar el tablero. Intente nuevamente.", "danger");
            }
        }

        // ------------------------------------- compartido con la descarga

        /// <summary>
        /// Login.aspx "reinicia" la sesion escribiendo "" en UserLogin y
        /// Cod_Usuario (sin tocar Id_Perfil), asi que null no basta: hay que
        /// exigir que ambos tengan texto. Un solo lugar para los dos handlers.
        /// </summary>
        internal static bool Autenticado(HttpContext context)
        {
            return context.Session != null
                && !string.IsNullOrWhiteSpace(Convert.ToString(context.Session["UserLogin"], CultureInfo.InvariantCulture))
                && !string.IsNullOrWhiteSpace(Convert.ToString(context.Session["Cod_Usuario"], CultureInfo.InvariantCulture));
        }

        internal static bool Autorizado(HttpContext context)
        {
            if (!Autenticado(context))
            {
                return false;
            }
            int idPerfil;
            int? perfil = int.TryParse(Convert.ToString(context.Session["Id_Perfil"], CultureInfo.InvariantCulture), out idPerfil)
                ? idPerfil : (int?)null;
            return NegTableroCostos.PuedeVer(perfil, Convert.ToString(context.Session["Cod_Usuario"], CultureInfo.InvariantCulture));
        }

        internal static bool LeerRango(string desdeTexto, string hastaTexto, out DateTime desde, out DateTime hasta, out string error)
        {
            desde = DateTime.MinValue;
            hasta = DateTime.MinValue;
            if (!DateTime.TryParseExact(desdeTexto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out desde)
                || !DateTime.TryParseExact(hastaTexto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out hasta))
            {
                error = "Indique las fechas Desde y Hasta.";
                return false;
            }
            error = NegTableroCostos.ValidarRango(desde, hasta);
            return error == "";
        }

        internal static EntTcFiltro LeerFiltro(Func<string, string> campo)
        {
            return new EntTcFiltro
            {
                Mes = campo("mes"),
                Responsable = campo("responsable"),
                Empresa = campo("empresa"),
                Contrato = campo("contrato"),
                Recargo = campo("recargo"),
                EstadoHE = campo("estadoHE")
            };
        }

        /// <summary>Las actividades del rango, de la sesion si ya estan; si no, de la base.</summary>
        internal static EntTcClasificado Datos(HttpContext context, DateTime desde, DateTime hasta, bool refrescar)
        {
            string clave = desde.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "|"
                         + hasta.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            EntTcClasificado enSesion = context.Session[ClaveDatos] as EntTcClasificado;
            if (!refrescar && enSesion != null && clave == context.Session[ClaveRango] as string)
            {
                return enSesion;
            }

            EntTcClasificado nuevo = NegTableroCostos.Cargar(desde, hasta);
            context.Session[ClaveRango] = clave;
            context.Session[ClaveDatos] = nuevo;
            return nuevo;
        }

        // ------------------------------------------------------- salida

        /// <summary>
        /// El detalle viaja como arreglos (ver el orden en tableroCostos.js,
        /// constantes D_*): con unas 22.000 filas en 24 meses, repetir los
        /// nombres de propiedad duplicaria el peso del JSON.
        /// </summary>
        private static object ParaPantalla(EntTableroCostos t)
        {
            return new
            {
                t.Indicadores,
                t.Resumen,
                t.ExtrasPorEstado,
                t.TrasladosPorEstado,
                t.PorAnio,
                t.PorMes,
                t.TopNormal,
                t.TopExtra,
                t.TopTraslado,
                t.Meses,
                t.Responsables,
                t.Empresas,
                t.Contratos,
                t.SinSalario,
                t.Aviso,
                t.ParametrosPorDefecto,
                Detalle = t.Detalle.Select(r => new object[]
                {
                    r.IdDetalle,
                    r.FechaInicio.ToString("dd'/'MM'/'yyyy HH:mm", CultureInfo.InvariantCulture),
                    r.Responsable,
                    r.Empresa,
                    r.Contrato,
                    r.Ticket,
                    r.Tarea,
                    r.Estado,
                    r.Tipo,
                    r.Recargo,
                    r.EstadoHE,
                    Math.Round(r.Horas, 4),
                    Math.Round(r.Valor, 2)
                }).ToList()
            };
        }

        private static string Campo(object campos, string nombre)
        {
            IDictionary<string, object> d = campos as IDictionary<string, object>;
            object v;
            if (d == null || !d.TryGetValue(nombre, out v) || v == null) { return ""; }
            return Convert.ToString(v, CultureInfo.InvariantCulture).Trim();
        }

        private static string Mensaje(string estado, string mensaje, string tipo)
        {
            return Serializar(new EntRespuesta { estado = estado, mensaje = mensaje, tipoMensaje = tipo, resultado = "" });
        }

        /// <summary>JavaScriptSerializer escapa las tildes como \uXXXX; MaxJsonLength al maximo por el detalle.</summary>
        private static string Serializar(object o)
        {
            JavaScriptSerializer s = new JavaScriptSerializer();
            s.MaxJsonLength = int.MaxValue;
            return s.Serialize(o);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
