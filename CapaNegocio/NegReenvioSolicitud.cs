using CapaEntidad;

namespace CapaNegocio
{
    /// <summary>
    /// Reglas para reenviarle al jefe el correo de aprobacion de una solicitud.
    ///
    /// Existe porque el correo se manda una sola vez, al guardar. Si ese envio
    /// falla -paso desde el 18-09-2026, con los binarios de capa desparejos- la
    /// solicitud queda POR APROBAR y el jefe nunca se entera, y no habia forma de
    /// volver a mandarlo.
    ///
    /// El servidor no confia en que el boton este oculto: solo reenvia la
    /// solicitud propia y pendiente, porque despues de que el jefe decidio el
    /// enlace de aprobar ya no tiene sentido.
    /// </summary>
    public class NegReenvioSolicitud
    {
        public const string EstadoPendiente = "POR APROBAR";

        /// <summary>
        /// Por que no se puede reenviar, o null si se puede.
        /// </summary>
        public static string MotivoParaNoReenviar(EntSolicitud solicitud, string codUsuarioSesion)
        {
            if (solicitud == null || solicitud.IdVacaciones <= 0)
            {
                return "No se encontró la solicitud.";
            }

            string duenio = (solicitud.Cod_Usuario ?? "").Trim();
            string sesion = (codUsuarioSesion ?? "").Trim();
            if (sesion.Length == 0 || duenio != sesion)
            {
                return "Solo puede reenviar el correo de sus propias solicitudes.";
            }

            if ((solicitud.EstadoSolicitud ?? "").Trim().ToUpperInvariant() != EstadoPendiente)
            {
                return "La solicitud ya no está pendiente de aprobación (estado: " + solicitud.EstadoSolicitud + "), no hace falta reenviarla.";
            }

            return null;
        }

        public static string Asunto(long idTipoSolicitud)
        {
            switch (idTipoSolicitud)
            {
                case 1: return "Solicitud de Permiso (Reenvío)";
                case 3: return "Solicitud de Planificación de Vacaciones (Reenvío)";
                default: return "Solicitud de Vacaciones (Reenvío)";
            }
        }

        /// <summary>
        /// El quinto valor que viaja cifrado en los enlaces de aprobar y
        /// rechazar. El guardado del permiso pone "NO" y el de vacaciones "EM".
        /// </summary>
        public static string MarcaDelEnlace(long idTipoSolicitud)
        {
            return idTipoSolicitud == 1 ? "NO" : "EM";
        }
    }
}
