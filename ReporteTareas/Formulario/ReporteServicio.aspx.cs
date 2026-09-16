using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    public partial class ReporteServicio : System.Web.UI.Page
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


                    }
                    catch (Exception ex)
                    { }
                }
            }
        }
    }
}