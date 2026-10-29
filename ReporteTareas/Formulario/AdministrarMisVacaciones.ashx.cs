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
    /// Handler de MisVacaciones.aspx. Una sola accion, ConsultarMisVacaciones,
    /// sin parametros: de quien es el saldo lo dice el Cod_Usuario de la sesion
    /// y nada mas, asi nadie puede pedir el de otra persona cambiando la
    /// peticion. Solo lee.
    /// </summary>
    public class AdministrarMisVacaciones : IHttpHandler, System.Web.SessionState.IRequiresSessionState
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
            else if (!Permisos.PuedeEntrar(context, Pantallas.MisVacaciones))
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
                object accion = null;
                if (peticion == null || !peticion.TryGetValue("action", out accion)) { return Mensaje("0", "Petición no válida.", "danger"); }

                if (Convert.ToString(accion, CultureInfo.InvariantCulture) == "ConsultarMisVacaciones")
                {
                    return Consultar(Convert.ToString(context.Session["Cod_Usuario"], CultureInfo.InvariantCulture).Trim());
                }
                return Mensaje("0", "La acción solicitada no existe.", "danger");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Mensaje("0", "Petición no válida.", "danger");
            }
        }

        private static string Consultar(string codUsuario)
        {
            try
            {
                EntVacMias r = NegVacMias.Consultar(codUsuario, DateTime.Today);
                if (r.Ficha == null) { return Mensaje("0", r.Aviso, "warning"); }

                EntVacFicha f = r.Ficha;
                EntVacColaborador c = f.Colaborador;
                EntVacSaldo s = f.Saldo;
                List<EntVacMovimiento> descuentan = f.Movimientos.Where(m => r.PeriodoDe.ContainsKey(m.IdMovimiento)).ToList();

                return Exito(new
                {
                    fechaCalculo = Fecha(s.FechaCalculo),
                    persona = new
                    {
                        c.Nombre,
                        c.Cedula,
                        c.Empresa,
                        c.Area,
                        c.Cargo,
                        ingreso = Fecha(c.FechaIngreso),
                        antiguedad = NegVacEstado.Antiguedad(c.FechaIngreso, s.FechaCalculo),
                        corte = Fecha(c.FechaCorte),
                        c.SaldoCorte
                    },
                    saldo = new
                    {
                        s.DiasPorAnio,
                        proximoAniversario = Fecha(s.ProximoAniversario),
                        inicioPeriodo = Fecha(s.InicioPeriodo),
                        s.Disponible,
                        s.Proporcional,
                        s.UsadoEnCurso,
                        s.Saldo,
                        s.PorVencer,
                        proximoVencimiento = Fecha(s.ProximoVencimiento),
                        s.Estado,
                        mensaje = NegVacEstado.Mensaje(s)
                    },
                    periodos = f.Periodos.OrderBy(p => p.N).Select(p => new
                    {
                        p.Periodo,
                        inicio = Fecha(p.Inicio),
                        fin = Fecha(p.Fin),
                        p.DiasGanados,
                        p.DiasTomados,
                        p.DiasVencidos,
                        p.Saldo,
                        vence = Fecha(p.Vence),
                        p.Estado
                    }).ToList(),
                    solicitudes = descuentan.Select(m => new
                    {
                        m.Tipo,
                        m.Estado,
                        desde = Fecha(m.Desde),
                        hasta = Fecha(m.Hasta),
                        m.Dias,
                        m.Horas,
                        m.Minutos,
                        m.DiasFinDeSemana,
                        m.DiasDescuenta,
                        m.BeneficioDos,
                        periodo = r.PeriodoDe[m.IdMovimiento],
                        m.Observacion
                    }).ToList(),
                    otras = f.Movimientos.Count - descuentan.Count
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Mensaje("0", "No se pudo consultar su saldo. Intente nuevamente.", "danger");
            }
        }

        private static string Fecha(DateTime? d)
        {
            return d.HasValue ? d.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "";
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
            return new JavaScriptSerializer().Serialize(o);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
