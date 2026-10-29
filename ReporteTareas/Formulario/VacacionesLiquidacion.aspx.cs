using ReporteTareas.clases;
using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Liquidacion de vacaciones por periodo (GTH). Muestra sueldos: la barrera
    /// de verdad esta en AdministrarVacLiquidacion.ashx; esto evita mostrar una
    /// pantalla vacia a quien no la tiene en su menu.
    /// </summary>
    public partial class VacacionesLiquidacion : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            if (!Permisos.PuedeEntrar(Context, Pantallas.VacacionesLiquidacion))
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }
    }
}
