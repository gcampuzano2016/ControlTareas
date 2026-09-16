using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    public partial class ParametrizacionPerfiles : System.Web.UI.Page
    {
        #region Variables
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            // La pantalla se alimenta por AJAX desde AdministrarPerfiles.ashx.
            // La sesión la valida ese handler en cada llamada.
        }
    }
}
