using CapaEntidad;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoMarcacion
    {
        /// <summary>Registra la entrada (accion=1) o la salida (accion=2) del usuario.</summary>
        public static EntMarcacion RegistrarMarcacion(decimal idUsuario, int accion)
        {
            EntMarcacion resultado = new EntMarcacion()
            {
                Accion = accion,
                Respuestas = 0,
                Mensaje = "",
                FechaHora = Convert.ToDateTime("1900-01-01"),
                IdProceso = 0
            };

            try
            {
                DaoReporTareaAranda conexion = new DaoReporTareaAranda();

                using (SqlConnection cnx = conexion.conectar())
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_RegistrarMarcacion", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id_Usuario", SqlDbType.Decimal).Value = idUsuario;
                    cmd.Parameters.Add("@Accion", SqlDbType.Int).Value = accion;
                    cnx.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            resultado.Respuestas = Convert.ToInt32(dr["Respuestas"].ToString());
                            resultado.Mensaje = dr["Mensaje"].ToString();

                            if (dr["FechaHora"] != DBNull.Value)
                            {
                                resultado.FechaHora = Convert.ToDateTime(dr["FechaHora"]);
                            }

                            if (dr["IdProceso"] != DBNull.Value)
                            {
                                resultado.IdProceso = Convert.ToInt64(dr["IdProceso"]);
                            }
                        }
                        else
                        {
                            resultado.Mensaje = "El procedimiento no devolvió información.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resultado.Respuestas = 0;
                resultado.Mensaje = "Ocurrió un error al registrar la marcación. Detalle: " + ex.Message;
            }

            return resultado;
        }
    }
}
