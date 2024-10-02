using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CapaEntidad;
using CapaNegocio;
using ReporteTareas.Controles;

namespace ReporteTareas.Formulario
{
    public partial class CreaTarea : System.Web.UI.Page
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

                        //txt_Categoria.Text = "Administrativa";
                        //txt_Categoria.Enabled = false;

                        txt_FchRegistro.Text = System.DateTime.Now.ToString();
                        txt_FchRegistro.Enabled = false;


                        dbl_SubCategoria.DataSource = NegBoolean.listaCategoria();
                        dbl_SubCategoria.DataValueField = "IdBoolean";
                        dbl_SubCategoria.DataTextField = "NomBoolean";
                        dbl_SubCategoria.DataBind();


                        dbl_OS.DataSource = NegBoolean.listaOsAdministrativas();
                        dbl_OS.DataValueField = "IdBoolean";
                        dbl_OS.DataTextField = "NomBoolean";
                        dbl_OS.DataBind();



                    }
                    catch (Exception ex)
                    { }
                }
            }
        }

        protected void btn_Guardar_Click(object sender, EventArgs e)
        {

            var control = 0;
            EntTareas objTarea = new EntTareas();
            //txt_Os
            //txt_Cliente
            //txt_FchRegistro
            //txt_DetCamEstado

            if (!dbl_OS.SelectedItem.ToString().Equals("--Seleccione OS--") && !txt_Cliente.Text.Equals("") && !txt_DetCamEstado.Text.Equals(""))
            {
                EntUsuario objUsuario = new EntUsuario();

                objUsuario = NegUsuario.RTA_ConsultaUsuarioRTA(Session["UserLogin"].ToString());

                var OS = dbl_OS.SelectedItem.ToString().Substring(0, 9);

                objTarea.Num_OrdenServicio = OS; //dbl_OS.SelectedItem.ToString();
                objTarea.Fch_Registro = txt_FchRegistro.Text;
                objTarea.Id_Responsable = objUsuario.Cod_Usuario;
                objTarea.Nom_Responsable = objUsuario.Nom_Usuario;
                objTarea.Nom_Empresa = txt_Cliente.Text;
                objTarea.Det_Tarea = txt_DetCamEstado.Text;
                objTarea.Estado = "A";
                objTarea.Categoria = "Administrativa";
                objTarea.SubCategoria = dbl_SubCategoria.SelectedItem.ToString();
                objTarea.EstadoApro = "";


                control = NegTareas.RTA_CreaTareas(objTarea);

                if (control > 0)
                {
                    Response.Write("<script>alert('La tarea fue creada con exito');</script>");
                }

                Response.Redirect("CambiarEstadoTarea.aspx");


            }
            else
            {
                //if (txt_Os.Text.Equals(""))
                if(dbl_OS.SelectedItem.ToString().Equals("--Seleccione OS--"))
                {
                    //Response.Write("<script>alert('Se debe ingresar una orden orden de servicio');</script>");
                    Response.Write("<script>alert('Se debe seleccionar una orden orden de servicio valida');</script>");
                }
                else if (txt_Cliente.Text.Equals(""))
                {
                    Response.Write("<script>alert('Se debe ingresar el nombre del cliente');</script>");
                }
                else if (txt_DetCamEstado.Text.Equals(""))
                {
                    Response.Write("<script>alert('Se debe ingresar el detalle de la actividad');</script>");
                }
            }



        }
    }
}