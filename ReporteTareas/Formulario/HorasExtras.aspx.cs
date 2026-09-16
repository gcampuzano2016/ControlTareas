using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Cálculo de horas extras 50% y 100%. Reemplaza el registro que hasta
    /// ahora llevaba Nómina en un Excel.
    ///
    /// No pasa ningún identificador de usuario a la página: el handler toma
    /// la identidad de la sesión, siempre.
    /// </summary>
    public partial class HorasExtras : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);
        }
    }
}
