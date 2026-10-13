using CapaDato;
using CapaEntidad;
using System.Collections.Generic;

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
