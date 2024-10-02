using System;
using System.Collections.Generic;
using CapaDato;
using CapaEntidad;
using System.Data;

namespace CapaNegocio
{
    public class NegMenu
    {


        public static EntRespuesta ConsultaSubmenus(string IdUsuario)
        {
            EntRespuesta respuesta = new EntRespuesta();

            respuesta = DaoMenu.RTA_ConsultaSubmenus(IdUsuario);

            return respuesta;
        }


    }
}
