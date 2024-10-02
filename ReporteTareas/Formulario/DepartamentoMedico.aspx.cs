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
    public partial class DepartamentoMedico : System.Web.UI.Page
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
                        string CodUnico = Session["Cod_Usuario"].ToString();
                        SeguridadHelper seguridad = new SeguridadHelper();
                        txtUsuario.Text = seguridad.Encripta(CodUnico);
                        txtPerfil.Text = Session["Id_Perfil"].ToString();
                        if (Session["IdCliente"] != null)
                        {
                            string IdCliente = Session["IdCliente"].ToString();
                            txtIdCliente.Text = IdCliente;
                        }

                        string cedulaEmpleado = Request.Form["cedula"];
                        if (!string.IsNullOrEmpty(cedulaEmpleado))
                        {
                            hiddenCedulaField.Text = cedulaEmpleado; // Pasar al campo oculto
                        }

                    }
                    catch (Exception ex)
                    {
                        // Imprimir el error en la consola del navegador
                        Response.Write($"<script>console.error('Error en Page_Load: {ex.Message}');</script>");
                    }
                }
                else
                {
                        // Imprimir el error en la consola del navegador
                     Response.Write($"<script>console.error('Error en PostBack:');</script>");
                    
                }
            }
        }
    }
}