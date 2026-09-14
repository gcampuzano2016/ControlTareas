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

        public EntPerfilCompleto()
        {
            Cabecera = new EntPerfilCabecera();
            Contacto = new EntPerfilContacto();
            Emergencia = new List<EntPerfilEmergencia>();
        }
    }
}
