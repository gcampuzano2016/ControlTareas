using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using CapaEntidad;
using CapaNegocio;
using Newtonsoft.Json;
using System.Data;

namespace WebServiceTareas
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "ServiceTareas" en el código, en svc y en el archivo de configuración a la vez.
    // NOTA: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione ServiceTareas.svc o ServiceTareas.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class ServiceTareas : IServiceTareas
    {
        #region BuscarTarea
        public string BuscarTarea(int IdUsuario)
        {

            List<EntTareas> listaTareas = new List<EntTareas>();
            string CodUnico = "";
            CodUnico = IdUsuario.ToString(); 
            listaTareas = NegTareas.ListaTareas(CodUnico);

            string json = JsonConvert.SerializeObject(listaTareas, Formatting.Indented);

            return json;
        }
        #endregion

        #region ConsultarVacaciones
        public string ConsultarVacaciones(int codSap)
        {
            bool bandera = true;
            string json = "";
            NegVacaciones negVaca = new NegVacaciones();
            try
            {
                List<EntVacaciones> listaVacaciones = new List<EntVacaciones>();
                EntRespuesta respuesta;
                negVaca.EscribirLog("codSap: " + codSap.ToString(), "EscribirLog", "Log", bandera);
                respuesta = NegVacaciones.ConsultarVacaciones(codSap);
                negVaca.EscribirLog("respuesta.resultadoTabla.Rows.Count: " + respuesta.resultadoTabla.Rows.Count.ToString(), "EscribirLog", "Log", bandera);
                if (respuesta.resultadoTabla.Rows.Count > 0)
                {
                    List<EntVacaciones> listName = respuesta.resultadoTabla.AsEnumerable().Select(m => new EntVacaciones()
                    {
                        PERIODO = m.Field<string>("PERIODO"),
                        DIASGENERADOS = m.Field<double>("DIASGENERADOS"),
                        DIASTOMADOS = m.Field<double>("DIASTOMADOS"),
                        SALDO = m.Field<double>("SALDO"),
                    }).ToList();

                    listaVacaciones = listName;
                    json = JsonConvert.SerializeObject(listaVacaciones, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                negVaca.EscribirLog("ex: " + ex.Message.ToString(), "EscribirLog", "Log", bandera);
            }

            return json;
        }
        #endregion

    }
}
