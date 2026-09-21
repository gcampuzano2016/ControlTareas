using ReporteTareas.Controles;
using SeguridadAppHelper;
using System;

namespace ReporteTareas.Formulario
{
    public partial class ParametrizacionUsuarios : System.Web.UI.Page
    {
        #region Variables
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);

            if (Session["UserLogin"] != null)
            {
                if (!IsPostBack)
                {
                    try
                    {
                        string CodUnico = Session["Cod_Usuario"].ToString();
                        SeguridadHelper seguridad = new SeguridadHelper();
                        txtUsuario.Text = seguridad.Encripta(CodUnico.ToString());
                        txtLoginUsuario.Text = Session["UserLogin"].ToString();

                        if (Session["IdCliente"] != null)
                        {
                            txtIdCliente.Text = Session["IdCliente"].ToString();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Solo Super Admin (18) ve el control para cambiar el perfil de otro
        /// usuario.
        ///
        /// La barrera de verdad esta en AdministrarUsuarios.ashx.cs, que lo
        /// vuelve a comprobar en las dos acciones: esto es cortesia para quien
        /// no puede, no una barrera para quien no debe.
        /// </summary>
        protected bool PuedeCambiarPerfil
        {
            get
            {
                int idPerfil;
                return int.TryParse(Convert.ToString(Session["Id_Perfil"]), out idPerfil) && idPerfil == 18;
            }
        }
    }
}
