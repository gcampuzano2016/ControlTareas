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
   public class DaoMensajeForeCast
    {
        CultureInfo ci = new CultureInfo("es-EC");

        #region Constructores
        public DaoMensajeForeCast()
        {
            ci.NumberFormat.NumberDecimalSeparator = ","; //especificas el separador para decimales
            ci.NumberFormat.NumberGroupSeparator = ""; //especificas el separador para millares
        }
        #endregion

        #region RTAInsertaNuevaMensajeForeCast
        public static EntRespuesta RTAInsertaNuevaMensajeForeCast(EntMensajeForeCast objEntForeCast)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            Int32 respuestaSP = 0;
            string respuestaSP1 = "";
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAInsertaNuevaMensajeForeCast", cnx);

                cmd.Parameters.AddWithValue("@IdMensaje", Convert.ToInt32(objEntForeCast.IdMensaje));
                cmd.Parameters.AddWithValue("@IdForeCast", Convert.ToInt32(objEntForeCast.IdForeCast));
                cmd.Parameters.AddWithValue("@Mensaje", objEntForeCast.Mensaje);
                cmd.Parameters.AddWithValue("@Usuario", objEntForeCast.Usuario);
                cmd.Parameters.AddWithValue("@Tipo", objEntForeCast.Tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();

                respuestaSP1 = dr["Respuestas"].ToString();
                if (respuestaSP1 == "")
                {
                    respuestaSP = 1;
                }
                else
                {
                    respuestaSP = Convert.ToInt32(respuestaSP1);
                }

                if (respuestaSP >= 1)
                {
                    //Respuesta.estado = respuestaSP.ToString();
                    Respuesta.estado = "1";
                    Respuesta.mensaje = "Datos Guardados con Exito.";
                    Respuesta.tipoMensaje = "success";
                    Respuesta.resultado = respuestaSP.ToString();
                }
                else if (respuestaSP == -1)
                {
                    //Respuesta.estado = respuestaSP.ToString();
                    Respuesta.estado = "-1";
                    Respuesta.mensaje = "Por favor validar que la informacion este correcto";
                    Respuesta.tipoMensaje = "danger";
                    Respuesta.resultado = respuestaSP.ToString();
                }
                else
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Ocurrio un error al guardar los datos.";
                    Respuesta.tipoMensaje = "danger";
                }
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
        #endregion

        public static List<EntMensajeForeCast> ConsultaSp_RTAMostrarMensajeForeCast(int IdForeCast)
        {
            List<EntMensajeForeCast> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAMostrarMensajeForeCast", cnx);
                cmd.Parameters.AddWithValue("@IdForeCast", IdForeCast);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntMensajeForeCast>();

                while (dr.Read())
                {
                    EntMensajeForeCast Tarea = new EntMensajeForeCast();
                    Tarea.Mensaje = dr["Mensaje"].ToString();
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

    }
}
