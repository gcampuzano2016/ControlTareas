using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Tablero de Costos. No pasa ningun identificador a la pagina: el handler
    /// toma la identidad y el perfil de la sesion, siempre.
    ///
    /// El handler ya rechaza a quien no sea perfil 14 o 18. Este redirect evita
    /// que alguien que escriba la URL a mano vea la pantalla vacia cargarse y
    /// reciba el error recien al pedir los datos. La verificacion es la del
    /// handler (AdministrarTableroCostos.Autorizado: sesion con usuario y
    /// perfil autorizado), no una copia.
    /// </summary>
    public partial class TableroCostos : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            if (!JsonJQueryNetTableroCostos.AdministrarTableroCostos.Autorizado(Context))
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }
    }
}
