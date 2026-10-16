using ReporteTareas.clases;
using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Catalogo de areas de la agenda y area de cada persona. La barrera de
    /// verdad esta en AdministrarAgenda.ashx; esto evita mostrar una pantalla
    /// vacia a quien no la tiene en su menu.
    /// </summary>
    public partial class ParametrizacionAreasAgenda : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            if (!Permisos.PuedeEntrar(Context, Pantallas.ParametrizacionAreasAgenda))
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }
    }
}
