using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoVentanaAprobacion
    {
        /// <summary>Lista los jefes inmediatos (MailCodJefeInm) con su ventana vigente.</summary>
        public static List<EntVentanaAprobacionJefe> ListarJefes(string filtro)
        {
            List<EntVentanaAprobacionJefe> lista = new List<EntVentanaAprobacionJefe>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarJefesVentanaAprobacion", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Filtro", SqlDbType.VarChar, 150).Value = filtro ?? string.Empty;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntVentanaAprobacionJefe()
                        {
                            MailJefe = dr["MailJefe"].ToString(),
                            NombreJefe = dr["NombreJefe"].ToString(),
                            NumColaboradores = Convert.ToInt32(dr["NumColaboradores"].ToString()),
                            FechaDesde = dr["FechaDesde"] == DBNull.Value ? "" : dr["FechaDesde"].ToString(),
                            FechaHasta = dr["FechaHasta"] == DBNull.Value ? "" : dr["FechaHasta"].ToString(),
                            TieneVentana = Convert.ToInt32(dr["TieneVentana"].ToString())
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>Inserta o actualiza (una por jefe) la ventana de aprobación.</summary>
        public static EntRespuesta GuardarVentana(string mailJefe, string fechaDesde, string fechaHasta, string usuarioRegistro)
        {
            EntRespuesta respuesta = new EntRespuesta()
            {
                estado = "0",
                resultado = "0",
                tipoMensaje = "danger",
                mensaje = ""
            };

            try
            {
                DaoReporTareaAranda conexion = new DaoReporTareaAranda();

                using (SqlConnection cnx = conexion.conectar())
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_GuardarVentanaAprobacionJefe", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@MailJefe", SqlDbType.VarChar, 150).Value = mailJefe ?? string.Empty;
                    cmd.Parameters.Add("@FechaDesde", SqlDbType.Date).Value = Convert.ToDateTime(fechaDesde);
                    cmd.Parameters.Add("@FechaHasta", SqlDbType.Date).Value = Convert.ToDateTime(fechaHasta);
                    cmd.Parameters.Add("@UsuarioRegistro", SqlDbType.VarChar, 100).Value = (object)usuarioRegistro ?? DBNull.Value;

                    cnx.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            int respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());
                            respuesta.resultado = respuestaSP.ToString();
                            respuesta.mensaje = dr["Mensaje"].ToString();

                            if (respuestaSP > 0)
                            {
                                respuesta.estado = "1";
                                respuesta.tipoMensaje = "success";
                            }
                            else
                            {
                                respuesta.estado = "0";
                                respuesta.tipoMensaje = "warning";
                            }
                        }
                        else
                        {
                            respuesta.mensaje = "El procedimiento no devolvió información.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.estado = "0";
                respuesta.resultado = "0";
                respuesta.tipoMensaje = "danger";
                respuesta.mensaje = "Ocurrió un error al guardar la ventana. Detalle: " + ex.Message;
            }

            return respuesta;
        }

        /// <summary>
        /// Valida si el jefe puede aprobar en la fecha dada.
        /// Sin ventana => permite. Ante error inesperado => permite (fail-open, opt-in).
        /// </summary>
        public static EntRespuesta ValidarVentana(string mailJefe, DateTime fecha)
        {
            EntRespuesta respuesta = new EntRespuesta()
            {
                estado = "1",
                resultado = "1",
                tipoMensaje = "success",
                mensaje = ""
            };

            try
            {
                DaoReporTareaAranda conexion = new DaoReporTareaAranda();

                using (SqlConnection cnx = conexion.conectar())
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_ValidarVentanaAprobacionJefe", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@MailJefe", SqlDbType.VarChar, 150).Value = mailJefe ?? string.Empty;
                    cmd.Parameters.Add("@Fecha", SqlDbType.Date).Value = fecha.Date;

                    cnx.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            bool puedeAprobar = Convert.ToBoolean(dr["PuedeAprobar"]);
                            if (puedeAprobar)
                            {
                                respuesta.estado = "1";
                                respuesta.tipoMensaje = "success";
                                respuesta.mensaje = "";
                            }
                            else
                            {
                                respuesta.estado = "0";
                                respuesta.tipoMensaje = "warning";
                                respuesta.mensaje = dr["Mensaje"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Fail-open: un problema al validar no debe frenar la operación de aprobación.
                respuesta.estado = "1";
                respuesta.resultado = "1";
                respuesta.tipoMensaje = "success";
                respuesta.mensaje = "";
            }

            return respuesta;
        }
    }
}
