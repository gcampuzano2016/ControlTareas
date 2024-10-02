using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CapaEntidad;
using CapaNegocio;
using ReporteTareas.Controles;
using System.Data;
using SeguridadAppHelper;
using System.Text;

namespace ReporteTareas.Formulario
{
    public partial class CreaTareas : System.Web.UI.Page
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
                        List<EntTareas> listaTareas = new List<EntTareas>();
                        string CodUnico = "";
                        CodUnico = Session["Cod_Usuario"].ToString();
                        SeguridadHelper seguridad = new SeguridadHelper();
                        txtUsuario.Text = seguridad.Encripta(CodUnico.ToString());

                    }
                    catch (Exception ex)
                    { }
                }
            }

        }          
    }
}