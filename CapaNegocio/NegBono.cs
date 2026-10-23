using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CapaNegocio
{
    /// <summary>
    /// Orquestacion del modulo de bonos: carga (NegBono.Datos.cs), pregunta a
    /// las reglas puras (NegBonoFechas, NegBonoReglas), guarda, anota y avisa
    /// por correo. Las reglas se aplican antes de llamar al procedimiento aunque
    /// este tambien chequee. Partes: este archivo (lectura, Excel y lo comun),
    /// NegBono.Jefe.cs y NegBono.TalentoHumano.cs.
    ///
    /// <para><c>yo</c> es el Cod_Usuario de la sesion; se normaliza igual.</para>
    /// <para>EntBonoResultado: Ok = true guardo (Aviso, si hay, es informativo:
    /// p. ej. un correo que no salio). Ok = false con Mensaje = error. Ok = false
    /// con Mensaje vacio y Aviso = pide confirmar (bono no semanal repetido):
    /// se vuelve a llamar con confirmarRepetido = true.</para>
    /// </summary>
    public static partial class NegBono
    {
        private const string MsgCambio = "El bono cambió mientras lo revisaba; recargue la pantalla.";
        private const string MsgSinMes = "No hay un mes de bonos abierto.";
        private const decimal ValorMaximo = 9999999.99m;

        private static readonly CultureInfo Es = new CultureInfo("es-EC");

        // ------------------------------------------------------------ lectura

        /// <summary>Pantalla del jefe. Sin mes abierto, Periodo = null y lo demas igual (sus pendientes, su gente).</summary>
        public static EntBonoPantallaProponer PantallaProponer(string yo, DateTime hoy)
        {
            yo = NegBonoReglas.Normalizar(yo);
            EntBonoPantallaProponer p = new EntBonoPantallaProponer();
            p.Periodo = Abierto(LeerPeriodos());
            p.Tipos = LeerTipos().Where(t => t.Activo).ToList();
            p.SemanasMarcables = Marcables(hoy);
            if (yo.Length == 0) return p;

            p.Bonos = LeerDetalles(null)
                .Where(b => NegBonoReglas.Normalizar(b.CodJefe) == yo)
                .OrderBy(b => b.Nombre ?? "", StringComparer.Create(Es, true))
                .ThenBy(b => b.IdBono)
                .ToList();

            foreach (EntBonoPersona persona in NegBonoReglas.MiGente(LeerPersonasActivas(), yo))
            {
                List<EntBono> suyos = LeerDePersona(persona.CodUsuario);
                string falta;
                NegBonoReglas.Identificacion(persona, out falta);
                p.Gente.Add(new EntBonoGente
                {
                    CodUsuario = persona.CodUsuario,
                    Nombre = persona.Nombre,
                    Empresa = NegBonoReglas.EmpresaNormalizada(persona.Empresa),
                    LugarSugerido = suyos.Where(b => !string.IsNullOrWhiteSpace(b.Lugar))
                                         .OrderByDescending(b => b.IdBono)
                                         .Select(b => b.Lugar.Trim())
                                         .FirstOrDefault() ?? "",
                    AvisoIdentificacion = falta ?? ""
                });
                foreach (EntBono b in suyos.Where(b => b.Lunes != null && b.Lunes.Count > 0))
                {
                    p.SemanasTomadas.Add(new EntBonoSemanasTomadas
                    {
                        IdBono = b.IdBono,
                        CodUsuario = b.CodUsuario,
                        IdTipo = b.IdTipo,
                        Lunes = b.Lunes.ToList()
                    });
                }
            }
            return p;
        }

        /// <summary>Pantalla de Talento Humano: mes abierto (o null), bonos no Borrador, resumen, personas activas y catalogo.</summary>
        public static EntBonoPantallaRevisar PantallaRevisar()
        {
            EntBonoPantallaRevisar p = new EntBonoPantallaRevisar();
            p.Periodo = Abierto(LeerPeriodos());
            List<EntBono> todos = LeerDetalles(null);
            p.Bonos = todos.Where(b => b.Estado != BonoEstados.Borrador)
                           .OrderBy(b => b.CodJefe ?? "", StringComparer.OrdinalIgnoreCase)
                           .ThenBy(b => b.Nombre ?? "", StringComparer.Create(Es, true))
                           .ThenBy(b => b.IdBono)
                           .ToList();
            p.Resumen = new EntBonoResumen
            {
                Borradores = todos.Count(b => b.Estado == BonoEstados.Borrador),
                Enviados = todos.Count(b => b.Estado == BonoEstados.Enviado),
                Aprobados = todos.Count(b => b.Estado == BonoEstados.Aprobado),
                Rechazados = todos.Count(b => b.Estado == BonoEstados.Rechazado),
                TotalAprobado = todos.Where(b => b.Estado == BonoEstados.Aprobado).Sum(b => b.Valor)
            };
            p.Personas = LeerPersonasActivas();
            p.Tipos = LeerTipos();
            return p;
        }

        /// <summary>Todo el catalogo, activos e inactivos.</summary>
        public static List<EntBonoTipo> Catalogo()
        {
            return LeerTipos();
        }

        /// <summary>Un renglon por periodo, del mas reciente al mas viejo; variacion contra el mes calendario anterior.</summary>
        public static List<EntBonoMes> Reporte()
        {
            List<EntBonoPeriodo> periodos = LeerPeriodos();
            Dictionary<int, EntBonoPeriodo> porMes = periodos
                .GroupBy(p => Clave(p.Anio, p.Mes))
                .ToDictionary(g => g.Key, g => g.First());

            List<EntBonoMes> filas = new List<EntBonoMes>();
            foreach (EntBonoPeriodo p in periodos.OrderByDescending(x => x.Anio).ThenByDescending(x => x.Mes))
            {
                DateTime anterior = new DateTime(p.Anio, p.Mes, 1).AddMonths(-1);
                EntBonoPeriodo previo;
                porMes.TryGetValue(Clave(anterior.Year, anterior.Month), out previo);
                filas.Add(new EntBonoMes
                {
                    IdPeriodo = p.IdPeriodo,
                    Anio = p.Anio,
                    Mes = p.Mes,
                    Estado = p.Estado,
                    Total = p.TotalAprobado,
                    Variacion = NegBonoReglas.Variacion(p.TotalAprobado, previo == null ? (decimal?)null : previo.TotalAprobado)
                });
            }
            return filas;
        }

        /// <summary>Lo que se ve antes de cerrar el mes abierto; null si no hay mes abierto.</summary>
        public static EntBonoCierre PrevioCierre()
        {
            EntBonoPeriodo periodo = Abierto(LeerPeriodos());
            if (periodo == null) return null;
            return NegBonoReglas.Cierre(LeerDetalles(periodo.IdPeriodo), LeerTipos());
        }

        /// <summary>
        /// Excel para Nomina. null si el periodo no existe o es Historico.
        /// Abierto: los Aprobados hasta ahora y "_PROVISIONAL" en el nombre.
        /// </summary>
        public static byte[] Excel(int idPeriodo, out string nombreArchivo)
        {
            nombreArchivo = null;
            EntBonoPeriodo p = LeerPeriodos().FirstOrDefault(x => x.IdPeriodo == idPeriodo);
            if (p == null || p.Estado == BonoPeriodoEstados.Historico) return null;

            List<EntBono> aprobados = LeerDetalles(p.IdPeriodo).Where(b => b.Estado == BonoEstados.Aprobado).ToList();
            Dictionary<int, EntBonoTipo> tipos = LeerTipos().GroupBy(t => t.IdTipo).ToDictionary(g => g.Key, g => g.First());

            nombreArchivo = "Bonos_" + p.Anio.ToString(CultureInfo.InvariantCulture) + "_"
                          + p.Mes.ToString("00", CultureInfo.InvariantCulture)
                          + (p.Estado == BonoPeriodoEstados.Abierto ? "_PROVISIONAL" : "") + ".xlsx";
            return NegBonoExportacion.Construir(p, aprobados, tipos);
        }

        // ------------------------------------------------------------ comun

        private static int Clave(int anio, int mes)
        {
            return anio * 100 + mes;
        }

        private static List<DateTime> Marcables(DateTime hoy)
        {
            List<DateTime> lunes = new List<DateTime>();
            for (DateTime l = NegBonoFechas.PrimerLunes; NegBonoFechas.Terminada(l, hoy); l = l.AddDays(7))
                lunes.Add(l);
            return lunes;
        }

        private static EntBonoPeriodo Abierto(IEnumerable<EntBonoPeriodo> periodos)
        {
            return (periodos ?? new EntBonoPeriodo[0]).FirstOrDefault(p => p.Estado == BonoPeriodoEstados.Abierto);
        }

        private static EntBonoPersona Persona(IEnumerable<EntBonoPersona> activas, string cod)
        {
            string c = NegBonoReglas.Normalizar(cod);
            if (c.Length == 0) return null;
            return (activas ?? new EntBonoPersona[0]).FirstOrDefault(p => NegBonoReglas.Normalizar(p.CodUsuario) == c);
        }

        private static HashSet<string> CodigosMiGente(IEnumerable<EntBonoPersona> activas, string yo)
        {
            return new HashSet<string>(NegBonoReglas.MiGente(activas, yo).Select(p => NegBonoReglas.Normalizar(p.CodUsuario)));
        }

        private static EntBonoTipo Tipo(IEnumerable<EntBonoTipo> tipos, int idTipo)
        {
            return (tipos ?? new EntBonoTipo[0]).FirstOrDefault(t => t.IdTipo == idTipo);
        }

        /// <summary>El bono tal como esta ahora en la base (mes abierto o pendiente); null si ya no existe ahi.</summary>
        private static EntBono BonoVivo(int idBono)
        {
            return LeerDetalles(null).FirstOrDefault(b => b.IdBono == idBono);
        }

        private static EntBonoResultado Error(string mensaje)
        {
            return new EntBonoResultado { Ok = false, Mensaje = mensaje ?? "" };
        }

        private static EntBonoResultado Bien(string mensaje, IEnumerable<string> avisos)
        {
            return new EntBonoResultado
            {
                Ok = true,
                Mensaje = mensaje ?? "",
                Aviso = string.Join(" ", (avisos ?? new string[0]).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct())
            };
        }

        /// <summary>Codigos de los procedimientos que escriben un bono.</summary>
        private static string MensajeCodigo(int codigo, EntBono b)
        {
            switch (codigo)
            {
                case -1: return MsgCambio;
                case -2: return MsgSinMes;
                case -3: return MensajeDoblePago(b);
                default: return "La base de datos devolvió un código inesperado (" + codigo.ToString(CultureInfo.InvariantCulture) + ").";
            }
        }

        /// <summary>El mensaje de NegBonoReglas con las semanas que chocan ahora; si no se ven, todas las del bono.</summary>
        private static string MensajeDoblePago(EntBono b)
        {
            List<DateTime> choques = NegBonoReglas.ChocaDoblePago(b, LeerDePersona(b.CodUsuario));
            return NegBonoReglas.MensajeDoblePago(choques.Count > 0 ? choques : b.Lunes);
        }

        private static string Largo(string valor, int maximo, string campo)
        {
            return (valor ?? "").Trim().Length > maximo
                ? "«" + campo + "» no puede pasar de " + maximo.ToString(CultureInfo.InvariantCulture) + " caracteres."
                : null;
        }

        private static decimal Redondear(decimal v)
        {
            return Math.Round(v, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>Copia normalizada del bono que llega de la pantalla, con la persona y el tipo de la base.</summary>
        private static EntBono Preparar(EntBono b, EntBonoPersona persona, EntBonoTipo tipo, EntBonoPeriodo periodo)
        {
            List<DateTime> lunes = (b.Lunes ?? new List<DateTime>()).Select(l => l.Date).ToList();
            int semanas = lunes.Distinct().Count();
            return new EntBono
            {
                IdBono = b.IdBono,
                IdPeriodo = periodo.IdPeriodo,
                CodUsuario = persona.CodUsuario,
                Empresa = NegBonoReglas.EmpresaNormalizada(persona.Empresa),
                Nombre = (persona.Nombre ?? "").Trim(),
                Lugar = (b.Lugar ?? "").Trim(),
                IdTipo = tipo.IdTipo,
                Semanas = semanas,
                ValorSugerido = NegBonoReglas.Sugerido(tipo, semanas),
                Valor = Redondear(b.Valor),
                MotivoAjuste = (b.MotivoAjuste ?? "").Trim(),
                Observacion = (b.Observacion ?? "").Trim(),
                NombreTipo = tipo.Nombre,
                Lunes = lunes
            };
        }

        /// <summary>Semanas, valor y largos. null = valido. Deja los lunes sin repetidos y ordenados.</summary>
        private static string ValidarContenido(EntBono b, EntBonoTipo tipo, DateTime hoy)
        {
            string error = NegBonoFechas.ValidarSemanas(b.Lunes, tipo.PorSemana, hoy)
                        ?? NegBonoReglas.ValidarValor(tipo, b.Semanas, b.Valor, b.MotivoAjuste)
                        ?? (b.Valor > ValorMaximo ? "El valor es demasiado grande." : null)
                        ?? Largo(b.Lugar, 100, "Lugar")
                        ?? Largo(b.Observacion, 500, "Observación")
                        ?? Largo(b.MotivoAjuste, 500, "Motivo del ajuste");
            b.Lunes = b.Lunes.Distinct().OrderBy(l => l).ToList();
            return error;
        }

        /// <summary>Doble pago y bono no semanal repetido contra los bonos de la persona. null = puede seguir.</summary>
        private static EntBonoResultado ChequearRepeticiones(EntBono b, EntBonoTipo tipo, bool confirmarRepetido)
        {
            List<EntBono> suyos = LeerDePersona(b.CodUsuario);
            List<DateTime> choques = NegBonoReglas.ChocaDoblePago(b, suyos);
            if (choques.Count > 0) return Error(NegBonoReglas.MensajeDoblePago(choques));
            if (!confirmarRepetido)
            {
                string aviso = NegBonoReglas.AvisoRepetido(b, tipo, suyos);
                if (aviso != null) return new EntBonoResultado { Ok = false, Mensaje = "", Aviso = aviso };
            }
            return null;
        }

        /// <summary>
        /// Envia un correo y lo anota en Bono_Bitacora (Accion = Correo). Nunca
        /// lanza: un fallo (sin direccion, rechazado, excepcion) va a la bitacora
        /// con su error y a <paramref name="avisos"/>.
        /// </summary>
        private static void EnviarYAnotar(Func<string, string, string, bool> enviarCorreo, string destinatario, string quien,
            string asunto, string cuerpo, int? idBono, int? idPeriodo, string yo, List<string> avisos)
        {
            string dest = (destinatario ?? "").Trim();
            string para = dest.Length > 0 ? dest : quien;
            string error = null;
            if (dest.Length == 0) error = "sin dirección de correo";
            else if (enviarCorreo == null) error = "no hay cómo enviar correos";
            else
            {
                try
                {
                    if (!enviarCorreo(dest, asunto, cuerpo)) error = "el envío falló";
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
            }

            try
            {
                BitacoraEnBase(idBono, idPeriodo, "Correo", null, error == null ? "Enviado" : "Fallido",
                    "Para: " + para + "; asunto: " + asunto + (error == null ? "" : "; error: " + error), yo);
            }
            catch (Exception)
            {
                avisos.Add("No se pudo anotar en la bitácora el correo a " + para + ".");
            }

            if (error != null) avisos.Add("No se pudo enviar el correo a " + para + ".");
        }

        private static string MesTexto(int anio, int mes)
        {
            return Es.DateTimeFormat.GetMonthName(mes).ToLower(Es) + " " + anio.ToString(CultureInfo.InvariantCulture);
        }

        private static readonly NumberFormatInfo Moneda = new NumberFormatInfo
        {
            NumberDecimalSeparator = ",",
            NumberGroupSeparator = ".",
            NumberGroupSizes = new[] { 3 }
        };

        private static string Dinero(decimal v)
        {
            return "$" + v.ToString("N2", Moneda);
        }
    }
}
