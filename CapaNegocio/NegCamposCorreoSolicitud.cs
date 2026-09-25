using System.Collections.Generic;
using CapaEntidad;

namespace CapaNegocio
{
    /// <summary>
    /// Los campos que se reemplazan en la plantilla del correo al jefe
    /// ([tituloNotificacion], [texto1], [urlBoton1]...).
    ///
    /// Estaban escritos a mano dentro de GuardarNuevaPermisoBase y de
    /// GuardarNuevaVacacionesBase. Se sacaron aca para que el reenvio mande
    /// exactamente lo mismo que el envio original, sin una tercera copia. Se
    /// conservan tal cual, incluido el "Jefe Inmedianto:" que ya viaja asi.
    /// </summary>
    public class NegCamposCorreoSolicitud
    {
        public static List<EntItemValor> Permiso(EntSolicitud s, string urlAprobacion, string urlRechazo)
        {
            List<EntItemValor> campos = new List<EntItemValor>();
            Agregar(campos, "tituloNotificacion", "SOLICITUD DE PERMISO");
            Agregar(campos, "etiqueta1", "Fecha:");
            Agregar(campos, "texto1", s.FechaRegistro);
            Agregar(campos, "etiqueta11", "Cédula:");
            Agregar(campos, "texto11", s.Cedula);
            Agregar(campos, "etiqueta2", "Colaborador:");
            Agregar(campos, "texto2", s.Colaborador);
            Agregar(campos, "etiqueta21", "Departamento:");
            Agregar(campos, "texto21", s.Departamento);
            Agregar(campos, "etiqueta3", "Jefe Inmedianto:");
            Agregar(campos, "texto3", s.JefeInmediato);
            Agregar(campos, "etiqueta4", "Actividad a Realizar:");
            Agregar(campos, "texto4", s.Actividad);
            Agregar(campos, "texto22", "Horas de Permiso");
            Agregar(campos, "etiqueta23", "Desde:");
            Agregar(campos, "texto23", s.FechaDesde);
            Agregar(campos, "etiqueta25", "Hasta:");
            Agregar(campos, "texto25", s.FechaHasta);
            Agregar(campos, "etiqueta26", "Total de Horas:");
            Agregar(campos, "texto26", s.Horas);
            Agregar(campos, "etiqueta27", "Cargo a vacaciones:");
            Agregar(campos, "texto27", s.StrCargoVacaciones);
            Agregar(campos, "etiqueta28", "Observación:");
            Agregar(campos, "texto28", s.Observacion);
            AgregarBotones(campos, urlAprobacion, urlRechazo);
            return campos;
        }

        /// <summary>
        /// Vacaciones (tipo 2) y planificacion (tipo 3) comparten plantilla; solo
        /// cambia el titulo. Con otro tipo el titulo no se agrega, igual que antes.
        /// </summary>
        public static List<EntItemValor> Vacaciones(EntSolicitud s, long tipoSolicitud, string urlAprobacion, string urlRechazo)
        {
            List<EntItemValor> campos = new List<EntItemValor>();
            if (tipoSolicitud == 2)
            {
                Agregar(campos, "tituloNotificacion", "SOLICITUD DE VACACIONES");
            }
            else if (tipoSolicitud == 3)
            {
                Agregar(campos, "tituloNotificacion", "SOLICITUD DE PLANIFICACIÓN DE VACACIONES");
            }
            Agregar(campos, "etiqueta1", "Fecha:");
            Agregar(campos, "texto1", s.FechaRegistro);
            Agregar(campos, "etiqueta11", "Cédula:");
            Agregar(campos, "texto11", s.Cedula);
            Agregar(campos, "etiqueta2", "Colaborador:");
            Agregar(campos, "texto2", s.Colaborador);
            Agregar(campos, "etiqueta21", "Departamento:");
            Agregar(campos, "texto21", s.Departamento);
            Agregar(campos, "etiqueta3", "Jefe Inmedianto:");
            Agregar(campos, "texto3", s.JefeInmediato);
            Agregar(campos, "etiqueta4", "Reemplazo:");
            Agregar(campos, "texto4", s.Remplazo);
            Agregar(campos, "texto22", "Dias de Vacaciones");
            Agregar(campos, "etiqueta23", "Desde:");
            Agregar(campos, "texto23", s.FechaDesde);
            Agregar(campos, "etiqueta25", "Hasta:");
            Agregar(campos, "texto25", s.FechaHasta);
            Agregar(campos, "etiqueta26", "Días Solicitados:");
            Agregar(campos, "texto26", s.TotalDias.ToString());
            Agregar(campos, "etiqueta27", "Saldo de días:");
            Agregar(campos, "texto27", s.SaldoDias.ToString());
            AgregarBotones(campos, urlAprobacion, urlRechazo);
            return campos;
        }

        private static void AgregarBotones(List<EntItemValor> campos, string urlAprobacion, string urlRechazo)
        {
            Agregar(campos, "etiquetaBoton1", "Aprobar");
            Agregar(campos, "urlBoton1", urlAprobacion);
            Agregar(campos, "etiquetaBoton2", "Rechazar");
            Agregar(campos, "urlBoton2", urlRechazo);
        }

        private static void Agregar(List<EntItemValor> campos, string item, string valor)
        {
            campos.Add(new EntItemValor() { Item = item, Valor = valor });
        }
    }
}
