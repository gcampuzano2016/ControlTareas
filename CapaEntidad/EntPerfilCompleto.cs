using System.Collections.Generic;

namespace CapaEntidad
{
    /// <summary>
    /// Lo que devuelve una sola llamada a Sp_RTA_PerfilColaborador. La fase 1
    /// llena cabecera, contacto y emergencia; la fase 2 agrego Estudios,
    /// Certificaciones, Experiencia y CargasFamiliares; la fase 3a agrego
    /// Documentos (el septimo conjunto, que hasta ahora se saltaba) y Foto
    /// (el noveno, nuevo).
    /// </summary>
    public class EntPerfilCompleto
    {
        public EntPerfilCabecera Cabecera { get; set; }
        public EntPerfilContacto Contacto { get; set; }
        public List<EntPerfilEmergencia> Emergencia { get; set; }
        public List<EntPerfilEstudio> Estudios { get; set; }
        public List<EntPerfilCertificacion> Certificaciones { get; set; }
        public List<EntPerfilExperiencia> Experiencia { get; set; }

        /// <summary>
        /// Cuelgan de Cod_Usuario, no de la ficha de empleado: los 119 usuarios
        /// sin ficha tambien las registran.
        /// </summary>
        public List<EntPerfilCargaFamiliar> CargasFamiliares { get; set; }

        /// <summary>
        /// Los respaldos de todas las certificaciones y cargas familiares en una
        /// sola lista. La pantalla la reparte por Origen e IdOrigen; asi una sola
        /// consulta sirve a las dos pestanias.
        /// </summary>
        public List<EntPerfilDocumento> Documentos { get; set; }

        /// <summary>La foto. DataUri vacia cuando la persona no subio ninguna.</summary>
        public EntPerfilFoto Foto { get; set; }

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
            Estudios = new List<EntPerfilEstudio>();
            Certificaciones = new List<EntPerfilCertificacion>();
            Experiencia = new List<EntPerfilExperiencia>();
            CargasFamiliares = new List<EntPerfilCargaFamiliar>();
            Documentos = new List<EntPerfilDocumento>();
            Foto = new EntPerfilFoto();
        }
    }
}
