using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>Acciones de Talento Humano: revisar, registrar directo, catalogo, abrir y cerrar el mes.</summary>
    public static partial class NegBono
    {
        private static readonly string[] Familias = { "Standby", "Contrato", "Otro" };

        /// <summary>
        /// accion = Aprobar | Ajustar | Rechazar sobre un bono del mes abierto.
        /// Aprobar y Ajustar dejan Aprobado: vuelven a exigir identificacion y a
        /// chequear doble pago, y toman la copia de empresa, nombre e
        /// identificacion. Ajustar exige valor y motivo; Rechazar, motivo, y
        /// avisa al jefe por correo. <paramref name="valor"/> solo cuenta al ajustar.
        /// </summary>
        public static EntBonoResultado Revisar(int idBono, string estadoEsperado, string accion, decimal? valor, string motivo,
            string yo, Func<string, string, string, bool> enviarCorreo, string urlProponer)
        {
            yo = NegBonoReglas.Normalizar(yo);
            accion = (accion ?? "").Trim();
            motivo = (motivo ?? "").Trim();

            EntBonoPeriodo periodo = Abierto(LeerPeriodos());
            if (periodo == null) return Error(MsgSinMes);

            EntBono actual = BonoVivo(idBono);
            if (actual == null || actual.Estado != (estadoEsperado ?? "")) return Error(MsgCambio);

            string error = NegBonoReglas.PuedeRevisar(actual, periodo, accion);
            if (error != null) return Error(error);
            if (actual.IdPeriodo != periodo.IdPeriodo) return Error("El bono no pertenece al mes abierto.");

            string estadoNuevo;
            decimal? nuevoValor = null;
            string motivoEnviar = null;
            string snapEmpresa = null, snapNombre = null, snapIdent = null;
            List<EntBonoPersona> activas = LeerPersonasActivas();

            if (accion == "Rechazar")
            {
                if (motivo.Length == 0) return Error("Escriba el motivo del rechazo.");
                error = Largo(motivo, 500, "Motivo del rechazo");
                if (error != null) return Error(error);
                estadoNuevo = BonoEstados.Rechazado;
                motivoEnviar = motivo;
            }
            else
            {
                if (accion == "Ajustar")
                {
                    if (!valor.HasValue) return Error("Escriba el nuevo valor.");
                    if (motivo.Length == 0) return Error("Escriba el motivo del ajuste.");
                    decimal v = Redondear(valor.Value);
                    error = NegBonoReglas.ValidarValor(Tipo(LeerTipos(), actual.IdTipo), actual.Semanas, v, motivo)
                         ?? (v > ValorMaximo ? "El valor es demasiado grande." : null)
                         ?? Largo(motivo, 500, "Motivo del ajuste");
                    if (error != null) return Error(error);
                    nuevoValor = v;
                    motivoEnviar = motivo;
                }

                EntBonoPersona persona = Persona(activas, actual.CodUsuario);
                if (persona != null)
                {
                    snapIdent = NegBonoReglas.Identificacion(persona, out error);
                    if (error != null) return Error(error);
                    snapEmpresa = NegBonoReglas.EmpresaNormalizada(persona.Empresa);
                    snapNombre = (persona.Nombre ?? "").Trim();
                }
                else if (string.IsNullOrWhiteSpace(actual.Identificacion) || NegBonoReglas.EmpresaNormalizada(actual.Empresa).Length == 0)
                {
                    // Ya no esta activa: vale la copia tomada al enviar, si esta completa.
                    return Error(NombreBono(actual, null) + " ya no está activo y el bono no tiene su identificación: no se puede aprobar.");
                }

                List<DateTime> choques = NegBonoReglas.ChocaDoblePago(actual, LeerDePersona(actual.CodUsuario));
                if (choques.Count > 0) return Error(NegBonoReglas.MensajeDoblePago(choques));
                estadoNuevo = BonoEstados.Aprobado;
            }

            int r = CambiarEstadoEnBase(actual.IdBono, actual.Estado, estadoNuevo, nuevoValor, motivoEnviar,
                snapEmpresa, snapNombre, snapIdent, yo);
            if (r != 0) return Error(MensajeCodigo(r, actual));

            List<string> avisos = new List<string>();
            if (accion == "Rechazar" && NegBonoReglas.Normalizar(actual.CodJefe).Length > 0)
            {
                actual.MotivoRechazo = motivo;
                EntBonoPersona jefe = Persona(activas, actual.CodJefe);
                EnviarYAnotar(enviarCorreo, jefe == null ? "" : jefe.Correo, "el jefe " + actual.CodJefe,
                    NegBonoCorreo.AsuntoRechazo(actual, urlProponer), NegBonoCorreo.CuerpoRechazo(actual, urlProponer),
                    actual.IdBono, periodo.IdPeriodo, yo, avisos);
            }

            string mensaje = accion == "Rechazar" ? "Bono rechazado."
                           : accion == "Ajustar" ? "Bono ajustado a " + Dinero(nuevoValor.Value) + " y aprobado."
                           : "Bono aprobado.";
            return Bien(mensaje, avisos);
        }

        /// <summary>
        /// Bono nuevo para cualquier persona activa: queda Aprobado, Origen =
        /// TalentoHumano, sin jefe, con la copia de empresa, nombre e
        /// identificacion (obligatoria) tomada al guardar.
        /// </summary>
        public static EntBonoResultado RegistrarDirecto(EntBono b, string yo, bool confirmarRepetido, DateTime hoy)
        {
            if (b == null) return Error("Faltan los datos del bono.");
            yo = NegBonoReglas.Normalizar(yo);

            EntBonoPeriodo periodo = Abierto(LeerPeriodos());
            if (periodo == null) return Error(MsgSinMes);

            EntBonoPersona persona = Persona(LeerPersonasActivas(), b.CodUsuario);
            if (persona == null) return Error("Esa persona no está activa.");

            EntBonoTipo tipo = Tipo(LeerTipos(), b.IdTipo);
            if (tipo == null) return Error("El tipo de bono no existe.");
            if (!tipo.Activo) return Error("El tipo «" + tipo.Nombre + "» está desactivado.");

            string error;
            string ident = NegBonoReglas.Identificacion(persona, out error);
            if (error != null) return Error(error);

            EntBono nuevo = Preparar(b, persona, tipo, periodo);
            nuevo.IdBono = 0;
            error = ValidarContenido(nuevo, tipo, hoy);
            if (error != null) return Error(error);

            EntBonoResultado repetido = ChequearRepeticiones(nuevo, tipo, confirmarRepetido);
            if (repetido != null) return repetido;

            nuevo.Identificacion = ident;
            nuevo.Origen = BonoOrigenes.TalentoHumano;
            nuevo.CodJefe = "";

            int r = GuardarEnBase(nuevo, "", BonoEstados.Aprobado, yo);
            if (r <= 0) return Error(MensajeCodigo(r, nuevo));
            return Bien("Bono registrado y aprobado.", null);
        }

        /// <summary>
        /// Crea (IdTipo = 0) o edita un tipo. Conceptos: claves DOS / AGILITY
        /// (en cualquier caso); vacio borra el de esa empresa.
        /// </summary>
        public static EntBonoResultado GuardarTipo(EntBonoTipo t, string yo)
        {
            if (t == null) return Error("Faltan los datos del tipo.");
            yo = NegBonoReglas.Normalizar(yo);

            string nombre = (t.Nombre ?? "").Trim();
            if (nombre.Length == 0) return Error("Escriba el nombre del tipo.");
            string error = Largo(nombre, 150, "Nombre");
            if (error != null) return Error(error);

            string familia = Familias.FirstOrDefault(f => string.Equals(f, (t.Familia ?? "").Trim(), StringComparison.OrdinalIgnoreCase));
            if (familia == null) return Error("La familia debe ser Standby, Contrato u Otro.");

            decimal? sugerido = t.ValorSugerido.HasValue ? Redondear(t.ValorSugerido.Value) : (decimal?)null;
            if (sugerido.HasValue && sugerido.Value <= 0m) return Error("El valor sugerido debe ser mayor que cero (o quedar vacío).");
            if (sugerido.HasValue && sugerido.Value > ValorMaximo) return Error("El valor sugerido es demasiado grande.");

            string dos = Concepto(t.Conceptos, "DOS");
            string agility = Concepto(t.Conceptos, "AGILITY");
            error = Largo(dos, 10, "Concepto SAP DOS") ?? Largo(agility, 10, "Concepto SAP AGILITY");
            if (error != null) return Error(error);

            if (t.IdTipo != 0)
            {
                EntBonoTipo antes = Tipo(LeerTipos(), t.IdTipo);
                if (antes == null) return Error("El tipo ya no existe; recargue la pantalla.");
                if (antes.PorSemana != t.PorSemana && TipoTieneBonos(t.IdTipo))
                    return Error("No se puede cambiar «por semana» de un tipo que ya tiene bonos; cree un tipo nuevo.");
            }

            int r = GuardarTipoEnBase(t.IdTipo, nombre, familia, sugerido, t.PorSemana, t.Activo, dos, agility, yo);
            if (r == -1) return Error("Ya existe un tipo con ese nombre.");
            if (r == -2) return Error("El tipo ya no existe; recargue la pantalla.");
            if (r <= 0) return Error(MensajeCodigo(r, new EntBono()));
            return Bien("Tipo guardado.", null);
        }

        private static string Concepto(IDictionary<string, string> conceptos, string empresa)
        {
            if (conceptos == null) return "";
            string codigo = conceptos.Where(kv => NegBonoReglas.Normalizar(kv.Key) == empresa)
                                     .Select(kv => kv.Value)
                                     .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
            return (codigo ?? "").Trim().ToUpperInvariant();
        }

        /// <summary>
        /// Abre el mes siguiente al ultimo y avisa a los jefes con bonos en los 3
        /// ultimos Cerrado o con pendientes (los que el mes nuevo engancha).
        /// </summary>
        public static EntBonoResultado AbrirMes(string yo, Func<string, string, string, bool> enviarCorreo, string urlProponer)
        {
            yo = NegBonoReglas.Normalizar(yo);
            List<EntBonoPeriodo> periodos = LeerPeriodos();
            if (Abierto(periodos) != null) return Error("Ya hay un mes de bonos abierto.");

            // Antes de abrir: al abrir, los pendientes se enganchan al mes nuevo.
            List<EntBono> pendientes = LeerDetalles(null).Where(b => !b.IdPeriodo.HasValue).ToList();
            List<EntBono> recientes = periodos
                .Where(p => p.Estado == BonoPeriodoEstados.Cerrado)
                .OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes)
                .Take(3)
                .SelectMany(p => LeerDetalles(p.IdPeriodo))
                .ToList();
            // Todo lo que se pueda leer va antes de abrir: despues de abrir, nada debe lanzar.
            List<EntBonoPersona> activas = LeerPersonasActivas();
            List<string> jefes = NegBonoCorreo.JefesApertura(periodos, recientes, pendientes);
            DateTime previsto = MesSiguiente(periodos);

            int idPeriodo = AbrirPeriodoEnBase(yo);
            if (idPeriodo == -1) return Error("Ya hay un mes de bonos abierto.");
            if (idPeriodo <= 0) return Error(MensajeCodigo(idPeriodo, new EntBono()));

            List<string> avisos = new List<string>();
            int anio = previsto.Year, mes = previsto.Month;
            try
            {
                EntBonoPeriodo nuevo = LeerPeriodos().FirstOrDefault(p => p.IdPeriodo == idPeriodo);
                if (nuevo != null) { anio = nuevo.Anio; mes = nuevo.Mes; }
            }
            catch (Exception)
            {
                // Se usa el mes previsto (mismo calculo que Sp_RTA_BonoAbrirPeriodo).
            }

            try
            {
                string asunto = NegBonoCorreo.AsuntoApertura(anio, mes);
                string cuerpo = NegBonoCorreo.CuerpoApertura(anio, mes, urlProponer);
                foreach (string cod in jefes)
                {
                    EntBonoPersona jefe = Persona(activas, cod);
                    EnviarYAnotar(enviarCorreo, jefe == null ? "" : jefe.Correo, "el jefe " + cod, asunto, cuerpo,
                        null, idPeriodo, yo, avisos);
                }
            }
            catch (Exception ex)
            {
                avisos.Add("El mes se abrió, pero no se pudo avisar a los jefes.");
                try
                {
                    BitacoraEnBase(null, idPeriodo, "Correo", null, "Fallido",
                        "Aviso de apertura a los jefes no enviado; error: " + ex.Message, yo);
                }
                catch (Exception)
                {
                    avisos.Add("Tampoco se pudo anotar en la bitácora.");
                }
            }

            string mensaje = "Se abrió el mes de bonos de " + MesTexto(anio, mes) + ".";
            if (pendientes.Count > 0)
                mensaje += " Entraron " + pendientes.Count.ToString(CultureInfo.InvariantCulture) + " bono(s) pendiente(s).";
            mensaje += " Jefes a avisar: " + jefes.Count.ToString(CultureInfo.InvariantCulture) + ".";
            return Bien(mensaje, avisos);
        }

        /// <summary>El mes que abriria Sp_RTA_BonoAbrirPeriodo: el siguiente al ultimo existente, o el actual si no hay ninguno.</summary>
        private static DateTime MesSiguiente(IEnumerable<EntBonoPeriodo> periodos)
        {
            EntBonoPeriodo ultimo = periodos.OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes).FirstOrDefault();
            DateTime hoy = DateTime.Today;
            return ultimo == null ? new DateTime(hoy.Year, hoy.Month, 1) : new DateTime(ultimo.Anio, ultimo.Mes, 1).AddMonths(1);
        }

        /// <summary>Si el tipo tiene algun bono no rechazado, en cualquier periodo o pendiente (solo lectura).</summary>
        private static bool TipoTieneBonos(int idTipo)
        {
            if (LeerDetalles(null).Any(b => b.IdTipo == idTipo && b.Estado != BonoEstados.Rechazado)) return true;
            foreach (EntBonoPeriodo p in LeerPeriodos())
            {
                if (LeerDetalles(p.IdPeriodo).Any(b => b.IdTipo == idTipo && b.Estado != BonoEstados.Rechazado)) return true;
            }
            return false;
        }

        /// <summary>
        /// Cierra el mes abierto: entra lo Aprobado (con la copia de tipo y
        /// concepto), el resto queda pendiente. No cierra si falta un concepto SAP.
        /// </summary>
        public static EntBonoResultado CerrarMes(string yo)
        {
            yo = NegBonoReglas.Normalizar(yo);
            EntBonoPeriodo periodo = Abierto(LeerPeriodos());
            if (periodo == null) return Error(MsgSinMes);

            EntBonoCierre previo = NegBonoReglas.Cierre(LeerDetalles(periodo.IdPeriodo), LeerTipos());
            if (!previo.PuedeCerrar)
                return Error("No se puede cerrar: falta el concepto SAP de " + string.Join(", ", previo.SinConcepto)
                             + ". Complételo en el Catálogo.");

            int r = CerrarPeriodoEnBase(periodo.IdPeriodo, yo);
            if (r == -1) return Error("El mes ya no está abierto; recargue la pantalla.");
            if (r == -2) return Error("No se puede cerrar: a algún bono aprobado le falta el concepto SAP. Revise el Catálogo.");
            if (r != 0) return Error(MensajeCodigo(r, new EntBono()));

            int fuera = previo.QuedanFuera.Count;
            string mensaje = "Se cerró el mes de " + MesTexto(periodo.Anio, periodo.Mes) + " con " + Dinero(previo.Total) + " aprobados.";
            if (fuera > 0)
                mensaje += " " + fuera.ToString(CultureInfo.InvariantCulture) + " bono(s) quedan pendientes para el próximo mes.";
            return Bien(mensaje, null);
        }
    }
}
