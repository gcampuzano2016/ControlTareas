using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using CapaEntidad;


namespace WebServiceTareas
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "IServiceTareas" en el código y en el archivo de configuración a la vez.
    [ServiceContract]
    public interface IServiceTareas
    {
        [XmlSerializerFormat(Style = OperationFormatStyle.Document), OperationContract]
        string BuscarTarea(int IdUsuario);

        [XmlSerializerFormat(Style = OperationFormatStyle.Document), OperationContract]
        string ConsultarVacaciones(int codSap);

    }
}
