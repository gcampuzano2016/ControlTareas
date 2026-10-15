namespace CapaNegocio
{
    /// <summary>
    /// Que lista de empleados le corresponde a quien la pide. Clase pura: no
    /// toca base, sesion ni HttpContext; los permisos los decide quien llama.
    ///
    /// Sp_RTAConsultaEmpleados devuelve TODOS los empleados con @Tipo = 0, y con
    /// cualquier otro valor solo la ficha de @Cod_Usuario. Antes el handler
    /// recibia ambos del navegador: cualquiera que mandara tipo=0 se llevaba la
    /// nomina completa (cedula, direccion, telefono, fecha de nacimiento).
    /// Ahora salen de aca, con el usuario de la sesion.
    /// </summary>
    public static class NegEmpleadoAcceso
    {
        /// <summary>@Tipo con el que el procedimiento devuelve todos los empleados.</summary>
        public const int TipoTodos = 0;

        /// <summary>@Tipo con el que el procedimiento devuelve solo la ficha de @Cod_Usuario.</summary>
        public const int TipoPropia = 1;

        /// <summary>
        /// Arma los parametros de Sp_RTAConsultaEmpleados.
        /// - La lista completa: solo si la pide y puede entrar a «Lista de Empleados».
        /// - Si no: su propia ficha, si puede entrar a alguna de las dos pantallas.
        /// Devuelve false si no le corresponde nada.
        /// </summary>
        public static bool ConsultaLista(bool veLista, bool veFicha, bool pideLista, string codUsuarioSesion,
            out string codUsuario, out int tipo)
        {
            codUsuario = "";
            tipo = TipoPropia;

            if (pideLista && veLista)
            {
                tipo = TipoTodos;
                return true;
            }

            if ((veLista || veFicha) && !string.IsNullOrWhiteSpace(codUsuarioSesion))
            {
                codUsuario = codUsuarioSesion;
                return true;
            }

            return false;
        }
    }
}
