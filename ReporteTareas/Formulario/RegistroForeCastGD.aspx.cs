using CapaEntidad;
using ReporteTareas.Controles;
using SeguridadAppHelper;
using System;
using System.Collections.Generic;

namespace ReporteTareas.Formulario
{
    public partial class RegistroForeCastGD : System.Web.UI.Page
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
                        txtLoginUsuario.Text = Session["UserLogin"].ToString();
                        txtIdTipo.Text = Session["Id_Usuario"].ToString();
                        if (Session["IdCliente"] != null)
                        {
                            string IdCliente = "";
                            IdCliente = Session["IdCliente"].ToString();

                            if (IdCliente == "0")
                            {
                                txtIdCliente.Text = IdCliente.ToString();
                            }
                            else
                            {
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