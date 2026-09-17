using ReporteTareas.Controles;
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

            /* La lista de perfiles vive en UN solo sitio -el handler- y esta
               pagina la reusa. Tenerla dos veces no seria un riesgo de
               seguridad -la barrera de verdad es la del handler- sino de
               mantenimiento callado: anadir un perfil alli y olvidarlo aqui
               deja a esa gente con permiso para llamar al handler y sin poder
               abrir la pantalla, y nadie relaciona una cosa con la otra. */
            int idPerfil;
            bool tienePermiso = int.TryParse(Convert.ToString(Session["Id_Perfil"]), out idPerfil)
                                 && Array.IndexOf(
                                        JsonJQueryNetHorasExtras.AdministrarParametrosHE.PerfilesAutorizados,
                                        idPerfil) >= 0;

            if (!tienePermiso)
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }
    }
}
