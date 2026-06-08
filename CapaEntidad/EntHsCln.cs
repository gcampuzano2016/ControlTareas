namespace CapaEntidad
{
    public class EntHsCln
    {
        // Control de operación
        public int opcion { get; set; }

        // Campos para HsCln
        public string hsCln_Identificador { get; set; }
        public int idHsCln { get; set; }
        public string hsCln_Nombre { get; set; }
        public string hsCln_ciEmpleado { get; set; }
        public string hsCln_Fecha { get; set; } // Se puede usar DateTime si prefieres
        public string hsCln_Hora { get; set; }
        public string hsCln_Motivo { get; set; }
        public string hsCln_SigVit { get; set; }
        public string hsCln_Antece { get; set; }
        public string hsCln_Interv { get; set; }
        public string hsCln_Recom { get; set; }
        public string hsCln_Obs { get; set; }
        public string hsCln_Estado { get; set; }
        public int hsCln_Seguir { get; set; }

        // Campos para HsCln_Vul
        //public int idHsCln_Vul { get; set; }
        public string hsVul_ciEmpleado { get; set; }
        public string hsVul_Fecha { get; set; } // También puede ser DateTime
        public string hsVul_Vulnerds { get; set; }
        public string hsVul_Rec_Obs { get; set; }


    }

}
