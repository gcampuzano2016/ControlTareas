using System;
using System.Globalization;

namespace CapaEntidad
{
    /// <summary>
    /// Un segmento que Nomina puede incluir en el pago de un periodo. Los primeros
    /// campos son el primer conjunto de Sp_RTA_HeTramosCandidatos, tal cual; los
    /// demas los llena NegHeDecisiones.Resolver con la decision vigente, el valor
    /// por omision y la proteccion contra el pago doble.
    ///
    /// Un tramo aprobado es UN segmento: el tramo completo. Cualquier otro tramo
    /// aporta solo sus segmentos fuera de horario, cada uno con su SegmentoInicio.
    /// </summary>
    public class EntHeCandidato
    {
        /// <summary>Numera minutos igual que NegHeFueraDeHorario: cualquier fecha anterior a los datos sirve.</summary>
        private static readonly DateTime Origen = new DateTime(2000, 1, 1);

        // ------------------------------------------- lo que trae la base ------

        public long IdEmpleado { get; set; }
        public long IdRegDetTareas { get; set; }

        /// <summary>El tramo tal como lo registro la persona.</summary>
        public DateTime TramoInicio { get; set; }
        public DateTime TramoFin { get; set; }

        /// <summary>La parte del tramo que se decide. En un aprobado coincide con el tramo.</summary>
        public DateTime SegmentoInicio { get; set; }
        public DateTime SegmentoFin { get; set; }

        /// <summary>1 = 50%, 2 = 100%: el aprobado por la jefatura, o el del horario si no esta aprobado.</summary>
        public int TipoHora { get; set; }

        /// <summary>Det_Horas_Extras_Estado: 0 sin solicitar, 1 solicitado, 2 aprobado, 3 rechazado.</summary>
        public int EstadoTramo { get; set; }

        /// <summary>Det_Horas_Extras_Tipo: 0 sin tipo, 1 = 50%, 2 = 100%.</summary>
        public int TipoRegistrado { get; set; }

        public string Actividad { get; set; } = "";

        /// <summary>El tramo empieza dentro de [FechaInicio, FechaFin] del periodo pedido, no en los 7 dias de borde.</summary>
        public bool DentroDelPeriodo { get; set; }

        // ------------------------------------------- lo que resuelve Negocio --

        /// <summary>Si se paga en este periodo. Nunca es true en un segmento bloqueado.</summary>
        public bool Incluir { get; set; }

        /// <summary>1 = 50%, 2 = 100%: el de la decision, o TipoHora si no hay decision.</summary>
        public int TipoElegido { get; set; }

        /// <summary>null = el segmento completo; si no, los primeros N minutos.</summary>
        public int? MinutosIncluidos { get; set; }

        public string Motivo { get; set; } = "";
        public string DecididoPor { get; set; } = "";
        public DateTime? DecididoEl { get; set; }

        /// <summary>Otro periodo ya lo tiene incluido: incluirlo aqui seria pagarlo dos veces.</summary>
        public bool Bloqueado { get; set; }

        /// <summary>"incluido en el periodo dd/MM/yyyy - dd/MM/yyyy", o vacio si no esta bloqueado.</summary>
        public string BloqueadoPor { get; set; } = "";

        // ------------------------------------------- calculados ---------------

        /// <summary>Aprobado con tipo: la misma regla que usa el procedimiento para decidir el segmento unico.</summary>
        public bool EsAprobado
        {
            get { return EstadoTramo == 2 && (TipoRegistrado == 1 || TipoRegistrado == 2); }
        }

        /// <summary>Como se agrupa en la ventana de detalle.</summary>
        public string Grupo
        {
            get
            {
                if (EsAprobado) { return "Aprobado"; }
                switch (EstadoTramo)
                {
                    case 1: return "Pendiente";
                    case 2: return "Aprobado sin tipo";
                    case 3: return "Rechazado";
                    default: return "Sin solicitar";
                }
            }
        }

        /// <summary>
        /// Minutos enteros del segmento, truncando cada extremo al minuto: coincide
        /// con DATEDIFF(MINUTE) del SQL y con NegHeFueraDeHorario.
        /// </summary>
        public int MinutosSegmento
        {
            get
            {
                long minutos = Minuto(SegmentoFin) - Minuto(SegmentoInicio);
                return minutos > 0 ? (int)minutos : 0;
            }
        }

        /// <summary>
        /// Lo que entra en el pago: nada si no se incluye; si se incluye, el
        /// segmento completo o los minutos reducidos. El tope en MinutosSegmento es
        /// defensivo: el procedimiento valida los minutos contra el fin del TRAMO
        /// (no del segmento), asi que una decision guardada por fuera de la pantalla
        /// podria pasarse del segmento, y ese exceso no es tiempo fuera de horario.
        /// </summary>
        public int MinutosEfectivos
        {
            get
            {
                if (!Incluir) { return 0; }
                int total = MinutosSegmento;
                int pedido = MinutosIncluidos ?? total;
                if (pedido < 0) { return 0; }
                return pedido < total ? pedido : total;
            }
        }

        /// <summary>El segmento como lo lee Nomina en un mensaje: dd/MM/yyyy HH:mm - HH:mm.</summary>
        public string SegmentoTexto
        {
            get { return SegmentoInicio.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) + " - " + SegmentoFin.ToString("HH:mm", CultureInfo.InvariantCulture); }
        }

        public static long Minuto(DateTime t)
        {
            return (long)Math.Floor((t - Origen).TotalMinutes);
        }
    }
}
