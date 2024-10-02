using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDato;
using CapaEntidad;

namespace CapaNegocio
{
    public class NegTransaccion
    {

        //public static List<EntTransaccion> ListaTransaccion(Int64 idPerfil, int idModulo, int IdPais)
        public static List<EntTransaccion> ListaTransaccion(int idModulo)
        {
            //return DaoTransaccion.ListaTransaccion(idPerfil, idModulo, IdPais);
            return DaoTransaccion.ListaTransaccion(idModulo);
        }

    }
}
