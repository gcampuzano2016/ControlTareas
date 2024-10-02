using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDato;
using CapaEntidad;


namespace CapaNegocio
{
    public class NegInventario
    {
        public static List<EntInventario> RTA_ConsultaInventarioLike(int tipo, string Descripcion)
        {
            return DaoInventario.RTA_ConsultaInventarioLike(tipo, Descripcion);
        }

        public static EntRespuesta RTA_ConsultaInventarioLikeDescargar(int tipo, string Descripcion)
        {
            return DaoInventario.RTA_ConsultaInventarioLikeDescargar(tipo, Descripcion);
        }
        public static EntRespuesta RTAInsertaNuevoInventario(EntInventario objInventario)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta = DaoInventario.RTAInsertaNuevoInventario(objInventario);
            return respuesta;
        }
    }
}
