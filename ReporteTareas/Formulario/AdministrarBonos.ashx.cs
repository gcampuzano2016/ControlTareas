using CapaEntidad;
using CapaNegocio;
using CorreoHelper;
using ReporteTareas.clases;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

namespace JsonJQueryNetBonos
{
    /// <summary>
    /// Handler de las cuatro pantallas de Bonos (BonosProponer, BonosRevisar,
    /// BonosCatalogo, BonosReporte).
    ///
    /// Quien es el usuario sale SIEMPRE de la sesion (Cod_Usuario). Cada accion
    /// comprueba la pantalla que la habilita ANTES de llamar a NegBono: NegBono
    /// no mira pantallas. Las reglas viven en CapaNegocio; este archivo lee la
    /// peticion, pregunta y responde.
    ///
    /// Peticion: POST application/json, un objeto {action, parameters:{...}} o
    /// una lista con ese objeto (como AdministrarAgenda). Tambien vale
    /// "accion" en lugar de "action" y los campos sueltos en lugar de
    /// "parameters". Respuesta: {estado, mensaje, aviso, tipoMensaje, resultado}
    /// con estado "1" = hecho, "0" = no, "2" = pide confirmar (bono repetido:
    /// vuelva a enviar con confirmarRepetido = true).
    ///
    /// IRequiresSessionState y no la de solo lectura: Permisos guarda su cache
    /// en la sesion.
    /// </summary>
    public class AdministrarBonos : IHttpHandler, IRequiresSessionState
    {
        private static readonly string[] AccionesProponer = { "CargarProponer", "GuardarBorrador", "Borrar", "Enviar" };
        private static readonly string[] AccionesRevisar =
        {
            "CargarRevisar", "Aprobar", "Ajustar", "Rechazar", "Desactivar", "AprobarVarios",
            "RegistrarDirecto", "EditarBono", "EliminarBono", "AbrirMes", "PrevioCierre", "CerrarMes", "SemanasTomadas"
        };
        private static readonly string[] AccionesCatalogo = { "ListarTipos", "GuardarTipo" };
        private static readonly string[] AccionesReporte = { "ListarMeses" };

        private const string SinPermiso = "No tiene permisos para esta acción.";
        private const int MaximoAprobarVarios = 500;

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
            else if (!Interno(context))
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

        /// <summary>Los usuarios de clientes (IdCliente distinto de 0) no usan los bonos.</summary>
        internal static bool Interno(HttpContext context)
        {
            if (context == null || context.Session == null) { return false; }
            string id = Convert.ToString(context.Session["IdCliente"], CultureInfo.InvariantCulture).Trim();
            return id == "" || id == "0";
        }

