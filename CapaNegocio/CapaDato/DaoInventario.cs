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
    public class DaoInventario
    {
        public static List<EntInventario> RTA_ConsultaInventarioLike(int tipo, string Descripcion)
        {
            List<EntInventario> objInventario = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("RTA_ConsultaLike", cnx);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.Parameters.AddWithValue("@Descripcion", Descripcion);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                objInventario = new List<EntInventario>();
                while (dr.Read())
                {
                    EntInventario Tarea = new EntInventario();
                    Tarea.IdInventario =Convert.ToInt32(dr["IdInventario"].ToString());
                    Tarea.IdMarca = Convert.ToInt32(dr["IdMarca"].ToString());
                    Tarea.CodigoSAP = (dr["CodigoSAP"].ToString());
                    Tarea.NumParte = (dr["NumParte"].ToString());
                    Tarea.Descripcion =(dr["Descripcion"].ToString());
                    Tarea.Cantidad = Convert.ToDouble(dr["Cantidad"].ToString());
                    Tarea.Ubicacion = (dr["Ubicacion"].ToString());
                    Tarea.Almacen = (dr["Almacen"].ToString());
                    Tarea.NumSerie = (dr["NumSerie"].ToString());
                    Tarea.PrecioUnitario = Convert.ToDouble(dr["PrecioUnitario"].ToString());
                    Tarea.PrecioTotal = Convert.ToDouble(dr["PrecioTotal"].ToString());
                    objInventario.Add(Tarea);
                }
            }
            catch (Exception ex)
            {
                objInventario = null;

            }
            finally
            {
                cmd.Connection.Close();
            }
            return objInventario;
        }

        public static EntRespuesta RTAInsertaNuevoInventario(EntInventario objInventario)
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
                cmd = new SqlCommand("Sp_RTAInsertaNuevoInventario", cnx);

                cmd.Parameters.AddWithValue("@IdInventario", Convert.ToInt32(objInventario.IdInventario));
                cmd.Parameters.AddWithValue("@IdMarca", Convert.ToInt32(objInventario.IdMarca));
                cmd.Parameters.AddWithValue("@CodigoSAP", objInventario.CodigoSAP);
                cmd.Parameters.AddWithValue("@NumParte", objInventario.NumParte);
                cmd.Parameters.AddWithValue("@Descripcion", objInventario.Descripcion);
                cmd.Parameters.AddWithValue("@Cantidad", objInventario.Cantidad );
                cmd.Parameters.AddWithValue("@Ubicacion", objInventario.Ubicacion );
                cmd.Parameters.AddWithValue("@Almacen", objInventario.Almacen);
                cmd.Parameters.AddWithValue("@NumSerie", objInventario.NumSerie );
                cmd.Parameters.AddWithValue("@PrecioUnitario", objInventario.PrecioUnitario);
                cmd.Parameters.AddWithValue("@Usuario", objInventario.Usuario );
                cmd.Parameters.AddWithValue("@Estado", objInventario.Estado);
                cmd.Parameters.AddWithValue("@Tipo", objInventario.Tipo);
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

        public static EntRespuesta RTA_ConsultaInventarioLikeDescargar(int tipo, string Descripcion)
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
                cmd = new SqlCommand("RTA_ConsultaLike", cnx);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.Parameters.AddWithValue("@Descripcion", Descripcion);
                cmd.CommandType = CommandType.StoredProcedure;

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
