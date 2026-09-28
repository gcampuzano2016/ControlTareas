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
