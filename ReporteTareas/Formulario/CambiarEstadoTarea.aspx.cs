using CapaEntidad;
using ReporteTareas.Controles;
using SeguridadAppHelper;
using System;
using System.Collections.Generic;

namespace ReporteTareas.Formulario
{
    public partial class CambiarEstadoTarea : System.Web.UI.Page
    {
        #region Variables
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);
            if (Session["UserLogin"] != null)
            {
                if (!IsPostBack)
                {
                    try
                    {
                        List<EntTareas> listaTareas = new List<EntTareas>();
                        string CodUnico = "";
                        CodUnico = Session["Cod_Usuario"].ToString();
                        SeguridadHelper seguridad = new SeguridadHelper();
                        txtUsuario.Text = seguridad.Encripta(CodUnico.ToString());

                    }
                    catch (Exception ex)
                    { }
                }
            }

        }


    }
}