using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Mi perfil. Es la unica pantalla del sistema a la que entra cualquiera que
    /// haya iniciado sesion: no se registra en MenuDos ni en PerfilMenu porque
    /// se llega por el desplegable de usuario, y el perfil propio le corresponde
    /// a todos.
    ///
    /// No pasa el Cod_Usuario a la pagina: el handler lo toma de la sesion.
    /// </summary>
    public partial class MiPerfil : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);
        }
    }
}
