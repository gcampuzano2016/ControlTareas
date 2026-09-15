using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>
    /// Lo que devuelve una sola llamada a Sp_RTA_PerfilEquipo: seis result sets
    /// en este orden -cabecera, emergencia, estudios, certificaciones,
    /// experiencia, foto-.
    ///
    /// Las listas reusan las entidades del perfil propio porque son el mismo
    /// dato. La cabecera NO: esa es recortada y tiene su propia clase, que es
    /// donde vive la restriccion. Ver EntPerfilEquipoCabecera.
    /// </summary>
    public class EntPerfilEquipo
    {
        public EntPerfilEquipoCabecera Cabecera { get; set; }
        public List<EntPerfilEmergencia> Emergencia { get; set; }
        public List<EntPerfilEstudio> Estudios { get; set; }
        public List<EntPerfilCertificacion> Certificaciones { get; set; }
        public List<EntPerfilExperiencia> Experiencia { get; set; }
        public EntPerfilFoto Foto { get; set; }

        /// <summary>
        /// false cuando el procedimiento no devolvio cabecera. Son dos casos y
        /// se tratan igual a proposito: que esa persona no le reporte a quien
        /// pregunta, o que alguno de los dos codigos de usuario este repetido.
        /// Distinguirlos en la respuesta le confirmaria a quien esta probando
        /// codigos cual existe.
        /// </summary>
        public bool PerfilEncontrado { get; set; }

        public EntPerfilEquipo()
        {
            Cabecera = new EntPerfilEquipoCabecera();
            Emergencia = new List<EntPerfilEmergencia>();
            Estudios = new List<EntPerfilEstudio>();
            Certificaciones = new List<EntPerfilCertificacion>();
            Experiencia = new List<EntPerfilExperiencia>();
            Foto = new EntPerfilFoto();
        }
    }
}
