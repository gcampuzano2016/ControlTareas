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
    public partial class Tareas : System.Web.UI.Page
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
                        CodUnico = Session["Cod_Usuario"].ToString(); //
                        listaTareas = NegTareas.ListaTareas(CodUnico);// negDetalleFacturaTemp.listaDetalleTempPresenta(Session["CodUnico"].ToString());

                        dgv_Tareas.DataSource = listaTareas;
                        dgv_Tareas.DataBind();
                    }
                    catch (Exception ex)
                    { }
                }
            }
            //Session["Cod_Usuario"]
        }

        protected void dgv_Tareas_SelectedIndexChanged(object sender, EventArgs e)
        {
            var estado = "";
            var IdRegistro = 0;
            string  IdRegistroAranda = "";
            IdRegistro = Convert.ToInt32(dgv_Tareas.Rows[dgv_Tareas.SelectedIndex].Cells[1].Text);

            IdRegistroAranda = Convert.ToString(dgv_Tareas.Rows[dgv_Tareas.SelectedIndex].Cells[2].Text);

            EntTareas objTarea1 = new EntTareas();
            objTarea1 = NegTareas.RTA_ConsultaTareaRTA(Convert.ToInt32(IdRegistro));
            LabelTareaActualText.Text = objTarea1.EstadoTarea;


            if (IdRegistroAranda== "---------------")
            {
                Session["IdRegistro"] = IdRegistro;
                Session["IdRegistroAranda"] =0;
                Parte0.Visible = false;
                Parte1.Visible = false;
                Parte2.Visible = false;
                txtDetalleCaso.Text = objTarea1.Det_Tarea.ToString();
                Parte3.Visible = true;
                Parte4.Visible = false;
            }
            else
            {
                Parte0.Visible = true;
                Parte1.Visible = true;
                Parte2.Visible = true;
                Parte3.Visible = true;
                Parte4.Visible = true;

                string[] datosDoc = IdRegistroAranda.Split('-');
                Session["IdRegistro"] = IdRegistro;
                Session["IdRegistroAranda"] = datosDoc[1];


                if (datosDoc[0].ToString() == "IM")
                {
                    ConsultarIncidente2.IncidentSoapClient incidente = new ConsultarIncidente2.IncidentSoapClient();
                    ConsultarIncidente2.IncidentDescription getObjectIncidentResponse = new ConsultarIncidente2.IncidentDescription();

                    if(getObjectIncidentResponse != null)
                    {
                        getObjectIncidentResponse = incidente.GetObject(Convert.ToInt32(datosDoc[1]));
                        txtDescripcionCaso.Text = getObjectIncidentResponse.Subject.ToString();
                        txtDetalleCaso.Text = getObjectIncidentResponse.DescriptionNoHtml.ToString();
                        txtCategoria.Text = getObjectIncidentResponse.CategoryName.ToString();
                        txtcliente.Text = getObjectIncidentResponse.CustomerName.ToString();
                        txtGrupo.Text = getObjectIncidentResponse.GroupName.ToString();
                        txt_Cliente.Text = getObjectIncidentResponse.CompanyName.ToString();
                        txtServicio.Text = getObjectIncidentResponse.ServiceName.ToString();
                        //txtApertura.Text = getObjectIncidentResponse.OpenedDate.ToString();
                        //txtAtencionReal.Text = getObjectIncidentResponse.AttentionRealDate.ToString();
                        //txtAtencionEstimada.Text = getObjectIncidentResponse.AttentionEstimatedDate.ToString();
                        //txtExpira.Text = getObjectIncidentResponse.ExpiredDate.ToString();
                        txtEstadoAranda.Text = getObjectIncidentResponse.StatusName.ToString();
                        txtImpacto.Text = getObjectIncidentResponse.ImpactName.ToString();
                        txtPrioridad.Text = getObjectIncidentResponse.PriorityName.ToString();
                        txtUrgencia.Text = getObjectIncidentResponse.UrgencyName.ToString();
                        txtProyecto.Text = getObjectIncidentResponse.ProjectName.ToString();
                    }
                }
                else if(datosDoc[0].ToString() == "RF")
                {
                    ConsultarTicket2.ServiceCallSoapClient service = new ConsultarTicket2.ServiceCallSoapClient();
                    ConsultarTicket2.ServiceCallDescription getObjectResponse = new ConsultarTicket2.ServiceCallDescription();

                    getObjectResponse = service.GetObject(Convert.ToInt32(datosDoc[1]));
                    txtDescripcionCaso.Text = getObjectResponse.Subject.ToString();
                    txtDetalleCaso.Text = getObjectResponse.DescriptionNoHtml.ToString();
                    txtCategoria.Text = getObjectResponse.CategoryName.ToString();
                    txtcliente.Text = getObjectResponse.CustomerName.ToString();
                    txtGrupo.Text = getObjectResponse.GroupName.ToString();
                    txt_Cliente.Text = getObjectResponse.CompanyName.ToString();
                    txtServicio.Text = getObjectResponse.ServiceName.ToString();
                    //txtApertura.Text = getObjectResponse.OpenedDate.ToString();
                    //txtAtencionReal.Text = getObjectResponse.AttentionRealDate.ToString();
                    //txtAtencionEstimada.Text = getObjectResponse.AttentionEstimatedDate.ToString();
                    //txtExpira.Text = getObjectResponse.ExpiredDate.ToString();
                    txtEstadoAranda.Text = getObjectResponse.StatusName.ToString();
                    txtImpacto.Text = getObjectResponse.ImpactName.ToString();
                    txtPrioridad.Text = getObjectResponse.PriorityName.ToString();
                    txtUrgencia.Text = getObjectResponse.UrgencyName.ToString();
                    txtProyecto.Text = getObjectResponse.ProjectName.ToString();
                }
                else
                {
                    ConsultarTicket2.ServiceCallSoapClient service = new ConsultarTicket2.ServiceCallSoapClient();
                    ConsultarTicket2.ServiceCallDescription getObjectResponse = new ConsultarTicket2.ServiceCallDescription();

                    getObjectResponse = service.GetObject(Convert.ToInt32(datosDoc[1]));
                    if (getObjectResponse != null)
                    {
                        txtDescripcionCaso.Text = getObjectResponse.Subject.ToString();
                        txtDetalleCaso.Text = getObjectResponse.DescriptionNoHtml.ToString();
                        txtCategoria.Text = getObjectResponse.CategoryName.ToString();
                        txtcliente.Text = getObjectResponse.CustomerName.ToString();
                        txtGrupo.Text = getObjectResponse.GroupName.ToString();
                        txt_Cliente.Text = getObjectResponse.CompanyName.ToString();
                        txtServicio.Text = getObjectResponse.ServiceName.ToString();
                        //txtApertura.Text = getObjectResponse.OpenedDate.ToString();
                        //txtAtencionReal.Text = getObjectResponse.AttentionRealDate.ToString();
                        //txtAtencionEstimada.Text = getObjectResponse.AttentionEstimatedDate.ToString();
                        //txtExpira.Text = getObjectResponse.ExpiredDate.ToString();
                        txtEstadoAranda.Text = getObjectResponse.StatusName.ToString();
                        txtImpacto.Text = getObjectResponse.ImpactName.ToString();
                        txtPrioridad.Text = getObjectResponse.PriorityName.ToString();
                        txtUrgencia.Text = getObjectResponse.UrgencyName.ToString();
                        txtProyecto.Text = getObjectResponse.ProjectName.ToString();
                    }
                    else
                    {
                        txtDescripcionCaso.Text = "";
                        txtDetalleCaso.Text = "";
                        txtCategoria.Text = "";
                        txtcliente.Text = "";
                        txtGrupo.Text = "";
                        txt_Cliente.Text = "";
                        txtServicio.Text = "";
                        //txtApertura.Text = "";
                        //txtAtencionReal.Text = "";
                        //txtAtencionEstimada.Text = "";
                        //txtExpira.Text = "";
                        txtEstadoAranda.Text = "";
                        txtImpacto.Text = "";
                        txtPrioridad.Text = "";
                        txtUrgencia.Text = "";
                        txtProyecto.Text = "";

                        // Como se envía en blanco la información se oculta los campos en el formulario
                        Parte0.Visible = false;
                        Parte1.Visible = false;
                        Parte2.Visible = false;
                        Parte3.Visible = false;
                        Parte4.Visible = false;
                    }
                }

            }

            EntTareas objTarea = new EntTareas();

            objTarea = NegTareas.RTA_ConsultaTareaRTA(IdRegistro);

            DataSet data = NegTareas.RTAListaCatalogoSecuencia(objTarea.IdEstado);
            //Sp_RTAListaCatalogoSecuencia
            txt_Os.Text = objTarea.Num_OrdenServicio.ToString();
            txt_Ticket.Text = objTarea.Id_CompAranda;
            txt_FchRegistro.Text = objTarea.Fch_Registro;

            txt_Estado.Text = objTarea.Estado;

            if (!txt_Estado.Text.Equals("En Progreso")) //
            {
                dbl_Estado.DataSource = data.Tables[0];// NegBoolean.listaEstadoIni();
                dbl_Estado.DataValueField = "IdCatalogo";
                dbl_Estado.DataTextField = "Descripcion";
                dbl_Estado.DataBind();

            }
            else
            {
                dbl_Estado.DataSource = data.Tables[0];// NegBoolean.listaEstado();
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
          
            Panel1.Enabled = false;
            Panel1.Visible = false;

            Panel2.Enabled = true;
            Panel2.Visible = true;

            Panel3.Enabled = true;
            Panel3.Visible = true;


        }

        //protected void dbl_Estado_SelectedIndexChanged(object sender, EventArgs e)
        //{

        //    if (Convert.ToInt16(dbl_Estado.SelectedValue) == 2)
        //    {

        //        dbl_DetEstado.Enabled = true;

        //    }

        //}

        protected void dbl_Estado_SelectedIndexChanged1(object sender, EventArgs e)
        {

            if (Convert.ToString(dbl_Estado.SelectedValue) == "0")
            {
                btn_Guardar.Enabled = false;
                btn_Guardar.Visible = false;
                txt_Comentario.Enabled = false;
                txt_Comentario.Visible = false;
                lbl_Comentario.Visible = false;
                txt_Comentario.Text = "";
            }
            else
                {

                    string[] datosDoc = dbl_Estado.SelectedValue.Split('-');
                    
                    DataSet dataCatalogo = NegTareas.RTAConsultaCatalogoPorPadre(1, Convert.ToInt32(datosDoc[0]));
                    

                    foreach (DataRow drCatalogo in dataCatalogo.Tables[0].Rows)
                    {
                        if (drCatalogo[8].ToString() == "1")
                        {
                            txt_Comentario.Text = "";
                            txt_Comentario.Enabled = true;
                            txt_Comentario.Visible = true;
                            lbl_Comentario.Visible = true;
                    }
                        else
                        {
                            txt_Comentario.Text = dbl_Estado.SelectedItem.ToString();
                            txt_Comentario.Enabled = false;
                            txt_Comentario.Visible = true;
                            lbl_Comentario.Visible = true;
                        }

                    }

                    btn_Guardar.Enabled = true;
                    btn_Guardar.Visible = true;
                   

            }

        }

        protected void btn_Guardar_Click(object sender, EventArgs e)
        {

            if (txt_Comentario.Text == "")
            {
                Response.Write("<script>alert('Debe escribir un comentario');</script>");
            }
            else
            {
                ConsultarTicket2.ServiceCallSoapClient service = new ConsultarTicket2.ServiceCallSoapClient();
                ConsultarTicket2.ServiceCallDescription getObjectResponse = new ConsultarTicket2.ServiceCallDescription();

                //string IdProjet = "1"; // Session["IdProjet"].ToString();
                string Id = Session["IdRegistroAranda"].ToString();
                //string StatusId = Session["StatusId"].ToString();
                //string Description = Session["Description"].ToString();

                EntHisTarea HisTarea = new EntHisTarea();

                var user = Session["Cod_Usuario"].ToString();
                var respu = 0;
                var ars = 0;

                if (Convert.ToString(dbl_Estado.SelectedValue) == "0")
                {
                    // control = "X";
                    Response.Write("<script>alert('Debe seleccionar un estado para actulizar la actividad');</script>");
                }
                else
                {
                    string[] datosDoc = dbl_Estado.SelectedValue.ToString().Split('-');

                    
                    //ConsultarTicket2.ServiceCall serviceCall = new ConsultarTicket2.ServiceCall();
                    //serviceCall.Id = Convert.ToInt32(Id);
                    //serviceCall.StatusId = Convert.ToInt32(datosDoc[1]);
                    //serviceCall.Description = Description;

                    //bool Resultado = service.Update(serviceCall);

                    //pongo en pausa otra actividad de forma automatica

                    //ars = NegTareas.RTA_ActuaProgHisTarea(user);



                    HisTarea.Id_RegTareas = Convert.ToInt32(txt_IdRegistro.Text);
                    HisTarea.Det_Num_OrdenServicio = txt_Os.Text;
                    HisTarea.Det_Id_CompAranda = txt_Ticket.Text;
                    HisTarea.Det_Fch_RegDetalleIni = System.DateTime.Now.ToString();
                    HisTarea.Det_Fch_RegDetalleFin = "";
                    HisTarea.Det_EstadoIni =  datosDoc[2];//
                    HisTarea.Det_EstadoFin = "";
                    HisTarea.Det_Nom_Empresa = txt_Cliente.Text;
                    HisTarea.Det_Det_Tarea = txt_Comentario.Text;
                    //HisTarea.Det_Estado = dbl_Estado.SelectedItem.ToString();
                    HisTarea.Det_Estado = datosDoc[0].ToString(); 
                    HisTarea.Det_Motivo_Cambio_Estado = dbl_Estado.SelectedItem.ToString();
                    respu = NegTareas.RTA_IngresaHisTarea(HisTarea);
                    btn_Guardar.Enabled = false;
                    Response.Write("<script>alert('Actualización registrada con exito');</script>");
                    Response.Redirect("Tareas.aspx");
                }
            }
        }
    }
}