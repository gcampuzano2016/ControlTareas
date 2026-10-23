using ReporteTareas.clases;
using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Procesos entre areas. La barrera de verdad esta en AdministrarAgenda.ashx;
    /// esto evita mostrar una pantalla vacia a quien no la tiene en su menu.
    /// </summary>
    public partial class Procesos : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            if (!Permisos.PuedeEntrar(Context, Pantallas.Procesos))
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }
    }
}
