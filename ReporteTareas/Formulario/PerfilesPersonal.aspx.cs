using CapaNegocio;
using ReporteTareas.Controles;
using System;

namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Perfiles del personal. Talento Humano busca a cualquier empleado y edita
    /// su perfil con las mismas fichas que cada quien usa para el suyo.
    ///
    /// No pasa ningun identificador a la pagina: el codigo de la persona elegida
    /// lo fija el buscador en el cliente y lo valida el handler contra la sesion.
    ///
    /// El handler ya rechaza cualquier accion de quien no sea perfil 14 o 18 -no
    /// hay fuga de datos sin esta comprobacion aqui-, pero sin ella un usuario
    /// que teclee la URL a mano ve la pantalla cargarse entera y recien le salta
    /// el mensaje al buscar. Un Response.Redirect es mas honesto.
    ///
    /// La lista de perfiles NO se escribe aqui: sale de NegPerfilAcceso, que es
    /// donde vive y donde esta probada. Tenerla dos veces no es un riesgo de
    /// seguridad -la barrera de verdad es la del handler- sino de mantenimiento
    /// callado.
    /// </summary>
    public partial class PerfilesPersonal : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            int idPerfil;
            bool tienePermiso = int.TryParse(Convert.ToString(Session["Id_Perfil"]), out idPerfil)
                                 && Array.IndexOf(NegPerfilAcceso.PerfilesRRHH, idPerfil) >= 0;

            if (!tienePermiso)
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }
    }
}
