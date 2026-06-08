using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using CapaEntidad;
using System.Globalization;

namespace CapaDato
{
    public class DaoReporteGe
    {
        public static List<EntReporteGe> ConsultaSp_RTA_GenerarReporteGDOS(int Anio, int tipo)
        {
            List<EntReporteGe> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTA_GenerarReporteGDOS", cnx);
                cmd.Parameters.AddWithValue("@Anio", Anio);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntReporteGe>();

                while (dr.Read())
                {
                    EntReporteGe Tarea = new EntReporteGe();
                    Tarea.SUCURSAL = dr["SUCURSAL"].ToString();
                    Tarea.CUOTAANUAL = Convert.ToDecimal(dr["CUOTAANUAL"].ToString());
                    Tarea.CUMPLIALAFECHA = Convert.ToDecimal(dr["CUMPLIALAFECHA"].ToString());
                    Tarea.PORFACTURAR = Convert.ToDecimal(dr["PORFACTURAR"].ToString());
                    Tarea.PROYECIONTOANUAL = Convert.ToDecimal(dr["PROYECIONTOANUAL"].ToString());
                    Tarea.PROYECIONPORCANUAL = Convert.ToDecimal(dr["PROYECIONPORCANUAL"].ToString());
                    Tarea.PORCERRARFORECAST = Convert.ToDecimal(dr["PORCERRARFORECAST"].ToString());
                    Tarea.PROYECIONTOANUALFORECAST = Convert.ToDecimal(dr["PROYECIONTOANUALFORECAST"].ToString());
                    Tarea.PROYECIONPORCANUALFORECAST = Convert.ToDecimal(dr["PROYECIONPORCANUALFORECAST"].ToString());
                    Tarea.ESTIMADA25MARGE = Convert.ToDecimal(dr["ESTIMADA25MARGE"].ToString());
                    Tarea.ESTIMADALAFECHA = Convert.ToDecimal(dr["ESTIMADALAFECHA"].ToString());
                    Tarea.ESTIMADAPORCALAFECHA = Convert.ToDecimal(dr["ESTIMADAPORCALAFECHA"].ToString());
                    Tarea.VALORFACTURADO = Convert.ToDecimal(dr["VALORFACTURADO"].ToString());
                    Tarea.VALORPROYECCIONANUAL = Convert.ToDecimal(dr["VALORPROYECCIONANUAL"].ToString());
                    Tarea.VALORPORCPROYECIERRE = Convert.ToDecimal(dr["VALORPORCPROYECIERRE"].ToString());
                    Tarea.PORCERRARFORECASTPVP = Convert.ToDecimal(dr["PORCERRARFORECASTPVP"].ToString());
                    Tarea.PROYECIONTOANUALFORECASTPVP = Convert.ToDecimal(dr["PROYECIONTOANUALFORECASTPVP"].ToString());
                    Tarea.PROYECIONPORCANUALFORECASTPVP = Convert.ToDecimal(dr["PROYECIONPORCANUALFORECASTPVP"].ToString());

                    listaTareas.Add(Tarea);
                }

            }
            catch (Exception ex)
            {
                listaTareas = null;
            }
            finally
            {
                cmd.Connection.Close();
            }

            return listaTareas;
        }

        public static EntRespuesta ConsultaSp_RTA_GenerarReporteGDOSDescargar(int Anio, int tipo)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            DataTable dtResultados = new DataTable();

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTA_GenerarReporteGDOS", cnx);
                cmd.Parameters.AddWithValue("@Anio", Anio);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();

                dtResultados.Load(dr);

                Respuesta.estado = "1";
                Respuesta.mensaje = "OK";
                Respuesta.tipoMensaje = "success";
                Respuesta.resultadoTabla = dtResultados;

            }
            catch (Exception ex)
            {
                Respuesta.estado = "0";
                Respuesta.mensaje = ex.Message.ToString();
                Respuesta.tipoMensaje = "danger";
            }
            finally
            {
                cmd.Connection.Close();
            }

            return Respuesta;
        }

    }
}
