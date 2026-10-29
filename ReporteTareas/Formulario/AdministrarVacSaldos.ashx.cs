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
    /// Handler de VacacionesSaldos.aspx. Acciones: ConsultarSaldos y
    /// ConsultarFicha. Solo lee: no hay ninguna accion que escriba.
    ///
    /// La pantalla muestra cedulas y saldos de todo el personal, asi que el
    /// acceso se comprueba aqui y no solo en el menu: Permisos, pantalla
    /// VacacionesSaldos.aspx (por perfil o como extra del usuario).
    /// </summary>
    public class AdministrarVacSaldos : IHttpHandler, System.Web.SessionState.IRequiresSessionState
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

        internal static bool Autorizado(HttpContext context)
        {
            return Permisos.PuedeEntrar(context, Pantallas.VacacionesSaldos);
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

                if (accion == "ConsultarSaldos") { return ConsultarSaldos(parametros); }
                if (accion == "ConsultarFicha") { return ConsultarFicha(parametros); }
                return Mensaje("0", "La acción solicitada no existe.", "danger");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Mensaje("0", "Petición no válida.", "danger");
            }
        }

        private string ConsultarSaldos(object p)
        {
            try
            {
                DateTime fecha;
                string error;
                if (!LeerFecha(Campo(p, "fecha"), out fecha, out error)) { return Mensaje("0", error, "warning"); }

                EntVacTablero t = NegVacSaldos.Cargar(fecha);
                return Exito(new
                {
                    fechaCalculo = Fecha(t.FechaCalculo),
                    resumen = t.Resumen,
                    avisos = t.Avisos,
                    filas = t.Filas.Select(f => Fila(f.Colaborador, f.Saldo)).ToList()
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Mensaje("0", "No se pudieron consultar los saldos. Intente nuevamente.", "danger");
            }
        }

        private string ConsultarFicha(object p)
        {
            try
            {
                DateTime fecha;
                string error;
                if (!LeerFecha(Campo(p, "fecha"), out fecha, out error)) { return Mensaje("0", error, "warning"); }

                int id;
                if (!int.TryParse(Campo(p, "idColaborador"), NumberStyles.None, CultureInfo.InvariantCulture, out id) || id <= 0)
                {
                    return Mensaje("0", "Colaborador no válido.", "warning");
                }

                EntVacFicha f = NegVacSaldos.Ficha(fecha, id);
                if (f == null) { return Mensaje("0", "El colaborador no existe.", "warning"); }

                return Exito(new
                {
                    fechaCalculo = Fecha(f.FechaCalculo),
                    persona = Fila(f.Colaborador, f.Saldo),
                    periodos = f.Periodos.Select(x => new
                    {
                        x.N,
                        x.Periodo,
                        inicio = Fecha(x.Inicio),
                        fin = Fecha(x.Fin),
                        x.DiasGanados,
                        x.DiasTomados,
                        x.DiasVencidos,
                        x.Saldo,
                        vence = Fecha(x.Vence),
                        x.Estado
                    }).ToList(),
                    movimientos = f.Movimientos.Select(x => new
                    {
                        x.IdMovimiento,
                        x.Tipo,
                        x.Estado,
                        desde = Fecha(x.Desde),
                        hasta = Fecha(x.Hasta),
                        x.Situacion,
                        x.AnioServicio,
                        x.Dias,
                        x.Horas,
                        x.Minutos,
                        x.DiasBase,
                        x.DiasFinDeSemana,
                        x.DiasDescuenta,
                        x.Cruces,
                        x.BeneficioDos,
                        x.Observacion,
                        x.Origen
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Mensaje("0", "No se pudo consultar el detalle. Intente nuevamente.", "danger");
            }
        }

        // ------------------------------------------------------- salida

        /// <summary>Fechas como texto yyyy-MM-dd: JavaScriptSerializer las manda como \/Date(...)\/ y en UTC.</summary>
        private static object Fila(EntVacColaborador c, EntVacSaldo s)
        {
            return new
            {
                c.IdColaborador,
                c.Cedula,
                c.Nombre,
                c.Empresa,
                c.Area,
                c.Cargo,
                ingreso = Fecha(c.FechaIngreso),
                estadoColaborador = c.Estado,
                salida = Fecha(c.FechaSalida),
                corte = Fecha(c.FechaCorte),
                c.SaldoCorte,
                observacionColaborador = c.Observacion,
                fechaSaldo = s == null ? "" : Fecha(s.FechaCalculo),
                cumplidos = s == null ? (int?)null : s.Cumplidos,
                diasPorAnio = s == null ? (decimal?)null : s.DiasPorAnio,
                inicioPeriodo = s == null ? "" : Fecha(s.InicioPeriodo),
                proximoAniversario = s == null ? "" : Fecha(s.ProximoAniversario),
                ajusteCorte = s == null ? (decimal?)null : s.AjusteCorte,
                disponible = s == null ? (decimal?)null : s.Disponible,
                usadoEnCurso = s == null ? (decimal?)null : s.UsadoEnCurso,
                proporcional = s == null ? (decimal?)null : s.Proporcional,
                saldo = s == null ? (decimal?)null : s.Saldo,
                porVencer = s == null ? (decimal?)null : s.PorVencer,
                proximoVencimiento = s == null ? "" : Fecha(s.ProximoVencimiento),
                vencidos = s == null ? (decimal?)null : s.Vencidos,
                totalGozado = s == null ? (decimal?)null : s.TotalGozado,
                estadoSaldo = s == null ? "" : s.Estado
            };
        }

        private static string Fecha(DateTime? d)
        {
            return d.HasValue ? d.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "";
        }

        private static bool LeerFecha(string texto, out DateTime fecha, out string error)
        {
            if (texto == "")
            {
                fecha = DateTime.Today;
            }
            else if (!DateTime.TryParseExact(texto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
            {
                error = "Indique una fecha de cálculo válida.";
                return false;
            }
            error = NegVacSaldos.ValidarFecha(fecha, DateTime.Today);
            return error == "";
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
