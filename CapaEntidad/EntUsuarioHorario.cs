using System;

namespace CapaEntidad
{
    public class EntUsuarioHorario
    {
        public string Cod_Usuario { get; set; }
        public string Nom_Usuario { get; set; }
        public string Cedula { get; set; }
        public string Departamento { get; set; }
        public string Empresa { get; set; }
        public Int32 IdHorarioLaboral { get; set; }
        public string CodigoHorario { get; set; }
        public string NombreHorario { get; set; }
        public Int32 EsPredeterminado { get; set; }
        public string FechaDesde { get; set; }
    }
}
