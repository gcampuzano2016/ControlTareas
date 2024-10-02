using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using CapaEntidad;

namespace CapaDato
{
    public class DaoParametrosConfiguracion
    {

        public static EntParametrosConfiguracion RTA_ConsultaParametroConfiguracion(String NombreParametro)
        {

            EntParametrosConfiguracion objParametroConfiguracion = null;// new entUsuario();
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAConsultaParametroConfiguracion", cnx);
                cmd.Parameters.AddWithValue("@NombreParametro", NombreParametro);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                objParametroConfiguracion = new EntParametrosConfiguracion();
                dr.Read();

                objParametroConfiguracion.Valor = dr["Valor"].ToString();

                //resultado = dr["Respuesta"].ToString();
            }
            catch (Exception ex)
            {
                objParametroConfiguracion = null;

            }
            finally
            {
                cmd.Connection.Close();
            }
            return objParametroConfiguracion;
        }

        public static string RTA_ValorParametroConfiguracion(String NombreParametro)
        {

            EntParametrosConfiguracion objParametroConfiguracion = null;// new entUsuario();
            string valor = "";
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAConsultaParametroConfiguracion", cnx);
                cmd.Parameters.AddWithValue("@NombreParametro", NombreParametro);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                objParametroConfiguracion = new EntParametrosConfiguracion();
                dr.Read();

                valor = dr["Valor"].ToString();

                //resultado = dr["Respuesta"].ToString();
            }
            catch (Exception ex)
            {
                valor = null;

            }
            finally
            {
                cmd.Connection.Close();
            }
            return valor;
        }


        public static EntRespuesta RTA_ConsultaParametrosConfiguracion(Int32 IdGrupoParametros)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            List<EntParametrosConfiguracion> parametrosConfiguracion = new List<EntParametrosConfiguracion>();
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultaParametrosConfiguracion", cnx);
                cmd.Parameters.AddWithValue("@Id_GrupoParametros", IdGrupoParametros);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    EntParametrosConfiguracion parametroConfiguracion = new EntParametrosConfiguracion();

                    parametroConfiguracion.Nombre = dr["Nombre"].ToString();
                    parametroConfiguracion.Valor = dr["Valor"].ToString();

                    parametrosConfiguracion.Add(parametroConfiguracion);
                }

                Respuesta.estado = "1";
                Respuesta.mensaje = "OK";
                Respuesta.tipoMensaje = "success";
                Respuesta.resultado = parametrosConfiguracion;

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
