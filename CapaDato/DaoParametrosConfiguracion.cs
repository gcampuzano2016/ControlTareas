using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

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

                /* Se recorta al leer. Un parametro de configuracion no tiene espacios ni
                   saltos de linea significativos al principio ni al final, y en cambio
                   llegan solos: una clave pegada desde un correo trae un CR+LF invisible
                   detras. Paso de verdad -la clave de controldetareas@dos.com.ec quedo
                   guardada como 8 caracteres + CR + LF- y dejo sin correo a todo el
                   sistema, sin un solo mensaje que lo delatara: SmtpClient envia la clave
                   con el salto pegado y Office 365 responde 535, igual que ante una clave
                   equivocada.

                   Aqui y no en quien llama: son 166 llamadas repartidas por todo el
                   sistema y ninguna tiene por que saber esto. */
                objParametroConfiguracion.Valor = dr["Valor"].ToString().Trim();

                //resultado = dr["Respuesta"].ToString();
            }
            catch (Exception ex)
            {
                objParametroConfiguracion = null;

            }
            finally
            {
                if (cmd != null && cmd.Connection != null)
                {
                    cmd.Connection.Close();
                }
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

                /* Se recorta al leer. Un parametro de configuracion no tiene espacios ni
                   saltos de linea significativos al principio ni al final, y en cambio
                   llegan solos: una clave pegada desde un correo trae un CR+LF invisible
                   detras. Paso de verdad -la clave de controldetareas@dos.com.ec quedo
                   guardada como 8 caracteres + CR + LF- y dejo sin correo a todo el
                   sistema, sin un solo mensaje que lo delatara: SmtpClient envia la clave
                   con el salto pegado y Office 365 responde 535, igual que ante una clave
                   equivocada.

                   El otro lector de esta misma clase -el que devuelve la lista de un
                   grupo- NO recorta a proposito: alimenta el mantenimiento de
                   parametros, y ahi conviene ver lo que esta guardado tal cual. */
                valor = dr["Valor"].ToString().Trim();

                //resultado = dr["Respuesta"].ToString();
            }
            catch (Exception ex)
            {
                valor = null;

            }
            finally
            {
                if (cmd != null && cmd.Connection != null)
                {
                    cmd.Connection.Close();
                }
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
                if (cmd != null && cmd.Connection != null)
                {
                    cmd.Connection.Close();
                }
            }


            return Respuesta;
        }

    }
}
