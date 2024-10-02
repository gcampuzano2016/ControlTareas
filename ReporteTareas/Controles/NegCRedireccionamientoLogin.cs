using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReporteTareas.Controles
{
    public class NegCRedireccionamientoLogin
    {
        #region RedireccionarALogin
        public bool RedireccionarALogin(System.Web.UI.Page p)
        {
            //return true;
            if (p.Session["UserLogin"] == null || p.Session["UserLogin"].ToString() == "")
            {
                string strEstadoLogin = "";
                if (!p.IsPostBack)
                    strEstadoLogin = "Termino el tiempo de Sesión, por favor ingrese al sistema nuevamente.";
                else
                    strEstadoLogin = "Termino el tiempo de Sesión, por favor ingrese al sistema nuevamente.";

                p.Response.Redirect("~/Formulario/Login.aspx?strEstadoLogin="
                            + strEstadoLogin, true);
                return false;
            }
            else
            {
                return true;
                //validar en base de datos si el usuario logueado tiene permisos para acceder a esta página
            }
        }
        #endregion
    }
}
