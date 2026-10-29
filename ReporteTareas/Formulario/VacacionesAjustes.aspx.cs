using ReporteTareas.clases;
using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Ajustes de vacaciones (GTH): saldo de corte y compra, con bitacora. La
    /// barrera de verdad esta en AdministrarVacAjustes.ashx; esto evita
    /// mostrar una pantalla vacia a quien no la tiene en su menu.
    /// </summary>
    public partial class VacacionesAjustes : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            if (!Permisos.PuedeEntrar(Context, Pantallas.VacacionesAjustes))
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }
    }
}
