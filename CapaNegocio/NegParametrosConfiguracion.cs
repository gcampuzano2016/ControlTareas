using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDato;
using CapaEntidad;

namespace CapaNegocio
{
    public class NegParametrosConfiguracion
    {


        public static EntParametrosConfiguracion RTA_ConsultaParametroConfiguracion(String NombreParametro)
        {
            EntParametrosConfiguracion respuesta = new EntParametrosConfiguracion();
            //var respuesta = "";

            respuesta = DaoParametrosConfiguracion.RTA_ConsultaParametroConfiguracion(NombreParametro);

            return respuesta;
        }

        
        public static string RTA_ValorParametroConfiguracion(String NombreParametro)
        {
            string respuesta = "";

            respuesta = DaoParametrosConfiguracion.RTA_ValorParametroConfiguracion(NombreParametro);

            return respuesta;
        }

    }
}
