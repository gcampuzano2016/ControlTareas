using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CapaEntidad;
using CapaNegocio;
using ReporteTareas.Controles;
using System.IO;
using System.Data;
using System.Text;
using System.Reflection;


namespace ReporteTareas.Formulario
{
    public partial class ReporteHorasExtras1 : System.Web.UI.Page
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
                        //listaDetTareas = NegTareas.ListaDetTareasHorasExtras(CodUnico);// negDetalleFacturaTemp.listaDetalleTempPresenta(Session["CodUnico"].ToString());
                        //dgv_Tareas.DataSource = listaDetTareas;
                        //dgv_Tareas.DataBind();

                        List<EntDetTarea> listaDetTareas = new List<EntDetTarea>();
                        //listaDetTareas = NegTareas.Sp_RTAConsultaUsuarioPorJefe("1001");
                        listaDetTareas = NegTareas.Sp_RTAConsultaUsuarioPorJefe(Session["Cod_Usuario"].ToString());
                        if (listaDetTareas.Count > 0)
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
            System.Text.Encoding utf_8 = System.Text.Encoding.UTF8;
            var control = 0;
            if (control == 0)
            {
                control = 1;
                var linea = "";
                Response.Clear();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment;filename=GridViewExport.xls");
                Response.Charset = "";
                Response.ContentType = "application/vnd.ms-excel";

                StringWriter sw = new StringWriter();

                linea = "Nom Cliente@N. OS@Id. Responsable@Descripcion Tarea@Fch. Tarea@H. Inicio@H. Fin@Tiempo@Detalle Tarea";
                sw.WriteLineAsync(linea);
                for (int i = 0; i < dgv_Tareas.Rows.Count; i++)
                {
                    linea = "";
                    GridViewRow row = dgv_Tareas.Rows[i];
                    for (int j = 0; j < 9; j++)
                    {
                        if (j == 0)
                        {
                            linea = dgv_Tareas.Rows[i].Cells[j].Text;
                        }
                        else
                        {
                            linea = linea + "@" + dgv_Tareas.Rows[i].Cells[j].Text;
                        }
                    }
                    sw.WriteLineAsync(linea);
                }
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
                listaDetTareas = NegTareas.ListaDetTareasHorasExtras(CodUnico, fchIni, fchFin, 1); // negDetalleFacturaTemp.listaDetalleTempPresenta(Session["CodUnico"].ToString());
            }
            else
            {
                listaDetTareas = NegTareas.ListaDetTareasHorasExtras(CboUsuarios.SelectedValue, fchIni, fchFin, 0); // negDetalleFacturaTemp.listaDetalleTempPresenta(Session["CodUnico"].ToString());
            }
            Session["Tabla"] = ToDataTable(listaDetTareas);

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