        private string Despachar(HttpContext context)
        {
            IDictionary<string, object> peticion = null;
            try
            {
                string json = new System.IO.StreamReader(context.Request.InputStream, Encoding.UTF8).ReadToEnd();
                object raiz = new JavaScriptSerializer().DeserializeObject(json);
                peticion = raiz as IDictionary<string, object>;
                if (peticion == null)
                {
                    IEnumerable lista = raiz as IEnumerable;
                    if (lista != null)
                    {
                        foreach (object primero in lista) { peticion = primero as IDictionary<string, object>; break; }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
            }
            if (peticion == null) { return Mensaje("0", "Petición no válida.", "danger"); }

            string accion = Campo(peticion, "action");
            if (accion == "") { accion = Campo(peticion, "accion"); }
            object p;
            if (!peticion.TryGetValue("parameters", out p) || !(p is IDictionary<string, object>)) { p = peticion; }

            string pantalla = PantallaDe(accion);
            if (pantalla == null) { return Mensaje("0", "La acción solicitada no existe.", "danger"); }
            if (!Permisos.PuedeEntrar(context, pantalla)) { return Mensaje("0", SinPermiso, "danger"); }

            try
            {
                switch (accion)
                {
                    case "CargarProponer": return CargarProponer(context);
                    case "GuardarBorrador": return GuardarBorrador(context, p);
                    case "Borrar": return Borrar(context, p);
                    case "Enviar": return Enviar(context);

                    case "CargarRevisar": return CargarRevisar();
                    case "Aprobar":
                    case "Ajustar":
                    case "Rechazar":
                    case "Desactivar": return Revisar(context, p, accion);
                    case "SemanasTomadas": return SemanasTomadas(p);
                    case "AprobarVarios": return AprobarVarios(context, p);
                    case "RegistrarDirecto": return RegistrarDirecto(context, p);
                    case "EditarBono": return EditarBono(context, p);
                    case "EliminarBono":
                        return Responder(NegBono.EliminarRevisando(Entero(Campo(p, "idBono")), Campo(p, "estado"), Usuario(context)));
                    case "AbrirMes": return AbrirMes(context);
                    case "PrevioCierre": return PrevioCierre();
                    case "CerrarMes": return Responder(NegBono.CerrarMes(Usuario(context)));

                    case "ListarTipos": return Ok(NegBono.Catalogo().Select(ProyTipo).ToList());
                    case "GuardarTipo": return GuardarTipo(context, p);

                    case "ListarMeses": return Ok(NegBono.Reporte().Select(ProyMes).ToList());
                }
                return Mensaje("0", "La acción solicitada no existe.", "danger");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Mensaje("0", "No se pudo completar la operación. Intente nuevamente.", "danger");
            }
        }

        /// <summary>La pantalla que habilita cada accion; null si la accion no existe.</summary>
        private static string PantallaDe(string accion)
        {
            if (AccionesProponer.Contains(accion)) { return Pantallas.BonosProponer; }
            if (AccionesRevisar.Contains(accion)) { return Pantallas.BonosRevisar; }
            if (AccionesCatalogo.Contains(accion)) { return Pantallas.BonosCatalogo; }
            if (AccionesReporte.Contains(accion)) { return Pantallas.BonosReporte; }
            return null;
        }

        private static string Usuario(HttpContext context)
        {
            return NegBonoReglas.Normalizar(Convert.ToString(context.Session["Cod_Usuario"], CultureInfo.InvariantCulture));
        }

        // ------------------------------------------------------ jefe

        private string CargarProponer(HttpContext context)
        {
            string yo = Usuario(context);
            if (yo == "") { return Mensaje("0", "No se pudo identificar su usuario.", "warning"); }
            EntBonoPantallaProponer p = NegBono.PantallaProponer(yo, DateTime.Today);
            return Ok(new
            {
                periodo = ProyPeriodo(p.Periodo),
                gente = p.Gente.Select(g => new { codUsuario = g.CodUsuario, nombre = g.Nombre, empresa = g.Empresa, lugarSugerido = g.LugarSugerido, avisoIdentificacion = g.AvisoIdentificacion }).ToList(),
                bonos = p.Bonos.Select(ProyBono).ToList(),
                tipos = p.Tipos.Select(ProyTipo).ToList(),
                semanasTomadas = p.SemanasTomadas.Select(s => new { idBono = s.IdBono, codUsuario = s.CodUsuario, idTipo = s.IdTipo, lunes = Isos(s.Lunes) }).ToList(),
                semanasMarcables = Isos(p.SemanasMarcables)
            });
        }

        private string GuardarBorrador(HttpContext context, object p)
        {
            string error;
            EntBono b = LeerBono(p, out error);
            if (b == null) { return Mensaje("0", error, "warning"); }
            return Responder(NegBono.GuardarBorrador(b, Usuario(context), Bool(Campo(p, "confirmarRepetido")), DateTime.Today));
        }

        private string Borrar(HttpContext context, object p)
        {
            return Responder(NegBono.Borrar(Entero(Campo(p, "idBono")), Campo(p, "estado"), Usuario(context)));
        }

        private string Enviar(HttpContext context)
        {
            return Responder(NegBono.Enviar(Usuario(context), DateTime.Today, Correo(), Url(context, "BonosRevisar.aspx")));
        }

        // ------------------------------------------------------ Talento Humano

        private string CargarRevisar()
        {
            EntBonoPantallaRevisar p = NegBono.PantallaRevisar();
            return Ok(new
            {
                periodo = ProyPeriodo(p.Periodo),
                bonos = p.Bonos.Select(ProyBono).ToList(),
                resumen = new
                {
                    borradores = p.Resumen.Borradores,
                    enviados = p.Resumen.Enviados,
                    aprobados = p.Resumen.Aprobados,
                    rechazados = p.Resumen.Rechazados,
                    totalAprobado = p.Resumen.TotalAprobado
                },
                personas = p.Personas.Select(x => new { codUsuario = x.CodUsuario, nombre = x.Nombre, empresa = NegBonoReglas.EmpresaNormalizada(x.Empresa), codJefe = x.CodJefe }).ToList(),
                tipos = p.Tipos.Select(ProyTipo).ToList(),
                semanasMarcables = Isos(NegBono.SemanasMarcables(DateTime.Today))
            });
        }

        /// <summary>Lectura: semanas ya tomadas por una persona en todos los periodos (calendario del bono directo).</summary>
        private string SemanasTomadas(object p)
        {
            string cod = Campo(p, "codUsuario");
            if (cod == "") { return Mensaje("0", "Falta la persona.", "warning"); }
            return Ok(NegBono.SemanasTomadasDe(cod)
                .Select(s => new { idBono = s.IdBono, codUsuario = s.CodUsuario, idTipo = s.IdTipo, lunes = Isos(s.Lunes) }).ToList());
        }

        private string Revisar(HttpContext context, object p, string accion)
        {
            decimal? valor = null;
            if (accion == "Ajustar")
            {
                string texto = Campo(p, "valor");
                if (texto != "")
                {
                    decimal v;
                    if (!LeerDecimal(texto, out v)) { return Mensaje("0", "El valor no es válido.", "warning"); }
                    valor = v;
                }
            }
            return Responder(NegBono.Revisar(Entero(Campo(p, "idBono")), Campo(p, "estado"), accion, valor,
                Campo(p, "motivo"), Usuario(context), Correo(), Url(context, "BonosProponer.aspx")));
        }

        /// <summary>
        /// Aprueba uno por uno con la misma regla que Aprobar (estado esperado
        /// incluido): el que no se puede se nombra, los demas se aprueban.
        /// </summary>
        private string AprobarVarios(HttpContext context, object p)
        {
            List<KeyValuePair<int, string>> items = new List<KeyValuePair<int, string>>();
            IDictionary<string, object> d = p as IDictionary<string, object>;
            object valor;
            IEnumerable lista = d != null && d.TryGetValue("items", out valor) && !(valor is string) ? valor as IEnumerable : null;
            if (lista != null)
            {
                foreach (object item in lista)
                {
                    int id = Entero(Campo(item, "idBono"));
                    if (id > 0 && !items.Any(x => x.Key == id)) { items.Add(new KeyValuePair<int, string>(id, Campo(item, "estado"))); }
                }
            }
            if (items.Count == 0) { return Mensaje("0", "Elija al menos un bono para aprobar.", "warning"); }
            if (items.Count > MaximoAprobarVarios) { return Mensaje("0", "No se pueden aprobar más de " + MaximoAprobarVarios + " bonos a la vez.", "warning"); }

            string yo = Usuario(context);
            string url = Url(context, "BonosProponer.aspx");
            Func<string, string, string, bool> correo = Correo();
            int aprobados = 0;
            List<object> fallidos = new List<object>();
            List<string> avisos = new List<string>();
            foreach (KeyValuePair<int, string> it in items)
            {
                string mensaje;
                try
                {
                    EntBonoResultado r = NegBono.Revisar(it.Key, it.Value, "Aprobar", null, "", yo, correo, url);
                    if (r.Ok)
                    {
                        aprobados++;
                        if (!string.IsNullOrWhiteSpace(r.Aviso)) { avisos.Add(r.Aviso); }
                        continue;
                    }
                    mensaje = string.IsNullOrWhiteSpace(r.Mensaje) ? (r.Aviso ?? "") : r.Mensaje;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.ToString());
                    mensaje = "No se pudo completar la operación; recargue la pantalla para ver si quedó aprobado.";
                }
                fallidos.Add(new { idBono = it.Key, mensaje });
            }

            string texto = "Se aprobaron " + aprobados + " de " + items.Count + " bono(s).";
            return Serializar(new Respuesta
            {
                estado = aprobados > 0 || fallidos.Count == 0 ? "1" : "0",
                mensaje = texto,
                aviso = string.Join(" ", avisos.Distinct()),
                tipoMensaje = fallidos.Count == 0 ? "success" : "warning",
                resultado = new { aprobados, total = items.Count, fallidos }
            });
        }

        private string RegistrarDirecto(HttpContext context, object p)
        {
            string error;
            EntBono b = LeerBono(p, out error);
            if (b == null) { return Mensaje("0", error, "warning"); }
            return Responder(NegBono.RegistrarDirecto(b, Usuario(context), Bool(Campo(p, "confirmarRepetido")), DateTime.Today));
        }

        private string EditarBono(HttpContext context, object p)
        {
            string error;
            EntBono b = LeerBono(p, out error);
            if (b == null) { return Mensaje("0", error, "warning"); }
            if (b.IdBono <= 0) { return Mensaje("0", "Falta el bono a editar.", "warning"); }
            return Responder(NegBono.EditarRevisando(b, Usuario(context), Bool(Campo(p, "confirmarRepetido")), DateTime.Today));
        }

        private string AbrirMes(HttpContext context)
        {
            return Responder(NegBono.AbrirMes(Usuario(context), Correo(), Url(context, "BonosProponer.aspx")));
        }

        private string PrevioCierre()
        {
            EntBonoCierre c = NegBono.PrevioCierre();
            if (c == null) { return Mensaje("0", "No hay un mes de bonos abierto.", "warning"); }
            return Ok(new
            {
                quedanFuera = c.QuedanFuera.Select(ProyBono).ToList(),
                sinConcepto = c.SinConcepto,
                puedeCerrar = c.PuedeCerrar,
                total = c.Total
            });
        }

        // ------------------------------------------------------ catalogo

        private string GuardarTipo(HttpContext context, object p)
        {
            EntBonoTipo t = new EntBonoTipo
            {
                IdTipo = Entero(Campo(p, "idTipo")),
                Nombre = Campo(p, "nombre"),
                Familia = Campo(p, "familia"),
                PorSemana = Bool(Campo(p, "porSemana")),
                Activo = Bool(Campo(p, "activo"))
            };
            string sugerido = Campo(p, "valorSugerido");
            if (sugerido != "")
            {
                decimal v;
                if (!LeerDecimal(sugerido, out v)) { return Mensaje("0", "El valor sugerido no es válido.", "warning"); }
                t.ValorSugerido = v;
            }

            IDictionary<string, object> d = p as IDictionary<string, object>;
            object conceptos;
            if (d != null && d.TryGetValue("conceptos", out conceptos))
            {
                IDictionary<string, object> c = conceptos as IDictionary<string, object>;
                if (c != null)
                {
                    foreach (KeyValuePair<string, object> kv in c)
                    {
                        t.Conceptos[kv.Key] = kv.Value == null ? "" : Convert.ToString(kv.Value, CultureInfo.InvariantCulture);
                    }
                }
            }
            return Responder(NegBono.GuardarTipo(t, Usuario(context)));
        }

        // ------------------------------------------------------ lectura

        /// <summary>
        /// El bono que manda la pantalla. "estado" es el estado esperado (el que
        /// la pantalla tenia). null y el motivo si algun campo no se entiende.
        /// </summary>
        private static EntBono LeerBono(object p, out string error)
        {
            error = null;
            EntBono b = new EntBono
            {
                IdBono = Entero(Campo(p, "idBono")),
                Estado = Campo(p, "estado"),
                CodUsuario = Campo(p, "codUsuario"),
                IdTipo = Entero(Campo(p, "idTipo")),
                Semanas = Entero(Campo(p, "semanas")),
                Lugar = Campo(p, "lugar"),
                MotivoAjuste = Campo(p, "motivoAjuste"),
                Observacion = Campo(p, "observacion")
            };

            decimal v;
            if (!LeerDecimal(Campo(p, "valor"), out v))
            {
                error = "El valor no es válido.";
                return null;
            }
            b.Valor = v;

            IDictionary<string, object> d = p as IDictionary<string, object>;
            object valor;
            IEnumerable lista = d != null && d.TryGetValue("lunes", out valor) && !(valor is string) ? valor as IEnumerable : null;
            if (lista != null)
            {
                foreach (object item in lista)
                {
                    DateTime f;
                    if (item == null || !DateTime.TryParseExact(Convert.ToString(item, CultureInfo.InvariantCulture).Trim(), "yyyy-MM-dd",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out f))
                    {
                        error = "Una de las semanas no es una fecha válida.";
                        return null;
                    }
                    b.Lunes.Add(f.Date);
                }
            }
            return b;
        }

        private static string Campo(object campos, string nombre)
        {
            IDictionary<string, object> d = campos as IDictionary<string, object>;
            object v;
            if (d == null || !d.TryGetValue(nombre, out v) || v == null) { return ""; }
            if (v is bool) { return (bool)v ? "1" : "0"; }
            return Convert.ToString(v, CultureInfo.InvariantCulture).Trim();
        }

        private static int Entero(string texto)
        {
            int n;
            return int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out n) ? n : 0;
        }

