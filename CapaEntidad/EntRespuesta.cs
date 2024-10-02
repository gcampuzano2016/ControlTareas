using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Data;

namespace CapaEntidad
{
    //
    // Clase estandar para devolver los resultados de una método
    //
    public class EntRespuesta
    {
        // Codigo del estado de la respuesta
        // 0 - Error, 1 - Exito
        public string estado { get; set; }
        // Contendra el resultado de la consulta, puede ser: arreglo para grid.
        public dynamic resultado { get; set; } 
        // Mensaje que se muestra al usaurio, puede ser de exito o error dependiendo del estado
        public string mensaje { get; set; }
        // Tipo de mensaje que permitira indicar si el mensaje a mostrarse es confirmación, warning, alerta, informativo
        // valores permitidos: success, info, warning, danger 
        public string tipoMensaje { get; set; }
        // Determina el codigo de error en caso de producirse y existir para evitar mostrar mensajes de error técnicos
        public string codigoError { get; set; }
        // Contendra el resultado de la consulta cuando retorna un datatable.
        public DataTable resultadoTabla { get; set; }

        public string rutapdf { get; set; }
        public string Archivo { get; set; }

    }
}
