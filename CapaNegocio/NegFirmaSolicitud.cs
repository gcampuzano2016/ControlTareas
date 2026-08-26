using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class NegFirmaSolicitud
    {
        /// <summary>Prefijo del folio, fijado por Talento Humano en CB-GAP-POL-01.</summary>
        private const string PrefijoFolio = "CB-GAP-REG-";

        public static EntRespuesta Guardar(EntFirmaSolicitud firma, byte[] trazo, string ip, string dispositivo)
        {
            return DaoFirmaSolicitud.Guardar(firma, trazo, ip, dispositivo);
        }

        public static List<EntFirmaSolicitud> Listar(long idVacaciones)
        {
            return DaoFirmaSolicitud.Listar(idVacaciones);
        }

        /// <summary>
        /// Folio y código de verificación de una solicitud.
        ///
        /// Se deriva de IdVacaciones en vez de llevar un contador aparte. La
        /// especificación pide "una secuencia única y consecutiva sin importar si
        /// es permiso o vacaciones, un solo contador para todas las solicitudes",
        /// y eso es exactamente lo que ya es IdVacaciones: identidad única
        /// compartida por los tres tipos. Un contador nuevo solo agregaría algo
        /// que se puede desincronizar.
        ///
        /// Se rellena a dos dígitos porque así lo muestran los ejemplos de la
        /// especificación; con solicitudes de cuatro cifras el número crece solo.
        /// </summary>
        public static string Folio(long idVacaciones)
        {
            return PrefijoFolio + idVacaciones.ToString("00");
        }
    }
}
