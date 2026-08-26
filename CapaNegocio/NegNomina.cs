using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
   public class NegNomina
    {
        public static EntRespuesta InsertarModificarEliminarRegistroNomina(EntNomina nomina)
        {
            return DaoNomina.InsertarModificarEliminarRegistroNomina(nomina);
        }
    }
}