        private static bool Bool(string texto)
        {
            string t = (texto ?? "").Trim().ToLowerInvariant();
            return t == "1" || t == "true" || t == "si" || t == "sí";
        }

        /// <summary>Decimal con punto; si solo trae coma, la coma es el separador decimal. Vacio es 0.</summary>
        private static bool LeerDecimal(string texto, out decimal valor)
        {
            valor = 0m;
            string t = (texto ?? "").Trim();
            if (t == "") { return true; }
            if (t.Contains(",") && !t.Contains(".")) { t = t.Replace(',', '.'); }
            return decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
        }

        // ------------------------------------------------------ correo y enlaces

        private static Func<string, string, string, bool> Correo()
        {
            return (destinatario, asunto, cuerpo) => new EnvioCorreoHelper().EnviarCorreoBonos(destinatario, asunto, cuerpo);
        }

        /// <summary>Enlace absoluto a una pantalla de Bonos: URL_SITE si esta configurada; si no, el host de esta peticion.</summary>
        private static string Url(HttpContext context, string pagina)
        {
            string sitio = "";
            try
            {
                sitio = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_SITE");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Bonos: no se pudo leer URL_SITE. " + ex);
            }
            if (!string.IsNullOrWhiteSpace(sitio)) { return sitio.Trim().TrimEnd('/') + "/Formulario/" + pagina; }
            return context.Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/Formulario/" + pagina);
        }

