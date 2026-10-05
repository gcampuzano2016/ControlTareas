using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CapaNegocio
{
    /// <summary>
    /// Reglas del modulo Curva de Aprendizaje (especificacion, seccion 7):
    /// alta, avance de etapa, completar/editar/eliminar evaluaciones, plan de
    /// accion, enlace remoto y dashboard.
    ///
    /// Los errores de regla de negocio salen como CurvaReglaException con el
    /// texto EXACTO que ve el usuario (los de la especificacion). Todo lo demas
    /// es un error tecnico y el handler lo muestra con un mensaje generico.
    ///
    /// Las llamadas a la IA van SIEMPRE fuera de la transaccion: son lentas y
    /// no pueden dejar filas bloqueadas, y nunca hacen fallar un guardado.
    ///
    /// Los perfiles NO se comprueban aqui: la barrera esta en los handlers.
    /// </summary>
    public static class NegCurvaOnboarding
    {
        /* ===================================================== colaboradores */

        public static List<EntCurvaEmpleado> ListarColaboradores()
        {
            using (var u = DaoCurvaUnidad.Abrir(false)) return DaoCurva.ListarEmpleados(u);
        }

        public static List<EntCurvaEmpleadoSistema> BuscarEmpleadoSistema(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto) || texto.Trim().Length < 3) return new List<EntCurvaEmpleadoSistema>();
            using (var u = DaoCurvaUnidad.Abrir(false)) return DaoCurva.BuscarEmpleadoSistema(u, texto.Trim());
        }

        /// <summary>
        /// Alta (seccion 7.1): empleado + proceso en_curso + SOLO las
        /// evaluaciones de la etapa 30, en una transaccion. Devuelve el id.
        /// </summary>
        public static int CrearColaborador(EntCurvaEmpleado e, string usuario)
        {
            e.NombreCompleto = Limpio(e.NombreCompleto);
            if (e.NombreCompleto == null || e.FechaIngreso == default(DateTime))
                throw new CurvaReglaException("Nombre completo y fecha de ingreso son obligatorios");
            e.Cargo = Limpio(e.Cargo);
            e.Area = Limpio(e.Area);
            e.Empresa = Limpio(e.Empresa);
            e.UnidadNegocio = Limpio(e.UnidadNegocio);

            using (var u = DaoCurvaUnidad.Abrir(true))
            {
                int id = DaoCurva.CrearEmpleado(u, e, usuario);
                int procesoId = DaoCurva.CrearProceso(u, id, e.FechaIngreso);
                var proceso = DaoCurva.ProcesoPorId(u, procesoId);
                var primera = DaoCurva.Etapas(u).OrderBy(x => x.Orden).FirstOrDefault();
                if (primera == null) throw new InvalidOperationException("El catálogo de etapas está vacío: falta la siembra.");
                GenerarEvaluacionesDeEtapa(u, proceso, primera);
                u.Confirmar();
                return id;
            }
        }

        /// <summary>Solo cambia el estado del colaborador: no cierra el proceso ni cancela evaluaciones (7.10).</summary>
        public static void CambiarEstado(int id, string estado)
        {
            if (estado != "activo" && estado != "inactivo") throw new CurvaReglaException("Estado no válido.");
            using (var u = DaoCurvaUnidad.Abrir(false))
            {
                if (DaoCurva.CambiarEstadoEmpleado(u, id, estado) == 0) throw new CurvaReglaException("Empleado no encontrado");
            }
        }

        public static void EliminarColaborador(int id)
        {
            using (var u = DaoCurvaUnidad.Abrir(false))
            {
                if (DaoCurva.EliminarEmpleado(u, id) == 0) throw new CurvaReglaException("Empleado no encontrado");
            }
        }

        /* ============================================================= ficha */

        public static EntCurvaFicha Ficha(int empleadoId)
        {
            using (var u = DaoCurvaUnidad.Abrir(false))
            {
                var emp = DaoCurva.EmpleadoPorId(u, empleadoId);
                if (emp == null) throw new CurvaReglaException("Empleado no encontrado");
                var ficha = new EntCurvaFicha
                {
                    Empleado = emp,
                    Evaluaciones = new List<EntCurvaTarjeta>(),
                    Compromisos = new List<EntCurvaCompromiso>(),
                    Curva = new EntCurvaSerie(),
                    Alerta = new EntCurvaAlerta(),
                    AlertaPorEtapa = new List<EntCurvaAlertaEtapa>(),
                    Competencias = NegCurvaCurva.CompetenciasJefe(new EntCurvaInsumoEvaluacion[0]),
                    Etapas = DaoCurva.Etapas(u)
                };
                if (!emp.ProcesoId.HasValue) return ficha;

                var x = Expediente(u, emp.ProcesoId.Value, emp);
                ficha.Proceso = x.Proceso;
                ficha.Proceso.ResumenEjecutivoAnual = null;   // la ficha no lo muestra; va en el PDF anual
                ficha.Compromisos = x.Compromisos;
                ficha.Curva = x.Serie;
                ficha.Alerta = NegCurvaCurva.CalcularAlerta(x.Serie);
                ficha.AlertaPorEtapa = NegCurvaCurva.AlertaPorEtapa(x.Serie);
                ficha.Competencias = NegCurvaCurva.CompetenciasJefe(x.Completadas);
                foreach (var ev in x.Evaluaciones)
                {
                    int? indice = ev.EstaCompletada ? NegCurvaCurva.IndiceDe(x.Serie, ev.EtapaCodigo, ev.EvaluadorRol) : null;
                    string nivel = indice.HasValue ? NegCurvaCurva.Nivel(indice.Value) : null;
                    ficha.Evaluaciones.Add(new EntCurvaTarjeta
                    {
                        Id = ev.Id,
                        EtapaCodigo = ev.EtapaCodigo,
                        EtapaNombre = ev.EtapaNombre,
                        Rol = ev.EvaluadorRol,
                        Estado = ev.Estado,
                        FechaObjetivo = ev.FechaObjetivo,
                        FechaCompletada = ev.FechaCompletada,
                        Indice = indice,
                        Nivel = nivel,
                        AvisoPlanAccion = ev.EvaluadorRol == CurvaConstantes.RolJefe && nivel != null && nivel != CurvaConstantes.NivelOk,
                        MostrarPlanAccion = nivel == CurvaConstantes.NivelSeguimiento,
                        PlanAccionGth = ev.PlanAccionGth,
                        Conclusion = ev.Conclusion,
                        TieneEnlace = !string.IsNullOrEmpty(ev.Token) && !ev.EstaCompletada
                    });
                }
                return ficha;
            }
        }

        /// <summary>Todo el proceso leido de una vez (ficha, PDF, prompts de IA).</summary>
        public static EntCurvaExpediente Expediente(int procesoId)
        {
            using (var u = DaoCurvaUnidad.Abrir(false))
            {
                var p = DaoCurva.ProcesoPorId(u, procesoId);
                if (p == null) return null;
                return Expediente(u, procesoId, DaoCurva.EmpleadoPorId(u, p.EmpleadoId));
            }
        }

        private static EntCurvaExpediente Expediente(DaoCurvaUnidad u, int procesoId, EntCurvaEmpleado emp)
        {
            var x = new EntCurvaExpediente
            {
                Empleado = emp,
                Proceso = DaoCurva.ProcesoPorId(u, procesoId),
                Evaluaciones = DaoCurva.EvaluacionesPorProceso(u, procesoId),
                Respuestas = DaoCurva.RespuestasPorProceso(u, procesoId),
                Compromisos = DaoCurva.CompromisosPorProceso(u, procesoId),
                Seguimientos = DaoCurva.SeguimientosPorProceso(u, procesoId),
                Decisiones = DaoCurva.DecisionesPorProceso(u, procesoId)
            };
            x.Completadas = x.Evaluaciones.Where(e => e.EstaCompletada).Select(e =>
            {
                var ins = new EntCurvaInsumoEvaluacion { EvaluacionId = e.Id, Rol = e.EvaluadorRol, EtapaCodigo = e.EtapaCodigo };
                ins.Respuestas.AddRange(x.Respuestas.Where(r => r.EvaluacionId == e.Id));
                ins.EstadosSeguimiento.AddRange(x.Seguimientos.Where(s => s.EvaluacionId == e.Id).Select(s => s.Estado));
                return ins;
            }).ToList();
            x.Serie = NegCurvaCurva.Calcular(x.Completadas);
            return x;
        }

        /* ======================================================== formulario */

        /// <summary>
        /// Los datos para pintar el formulario. paraRemota = true arma SOLO lo
        /// que la pagina publica puede ver (seccion 11.3).
        /// </summary>
        public static EntCurvaFormularioDatos CargarFormulario(int evaluacionId, bool paraRemota)
        {
            using (var u = DaoCurvaUnidad.Abrir(false))
            {
                var ev = DaoCurva.EvaluacionPorId(u, evaluacionId);
                if (ev == null) throw new CurvaReglaException("Evaluación no encontrada");
                return ArmarFormulario(u, ev, paraRemota);
            }
        }

        private static EntCurvaFormularioDatos ArmarFormulario(DaoCurvaUnidad u, EntCurvaEvaluacion ev, bool paraRemota)
        {
            var emp = DaoCurva.EmpleadoPorId(u, ev.EmpleadoId);
            var datos = new EntCurvaFormularioDatos
            {
                EvaluacionId = ev.Id,
                EmpleadoId = paraRemota ? 0 : ev.EmpleadoId,
                EmpleadoNombre = emp == null ? "" : emp.NombreCompleto,
                EmpleadoCargo = emp == null ? null : emp.Cargo,
                EtapaCodigo = ev.EtapaCodigo,
                EtapaNombre = ev.EtapaNombre,
                Rol = ev.EvaluadorRol,
                Estado = ev.Estado,
                Preguntas = new List<EntCurvaPreguntaFormulario>(),
                Respuestas = new Dictionary<int, object>(),
                Abiertos = new List<EntCurvaAbierto>(),
                Categorias = CurvaConstantes.CategoriasCompromiso
            };
            // Ya completada en la pagina remota: no se carga nada mas.
            if (paraRemota && ev.EstaCompletada) return datos;

            foreach (var p in DaoCurva.Preguntas(u, ev.EtapaId, ev.EvaluadorRol))
                datos.Preguntas.Add(new EntCurvaPreguntaFormulario
                {
                    Id = p.Id, Seccion = p.Seccion, Texto = p.TextoPregunta, Tipo = p.TipoRespuesta,
                    Opciones = NegCurvaValorRespuesta.Opciones(p.OpcionesJson)
                });

            if (!paraRemota)
                foreach (var r in DaoCurva.RespuestasPorEvaluacion(u, ev.Id))
                    datos.Respuestas[r.PreguntaId] = NegCurvaValorRespuesta.Parsear(r.ValorRespuesta);

            foreach (var c in DaoCurva.CompromisosAbiertos(u, ev.Id))
                datos.Abiertos.Add(new EntCurvaAbierto
                {
                    Id = c.Id, Descripcion = c.Descripcion, Categoria = c.Categoria, Responsable = c.Responsable,
                    EtapaCodigo = c.EtapaCodigo, FechaLimite = c.FechaLimite,
                    EstadoActual = paraRemota ? null : c.EstadoActual,
                    JustificacionActual = paraRemota ? null : c.JustificacionActual
                });
            return datos;
        }

        /* ================================================ completar / editar */

        /// <summary>Completar (seccion 7.3). Devuelve el id del colaborador.</summary>
        public static Task<int> CompletarAsync(int evaluacionId, string payloadJson)
        {
            return GuardarAsync(evaluacionId, payloadJson, false);
        }

        /// <summary>Editar una evaluacion ya completada (seccion 7.8). Devuelve el id del colaborador.</summary>
        public static Task<int> EditarAsync(int evaluacionId, string payloadJson)
        {
            return GuardarAsync(evaluacionId, payloadJson, true);
        }

        private static async Task<int> GuardarAsync(int evaluacionId, string payloadJson, bool editar)
        {
            EntCurvaEvaluacion ev;
            using (var u = DaoCurvaUnidad.Abrir(true))
            {
                ev = DaoCurva.EvaluacionPorId(u, evaluacionId);
                if (ev == null) throw new CurvaReglaException("Evaluación no encontrada");
                if (!editar && ev.EstaCompletada) throw new CurvaReglaException("Esta evaluación ya fue completada");
                if (editar && !ev.EstaCompletada) throw new CurvaReglaException("Solo se puede editar una evaluación ya completada");

                var preguntas = DaoCurva.Preguntas(u, ev.EtapaId, ev.EvaluadorRol);
                EntCurvaGuardado g;
                try { g = NegCurvaFormulario.Normalizar(preguntas, payloadJson); }
                catch (ArgumentException ex) { throw new CurvaReglaException(ex.Message); }

                foreach (var kv in g.Valores) DaoCurva.GuardarRespuesta(u, ev.Id, kv.Key, kv.Value);

                if (g.Compromisos != null)
                {
                    if (!editar)
                    {
                        foreach (var f in g.Compromisos) DaoCurva.CrearCompromiso(u, ev.Id, f);
                    }
                    else
                    {
                        /* Se borran los que no tienen seguimiento y se insertan los
                           enviados. Los que ya tienen seguimiento se conservan, y
                           como el formulario los vuelve a enviar (vienen de la
                           respuesta guardada), una fila igual a uno conservado no
                           se inserta otra vez: si no, cada edicion lo duplicaria. */
                        var conservados = DaoCurva.CompromisosPorProceso(u, ev.ProcesoId)
                            .Where(c => c.EvaluacionId == ev.Id && c.NSeguimientos > 0).ToList();
                        DaoCurva.BorrarCompromisosSinSeguimiento(u, ev.Id);
                        foreach (var f in g.Compromisos)
                        {
                            var igual = conservados.FirstOrDefault(c => string.Equals((c.Descripcion ?? "").Trim(), f.Descripcion, StringComparison.Ordinal));
                            if (igual != null) { conservados.Remove(igual); continue; }
                            DaoCurva.CrearCompromiso(u, ev.Id, f);
                        }
                    }
                }

                foreach (var s in g.Seguimientos) DaoCurva.GuardarSeguimiento(u, ev.Id, s);

                if (!editar)
                {
                    if (!DaoCurva.MarcarCompletada(u, ev.Id, g.Conclusion))
                        throw new CurvaReglaException("Esta evaluación ya fue completada");
                }
                else
                {
                    DaoCurva.GuardarConclusion(u, ev.Id, g.Conclusion);
                }

                if (g.Decision != null)
                {
                    DaoCurva.GuardarDecision(u, ev.ProcesoId, ev.Id, g.Decision);
                    DaoCurva.CambiarEstadoProceso(u, ev.ProcesoId, EstadoPorDecision(g.Decision.Decision));
                }

                AvanzarProceso(u, ev.Id);
                u.Confirmar();
            }

            // Fuera de la transaccion: la IA es lenta y nunca hace fallar el guardado.
            await ResumenesSinFallarAsync(ev.Id, ev.ProcesoId).ConfigureAwait(false);
            return ev.EmpleadoId;
        }

        /// <summary>
        /// Avance del proceso (seccion 7.2). Idempotente. Si todas las
        /// evaluaciones de la etapa estan completas, genera la siguiente; en
        /// 365 cierra el proceso. Un proceso rechazado no avanza.
        /// </summary>
        private static void AvanzarProceso(DaoCurvaUnidad u, int evaluacionId)
        {
            var ev = DaoCurva.EvaluacionPorId(u, evaluacionId);
            var proceso = DaoCurva.ProcesoPorId(u, ev.ProcesoId);
            if (proceso.Estado == CurvaConstantes.ProcesoRechazado) return;

            var deLaEtapa = DaoCurva.EvaluacionesPorProceso(u, proceso.Id).Where(e => e.EtapaId == ev.EtapaId);
            if (!deLaEtapa.All(e => e.EstaCompletada)) return;

            if (ev.EtapaCodigo == CurvaConstantes.EtapaFinal)
            {
                if (proceso.Estado == CurvaConstantes.ProcesoAprobado || proceso.Estado == CurvaConstantes.ProcesoExtendido
                    || proceso.Estado == CurvaConstantes.ProcesoEnCurso)
                    DaoCurva.CambiarEstadoProceso(u, proceso.Id, CurvaConstantes.ProcesoCompletado);
                return;
            }

            var siguiente = DaoCurva.Etapas(u).Where(e => e.Orden > ev.EtapaOrden).OrderBy(e => e.Orden).FirstOrDefault();
            if (siguiente == null) return;
            GenerarEvaluacionesDeEtapa(u, proceso, siguiente);
        }

        /// <summary>Crea las evaluaciones de los roles de la etapa que falten. fecha_objetivo = inicio + dias.</summary>
        private static void GenerarEvaluacionesDeEtapa(DaoCurvaUnidad u, EntCurvaProceso proceso, EntCurvaEtapa etapa)
        {
            string[] roles;
            if (!CurvaConstantes.RolesPorEtapa.TryGetValue(etapa.Codigo, out roles)) return;
            DateTime objetivo = proceso.FechaInicio.Date.AddDays(etapa.DiasOffset);
            foreach (var rol in roles) DaoCurva.CrearEvaluacion(u, proceso.Id, etapa.Id, rol, objetivo);
        }

        public static string EstadoPorDecision(string d)
        {
            if (d == CurvaConstantes.ProcesoRechazado) return CurvaConstantes.ProcesoRechazado;
            if (d == CurvaConstantes.ProcesoExtendido) return CurvaConstantes.ProcesoExtendido;
            return CurvaConstantes.ProcesoAprobado;
        }

        /* =================================================== eliminar / plan */

        /// <summary>Elimina una evaluacion (pendiente o completada) y revierte sus efectos (7.8). Devuelve el id del colaborador.</summary>
        public static int EliminarEvaluacion(int evaluacionId)
        {
            using (var u = DaoCurvaUnidad.Abrir(false))
            {
                var ev = DaoCurva.EvaluacionPorId(u, evaluacionId);
                if (ev == null) throw new CurvaReglaException("Evaluación no encontrada");
                int r = DaoCurva.EliminarEvaluacion(u, evaluacionId);
                if (r == -1) throw new CurvaReglaException("Evaluación no encontrada");
                if (r == -2) throw new CurvaReglaException("No se puede eliminar: el proceso ya avanzó a una etapa posterior. Usa \"Editar\" para corregirla.");
                if (r == -3) throw new CurvaReglaException("No se puede eliminar: algunos compromisos creados aquí ya tienen seguimiento en otra evaluación.");
                return ev.EmpleadoId;
            }
        }

        /// <summary>
        /// Plan de accion GTH. Solo exige que la evaluacion este completada; el
        /// nivel NO se valida a proposito (seccion 7.7). Regenera el resumen anual.
        /// </summary>
        public static async Task GuardarPlanAccionAsync(int evaluacionId, string plan)
        {
            int procesoId;
            using (var u = DaoCurvaUnidad.Abrir(false))
            {
                var ev = DaoCurva.EvaluacionPorId(u, evaluacionId);
                if (ev == null) throw new CurvaReglaException("Evaluación no encontrada");
                if (!ev.EstaCompletada) throw new CurvaReglaException("Solo se puede registrar un plan de acción sobre una evaluación completada");
                var t = (plan ?? "").Trim();
                DaoCurva.GuardarPlanAccion(u, evaluacionId, t.Length > 0 ? t : null);
                procesoId = ev.ProcesoId;
            }
            await ResumenesSinFallarAsync(null, procesoId).ConfigureAwait(false);
        }

        /* ===================================================== enlace remoto */

        /// <summary>Token nuevo (24 bytes criptograficos -&gt; 48 hex). Regenerar invalida el anterior.</summary>
        public static string GenerarEnlace(int evaluacionId)
        {
            using (var u = DaoCurvaUnidad.Abrir(false))
            {
                var ev = DaoCurva.EvaluacionPorId(u, evaluacionId);
                if (ev == null) throw new CurvaReglaException("Evaluación no encontrada");
                if (ev.EstaCompletada) throw new CurvaReglaException("Esta evaluación ya fue completada, no se puede generar un enlace nuevo");
                var token = NuevoToken();
                if (!DaoCurva.GuardarToken(u, evaluacionId, token))
                    throw new CurvaReglaException("Esta evaluación ya fue completada, no se puede generar un enlace nuevo");
                return token;
            }
        }

        public static string NuevoToken()
        {
            var b = new byte[24];
            using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(b);
            return BitConverter.ToString(b).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// La pagina remota: busca por token. null = token inexistente. Si ya
        /// esta completada devuelve solo el estado (sin preguntas ni respuestas).
        /// </summary>
        public static EntCurvaFormularioDatos CargarRemota(string token)
        {
            if (!TokenValido(token)) return null;
            using (var u = DaoCurvaUnidad.Abrir(false))
            {
                var ev = DaoCurva.EvaluacionPorToken(u, token);
                return ev == null ? null : ArmarFormulario(u, ev, true);
            }
        }

        /// <summary>Envio desde la pagina remota: misma logica que completar, buscando por token.</summary>
        public static async Task CompletarRemotaAsync(string token, string payloadJson)
        {
            if (!TokenValido(token)) throw new CurvaReglaException("Este enlace ya no está disponible. Pide a Talento Humano que te genere uno nuevo.");
            int id;
            using (var u = DaoCurvaUnidad.Abrir(false))
            {
                var ev = DaoCurva.EvaluacionPorToken(u, token);
                if (ev == null) throw new CurvaReglaException("Este enlace ya no está disponible. Pide a Talento Humano que te genere uno nuevo.");
                if (ev.EstaCompletada) throw new CurvaReglaException("Esta evaluación ya fue completada");
                id = ev.Id;
            }
            await CompletarAsync(id, payloadJson).ConfigureAwait(false);
        }

        private static bool TokenValido(string token)
        {
            return token != null && token.Length == 48 && token.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'));
        }

        /* ================================================================ IA */

        /// <summary>
        /// Los resumenes que siguen a un guardado YA confirmado. Un fallo aqui
        /// (base caida entre medio, por ejemplo) no puede convertir en error un
        /// guardado que si quedo hecho: se traga y queda el resumen anterior.
        /// </summary>
        private static async Task ResumenesSinFallarAsync(int? evaluacionId, int procesoId)
        {
            try
            {
                if (evaluacionId.HasValue) await RegenerarResumenEvaluacionAsync(evaluacionId.Value, procesoId).ConfigureAwait(false);
                await RegenerarResumenAnualAsync(procesoId).ConfigureAwait(false);
            }
            catch (Exception) { /* el guardado ya esta confirmado */ }
        }

        private static async Task RegenerarResumenEvaluacionAsync(int evaluacionId, int procesoId)
        {
            var x = Expediente(procesoId);
            var ev = x.Evaluaciones.First(e => e.Id == evaluacionId);
            string resumen = await NegCurvaResumenIA.GenerarAsync(NegCurvaResumenIA.SistemaIndividual,
                NegCurvaResumenIA.PromptIndividual(x, ev), NegCurvaResumenIA.ErrorIndividual).ConfigureAwait(false);
            using (var u = DaoCurvaUnidad.Abrir(false)) DaoCurva.GuardarResumenEvaluacion(u, evaluacionId, resumen);
        }

        /// <summary>Regenera y guarda el resumen anual; solo si hay al menos una evaluacion completada.</summary>
        public static async Task<string> RegenerarResumenAnualAsync(int procesoId)
        {
            var x = Expediente(procesoId);
            if (x == null || !x.Evaluaciones.Any(e => e.EstaCompletada)) return null;
            string resumen = await NegCurvaResumenIA.GenerarAsync(NegCurvaResumenIA.SistemaAnual,
                NegCurvaResumenIA.PromptConsolidado(x, null), NegCurvaResumenIA.ErrorAnual).ConfigureAwait(false);
            using (var u = DaoCurvaUnidad.Abrir(false)) DaoCurva.GuardarResumenAnual(u, procesoId, resumen);
            return resumen;
        }

        /// <summary>Resumen por rango: se genera en cada descarga y no se guarda.</summary>
        public static Task<string> ResumenRangoAsync(EntCurvaExpediente x, ICollection<string> etapas)
        {
            return NegCurvaResumenIA.GenerarAsync(NegCurvaResumenIA.SistemaRango,
                NegCurvaResumenIA.PromptConsolidado(x, etapas), NegCurvaResumenIA.ErrorRango);
        }

        /* ========================================================= dashboard */

        public static EntCurvaDashboard Dashboard()
        {
            using (var u = DaoCurvaUnidad.Abrir(false)) return NegCurvaDashboard.Agregar(DaoCurva.Dashboard(u));
        }

        private static string Limpio(string s)
        {
            if (s == null) return null;
            s = s.Trim();
            return s.Length == 0 ? null : s;
        }
    }

    /// <summary>Una regla de negocio no se cumple; Message es el texto exacto para el usuario.</summary>
    public class CurvaReglaException : Exception
    {
        public CurvaReglaException(string mensaje) : base(mensaje) { }
    }
}
