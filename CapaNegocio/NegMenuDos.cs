using CapaEntidad;
using CapaDato;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class NegMenuDos
    {

        public static List<EntMenuDos> Sp_RTA_ConsultarUsuariosPerfil(int tipo)
        {
            return DaoMenuDos.Sp_RTA_ConsultarMenuDos(tipo);
        }

        public static List<EntMenuDos> Sp_RTA_ConsultarMenuPerfilUsuario(int tipo)
        {
            return DaoMenuDos.Sp_RTA_ConsultarMenuPerfilUsuario(tipo);
        }

        public static EntRespuesta Sp_RTA_InsertarMenuNuevo(int tipoMenu, String Titulo, String Descripcion, String Icono, String Referencia,int MenuPadre)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta= DaoMenuDos.Sp_RTA_InsertarMenuNuevo(tipoMenu, Titulo, Descripcion, Icono,Referencia, MenuPadre);
            return respuesta;
        }

        public static List<EntMenuDos> Sp_RTA_ConsultarMenuPerfil(int tipo)
        {
            return DaoMenuDos.Sp_RTA_ConsultarMenuPerfil(tipo);
        }

        public static EntRespuesta Sp_RTActualizarPerfilMenu(EntMenuDos objPerfil)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta = DaoMenuDos.Sp_RTActualizarPerfilMenu(objPerfil);
            return respuesta;
        }

        public static EntRespuesta Sp_RTActualizarIconoMenu(EntMenuDos objPerfil)
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta = DaoMenuDos.Sp_RTActualizarIconoMenu(objPerfil);
            return respuesta;
        }
    }
}
