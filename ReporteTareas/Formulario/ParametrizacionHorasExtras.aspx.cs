using ReporteTareas.Controles;
using ReporteTareas.clases;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Parametros de calculo de horas extras. Hasta la fase 4, cambiar un
    /// factor era un UPDATE a mano contra HE_Parametro: sin historial, sin
    /// quien lo hizo, y pisando el valor con el que ya se habian pagado
    /// periodos cerrados. Desde esta pantalla cada cambio es una version
    /// nueva con su fecha, y lo viejo sigue ahi.
    ///
    /// No se le pasa ningun identificador de usuario a la pagina: el handler
    /// toma la identidad de la sesion, siempre.
    ///
    /// La comprobacion de perfil de aqui es cortesia visual. La de verdad
    /// esta en AdministrarParametrosHE.ashx.cs, que es el unico camino por el
    /// que se leen y se escriben estos valores; sin esta, quien teclee la URL
    /// a mano ve la pantalla entera cargarse y recien le saltan los modales
    /// de "no tiene permisos" al pedir cada dato. Un Response.Redirect es mas
    /// honesto: ni siquiera llega a ver la tabla vacia.
    /// </summary>
    public partial class ParametrizacionHorasExtras : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            /* Quien puede entrar lo responde Permisos, el mismo lugar que usa el
               handler; la barrera de verdad es la del handler y esto es cortesia
               para no mostrar una pantalla vacia. */
            if (!Permisos.PuedeEntrar(Context, Pantallas.ParametrizacionHorasExtras))
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }
    }
}
