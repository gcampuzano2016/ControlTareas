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
    /// Handler de VacacionesAjustes.aspx. Acciones: ListarColaboradores,
    /// Cargar, SimularCorte, GuardarCorte, RegistrarCompra y AnularCompra.
    ///
    /// Escribe saldos: el permiso (pantalla VacacionesAjustes.aspx) se
    /// comprueba en cada llamada, y cada escritura deja en la bitacora el
    /// Cod_Usuario de la sesion, nunca uno que mande el navegador.
    /// </summary>
    public class AdministrarVacAjustes : IHttpHandler, System.Web.SessionState.IRequiresSessionState
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
            else if (!Permisos.PuedeEntrar(context, Pantallas.VacacionesAjustes))
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

                object p;
                peticion.TryGetValue("parameters", out p);
                string accion = Campo(peticion, "action");
                if (accion == "ListarColaboradores") { return Listar(); }

                int id;
                if (!int.TryParse(Campo(p, "idColaborador"), NumberStyles.None, CultureInfo.InvariantCulture, out id) || id <= 0)
                {
                    return Mensaje("0", "Elija un colaborador.", "warning");
                }
                string usuario = Convert.ToString(context.Session["Cod_Usuario"], CultureInfo.InvariantCulture).Trim();
                string ip = context.Request.UserHostAddress;

                if (accion == "Cargar") { return Cargar(id); }
                if (accion == "SimularCorte") { return SimularCorte(id, p); }
                if (accion == "GuardarCorte") { return GuardarCorte(id, p, usuario, ip); }
                if (accion == "RegistrarCompra") { return RegistrarCompra(id, p, usuario, ip); }
                if (accion == "AnularCompra") { return AnularCompra(id, p, usuario, ip); }
                return Mensaje("0", "La acción solicitada no existe.", "danger");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Mensaje("0", "No se pudo completar la operación. Intente nuevamente.", "danger");
            }
        }

        private static string Listar()
        {
            return Exito(NegVacAjustes.Colaboradores().Select(c => new
            {
                c.IdColaborador,
                c.Nombre,
                c.Cedula,
                c.Estado
            }).ToList());
        }

        private static string Cargar(int id)
        {
            EntVacAjustes a = NegVacAjustes.Cargar(id, DateTime.Today);
            if (a == null || a.Ficha.Saldo == null) { return Mensaje("0", "El colaborador no existe.", "warning"); }

            EntVacColaborador c = a.Ficha.Colaborador;
            EntVacSaldo s = a.Ficha.Saldo;
            return Exito(new
            {
                persona = new
                {
                    c.Nombre,
                    c.Cedula,
                    c.Estado,
                    ingreso = Fecha(c.FechaIngreso),
                    salida = Fecha(c.FechaSalida),
                    corte = Fecha(c.FechaCorte),
                    c.SaldoCorte
                },
                saldo = Saldo(s),
                compras = NegVacAjustes.Compras(a.Ficha).Select(m => new
                {
                    m.IdMovimiento,
                    fecha = Fecha(m.Desde),
                    m.Dias,
                    m.Estado,
                    m.Situacion,
                    m.Observacion,
                    m.Origen
                }).ToList(),
                bitacora = a.Bitacora.Select(b => new
                {
                    fecha = b.Fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                    b.Usuario,
                    b.Tipo,
                    b.Accion,
                    fechaAntes = Fecha(b.FechaAntes),
                    b.DiasAntes,
                    fechaDespues = Fecha(b.FechaDespues),
                    b.DiasDespues,
                    b.Motivo
                }).ToList(),
                a.Emitidos,
                ultimoEmitido = Fecha(a.UltimoEmitido)
            });
        }

        private static string SimularCorte(int id, object p)
        {
            DateTime? fecha;
            decimal? saldo;
            string error = LeerCorte(p, out fecha, out saldo);
            if (error != "") { return Mensaje("0", error, "warning"); }
            try
            {
                EntVacSaldo s = NegVacAjustes.SimularCorte(id, fecha, saldo, DateTime.Today);
                if (s == null) { return Mensaje("0", "El colaborador no existe.", "warning"); }
                return Exito(Saldo(s));
            }
            catch (ArgumentException ex)
            {
                return Mensaje("0", ex.Message, "warning");
            }
        }

        private static string GuardarCorte(int id, object p, string usuario, string ip)
        {
            DateTime? fecha, fechaAnterior;
            decimal? saldo, saldoAnterior;
            string error = LeerCorte(p, out fecha, out saldo);
            if (error == "") { error = LeerFecha(p, "fechaAnterior", out fechaAnterior); } else { fechaAnterior = null; }
            if (error == "") { error = LeerDias(p, "saldoAnterior", out saldoAnterior); } else { saldoAnterior = null; }
            if (error != "") { return Mensaje("0", error, "warning"); }

            error = NegVacAjustes.GuardarCorte(id, fecha, saldo, fechaAnterior, saldoAnterior, Campo(p, "motivo"), usuario, ip, DateTime.Today);
            return error == "" ? Exito("") : Mensaje("0", error, "warning");
        }

        private static string RegistrarCompra(int id, object p, string usuario, string ip)
        {
            DateTime? fecha;
            decimal? dias;
            string error = LeerFecha(p, "fecha", out fecha);
            if (error == "") { error = LeerDias(p, "dias", out dias); } else { dias = null; }
            if (error == "" && (!fecha.HasValue || !dias.HasValue)) { error = "Indique la fecha y los días comprados."; }
            if (error != "") { return Mensaje("0", error, "warning"); }

            error = NegVacAjustes.RegistrarCompra(id, fecha.Value, dias.Value, Campo(p, "motivo"), usuario, ip, DateTime.Today);
            return error == "" ? Exito("") : Mensaje("0", error, "warning");
        }

        private static string AnularCompra(int id, object p, string usuario, string ip)
        {
            int idMovimiento;
            if (!int.TryParse(Campo(p, "idMovimiento"), NumberStyles.None, CultureInfo.InvariantCulture, out idMovimiento) || idMovimiento <= 0)
            {
                return Mensaje("0", "Elija la compra a anular.", "warning");
            }
            string error = NegVacAjustes.AnularCompra(id, idMovimiento, Campo(p, "motivo"), usuario, ip);
            return error == "" ? Exito("") : Mensaje("0", error, "warning");
        }

        private static object Saldo(EntVacSaldo s)
        {
            return new
            {
                fechaCalculo = Fecha(s.FechaCalculo),
                s.Disponible,
                s.Proporcional,
                s.UsadoEnCurso,
                s.Saldo,
                s.AjusteCorte,
                s.Estado
            };
        }

        /// <summary>Fecha y saldo de corte; los dos vacios = sin corte.</summary>
        private static string LeerCorte(object p, out DateTime? fecha, out decimal? saldo)
        {
            saldo = null;
            string error = LeerFecha(p, "fecha", out fecha);
            return error != "" ? error : LeerDias(p, "saldo", out saldo);
        }

        private static string LeerFecha(object p, string nombre, out DateTime? valor)
        {
            valor = null;
            string t = Campo(p, nombre);
            if (t == "") { return ""; }
            DateTime f;
            if (!DateTime.TryParseExact(t, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out f)) { return "Indique una fecha válida."; }
            valor = f;
            return "";
        }

        private static string LeerDias(object p, string nombre, out decimal? valor)
        {
            valor = null;
            string t = Campo(p, nombre).Replace(" ", "");
            if (t == "") { return ""; }
            decimal d;
            if (!decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out d)) { return "Los días no son un número válido."; }
            valor = d;
            return "";
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
