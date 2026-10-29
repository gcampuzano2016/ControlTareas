using ReporteTareas.clases;
using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Nomina y provision de vacaciones (GTH). Muestra sueldos: la barrera de
    /// verdad esta en AdministrarVacNomina.ashx; esto evita mostrar una
    /// pantalla vacia a quien no la tiene en su menu.
    /// </summary>
    public partial class VacacionesNomina : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            if (!Permisos.PuedeEntrar(Context, Pantallas.VacacionesNomina))
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }
    }
}
