using ReporteTareas.clases;
using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Equipo y agenda. La abre cualquier usuario interno con sesion: lo que ve
    /// cada uno lo decide AdministrarAgenda.ashx. Este redirect solo evita que
    /// un usuario de cliente vea la pantalla vacia.
    /// </summary>
    public partial class Agenda : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            if (!Permisos.Autenticado(Context) || !JsonJQueryNetAgenda.AdministrarAgenda.Interno(Context))
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }
    }
}
