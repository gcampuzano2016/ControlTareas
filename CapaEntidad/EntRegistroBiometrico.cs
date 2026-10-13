using System;

namespace CapaEntidad
{
    public class EntRegistroBiometrico
    {
        public Int64 IdProceso { get; set; }
        public decimal Id_Usuario { get; set; }
        public DateTime FechaEntrada { get; set; }
        public DateTime FechaAlmorzar { get; set; }
        public DateTime FechaRegAlmorzar { get; set; }
        public DateTime FechaSalida { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int Estado { get; set; }

        public string Observacion { get; set; }
        public int Tipo { get; set; }
    }
}
