using System;
using System.Collections.Generic;
using System.Globalization;

namespace CapaEntidad
{
    /// <summary>
    /// El tiempo registrado fuera de horario que NO se va a pagar, por persona.
    /// Es informacion para que Nomina decida antes de cerrar; ningun monto sale
    /// de aqui.
    ///
    /// Calculado = false significa "no se pudo calcular" (Error dice por que), y
    /// no "no hay nada": confundir las dos cosas llevaria a cerrar un periodo
    /// creyendo que esta limpio.
    /// </summary>
    public class EntHeFueraDeHorario
    {
        public bool Calculado { get; set; }
        public string Error { get; set; } = "";
        public List<EntHeFueraDeHorarioPersona> Personas { get; set; } = new List<EntHeFueraDeHorarioPersona>();
        public List<EntHeFueraDeHorarioDetalle> Detalle { get; set; } = new List<EntHeFueraDeHorarioDetalle>();

        /// <summary>
        /// Empleados activos con horas en el periodo que NO estan en el modulo. No
        /// tienen sueldo cargado, asi que aqui no hay montos: solo horas y el motivo.
        /// </summary>
        public List<EntHeFueraDelModuloPersona> FueraDelModulo { get; set; } = new List<EntHeFueraDelModuloPersona>();

        public int PersonasFueraDelModulo { get { return FueraDelModulo.Count; } }

        /// <summary>
        /// Todos los segmentos candidatos del periodo ya resueltos (decision vigente,
        /// valor por omision y bloqueo), con su ValorUsd. Alimenta la ventana de
        /// detalle de cada persona y la hoja «Decisiones» del Excel.
        /// </summary>
        public List<EntHeCandidato> Candidatos { get; set; } = new List<EntHeCandidato>();

        /// <summary>Una fila por persona del periodo: que se paga, que no, y el total a pagar.</summary>
        public List<EntHeConciliacionFila> Conciliacion { get; set; } = new List<EntHeConciliacionFila>();

        /// <summary>
        /// Cuantas filas de la conciliacion estan desactualizadas (su TotalHE no es
        /// lo que pagan los minutos incluidos hoy). Distinto de 0: hay que pulsar
        /// «Recalcular» antes de cerrar.
        /// </summary>
        public int FilasDesactualizadas
        {
            get
            {
                int n = 0;
                foreach (EntHeConciliacionFila c in Conciliacion) { if (c != null && c.Desactualizada) { n++; } }
                return n;
            }
        }

        /// <summary>Los candidatos de una persona, para su ventana de detalle.</summary>
        public List<EntHeCandidato> CandidatosDe(long idEmpleado)
        {
            List<EntHeCandidato> lista = new List<EntHeCandidato>();
            foreach (EntHeCandidato c in Candidatos) { if (c.IdEmpleado == idEmpleado) { lista.Add(c); } }
            return lista;
        }

        public decimal TotalHorasAprobadasFueraDelModulo
        {
            get
            {
                int minutos = 0;
                foreach (EntHeFueraDelModuloPersona p in FueraDelModulo) { minutos += p.MinutosAprobados50 + p.MinutosAprobados100; }
                return EntHeFueraDeHorarioPersona.Horas(minutos);
            }
        }

        public decimal TotalHorasSinAprobar
        {
            get
            {
                int minutos = 0;
                foreach (EntHeFueraDeHorarioPersona p in Personas) { minutos += p.MinutosSinAprobar50 + p.MinutosSinAprobar100; }
                return EntHeFueraDeHorarioPersona.Horas(minutos);
            }
        }

        public decimal TotalHorasRechazado
        {
            get
            {
                int minutos = 0;
                foreach (EntHeFueraDeHorarioPersona p in Personas) { minutos += p.MinutosRechazado50 + p.MinutosRechazado100; }
                return EntHeFueraDeHorarioPersona.Horas(minutos);
            }
        }

        public int PersonasConSinAprobar
        {
            get
            {
                int n = 0;
                foreach (EntHeFueraDeHorarioPersona p in Personas) { if (p.MinutosSinAprobar50 + p.MinutosSinAprobar100 > 0) { n++; } }
                return n;
            }
        }

        public int PersonasConRechazado
        {
            get
            {
                int n = 0;
                foreach (EntHeFueraDeHorarioPersona p in Personas) { if (p.MinutosRechazado50 + p.MinutosRechazado100 > 0) { n++; } }
                return n;
            }
        }

