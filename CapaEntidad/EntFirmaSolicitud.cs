using System;

namespace CapaEntidad
{
    /// <summary>
    /// Una firma sobre una solicitud de vacaciones o permiso.
    ///
    /// La identidad del firmante (nombre, cargo, cédula) viaja copiada en esta
    /// entidad y no se resuelve contra R_Usuarios al leerla: el documento que
    /// alguien firmó tiene que seguir diciendo lo que decía cuando lo firmó.
    /// </summary>
    public class EntFirmaSolicitud
    {
        public int IdFirma { get; set; }
        public int IdVacaciones { get; set; }

        /// <summary>COLABORADOR, JEFE o GTH.</summary>
        public string Rol { get; set; }

        /// <summary>1 salvo la reconfirmación del jefe cuando hubo recuperación.</summary>
        public int Secuencia { get; set; }

        /// <summary>APROBADO o RECHAZADO.</summary>
        public string Decision { get; set; }
        public string Comentario { get; set; }

        /// <summary>
        /// El trazo en base64, listo para el src de un img. Va así y no como
        /// byte[] porque de aquí sale directo al HTML que se convierte a PDF.
        /// </summary>
        public string TrazoBase64 { get; set; }
        public string TrazoTipo { get; set; }

        public string Cod_Usuario { get; set; }
        public string Nombre { get; set; }
        public string Cargo { get; set; }
        public string Cedula { get; set; }

        public DateTime FechaFirma { get; set; }
        public string Ip { get; set; }
        public string Dispositivo { get; set; }

        /// <summary>El trazo como data URI, para incrustarlo en el HTML del PDF.</summary>
        public string TrazoDataUri
        {
            get
            {
                if (string.IsNullOrEmpty(TrazoBase64)) { return string.Empty; }
                string tipo = string.IsNullOrEmpty(TrazoTipo) ? "image/png" : TrazoTipo;
                return "data:" + tipo + ";base64," + TrazoBase64;
            }
        }
    }
}
