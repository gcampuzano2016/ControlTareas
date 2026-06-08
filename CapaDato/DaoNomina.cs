using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDato
{
    public class DaoNomina
    {
        public static EntRespuesta InsertarModificarEliminarRegistroNomina(EntNomina nomina)
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
                cmd = new SqlCommand("InsertarModificarEliminarRegistroNomina", cnx);
                cmd.Parameters.AddWithValue("@Cod_Usuario", nomina.Cod_Usuario );
                cmd.Parameters.AddWithValue("@Json", nomina.Json );
                cmd.Parameters.AddWithValue("@Estado", nomina.Estado);
                cmd.Parameters.AddWithValue("@Tipo", nomina.Tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dtResultados.Load(dr);
                Respuesta.estado = dtResultados.Rows[0][1].ToString();
                Respuesta.mensaje = dtResultados.Rows[0][0].ToString();
                Respuesta.tipoMensaje = "success";
                //Respuesta.resultadoTabla = dtResultados;

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
