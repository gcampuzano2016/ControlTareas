namespace CapaEntidad
{
    /// <summary>
    /// Un candidato a jefe inmediato: el codigo que se guarda y el nombre que se
    /// muestra.
    ///
    /// R_Usuarios.Cod_Jefe_Inm guarda un Cod_Usuario, asi que el combo tiene que
    /// enviar el codigo aunque la persona elija por nombre. Ver la seccion 3 del
    /// diseno: un texto libre ahi deja la solicitud de vacaciones sin aprobador y
    /// nadie se entera hasta que alguien pide vacaciones.
    /// </summary>
    public class EntPerfilJefe
    {
        public string CodUsuario { get; set; } = "";
        public string Nombre { get; set; } = "";
    }
}
