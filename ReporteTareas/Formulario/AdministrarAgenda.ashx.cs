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

namespace JsonJQueryNetAgenda
{
    /// <summary>
    /// Handler de Agenda.aspx y de ParametrizacionAreasAgenda.aspx.
    ///
    /// Quien es el usuario sale SIEMPRE de la sesion: lo que mande el navegador
    /// sobre identidad o perfil no se usa. Quien ve a quien y que se puede hacer
    /// lo deciden las reglas puras de CapaNegocio (NegAgendaEquipo,
    /// NegAgendaActividad, NegAgendaVista); este archivo carga, pregunta y
    /// responde.
    ///
    /// IRequiresSessionState y no la de solo lectura: Permisos guarda su cache
    /// en la sesion.
    /// </summary>
    public class AdministrarAgenda : IHttpHandler, IRequiresSessionState
    {
        private static readonly string[] AccionesDeAreas = { "ListarAreas", "GuardarArea", "ListarPersonas", "AsignarArea" };

        private const string NoActivo = "Su usuario no figura como activo en la agenda. Pida a Talento Humano que revise su estado.";

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

        /// <summary>Los usuarios de clientes (IdCliente distinto de 0) no usan la agenda.</summary>
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
                object[] lista = new JavaScriptSerializer().Deserialize<object[]>(json);
                peticion = lista != null && lista.Length > 0 ? lista[0] as IDictionary<string, object> : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
            }
            if (peticion == null) { return Mensaje("0", "Petición no válida.", "danger"); }

            string accion = Campo(peticion, "action");
            object p;
            peticion.TryGetValue("parameters", out p);

            if (AccionesDeAreas.Contains(accion) && !Permisos.PuedeEntrar(context, Pantallas.ParametrizacionAreasAgenda))
            {
                return Mensaje("0", "No tiene permisos para esta pantalla.", "danger");
            }

