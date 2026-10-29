using CapaEntidad;
using CapaNegocio;
using ReporteTareas.clases;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace JsonJQueryNetVacSaldos
{
    /// <summary>
    /// Handler de VacacionesNomina.aspx. Accion: ConsultarNomina. Solo lee.
    ///
    /// Muestra el sueldo de todo el personal: el permiso se comprueba en cada
    /// llamada (pantalla VacacionesNomina.aspx, por perfil o como extra), no
    /// solo en el menu.
    /// </summary>
    public class AdministrarVacNomina : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            /* Sin esto los bytes salen en windows-1252 (Web.config) aunque la
               cabecera diga utf-8, y cada tilde llega rota. */
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();

            string salida;
            if (!Permisos.Autenticado(context))
            {
                context.Response.TrySkipIisCustomErrors = true;
                context.Response.StatusCode = 401;
                salida = Mensaje("0", "Su sesión expiró. Vuelva a iniciar sesión.", "danger");
            }
            else if (!Permisos.PuedeEntrar(context, Pantallas.VacacionesNomina))
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

                object parametros;
                peticion.TryGetValue("parameters", out parametros);
                if (Campo(peticion, "action") == "ConsultarNomina") { return Consultar(Campo(parametros, "fecha")); }
                return Mensaje("0", "La acción solicitada no existe.", "danger");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Mensaje("0", "Petición no válida.", "danger");
            }
        }

        private static string Consultar(string texto)
        {
            try
            {
                DateTime fecha = DateTime.Today;
                if (texto != "" && !DateTime.TryParseExact(texto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                {
                    return Mensaje("0", "Indique una fecha de cálculo válida.", "warning");
                }
                string error = NegVacSaldos.ValidarFecha(fecha, DateTime.Today);
                if (error != "") { return Mensaje("0", error, "warning"); }

                EntVacNomina n = NegVacNomina.Cargar(fecha);
                return Exito(new
                {
                    fechaCalculo = Fecha(n.FechaCalculo),
                    n.Activos,
                    n.TotalDias,
                    n.TotalValor,
                    n.SinSueldo,
                    n.ValorAnticipos,
                    n.PorEmpresa,
                    avisos = n.Avisos,
                    filas = n.Filas.Select(f => new
                    {
                        f.Colaborador.IdColaborador,
                        f.Colaborador.Cedula,
                        f.Colaborador.Nombre,
                        f.Colaborador.Empresa,
                        f.Colaborador.Area,
                        estado = f.Colaborador.Estado,
                        ingreso = Fecha(f.Colaborador.FechaIngreso),
                        salida = Fecha(f.Colaborador.FechaSalida),
                        fechaSaldo = Fecha(f.Saldo.FechaCalculo),
                        f.Sueldo,
                        f.FuenteSueldo,
                        f.ValorDia,
                        disponible = f.Saldo.Disponible,
                        proporcional = f.Saldo.Proporcional,
                        usadoEnCurso = f.Saldo.UsadoEnCurso,
                        dias = f.Saldo.Saldo,
                        f.Valor,
                        f.Observacion
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Mensaje("0", "No se pudo consultar la nómina. Intente nuevamente.", "danger");
            }
        }

        private static string Fecha(DateTime? d)
        {
            return d.HasValue ? d.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "";
        }

        private static string Campo(object campos, string nombre)
        {
            IDictionary<string, object> d = campos as IDictionary<string, object>;
            object v;
            if (d == null || !d.TryGetValue(nombre, out v) || v == null) { return ""; }
            return Convert.ToString(v, CultureInfo.InvariantCulture).Trim();
        }

        private static string Exito(object resultado)
        {
            return Serializar(new EntRespuesta { estado = "1", mensaje = "", tipoMensaje = "success", resultado = resultado });
        }

        private static string Mensaje(string estado, string mensaje, string tipo)
        {
            return Serializar(new EntRespuesta { estado = estado, mensaje = mensaje, tipoMensaje = tipo, resultado = "" });
        }

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
