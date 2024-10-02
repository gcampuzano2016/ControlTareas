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

namespace ReporteTareas.Formulario
{
    public partial class AprobarHorasExtras : System.Web.UI.Page
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
                        CargarTareas();
                    }
                    catch (Exception ex)
                    { }
                }
            }  

        }

        protected void dgv_Tareas_SelectedIndexChanged(object sender, EventArgs e)
        {           
            if (dgv_Tareas.Rows[dgv_Tareas.SelectedIndex].Cells[0].Text == "Aprobar H. Extras")
            {
               
            }
            else if (dgv_Tareas.Rows[dgv_Tareas.SelectedIndex].Cells[1].Text == "Revisar Tarea")
            {

            }
        }

        public void CargarTareas()
        {
            List<EntTareas> listaTareas = new List<EntTareas>();
            string CodUnico = "";
            CodUnico = Session["Cod_Usuario"].ToString(); //

            listaTareas = NegTareas.ListaTareasHorasExtras(CodUnico);// negDetalleFacturaTemp.listaDetalleTempPresenta(Session["CodUnico"].ToString());



            dgv_Tareas.DataSource = listaTareas;
            dgv_Tareas.DataBind();
        }

        protected void btn_Guardar_Click(object sender, EventArgs e)
        {
            EntHisTarea HisTarea = new EntHisTarea();
            var respu = 0;
            string[] datosDoc = dbl_Estado.SelectedValue.ToString().Split('-');

            HisTarea.Id_RegTareas = Convert.ToInt32(txt_IdRegistro.Text);
            HisTarea.Det_Num_OrdenServicio = txt_Os.Text;
            HisTarea.Det_Id_CompAranda = txt_Ticket.Text;
            HisTarea.Det_Fch_RegDetalleIni = System.DateTime.Now.ToString();
            HisTarea.Det_Fch_RegDetalleFin = "";
            HisTarea.Det_EstadoIni = datosDoc[2];//
            HisTarea.Det_EstadoFin = "";
            HisTarea.Det_Nom_Empresa = txt_Cliente.Text;
            //HisTarea.Det_Det_Tarea = txt_Comentario.Text;
            HisTarea.Det_Estado = dbl_Estado.SelectedItem.ToString();
            HisTarea.Det_Motivo_Cambio_Estado = dbl_Estado.SelectedItem.ToString();
            HisTarea.Det_Det_Tarea = "";
            respu = NegTareas.RTAActualizaHisTareaHorasExtras(HisTarea);
            btn_Guardar.Enabled = false;
            Response.Write("<script>alert('Actualización registrada con exito');</script>");
            Response.Redirect("AprobarHorasExtras.aspx");

            //EntHisTarea HisTarea = new EntHisTarea();

            //var user = Session["Cod_Usuario"].ToString();
            //var respu = 0;
            //var ars = 0;
            ////pongo en pausa otra actividad de forma automatica
            //ars = NegTareas.RTA_ActuaProgHisTarea(user);
            //HisTarea.Id_RegTareas = Convert.ToInt32(txt_IdRegistro.Text);
            //HisTarea.Det_Num_OrdenServicio = txt_Os.Text;
            //HisTarea.Det_Id_CompAranda = txt_Ticket.Text;
            //HisTarea.Det_Fch_RegDetalleIni = System.DateTime.Now.ToString();
            //HisTarea.Det_Fch_RegDetalleFin = "";
            //HisTarea.Det_EstadoIni = "E";
            //HisTarea.Det_EstadoFin = "";
            //HisTarea.Det_Nom_Empresa = txt_Cliente.Text;
            //HisTarea.Det_Det_Tarea = dbl_DetEstado.SelectedItem.ToString();
            //HisTarea.Det_Estado = dbl_Estado.SelectedItem.ToString();

            //respu = NegTareas.RTA_IngresaHisTarea(HisTarea);

            //btn_Guardar.Enabled = false;
            //Response.Write("<script>alert('Actualización registrada con exito');</script>");
            //Response.Redirect("AprobarHorasExtras.aspx");

        }

        protected void dbl_Estado_SelectedIndexChanged1(object sender, EventArgs e)
        {

            if (Convert.ToString(dbl_Estado.SelectedValue) == "0")
            {
                btn_Guardar.Enabled = false;
            }
            else
            {
                btn_Guardar.Enabled = true;
            }
            //var control = "";

            //if (Convert.ToInt16(dbl_Estado.SelectedValue) == 0)
            //{
            //    control = "X";
            //}
            //else if (Convert.ToInt16(dbl_Estado.SelectedValue) == 1)
            //{
            //    control = "En Progreso";
            //}
            //else if (Convert.ToInt16(dbl_Estado.SelectedValue) == 2)
            //{
            //    control = "Pausado";
            //}
            //else if (Convert.ToInt16(dbl_Estado.SelectedValue) == 3)
            //{
            //    control = "Finalizado";
            //}

            //if (control.Equals(txt_Estado.Text))
            //{
            //    Response.Write("<script>alert('No se puede selecionar el mismo estado que al momento tiene la actividad');</script>");
            //}
            //else
            //{
            //    if (Convert.ToInt16(dbl_Estado.SelectedValue) == 2)
            //    {

            //        dbl_DetEstado.Enabled = true;

            //    }
            //    else
            //    {

            //        dbl_DetEstado.DataSource = NegBoolean.listaDetEstado();
            //        dbl_DetEstado.DataValueField = "IdBoolean";
            //        dbl_DetEstado.DataTextField = "NomBoolean";
            //        dbl_DetEstado.DataBind();
            //        dbl_DetEstado.Enabled = false;

            //    }

            //    if (Convert.ToInt16(dbl_Estado.SelectedValue) == 0)
            //    {
            //        btn_Guardar.Enabled = false;
            //    }
            //    else
            //    {
            //        btn_Guardar.Enabled = true;
            //    }
            //}
        }

        protected void btn_Guardar0_Click(object sender, EventArgs e)
        {
            Panel1.Enabled = true;
            Panel1.Visible = true;

            Panel2.Enabled = false;
            Panel2.Visible = false;

            Panel3.Enabled = false;
            Panel3.Visible = false;
        }

        protected void dgv_Tareas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "New")
            {
                //if (GenerarTareas == false)
                //{
                var IdRegistro = 0;
                string ResponsableAprobacion = "";
                int index = 0;
                GridViewRow row;
                GridView grid = sender as GridView;
                index = Convert.ToInt32(e.CommandArgument);
                row = grid.Rows[index];

                IdRegistro = Convert.ToInt32(row.Cells[2].Text);
                ResponsableAprobacion = row.Cells[16].Text;

                if (ResponsableAprobacion == "&nbsp;")
                {
                    if (IdRegistro == 0)
                    {
                    }
                    else
                    {
                        bool GenerarTareas = false;
                        //foreach (GridViewRow row1 in dgv_Tareas.Rows)
                        //{
                        //    string valorcol2 = row1.Cells[16].Text;
                        //    if (valorcol2 != "&nbsp;")
                        //    {
                        //        GenerarTareas = true;
                        //        break;
                        //    }
                        //}

                        foreach (GridViewRow row1 in dgv_Tareas.Rows)
                        {
                            string valorcol2 = row1.Cells[14].Text;
                            if (valorcol2 == "Finalizado")
                            {
                                GenerarTareas = false;
                                break;
                            }
                        }

                        if (GenerarTareas == false)
                        {
                            var control = 0;
                            control = NegTareas.RTA_CreaTareasHorasExtras(IdRegistro);

                            if (control > 0)
                            {
                                CargarTareas();
                                Response.Write("<script>alert('La tarea de horas extra fue enviada con exito para su aprobación');</script>");
                            }

                        }
                        else
                        {
                            Response.Write("<script>alert('No se puede generar  Horas Extras para otra Tarea');</script>");
                        }
                    }
                }
                else if (ResponsableAprobacion == "&nbsp;")
                {
                    Response.Write("<script>alert('La tarea esta pendiente de Aprobaciòn');</script>");
                }
            }

            else if (e.CommandName == "Edit")
            {

                var IdRegistro = 0;
                int index = 0;
                GridViewRow row;
                GridView grid = sender as GridView;
                index = Convert.ToInt32(e.CommandArgument);
                row = grid.Rows[index];

                IdRegistro = Convert.ToInt32(row.Cells[2].Text);

                if (row.Cells[14].Text == "Rechazado")
                {
                    Response.Write("<script>alert('La tarea esta rechazada no se puede cambiar el estado');</script>");
                }
                else
                {
                    EntTareas objTarea = new EntTareas();
                    objTarea = NegTareas.RTA_ConsultaTareaHorasExtrasRTA(IdRegistro);
                    if (objTarea == null)
                    {
                        Response.Write("<script>alert('No hay horas extras Agsinadas para esta tarea');</script>");
                    }
                    else
                    {
                        txt_Os.Text = objTarea.Num_OrdenServicio.ToString();
                        txt_Ticket.Text = objTarea.Id_CompAranda;
                        txt_FchRegistro.Text = objTarea.Fch_Registro;

                        txt_Estado.Text = objTarea.Estado;

                        DataSet data = NegTareas.RTAConsultaCatalogo(objTarea.IdEstado);

                        if (txt_Estado.Text.Equals("Asignado") || txt_Estado.Text.Equals("Pausado"))
                        {
                            dbl_Estado.DataSource = data.Tables[0];// NegBoolean.listaEstadoIni();
                            dbl_Estado.DataValueField = "IdCatalogo";
                            dbl_Estado.DataTextField = "Descripcion";
                            dbl_Estado.DataBind();
                        }
                        else
                        {
                            dbl_Estado.DataSource = data.Tables[0];// NegBoolean.listaEstadoIni();
                            dbl_Estado.DataValueField = "IdCatalogo";
                            dbl_Estado.DataTextField = "Descripcion";
                            dbl_Estado.DataBind();
                        }



                        txt_Cliente.Text = objTarea.Nom_Empresa;
                        txt_NomResponsable.Text = objTarea.Nom_Responsable;
                        txt_FchTenInicio.Text = objTarea.Fch_EstAtencion;
                        txt_FchTenFin.Text = objTarea.Fch_EstSolucion;

                        txt_IdRegistro.Text = objTarea.Id_RegTareas.ToString();

                        txt_Os.Enabled = false;
                        txt_Ticket.Enabled = false;
                        txt_FchRegistro.Enabled = false;
                        txt_Estado.Enabled = false;
                        txt_Cliente.Enabled = false;
                        txt_NomResponsable.Enabled = false;
                        txt_FchTenInicio.Enabled = false;
                        txt_FchTenFin.Enabled = false;

                        dbl_DetEstado.DataSource = NegBoolean.listaDetEstado();
                        dbl_DetEstado.DataValueField = "IdBoolean";
                        dbl_DetEstado.DataTextField = "NomBoolean";
                        dbl_DetEstado.DataBind();

                        Panel1.Enabled = false;
                        Panel1.Visible = false;

                        Panel2.Enabled = true;
                        Panel2.Visible = true;

                        Panel3.Enabled = true;
                        Panel3.Visible = true;
                    }
                }
            }
        }

        protected void dgv_Tareas_RowEditing(object sender, GridViewEditEventArgs e)
        {

        }
    }
}