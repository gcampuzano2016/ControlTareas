using CapaNegocio;
using ReporteTareas.clases;
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
    /// El handler ya rechaza cualquier accion de quien no tenga esta pantalla -no
    /// hay fuga de datos sin esta comprobacion aqui-, pero sin ella un usuario
    /// que teclee la URL a mano ve la pantalla cargarse entera y recien le salta
    /// el mensaje al buscar. Un Response.Redirect es mas honesto.
    ///
    /// Quien entra no se escribe aqui: lo decide el portal, con el permiso de
    /// esta pantalla, y el handler pregunta lo mismo a traves de PerfilIdentidad.
    /// </summary>
    public partial class PerfilesPersonal : System.Web.UI.Page
    {
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            if (!Permisos.PuedeEntrar(Context, Pantallas.PerfilesPersonal))
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
        }
    }
}
