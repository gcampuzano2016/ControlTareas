using System;

namespace ReporteTareas.Formulario
{
    public partial class ParametrizacionPerfiles : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // La pantalla se alimenta por AJAX desde AdministrarPerfiles.ashx.
            // La sesión la valida ese handler en cada llamada.
        }
    }
}