        // ------------------------------------------------------ proyecciones (fechas como yyyy-MM-dd)

        private static string Iso(DateTime d)
        {
            return d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static string Iso(DateTime? d)
        {
            return d.HasValue ? Iso(d.Value) : null;
        }

        private static List<string> Isos(IEnumerable<DateTime> lunes)
        {
            return (lunes ?? new DateTime[0]).Select(Iso).ToList();
        }

        private static object ProyPeriodo(EntBonoPeriodo p)
        {
            if (p == null) { return null; }
            return new
            {
                idPeriodo = p.IdPeriodo,
                anio = p.Anio,
                mes = p.Mes,
                estado = p.Estado,
                totalAprobado = p.TotalAprobado,
                fechaApertura = Iso(p.FechaApertura),
                fechaCierre = Iso(p.FechaCierre)
            };
        }

        private static object ProyBono(EntBono b)
        {
            return new
            {
                idBono = b.IdBono,
                idPeriodo = b.IdPeriodo,
                codUsuario = b.CodUsuario,
                empresa = b.Empresa,
                nombre = b.Nombre,
                identificacion = b.Identificacion,
                lugar = b.Lugar,
                idTipo = b.IdTipo,
                nombreTipo = b.NombreTipo,
                semanas = b.Semanas,
                valorSugerido = b.ValorSugerido,
                valor = b.Valor,
                motivoAjuste = b.MotivoAjuste,
                observacion = b.Observacion,
                estado = b.Estado,
                origen = b.Origen,
                codJefe = b.CodJefe,
                motivoRechazo = b.MotivoRechazo,
                lunes = Isos(b.Lunes)
            };
        }

        private static object ProyTipo(EntBonoTipo t)
        {
            return new
            {
                idTipo = t.IdTipo,
                nombre = t.Nombre,
                familia = t.Familia,
                valorSugerido = t.ValorSugerido,
                porSemana = t.PorSemana,
                activo = t.Activo,
                conceptos = t.Conceptos
            };
        }

        private static object ProyMes(EntBonoMes m)
        {
            return new { idPeriodo = m.IdPeriodo, anio = m.Anio, mes = m.Mes, estado = m.Estado, total = m.Total, variacion = m.Variacion };
        }

        // ------------------------------------------------------ respuesta

        private sealed class Respuesta
        {
            public string estado { get; set; }
            public string mensaje { get; set; }
            public string aviso { get; set; }
            public string tipoMensaje { get; set; }
            public object resultado { get; set; }
        }

        /// <summary>
        /// Ok = true: estado "1" (con aviso si lo hay; "warning" si hay aviso o
        /// quedo incompleto, p. ej. Enviar salto algun bono). Ok = false con Mensaje:
        /// "0". Ok = false sin Mensaje y con Aviso: "2", pide confirmar.
        /// </summary>
        private static string Responder(EntBonoResultado r)
        {
            if (r.Ok)
            {
                bool advertir = !string.IsNullOrWhiteSpace(r.Aviso) || r.Incompleto;
                return Serializar(new Respuesta { estado = "1", mensaje = r.Mensaje ?? "", aviso = r.Aviso ?? "", tipoMensaje = advertir ? "warning" : "success", resultado = "" });
            }
            if (string.IsNullOrWhiteSpace(r.Mensaje) && !string.IsNullOrWhiteSpace(r.Aviso))
            {
                return Serializar(new Respuesta { estado = "2", mensaje = "", aviso = r.Aviso, tipoMensaje = "warning", resultado = "" });
            }
            return Serializar(new Respuesta { estado = "0", mensaje = r.Mensaje ?? "", aviso = r.Aviso ?? "", tipoMensaje = "warning", resultado = "" });
        }

        private static string Ok(object resultado)
        {
            return Serializar(new Respuesta { estado = "1", mensaje = "", aviso = "", tipoMensaje = "success", resultado = resultado });
        }

        private static string Mensaje(string estado, string mensaje, string tipo)
        {
            return Serializar(new Respuesta { estado = estado, mensaje = mensaje, aviso = "", tipoMensaje = tipo, resultado = "" });
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
