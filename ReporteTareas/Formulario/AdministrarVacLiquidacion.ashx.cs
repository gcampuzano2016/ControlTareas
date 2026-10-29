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
    /// Handler de VacacionesLiquidacion.aspx. Acciones: ListarColaboradores y
    /// Calcular. Solo lee: la liquidacion se simula, no se registra.
    ///
    /// Muestra sueldos: el permiso (pantalla VacacionesLiquidacion.aspx) se
    /// comprueba en cada llamada, no solo en el menu.
    /// </summary>
    public class AdministrarVacLiquidacion : IHttpHandler, System.Web.SessionState.IRequiresSessionState
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
            else if (!Permisos.PuedeEntrar(context, Pantallas.VacacionesLiquidacion))
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
                string accion = Campo(peticion, "action");
                if (accion == "ListarColaboradores") { return Listar(); }
                if (accion == "Calcular") { return Calcular(parametros); }
                return Mensaje("0", "La acción solicitada no existe.", "danger");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Mensaje("0", "Petición no válida.", "danger");
            }
        }

        private static string Listar()
        {
            try
            {
                DateTime hoy = DateTime.Today;
                return Exito(NegVacLiquidacion.Colaboradores().Select(c => new
                {
                    c.IdColaborador,
                    c.Nombre,
                    c.Cedula,
                    c.Estado,
                    fechaPorDefecto = Fecha(NegVacLiquidacion.FechaPorDefecto(c, hoy))
                }).ToList());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Mensaje("0", "No se pudo cargar la lista de colaboradores.", "danger");
            }
        }

        private static string Calcular(object p)
        {
            try
            {
                int id;
                if (!int.TryParse(Campo(p, "idColaborador"), NumberStyles.None, CultureInfo.InvariantCulture, out id) || id <= 0)
                {
                    return Mensaje("0", "Elija un colaborador.", "warning");
                }

                DateTime? fecha = null;
                string texto = Campo(p, "fecha");
                if (texto != "")
                {
                    DateTime f;
                    if (!DateTime.TryParseExact(texto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out f))
                    {
                        return Mensaje("0", "Indique una fecha de salida válida.", "warning");
                    }
                    string error = NegVacSaldos.ValidarFecha(f, DateTime.Today);
                    if (error != "") { return Mensaje("0", error, "warning"); }
                    fecha = f;
                }

                decimal? ganado = null;
                string g = Campo(p, "ganado").Replace(" ", "");
                if (g != "")
                {
                    decimal v;
                    if (!decimal.TryParse(g, NumberStyles.Number, CultureInfo.InvariantCulture, out v))
                    {
                        return Mensaje("0", "Lo ganado en los últimos 12 meses no es un número válido.", "warning");
                    }
                    string error = NegVacLiquidacion.ValidarGanado(v);
                    if (error != "") { return Mensaje("0", error, "warning"); }
                    ganado = v;
                }

                EntVacLiquidacion l = NegVacLiquidacion.Calcular(id, fecha, ganado, DateTime.Today);
                if (l == null) { return Mensaje("0", "El colaborador no existe.", "warning"); }

                EntVacFicha f2 = l.Ficha;
                EntVacColaborador c = f2.Colaborador;
                EntVacSaldo s = f2.Saldo;
                return Exito(new
                {
                    fechaSalida = Fecha(l.FechaSalida),
                    persona = new
                    {
                        c.Nombre,
                        c.Cedula,
                        c.Empresa,
                        c.Cargo,
                        c.Estado,
                        ingreso = Fecha(c.FechaIngreso),
                        salidaRegistrada = Fecha(c.FechaSalida),
                        corte = Fecha(c.FechaCorte),
                        c.SaldoCorte
                    },
                    l.Antiguedad,
                    l.Sueldo,
                    l.FuenteSueldo,
                    enCurso = new
                    {
                        inicio = Fecha(s.InicioPeriodo),
                        proximoAniversario = Fecha(s.ProximoAniversario),
                        l.DiasTranscurridos,
                        l.DiasDelPeriodo,
                        s.DiasPorAnio,
                        s.Proporcional
                    },
                    resumen = new
                    {
                        s.Disponible,
                        s.Proporcional,
                        s.UsadoEnCurso,
                        dias = s.Saldo,
                        l.ValorReferencial,
                        l.GanadoUltimoAnio,
                        l.ValorArt71
                    },
                    periodos = f2.Periodos.Where(x => x.Estado != "FUTURO" && x.Estado != "CERRADO AL CORTE").OrderBy(x => x.N).Select(x => new
                    {
                        x.Periodo,
                        inicio = Fecha(x.Inicio),
                        fin = Fecha(x.Fin),
                        x.DiasGanados,
                        x.DiasTomados,
                        /* El periodo en curso se liquida por su proporcional, como LIQUIDACION!F. */
                        saldo = x.Estado == "EN CURSO" ? s.Proporcional - x.DiasTomados : x.Saldo,
                        x.DiasVencidos,
                        vence = x.Estado == "EN CURSO" ? "" : Fecha(x.Vence),
                        estado = x.Estado == "EN CURSO" ? "EN CURSO (proporcional)" : x.Estado
                    }).ToList(),
                    cerradosAlCorte = f2.Periodos.Count(x => x.Estado == "CERRADO AL CORTE"),
                    solicitudes = f2.Movimientos.Where(m => l.PeriodoDe.ContainsKey(m.IdMovimiento)).Select(m => new
                    {
                        desde = Fecha(m.Desde),
                        hasta = Fecha(m.Hasta),
                        m.Tipo,
                        m.Estado,
                        m.Dias,
                        m.Horas,
                        m.Minutos,
                        m.DiasDescuenta,
                        periodo = l.PeriodoDe[m.IdMovimiento]
                    }).ToList(),
                    l.Posteriores
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Mensaje("0", "No se pudo calcular la liquidación. Intente nuevamente.", "danger");
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
