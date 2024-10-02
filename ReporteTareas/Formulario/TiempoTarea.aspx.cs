using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CapaEntidad;
using CapaNegocio;
using ReporteTareas.Controles;
using System.Data;
using System.Text;
using System.Reflection;


namespace ReporteTareas.Formulario
{
    public partial class TiempoTarea : System.Web.UI.Page
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
                        List<EntDetTarea> listaDetTareas = new List<EntDetTarea>();
                        //listaDetTareas = NegTareas.Sp_RTAConsultaUsuarioPorJefe("1001");
                        listaDetTareas = NegTareas.Sp_RTAConsultaUsuarioPorJefe(Session["Cod_Usuario"].ToString());
                        if (listaDetTareas.Count>0)
                        {
                            CboUsuarios.DataSource = ToDataTable(listaDetTareas);
                            CboUsuarios.DataTextField = "Nom_Cliente";                            // FieldName of Table in DataBase
                            CboUsuarios.DataValueField = "Id_Responsable";
                            CboUsuarios.DataBind();
                            Combo.Visible = true;
                        }
                        else
                        {
                            Combo.Visible = false;
                        }

                    }
                    catch (Exception ex)
                    { }
                }
            }
        }


        protected void btn_Descarga_Click(object sender, EventArgs e)
        {
            NegUsuario negUser = new NegUsuario();
            if (Session["Tabla"] == null)
            {
                Response.Write(negUser.mensajeInformativo("No existen datos para exportar.", "warning", true, "divMensajesContenido_2"));
                return;
            }
            System.Text.Encoding utf_8 = System.Text.Encoding.UTF8;
            var control = 0;
            if (control == 0)
            {
                DataTable dt = new DataTable();
                dt = (DataTable)Session["Tabla"];

                dt.Columns.Remove("Id_Responsable");
                //dt.Columns.Remove("Det_Det_Tarea");
                dt.Columns.Remove("Det_Det_TareaFin");
                dt.Columns.Remove("Motivo");
                dt.Columns.Remove("EstadoInicial");

                dt.Columns["Nom_Cliente"].ColumnName = "Cliente";
                dt.Columns["Num_OrdenServicio"].ColumnName = "N° de orden de servicio";
                dt.Columns["Actividad"].ColumnName = "Tipo de actividad";
                dt.Columns["Det_Tarea"].ColumnName = "Descripción breve";
                dt.Columns["Det_Fecha"].ColumnName = "Fecha actividad";
                dt.Columns["Det_Fch_RegDetalleIni"].ColumnName = "Inicio actividad";
                dt.Columns["Det_Fch_RegDetalleFin"].ColumnName = "Fin actividad";
                dt.Columns["Det_Tiempo"].ColumnName = "Tiempo total de la actividad ";
                dt.AcceptChanges();

                String[] texto;
                texto = new String[dt.Rows.Count + 1];

                const string FIELDSEPARATOR = "\t";
                const string ROWSEPARATOR = "\n";
                StringBuilder output = new StringBuilder();

                foreach (DataColumn dc in dt.Columns) //Encabezado
                {
                    output.Append(dc.ColumnName);
                    output.Append(FIELDSEPARATOR);
                }
                output.Append(ROWSEPARATOR);
                foreach (DataRow item in dt.Rows) //Filas
                {
                    foreach (object value in item.ItemArray)
                    {
                        output.Append(value.ToString().Replace('\n', ' ').Replace('\r', ' ').ToString());
                        output.Append(FIELDSEPARATOR);
                    }
                    output.Append(ROWSEPARATOR);
                }
                //Agrega el contenido en el nuevo archivo

                Response.Clear();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment;filename=GridViewExport.xls");
                Response.Charset = "windows-1252";
                Response.ContentType = "application/vnd.ms-excel";
             
                StringWriter sw = new StringWriter();
                sw.WriteLineAsync(output.ToString());
                Response.Output.Write(sw.ToString());
                Response.End();
            }
        }

        protected void btn_consulta_Click(object sender, EventArgs e)
        {
            var fchIni = TexfechaInicio.Text.ToString();
            var fchFin = TexfechaFin.Text.ToString();

            List<EntDetTarea> listaDetTareas = new List<EntDetTarea>();
            string CodUnico = "";
            //CodUnico = Session["Cod_Usuario"].ToString(); //
            if (CboUsuarios.SelectedItem.ToString() == "SELECCIONAR EMPLEADO")
            {
                CodUnico = Session["Cod_Usuario"].ToString();
                listaDetTareas = NegTareas.ListaDetTareas(CodUnico, fchIni, fchFin,"","",0,0,0,0,1); // negDetalleFacturaTemp.listaDetalleTempPresenta(Session["CodUnico"].ToString());
            }
            else
            {
                listaDetTareas = NegTareas.ListaDetTareas(CboUsuarios.SelectedValue, fchIni, fchFin,"","",0,0,0,0,0); // negDetalleFacturaTemp.listaDetalleTempPresenta(Session["CodUnico"].ToString());
            }
            Session["Tabla"] =ToDataTable(listaDetTareas);

            dgv_Tareas.DataSource = listaDetTareas;
            dgv_Tareas.DataBind();



            btn_Descarga.Enabled = true;
            btn_Descarga.Visible = true;

        }

        private void ExportGridToExcel()
        {
            Response.Clear();
            Response.Buffer = true;
            Response.ClearContent();
            Response.ClearHeaders();
            Response.Charset = "";
            string FileName = "Vithal" + DateTime.Now + ".xls";
            StringWriter strwritter = new StringWriter();
            HtmlTextWriter htmltextwrtter = new HtmlTextWriter(strwritter);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment;filename=" + FileName);
            dgv_Tareas.GridLines = GridLines.Both;
            dgv_Tareas.HeaderStyle.Font.Bold = true;
            dgv_Tareas.RenderControl(htmltextwrtter);
            Response.Write(strwritter.ToString());
            Response.End();
        }

        public DataTable ToDataTable<T>(List<T> items)
        {

            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }

            if (items is null)
            {
                return dataTable;
            }

            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

    }
}