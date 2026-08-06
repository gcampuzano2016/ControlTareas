namespace CapaEntidad
{
    public class EntUsuarioAdmin
    {
        public decimal Id_Usuario { get; set; }
        public string Cod_Usuario { get; set; }
        public string Nom_Usuario { get; set; }
        public string Log_Usuario { get; set; }
        public string E_Mail { get; set; }
        public string Cedula { get; set; }
        public string Departamento { get; set; }
        public string Empresa { get; set; }
        public string Cod_Sap { get; set; }
        public string Cod_Jefe_Inm { get; set; }
        public string MailCodJefeInm { get; set; }
        public long Id_Perfil { get; set; }
        public string NombrePerfil { get; set; }
        public string Usuario_Estado { get; set; }
    }
}
