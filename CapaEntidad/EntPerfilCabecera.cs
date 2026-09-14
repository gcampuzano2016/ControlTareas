namespace CapaEntidad
{
    /// <summary>
    /// Los datos que RRHH administra. El colaborador los ve y no los edita.
    /// </summary>
    public class EntPerfilCabecera
    {
        /* Todas las propiedades string arrancan en "" y no en null: es el mismo
           valor por defecto que produce Texto() en el camino normal. Sin esto,
           un perfil sin fila de cabecera (usuario con Cod_Usuario repetido en
           R_Usuarios, vease PerfilEncontrado en EntPerfilCompleto) quedaria con
           propiedades en null y cualquier .Trim() o .ToUpper() de la pantalla
           reventaria solo por ese camino. */
        public string CodUsuario { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string Cedula { get; set; } = "";

        /// <summary>
        /// Tal como esta en la base: texto dd/MM/yyyy. No se convierte en SQL a
        /// proposito; la convierte NegPerfilCampos.EdadDesdeTexto.
        /// </summary>
        public string FechaNacTexto { get; set; } = "";

        /// <summary>Anios cumplidos, o null si la fecha falta o no es valida.</summary>
        public int? Edad { get; set; }

        public string Cargo { get; set; } = "";
        public string Area { get; set; } = "";
        public string Ciudad { get; set; } = "";
        public string CorreoNotificacion { get; set; } = "";
        public string JefeInmediato { get; set; } = "";

        /// <summary>
        /// Nombre del horario vigente, o vacio para los 143 usuarios que no
        /// tienen ninguno asignado.
        /// </summary>
        public string Horario { get; set; } = "";

        /// <summary>
        /// false para los 113 usuarios sin ficha de empleado enlazada. La
        /// pantalla lo usa para explicar por que faltan datos en vez de mostrar
        /// campos vacios sin motivo aparente.
        /// </summary>
        public bool TieneFicha { get; set; }

        /// <summary>Tiene al menos un subordinado directo (habilita la fase 3).</summary>
        public bool EsJefe { get; set; }
    }
}
