using CapaEntidad;
using ReporteTareas.Controles;
using ReporteTareas.clases;
using SeguridadAppHelper;
using System;
using System.Collections.Generic;

namespace ReporteTareas.Formulario
{
    public partial class ListaEmpleados : System.Web.UI.Page
    {
        #region Variables
        protected NegCRedireccionamientoLogin GenLogin = new NegCRedireccionamientoLogin();
        #endregion
        protected void Page_Load(object sender, EventArgs e)
        {
            GenLogin.RedireccionarALogin(this);
            /* La barrera de verdad esta en ObtenerListaTareas.ashx.cs; esto es
               cortesia para no mostrar una pantalla vacia. */
            if (!Permisos.PuedeEntrar(Context, Pantallas.ListaEmpleados))
            {
                Response.Redirect("~/Formulario/Principal.aspx", true);
            }
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
                        txtPerfil.Text = Session["Id_Perfil"].ToString();
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