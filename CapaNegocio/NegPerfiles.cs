using CapaDato;
using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class  NegPerfiles
    {
        public static EntRespuesta RTAInsertarNuevoPerfil(EntPerfiles objPerfil)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta = DaoPerfiles.RTAInsertarNuevoPerfil(objPerfil);
            return respuesta;
        }

        public static EntRespuesta Sp_RTActualizarPerfil(EntPerfiles objPerfil)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta = DaoPerfiles.Sp_RTActualizarPerfil(objPerfil);
            return respuesta;
        }

        public static List<EntCombo> RTAListaComboContrato(Int32 Tipo, string Cod_Usuario, Int32 IdSucursal, Int32 IdCliente, string IdSucursalGerente)
        {
            return DaoPerfiles.RTAListaComboContrato(Tipo, Cod_Usuario, IdSucursal, IdCliente, IdSucursalGerente);
        }

        public static List<EntPerfiles> Sp_RTA_ConsultarUsuariosPerfil(int tipo)
        {
            return DaoPerfiles.Sp_RTA_ConsultarUsuariosPerfil(tipo);
        }


    }

   
}
