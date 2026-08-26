using System;

namespace CapaEntidad
{
    public class EntPerfiles
    {
        //VERSION NUEVA 
        public Int32 IdPerfil { get; set; }
        public Int32 Codigo { get; set; }
        public String NombrePerfil { get; set; }
        public Int32 Estado { get; set; }
        public DateTime Fecha { get; set; }

        public Int32 IdUsuario { get; set; }

        public String NombreUsuario { get; set; }
        public String Mail { get; set; }
        public String StrEstado { get; set; }
        public String Combo { get; set; }

        // Cantidad de usuarios que tienen este perfil. La usa la pantalla de
        // administracion para saber si el perfil se puede eliminar.
        public Int32 Usuarios { get; set; }

    }
}
