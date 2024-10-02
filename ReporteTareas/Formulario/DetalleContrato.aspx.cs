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
    public partial class DetalleContrato : System.Web.UI.Page
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
                        txtIdTipo.Text = Session["Id_Usuario"].ToString();
                        if (Session["IdCliente"] != null)
                        {
                            string IdCliente = "";
                            IdCliente = Session["IdCliente"].ToString();

                            if (IdCliente == "0")
                            {
                                //idGerente.Visible = true;
                                //Detalle.Visible = true;
                                txtIdCliente.Text= IdCliente.ToString();
                            }
                            else
                            {
                                //idGerente.Visible = false;
                                //Detalle.Visible = false;
                                txtIdCliente.Text = IdCliente.ToString();
                            }
                        }

                    }
                    catch (Exception ex)
                    { }
                }
            }
            
        }


    }
}