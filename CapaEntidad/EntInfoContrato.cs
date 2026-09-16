using System;

namespace CapaEntidad
{
    public class EntInfoContrato
    {
        public int opcion { get; set; }
        //public Int32 IdGerenteCuenta { get; set; }
        //public Int32 IdGestorResponsable { get; set; }
        public Int32 IdContrato { get; set; }
        public Int32 conteoArchivosAdjuntos { get; set; }
        public string FECHA { get; set; }
        public string CLIENTE { get; set; }
        public string CLI_NOMBRE { get; set; }
        public string CLI_TELEFONO { get; set; }
        public string CLI_DIRECCION { get; set; }
        public string CLI_CORREO { get; set; }
        public string NUM_CONTRATO { get; set; }
        public string NUM_PEDIDO { get; set; }
        public string OBJETO { get; set; }
        public decimal VALOR_TOTAL_CONTRATO { get; set; }
        public decimal MARGEN { get; set; }
        public string ALCANCE { get; set; }
        public string HARDWARE { get; set; }
        public string LICENCIAS { get; set; }
        public string SERVICIOS_FABRICANTE { get; set; }
        public string SERVICIO_DOS { get; set; }
        public string SERVICIO_EXTERNOS { get; set; }
        public string POLIZAS { get; set; }
        public string TERMINOS_TDR { get; set; }
        public string ACTA_PREGUNTAS { get; set; }
        public string ACTA_ADJUDICACION { get; set; }
        public string ACTA_NEGOCIACION { get; set; }
        public string BOM_SOLUCION { get; set; }
        public string ACUERDOS_MAY { get; set; }
        public string ACUERDOS_FAB { get; set; }
        public string FORMA_PAGO { get; set; }
        public string GARANTIAS_FIN { get; set; }
        public string GARANTIAS_TEC { get; set; }
        public string GENERACION_PEDIDOS { get; set; }
        public string FECHA_SUSCRIPCION_CONTRATO { get; set; }
        public string FECHA_NOTIF_ANTICIPO { get; set; }
        public string FECHA_INICIO_GARANTIA { get; set; }
        public string FECHA_FIN_GARANTIA { get; set; }
        public string PLAZO_ACTIVACION { get; set; }
        public string PLAZO_ACTIVACION_LIC { get; set; }
        public string DURACION_VIGENCIA_TEC { get; set; }
        public string ENTREGA_LIC_TEMPORALES { get; set; }
        public string ORDEN_SERVICIO { get; set; }
        public string PERSONAL { get; set; }
        public string ITEMS { get; set; }
        public string CORREOS_NOTIFICACION { get; set; }


        //--------------------------------------------
        public string OBS_CLIENTE { get; set; }
        //public string OBS_REFERENCIA_CLIENTE { get; set; }       
        public string OBS_NUM_CONTRATO { get; set; }
        public string OBS_VALOR_TOTAL { get; set; }
        public string OBS_OBJETO { get; set; }
        public string OBS_ALCANCE { get; set; }
        public string OBS_HARDWARE { get; set; }
        public string OBS_LICENCIAS { get; set; }
        public string OBS_SERVICIOS_FABRICANTE { get; set; }
        public string OBS_SERVICIO_DOS { get; set; }
        public string OBS_SERVICIO_EXTERNOS { get; set; }
        public string OBS_POLIZAS { get; set; }
        public string OBS_TERMINOS_TDR { get; set; }
        public string OBS_ACTA_PREGUNTAS { get; set; }
        public string OBS_ACTA_ADJUDICACION { get; set; }
        public string OBS_ACTA_NEGOCIACION { get; set; }
        public string OBS_BOM_SOLUCION { get; set; }
        public string OBS_ACUERDOS_MAY { get; set; }
        public string OBS_ACUERDOS_FAB { get; set; }
        public string OBS_FORMA_PAGO { get; set; }
        public string OBS_GARANTIAS_FIN { get; set; }
        public string OBS_GARANTIAS_TEC { get; set; }
        public string OBS_GENERACION_PEDIDOS { get; set; }

        //----------------------------------------------------------
        public string CREADOR_CONTRATO { get; set; }
        public string COD_USUARIO { get; set; }
        public string PERMISOS { get; set; }
        public string OBSERVACIONES { get; set; }
        public string FECHA_CREACION { get; set; }

    }
}
