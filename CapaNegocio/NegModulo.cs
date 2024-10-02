using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDato;
using CapaEntidad;

namespace CapaNegocio
{
    public class NegModulo
    {


        public static List<EntModulo> Modulo()
        {
            return DaoModulo.Modulo();
        }

    }
}
