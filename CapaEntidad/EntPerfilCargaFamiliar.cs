namespace CapaEntidad
{
    /// <summary>
    /// Una carga familiar. Cuelga de Cod_Usuario y no de IdEmpleado, que es
    /// nullable: 119 de 231 usuarios no tienen ficha de empleado enlazada y
    /// tienen que poder registrar sus cargas igual.
    /// </summary>
    public class EntPerfilCargaFamiliar
    {
        public int IdCargaFam { get; set; }
        public string Nombre { get; set; } = "";
        public string Parentesco { get; set; } = "";

        /// <summary>Texto "yyyy-MM-dd", que es lo que produce un input type="date".</summary>
        public string FechaNacimiento { get; set; } = "";
    }
}
