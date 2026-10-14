using ReporteTareas.Controles;
using ReporteTareas.clases;
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
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            /* Quien puede entrar lo responde Permisos, el mismo lugar que usa el
               handler; la barrera de verdad es la del handler y esto es cortesia
               para no mostrar una pantalla vacia. */
            if (!Permisos.PuedeEntrar(Context, Pantallas.HorasExtras))
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
            get { return Permisos.PuedeHacer(Context, Acciones.HorasExtrasReabrir); }
        }

        /// <summary>
        /// Solo para MOSTRAR los botones de completar datos y de ajuste de sueldo.
        /// La regla vive en Permisos y la barrera de verdad es la del handler.
        /// </summary>
        protected bool PuedeEditarMaestro
        {
            get { return Permisos.PuedeHacer(Context, Acciones.HorasExtrasEditarMaestro); }
        }
    }
}