            try
            {
                switch (accion)
                {
                    case "Inicio": return Inicio(context);
                    case "Equipo": return Equipo(context, p);
                    case "Agenda": return Agenda(context, p);
                    case "GuardarActividad": return GuardarActividad(context, p);
                    case "MarcarEntregable": return MarcarEntregable(context, p);
                    case "MarcarCompleta": return MarcarCompleta(context, p);
                    case "Anular": return Anular(context, p);
                    case "ListarAreas": return ListarAreas();
                    case "GuardarArea": return GuardarArea(context, p);
                    case "ListarPersonas": return ListarPersonas();
                    case "AsignarArea": return AsignarArea(context, p);
                }
                return Mensaje("0", "La acción solicitada no existe.", "danger");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return Mensaje("0", "No se pudo completar la operación. Intente nuevamente.", "danger");
            }
        }

        // ------------------------------------------------------ quien pregunta

        private sealed class Quien
        {
            public string Yo;
            public bool VerTodos;
            public List<EntAgendaPersona> Personas;
            public HashSet<string> Cadena;

            public EntAgendaPersona Persona(string cod)
            {
                string c = NegAgendaEquipo.Normalizar(cod);
                return Personas.FirstOrDefault(x => NegAgendaEquipo.Codigos.Equals(NegAgendaEquipo.Normalizar(x.CodUsuario), c));
            }
        }

        private static Quien Cargar(HttpContext context)
        {
            Quien q = new Quien
            {
                Yo = Usuario(context),
                VerTodos = Permisos.PuedeHacer(context, Acciones.AgendaVerTodos),
                Personas = NegAgenda.PersonasActivas()
            };
            q.Cadena = NegAgendaEquipo.Cadena(q.Personas, q.Yo);
            return q;
        }

        private static string Usuario(HttpContext context)
        {
            return NegAgendaEquipo.Normalizar(Convert.ToString(context.Session["Cod_Usuario"], CultureInfo.InvariantCulture));
        }

        // ------------------------------------------------------ agenda

        private string Inicio(HttpContext context)
        {
            Quien q = Cargar(context);
            EntAgendaPersona yo = q.Persona(q.Yo);
            if (yo == null) { return Mensaje("0", NoActivo, "warning"); }

            DateTime hoy = DateTime.Today;
            return Ok(new
            {
                CodUsuario = yo.CodUsuario,
                Nombre = yo.Nombre,
                TieneEquipo = q.VerTodos || q.Cadena.Count > 0,
                Hoy = Iso(hoy),
                Lunes = Iso(NegAgendaActividad.Lunes(hoy)),
                ProximoHabil = Iso(NegAgendaActividad.ProximoDiaHabil(hoy)),
                Asignables = NegAgendaVista.Visibles(q.Yo, q.VerTodos, q.Personas)
                    .Select(x => new { x.CodUsuario, x.Nombre, x.Area })
                    .ToList()
            });
        }

        private string Equipo(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            if (q.Persona(q.Yo) == null) { return Mensaje("0", NoActivo, "warning"); }
            DateTime hoy = DateTime.Today;
            DateTime lunes = LeerLunes(Campo(p, "lunes"), hoy);
            List<EntAgendaActividad> actividades = NegAgenda.Actividades(lunes, lunes.AddDays(4), hoy);
            return Ok(NegAgendaVista.Equipo(q.Yo, q.VerTodos, q.Personas, actividades, lunes, hoy));
        }

        private string Agenda(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            if (q.Persona(q.Yo) == null) { return Mensaje("0", NoActivo, "warning"); }
            DateTime hoy = DateTime.Today;
            DateTime lunes = LeerLunes(Campo(p, "lunes"), hoy);
            string cod = Campo(p, "cod");
            if (cod == "") { cod = q.Yo; }

            List<EntAgendaActividad> actividades = NegAgenda.Actividades(lunes, lunes.AddDays(4), hoy);
            EntAgendaVistaPersona vista = NegAgendaVista.Persona(q.Yo, q.VerTodos, q.Personas, actividades, cod, lunes, hoy);
            if (vista == null) { return Mensaje("0", "No puede ver la agenda de esa persona.", "warning"); }
            return Ok(vista);
        }

        private string GuardarActividad(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            if (q.Persona(q.Yo) == null) { return Mensaje("0", NoActivo, "warning"); }

            int id = Entero(Campo(p, "id"));
            string titulo = Campo(p, "titulo");
            DateTime? fecha = LeerFecha(Campo(p, "fecha"));
            List<EntAgendaEntregable> entregables = NegAgendaActividad.Limpiar(LeerEntregables(p));
            string error = NegAgendaActividad.Validar(titulo, fecha, entregables);
            if (error != null) { return Mensaje("0", error, "warning"); }

            if (id > 0)
            {
                EntAgendaActividad a = NegAgenda.Actividad(id);
                if (a == null || a.Anulada) { return Mensaje("0", "La actividad no existe o fue anulada.", "warning"); }
                if (!NegAgendaActividad.PuedeEditar(q.Yo, a, q.Cadena, q.VerTodos))
                {
                    return Mensaje("0", "No puede modificar esta actividad.", "warning");
                }
                if (entregables.Any(e => e.IdEntregable != 0 && !a.Entregables.Any(x => x.IdEntregable == e.IdEntregable)))
                {
                    return Mensaje("0", "Petición no válida.", "danger");
                }
                NegAgenda.GuardarActividad(id, titulo, a.CodResponsable, a.CodAsignadoPor, fecha.Value, entregables, q.Yo);
                return Ok(id, "Actividad actualizada.", "success");
            }

            string cod = NegAgendaEquipo.Normalizar(Campo(p, "cod"));
            if (cod == "") { cod = q.Yo; }
            EntAgendaPersona responsable = q.Persona(cod);
            if (responsable == null) { return Mensaje("0", "La persona elegida no está activa.", "warning"); }

            bool propia = NegAgendaEquipo.Codigos.Equals(NegAgendaEquipo.Normalizar(responsable.CodUsuario), q.Yo);
            if (!propia && !NegAgendaEquipo.PuedeAsignarA(q.Yo, responsable.CodUsuario, q.Cadena, q.VerTodos))
            {
                return Mensaje("0", "No puede asignarle actividades a esa persona.", "warning");
            }
            foreach (EntAgendaEntregable e in entregables) { e.IdEntregable = 0; }

            int nueva = NegAgenda.GuardarActividad(0, titulo, responsable.CodUsuario, q.Yo, fecha.Value, entregables, q.Yo);
            if (propia) { return Ok(nueva, "Actividad guardada.", "success"); }

            return Avisar(q, nueva, responsable)
                ? Ok(nueva, "Actividad asignada a " + responsable.Nombre + ".", "success")
                : Ok(nueva, "Actividad asignada, pero no se pudo avisar por correo a " + responsable.Nombre + ".", "warning");
        }

        /// <summary>
        /// El correo nunca deshace el guardado. Cada intento queda en
        /// AG_CorreoBitacora DESPUES de saber si salio. Devuelve si salio.
        /// </summary>
        private static bool Avisar(Quien q, int idActividad, EntAgendaPersona responsable)
        {
            string asunto = "";
            bool enviado = false;
            string error = null;
            string destinatario = string.IsNullOrWhiteSpace(responsable.Correo) ? null : responsable.Correo.Trim();
            try
            {
                EntAgendaActividad a = NegAgenda.Actividad(idActividad);
                if (a == null || !NegAgendaCorreo.DebeAvisar(a)) { return true; }
                asunto = NegAgendaCorreo.Asunto(a);
                if (destinatario == null)
                {
                    error = "La persona no tiene correo registrado.";
                }
                else
                {
                    EntAgendaPersona asignador = q.Persona(a.CodAsignadoPor);
                    string cuerpo = NegAgendaCorreo.Cuerpo(a, responsable.Nombre,
                        asignador != null ? asignador.Nombre : a.CodAsignadoPor,
                        NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_SITE"));
                    EnvioCorreoHelper correo = new EnvioCorreoHelper();
                    enviado = correo.EnviarCorreoAgenda(destinatario, asunto, cuerpo);
                    if (!enviado)
                    {
                        error = string.IsNullOrEmpty(correo.ErrorProceso) ? "El servidor de correo no aceptó el envío." : correo.ErrorProceso;
                    }
                }
            }
            catch (Exception ex)
            {
                enviado = false;
                error = ex.Message;
            }

            try
            {
                NegAgenda.RegistrarCorreo(idActividad, destinatario, asunto, enviado, error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Agenda: no se pudo registrar el correo. " + ex);
            }
            return enviado;
        }

        private string MarcarEntregable(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            EntAgendaActividad a = NegAgenda.Actividad(Entero(Campo(p, "idActividad")));
            if (a == null || !NegAgendaActividad.PuedeMarcar(q.Yo, a))
            {
                return Mensaje("0", "Solo el responsable puede marcar sus entregables.", "warning");
            }
            int idEntregable = Entero(Campo(p, "idEntregable"));
            if (!a.Entregables.Any(e => e.IdEntregable == idEntregable)) { return Mensaje("0", "Petición no válida.", "danger"); }
            NegAgenda.MarcarEntregable(idEntregable, Campo(p, "hecho") == "1", q.Yo);
            return Ok(a.IdActividad, "", "success");
        }

        private string MarcarCompleta(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            EntAgendaActividad a = NegAgenda.Actividad(Entero(Campo(p, "id")));
            if (a == null || !NegAgendaActividad.PuedeMarcar(q.Yo, a))
            {
                return Mensaje("0", "Solo el responsable puede marcar su actividad.", "warning");
            }
            if (a.Entregables.Count > 0)
            {
                return Mensaje("0", "Esta actividad se completa marcando sus entregables.", "warning");
            }
            NegAgenda.MarcarCompleta(a.IdActividad, Campo(p, "completa") == "1", q.Yo);
            return Ok(a.IdActividad, "", "success");
        }

        private string Anular(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            EntAgendaActividad a = NegAgenda.Actividad(Entero(Campo(p, "id")));
            if (a == null || !NegAgendaActividad.PuedeEditar(q.Yo, a, q.Cadena, q.VerTodos))
            {
                return Mensaje("0", "No puede anular esta actividad.", "warning");
            }
            NegAgenda.Anular(a.IdActividad, q.Yo);
            return Ok(a.IdActividad, "Actividad anulada.", "success");
        }

        // ------------------------------------------------------ areas

        private string ListarAreas()
        {
            return Ok(NegAgendaAreas.ConPersonas(NegAgenda.Areas(), NegAgenda.PersonasActivas()));
        }

        private string GuardarArea(HttpContext context, object p)
        {
            string nombre = Campo(p, "nombre");
            string color = Campo(p, "color").ToUpperInvariant();
            string error = NegAgendaAreas.Validar(nombre, color);
            if (error != null) { return Mensaje("0", error, "warning"); }

            int resultado = NegAgenda.GuardarArea(Entero(Campo(p, "id")), nombre, color, Campo(p, "activa") == "1", Usuario(context));
            string mensaje = NegAgendaAreas.MensajeResultado(resultado);
            if (mensaje != null) { return Mensaje("0", mensaje, "warning"); }
            return Ok(resultado, "Área guardada.", "success");
        }

        private string ListarPersonas()
        {
            return Ok(NegAgenda.PersonasActivas().Select(x => new { x.CodUsuario, x.Nombre, x.IdArea }).ToList());
        }

        private string AsignarArea(HttpContext context, object p)
        {
            string cod = NegAgendaEquipo.Normalizar(Campo(p, "cod"));
            EntAgendaPersona persona = NegAgenda.PersonasActivas()
                .FirstOrDefault(x => NegAgendaEquipo.Codigos.Equals(NegAgendaEquipo.Normalizar(x.CodUsuario), cod));
            if (persona == null) { return Mensaje("0", "La persona no está activa.", "warning"); }

            int idArea = Entero(Campo(p, "idArea"));
            EntAgendaArea area = NegAgenda.Areas().FirstOrDefault(a => a.IdArea == idArea);
            if (area == null || (!area.Activa && !area.EsSinArea)) { return Mensaje("0", "Elija un área activa.", "warning"); }

            NegAgenda.AsignarArea(persona.CodUsuario, idArea, Usuario(context));
            return Ok(idArea, persona.Nombre + " quedó en " + area.Nombre + ".", "success");
        }

        // ------------------------------------------------------ lectura y respuesta

        private static string Campo(object campos, string nombre)
        {
            IDictionary<string, object> d = campos as IDictionary<string, object>;
            object v;
            if (d == null || !d.TryGetValue(nombre, out v) || v == null) { return ""; }
            return Convert.ToString(v, CultureInfo.InvariantCulture).Trim();
        }

        private static int Entero(string texto)
        {
            int n;
            return int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out n) ? n : 0;
        }

        private static DateTime? LeerFecha(string texto)
        {
            DateTime d;
            return DateTime.TryParseExact(texto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d)
                ? d.Date
                : (DateTime?)null;
        }

        private static DateTime LeerLunes(string texto, DateTime hoy)
        {
            return NegAgendaActividad.Lunes(LeerFecha(texto) ?? hoy);
        }

        private static string Iso(DateTime d)
        {
            return d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static List<EntAgendaEntregable> LeerEntregables(object p)
        {
            List<EntAgendaEntregable> lista = new List<EntAgendaEntregable>();
            IDictionary<string, object> d = p as IDictionary<string, object>;
            object valor;
            if (d == null || !d.TryGetValue("entregables", out valor) || valor == null || valor is string) { return lista; }
            IEnumerable items = valor as IEnumerable;
            if (items == null) { return lista; }
            foreach (object item in items)
            {
                lista.Add(new EntAgendaEntregable
                {
                    IdEntregable = Entero(Campo(item, "id")),
                    Descripcion = Campo(item, "descripcion")
                });
            }
            return lista;
        }

        private static string Ok(object resultado, string mensaje = "", string tipo = "success")
        {
            return Serializar(new EntRespuesta { estado = "1", mensaje = mensaje, tipoMensaje = tipo, resultado = resultado });
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
