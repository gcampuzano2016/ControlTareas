using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// Firmas de las solicitudes de vacaciones y permisos.
    /// </summary>
    public class DaoFirmaSolicitud
    {
        /// <summary>
        /// Registra la firma de un rol sobre una solicitud. El nombre, el cargo y
        /// la cédula los copia el procedimiento desde R_Usuarios: no se los pide
        /// a la pantalla, para que no se puedan digitar.
        /// </summary>
        public static EntRespuesta Guardar(EntFirmaSolicitud firma, byte[] trazo, string ip, string dispositivo)
        {
            EntRespuesta respuesta = new EntRespuesta();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_GuardarFirmaSolicitud", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdVacaciones", SqlDbType.BigInt).Value = firma.IdVacaciones;
                cmd.Parameters.Add("@Rol", SqlDbType.VarChar, 20).Value = firma.Rol ?? string.Empty;
                cmd.Parameters.Add("@Secuencia", SqlDbType.Int).Value = firma.Secuencia <= 0 ? 1 : firma.Secuencia;
                cmd.Parameters.Add("@Decision", SqlDbType.VarChar, 10).Value = firma.Decision ?? string.Empty;
                cmd.Parameters.Add("@Comentario", SqlDbType.VarChar, 500).Value = firma.Comentario ?? string.Empty;
                cmd.Parameters.Add("@TrazoTipo", SqlDbType.VarChar, 30).Value = firma.TrazoTipo ?? "image/png";
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 64).Value = firma.Cod_Usuario ?? string.Empty;
                cmd.Parameters.Add("@Ip", SqlDbType.VarChar, 45).Value = ip ?? string.Empty;
                cmd.Parameters.Add("@Dispositivo", SqlDbType.VarChar, 300).Value = dispositivo ?? string.Empty;

                SqlParameter pTrazo = cmd.Parameters.Add("@Trazo", SqlDbType.VarBinary, -1);
                pTrazo.Value = (trazo == null || trazo.Length == 0) ? (object)DBNull.Value : trazo;

                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        respuesta.estado = dr["Respuestas"].ToString();
                        respuesta.mensaje = dr["Mensaje"].ToString();
                    }
                }
            }

            respuesta.tipoMensaje = respuesta.estado == "1" ? "success" : "danger";
            return respuesta;
        }

        /// <summary>Firmas de una solicitud, en el orden del flujo.</summary>
        public static List<EntFirmaSolicitud> Listar(long idVacaciones)
        {
            List<EntFirmaSolicitud> lista = new List<EntFirmaSolicitud>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarFirmasSolicitud", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdVacaciones", SqlDbType.BigInt).Value = idVacaciones;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntFirmaSolicitud()
                        {
                            IdFirma = Convert.ToInt32(dr["IdFirma"]),
                            IdVacaciones = Convert.ToInt64(dr["IdVacaciones"]),
                            Rol = dr["Rol"].ToString(),
                            Secuencia = Convert.ToInt32(dr["Secuencia"]),
                            Decision = dr["Decision"].ToString(),
                            Comentario = dr["Comentario"].ToString(),
                            TrazoBase64 = dr["TrazoBase64"].ToString(),
                            TrazoTipo = dr["TrazoTipo"].ToString(),
                            Cod_Usuario = dr["Cod_Usuario"].ToString(),
                            Nombre = dr["Nombre"].ToString(),
                            Cargo = dr["Cargo"].ToString(),
                            Cedula = dr["Cedula"].ToString(),
                            FechaFirma = Convert.ToDateTime(dr["FechaFirma"]),
                            Ip = dr["Ip"].ToString(),
                            Dispositivo = dr["Dispositivo"].ToString()
                        });
                    }
                }
            }

            return lista;
        }
    }
}
