using System;

namespace CapaEntidad
{
    public class ERegistroTrabajo
    {
        public int IdTrabajo { get; set; }
        public int Id_Usuario { get; set; }
        public DateTime HoraEntrada { get; set; }
        public DateTime HoraAlmuerzoSale { get; set; }
        public DateTime HoraAlmuerzoEntra { get; set; }
        public DateTime HoraSalida { get; set; }
        public int Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Log_Usuario { get; set; }
        public string Nom_Usuario { get; set; }
    }
}
