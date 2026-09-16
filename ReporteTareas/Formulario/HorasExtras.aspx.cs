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
    ///
    /// El handler ya rechaza cualquier acción de quien no sea perfil 14 o 18
    /// -no hay fuga de datos sin esta comprobación aquí-, pero sin ella un
    /// usuario que teclee la URL a mano ve la pantalla completa cargarse y
    /// recién le saltan los modales de "no tiene permisos" al pedir cada
    /// dato. Un Response.Redirect en el Page_Load es más honesto: ni siquiera
    /// llega a ver la grilla vacía.
    /// </summary>
    public partial class HorasExtras : System.Web.UI.Page
    {
        private static readonly int[] PerfilesAutorizados = { 14, 18 };

        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            int idPerfil;
            bool tienePermiso = int.TryParse(Convert.ToString(Session["Id_Perfil"]), out idPerfil)
                                 && Array.IndexOf(PerfilesAutorizados, idPerfil) >= 0;

            if (!tienePermiso)
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }

        /// <summary>
        /// Solo para que la pantalla decida si ENSEnA el boton de reabrir. La
        /// barrera de verdad esta en AdministrarHorasExtras.ashx.cs: esto es
        /// cortesia para quien no puede, no una barrera para quien no debe.
        /// </summary>
        protected bool PuedeReabrir
        {
            get
            {
                int idPerfil;
                return int.TryParse(Convert.ToString(Session["Id_Perfil"]), out idPerfil) && idPerfil == 18;
            }
        }
    }
}
