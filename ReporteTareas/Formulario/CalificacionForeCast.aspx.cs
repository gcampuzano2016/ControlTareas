using ReporteTareas.Controles;
using SeguridadAppHelper;
using System;

namespace ReporteTareas.Formulario
{
    public partial class CalificacionForeCast : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            if (Session["UserLogin"] != null && !IsPostBack)
            {
                try
                {
                    SeguridadHelper seguridad = new SeguridadHelper();
                    txtUsuario.Text = seguridad.Encripta(Session["Cod_Usuario"].ToString());
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
