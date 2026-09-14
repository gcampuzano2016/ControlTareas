using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>
    /// Lo que devuelve una sola llamada a Sp_RTA_PerfilColaborador. En la fase 1
    /// se llenan cabecera, contacto y emergencia; las listas de las fases 2 y 3
    /// se agregan aca cuando existan.
    /// </summary>
    public class EntPerfilCompleto
    {
        public EntPerfilCabecera Cabecera { get; set; }
        public EntPerfilContacto Contacto { get; set; }
        public List<EntPerfilEmergencia> Emergencia { get; set; }

        /// <summary>
        /// false por defecto: el usuario no se pudo identificar de forma unica
        /// (Cod_Usuario repetido en R_Usuarios) y el procedimiento no devolvio
        /// fila de cabecera. La pantalla debe explicarlo en vez de mostrar
        /// campos vacios sin motivo, igual que ya hace TieneFicha para los
        /// usuarios sin ficha de empleado. Se pone en true solo cuando si hubo
        /// fila de cabecera.
        /// </summary>
        public bool PerfilEncontrado { get; set; }

        public EntPerfilCompleto()
        {
            Cabecera = new EntPerfilCabecera();
            Contacto = new EntPerfilContacto();
            Emergencia = new List<EntPerfilEmergencia>();
        }
    }
}
