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
    /// Handler de Agenda.aspx, ParametrizacionAreasAgenda.aspx y (Entrega 2)
    /// Procesos.aspx y ParametrizacionPlantillasProcesos.aspx. Cada accion de
    /// esas pantallas comprueba Permisos.PuedeEntrar de la suya antes de todo.
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

        private static readonly string[] AccionesDeProcesos =
        {
            "ListarProcesos", "DatosIniciar", "IniciarProceso", "ReasignarPaso", "CambiarPasoPendiente",
            "CambiarFechaPaso", "CancelarProceso"
        };

        private static readonly string[] AccionesDePlantillas = { "ListarPlantillas", "GuardarPlantilla" };

        private const string NoActivo = "Su usuario no figura como activo en la agenda. Pida a Talento Humano que revise su estado.";

        private const string EsPaso = "Esta actividad es un paso de un proceso: se gestiona desde Procesos.";
        private const string ProcesoCambio = "El proceso cambió; recargue la pantalla.";
        private const string PasoCerrado = "Este paso ya está completo o cancelado: no admite cambios.";
        private const string SinFechaSiguiente = "No se pudo calcular la fecha del paso siguiente. Recargue la pantalla e intente nuevamente.";
        private const string NoGestiona = "Solo quien inició el proceso puede reasignar, cambiar fechas o cancelarlo.";

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
            if (AccionesDeProcesos.Contains(accion) && !Permisos.PuedeEntrar(context, Pantallas.Procesos))
            {
                return Mensaje("0", "No tiene permisos para esta pantalla.", "danger");
            }
            if (AccionesDePlantillas.Contains(accion) && !Permisos.PuedeEntrar(context, Pantallas.ParametrizacionPlantillasProcesos))
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
                    case "ListarProcesos": return ListarProcesos(context, p);
                    case "DatosIniciar": return DatosIniciar(context);
                    case "IniciarProceso": return IniciarProceso(context, p);
                    case "ReasignarPaso": return ReasignarPaso(context, p);
                    case "CambiarPasoPendiente": return CambiarPasoPendiente(context, p);
                    case "CambiarFechaPaso": return CambiarFechaPaso(context, p);
                    case "CancelarProceso": return CancelarProceso(context, p);
                    case "ListarPlantillas": return ListarPlantillas();
                    case "GuardarPlantilla": return GuardarPlantilla(context, p);
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

            /// <summary>Nombre de una persona activa; si no esta activa, su codigo.</summary>
            public string Nombre(string cod)
            {
                EntAgendaPersona x = Persona(cod);
                return x != null ? x.Nombre : NegAgendaEquipo.Normalizar(cod);
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
            MarcarPasos(q, vista, actividades);
            return Ok(vista);
        }

        /// <summary>
        /// Entrega 2: completa los campos de proceso de los items que son el
        /// paso de un proceso. Los demas items no se tocan (y si la semana no
        /// tiene pasos, ni siquiera se leen los procesos).
        /// </summary>
        private static void MarcarPasos(Quien q, EntAgendaVistaPersona vista, List<EntAgendaActividad> actividades)
        {
            Dictionary<int, EntAgendaActividad> pasos = actividades
                .Where(a => a != null && a.IdProcesoPaso.HasValue)
                .GroupBy(a => a.IdActividad)
                .ToDictionary(g => g.Key, g => g.First());
            if (pasos.Count == 0) { return; }

            List<EntAgendaItem> items = vista.Atrasadas.Concat(vista.Dias.SelectMany(d => d.Actividades)).ToList();
            if (!items.Any(i => pasos.ContainsKey(i.IdActividad))) { return; }

            List<EntAgendaProceso> procesos = NegAgenda.Procesos();
            foreach (EntAgendaItem it in items)
            {
                EntAgendaActividad a;
                if (!pasos.TryGetValue(it.IdActividad, out a)) { continue; }

                EntAgendaProcesoPaso paso;
                EntAgendaProceso proceso = BuscarPaso(procesos, a.IdProcesoPaso.Value, out paso);
                it.EsPasoDeProceso = true;
                it.IdProceso = proceso != null ? proceso.IdProceso : a.IdProceso;
                it.NombreProceso = proceso != null ? proceso.Nombre : a.NombreProceso;
                it.PuedeEditar = false;

                string estado = paso != null ? paso.Estado : a.EstadoPaso;
                if (estado == AgendaPasoEstados.Completo || estado == AgendaPasoEstados.Cancelado)
                {
                    it.PuedeMarcar = false;
                }
                if (proceso != null && paso != null && paso.Estado == AgendaPasoEstados.EnCurso)
                {
                    EntAgendaProcesoPaso siguiente = Siguiente(proceso, paso);
                    it.AvisoAlCompletar = NegAgendaProceso.AvisoAlCompletar(proceso, paso,
                        siguiente != null ? q.Nombre(siguiente.CodResponsable) : "");
                }
            }
        }

        private static EntAgendaProceso BuscarPaso(IEnumerable<EntAgendaProceso> procesos, int idPaso, out EntAgendaProcesoPaso paso)
        {
            paso = null;
            foreach (EntAgendaProceso p in procesos ?? Enumerable.Empty<EntAgendaProceso>())
            {
                paso = p.Pasos.FirstOrDefault(s => s.IdProcesoPaso == idPaso);
                if (paso != null) { return p; }
            }
            return null;
        }

        private static EntAgendaProcesoPaso Siguiente(EntAgendaProceso p, EntAgendaProcesoPaso paso)
        {
            return p.Pasos.Where(s => s.Orden > paso.Orden).OrderBy(s => s.Orden).FirstOrDefault();
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
                if (a.IdProcesoPaso.HasValue) { return Mensaje("0", EsPaso, "warning"); }
                if (!NegAgendaActividad.PuedeEditar(q.Yo, a, q.Cadena, q.VerTodos))
                {
                    return Mensaje("0", "No puede modificar esta actividad.", "warning");
                }
                if (entregables.Any(e => e.IdEntregable != 0 && !a.Entregables.Any(x => x.IdEntregable == e.IdEntregable)))
                {
                    return Mensaje("0", "Petición no válida.", "danger");
                }
                int guardada = NegAgenda.GuardarActividad(id, titulo, a.CodResponsable, a.CodAsignadoPor, fecha.Value, entregables, q.Yo);
                if (guardada == -9) { return Mensaje("0", EsPaso, "warning"); }
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
            DateTime? fechaSiguiente = FechaSiguiente(a);
            EntAgendaAvance av = NegAgenda.MarcarEntregable(idEntregable, Campo(p, "hecho") == "1", q.Yo, fechaSiguiente);
            return TrasMarcar(q, a, av);
        }

        /// <summary>
        /// Solo para la actividad de un paso con paso siguiente: la fecha de
        /// entrega que tendra el siguiente si esta marca completa el paso (hoy
        /// + su plazo en dias habiles). El procedimiento la usa solo si el
        /// paso realmente se completa. Sin proceso, o sin siguiente, null: la
        /// llamada al procedimiento queda igual que en la Entrega 1.
        /// </summary>
        private static DateTime? FechaSiguiente(EntAgendaActividad a)
        {
            if (!a.IdProcesoPaso.HasValue) { return null; }
            EntAgendaProcesoPaso paso;
            EntAgendaProceso proceso = BuscarPaso(NegAgenda.Procesos(), a.IdProcesoPaso.Value, out paso);
            if (proceso == null) { return null; }
            EntAgendaProcesoPaso siguiente = Siguiente(proceso, paso);
            if (siguiente == null || siguiente.PlazoDias < NegAgendaDiasHabiles.PlazoMinimo
                || siguiente.PlazoDias > NegAgendaDiasHabiles.PlazoMaximo)
            {
                return null;
            }
            return NegAgendaDiasHabiles.SumarDiasHabiles(DateTime.Today, siguiente.PlazoDias, NegAgenda.Feriados());
        }

        /// <summary>
        /// Respuesta comun de MarcarEntregable y MarcarCompleta. Sin proceso
        /// (Respuestas 0 y sin avance) es la misma de la Entrega 1.
        /// </summary>
        private static string TrasMarcar(Quien q, EntAgendaActividad a, EntAgendaAvance av)
        {
            if (av.Respuestas == -9) { return Mensaje("0", PasoCerrado, "warning"); }
            if (av.Respuestas == -8) { return Mensaje("0", SinFechaSiguiente, "warning"); }
            if (av.Respuestas != 0) { return Mensaje("0", ProcesoCambio, "warning"); }
            if (!av.IdProcesoPasoActivado.HasValue && !av.IdProcesoTerminado.HasValue)
            {
                return Ok(a.IdActividad, "", "success");
            }

            List<EntAgendaProceso> procesos = NegAgenda.Procesos();
            if (av.IdProcesoPasoActivado.HasValue)
            {
                EntAgendaProcesoPaso paso;
                EntAgendaProceso proceso = BuscarPaso(procesos, av.IdProcesoPasoActivado.Value, out paso);
                if (proceso == null) { return Ok(a.IdActividad, "Paso completo.", "success"); }
                string nombre = q.Nombre(paso.CodResponsable);
                bool? aviso = AvisarPaso(q, proceso, paso);
                if (aviso == null) { return Ok(a.IdActividad, "Paso completo. El paso siguiente quedó en curso.", "success"); }
                return aviso.Value
                    ? Ok(a.IdActividad, "Paso completo. Se le avisó a " + nombre + " para el paso siguiente.", "success")
                    : Ok(a.IdActividad, "Paso completo, pero no se pudo avisar por correo a " + nombre + ".", "warning");
            }

            EntAgendaProceso terminado = procesos.FirstOrDefault(x => x.IdProceso == av.IdProcesoTerminado.Value);
            if (terminado == null) { return Ok(a.IdActividad, "Paso completo: el proceso terminó.", "success"); }
            return AvisarTerminado(q, terminado)
                ? Ok(a.IdActividad, "Paso completo: el proceso terminó.", "success")
                : Ok(a.IdActividad, "Paso completo: el proceso terminó, pero no se pudo avisar por correo a quien lo inició.", "warning");
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
            DateTime? fechaSiguiente = FechaSiguiente(a);
            EntAgendaAvance av = NegAgenda.MarcarCompleta(a.IdActividad, Campo(p, "completa") == "1", q.Yo, fechaSiguiente);
            return TrasMarcar(q, a, av);
        }

        private string Anular(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            EntAgendaActividad a = NegAgenda.Actividad(Entero(Campo(p, "id")));
            // El SQL no lo bloquea: un paso anulado desde la agenda congelaria el proceso.
            if (a != null && a.IdProcesoPaso.HasValue) { return Mensaje("0", EsPaso, "warning"); }
            if (a == null || !NegAgendaActividad.PuedeEditar(q.Yo, a, q.Cadena, q.VerTodos))
            {
                return Mensaje("0", "No puede anular esta actividad.", "warning");
            }
            NegAgenda.Anular(a.IdActividad, q.Yo);
            return Ok(a.IdActividad, "Actividad anulada.", "success");
        }

        // ------------------------------------------------------ correos de procesos

        /// <summary>
        /// «Te toca un paso» al responsable del paso (recien activado o
        /// reasignado). null = no correspondia avisar (el responsable es quien
        /// inicio el proceso); si no, si salio. Como Avisar: nunca deshace nada
        /// y cada intento queda en AG_CorreoBitacora despues de saber si salio.
        /// </summary>
        private static bool? AvisarPaso(Quien q, EntAgendaProceso proceso, EntAgendaProcesoPaso paso)
        {
            if (!NegAgendaCorreo.DebeAvisarPaso(proceso, paso)) { return null; }

            string asunto = "";
            bool enviado = false;
            string error = null;
            EntAgendaPersona responsable = q.Persona(paso.CodResponsable);
            string destinatario = responsable == null || string.IsNullOrWhiteSpace(responsable.Correo) ? null : responsable.Correo.Trim();
            try
            {
                asunto = NegAgendaCorreo.AsuntoPaso(proceso, paso);
                if (responsable == null)
                {
                    error = "El responsable del paso no está activo.";
                }
                else if (destinatario == null)
                {
                    error = "La persona no tiene correo registrado.";
                }
                else
                {
                    DateTime fecha = paso.FechaEntrega ?? DateTime.Today;
                    string cuerpo = NegAgendaCorreo.CuerpoPaso(proceso, paso, responsable.Nombre,
                        q.Nombre(proceso.CodIniciadoPor), fecha,
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

            Registrar(paso.IdActividad, paso.IdActividad.HasValue ? (int?)null : proceso.IdProceso, destinatario, asunto, enviado, error);
            return enviado;
        }

        /// <summary>«Proceso terminado» a quien lo inicio. Devuelve si salio.</summary>
        private static bool AvisarTerminado(Quien q, EntAgendaProceso proceso)
        {
            string asunto = "";
            bool enviado = false;
            string error = null;
            EntAgendaPersona iniciador = q.Persona(proceso.CodIniciadoPor);
            string destinatario = iniciador == null || string.IsNullOrWhiteSpace(iniciador.Correo) ? null : iniciador.Correo.Trim();
            try
            {
                asunto = NegAgendaCorreo.AsuntoTerminado(proceso);
                if (iniciador == null)
                {
                    error = "Quien inició el proceso no está activo.";
                }
                else if (destinatario == null)
                {
                    error = "La persona no tiene correo registrado.";
                }
                else
                {
                    Dictionary<string, string> nombres = new Dictionary<string, string>(NegAgendaEquipo.Codigos);
                    foreach (EntAgendaProcesoPaso s in proceso.Pasos)
                    {
                        string cod = NegAgendaEquipo.Normalizar(s.CodResponsable);
                        if (cod != "" && !nombres.ContainsKey(cod)) { nombres[cod] = q.Nombre(cod); }
                    }
                    string cuerpo = NegAgendaCorreo.CuerpoTerminado(proceso, nombres,
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

            Registrar(null, proceso.IdProceso, destinatario, asunto, enviado, error);
            return enviado;
        }

        private static void Registrar(int? idActividad, int? idProceso, string destinatario, string asunto, bool enviado, string error)
        {
            try
            {
                NegAgenda.RegistrarCorreo(idActividad, destinatario, asunto, enviado, error, idProceso);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Agenda: no se pudo registrar el correo. " + ex);
            }
        }

        // ------------------------------------------------------ procesos

        private string ListarProcesos(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            DateTime hoy = DateTime.Today;
            string estado = Campo(p, "estado");

            List<object> lista = new List<object>();
            foreach (EntAgendaProceso x in NegAgenda.Procesos())
            {
                if (estado != "" && x.Estado != estado) { continue; }
                if (!NegAgendaProceso.PuedeVer(q.Yo, x, q.Cadena, q.VerTodos)) { continue; }
                lista.Add(Proceso(q, x, hoy));
            }

            return Ok(new
            {
                CodUsuario = q.Yo,
                Activo = q.Persona(q.Yo) != null,
                VerTodos = q.VerTodos,
                Hoy = Iso(hoy),
                Areas = NegAgenda.Areas()
                    .Where(a => !a.EsSinArea)
                    .Select(a => new { a.IdArea, a.Nombre, a.Color, a.Activa })
                    .ToList(),
                Procesos = lista
            });
        }

        private static object Proceso(Quien q, EntAgendaProceso x, DateTime hoy)
        {
            bool gestiona = NegAgendaProceso.PuedeGestionar(q.Yo, x, q.VerTodos);
            List<EntAgendaProcesoPaso> pasos = x.Pasos.OrderBy(s => s.Orden).ToList();
            return new
            {
                x.IdProceso,
                x.Nombre,
                x.IdPlantilla,
                x.Plantilla,
                x.Estado,
                x.CodIniciadoPor,
                IniciadoPor = q.Nombre(x.CodIniciadoPor),
                FechaInicio = Iso(x.FechaInicio),
                FechaFin = x.FechaFin.HasValue ? Iso(x.FechaFin.Value) : "",
                x.MotivoCancelacion,
                Resumen = NegAgendaProceso.ResumenAvance(x),
                PasosCompletos = pasos.Count(s => s.Estado == AgendaPasoEstados.Completo),
                PasosTotal = pasos.Count,
                PuedeGestionar = gestiona,
                Pasos = pasos.Select(s => new
                {
                    s.IdProcesoPaso,
                    s.Orden,
                    s.IdArea,
                    s.Area,
                    s.Color,
                    s.Titulo,
                    s.PlazoDias,
                    s.Entregables,
                    s.CodResponsable,
                    Responsable = q.Nombre(s.CodResponsable),
                    s.Estado,
                    FechaActivacion = s.FechaActivacion.HasValue ? Iso(s.FechaActivacion.Value) : "",
                    FechaCompletado = s.FechaCompletado.HasValue ? Iso(s.FechaCompletado.Value) : "",
                    FechaEntrega = s.FechaEntrega.HasValue ? Iso(s.FechaEntrega.Value) : "",
                    s.IdActividad,
                    s.ActividadCompleta,
                    Atrasado = s.Estado == AgendaPasoEstados.EnCurso && s.FechaEntrega.HasValue
                        && !s.ActividadCompleta && s.FechaEntrega.Value.Date < hoy,
                    ResponsableInactivo = NegAgendaProceso.ResponsableInactivo(s, q.Personas)
                }).ToList()
            };
        }

        private string DatosIniciar(HttpContext context)
        {
            Quien q = Cargar(context);
            if (q.Persona(q.Yo) == null) { return Mensaje("0", NoActivo, "warning"); }

            var plantillas = NegAgenda.Plantillas()
                .Where(t => t.Activa && t.Pasos.Count > 0)
                .Select(t => new
                {
                    t.IdPlantilla,
                    t.Nombre,
                    t.Descripcion,
                    Pasos = t.Pasos.OrderBy(s => s.Orden).Select(s => new { s.Orden, s.IdArea, s.Area, s.Titulo, s.PlazoDias, s.Entregables }).ToList()
                })
                .ToList();

            var areas = q.Personas
                .Where(x => !x.SinArea)
                .GroupBy(x => x.IdArea)
                .Select(g => new
                {
                    IdArea = g.Key,
                    Area = g.First().Area,
                    Personas = g.OrderBy(x => x.Nombre).Select(x => new { x.CodUsuario, x.Nombre }).ToList()
                })
                .OrderBy(a => a.Area)
                .ToList();

            return Ok(new { Hoy = Iso(DateTime.Today), Plantillas = plantillas, PersonasPorArea = areas });
        }

        private string IniciarProceso(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            if (q.Persona(q.Yo) == null) { return Mensaje("0", NoActivo, "warning"); }

            int idPlantilla = Entero(Campo(p, "idPlantilla"));
            string nombre = Campo(p, "nombre");
            EntAgendaPlantilla t = NegAgenda.Plantillas().FirstOrDefault(x => x.IdPlantilla == idPlantilla);
            if (t == null) { return Mensaje("0", "La plantilla no está activa.", "warning"); }
            List<EntAgendaPlantillaPaso> delaPlantilla = t.Pasos.OrderBy(s => s.Orden).ToList();
            t.Pasos = delaPlantilla;

            List<EntAgendaProcesoPaso> pasos = new List<EntAgendaProcesoPaso>();
            foreach (object item in Lista(p, "pasos"))
            {
                EntAgendaPersona r = q.Persona(Campo(item, "codResponsable"));
                pasos.Add(new EntAgendaProcesoPaso
                {
                    Orden = Entero(Campo(item, "orden")),
                    CodResponsable = r != null ? r.CodUsuario : NegAgendaEquipo.Normalizar(Campo(item, "codResponsable")),
                    Titulo = Campo(item, "titulo"),
                    PlazoDias = Entero(Campo(item, "plazoDias")),
                    Entregables = Campo(item, "entregables")
                });
            }
            pasos = pasos.OrderBy(s => s.Orden).ToList();
            // El area de cada paso sale de la plantilla, nunca del navegador.
            for (int i = 0; i < pasos.Count && i < delaPlantilla.Count; i++)
            {
                pasos[i].IdArea = delaPlantilla[i].IdArea;
                pasos[i].Area = delaPlantilla[i].Area;
            }

            string error = NegAgendaProceso.ValidarInicio(nombre, t, pasos, q.Personas);
            if (error != null) { return Mensaje("0", error, "warning"); }
            for (int i = 0; i < pasos.Count; i++)
            {
                if (pasos[i].Entregables.Length > 2000)
                {
                    return Mensaje("0", "Paso " + (i + 1) + ": los entregables no pueden pasar de 2000 caracteres.", "warning");
                }
            }

            DateTime fechaPrimero = NegAgendaDiasHabiles.SumarDiasHabiles(DateTime.Today, pasos[0].PlazoDias, NegAgenda.Feriados());
            int? idPasoActivado;
            int idProceso = NegAgenda.IniciarProceso(nombre, idPlantilla, pasos, fechaPrimero, q.Yo, out idPasoActivado);
            if (idProceso == -3) { return Mensaje("0", "La plantilla no está activa.", "warning"); }
            if (idProceso <= 0)
            {
                return Mensaje("0", "No se pudo iniciar el proceso: algún paso tiene un área inactiva o datos no válidos. Revise la plantilla.", "warning");
            }

            if (!idPasoActivado.HasValue) { return Ok(idProceso, "Proceso iniciado.", "success"); }
            EntAgendaProcesoPaso activado;
            EntAgendaProceso proceso = BuscarPaso(NegAgenda.Procesos(), idPasoActivado.Value, out activado);
            if (proceso == null) { return Ok(idProceso, "Proceso iniciado.", "success"); }
            string nombreResp = q.Nombre(activado.CodResponsable);
            bool? aviso = AvisarPaso(q, proceso, activado);
            if (aviso == null) { return Ok(idProceso, "Proceso iniciado. El primer paso quedó en su agenda.", "success"); }
            return aviso.Value
                ? Ok(idProceso, "Proceso iniciado. Se le avisó a " + nombreResp + " del primer paso.", "success")
                : Ok(idProceso, "Proceso iniciado, pero no se pudo avisar por correo a " + nombreResp + ".", "warning");
        }

        /// <summary>
        /// Carga el paso pedido y su proceso, y comprueba que quien pregunta
        /// lo puede gestionar. Devuelve el mensaje de rechazo o null.
        /// </summary>
        private static string PasoGestionable(Quien q, int idPaso, out EntAgendaProceso proceso, out EntAgendaProcesoPaso paso)
        {
            proceso = BuscarPaso(NegAgenda.Procesos(), idPaso, out paso);
            if (proceso == null || paso == null) { return ProcesoCambio; }
            if (proceso.Estado != AgendaProcesoEstados.EnCurso) { return ProcesoCambio; }
            if (!NegAgendaProceso.PuedeGestionar(q.Yo, proceso, q.VerTodos)) { return NoGestiona; }
            return null;
        }

        private string ReasignarPaso(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            EntAgendaProceso proceso;
            EntAgendaProcesoPaso paso;
            string error = PasoGestionable(q, Entero(Campo(p, "idPaso")), out proceso, out paso);
            if (error != null) { return Mensaje("0", error, "warning"); }

            string esperado = Campo(p, "estado");
            if (esperado != "" && esperado != paso.Estado) { return Mensaje("0", ProcesoCambio, "warning"); }

            EntAgendaPersona nuevo = q.Persona(Campo(p, "codResponsable"));
            error = NegAgendaProceso.PuedeReasignar(paso, nuevo != null ? nuevo.CodUsuario : "", q.Personas);
            if (error != null) { return Mensaje("0", error, "warning"); }
            if (NegAgendaEquipo.Codigos.Equals(NegAgendaEquipo.Normalizar(paso.CodResponsable), NegAgendaEquipo.Normalizar(nuevo.CodUsuario)))
            {
                return Mensaje("0", "Esa persona ya es la responsable del paso.", "warning");
            }

            string estadoAntes = paso.Estado;
            int r = NegAgenda.ReasignarPaso(paso.IdProcesoPaso, estadoAntes, nuevo.CodUsuario, q.Yo);
            if (r == -1) { return Mensaje("0", ProcesoCambio, "warning"); }
            if (r != 0) { return Mensaje("0", "Elija a la persona responsable.", "warning"); }

            if (estadoAntes != AgendaPasoEstados.EnCurso)
            {
                return Ok(paso.IdProcesoPaso, "Paso reasignado a " + nuevo.Nombre + ".", "success");
            }

            EntAgendaProcesoPaso actual;
            EntAgendaProceso recargado = BuscarPaso(NegAgenda.Procesos(), paso.IdProcesoPaso, out actual);
            bool? aviso = recargado == null ? null : AvisarPaso(q, recargado, actual);
            if (aviso == null || aviso.Value)
            {
                return Ok(paso.IdProcesoPaso, "Paso reasignado a " + nuevo.Nombre + (aviso == true ? "; se le avisó por correo." : "."), "success");
            }
            return Ok(paso.IdProcesoPaso, "Paso reasignado a " + nuevo.Nombre + ", pero no se le pudo avisar por correo.", "warning");
        }

        private string CambiarPasoPendiente(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            EntAgendaProceso proceso;
            EntAgendaProcesoPaso paso;
            string error = PasoGestionable(q, Entero(Campo(p, "idPaso")), out proceso, out paso);
            if (error != null) { return Mensaje("0", error, "warning"); }
            if (paso.Estado != AgendaPasoEstados.Pendiente) { return Mensaje("0", ProcesoCambio, "warning"); }

            string titulo = Campo(p, "titulo");
            int plazo = Entero(Campo(p, "plazoDias"));
            string entregables = Campo(p, "entregables");
            error = NegAgendaProceso.PuedeCambiarPlazo(paso, plazo);
            if (error != null) { return Mensaje("0", error, "warning"); }
            if (titulo.Length < 1 || titulo.Length > 200) { return Mensaje("0", "El título es obligatorio (hasta 200 caracteres).", "warning"); }
            if (entregables.Length > 2000) { return Mensaje("0", "Los entregables no pueden pasar de 2000 caracteres.", "warning"); }

            int r = NegAgenda.CambiarPasoPendiente(new EntAgendaProcesoPaso
            {
                IdProcesoPaso = paso.IdProcesoPaso,
                Titulo = titulo,
                PlazoDias = plazo,
                Entregables = entregables
            }, q.Yo);
            if (r == -1) { return Mensaje("0", ProcesoCambio, "warning"); }
            if (r != 0) { return Mensaje("0", "Revise el título y el plazo (1 a 60 días hábiles).", "warning"); }
            return Ok(paso.IdProcesoPaso, "Paso actualizado.", "success");
        }

        private string CambiarFechaPaso(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            EntAgendaProceso proceso;
            EntAgendaProcesoPaso paso;
            string error = PasoGestionable(q, Entero(Campo(p, "idPaso")), out proceso, out paso);
            if (error != null) { return Mensaje("0", error, "warning"); }
            if (paso.Estado != AgendaPasoEstados.EnCurso) { return Mensaje("0", ProcesoCambio, "warning"); }

            DateTime? fecha = LeerFecha(Campo(p, "fecha"));
            if (!fecha.HasValue) { return Mensaje("0", "Elija una fecha de entrega.", "warning"); }
            error = NegAgendaProceso.PuedeCambiarFecha(paso, fecha.Value, DateTime.Today);
            if (error != null) { return Mensaje("0", error, "warning"); }

            int r = NegAgenda.CambiarFechaPaso(paso.IdProcesoPaso, fecha.Value, q.Yo);
            if (r == -1) { return Mensaje("0", ProcesoCambio, "warning"); }
            if (r != 0) { return Mensaje("0", "Elija una fecha de entrega.", "warning"); }
            return Ok(paso.IdProcesoPaso, "Fecha de entrega actualizada.", "success");
        }

        private string CancelarProceso(HttpContext context, object p)
        {
            Quien q = Cargar(context);
            int id = Entero(Campo(p, "idProceso"));
            EntAgendaProceso proceso = NegAgenda.Procesos().FirstOrDefault(x => x.IdProceso == id);
            if (proceso == null || proceso.Estado != AgendaProcesoEstados.EnCurso) { return Mensaje("0", ProcesoCambio, "warning"); }
            if (!NegAgendaProceso.PuedeGestionar(q.Yo, proceso, q.VerTodos)) { return Mensaje("0", NoGestiona, "warning"); }

            string motivo = Campo(p, "motivo");
            string error = NegAgendaProceso.PuedeCancelar(proceso, motivo);
            if (error != null) { return Mensaje("0", error, "warning"); }

            int r = NegAgenda.CancelarProceso(id, motivo, q.Yo);
            if (r != 0) { return Mensaje("0", ProcesoCambio, "warning"); }
            return Ok(id, "Proceso cancelado.", "success");
        }

        // ------------------------------------------------------ plantillas

        private string ListarPlantillas()
        {
            return Ok(new
            {
                Plantillas = NegAgenda.Plantillas().Select(t => new
                {
                    t.IdPlantilla,
                    t.Nombre,
                    t.Descripcion,
                    t.Activa,
                    t.TieneProcesos,
                    Pasos = t.Pasos.OrderBy(s => s.Orden)
                        .Select(s => new { s.IdPlantillaPaso, s.Orden, s.IdArea, s.Area, s.Titulo, s.PlazoDias, s.Entregables })
                        .ToList()
                }).ToList(),
                Areas = NegAgenda.Areas()
                    .Where(a => !a.EsSinArea)
                    .Select(a => new { a.IdArea, a.Nombre, a.Color, a.Activa })
                    .ToList()
            });
        }

        private string GuardarPlantilla(HttpContext context, object p)
        {
            EntAgendaPlantilla t = new EntAgendaPlantilla
            {
                IdPlantilla = Entero(Campo(p, "id")),
                Nombre = Campo(p, "nombre"),
                Descripcion = Campo(p, "descripcion"),
                Activa = Si(Campo(p, "activa"))
            };
            int orden = 0;
            foreach (object item in Lista(p, "pasos"))
            {
                orden++;
                t.Pasos.Add(new EntAgendaPlantillaPaso
                {
                    // El orden es el de la lista que manda la pantalla.
                    Orden = orden,
                    IdArea = Entero(Campo(item, "idArea")),
                    Titulo = Campo(item, "titulo"),
                    PlazoDias = Entero(Campo(item, "plazoDias")),
                    Entregables = Campo(item, "entregables")
                });
            }

            HashSet<int> activas = new HashSet<int>(NegAgenda.Areas().Where(a => a.Activa && !a.EsSinArea).Select(a => a.IdArea));
            string error = NegAgendaProceso.ValidarPlantilla(t, activas);
            if (error != null) { return Mensaje("0", error, "warning"); }
            if (t.Descripcion.Length > 500) { return Mensaje("0", "La descripción no puede pasar de 500 caracteres.", "warning"); }
            for (int i = 0; i < t.Pasos.Count; i++)
            {
                if (t.Pasos[i].Entregables.Length > 2000)
                {
                    return Mensaje("0", "Paso " + (i + 1) + ": los entregables no pueden pasar de 2000 caracteres.", "warning");
                }
            }

            int r = NegAgenda.GuardarPlantilla(t, Usuario(context));
            if (r == -1) { return Mensaje("0", "Ya existe una plantilla con ese nombre.", "warning"); }
            if (r == -3) { return Mensaje("0", "La plantilla no existe; recargue la pantalla.", "warning"); }
            if (r <= 0) { return Mensaje("0", "Revise los pasos: cada uno necesita un área activa, un título y un plazo de 1 a 60 días hábiles.", "warning"); }
            return Ok(r, "Plantilla guardada.", "success");
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

        /// <summary>Los elementos de un arreglo JSON de la peticion; vacio si no viene o no es arreglo.</summary>
        private static List<object> Lista(object p, string nombre)
        {
            List<object> lista = new List<object>();
            IDictionary<string, object> d = p as IDictionary<string, object>;
            object valor;
            if (d == null || !d.TryGetValue(nombre, out valor) || valor == null || valor is string) { return lista; }
            IEnumerable items = valor as IEnumerable;
            if (items == null) { return lista; }
            foreach (object item in items) { lista.Add(item); }
            return lista;
        }

        /// <summary>"1" o "true" (en cualquier caja).</summary>
        private static bool Si(string texto)
        {
            return texto == "1" || string.Equals(texto, "true", StringComparison.OrdinalIgnoreCase);
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
