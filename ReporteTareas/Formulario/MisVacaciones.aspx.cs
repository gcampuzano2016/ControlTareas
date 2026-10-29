using ReporteTareas.clases;
using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Mis vacaciones: el saldo propio. La barrera de verdad esta en
    /// AdministrarMisVacaciones.ashx, que ademas solo devuelve el saldo del
    /// usuario de la sesion.
    /// </summary>
    public partial class MisVacaciones : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            if (!Permisos.PuedeEntrar(Context, Pantallas.MisVacaciones))
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }
    }
}
