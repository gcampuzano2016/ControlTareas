using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class NegProvinciasCiudades
    {        
        public static List<EntProvinciaCiudad> Sp_RTAConsultarProvinciasCiudades(int op, string provincia)
        {
            return DaoProvinciasCiudades.ObtenerProvinciasCiudades(op, provincia);
        }
    }
}
