using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

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