        /// <summary>La persona, o null si no tiene nada fuera de horario.</summary>
        public EntHeFueraDeHorarioPersona DePersona(long idEmpleado)
        {
            foreach (EntHeFueraDeHorarioPersona p in Personas) { if (p.IdEmpleado == idEmpleado) { return p; } }
            return null;
        }
    }

    public class EntHeFueraDeHorarioPersona
    {
        public long IdEmpleado { get; set; }
        public int MinutosSinAprobar50 { get; set; }
        public int MinutosSinAprobar100 { get; set; }
        public int MinutosRechazado50 { get; set; }
        public int MinutosRechazado100 { get; set; }

        /// <summary>Se redondea UNA vez, sobre los minutos sumados: redondear cada tramo y sumar descuadra centavos.</summary>
        public decimal HorasSinAprobar { get { return Horas(MinutosSinAprobar50 + MinutosSinAprobar100); } }
        public decimal HorasRechazado { get { return Horas(MinutosRechazado50 + MinutosRechazado100); } }

        public static decimal Horas(int minutos)
        {
            return Math.Round(minutos / 60m, 2, MidpointRounding.AwayFromZero);
        }
    }

    /// <summary>
    /// Una persona con horas en el periodo que no esta en el modulo. El motivo es
    /// siempre el mismo y se dice siempre: sin sus datos de RRHH no hay con que
    /// calcular el pago.
    /// </summary>
    public class EntHeFueraDelModuloPersona
    {
        public const string MotivoTexto = "No está en el módulo de Horas Extras: faltan sus datos de RRHH, no se puede calcular el pago.";

        public long IdEmpleado { get; set; }
        public string Nombre { get; set; } = "";
        public string Cargo { get; set; } = "";
        public int MinutosAprobados50 { get; set; }
        public int MinutosAprobados100 { get; set; }
        public int MinutosSinAprobar50 { get; set; }
        public int MinutosSinAprobar100 { get; set; }
        public int MinutosRechazado50 { get; set; }
        public int MinutosRechazado100 { get; set; }

        public decimal HorasAprobadas50 { get { return EntHeFueraDeHorarioPersona.Horas(MinutosAprobados50); } }
        public decimal HorasAprobadas100 { get { return EntHeFueraDeHorarioPersona.Horas(MinutosAprobados100); } }
        public decimal HorasSinAprobar { get { return EntHeFueraDeHorarioPersona.Horas(MinutosSinAprobar50 + MinutosSinAprobar100); } }
        public decimal HorasRechazado { get { return EntHeFueraDeHorarioPersona.Horas(MinutosRechazado50 + MinutosRechazado100); } }
        public string Motivo { get { return MotivoTexto; } }
    }

    public class EntHeFueraDeHorarioDetalle
    {
        public long IdEmpleado { get; set; }
        public long IdRegDetTareas { get; set; }
        public DateTime TramoInicio { get; set; }
        public DateTime TramoFin { get; set; }

        /// <summary>El pedazo que efectivamente suma, ya descontado lo pagado y lo repetido.</summary>
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public int Minutos { get; set; }
        public int TipoHora { get; set; }

        /// <summary>"SinAprobar" o "Rechazado" (constantes de NegHeFueraDeHorario).</summary>
        public string Categoria { get; set; } = "";

        /// <summary>Sin solicitar / Solicitado / Aprobado sin tipo / Rechazado.</summary>
        public string EstadoTexto { get; set; } = "";
        public string Actividad { get; set; } = "";

        // Textos ya armados: la pantalla y el Excel los muestran igual, y
        // JavaScriptSerializer entrega las fechas como \/Date(n)\/.
        public string Fecha { get { return Inicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture); } }
        public string TramoTexto { get { return Rango(TramoInicio, TramoFin); } }
        public string FueraTexto { get { return Rango(Inicio, Fin); } }
        public string TipoTexto { get { return TipoHora == 2 ? "100%" : "50%"; } }
        public decimal Horas { get { return EntHeFueraDeHorarioPersona.Horas(Minutos); } }

        private static string Rango(DateTime desde, DateTime hasta)
        {
            return desde.ToString("HH:mm", CultureInfo.InvariantCulture) + " - " + hasta.ToString("HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
